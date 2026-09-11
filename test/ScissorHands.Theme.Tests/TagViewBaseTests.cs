using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;

namespace ScissorHands.Theme.Tests;

public class TagViewBaseTests
{
    [Fact]
    public void Given_TagViewBase_When_Constructed_Then_It_Should_HaveNullCascadingParameters()
    {
        // Arrange

        // Act
        var view = new TestTagView();

        // Assert
        view.Tag.ShouldBeNull();
        view.TaggedPosts.ShouldBeNull();
        view.TaggedPages.ShouldBeNull();
        view.Plugins.ShouldBeNull();
        view.Theme.ShouldBeNull();
        view.Site.ShouldBeNull();
    }

    [Fact]
    public void Given_TagViewBase_When_RenderedWithCascadingValues_Then_It_Should_BindCascadingParameters()
    {
        // Arrange
        using var context = new BunitContext();

        var documents = new List<ContentDocument> { new() };
        var taggedPosts = new List<ContentDocument> { new() };
        var taggedPages = new List<ContentDocument> { new() };
        var plugins = new List<PluginManifest> { new() { Id = "test" } };
        var theme = new ThemeManifest { Name = "test", Slug = "test" };
        var site = new SiteManifest();

        const string tag = "tag";

        // Act
        var cut = context.Renderer.Render<CascadingMainLayoutBase>(p => p
            .Add(x => x.Documents, documents)
            .Add(x => x.Tag, tag)
            .Add(x => x.TaggedPosts, taggedPosts)
            .Add(x => x.TaggedPages, taggedPages)
            .Add(x => x.Plugins, plugins)
            .Add(x => x.Theme, theme)
            .Add(x => x.Site, site)
            .AddChildContent<TestTagView>());

        var view = cut.FindComponent<TestTagView>().Instance;

        // Assert
        view.Tag.ShouldBe(tag);
        view.TaggedPosts.ShouldBeSameAs(taggedPosts);
        view.TaggedPages.ShouldBeSameAs(taggedPages);
        view.Plugins.ShouldBeSameAs(plugins);
        view.Theme.ShouldBeSameAs(theme);
        view.Site.ShouldBeSameAs(site);
    }
}

internal class TestTagView : TagViewBase
{
}
