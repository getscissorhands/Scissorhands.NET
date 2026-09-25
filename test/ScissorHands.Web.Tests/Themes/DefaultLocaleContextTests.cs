using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;
using ScissorHands.Core.Services;
using ScissorHands.Plugin;
using ScissorHands.Theme;

namespace ScissorHands.Web.Tests.Themes;

public class DefaultLocaleContextTests
{
    [Theory]
    [InlineData(null, null)]
    [InlineData(new string[0], null)]
    [InlineData(new[] { "ja-JP" }, "ja-jp")]
    [InlineData(new[] { "ja-JP", "en-US" }, "ja-jp")]
    public void Given_SiteLocales_When_Render_Invoked_Then_It_Should_AnnotateLanguageOnlyWhenConfigured(
        string[]? siteLocales, string? expectedLanguage)
    {
        // Arrange
        using var context = new BunitContext();
        context.Services.AddSingleton(Substitute.For<IThemeService>());
        var site = new SiteManifest { Locales = siteLocales! };

        // Act
        var cut = context.Render<MainLayout>(parameters => parameters
            .Add(p => p.Site, site)
            .Add(p => p.Theme, new ThemeManifest()));

        // Assert
        cut.Find("html").GetAttribute("lang").ShouldBe(expectedLanguage);
        site.Locales.ShouldBe(siteLocales ?? []);
    }

    [Theory]
    [InlineData(false, false, ".", "tags")]
    [InlineData(true, false, ".", "tags")]
    [InlineData(true, true, "ko-kr/", null)]
    [InlineData(true, true, "ko-kr/", "ko-kr/tags")]
    [InlineData(true, true, "fr%20ca/", "fr%20ca/tags")]
    [InlineData(true, true, ".", "")]
    public void Given_OptionalLocaleTargets_When_Render_Invoked_Then_It_Should_UsePreparedLinksAndOmitTagsOnlyForNullTarget(
        bool supplyParameter, bool supplyContext, string homeUrl, string? tagIndexUrl)
    {
        // Arrange
        using var context = new BunitContext();
        context.Services.AddSingleton(Substitute.For<IThemeService>());
        var locale = supplyContext ? new LocaleContext { Locale = "ko-kr", HomeUrl = homeUrl, TagIndexUrl = tagIndexUrl } : null;
        var site = new SiteManifest { Title = "My <site>", BaseUrl = "/docs/", Locales = ["en-US"] };
        RenderFragment body = builder =>
        {
            builder.OpenComponent<PageView>(0);
            builder.CloseComponent();
        };

        // Act
        var cut = context.Render<MainLayout>(parameters =>
        {
            parameters.Add(p => p.Site, site)
                .Add(p => p.Theme, new ThemeManifest())
                .Add(p => p.Body, body);
            if (supplyParameter)
            {
                parameters.Add(p => p.LocaleContext, locale);
            }
        });

        // Assert
        cut.Find(".site-title").GetAttribute("href").ShouldBe(homeUrl);
        cut.Find(".site-title").TextContent.ShouldBe("My <site>");
        cut.Find(".site-title").Children.ShouldBeEmpty();
        var links = cut.FindAll(".navigation-list a");
        links[0].TextContent.ShouldBe("Home");
        links[0].GetAttribute("href").ShouldBe(homeUrl);
        links.Count.ShouldBe(tagIndexUrl is null ? 1 : 2);
        if (tagIndexUrl is not null)
        {
            links[1].TextContent.ShouldBe("Tags");
            links[1].GetAttribute("href").ShouldBe(tagIndexUrl);
        }

        cut.Find("base").GetAttribute("href").ShouldBe("/docs/");
        cut.FindComponent<CascadingMainLayoutBase>().Instance.LocaleContext.ShouldBeSameAs(locale);
        cut.FindComponent<PageView>().Instance.LocaleContext.ShouldBeSameAs(locale);
        site.Locales.ShouldBe(["en-US"]);
    }

    [Theory]
    [InlineData(" C# ", "ko-kr/tags/c%23")]
    [InlineData(" Topic/Subtopic ", "ko-kr/tags/topic%2Fsubtopic")]
    public void Given_LocaleContext_When_Render_Invoked_Then_It_Should_ForwardToViewsAndPluginsWithoutCascadingNavigation(
        string tag, string expectedHref)
    {
        // Arrange
        using var context = new BunitContext();
        context.Services.AddSingleton(Substitute.For<IThemeService>());
        var locale = new LocaleContext { Locale = "ko-kr", Route = "ko-kr/tags", HomeUrl = "ko-kr/", TagIndexUrl = "ko-kr/tags" };
        var document = new ContentDocument
        {
            Metadata = new ContentMetadata { Title = "Current", Slug = locale.Route, Tags = [tag] },
        };
        var pages = new[] { document };
        var tree = new[] { new NavigationNode { Title = "Prepared", Path = "ko-kr/prepared", Url = "ko-kr/prepared" } };
        var navigation = new PageNavigation { Next = new PageNavigationLink { Title = "Next", Url = "ko-kr/next" } };
        var taggedDocuments = new Dictionary<string, (IEnumerable<ContentDocument> Posts, IEnumerable<ContentDocument> Pages)>
        {
            [tag] = ([document], []),
        };
        RenderFragment body = builder =>
        {
            builder.OpenComponent<PostView>(0);
            builder.CloseComponent();
            builder.OpenComponent<PageView>(1);
            builder.CloseComponent();
            builder.OpenComponent<TagListView>(2);
            builder.CloseComponent();
            builder.OpenComponent<PluginProbe>(3);
            builder.AddAttribute(4, nameof(PluginProbe.Id), "test");
            builder.CloseComponent();
        };

        // Act
        var cut = context.Render<MainLayout>(parameters => parameters
            .Add(p => p.Site, new SiteManifest { BaseUrl = "/docs/" })
            .Add(p => p.Theme, new ThemeManifest())
            .Add(p => p.Document, document)
            .Add(p => p.TaggedDocuments, taggedDocuments)
            .Add(p => p.NavigationPages, pages)
            .Add(p => p.NavigationTree, tree)
            .Add(p => p.PageNavigation, navigation)
            .Add(p => p.LocaleContext, locale)
            .Add(p => p.Body, body));

        // Assert
        foreach (var link in cut.FindAll(".tag-list a, .tag-card"))
        {
            link.GetAttribute("href").ShouldBe(expectedHref);
            new Uri(new Uri("https://example.com/docs/"), link.GetAttribute("href")!).AbsoluteUri
                .ShouldBe($"https://example.com/docs/{expectedHref}");
        }

        cut.FindAll(".tag-list a, .tag-card").Count.ShouldBe(3);
        cut.FindComponent<PostView>().Instance.LocaleContext.ShouldBeSameAs(locale);
        cut.FindComponent<PageView>().Instance.LocaleContext.ShouldBeSameAs(locale);
        cut.FindComponent<TagListView>().Instance.LocaleContext.ShouldBeSameAs(locale);
        cut.FindComponent<PageView>().Instance.PageNavigation.ShouldBeSameAs(navigation);
        cut.Find(".page-navigation-next").GetAttribute("href").ShouldBe("ko-kr/next");
        cut.Find(".navigation-list a[href='ko-kr/prepared']").TextContent.ShouldBe("Prepared");
        var plugin = cut.FindComponent<PluginProbe>().Instance;
        plugin.BoundLocaleContext.ShouldBeSameAs(locale);
        plugin.BoundDocument.ShouldBeSameAs(document);
        plugin.NavigationPages.ShouldBeNull();
        plugin.NavigationTree.ShouldBeNull();
        cut.Instance.NavigationPages.ShouldBeSameAs(pages);
        cut.Instance.NavigationTree.ShouldBeSameAs(tree);
    }

    public sealed class PluginProbe : PluginComponentBase
    {
        public LocaleContext? BoundLocaleContext => LocaleContext;
        public ContentDocument? BoundDocument => Document;

        [CascadingParameter]
        public IReadOnlyList<ContentDocument>? NavigationPages { get; set; }

        [CascadingParameter]
        public IReadOnlyList<NavigationNode>? NavigationTree { get; set; }
    }
}
