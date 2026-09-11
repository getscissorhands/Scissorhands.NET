using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;
using ScissorHands.Plugin;
using ScissorHands.Web.Runners;

namespace ScissorHands.Web.Tests.Runners;

public class PluginDependencyTests
{
    [Theory]
    [InlineData(PluginStage.PreMarkdown, false)]
    [InlineData(PluginStage.PreMarkdown, true)]
    [InlineData(PluginStage.PostMarkdown, false)]
    [InlineData(PluginStage.PostMarkdown, true)]
    [InlineData(PluginStage.PostHtml, false)]
    [InlineData(PluginStage.PostHtml, true)]
    public async Task Given_TransitiveDependencies_When_AllStagesRun_Then_It_Should_OrderOnlyTheDeclaredStage(
        PluginStage stage,
        bool reverseInputs)
    {
        var first = new AppendingPlugin("a", [new("b", stage)]);
        var second = new AppendingPlugin("b", [new("c", stage)]);
        var third = new AppendingPlugin("c", []);
        IContentPlugin[] plugins = reverseInputs ? [third, second, first] : [first, third, second];
        PluginManifest[] manifests = reverseInputs
            ? [new() { Id = "c" }, new() { Id = "a" }, new() { Id = "b" }]
            : [new() { Id = "b" }, new() { Id = "a" }, new() { Id = "c" }];
        var runner = new PluginRunner(manifests, plugins, new SiteManifest { IsPreview = reverseInputs });

        var results = await RunAllStagesAsync(runner);

        results.Pre.ShouldBe(stage == PluginStage.PreMarkdown ? "sourcecba" : "sourceabc");
        results.Post.ShouldBe(stage == PluginStage.PostMarkdown ? "sourcecba" : "sourceabc");
        results.Html.ShouldBe(stage == PluginStage.PostHtml ? "sourcecba" : "sourceabc");
        runner.Manifests.ShouldBe(manifests);
        runner.Plugins.ShouldBe(plugins);
    }

    [Fact]
    public async Task Given_OppositeDependenciesInDifferentStages_When_AllStagesRun_Then_It_Should_NotTreatThemAsACycle()
    {
        var first = new AppendingPlugin("a", [new("b", PluginStage.PreMarkdown)]);
        var second = new AppendingPlugin("b", [new("a", PluginStage.PostMarkdown)]);
        var runner = CreateRunner(first, second);

        var results = await RunAllStagesAsync(runner);

        results.Pre.ShouldBe("sourceba");
        results.Post.ShouldBe("sourceab");
        results.Html.ShouldBe("sourceab");
    }

    [Fact]
    public async Task Given_MultipleDependenciesAndIndependentPlugin_When_RunInvoked_Then_It_Should_ExecuteEveryDependencyOnce()
    {
        var first = new AppendingPlugin("a", [new("b", PluginStage.PreMarkdown), new("c", PluginStage.PreMarkdown)]);
        var second = new AppendingPlugin("b", [new("c", PluginStage.PreMarkdown)]);
        var third = new AppendingPlugin("c", []);
        var independent = new AppendingPlugin("d", []);
        var runner = CreateRunner(independent, first, second, third);

        var results = await RunAllStagesAsync(runner);

        results.Pre.ShouldBe("sourcecbad");
        results.Post.ShouldBe("sourceabcd");
        results.Html.ShouldBe("sourceabcd");
    }

    [Fact]
    public async Task Given_SameDependencyDeclaredForEveryStage_When_AllStagesRun_Then_It_Should_ApplyEachDeclaration()
    {
        var dependencies = Enum.GetValues<PluginStage>().Select(stage => new PluginDependency("b", stage)).ToArray();
        var runner = CreateRunner(new AppendingPlugin("a", dependencies), new AppendingPlugin("b", []));

        var results = await RunAllStagesAsync(runner);

        results.Pre.ShouldBe("sourceba");
        results.Post.ShouldBe("sourceba");
        results.Html.ShouldBe("sourceba");
    }

    [Fact]
    public async Task Given_DependencyThrows_When_AllStagesRun_Then_It_Should_PropagateTheFailureWithoutRunningTheDependent()
    {
        var dependency = Substitute.For<IContentPlugin>();
        dependency.Id.Returns("b");
        dependency.Name.Returns("B");
        var dependent = Substitute.For<IContentPlugin, IContentPluginDependencies>();
        dependent.Id.Returns("a");
        dependent.Name.Returns("A");
        ((IContentPluginDependencies)dependent).DependsOn.Returns(
            Enum.GetValues<PluginStage>().Select(stage => new PluginDependency("b", stage)).ToArray());
        var failure = new InvalidOperationException("Dependency failed.");
        dependency.PreMarkdownAsync(Arg.Any<ContentDocument>(), Arg.Any<PluginManifest>(), Arg.Any<SiteManifest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<ContentDocument>(failure));
        dependency.PostMarkdownAsync(Arg.Any<ContentDocument>(), Arg.Any<PluginManifest>(), Arg.Any<SiteManifest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<ContentDocument>(failure));
        dependency.PostHtmlAsync(Arg.Any<string>(), Arg.Any<ContentDocument>(), Arg.Any<PluginManifest>(), Arg.Any<SiteManifest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<string>(failure));
        var runner = CreateRunner(dependent, dependency);
        var token = Xunit.TestContext.Current.CancellationToken;
        var document = new ContentDocument();

        var preException = await Should.ThrowAsync<InvalidOperationException>(() => runner.RunPreMarkdownAsync(document, token));
        var postException = await Should.ThrowAsync<InvalidOperationException>(() => runner.RunPostMarkdownAsync(document, token));
        var htmlException = await Should.ThrowAsync<InvalidOperationException>(() => runner.RunPostHtmlAsync("html", document, token));

        preException.ShouldBeSameAs(failure);
        postException.ShouldBeSameAs(failure);
        htmlException.ShouldBeSameAs(failure);
        await dependent.DidNotReceiveWithAnyArgs().PreMarkdownAsync(default!, default!, default!, token);
        await dependent.DidNotReceiveWithAnyArgs().PostMarkdownAsync(default!, default!, default!, token);
        await dependent.DidNotReceiveWithAnyArgs().PostHtmlAsync(default!, default!, default!, default!, token);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Given_DependencyNotEnabled_When_Constructed_Then_It_Should_ReportThePluginAndStage(bool installed)
    {
        var plugin = new AppendingPlugin("a", [new("b", PluginStage.PostMarkdown)]);
        var dependency = new AppendingPlugin("b", []);
        IContentPlugin[] plugins = installed ? [plugin, dependency] : [plugin];

        var exception = Should.Throw<InvalidOperationException>(() =>
            new PluginRunner([new PluginManifest { Id = "a" }], plugins, new SiteManifest()));

        exception.Message.ShouldContain("Plugin 'a'");
        exception.Message.ShouldContain("'b'");
        exception.Message.ShouldContain("PostMarkdown");
        exception.Message.ShouldContain(installed ? "not enabled" : "not installed");
    }

    [Theory]
    [InlineData(PluginStage.PreMarkdown)]
    [InlineData(PluginStage.PostMarkdown)]
    [InlineData(PluginStage.PostHtml)]
    public void Given_DependencyCycle_When_Constructed_Then_It_Should_ReportTheStageAndUnresolvedPlugins(PluginStage stage)
    {
        var first = new AppendingPlugin("a", [new("b", stage)]);
        var second = new AppendingPlugin("b", [new("c", stage)]);
        var third = new AppendingPlugin("c", [new("a", stage)]);

        var exception = Should.Throw<InvalidOperationException>(() => CreateRunner(third, first, second));

        exception.Message.ShouldContain("cycle");
        exception.Message.ShouldContain(stage.ToString());
        exception.Message.ShouldContain("a, b, c");
    }

    [Fact]
    public void Given_SelfDependency_When_Constructed_Then_It_Should_ReportConfigurationError()
    {
        var plugin = new AppendingPlugin("a", [new("a", PluginStage.PostHtml)]);

        var exception = Should.Throw<InvalidOperationException>(() => CreateRunner(plugin));

        exception.Message.ShouldContain("Plugin 'a'");
        exception.Message.ShouldContain("itself");
        exception.Message.ShouldContain("PostHtml");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("B")]
    [InlineData("plugin_name")]
    [InlineData("plugin--name")]
    public void Given_InvalidDependencyId_When_Constructed_Then_It_Should_ReportConfigurationError(string? id)
    {
        var plugin = new AppendingPlugin("a", [new(id!, PluginStage.PreMarkdown)]);

        var exception = Should.Throw<InvalidOperationException>(() => CreateRunner(plugin));

        exception.Message.ShouldContain("Plugin 'a'");
        exception.Message.ShouldContain("invalid plugin ID");
        exception.Message.ShouldContain("PreMarkdown");
    }

    [Fact]
    public void Given_InvalidDependencyStage_When_Constructed_Then_It_Should_ReportConfigurationError()
    {
        var plugin = new AppendingPlugin("a", [new("b", (PluginStage)123)]);

        var exception = Should.Throw<InvalidOperationException>(() => CreateRunner(plugin));

        exception.Message.ShouldContain("Plugin 'a'");
        exception.Message.ShouldContain("'b'");
        exception.Message.ShouldContain("invalid stage '123'");
    }

    [Fact]
    public void Given_DuplicateDependency_When_Constructed_Then_It_Should_ReportConfigurationError()
    {
        var plugin = new AppendingPlugin("a", [new("b", PluginStage.PreMarkdown), new("b", PluginStage.PreMarkdown)]);
        var dependency = new AppendingPlugin("b", []);

        var exception = Should.Throw<InvalidOperationException>(() => CreateRunner(plugin, dependency));

        exception.Message.ShouldContain("Plugin 'a'");
        exception.Message.ShouldContain("more than once");
        exception.Message.ShouldContain("PreMarkdown");
    }

    [Fact]
    public void Given_NullDependencyList_When_Constructed_Then_It_Should_ReportConfigurationError()
    {
        var plugin = new AppendingPlugin("a", null!);

        var exception = Should.Throw<InvalidOperationException>(() => CreateRunner(plugin));

        exception.Message.ShouldContain("Plugin 'a'");
        exception.Message.ShouldContain("non-null DependsOn list");
    }

    [Fact]
    public void Given_NullDependencyEntry_When_Constructed_Then_It_Should_ReportConfigurationError()
    {
        var plugin = new AppendingPlugin("a", [null!]);

        var exception = Should.Throw<InvalidOperationException>(() => CreateRunner(plugin));

        exception.Message.ShouldContain("Plugin 'a'");
        exception.Message.ShouldContain("null dependency");
    }

    [Fact]
    public async Task Given_DisabledPluginWithMissingDependency_When_AllStagesRun_Then_It_Should_IgnoreItsRequirements()
    {
        var enabled = new AppendingPlugin("a", []);
        var disabled = new AppendingPlugin("b", [new("missing", PluginStage.PreMarkdown)]);
        var runner = new PluginRunner([new PluginManifest { Id = "a" }], [disabled, enabled], new SiteManifest());

        var results = await RunAllStagesAsync(runner);

        results.Pre.ShouldBe("sourcea");
        results.Post.ShouldBe("sourcea");
        results.Html.ShouldBe("sourcea");
    }

    [Fact]
    public async Task Given_DependencyListChangedAfterConstruction_When_RunInvoked_Then_It_Should_UseTheOriginalPlan()
    {
        var dependencies = new List<PluginDependency> { new("b", PluginStage.PreMarkdown) };
        var first = new AppendingPlugin("a", dependencies);
        var second = new AppendingPlugin("b", []);
        var runner = CreateRunner(first, second);
        dependencies.Clear();
        dependencies.Add(new("missing", PluginStage.PostHtml));

        var results = await RunAllStagesAsync(runner);

        results.Pre.ShouldBe("sourceba");
        results.Post.ShouldBe("sourceab");
        results.Html.ShouldBe("sourceab");
    }

    [Fact]
    public void Given_DependencyMatchingOnlyDisplayName_When_Constructed_Then_It_Should_NotFallBackToName()
    {
        var dependency = Substitute.For<IContentPlugin>();
        dependency.Id.Returns("actual-id");
        dependency.Name.Returns("label");
        var dependent = new AppendingPlugin("dependent", [new("label", PluginStage.PreMarkdown)]);

        var exception = Should.Throw<InvalidOperationException>(() => CreateRunner(dependency, dependent));

        exception.Message.ShouldContain("Plugin 'dependent'");
        exception.Message.ShouldContain("'label'");
        exception.Message.ShouldContain("not installed");
    }

    private static PluginRunner CreateRunner(params IContentPlugin[] plugins)
    {
        return new PluginRunner(plugins.Select(plugin => new PluginManifest { Id = plugin.Id }), plugins, new SiteManifest());
    }

    private static async Task<(string Pre, string Post, string Html)> RunAllStagesAsync(PluginRunner runner)
    {
        var token = Xunit.TestContext.Current.CancellationToken;
        var document = new ContentDocument { Markdown = "source", Html = "source" };
        var preResult = await runner.RunPreMarkdownAsync(document, token);
        var postResult = await runner.RunPostMarkdownAsync(preResult, token);
        var htmlResult = await runner.RunPostHtmlAsync("source", postResult, token);
        return (preResult.Markdown, postResult.Html, htmlResult);
    }

    private sealed class AppendingPlugin(string id, IReadOnlyList<PluginDependency> dependencies) : ContentPlugin
    {
        public override string Id => id;

        public override string Name => $"Display {id}";

        public override IReadOnlyList<PluginDependency> DependsOn => dependencies;

        public override Task<ContentDocument> PreMarkdownAsync(
            ContentDocument document, PluginManifest plugin, SiteManifest site, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new ContentDocument { Markdown = document.Markdown + Id, Html = document.Html });
        }

        public override Task<ContentDocument> PostMarkdownAsync(
            ContentDocument document, PluginManifest plugin, SiteManifest site, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new ContentDocument { Markdown = document.Markdown, Html = document.Html + Id });
        }

        public override Task<string> PostHtmlAsync(
            string html, ContentDocument document, PluginManifest plugin, SiteManifest site, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(html + Id);
        }
    }
}
