using Microsoft.Extensions.DependencyInjection;

using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;
using ScissorHands.Core.Services;
using ScissorHands.Web.Navigation;

namespace ScissorHands.Web.Tests.Themes;

public class DefaultNavigationTests
{
    [Fact]
    public void Given_PreparedTree_When_Rendered_Then_It_Should_PreserveTheEngineStructureAndOrder()
    {
        using var context = new BunitContext();
        context.Services.AddSingleton(Substitute.For<IThemeService>());
        var tree = new[]
        {
            new NavigationNode
            {
                Title = "<img src=x>",
                Path = "group",
                Children =
                [
                    new() { Title = "Zebra", Path = "unrelated/z", Url = "unrelated/z" },
                    new() { Title = "Alpha", Path = "another/a", Url = "another/a" },
                ],
            },
        };

        var cut = context.Render<MainLayout>(parameters => parameters
            .Add(p => p.Site, new SiteManifest())
            .Add(p => p.Theme, new ThemeManifest())
            .Add(p => p.NavigationTree, tree));

        cut.Find(".navigation-label").TextContent.ShouldBe("<img src=x>");
        cut.FindAll("nav img").ShouldBeEmpty();
        cut.FindAll(".navigation-children a").Select(link => link.TextContent).ShouldBe(["Zebra", "Alpha"]);
        cut.FindAll(".navigation-children a").Select(link => link.GetAttribute("href")).ShouldBe(["unrelated/z", "another/a"]);
        cut.Find(".navigation-children").Id.ShouldBe(cut.Find(".navigation-toggle").GetAttribute("aria-controls"));
    }

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
        var site = new SiteManifest
        {
            BaseUrl = "/site/",
            Locale = "ja-jp",
            LocalizationFallbackMessages = new Dictionary<string, string?> { ["en-us"] = "English unavailable", ["ko-kr"] = "Korean unavailable" }
        };

        var cut = context.Render<MainLayout>(parameters => parameters
            .Add(p => p.Site, site)
            .Add(p => p.Theme, new ThemeManifest())
            .Add(p => p.NavigationPages, pages)
            .Add(p => p.NavigationTree, BuildTree(pages, site)));

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
            .Add(p => p.NavigationPages, new[] { docs, github, netlify })
            .Add(p => p.NavigationTree, BuildTree([docs, github, netlify])));

        var label = cut.Find(".navigation-label");
        label.TextContent.ShouldBe("Deployment");
        label.HasAttribute("href").ShouldBeFalse();
        label.Closest("a").ShouldBeNull();
        label.Closest("li")!.QuerySelectorAll(":scope > ul > li > .navigation-link > a")
            .Select(link => link.TextContent).ShouldBe(["GitHub Pages", "Netlify"]);
        cut.FindAll("nav a[href='docs/deployment']").ShouldBeEmpty();
        cut.Instance.NavigationPages.Count.ShouldBe(3);

        cut.Render(parameters => parameters.Add(p => p.NavigationTree, BuildTree([docs, github])));

        cut.Find(".navigation-label").TextContent.ShouldBe("Deployment");
        cut.FindAll("nav a").Select(link => link.TextContent).ShouldBe(["Home", "Docs", "GitHub Pages", "Tags"]);

        cut.Render(parameters => parameters.Add(p => p.NavigationTree,
            BuildTree([docs, Page("Deploy the site", "docs/deployment"), github])));

        cut.FindAll(".navigation-label").ShouldBeEmpty();
        cut.Find("nav a[href='docs/deployment']").TextContent.ShouldBe("Deploy the site");

        cut.Render(parameters => parameters.Add(p => p.NavigationTree, BuildTree([docs])));

        cut.FindAll(".navigation-label").ShouldBeEmpty();
        cut.FindAll(".navigation-children").ShouldBeEmpty();
        cut.FindAll(".navigation-toggle").ShouldBeEmpty();
        cut.FindAll("nav a").Select(link => link.TextContent).ShouldBe(["Home", "Docs", "Tags"]);
    }

    [Fact]
    public void Given_MultipleMissingAncestors_When_Rendered_Then_It_Should_PreserveEveryLevel()
    {
        using var context = new BunitContext();
        context.Services.AddSingleton(Substitute.For<IThemeService>());
        var cut = context.Render<MainLayout>(parameters => parameters
            .Add(p => p.Site, new SiteManifest())
            .Add(p => p.Theme, new ThemeManifest())
            .Add(p => p.NavigationTree, BuildTree([Page("GitHub Pages", "docs/deployment/github-pages")])));

        cut.FindAll(".navigation-label").Select(label => label.TextContent).ShouldBe(["Docs", "Deployment"]);
        cut.FindAll(".navigation-list > .navigation-item").ShouldHaveSingleItem();
        cut.FindAll("nav a").Select(link => link.GetAttribute("href"))
            .ShouldBe([".", "docs/deployment/github-pages", "tags"]);
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
            .Add(p => p.NavigationTree, BuildTree([Page("Docs", "docs"), Page("Child", $"docs/{segment}/child")])));

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

        var site = new SiteManifest
        {
            Locale = "ko-kr",
            LocalizationFallbackMessages = new Dictionary<string, string?> { ["en-us"] = "English unavailable" }
        };
        var cut = context.Render<MainLayout>(parameters => parameters
            .Add(p => p.Site, site)
            .Add(p => p.Theme, new ThemeManifest())
            .Add(p => p.NavigationTree, BuildTree(pages, site)));

        cut.FindAll(".navigation-label").Select(label => label.TextContent).ShouldBe(["Docs"]);
        cut.FindAll(".navigation-list > .navigation-item").ShouldHaveSingleItem();
        cut.FindAll("nav a").Select(link => link.TextContent)
            .ShouldBe(localePage ? ["Home", "English", "Quickstart", "Tags"] : ["Home", "Quickstart", "Tags"]);
        cut.Find("nav a[href='en-us/docs/quickstart']").ShouldNotBeNull();
    }

    [Fact]
    public void Given_UpdatedPreparedTree_When_Rerendered_Then_It_Should_DisplayTheNewHierarchy()
    {
        using var context = new BunitContext();
        context.Services.AddSingleton(Substitute.For<IThemeService>());
        var pages = new[] { Page("Docs", "docs"), Page("Deployment", "docs/deployment"), Page("GitHub Pages", "docs/deployment/github-pages") };
        var cut = context.Render<MainLayout>(parameters => parameters
            .Add(p => p.Site, new SiteManifest())
            .Add(p => p.Theme, new ThemeManifest())
            .Add(p => p.NavigationTree, BuildTree(pages)));

        cut.FindAll(".navigation-children").Count.ShouldBe(2);
        cut.FindAll(".navigation-toggle").Select(toggle => toggle.GetAttribute("aria-controls")).Distinct().Count().ShouldBe(2);

        cut.Render(parameters => parameters.Add(p => p.NavigationTree, Array.Empty<NavigationNode>()));

        cut.FindAll(".navigation-item").ShouldBeEmpty();
        cut.FindAll(".navigation-toggle").ShouldBeEmpty();
        cut.FindAll("nav a").Select(link => link.TextContent).ShouldBe(["Home", "Tags"]);
    }

    private static IReadOnlyList<NavigationNode> BuildTree(IReadOnlyList<ContentDocument> pages, SiteManifest? site = null)
        => NavigationTreeBuilder.Build(pages, site, Xunit.TestContext.Current.CancellationToken);

    private static ContentDocument Page(string title, string slug, string? locale = null) => new()
    {
        Kind = ContentKind.Page,
        Metadata = new ContentMetadata { Title = title, Slug = slug, Locale = locale, ShowInNavigation = true },
    };
}
