using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;
using ScissorHands.Plugin;
using ScissorHands.Web.Runners;

namespace ScissorHands.Web.Tests.Runners;

public class PluginRunnerTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task Given_PluginsWithoutDependencyMetadata_When_AllStagesRun_Then_It_Should_ChainPluginsInDeterministicNameOrder(
        bool reverseManifests,
        bool isPreview)
    {
        var first = Substitute.For<IContentPlugin>();
        first.Name.Returns("First");
        var second = Substitute.For<IContentPlugin>();
        second.Name.Returns("Second");
        var disabled = Substitute.For<IContentPlugin>();
        disabled.Name.Returns("Disabled");
        var firstManifest = new PluginManifest { Name = "FIRST" };
        var secondManifest = new PluginManifest { Name = "second" };
        PluginManifest[] manifests = reverseManifests ? [secondManifest, firstManifest] : [firstManifest, secondManifest];
        PluginManifest[] orderedManifests = [firstManifest, secondManifest];
        IContentPlugin[] orderedPlugins = [first, second];
        IContentPlugin[] installedPlugins = [orderedPlugins[1], disabled, orderedPlugins[0]];
        var site = new SiteManifest { IsPreview = isPreview };
        var token = Xunit.TestContext.Current.CancellationToken;
        ContentDocument[] preDocuments =
        [
            new() { Markdown = "source" },
            new() { Markdown = $"source|{orderedPlugins[0].Name}" },
            new() { Markdown = $"source|{orderedPlugins[0].Name}|{orderedPlugins[1].Name}" },
        ];
        ContentDocument[] postDocuments =
        [
            preDocuments[2],
            new() { Html = $"html|{orderedPlugins[0].Name}" },
            new() { Html = $"html|{orderedPlugins[0].Name}|{orderedPlugins[1].Name}" },
        ];
        string[] htmlOutputs =
        [
            postDocuments[2].Html,
            $"{postDocuments[2].Html}|{orderedPlugins[0].Name}",
            $"{postDocuments[2].Html}|{orderedPlugins[0].Name}|{orderedPlugins[1].Name}",
        ];
        for (var index = 0; index < orderedPlugins.Length; index++)
        {
            orderedPlugins[index].PreMarkdownAsync(preDocuments[index], orderedManifests[index], site, token)
                .Returns(preDocuments[index + 1]);
            orderedPlugins[index].PostMarkdownAsync(postDocuments[index], orderedManifests[index], site, token)
                .Returns(postDocuments[index + 1]);
            orderedPlugins[index].PostHtmlAsync(htmlOutputs[index], postDocuments[2], orderedManifests[index], site, token)
                .Returns(htmlOutputs[index + 1]);
        }
        var runner = new PluginRunner(manifests, installedPlugins, site);

        var preResult = await runner.RunPreMarkdownAsync(preDocuments[0], token);
        preResult.ShouldBeSameAs(preDocuments[2]);

        var postResult = await runner.RunPostMarkdownAsync(preResult, token);
        postResult.ShouldBeSameAs(postDocuments[2]);

        var htmlResult = await runner.RunPostHtmlAsync(postResult.Html, postResult, token);
        htmlResult.ShouldBe(htmlOutputs[2]);
        runner.Manifests.ShouldBe(manifests);
        runner.Plugins.ShouldBe(installedPlugins);
        for (var index = 0; index < orderedPlugins.Length; index++)
        {
            await orderedPlugins[index].Received(1).PreMarkdownAsync(preDocuments[index], orderedManifests[index], site, token);
            await orderedPlugins[index].Received(1).PostMarkdownAsync(postDocuments[index], orderedManifests[index], site, token);
            await orderedPlugins[index].Received(1).PostHtmlAsync(htmlOutputs[index], postDocuments[2], orderedManifests[index], site, token);
        }
        await disabled.DidNotReceiveWithAnyArgs().PreMarkdownAsync(default!, default!, default!, token);
        await disabled.DidNotReceiveWithAnyArgs().PostMarkdownAsync(default!, default!, default!, token);
        await disabled.DidNotReceiveWithAnyArgs().PostHtmlAsync(default!, default!, default!, default!, token);
    }

    [Fact]
    public async Task Given_DifferentlyCasedManifestName_When_RunInvoked_Then_It_Should_ExecutePlugin()
    {
        var plugin = Substitute.For<IContentPlugin>();
        plugin.Name.Returns("Example");
        var document = new ContentDocument();
        plugin.PreMarkdownAsync(document, Arg.Any<PluginManifest>(), Arg.Any<SiteManifest>(), Arg.Any<CancellationToken>())
            .Returns(document);
        var runner = new PluginRunner(
            [new PluginManifest { Name = "example" }],
            [plugin],
            new SiteManifest());

        var result = await runner.RunPreMarkdownAsync(document, CancellationToken.None);

        result.ShouldBeSameAs(document);
        await plugin.Received(1).PreMarkdownAsync(
            document,
            Arg.Is<PluginManifest>(manifest => manifest.Name == "example"),
            Arg.Any<SiteManifest>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public void Given_DuplicateManifestNames_When_Constructed_Then_It_Should_ReportConfigurationError()
    {
        var exception = Should.Throw<InvalidOperationException>(() => new PluginRunner(
            [
                new PluginManifest { Name = "Example" },
                new PluginManifest { Name = "example" },
            ],
            [],
            new SiteManifest()));

        exception.Message.ShouldContain("configured more than once");
    }

    [Fact]
    public void Given_UnnamedManifest_When_Constructed_Then_It_Should_ReportConfigurationError()
    {
        var exception = Should.Throw<InvalidOperationException>(() => new PluginRunner(
            [new PluginManifest()],
            [],
            new SiteManifest()));

        exception.Message.ShouldContain("non-empty name");
    }

    [Fact]
    public void Given_ManifestWithoutInstalledPlugin_When_Constructed_Then_It_Should_ReportConfigurationError()
    {
        var exception = Should.Throw<InvalidOperationException>(() => new PluginRunner(
            [new PluginManifest { Name = "Missing" }],
            [],
            new SiteManifest()));

        exception.Message.ShouldContain("does not match any installed plugin");
    }

    [Fact]
    public void Given_DuplicateInstalledPluginNames_When_Constructed_Then_It_Should_ReportConfigurationError()
    {
        var first = Substitute.For<IContentPlugin>();
        first.Name.Returns("Example");
        var second = Substitute.For<IContentPlugin>();
        second.Name.Returns("example");

        var exception = Should.Throw<InvalidOperationException>(() => new PluginRunner(
            [],
            [first, second],
            new SiteManifest()));

        exception.Message.ShouldContain("is not unique");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Given_UnnamedInstalledPlugin_When_Constructed_Then_It_Should_ReportConfigurationError(string name)
    {
        var plugin = Substitute.For<IContentPlugin>();
        plugin.Name.Returns(name);

        var exception = Should.Throw<InvalidOperationException>(() => new PluginRunner(
            [],
            [plugin],
            new SiteManifest()));

        exception.Message.ShouldContain("non-empty name");
    }

    [Fact]
    public async Task Given_CancelledToken_When_AllStagesRun_Then_It_Should_NotExecutePlugins()
    {
        var plugin = Substitute.For<IContentPlugin>();
        plugin.Name.Returns("Example");
        var runner = new PluginRunner([new PluginManifest { Name = "Example" }], [plugin], new SiteManifest());
        var document = new ContentDocument();
        using var cancellationSource = new CancellationTokenSource();
        cancellationSource.Cancel();
        var token = cancellationSource.Token;

        await Should.ThrowAsync<OperationCanceledException>(() => runner.RunPreMarkdownAsync(document, token));
        await Should.ThrowAsync<OperationCanceledException>(() => runner.RunPostMarkdownAsync(document, token));
        await Should.ThrowAsync<OperationCanceledException>(() => runner.RunPostHtmlAsync("html", document, token));

        await plugin.DidNotReceiveWithAnyArgs().PreMarkdownAsync(default!, default!, default!, token);
        await plugin.DidNotReceiveWithAnyArgs().PostMarkdownAsync(default!, default!, default!, token);
        await plugin.DidNotReceiveWithAnyArgs().PostHtmlAsync(default!, default!, default!, default!, token);
    }

    [Fact]
    public async Task Given_InstalledPluginWithoutManifest_When_RunInvoked_Then_It_Should_RemainDisabled()
    {
        var plugin = Substitute.For<IContentPlugin>();
        plugin.Name.Returns("Optional");
        var document = new ContentDocument();
        var runner = new PluginRunner([], [plugin], new SiteManifest());

        var result = await runner.RunPreMarkdownAsync(document, CancellationToken.None);
        var postResult = await runner.RunPostMarkdownAsync(document, CancellationToken.None);
        var htmlResult = await runner.RunPostHtmlAsync("html", document, CancellationToken.None);

        result.ShouldBeSameAs(document);
        postResult.ShouldBeSameAs(document);
        htmlResult.ShouldBe("html");
        await plugin.DidNotReceiveWithAnyArgs().PreMarkdownAsync(default!, default!, default!, Xunit.TestContext.Current.CancellationToken);
        await plugin.DidNotReceiveWithAnyArgs().PostMarkdownAsync(default!, default!, default!, Xunit.TestContext.Current.CancellationToken);
        await plugin.DidNotReceiveWithAnyArgs().PostHtmlAsync(default!, default!, default!, default!, Xunit.TestContext.Current.CancellationToken);
    }
}
