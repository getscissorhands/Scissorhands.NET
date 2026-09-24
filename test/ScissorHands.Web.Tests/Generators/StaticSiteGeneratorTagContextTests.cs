using System.IO.Abstractions.TestingHelpers;

using AngleSharp.Html.Parser;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;
using ScissorHands.Core.Services;
using ScissorHands.Plugin;
using ScissorHands.Web.Generators;
using ScissorHands.Web.Loaders;
using ScissorHands.Web.Renderers;
using ScissorHands.Web.Runners;
using ScissorHands.Web.Tests.TestDoubles;

namespace ScissorHands.Web.Tests.Generators;

public class StaticSiteGeneratorTagContextTests
{
    [Theory]
    [InlineData(false, "/", false, true)]
    [InlineData(true, "/", true, true)]
    [InlineData(false, "/blog/", true, true)]
    [InlineData(true, "/blog/", false, true)]
    [InlineData(false, "/", false, false)]
    [InlineData(true, "/blog/", true, false)]
    public async Task Given_TaggedContent_When_BuildInvoked_Then_It_Should_ExposeResolvedRoutesWithoutChangingMetadata(
        bool preview, string baseUrl, bool customNotFound, bool useLocale)
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var root = fileSystem.Path.GetPathRoot(Environment.CurrentDirectory) ?? fileSystem.Path.DirectorySeparatorChar.ToString();
        var baseRoot = fileSystem.Path.Combine(root, "base");
        var paths = new TestAppPaths(baseRoot, fileSystem.Path.Combine(baseRoot, "contents"), fileSystem.Path.Combine(baseRoot, "themes"));
        var destination = fileSystem.Path.Combine(baseRoot, preview ? "preview" : "dist");
        var site = new SiteManifest
        {
            Title = "Site title",
            Description = "Site description",
            Locale = useLocale ? "ko-KR" : null,
            SiteUrl = "https://example.com",
            BaseUrl = baseUrl,
        };
        var prefix = string.Empty;
        var post = new ContentDocument
        {
            Kind = ContentKind.Post,
            Markdown = "Post body",
            Metadata = new ContentMetadata
            {
                Title = "Post title",
                Slug = "ko-kr/post",
                Author = "Post author",
                TwitterHandle = "@post",
                Published = new DateTimeOffset(2026, 9, 1, 0, 0, 0, TimeSpan.Zero),
                HeroImage = "post.jpg",
                Tags = ["Plugins", "  C# / <Tools>  "],
            },
        };
        var page = new ContentDocument
        {
            Kind = ContentKind.Page,
            Markdown = "Page body",
            Metadata = new ContentMetadata { Title = "Page title", Slug = "ko-kr/page", Tags = ["Plugins"], ShowInNavigation = true },
        };
        var documents = new List<ContentDocument> { post, page };
        if (customNotFound)
        {
            documents.Add(new ContentDocument
            {
                Kind = ContentKind.Page,
                Markdown = "Custom missing body",
                Metadata = new ContentMetadata { Title = "Custom missing", Slug = "404.html" },
            });
        }

        var loader = Substitute.For<IContentLoader>();
        loader.LoadAsync(Arg.Any<CancellationToken>()).Returns(documents);
        var markdownService = Substitute.For<IMarkdownService>();
        markdownService.ToHtmlAsync(Arg.Any<string>(), Arg.Any<bool?>(), Arg.Any<CancellationToken>())
            .Returns(call => $"<p>{call.ArgAt<string>(0)}</p>");
        var themeService = Substitute.For<IThemeService>();
        themeService.LoadManifestAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new ThemeManifest { Slug = "default" });
        var observations = new RouteObservations();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(themeService);
        services.AddSingleton(observations);
        using var provider = services.BuildServiceProvider();
        var renderer = new ComponentRenderer(
            provider.GetRequiredService<IServiceScopeFactory>(),
            provider.GetRequiredService<ILoggerFactory>());
        var hookDocuments = new Dictionary<string, ContentDocument>();
        var pluginRunner = Substitute.For<IPluginRunner>();
        pluginRunner.Manifests.Returns([new PluginManifest { Id = "route-probe" }]);
        pluginRunner.RunPreMarkdownAsync(Arg.Any<ContentDocument>(), Arg.Any<CancellationToken>())
            .Returns(call => call.ArgAt<ContentDocument>(0));
        pluginRunner.RunPostMarkdownAsync(Arg.Any<ContentDocument>(), Arg.Any<CancellationToken>())
            .Returns(call => call.ArgAt<ContentDocument>(0));
        pluginRunner.RunPostHtmlAsync(Arg.Any<string>(), Arg.Any<ContentDocument>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var html = call.ArgAt<string>(0);
                var document = call.ArgAt<ContentDocument>(1);
                hookDocuments.Add(document.Metadata.Slug, document);
                if (document.Metadata.Slug.StartsWith(prefix + "tags", StringComparison.Ordinal))
                {
                    document.Html.ShouldBe(html);
                    observations.Documents.ContainsKey(document.Metadata.Slug).ShouldBeTrue();
                }
                return html;
            });
        var generator = new StaticSiteGenerator(
            loader, markdownService, pluginRunner, themeService, renderer, paths, fileSystem, site,
            Substitute.For<ILogger<StaticSiteGenerator>>());

        // Act
        await generator.BuildAsync<MainLayout, IndexView, PostView, PageView, NotFoundView, ProbeTagListView, ProbeTagView>(
            destination, preview, Xunit.TestContext.Current.CancellationToken);

        // Assert
        var parser = new HtmlParser();
        var expectedTags = new Dictionary<string, string>
        {
            [prefix + "tags"] = "Tags",
            [prefix + "tags/plugins"] = "Tag: plugins",
            [prefix + "tags/c%23%20%2F%20%3Ctools%3E"] = "Tag:   c# / <tools>  ",
        };
        observations.Documents.Keys.ShouldBe(expectedTags.Keys, ignoreOrder: true);
        foreach (var (route, hookTitle) in expectedTags)
        {
            var document = observations.Documents[route];
            document.Kind.ShouldBe(ContentKind.Page);
            document.SourcePath.ShouldBeEmpty();
            document.Markdown.ShouldBeEmpty();
            document.Html.ShouldBeEmpty();
            document.Metadata.Title.ShouldBeEmpty();
            document.Metadata.Description.ShouldBeNull();
            document.Metadata.Locale.ShouldBe(useLocale ? "ko-kr" : null);
            document.Metadata.Author.ShouldBeNull();
            document.Metadata.TwitterHandle.ShouldBeNull();
            document.Metadata.HeroImage.ShouldBeNull();
            document.Metadata.Published.ShouldBeNull();
            document.Metadata.Tags.ShouldBeEmpty();
            document.Metadata.ShowInNavigation.ShouldBeFalse();
            hookDocuments[route].Metadata.ShouldBe(document.Metadata with { Title = hookTitle });

            using var html = parser.ParseDocument(ReadOutput($"{route}/index.html"));
            html.Title.ShouldBe(site.Title);
            html.DocumentElement.GetAttribute("lang").ShouldBe(useLocale ? "ko-kr" : null);
            html.QuerySelector("meta[name='description']")!.GetAttribute("content").ShouldBe(site.Description);
            html.QuerySelector("base")!.GetAttribute("href").ShouldBe(baseUrl);
            var expectedUrl = $"https://example.com{baseUrl}{route}";
            html.QuerySelector("meta[property='og:url']")!.GetAttribute("content").ShouldBe(expectedUrl);
            PublicationUrl(site, hookDocuments[route]).ShouldBe(expectedUrl);
            html.QuerySelector("meta[name='route-preview']")!.GetAttribute("content").ShouldBe(preview.ToString());
            html.QuerySelectorAll(".site-header nav a").Select(link => link.TextContent).ShouldBe(["Home", "Page title", "Tags"]);
            html.QuerySelectorAll(".page-navigation").ShouldBeEmpty();
            html.QuerySelector("h1")!.TextContent.ShouldBe(route == prefix + "tags" ? "Tags" : $"#{hookTitle["Tag: ".Length..]}");
            html.QuerySelectorAll("tools").ShouldBeEmpty();
        }

        using var tagHtml = parser.ParseDocument(ReadOutput(prefix + "tags/plugins/index.html"));
        tagHtml.QuerySelectorAll("main a").Select(link => link.TextContent.Trim()).ShouldContain("Post title");
        tagHtml.QuerySelectorAll("main a").Select(link => link.TextContent.Trim()).ShouldContain("Page title");
        using var indexHtml = parser.ParseDocument(ReadOutput(prefix + "index.html"));
        indexHtml.Title.ShouldBe(site.Title);
        using var postHtml = parser.ParseDocument(ReadOutput("ko-kr/post/index.html"));
        postHtml.Title.ShouldBe("Post title | Site title");
        postHtml.Body!.TextContent.ShouldContain("Post body");
        using var pageHtml = parser.ParseDocument(ReadOutput("ko-kr/page/index.html"));
        pageHtml.Title.ShouldBe("Page title | Site title");
        pageHtml.Body!.TextContent.ShouldContain("Page body");
        using var missingHtml = parser.ParseDocument(ReadOutput("404.html"));
        missingHtml.Title.ShouldBe(customNotFound ? "Custom missing | Site title" : "404 - Not Found | Site title");
        hookDocuments["ko-kr/post"].ShouldBeSameAs(post);
        hookDocuments["ko-kr/page"].ShouldBeSameAs(page);
        await pluginRunner.Received(documents.Count).RunPreMarkdownAsync(Arg.Any<ContentDocument>(), Arg.Any<CancellationToken>());
        await pluginRunner.Received(documents.Count).RunPostMarkdownAsync(Arg.Any<ContentDocument>(), Arg.Any<CancellationToken>());

        string ReadOutput(string route) => fileSystem.File.ReadAllText(
            fileSystem.Path.Combine(destination, route.Replace('/', fileSystem.Path.DirectorySeparatorChar)));
    }

    private static string PublicationUrl(SiteManifest site, ContentDocument? document)
        => string.Join("/", new[] { site.SiteUrl.TrimEnd('/'), site.BaseUrl.Trim('/'), document?.Metadata.Slug }
            .Where(segment => !string.IsNullOrEmpty(segment)));

    private sealed class RouteObservations
    {
        public Dictionary<string, ContentDocument> Documents { get; } = new();
    }

    private sealed class RouteProbe : PluginComponentBase
    {
        [Inject]
        public RouteObservations Observations { get; set; } = null!;

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            if (Document is not null)
            {
                Observations.Documents.Add(Document.Metadata.Slug, Document);
            }
            builder.OpenElement(0, "meta");
            builder.AddAttribute(1, "property", "og:url");
            builder.AddAttribute(2, "content", PublicationUrl(Site!, Document));
            builder.CloseElement();
            builder.OpenElement(3, "meta");
            builder.AddAttribute(4, "name", "route-preview");
            builder.AddAttribute(5, "content", Site!.IsPreview.ToString());
            builder.CloseElement();
        }
    }

    private sealed class ProbeTagListView : TagListView
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            base.BuildRenderTree(builder);
            builder.OpenComponent<RouteProbe>(100);
            builder.AddAttribute(101, nameof(RouteProbe.Id), "route-probe");
            builder.CloseComponent();
        }
    }

    private sealed class ProbeTagView : TagView
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            base.BuildRenderTree(builder);
            builder.OpenComponent<RouteProbe>(100);
            builder.AddAttribute(101, nameof(RouteProbe.Id), "route-probe");
            builder.CloseComponent();
        }
    }
}
