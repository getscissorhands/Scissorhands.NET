using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;
using ScissorHands.Core.Services;
using ScissorHands.Theme;
using ScissorHands.Web.Renderers;

namespace ScissorHands.Web.Tests.Renderers;

public class ComponentRendererLocaleContextTests
{
    [Theory]
    [InlineData(false, false, "ko-KR", "ko-kr", "tags/c%23")]
    [InlineData(true, false, "ko-KR", "ko-kr", "tags/c%23")]
    [InlineData(true, false, "", "en-us", "tags/c%23")]
    [InlineData(true, true, "ko-KR", "fr-ca", "fr-ca/tags/c%23")]
    [InlineData(true, true, "", "fr-ca", "fr-ca/tags/c%23")]
    public async Task Given_SourceLessDocumentAndOptionalContext_When_RenderAsync_Invoked_Then_It_Should_PreserveDirectRenderingAndLocaleFallback(
        bool supplyParameter, bool supplyContext, string documentLocale, string expectedLocale, string expectedTagUrl)
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(Substitute.For<IThemeService>());
        using var provider = services.BuildServiceProvider();
        var renderer = new ComponentRenderer(
            provider.GetRequiredService<IServiceScopeFactory>(),
            provider.GetRequiredService<ILoggerFactory>());
        var site = new SiteManifest { Title = "My site", BaseUrl = "/docs/", Locales = ["en-US"] };
        var locale = supplyContext
            ? new LocaleContext { Locale = "fr-ca", Route = "fr-ca/about", HomeUrl = "fr-ca/", TagIndexUrl = "fr-ca/tags" }
            : null;
        var document = new ContentDocument
        {
            Metadata = new ContentMetadata { Title = "About", Slug = "fr-ca/about", Locale = documentLocale, Tags = ["C#", "Topic/Subtopic"] },
            Html = "<h1>About</h1>",
        };
        var tree = new[] { new NavigationNode { Title = "Prepared", Path = "prepared", Url = "prepared%20route" } };
        var parameters = new Dictionary<string, object?>
        {
            ["Site"] = site,
            ["Theme"] = new ThemeManifest { Slug = "default" },
            ["Document"] = document,
            ["NavigationPages"] = new[] { document },
            ["NavigationTree"] = tree,
            ["PageNavigation"] = new PageNavigation { Next = new PageNavigationLink { Title = "Next", Url = "fr-ca/next" } },
        };
        if (supplyParameter)
        {
            parameters["LocaleContext"] = locale;
        }

        // Act
        var html = await renderer.RenderAsync<PageView>(
            typeof(MainLayout), parameters, Xunit.TestContext.Current.CancellationToken);

        // Assert
        html.ShouldContain($"<html lang=\"{expectedLocale}\">");
        html.ShouldContain("<title>About | My site</title>");
        html.ShouldContain("<h1>About</h1>");
        html.ShouldContain("<base href=\"/docs/\"");
        html.ShouldContain($"href=\"{expectedTagUrl}\"");
        html.ShouldContain($"href=\"{(supplyContext ? "fr-ca/" : "")}tags/topic%2Fsubtopic\"");
        html.ShouldContain($"class=\"site-title\" href=\"{(supplyContext ? "fr-ca/" : ".")}\"");
        html.ShouldContain("href=\"prepared%20route\"");
        html.ShouldContain("href=\"fr-ca/next\"");
        html.ShouldNotContain("href=\"/docs/tags/");
        html.ShouldNotContain("href=\"/docs/fr-ca/");
        html.ShouldNotContain("%252F");
        parameters["NavigationTree"].ShouldBeSameAs(tree);
        parameters.ContainsKey("LocaleContext").ShouldBe(supplyParameter);
        document.SourcePath.ShouldBeEmpty();
        document.Metadata.Locale.ShouldBe(documentLocale);
        site.Locales.ShouldBe(["en-US"]);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Given_LegacyViewAndLocaleContext_When_RenderAsync_Invoked_Then_It_Should_FilterLocaleAttributesAndRetainNavigationCompatibility(bool supplyTree)
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(Substitute.For<IThemeService>());
        using var provider = services.BuildServiceProvider();
        var renderer = new ComponentRenderer(
            provider.GetRequiredService<IServiceScopeFactory>(),
            provider.GetRequiredService<ILoggerFactory>());
        var pages = new[]
        {
            new ContentDocument { Kind = ContentKind.Page, Metadata = new ContentMetadata { Title = "Source-less", Slug = "loose" } },
        };
        var locale = new LocaleContext { Locale = "ko-kr", Route = "ko-kr", HomeUrl = "ko-kr/" };
        var parameters = new Dictionary<string, object?>
        {
            ["Site"] = new SiteManifest(),
            ["NavigationPages"] = pages,
            ["LocaleContext"] = locale,
        };
        if (supplyTree)
        {
            parameters["NavigationTree"] = new[] { new NavigationNode { Title = "Explicit", Path = "explicit", Url = "explicit" } };
        }

        // Act
        var html = await renderer.RenderAsync<LegacyView>(
            typeof(LegacyLayout), parameters, Xunit.TestContext.Current.CancellationToken);

        // Assert
        html.ShouldContain(supplyTree ? "Tree:Explicit" : "Tree:Source-less");
        html.ShouldContain("Legacy body");
        parameters["LocaleContext"].ShouldBeSameAs(locale);
        parameters["NavigationPages"].ShouldBeSameAs(pages);
        parameters.ContainsKey("NavigationTree").ShouldBe(supplyTree);
    }

    [Fact]
    public async Task Given_SyntheticTagIndexContext_When_RenderAsync_Invoked_Then_It_Should_UsePreparedLanguageAndKeepTheHeading()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(Substitute.For<IThemeService>());
        using var provider = services.BuildServiceProvider();
        var renderer = new ComponentRenderer(
            provider.GetRequiredService<IServiceScopeFactory>(),
            provider.GetRequiredService<ILoggerFactory>());
        var locale = new LocaleContext { Locale = "ko-kr", Route = "ko-kr/tags", HomeUrl = "ko-kr/", TagIndexUrl = "ko-kr/tags" };
        var document = new ContentDocument { Metadata = new ContentMetadata { Title = "Tags", Slug = locale.Route } };
        var parameters = new Dictionary<string, object?>
        {
            ["Site"] = new SiteManifest { Title = "My site", Locales = ["en-US"] },
            ["Theme"] = new ThemeManifest(),
            ["Document"] = document,
            ["LocaleContext"] = locale,
        };

        // Act
        var html = await renderer.RenderAsync<TagListView>(
            typeof(MainLayout), parameters, Xunit.TestContext.Current.CancellationToken);

        // Assert
        html.ShouldContain("<html lang=\"ko-kr\">");
        html.ShouldContain("<title>Tags | My site</title>");
        html.ShouldContain("<h1>Tags</h1>");
        html.ShouldContain("href=\"ko-kr/tags\"");
        document.Metadata.Slug.ShouldBe(locale.Route);
    }

    private sealed class LegacyLayout : MainLayoutBase
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.AddContent(0, $"Tree:{string.Join("|", NavigationTree.Select(node => node.Title))}");
            builder.AddContent(1, Body);
        }
    }

    private sealed class LegacyView : ComponentBase
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder) => builder.AddContent(0, "Legacy body");
    }
}
