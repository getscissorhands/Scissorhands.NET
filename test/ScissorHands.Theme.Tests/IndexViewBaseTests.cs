using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;

namespace ScissorHands.Theme.Tests;

public class IndexViewBaseTests
{
    [Fact]
    public void Given_IndexViewBase_When_Constructed_Then_It_Should_HaveNullDocuments()
    {
        // Arrange

        // Act
        var view = new TestIndexView();

        // Assert
        view.Documents.ShouldBeNull();
    }

    [Fact]
    public void Given_IndexViewBase_When_RenderedWithCascadingValues_Then_It_Should_BindCascadingParameters()
    {
        // Arrange
        using var context = new BunitContext();

        var documents = new List<ContentDocument> { new() };
        var plugins = new List<PluginManifest> { new() { Id = "test" } };
        var theme = new ThemeManifest { Name = "test", Slug = "test" };
        var site = new SiteManifest();

        // Act
        var cut = context.Renderer.Render<CascadingMainLayoutBase>(p => p
            .Add(x => x.Documents, documents)
            .Add(x => x.Plugins, plugins)
            .Add(x => x.Theme, theme)
            .Add(x => x.Site, site)
            .AddChildContent<TestIndexView>());

        var view = cut.FindComponent<TestIndexView>().Instance;

        // Assert
        view.Documents.ShouldBeSameAs(documents);
        view.Plugins.ShouldBeSameAs(plugins);
        view.Theme.ShouldBeSameAs(theme);
        view.Site.ShouldBeSameAs(site);
    }
}

internal class TestIndexView : IndexViewBase
{
}
