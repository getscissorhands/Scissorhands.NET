using Microsoft.Extensions.DependencyInjection;

using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;
using ScissorHands.Core.Services;

namespace ScissorHands.Web.Tests.Themes;

public class DefaultNavigationTests
{
    [Theory]
    [InlineData("docs", "docs/quickstart", true)]
    [InlineData("/docs/", "docs//deployment/github-pages", true)]
    [InlineData("docs", "docs\\deployment", true)]
    [InlineData("docs", "docs-other/quickstart", false)]
    [InlineData("en-us/docs", "en-us/docs/quickstart", true)]
    [InlineData("en-us/docs", "ko-kr/docs/quickstart", false)]
    [InlineData("guides/about & team", "guides/about & team/quickstart", true)]
    [InlineData("docs", "docs%2Fother/quickstart", false)]
    public void Given_PageSlugs_When_Rendered_Then_It_Should_GroupOnlyMatchingPathAncestors(
        string parentSlug,
        string childSlug,
        bool nested)
    {
        using var context = new BunitContext();
        context.Services.AddSingleton(Substitute.For<IThemeService>());
        var childLocale = childSlug.StartsWith("ko-kr/", StringComparison.Ordinal) ? "ko-KR" : "en-US";
        var pages = new[] { Page("Parent", parentSlug, "en-US"), Page("Child", childSlug, childLocale) };

        var cut = context.Render<MainLayout>(parameters => parameters
            .Add(p => p.Site, new SiteManifest { BaseUrl = "/site/", UseLocaleInUrl = true })
            .Add(p => p.Theme, new ThemeManifest())
            .Add(p => p.NavigationPages, pages));

        var topLevelPages = cut.FindAll(".navigation-list > .navigation-item");
        topLevelPages.Count.ShouldBe(nested ? 1 : 2);
        var parentItem = cut.FindAll("nav a").Single(link => link.TextContent == "Parent").Closest("li")!;
        var childLink = cut.FindAll("nav a").Single(link => link.TextContent == "Child");
        parentItem.QuerySelectorAll("a").Contains(childLink).ShouldBe(nested);
        if (nested)
        {
            var toggle = parentItem.QuerySelector(".navigation-toggle")!;
            toggle.GetAttribute("aria-label").ShouldBe("Toggle Parent pages");
            toggle.GetAttribute("aria-expanded").ShouldBe("false");
            toggle.HasAttribute("hidden").ShouldBeTrue();
            var children = parentItem.QuerySelector(".navigation-children")!;
            children.Id.ShouldBe(toggle.GetAttribute("aria-controls"));
            children.HasAttribute("hidden").ShouldBeFalse();
        }
    }

    [Fact]
    public void Given_MissingParent_When_VisibleChildrenChange_Then_It_Should_ShowOnlyNonEmptyGroups()
    {
        using var context = new BunitContext();
        context.Services.AddSingleton(Substitute.For<IThemeService>());
        var docs = Page("Docs", "docs");
        var github = Page("GitHub Pages", "docs/deployment/github-pages");
        var netlify = Page("Netlify", "docs/deployment/netlify");
        var cut = context.Render<MainLayout>(parameters => parameters
            .Add(p => p.Site, new SiteManifest())
            .Add(p => p.Theme, new ThemeManifest())
            .Add(p => p.NavigationPages, new[] { docs, github, netlify }));

        var label = cut.Find(".navigation-label");
        label.TextContent.ShouldBe("Deployment");
        label.HasAttribute("href").ShouldBeFalse();
        label.Closest("a").ShouldBeNull();
        label.Closest("li")!.QuerySelectorAll(":scope > ul > li > .navigation-link > a")
            .Select(link => link.TextContent).ShouldBe(["GitHub Pages", "Netlify"]);
        cut.FindAll("nav a[href='docs/deployment']").ShouldBeEmpty();
        cut.Instance.NavigationPages.Count.ShouldBe(3);

        cut.Render(parameters => parameters.Add(p => p.NavigationPages, new[] { docs, github }));

        cut.Find(".navigation-label").TextContent.ShouldBe("Deployment");
        cut.FindAll("nav a").Select(link => link.TextContent).ShouldBe(["Home", "Tags", "Docs", "GitHub Pages"]);

        cut.Render(parameters => parameters.Add(p => p.NavigationPages,
            new[] { docs, Page("Deploy the site", "docs/deployment"), github }));

        cut.FindAll(".navigation-label").ShouldBeEmpty();
        cut.Find("nav a[href='docs/deployment']").TextContent.ShouldBe("Deploy the site");

        cut.Render(parameters => parameters.Add(p => p.NavigationPages, new[] { docs }));

        cut.FindAll(".navigation-label").ShouldBeEmpty();
        cut.FindAll(".navigation-children").ShouldBeEmpty();
        cut.FindAll(".navigation-toggle").ShouldBeEmpty();
        cut.FindAll("nav a").Select(link => link.TextContent).ShouldBe(["Home", "Tags", "Docs"]);
    }

    [Fact]
    public void Given_MultipleMissingAncestors_When_Rendered_Then_It_Should_PreserveEveryLevel()
    {
        using var context = new BunitContext();
        context.Services.AddSingleton(Substitute.For<IThemeService>());
        var cut = context.Render<MainLayout>(parameters => parameters
            .Add(p => p.Site, new SiteManifest())
            .Add(p => p.Theme, new ThemeManifest())
            .Add(p => p.NavigationPages, new[] { Page("GitHub Pages", "docs/deployment/github-pages") }));

        cut.FindAll(".navigation-label").Select(label => label.TextContent).ShouldBe(["Docs", "Deployment"]);
        cut.FindAll(".navigation-list > .navigation-item").ShouldHaveSingleItem();
        cut.FindAll("nav a").Select(link => link.GetAttribute("href"))
            .ShouldBe([".", "tags", "docs/deployment/github-pages"]);
        cut.FindAll(".navigation-toggle").Count.ShouldBe(2);
    }

    [Theory]
    [InlineData("deployment-tools", "Deployment Tools")]
    [InlineData("deployment_tools", "Deployment Tools")]
    [InlineData("100% ready", "100% Ready")]
    [InlineData("<img src=x>", "<Img Src=X>")]
    public void Given_MissingParentSegment_When_Rendered_Then_It_Should_UseAnEncodedReadableLabel(string segment, string expected)
    {
        using var context = new BunitContext();
        context.Services.AddSingleton(Substitute.For<IThemeService>());
        var cut = context.Render<MainLayout>(parameters => parameters
            .Add(p => p.Site, new SiteManifest())
            .Add(p => p.Theme, new ThemeManifest())
            .Add(p => p.NavigationPages, new[] { Page("Docs", "docs"), Page("Child", $"docs/{segment}/child") }));

        cut.Find(".navigation-label").TextContent.ShouldBe(expected);
        cut.FindAll("nav img").ShouldBeEmpty();
        cut.FindAll(".navigation-label[href]").ShouldBeEmpty();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Given_LocalePrefix_When_Rendered_Then_It_Should_OnlyShowThePrefixIfItHasAVisiblePage(bool localePage)
    {
        using var context = new BunitContext();
        context.Services.AddSingleton(Substitute.For<IThemeService>());
        var pages = new List<ContentDocument> { Page("Quickstart", "en-us/docs/quickstart", "en_US") };
        if (localePage)
        {
            pages.Add(Page("English", "en-us", "en_US"));
        }

        var cut = context.Render<MainLayout>(parameters => parameters
            .Add(p => p.Site, new SiteManifest { UseLocaleInUrl = true })
            .Add(p => p.Theme, new ThemeManifest())
            .Add(p => p.NavigationPages, pages));

        cut.FindAll(".navigation-label").Select(label => label.TextContent).ShouldBe(["Docs"]);
        cut.FindAll(".navigation-list > .navigation-item").ShouldHaveSingleItem();
        cut.FindAll("nav a").Select(link => link.TextContent)
            .ShouldBe(localePage ? ["Home", "Tags", "English", "Quickstart"] : ["Home", "Tags", "Quickstart"]);
        cut.Find("nav a[href='en-us/docs/quickstart']").ShouldNotBeNull();
    }

    [Fact]
    public void Given_UpdatedNavigationPages_When_Rerendered_Then_It_Should_RebuildTheHierarchy()
    {
        using var context = new BunitContext();
        context.Services.AddSingleton(Substitute.For<IThemeService>());
        var pages = new[] { Page("Docs", "docs"), Page("Deployment", "docs/deployment"), Page("GitHub Pages", "docs/deployment/github-pages") };
        var cut = context.Render<MainLayout>(parameters => parameters
            .Add(p => p.Site, new SiteManifest())
            .Add(p => p.Theme, new ThemeManifest())
            .Add(p => p.NavigationPages, pages));

        cut.FindAll(".navigation-children").Count.ShouldBe(2);
        cut.FindAll(".navigation-toggle").Select(toggle => toggle.GetAttribute("aria-controls")).Distinct().Count().ShouldBe(2);

        cut.Render(parameters => parameters.Add(p => p.NavigationPages, Array.Empty<ContentDocument>()));

        cut.FindAll(".navigation-item").ShouldBeEmpty();
        cut.FindAll(".navigation-toggle").ShouldBeEmpty();
        cut.FindAll("nav a").Select(link => link.TextContent).ShouldBe(["Home", "Tags"]);
    }

    private static ContentDocument Page(string title, string slug, string? locale = null) => new()
    {
        Kind = ContentKind.Page,
        Metadata = new ContentMetadata { Title = title, Slug = slug, Locale = locale, ShowInNavigation = true },
    };
}
