using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;

namespace ScissorHands.Theme.Tests;

public class TagListViewBaseTests
{
    [Fact]
    public void Given_TagListViewBase_When_Constructed_Then_It_Should_HaveNullTaggedDocuments()
    {
        // Arrange

        // Act
        var view = new TestTagListView();

        // Assert
        view.TaggedDocuments.ShouldBeNull();
    }

    [Fact]
    public void Given_TagListViewBase_When_RenderedWithCascadingValues_Then_It_Should_BindCascadingParameters()
    {
        // Arrange
        using var context = new BunitContext();

        var documents = new List<ContentDocument> { new() };
        var plugins = new List<PluginManifest> { new() };
        var theme = new ThemeManifest { Name = "test", Slug = "test" };
        var site = new SiteManifest();

        var taggedDocuments = new Dictionary<string, (IEnumerable<ContentDocument> Posts, IEnumerable<ContentDocument> Pages)>
        {
            ["tag"] = (new List<ContentDocument> { new() }, new List<ContentDocument> { new() })
        };

        // Act
        var cut = context.Renderer.Render<CascadingMainLayoutBase>(p => p
            .Add(x => x.Documents, documents)
            .Add(x => x.TaggedDocuments, taggedDocuments)
            .Add(x => x.Plugins, plugins)
            .Add(x => x.Theme, theme)
            .Add(x => x.Site, site)
            .AddChildContent<TestTagListView>());

        var view = cut.FindComponent<TestTagListView>().Instance;

        // Assert
        view.TaggedDocuments.ShouldBeSameAs(taggedDocuments);
        view.Plugins.ShouldBeSameAs(plugins);
        view.Theme.ShouldBeSameAs(theme);
        view.Site.ShouldBeSameAs(site);
    }
}

internal class TestTagListView : TagListViewBase
{
}
