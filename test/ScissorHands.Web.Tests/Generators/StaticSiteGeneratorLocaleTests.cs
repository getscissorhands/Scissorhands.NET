using System.IO.Abstractions.TestingHelpers;

using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;
using ScissorHands.Core.Services;
using ScissorHands.Web.Generators;
using ScissorHands.Web.Loaders;
using ScissorHands.Web.Renderers;
using ScissorHands.Web.Runners;
using ScissorHands.Web.Tests.TestDoubles;

namespace ScissorHands.Web.Tests.Generators;

public class StaticSiteGeneratorLocaleTests
{
    [Theory]
    [InlineData(false, "/")]
    [InlineData(true, "/")]
    [InlineData(false, "/blog/")]
    [InlineData(true, "/blog/")]
    [InlineData(false, "/blog")]
    [InlineData(true, "/blog")]
    [InlineData(false, "/manual/docs")]
    [InlineData(true, "/manual/docs")]
    public async Task Given_MultipleLocales_When_Generated_Then_It_Should_IsolateCollectionsNavigationAndRedirects(bool preview, string baseUrl)
    {
        var english = Page("English", "en-us/about", "en-US", @"en-us\01-about.md");
        var korean = Page("Korean", "ko-kr/about", "ko_KR", @"ko-kr\01-about.md");
        var koreanNext = Page("Korean next", "ko-kr/next", "ko-KR");
        var englishPost = Post("English post", "en-us/post", null, ["shared"]);
        var koreanPost = Post("Korean post", "ko-kr/post", "ko-KR", ["shared", "korean-only"]);
        var hidden = new ContentDocument
        {
            Kind = ContentKind.Page,
            Metadata = new() { Title = "Hidden Japanese", Slug = "ja-jp/hidden", Locale = "ja-JP" },
        };
        var draft = new ContentDocument
        {
            Kind = ContentKind.Post,
            Metadata = new() { Slug = "fr-fr/draft", Locale = "fr-FR", Draft = true, Tags = ["draft"] },
        };
        using var fixture = new Fixture([koreanNext, englishPost, koreanPost, korean, english, hidden, draft], baseUrl: baseUrl);
        var effectiveBaseUrl = baseUrl.EndsWith('/') ? baseUrl : baseUrl + "/";

        await fixture.BuildAsync(preview);

        fixture.Site.BaseUrl.ShouldBe(effectiveBaseUrl);
        fixture.Site.Locale.ShouldBe("en-US");
        fixture.Site.IsPreview.ShouldBe(preview);
        using var root = fixture.Html("index.html");
        root.QuerySelector("meta[http-equiv='refresh']")!.GetAttribute("content").ShouldBe($"0;url={effectiveBaseUrl}en-us/");
        root.QuerySelector("a")!.GetAttribute("href").ShouldBe($"{effectiveBaseUrl}en-us/");
        root.QuerySelectorAll("script").ShouldBeEmpty();
        foreach (var (locale, title, pageTitles) in new[]
        {
            ("en-us", "English post", new[] { "English" }),
            ("ko-kr", "Korean post", new[] { "Korean", "Korean next" }),
        })
        {
            using var index = fixture.Html($"{locale}/index.html");
            index.Title.ShouldBe("Site");
            index.DocumentElement.GetAttribute("lang").ShouldBe(locale);
            index.QuerySelectorAll(".post-link").Select(link => link.TextContent).ShouldBe([title]);
            index.QuerySelector(".site-title")!.GetAttribute("href").ShouldBe($"{locale}/");
            index.QuerySelectorAll(".site-header nav a").Select(link => link.TextContent)
                .ShouldBe(new[] { "Home" }.Concat(pageTitles).Append("Tags"));
            index.QuerySelectorAll(".site-header nav a").Last().GetAttribute("href").ShouldBe($"{locale}/tags");
            using var tags = fixture.Html($"{locale}/tags/shared/index.html");
            tags.QuerySelectorAll(".post-link").Select(link => link.TextContent.Trim()).ShouldBe([title]);
            tags.QuerySelector(".back-link")!.GetAttribute("href").ShouldBe($"{locale}/tags");
            using var post = fixture.Html($"{locale}/post/index.html");
            post.QuerySelector(".back-link")!.GetAttribute("href").ShouldBe($"{locale}/");
            var indexCall = fixture.Renderer.Calls.Single(call => call.View == typeof(IndexView)
                && ((LocaleContext)call.Parameters["LocaleContext"]!).Locale == locale);
            var context = (LocaleContext)indexCall.Parameters["LocaleContext"]!;
            context.Route.ShouldBe(locale);
            context.HomeUrl.ShouldBe($"{locale}/");
            var synthetic = (ContentDocument)indexCall.Parameters["Document"]!;
            synthetic.Metadata.Title.ShouldBeEmpty();
            synthetic.Metadata.Locale.ShouldBe(locale);
            fixture.HookDocuments[locale].Metadata.Slug.ShouldBe(synthetic.Metadata.Slug);
            fixture.HookDocuments[locale].Metadata.Locale.ShouldBe(locale);
        }

        using var koreanPage = fixture.Html("ko-kr/about/index.html");
        koreanPage.QuerySelector(".page-navigation-previous").ShouldBeNull();
        koreanPage.QuerySelector(".page-navigation-next")!.GetAttribute("href").ShouldBe("ko-kr/next");
        using var koreanLast = fixture.Html("ko-kr/next/index.html");
        koreanLast.QuerySelector(".page-navigation-next").ShouldBeNull();
        using var englishPage = fixture.Html("en-us/about/index.html");
        englishPage.QuerySelector(".page-navigation").ShouldBeNull();
        using var japanese = fixture.Html("ja-jp/index.html");
        japanese.QuerySelectorAll(".post-link").ShouldBeEmpty();
        japanese.QuerySelectorAll(".site-header nav a").Select(link => link.TextContent).ShouldBe(["Home"]);
        fixture.Exists("ja-jp/tags/index.html").ShouldBeFalse();
        fixture.Exists("fr-fr/index.html").ShouldBeFalse();
        using var legacy = fixture.Html("tags/shared/index.html");
        legacy.QuerySelector("a")!.GetAttribute("href").ShouldBe($"{effectiveBaseUrl}en-us/tags/shared/");
        fixture.Exists("tags/korean-only/index.html").ShouldBeFalse();
        using var missing = fixture.Html("404.html");
        missing.DocumentElement.GetAttribute("lang").ShouldBe("en-us");
        missing.QuerySelector(".back-link")!.GetAttribute("href").ShouldBe("en-us/");
        missing.QuerySelectorAll(".site-header nav a").Select(link => link.TextContent).ShouldBe(["Home", "English", "Tags"]);
        await fixture.Plugins.Received(6).RunPreMarkdownAsync(Arg.Any<ContentDocument>(), Arg.Any<CancellationToken>());
        await fixture.Plugins.Received(6).RunPostMarkdownAsync(Arg.Any<ContentDocument>(), Arg.Any<CancellationToken>());
        await fixture.Plugins.Received(fixture.HookDocuments.Count)
            .RunPostHtmlAsync(Arg.Any<string>(), Arg.Any<ContentDocument>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Given_EmptyDefaultLocale_When_Generated_Then_It_Should_ProvideAHomepageButNoDanglingTagRoutes(bool emptySite)
    {
        using var fixture = new Fixture(emptySite ? [] : [Post("Korean", "ko-kr/post", "ko-KR", ["korean"])]);

        await fixture.BuildAsync();

        using var home = fixture.Html("en-us/index.html");
        home.QuerySelectorAll(".post-link").ShouldBeEmpty();
        home.QuerySelectorAll(".site-header nav a").Select(link => link.TextContent).ShouldBe(["Home"]);
        fixture.Exists("en-us/tags/index.html").ShouldBeFalse();
        fixture.Exists("tags/index.html").ShouldBeFalse();
        fixture.Exists("tags/korean/index.html").ShouldBeFalse();
        fixture.Exists("ko-kr/index.html").ShouldBe(!emptySite);
        fixture.Exists("404.html").ShouldBeTrue();
    }

    [Theory]
    [InlineData(null, true)]
    [InlineData("en_US", true)]
    [InlineData("ko-KR", false)]
    public async Task Given_Custom404Locale_When_Generated_Then_It_Should_RequireTheDefaultLocale(string? locale, bool valid)
    {
        var document = new ContentDocument
        {
            SourcePath = "not-found.md",
            Kind = ContentKind.Page,
            Metadata = new() { Slug = "404.html", Locale = locale, Tags = ["not-found-only"] },
        };
        using var fixture = new Fixture([document]);
        if (!valid)
        {
            var error = await Should.ThrowAsync<InvalidDataException>(() => fixture.BuildAsync());
            error.Message.ShouldContain("not-found.md");
            error.Message.ShouldContain("locale");
            fixture.Renderer.Calls.ShouldBeEmpty();
            return;
        }

        await fixture.BuildAsync();

        fixture.Exists("tags/index.html").ShouldBeFalse();
        fixture.Exists("en-us/tags/index.html").ShouldBeFalse();
        using var html = fixture.Html("404.html");
        html.DocumentElement.GetAttribute("lang").ShouldBe("en-us");
    }

    [Fact]
    public async Task Given_DisabledLocaleRouting_When_Generated_Then_It_Should_RetainSharedCollectionsAnd404Metadata()
    {
        using var fixture = new Fixture([
            Post("English", "one", "en-US", ["shared"]),
            Post("Korean", "two", "ko-KR", ["shared"]),
            new() { Kind = ContentKind.Page, Metadata = new() { Slug = "404.html", Locale = "ko-KR" } },
        ], enabled: false);

        await fixture.BuildAsync();

        using var index = fixture.Html("index.html");
        index.QuerySelectorAll(".post-link").Select(link => link.TextContent).ShouldBe(["English", "Korean"]);
        using var tag = fixture.Html("tags/shared/index.html");
        tag.QuerySelectorAll(".post-link").Select(link => link.TextContent.Trim()).ShouldBe(["English", "Korean"]);
        tag.QuerySelector(".back-link")!.GetAttribute("href").ShouldBe("tags");
        using var missing = fixture.Html("404.html");
        missing.DocumentElement.GetAttribute("lang").ShouldBe("ko-kr");
        fixture.Renderer.Calls.ShouldAllBe(call => !call.Parameters.ContainsKey("LocaleContext"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("..")]
    [InlineData(".")]
    [InlineData(@"..\escape")]
    [InlineData("en:us")]
    [InlineData("en?us")]
    public async Task Given_InvalidDefaultLocale_When_Generated_Then_It_Should_FailBeforeRendering(string locale)
    {
        using var fixture = new Fixture([], defaultLocale: locale);

        var error = await Should.ThrowAsync<InvalidDataException>(() => fixture.BuildAsync());

        error.Message.ShouldContain("Site.Locale");
        fixture.Renderer.Calls.ShouldBeEmpty();
    }

    [Theory]
    [InlineData("https://example.com/")]
    [InlineData("//example.com/")]
    [InlineData("blog/")]
    [InlineData("/../")]
    [InlineData("/%2e%2e/")]
    [InlineData("/%2fhost/")]
    [InlineData("/blog\\other/")]
    [InlineData("/%zz/")]
    [InlineData("/%2/")]
    public async Task Given_UnsafeRedirectBase_When_Generated_Then_It_Should_FailBeforeRendering(string baseUrl)
    {
        using var fixture = new Fixture([], baseUrl: baseUrl);

        var error = await Should.ThrowAsync<InvalidDataException>(() => fixture.BuildAsync());

        error.Message.ShouldContain("Site.BaseUrl");
        fixture.Renderer.Calls.ShouldBeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("en-us")]
    [InlineData("en-us/tags")]
    [InlineData("tags")]
    [InlineData("tags/shared")]
    public async Task Given_SourceClaimsGeneratedRoute_When_Generated_Then_It_Should_RejectTheCollision(string route)
    {
        using var fixture = new Fixture([Page("Conflict", route, "en-US"), Post("Post", "en-us/post", null, ["shared"])]);

        var error = await Should.ThrowAsync<InvalidDataException>(() => fixture.BuildAsync());

        error.Message.ShouldContain("Output collision");
        fixture.Renderer.Calls.ShouldBeEmpty();
    }

    [Fact]
    public async Task Given_PluginChangesRouteToLocaleHome_When_Generated_Then_It_Should_NotOverwriteTheHomepage()
    {
        var source = Page("Source", "en-us/source", "en-US");
        using var fixture = new Fixture([source]);
        fixture.Plugins.RunPostMarkdownAsync(source, Arg.Any<CancellationToken>())
            .Returns(new ContentDocument { Kind = ContentKind.Page, Metadata = source.Metadata with { Slug = "en-us" } });

        var error = await Should.ThrowAsync<InvalidDataException>(() => fixture.BuildAsync());

        error.Message.ShouldContain("Output collision");
        using var home = fixture.Html("en-us/index.html");
        home.Title.ShouldBe("Site");
    }

    [Fact]
    public async Task Given_PluginReplacesLocaleMetadata_When_Generated_Then_It_Should_PreserveTheLoadedScopeSnapshot()
    {
        var source = Page("First", "en-us/first", "en-US", "01-first.md");
        var next = Page("Next", "en-us/next", "en-US", "02-next.md");
        var replacement = new ContentDocument
        {
            Kind = ContentKind.Page,
            Metadata = source.Metadata with { Title = "Changed", Slug = "en-us/changed", Locale = "ko-KR" },
        };
        using var fixture = new Fixture([source, next]);
        fixture.Plugins.RunPreMarkdownAsync(source, Arg.Any<CancellationToken>()).Returns(replacement);

        await fixture.BuildAsync();

        var call = fixture.Renderer.Calls.Single(call => call.Parameters.TryGetValue("Document", out var document)
            && ReferenceEquals(document, replacement));
        var context = (LocaleContext)call.Parameters["LocaleContext"]!;
        context.Locale.ShouldBe("en-us");
        context.Route.ShouldBe("en-us/first");
        context.HomeUrl.ShouldBe("en-us/");
        using var html = fixture.Html("en-us/changed/index.html");
        html.DocumentElement.GetAttribute("lang").ShouldBe("en-us");
        html.QuerySelector(".page-navigation-next")!.GetAttribute("href").ShouldBe("en-us/next");
        fixture.Exists("ko-kr/index.html").ShouldBeFalse();
        fixture.HookDocuments["en-us/changed"].ShouldBeSameAs(replacement);
        await fixture.Plugins.Received(1).RunPreMarkdownAsync(source, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Given_ContentAssetClaimsLocaleHomepage_When_Generated_Then_It_Should_NotOverwriteThePage()
    {
        using var fixture = new Fixture([], defaultLocale: "images");
        var image = Path.Combine(Path.GetDirectoryName(fixture.Destination)!, "contents", "images", "index.html");
        fixture.FileSystem.AddFile(image, new MockFileData("asset data"));

        var error = await Should.ThrowAsync<InvalidDataException>(() => fixture.BuildAsync());

        error.Message.ShouldContain("Output collision");
        using var home = fixture.Html("images/index.html");
        home.Title.ShouldBe("Site");
    }

    [Fact]
    public async Task Given_LinkedLocaleOutput_When_Generated_Then_It_Should_RejectTheLinkBeforeRendering()
    {
        using var fixture = new Fixture([]);
        var localeDirectory = Path.Combine(fixture.Destination, "en-us");
        fixture.FileSystem.AddDirectory(localeDirectory);
        fixture.FileSystem.File.SetAttributes(localeDirectory, FileAttributes.Directory | FileAttributes.ReparsePoint);

        var error = await Should.ThrowAsync<InvalidDataException>(() => fixture.BuildAsync());

        error.Message.ShouldContain("filesystem link");
        fixture.Renderer.Calls.ShouldBeEmpty();
    }

    private static ContentDocument Page(string title, string slug, string locale, string source = "") => new()
    {
        SourcePath = source,
        Kind = ContentKind.Page,
        Markdown = title,
        Metadata = new() { Title = title, Slug = slug, Locale = locale, ShowInNavigation = true },
    };

    private static ContentDocument Post(string title, string slug, string? locale, string[] tags) => new()
    {
        Kind = ContentKind.Post,
        Markdown = title,
        Metadata = new() { Title = title, Slug = slug, Locale = locale, Tags = tags },
    };

    private sealed class Fixture : IDisposable
    {
        private readonly ServiceProvider _provider;
        private readonly StaticSiteGenerator _generator;

        public Fixture(IReadOnlyList<ContentDocument> documents, string baseUrl = "/", string defaultLocale = "en-US", bool enabled = true)
        {
            var root = Path.Combine(Path.GetPathRoot(Environment.CurrentDirectory)!, "locale-test");
            Destination = Path.Combine(root, "output");
            var paths = new TestAppPaths(root, Path.Combine(root, "contents"), Path.Combine(root, "themes"));
            Site = new SiteManifest { Title = "Site", Locale = defaultLocale, BaseUrl = baseUrl, UseLocaleInUrl = enabled };
            var loader = Substitute.For<IContentLoader>();
            loader.LoadAsync(Arg.Any<CancellationToken>()).Returns(documents);
            var markdown = Substitute.For<IMarkdownService>();
            markdown.ToHtmlAsync(Arg.Any<string>(), Arg.Any<bool?>(), Arg.Any<CancellationToken>()).Returns("<p>Body</p>");
            var theme = Substitute.For<IThemeService>();
            theme.LoadManifestAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(new ThemeManifest { Slug = "default" });
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton(theme);
            _provider = services.BuildServiceProvider();
            Renderer = new RecordingRenderer(new ComponentRenderer(
                _provider.GetRequiredService<IServiceScopeFactory>(), _provider.GetRequiredService<ILoggerFactory>()));
            Plugins.Manifests.Returns([]);
            Plugins.RunPreMarkdownAsync(Arg.Any<ContentDocument>(), Arg.Any<CancellationToken>()).Returns(call => call.ArgAt<ContentDocument>(0));
            Plugins.RunPostMarkdownAsync(Arg.Any<ContentDocument>(), Arg.Any<CancellationToken>()).Returns(call => call.ArgAt<ContentDocument>(0));
            Plugins.RunPostHtmlAsync(Arg.Any<string>(), Arg.Any<ContentDocument>(), Arg.Any<CancellationToken>())
                .Returns(call =>
                {
                    var document = call.ArgAt<ContentDocument>(1);
                    HookDocuments[document.Metadata.Slug] = document;
                    return call.ArgAt<string>(0);
                });
            _generator = new StaticSiteGenerator(loader, markdown, Plugins, theme, Renderer, paths, FileSystem, Site,
                Substitute.For<ILogger<StaticSiteGenerator>>());
        }

        public MockFileSystem FileSystem { get; } = new();
        public string Destination { get; }
        public SiteManifest Site { get; }
        public IPluginRunner Plugins { get; } = Substitute.For<IPluginRunner>();
        public RecordingRenderer Renderer { get; }
        public Dictionary<string, ContentDocument> HookDocuments { get; } = new();
        public Task BuildAsync(bool preview = false) =>
            _generator.BuildAsync<MainLayout, IndexView, PostView, PageView, NotFoundView, TagListView, TagView>(
                Destination, preview, Xunit.TestContext.Current.CancellationToken);
        public bool Exists(string path) => FileSystem.File.Exists(Path.Combine(Destination, path.Replace('/', Path.DirectorySeparatorChar)));
        public IHtmlDocument Html(string path) => new HtmlParser().ParseDocument(
            FileSystem.File.ReadAllText(Path.Combine(Destination, path.Replace('/', Path.DirectorySeparatorChar))));
        public void Dispose() => _provider.Dispose();
    }

    private sealed class RecordingRenderer(IComponentRenderer inner) : IComponentRenderer
    {
        public List<(Type View, Dictionary<string, object?> Parameters)> Calls { get; } = [];

        public Task<string> RenderAsync<TComponent>(Type layoutType, IDictionary<string, object?> parameters, CancellationToken cancellationToken = default)
            where TComponent : IComponent
        {
            Calls.Add((typeof(TComponent), new(parameters)));
            return inner.RenderAsync<TComponent>(layoutType, parameters, cancellationToken);
        }
    }
}
