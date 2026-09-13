using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;
using ScissorHands.Core.Services;
using ScissorHands.Theme;

namespace ScissorHands.Web.Tests.Themes;

public class DefaultPageNavigationTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Given_NoNeighbors_When_Render_Invoked_Then_It_Should_OmitThePagerAndPreservePageContent(bool includeNavigation)
    {
        // Arrange
        using var context = new BunitContext();
        var document = new ContentDocument { Html = "<p>Page content.</p>" };

        // Act
        var cut = context.Render<PageView>(parameters =>
        {
            parameters.AddCascadingValue(document);
            if (includeNavigation)
            {
                parameters.AddCascadingValue(new PageNavigation());
            }
        });

        // Assert
        cut.Find("article > p").TextContent.ShouldBe("Page content.");
        cut.FindAll(".page-navigation").ShouldBeEmpty();
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void Given_AvailableNeighbors_When_Render_Invoked_Then_It_Should_ShowOnlySuppliedLinksBelowPageContent(bool hasPrevious, bool hasNext)
    {
        // Arrange
        using var context = new BunitContext();
        var previous = new PageNavigationLink { Title = "Start & <overview>", Url = "." };
        var next = new PageNavigationLink { Title = "Next \"quoted\" & steps", Url = "en-us/guides/next%20%26%20steps" };
        var navigation = new PageNavigation
        {
            Previous = hasPrevious ? previous : null,
            Next = hasNext ? next : null,
        };
        var document = new ContentDocument
        {
            Html = "<p>Page content.</p>",
            Metadata = new ContentMetadata { Tags = ["guide"] },
        };

        // Act
        var cut = context.Render<PageView>(parameters => parameters
            .AddCascadingValue(document)
            .AddCascadingValue(navigation));

        // Assert
        var region = cut.Find("section.page > article + nav.page-navigation");
        region.GetAttribute("aria-label").ShouldBe("Page navigation");
        region.QuerySelectorAll("a").Length.ShouldBe((hasPrevious ? 1 : 0) + (hasNext ? 1 : 0));
        cut.Find("article").LastElementChild.ShouldBe(cut.Find(".tag-list"));
        cut.FindAll(".page-navigation-previous").Count.ShouldBe(hasPrevious ? 1 : 0);
        cut.FindAll(".page-navigation-next").Count.ShouldBe(hasNext ? 1 : 0);
        if (hasPrevious)
        {
            var link = cut.Find("a.page-navigation-previous");
            link.GetAttribute("rel").ShouldBe("prev");
            link.GetAttribute("href").ShouldBe(previous.Url);
            link.QuerySelector(".page-navigation-direction")!.TextContent.ShouldBe("Previous");
            link.QuerySelector("span:last-child")!.TextContent.ShouldBe(previous.Title);
            link.InnerHtml.ShouldContain("&lt;overview&gt;");
        }

        if (hasNext)
        {
            var link = cut.Find("a.page-navigation-next");
            link.GetAttribute("rel").ShouldBe("next");
            link.GetAttribute("href").ShouldBe(next.Url);
            link.QuerySelector(".page-navigation-direction")!.TextContent.ShouldBe("Next");
            link.QuerySelector("span:last-child")!.TextContent.ShouldBe(next.Title);
            link.InnerHtml.ShouldContain("&amp; steps");
        }
    }

    [Theory]
    [InlineData("<img src=x onerror=alert(1)>")]
    [InlineData("<script>alert('title')</script>")]
    [InlineData("</span><svg onload=alert(1)></svg>")]
    [InlineData("<a href=\"javascript:alert(1)\">click</a>")]
    public void Given_MarkupInNeighborTitles_When_Render_Invoked_Then_It_Should_RenderEncodedTextWithoutExecutableElements(string title)
    {
        // Arrange
        using var context = new BunitContext();
        var navigation = new PageNavigation
        {
            Previous = new PageNavigationLink { Title = title, Url = "previous" },
            Next = new PageNavigationLink { Title = title, Url = "next" },
        };

        // Act
        var cut = context.Render<PageView>(parameters => parameters.AddCascadingValue(navigation));

        // Assert
        var region = cut.Find("nav.page-navigation");
        region.QuerySelectorAll("a").Length.ShouldBe(2);
        region.QuerySelectorAll("img, script, svg, iframe, [onerror], [onload], [onclick]").ShouldBeEmpty();
        foreach (var link in region.QuerySelectorAll("a"))
        {
            link.Children.Length.ShouldBe(2);
            link.QuerySelector("span:last-child")!.TextContent.ShouldBe(title);
            link.InnerHtml.ShouldContain("&lt;");
        }
    }

    [Theory]
    [InlineData("/", ".", "https://example.com/")]
    [InlineData("/site/", ".", "https://example.com/site/")]
    [InlineData("/", "ko-kr/guides/a%20%26%20b", "https://example.com/ko-kr/guides/a%20%26%20b")]
    [InlineData("/site/", "ko-kr/guides/a%20%26%20b", "https://example.com/site/ko-kr/guides/a%20%26%20b")]
    public void Given_PreformattedUrl_When_Render_Invoked_Then_It_Should_PreserveTheBaseRelativeHref(string baseUrl, string url, string expected)
    {
        // Arrange
        using var context = new BunitContext();
        var navigation = new PageNavigation { Next = new PageNavigationLink { Title = "Next", Url = url } };

        // Act
        var cut = context.Render<PageView>(parameters => parameters.AddCascadingValue(navigation));

        // Assert
        var href = cut.Find(".page-navigation-next").GetAttribute("href");
        href.ShouldBe(url);
        new Uri(new Uri($"https://example.com{baseUrl}"), href).AbsoluteUri.ShouldBe(expected);
    }

    [Fact]
    public void Given_LayoutNavigation_When_Render_Invoked_Then_It_Should_ForwardPageLinksWithoutCascadingFullCollections()
    {
        // Arrange
        using var context = new BunitContext();
        context.Services.AddSingleton(Substitute.For<IThemeService>());
        var page = new ContentDocument { Html = "<p>Current page.</p>" };
        var pages = new[] { page };
        var tree = new[] { new NavigationNode { Title = "Header link", Path = "header", Url = "header" } };
        var navigation = new PageNavigation { Next = new PageNavigationLink { Title = "Next page", Url = "next" } };
        RenderFragment body = builder =>
        {
            builder.OpenComponent<PageView>(0);
            builder.CloseComponent();
            builder.OpenComponent<NavigationCascadeProbe>(1);
            builder.CloseComponent();
        };

        // Act
        var cut = context.Render<MainLayout>(parameters => parameters
            .Add(p => p.Site, new SiteManifest())
            .Add(p => p.Theme, new ThemeManifest())
            .Add(p => p.Document, page)
            .Add(p => p.NavigationPages, pages)
            .Add(p => p.NavigationTree, tree)
            .Add(p => p.PageNavigation, navigation)
            .Add(p => p.Body, body));

        // Assert
        cut.Instance.NavigationPages.ShouldBeSameAs(pages);
        cut.Instance.NavigationTree.ShouldBeSameAs(tree);
        cut.FindComponent<CascadingMainLayoutBase>().Instance.PageNavigation.ShouldBeSameAs(navigation);
        cut.FindComponent<PageView>().Instance.PageNavigation.ShouldBeSameAs(navigation);
        cut.Find("main .page-navigation-next").GetAttribute("href").ShouldBe("next");
        cut.Find(".navigation-list a[href='header']").TextContent.ShouldBe("Header link");
        var probe = cut.FindComponent<NavigationCascadeProbe>().Instance;
        probe.PageNavigation.ShouldBeSameAs(navigation);
        probe.Document.ShouldBeSameAs(page);
        probe.NavigationPages.ShouldBeNull();
        probe.NavigationTree.ShouldBeNull();
    }

    [Fact]
    public void Given_LayoutWithoutNavigationParameter_When_Render_Invoked_Then_It_Should_PreserveDefaultBehavior()
    {
        // Arrange
        using var context = new BunitContext();
        context.Services.AddSingleton(Substitute.For<IThemeService>());
        RenderFragment body = builder =>
        {
            builder.OpenComponent<PageView>(0);
            builder.CloseComponent();
        };

        // Act
        var cut = context.Render<MainLayout>(parameters => parameters
            .Add(p => p.Site, new SiteManifest())
            .Add(p => p.Theme, new ThemeManifest())
            .Add(p => p.Body, body));

        // Assert
        cut.Instance.PageNavigation.ShouldBe(new PageNavigation());
        cut.Instance.NavigationPages.ShouldBeEmpty();
        cut.Instance.NavigationTree.ShouldBeEmpty();
        cut.FindComponent<PageView>().Instance.PageNavigation.ShouldBe(new PageNavigation());
        cut.FindAll(".page-navigation").ShouldBeEmpty();
        cut.FindAll(".navigation-list a").Select(link => link.TextContent).ShouldBe(["Home", "Tags"]);
    }

    [Fact]
    public void Given_RemovedNeighbors_When_Render_Invoked_Then_It_Should_RemoveThePager()
    {
        // Arrange
        using var context = new BunitContext();
        var navigation = new PageNavigation { Next = new PageNavigationLink { Title = "Next", Url = "next" } };
        var cut = context.Render<CascadingMainLayoutBase>(parameters => parameters
            .Add(p => p.PageNavigation, navigation)
            .AddChildContent<PageView>());

        // Act
        cut.Render(parameters => parameters.Add(p => p.PageNavigation, new PageNavigation()));

        // Assert
        cut.FindAll(".page-navigation").ShouldBeEmpty();
    }

    public sealed class NavigationCascadeProbe : PageViewBase
    {
        [CascadingParameter]
        public IReadOnlyList<ContentDocument>? NavigationPages { get; set; }

        [CascadingParameter]
        public IReadOnlyList<NavigationNode>? NavigationTree { get; set; }
    }
}
