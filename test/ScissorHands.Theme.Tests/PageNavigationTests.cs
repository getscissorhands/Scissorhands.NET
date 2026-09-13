using System.Reflection;

using Microsoft.AspNetCore.Components;

using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;

namespace ScissorHands.Theme.Tests;

public class PageNavigationTests
{
    [Fact]
    public void Given_DefaultComponents_When_Construct_Invoked_Then_It_Should_PreserveDefaultsAndKeepNavigationOptional()
    {
        // Arrange

        // Act
        var layout = new NavigationLayoutProbe();
        var cascade = new CascadingMainLayoutBase();
        var page = new NavigationPageProbe();

        // Assert
        layout.PageNavigation.ShouldBe(new PageNavigation());
        layout.NavigationPages.ShouldBeEmpty();
        layout.NavigationTree.ShouldBeEmpty();
        layout.Documents.ShouldBeNull();
        layout.Document.ShouldBeNull();
        layout.TaggedDocuments.ShouldBeNull();
        layout.Tag.ShouldBeNull();
        layout.TaggedPosts.ShouldBeNull();
        layout.TaggedPages.ShouldBeNull();
        layout.Plugins.ShouldBeNull();
        layout.Theme.ShouldBeNull();
        layout.Site.ShouldBeNull();
        cascade.PageNavigation.ShouldBe(new PageNavigation());
        cascade.ChildContent.ShouldBeNull();
        cascade.Documents.ShouldBeNull();
        cascade.Document.ShouldBeNull();
        cascade.TaggedDocuments.ShouldBeNull();
        cascade.Tag.ShouldBeNull();
        cascade.TaggedPosts.ShouldBeNull();
        cascade.TaggedPages.ShouldBeNull();
        cascade.Plugins.ShouldBeNull();
        cascade.Theme.ShouldBeNull();
        cascade.Site.ShouldBeNull();
        page.PageNavigation.ShouldBeNull();
        page.Document.ShouldBeNull();
        page.Plugins.ShouldBeNull();
        page.Theme.ShouldBeNull();
        page.Site.ShouldBeNull();
    }

    [Theory]
    [InlineData(typeof(MainLayoutBase), typeof(ParameterAttribute))]
    [InlineData(typeof(CascadingMainLayoutBase), typeof(ParameterAttribute))]
    [InlineData(typeof(PageViewBase), typeof(CascadingParameterAttribute))]
    public void Given_ComponentContract_When_GetProperty_Invoked_Then_It_Should_ExposeTypedPageNavigation(Type componentType, Type attributeType)
    {
        // Arrange

        // Act
        var property = componentType.GetProperty(nameof(PageNavigation));

        // Assert
        property.ShouldNotBeNull();
        property.PropertyType.ShouldBe(typeof(PageNavigation));
        property.GetCustomAttribute(attributeType).ShouldNotBeNull();
    }

    [Theory]
    [InlineData(typeof(CascadingMainLayoutBase))]
    [InlineData(typeof(PageViewBase))]
    public void Given_PerPageContract_When_GetProperties_Invoked_Then_It_Should_NotExposeFullNavigationCollections(Type componentType)
    {
        // Arrange

        // Act
        var properties = componentType.GetProperties();

        // Assert
        properties.Select(property => property.Name).ShouldNotContain(nameof(MainLayoutBase.NavigationPages));
        properties.Select(property => property.Name).ShouldNotContain(nameof(MainLayoutBase.NavigationTree));
    }

    [Fact]
    public void Given_PageNavigation_When_Render_Invoked_Then_It_Should_CascadeOnlyPerPageLinksAndPreserveExistingValues()
    {
        // Arrange
        using var context = new BunitContext();
        var document = new ContentDocument { Metadata = new ContentMetadata { Title = "Current" } };
        var documents = new[] { document };
        var plugins = new[] { new PluginManifest { Id = "test" } };
        var theme = new ThemeManifest { Name = "test", Slug = "test" };
        var site = new SiteManifest { Title = "Test" };
        var navigation = new PageNavigation
        {
            Previous = new PageNavigationLink { Title = "Previous", Url = "previous" },
            Next = new PageNavigationLink { Title = "Next", Url = "next" },
        };

        // Act
        var cut = context.Render<CascadingMainLayoutBase>(parameters => parameters
            .Add(p => p.Document, document)
            .Add(p => p.Documents, documents)
            .Add(p => p.Plugins, plugins)
            .Add(p => p.Theme, theme)
            .Add(p => p.Site, site)
            .Add(p => p.PageNavigation, navigation)
            .AddChildContent<NavigationPageProbe>());
        var page = cut.FindComponent<NavigationPageProbe>().Instance;

        // Assert
        cut.FindComponent<CascadingValue<PageNavigation>>().Instance.Value.ShouldBeSameAs(navigation);
        page.PageNavigation.ShouldBeSameAs(navigation);
        page.Document.ShouldBeSameAs(document);
        page.Documents.ShouldBeSameAs(documents);
        page.Plugins.ShouldBeSameAs(plugins);
        page.Theme.ShouldBeSameAs(theme);
        page.Site.ShouldBeSameAs(site);
        page.NavigationPages.ShouldBeNull();
        page.NavigationTree.ShouldBeNull();
    }

    [Fact]
    public void Given_OmittedNavigation_When_Render_Invoked_Then_It_Should_CascadeEmptyNavigation()
    {
        // Arrange
        using var context = new BunitContext();

        // Act
        var cut = context.Render<CascadingMainLayoutBase>(parameters => parameters
            .AddChildContent<NavigationPageProbe>());

        // Assert
        cut.FindComponent<NavigationPageProbe>().Instance.PageNavigation.ShouldBe(new PageNavigation());
    }

    [Fact]
    public void Given_UpdatedNavigation_When_Render_Invoked_Then_It_Should_UpdateTheTypedCascade()
    {
        // Arrange
        using var context = new BunitContext();
        var initial = new PageNavigation { Next = new PageNavigationLink { Title = "Next", Url = "next" } };
        var updated = new PageNavigation { Previous = new PageNavigationLink { Title = "Previous", Url = "previous" } };
        var cut = context.Render<CascadingMainLayoutBase>(parameters => parameters
            .Add(p => p.PageNavigation, initial)
            .AddChildContent<NavigationPageProbe>());

        // Act
        cut.Render(parameters => parameters.Add(p => p.PageNavigation, updated));

        // Assert
        cut.FindComponent<NavigationPageProbe>().Instance.PageNavigation.ShouldBeSameAs(updated);
        initial.Next.ShouldNotBeNull();
        initial.Previous.ShouldBeNull();
    }

    public sealed class NavigationLayoutProbe : MainLayoutBase
    {
    }

    public sealed class NavigationPageProbe : PageViewBase
    {
        [CascadingParameter]
        public IEnumerable<ContentDocument>? Documents { get; set; }

        [CascadingParameter]
        public IReadOnlyList<ContentDocument>? NavigationPages { get; set; }

        [CascadingParameter]
        public IReadOnlyList<NavigationNode>? NavigationTree { get; set; }
    }
}
