using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;

namespace ScissorHands.Theme.Tests;

public class PostViewBaseTests
{
    [Fact]
    public void Given_PostViewBase_When_Constructed_Then_It_Should_HaveNullDocument()
    {
        // Arrange

        // Act
        var view = new TestPostView();

        // Assert
        view.Document.ShouldBeNull();
    }

    [Fact]
    public void Given_PostViewBase_When_RenderedWithCascadingValues_Then_It_Should_BindCascadingParameters()
    {
        // Arrange
        using var context = new BunitContext();

        var document = new ContentDocument
        {
            Metadata = new ContentMetadata { Title = "Hello", Slug = "hello" }
        };
        var plugins = new List<PluginManifest> { new() { Id = "test" } };
        var theme = new ThemeManifest { Name = "test", Slug = "test" };
        var site = new SiteManifest();

        // Act
        var cut = context.Renderer.Render<CascadingMainLayoutBase>(p => p
            .Add(x => x.Document, document)
            .Add(x => x.Plugins, plugins)
            .Add(x => x.Theme, theme)
            .Add(x => x.Site, site)
            .AddChildContent<TestPostView>());

        var view = cut.FindComponent<TestPostView>().Instance;

        // Assert
        view.Document.ShouldBeSameAs(document);
        view.Plugins.ShouldBeSameAs(plugins);
        view.Theme.ShouldBeSameAs(theme);
        view.Site.ShouldBeSameAs(site);
    }
}

internal class TestPostView : PostViewBase
{
}
