using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;
using ScissorHands.Plugin;
using ScissorHands.Web.Runners;

namespace ScissorHands.Web.Tests.Runners;

public class PluginRunnerTests
{
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

    [Fact]
    public async Task Given_InstalledPluginWithoutManifest_When_RunInvoked_Then_It_Should_RemainDisabled()
    {
        var plugin = Substitute.For<IContentPlugin>();
        plugin.Name.Returns("Optional");
        var document = new ContentDocument();
        var runner = new PluginRunner([], [plugin], new SiteManifest());

        var result = await runner.RunPreMarkdownAsync(document, CancellationToken.None);

        result.ShouldBeSameAs(document);
        await plugin.DidNotReceiveWithAnyArgs().PreMarkdownAsync(default!, default!, default!, default);
    }
}
