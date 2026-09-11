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
}
