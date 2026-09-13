using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;
using ScissorHands.Theme;
using ScissorHands.Web.Renderers;

namespace ScissorHands.Web.Tests.Renderers;

public class ComponentRendererCascadingParametersTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Given_ReadingOrderAndOptionalPageContext_When_Rendered_Then_It_Should_PreserveExplicitTreesAndLegacyViews(bool supplyTree)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(Substitute.For<ScissorHands.Core.Services.IThemeService>());
        using var provider = services.BuildServiceProvider();
        var renderer = new ComponentRenderer(
            provider.GetRequiredService<IServiceScopeFactory>(),
            provider.GetRequiredService<ILoggerFactory>());
        var pages = new[]
        {
            new ContentDocument { Kind = ContentKind.Page, Metadata = new() { Title = "A Source-less", Slug = "loose" } },
            new ContentDocument { Kind = ContentKind.Page, SourcePath = "02-second.md", Metadata = new() { Title = "B Second", Slug = "alpha" } },
            new ContentDocument { Kind = ContentKind.Page, SourcePath = "01-first.md", Metadata = new() { Title = "Z First", Slug = "zulu" } },
        };
        var parameters = new Dictionary<string, object?>
        {
            ["Site"] = new SiteManifest(),
            ["NavigationPages"] = pages,
            ["PageNavigation"] = new PageNavigation { Next = new PageNavigationLink { Title = "Next", Url = "next" } },
        };
        if (supplyTree)
        {
            parameters["NavigationTree"] = new[] { new NavigationNode { Title = "Explicit", Path = "explicit", Url = "explicit" } };
        }

        var html = await renderer.RenderAsync<TestNavigationContent>(typeof(TestNavigationLayout), parameters, Xunit.TestContext.Current.CancellationToken);

        html.ShouldContain(supplyTree ? "Tree:Explicit" : "Tree:Z First|B Second|A Source-less");
        html.ShouldContain("Content body");
        parameters.ContainsKey("NavigationTree").ShouldBe(supplyTree);
        parameters["NavigationPages"].ShouldBeSameAs(pages);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Given_CustomLayout_When_RenderingNavigation_Then_It_Should_ConsumePreparedDataWithoutLeakingParameters(bool supplyTree)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(Substitute.For<ScissorHands.Core.Services.IThemeService>());
        using var provider = services.BuildServiceProvider();
        var renderer = new ComponentRenderer(
            provider.GetRequiredService<IServiceScopeFactory>(),
            provider.GetRequiredService<ILoggerFactory>());
        var parameters = new Dictionary<string, object?>
        {
            ["Site"] = new SiteManifest(),
            ["NavigationPages"] = new[]
            {
                new ContentDocument
                {
                    Kind = ContentKind.Page,
                    Metadata = new ContentMetadata { Title = "Legacy page", Slug = "parent/child", ShowInNavigation = true },
                },
            },
        };
        if (supplyTree)
        {
            parameters["NavigationTree"] = new[] { new NavigationNode { Title = "Prepared tree", Path = "prepared", Url = "prepared" } };
        }

        var html = await renderer.RenderAsync<TestNavigationContent>(typeof(TestNavigationLayout), parameters, Xunit.TestContext.Current.CancellationToken);

        html.ShouldContain("Flat:Legacy page");
        html.ShouldContain(supplyTree ? "Tree:Prepared tree" : "Tree:Parent|Legacy page");
        html.ShouldContain("Content body");
        parameters.ContainsKey("NavigationTree").ShouldBe(supplyTree);
    }

    [Fact]
    public async Task Given_ComponentRenderer_When_RenderingWithLayout_Then_It_Should_PassCascadingValuesViaLayout_NotAsComponentAttributes()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        using var provider = services.BuildServiceProvider();

        var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();
        var loggerFactory = provider.GetRequiredService<ILoggerFactory>();

        var renderer = new ComponentRenderer(scopeFactory, loggerFactory);

        var document = new ContentDocument
        {
            Metadata = new ContentMetadata { Title = "Hello", Slug = "hello" }
        };
        var plugins = new List<PluginManifest> { new() { Id = "test" } };
        var theme = new ThemeManifest { Name = "test", Slug = "test" };
        var site = new SiteManifest();

        var parameters = new Dictionary<string, object?>
        {
            ["Document"] = document,
            ["Plugins"] = plugins,
            ["Theme"] = theme,
            ["Site"] = site,
            ["Extra"] = "extra"
        };

        // Act
        var html = await renderer.RenderAsync<TestCascadingPageView>(typeof(TestCascadingLayout), parameters, Xunit.TestContext.Current.CancellationToken);

        // Assert
        html.ShouldContain("extra");
        html.ShouldContain("Hello");
    }

    [Fact]
    public async Task Given_DefaultThemeAndBaseUrl_When_Rendered_Then_LinksShouldResolveBelowBaseUrl()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(Substitute.For<ScissorHands.Core.Services.IThemeService>());
        using var provider = services.BuildServiceProvider();
        var renderer = new ComponentRenderer(
            provider.GetRequiredService<IServiceScopeFactory>(),
            provider.GetRequiredService<ILoggerFactory>());
        var site = new SiteManifest { BaseUrl = "/docs/", Locale = "ko-KR" };
        var theme = new ThemeManifest
        {
            Name = "Minimal",
            Slug = "minimal",
            Stylesheets = ["/assets/css/theme.css"],
            Scripts = ["/assets/js/theme.js"],
        };
        var document = new ContentDocument
        {
            Metadata = new ContentMetadata { Title = "About", Slug = "about", Locale = "ko-KR", Tags = ["C#", "  Mixed Case  "] },
            Html = "<p>About</p>",
        };
        var parameters = new Dictionary<string, object?>
        {
            ["Site"] = site,
            ["Theme"] = theme,
            ["Document"] = document,
            ["Plugins"] = Array.Empty<PluginManifest>(),
        };

        var html = await renderer.RenderAsync<ScissorHands.Web.PageView>(typeof(ScissorHands.Web.MainLayout), parameters, Xunit.TestContext.Current.CancellationToken);

        html.ShouldContain("<html lang=\"ko-kr\">");
        html.ShouldContain("<base href=\"/docs/\"");
        html.ShouldContain("href=\"themes/minimal/assets/css/theme.css\"");
        html.ShouldContain("src=\"themes/minimal/assets/js/theme.js\"");
        html.ShouldNotContain("href=\"/themes/");
        html.ShouldContain("aria-label=\"Page tags\"");
        html.ShouldContain("href=\"tags/c%23\"");
        html.ShouldContain("href=\"tags/mixed%20case\"");
        html.ShouldNotContain("href=\"/tags/");
    }

    private sealed class TestNavigationLayout : MainLayoutBase
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.AddContent(0, $"Flat:{string.Join("|", NavigationPages.Select(page => page.Metadata.Title))}");
            builder.AddContent(1, $"Tree:{string.Join("|", Titles(NavigationTree))}");
            builder.AddContent(2, Body);
        }

        private static IEnumerable<string> Titles(IEnumerable<NavigationNode> nodes)
        {
            foreach (var node in nodes)
            {
                yield return node.Title;
                foreach (var title in Titles(node.Children))
                {
                    yield return title;
                }
            }
        }
    }

    private sealed class TestNavigationContent : ComponentBase
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder) => builder.AddContent(0, "Content body");
    }

    private sealed class TestCascadingLayout : LayoutComponentBase
    {
        [Parameter]
        public IEnumerable<ContentDocument>? Documents { get; set; }

        [Parameter]
        public ContentDocument? Document { get; set; }

        [Parameter]
        public IEnumerable<PluginManifest>? Plugins { get; set; }

        [Parameter]
        public ThemeManifest? Theme { get; set; }

        [Parameter]
        public SiteManifest? Site { get; set; }

        [Parameter]
        public string? Extra { get; set; }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenComponent<CascadingMainLayoutBase>(0);
            builder.AddAttribute(1, nameof(CascadingMainLayoutBase.Documents), Documents);
            builder.AddAttribute(2, nameof(CascadingMainLayoutBase.Document), Document);
            builder.AddAttribute(3, nameof(CascadingMainLayoutBase.Plugins), Plugins);
            builder.AddAttribute(4, nameof(CascadingMainLayoutBase.Theme), Theme);
            builder.AddAttribute(5, nameof(CascadingMainLayoutBase.Site), Site);
            builder.AddAttribute(6, nameof(CascadingMainLayoutBase.ChildContent), (RenderFragment)(b => b.AddContent(0, Body)));
            builder.CloseComponent();
        }
    }

    private sealed class TestCascadingPageView : PageViewBase
    {
        [Parameter]
        public string? Extra { get; set; }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "div");
            builder.AddContent(1, $"{Extra}|{Document?.Metadata.Title}");
            builder.CloseElement();
        }
    }
}
