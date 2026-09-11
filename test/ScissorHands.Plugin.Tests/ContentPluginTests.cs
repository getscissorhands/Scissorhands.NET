using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;

namespace ScissorHands.Plugin.Tests;

public class ContentPluginTests
{
    [Fact]
    public void Given_DefaultContentPlugin_When_DependenciesRead_Then_It_Should_DeclareNoRequirements()
    {
        IContentPluginDependencies plugin = new TestPlugin();

        plugin.DependsOn.ShouldBeEmpty();
    }

    [Fact]
    public async Task Given_DefaultContentPlugin_When_PreMarkdownAsync_Invoked_Then_It_Should_ReturnSameDocumentInstance()
    {
        // Arrange
        var plugin = new TestPlugin();
        var document = new ContentDocument { SourcePath = "source.md", Kind = ContentKind.Post };
        var pluginManifest = new PluginManifest { Name = "Test" };
        var site = new SiteManifest();

        // Act
        var result = await plugin.PreMarkdownAsync(document, pluginManifest, site, Xunit.TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBeSameAs(document);
    }

    [Fact]
    public async Task Given_DefaultContentPlugin_When_PostMarkdownAsync_Invoked_Then_It_Should_ReturnSameDocumentInstance()
    {
        // Arrange
        var plugin = new TestPlugin();
        var document = new ContentDocument { SourcePath = "source.md", Kind = ContentKind.Page };
        var pluginManifest = new PluginManifest { Name = "Test" };
        var site = new SiteManifest();

        // Act
        var result = await plugin.PostMarkdownAsync(document, pluginManifest, site, Xunit.TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBeSameAs(document);
    }

    [Fact]
    public async Task Given_DefaultContentPlugin_When_PostHtmlAsync_Invoked_Then_It_Should_ReturnSameHtml()
    {
        // Arrange
        var plugin = new TestPlugin();
        var document = new ContentDocument();
        var pluginManifest = new PluginManifest { Name = "Test" };
        var site = new SiteManifest();
        var html = "<p>Hello</p>";

        // Act
        var result = await plugin.PostHtmlAsync(html, document, pluginManifest, site, Xunit.TestContext.Current.CancellationToken);

        // Assert
        result.ShouldBe(html);
    }
}

internal class TestPlugin : ContentPlugin
{
    public override string Name => "Test";
}
