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
        var first = new AppendingPlugin("A", [new("b", stage)]);
        var second = new AppendingPlugin("B", [new("c", stage)]);
        var third = new AppendingPlugin("C", []);
        IContentPlugin[] plugins = reverseInputs ? [third, second, first] : [first, third, second];
        PluginManifest[] manifests = reverseInputs
            ? [new() { Name = "c" }, new() { Name = "a" }, new() { Name = "b" }]
            : [new() { Name = "B" }, new() { Name = "A" }, new() { Name = "C" }];
        var runner = new PluginRunner(manifests, plugins, new SiteManifest { IsPreview = reverseInputs });

        var results = await RunAllStagesAsync(runner);

        results.Pre.ShouldBe(stage == PluginStage.PreMarkdown ? "sourceCBA" : "sourceABC");
        results.Post.ShouldBe(stage == PluginStage.PostMarkdown ? "sourceCBA" : "sourceABC");
        results.Html.ShouldBe(stage == PluginStage.PostHtml ? "sourceCBA" : "sourceABC");
        runner.Manifests.ShouldBe(manifests);
        runner.Plugins.ShouldBe(plugins);
    }

    [Fact]
    public async Task Given_OppositeDependenciesInDifferentStages_When_AllStagesRun_Then_It_Should_NotTreatThemAsACycle()
    {
        var first = new AppendingPlugin("A", [new("B", PluginStage.PreMarkdown)]);
        var second = new AppendingPlugin("B", [new("A", PluginStage.PostMarkdown)]);
        var runner = CreateRunner(first, second);

        var results = await RunAllStagesAsync(runner);

        results.Pre.ShouldBe("sourceBA");
        results.Post.ShouldBe("sourceAB");
        results.Html.ShouldBe("sourceAB");
    }

    [Fact]
    public async Task Given_MultipleDependenciesAndIndependentPlugin_When_RunInvoked_Then_It_Should_ExecuteEveryDependencyOnce()
    {
        var first = new AppendingPlugin("A", [new("B", PluginStage.PreMarkdown), new("C", PluginStage.PreMarkdown)]);
        var second = new AppendingPlugin("B", [new("C", PluginStage.PreMarkdown)]);
        var third = new AppendingPlugin("C", []);
        var independent = new AppendingPlugin("D", []);
        var runner = CreateRunner(independent, first, second, third);

        var results = await RunAllStagesAsync(runner);

        results.Pre.ShouldBe("sourceCBAD");
        results.Post.ShouldBe("sourceABCD");
        results.Html.ShouldBe("sourceABCD");
    }

    [Fact]
    public async Task Given_SameDependencyDeclaredForEveryStage_When_AllStagesRun_Then_It_Should_ApplyEachDeclaration()
    {
        var dependencies = Enum.GetValues<PluginStage>().Select(stage => new PluginDependency("B", stage)).ToArray();
        var runner = CreateRunner(new AppendingPlugin("A", dependencies), new AppendingPlugin("B", []));

        var results = await RunAllStagesAsync(runner);

        results.Pre.ShouldBe("sourceBA");
        results.Post.ShouldBe("sourceBA");
        results.Html.ShouldBe("sourceBA");
    }

    [Fact]
    public async Task Given_DependencyThrows_When_AllStagesRun_Then_It_Should_PropagateTheFailureWithoutRunningTheDependent()
    {
        var dependency = Substitute.For<IContentPlugin>();
        dependency.Name.Returns("B");
        var dependent = Substitute.For<IContentPlugin, IContentPluginDependencies>();
        dependent.Name.Returns("A");
        ((IContentPluginDependencies)dependent).DependsOn.Returns(
            Enum.GetValues<PluginStage>().Select(stage => new PluginDependency("B", stage)).ToArray());
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
        var plugin = new AppendingPlugin("A", [new("B", PluginStage.PostMarkdown)]);
        var dependency = new AppendingPlugin("B", []);
        IContentPlugin[] plugins = installed ? [plugin, dependency] : [plugin];

        var exception = Should.Throw<InvalidOperationException>(() =>
            new PluginRunner([new PluginManifest { Name = "A" }], plugins, new SiteManifest()));

        exception.Message.ShouldContain("Plugin 'A'");
        exception.Message.ShouldContain("'B'");
        exception.Message.ShouldContain("PostMarkdown");
        exception.Message.ShouldContain(installed ? "not enabled" : "not installed");
    }

    [Theory]
    [InlineData(PluginStage.PreMarkdown)]
    [InlineData(PluginStage.PostMarkdown)]
    [InlineData(PluginStage.PostHtml)]
    public void Given_DependencyCycle_When_Constructed_Then_It_Should_ReportTheStageAndUnresolvedPlugins(PluginStage stage)
    {
        var first = new AppendingPlugin("A", [new("B", stage)]);
        var second = new AppendingPlugin("B", [new("C", stage)]);
        var third = new AppendingPlugin("C", [new("a", stage)]);

        var exception = Should.Throw<InvalidOperationException>(() => CreateRunner(third, first, second));

        exception.Message.ShouldContain("cycle");
        exception.Message.ShouldContain(stage.ToString());
        exception.Message.ShouldContain("A, B, C");
    }

    [Fact]
    public void Given_SelfDependency_When_Constructed_Then_It_Should_ReportConfigurationError()
    {
        var plugin = new AppendingPlugin("A", [new("a", PluginStage.PostHtml)]);

        var exception = Should.Throw<InvalidOperationException>(() => CreateRunner(plugin));

        exception.Message.ShouldContain("Plugin 'A'");
        exception.Message.ShouldContain("itself");
        exception.Message.ShouldContain("PostHtml");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Given_UnnamedDependency_When_Constructed_Then_It_Should_ReportConfigurationError(string? name)
    {
        var plugin = new AppendingPlugin("A", [new(name!, PluginStage.PreMarkdown)]);

        var exception = Should.Throw<InvalidOperationException>(() => CreateRunner(plugin));

        exception.Message.ShouldContain("Plugin 'A'");
        exception.Message.ShouldContain("empty name");
        exception.Message.ShouldContain("PreMarkdown");
    }

    [Fact]
    public void Given_InvalidDependencyStage_When_Constructed_Then_It_Should_ReportConfigurationError()
    {
        var plugin = new AppendingPlugin("A", [new("B", (PluginStage)123)]);

        var exception = Should.Throw<InvalidOperationException>(() => CreateRunner(plugin));

        exception.Message.ShouldContain("Plugin 'A'");
        exception.Message.ShouldContain("'B'");
        exception.Message.ShouldContain("invalid stage '123'");
    }

    [Fact]
    public void Given_DuplicateDependency_When_Constructed_Then_It_Should_ReportConfigurationError()
    {
        var plugin = new AppendingPlugin("A", [new("B", PluginStage.PreMarkdown), new("b", PluginStage.PreMarkdown)]);
        var dependency = new AppendingPlugin("B", []);

        var exception = Should.Throw<InvalidOperationException>(() => CreateRunner(plugin, dependency));

        exception.Message.ShouldContain("Plugin 'A'");
        exception.Message.ShouldContain("more than once");
        exception.Message.ShouldContain("PreMarkdown");
    }

    [Fact]
    public void Given_NullDependencyList_When_Constructed_Then_It_Should_ReportConfigurationError()
    {
        var plugin = new AppendingPlugin("A", null!);

        var exception = Should.Throw<InvalidOperationException>(() => CreateRunner(plugin));

        exception.Message.ShouldContain("Plugin 'A'");
        exception.Message.ShouldContain("non-null DependsOn list");
    }

    [Fact]
    public void Given_NullDependencyEntry_When_Constructed_Then_It_Should_ReportConfigurationError()
    {
        var plugin = new AppendingPlugin("A", [null!]);

        var exception = Should.Throw<InvalidOperationException>(() => CreateRunner(plugin));

        exception.Message.ShouldContain("Plugin 'A'");
        exception.Message.ShouldContain("null dependency");
    }

    [Fact]
    public async Task Given_DisabledPluginWithMissingDependency_When_AllStagesRun_Then_It_Should_IgnoreItsRequirements()
    {
        var enabled = new AppendingPlugin("A", []);
        var disabled = new AppendingPlugin("B", [new("Missing", PluginStage.PreMarkdown)]);
        var runner = new PluginRunner([new PluginManifest { Name = "A" }], [disabled, enabled], new SiteManifest());

        var results = await RunAllStagesAsync(runner);

        results.Pre.ShouldBe("sourceA");
        results.Post.ShouldBe("sourceA");
        results.Html.ShouldBe("sourceA");
    }

    [Fact]
    public async Task Given_DependencyListChangedAfterConstruction_When_RunInvoked_Then_It_Should_UseTheOriginalPlan()
    {
        var dependencies = new List<PluginDependency> { new("B", PluginStage.PreMarkdown) };
        var first = new AppendingPlugin("A", dependencies);
        var second = new AppendingPlugin("B", []);
        var runner = CreateRunner(first, second);
        dependencies.Clear();
        dependencies.Add(new("Missing", PluginStage.PostHtml));

        var results = await RunAllStagesAsync(runner);

        results.Pre.ShouldBe("sourceBA");
        results.Post.ShouldBe("sourceAB");
        results.Html.ShouldBe("sourceAB");
    }

    private static PluginRunner CreateRunner(params IContentPlugin[] plugins)
    {
        return new PluginRunner(plugins.Select(plugin => new PluginManifest { Name = plugin.Name }), plugins, new SiteManifest());
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

    private sealed class AppendingPlugin(string name, IReadOnlyList<PluginDependency> dependencies) : ContentPlugin
    {
        public override string Name => name;

        public override IReadOnlyList<PluginDependency> DependsOn => dependencies;

        public override Task<ContentDocument> PreMarkdownAsync(
            ContentDocument document, PluginManifest plugin, SiteManifest site, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new ContentDocument { Markdown = document.Markdown + Name, Html = document.Html });
        }

        public override Task<ContentDocument> PostMarkdownAsync(
            ContentDocument document, PluginManifest plugin, SiteManifest site, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new ContentDocument { Markdown = document.Markdown, Html = document.Html + Name });
        }

        public override Task<string> PostHtmlAsync(
            string html, ContentDocument document, PluginManifest plugin, SiteManifest site, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(html + Name);
        }
    }
}
