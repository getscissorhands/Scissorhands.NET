using System.IO.Abstractions.TestingHelpers;

using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;
using ScissorHands.Core.Services;
using ScissorHands.Web.Generators;
using ScissorHands.Web.Loaders;
using ScissorHands.Web.Renderers;
using ScissorHands.Web.Runners;
using ScissorHands.Web.Services;
using ScissorHands.Web.Tests.TestDoubles;

namespace ScissorHands.Web.Tests.Generators;

public class LocaleCatalogGenerationTests
{
    [Theory]
    [InlineData("en-US", "ko-KR", true)]
    [InlineData("ko-KR", "en-US", true)]
    [InlineData("en-US", "ko-KR", false)]
    [InlineData("ko-KR", "en-US", false)]
    public async Task Given_OrderedLocales_When_Generated_Then_It_Should_KeepTheFirstPrimaryAndUseRequestedLanguageMessages(
        string primary, string additional, bool preview)
    {
        using var fixture = new Fixture([primary, additional]);
        fixture.Add("pages/about.md", "title: About\nshow_in_navigation: true", "# Primary body");
        fixture.Add("posts/post.md", "published: 2099-01-01\ndraft: true\ntags: [topic]", "# Draft body");

        await fixture.Build(preview);

        var primaryRoute = primary.ToLowerInvariant();
        var secondaryRoute = additional.ToLowerInvariant();
        using var home = fixture.Html("index.html");
        home.DocumentElement.GetAttribute("lang").ShouldBe(primaryRoute);
        fixture.Exists(primaryRoute + "/index.html").ShouldBeFalse();
        using var fallback = fixture.Html(secondaryRoute + "/about/index.html");
        fallback.QuerySelector("article")!.GetAttribute("lang").ShouldBe(primaryRoute);
        fallback.QuerySelector("[data-localization-fallback]")!.TextContent
            .ShouldBe(fixture.ApplicationTheme.Localization[secondaryRoute]!.TranslationUnavailable);
        fixture.Exists("post/index.html").ShouldBe(preview);
        fixture.Exists(secondaryRoute + "/post/index.html").ShouldBe(preview);
        if (preview)
        {
            foreach (var (route, locale) in new[] { ("post/index.html", primaryRoute), (secondaryRoute + "/post/index.html", secondaryRoute) })
            {
                using var post = fixture.Html(route);
                var labels = fixture.ApplicationTheme.Localization[locale]!;
                post.QuerySelector("[data-publication-badge='draft']")!.TextContent.ShouldBe(labels.Draft);
                post.QuerySelector("[data-publication-badge='scheduled']")!.TextContent
                    .ShouldBe(string.Format(System.Globalization.CultureInfo.InvariantCulture, labels.ScheduledOn!, "2099-01-01"));
                post.QuerySelector("[data-publication-badge='scheduled']")!.GetAttribute("data-publication-date").ShouldBe("2099-01-01");
            }
        }
    }

    [Fact]
    public async Task Given_ThreeLocalesAndAnAuthoredTranslation_When_Previewed_Then_It_Should_LocalizeBadgesWithoutChangingContentLanguage()
    {
        using var fixture = new Fixture(["en-us", "ja-jp", "ko-kr"]);
        fixture.Add("posts/post.md", "published: 2099-01-01\ndraft: true\ntags: [topic]", "# Primary body");
        fixture.Add("posts/ko-kr/post.md", "published: 2099-01-01\ntags: [topic]", "# Korean body");

        await fixture.Build(true);

        using var japanese = fixture.Html("ja-jp/post/index.html");
        japanese.QuerySelector("article")!.GetAttribute("lang").ShouldBe("en-us");
        japanese.QuerySelector("[data-localization-fallback]")!.TextContent.ShouldBe("日本語訳は現在利用できません。");
        japanese.QuerySelector("[data-publication-badge='draft']")!.TextContent.ShouldBe("下書き");
        japanese.QuerySelector("[data-publication-badge='scheduled']")!.TextContent.ShouldBe("2099-01-01に公開予定");
        using var korean = fixture.Html("ko-kr/post/index.html");
        korean.QuerySelector("[data-localization-fallback]").ShouldBeNull();
        korean.QuerySelector("article")!.TextContent.ShouldContain("Korean body");
        korean.QuerySelector("[data-publication-badge='draft']")!.TextContent.ShouldBe("초안");
        korean.QuerySelector("[data-publication-badge='scheduled']")!.TextContent.ShouldBe("2099-01-01 공개 예정");
        using var tags = fixture.Html("ja-jp/tags/topic/index.html");
        tags.QuerySelector("[data-publication-badge='draft']")!.TextContent.ShouldBe("下書き");
        using var home = fixture.Html("ko-kr/index.html");
        home.QuerySelector("[data-publication-badge='scheduled']")!.TextContent.ShouldBe("2099-01-01 공개 예정");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Given_MissingMessagesOnAnEmptySite_When_Generated_Then_It_Should_FailBeforeRenderingEvenForPrimary(bool preview)
    {
        using var fixture = new Fixture(["en-us"], new ThemeManifest());

        var error = await Should.ThrowAsync<InvalidDataException>(() => fixture.Build(preview));

        error.Message.ShouldContain("Theme:Localization:en-us");
        fixture.Exists("index.html").ShouldBeFalse();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Given_NoLocalesWithAnUnrelatedCatalog_When_Generated_Then_It_Should_UseEnglishWithoutDeclaringRoutes(bool nullLocales)
    {
        using var fixture = new Fixture(nullLocales ? null : []);
        fixture.Add("posts/post.md", "published: 2099-01-01\ndraft: true", "# Body");
        fixture.Add("pages/ko-kr/about.md", "title: Ordinary folder");

        await fixture.Build(true);

        using var home = fixture.Html("index.html");
        home.DocumentElement.GetAttribute("lang").ShouldBeNull();
        fixture.Exists("ko-kr/index.html").ShouldBeFalse();
        fixture.Exists("ko-kr/about/index.html").ShouldBeTrue();
        using var post = fixture.Html("post/index.html");
        post.QuerySelector("[data-publication-badge='draft']")!.TextContent.ShouldBe("Draft");
        post.QuerySelector("[data-publication-badge='scheduled']")!.TextContent.ShouldBe("Scheduled on 2099-01-01");
        post.QuerySelector("[data-localization-fallback]").ShouldBeNull();
    }

    [Fact]
    public async Task Given_ExtraCatalogEntries_When_Generated_Then_It_Should_NotEnableLocales()
    {
        using var fixture = new Fixture(["en-us"]);
        fixture.Add("pages/ko-kr/about.md", "title: Ordinary nested content");

        await fixture.Build(false);

        fixture.Exists("ko-kr/index.html").ShouldBeFalse();
        fixture.Exists("ja-jp/index.html").ShouldBeFalse();
        using var page = fixture.Html("ko-kr/about/index.html");
        page.DocumentElement.GetAttribute("lang").ShouldBe("en-us");
        page.QuerySelector("[data-localization-fallback]").ShouldBeNull();
    }

    private sealed class Fixture : IDisposable
    {
        private readonly ServiceProvider _provider;
        private readonly StaticSiteGenerator _generator;
        private readonly TestAppPaths _paths;
        private readonly MockFileSystem _files = new();
        private readonly string _output;

        public Fixture(string[]? locales, ThemeManifest? applicationTheme = null)
        {
            var root = Path.Combine(Path.GetPathRoot(Environment.CurrentDirectory)!, "catalog-generation");
            _paths = new TestAppPaths(root, Path.Combine(root, "contents"), Path.Combine(root, "themes"));
            _output = Path.Combine(root, "output");
            var site = new SiteManifest { Locales = locales!, Theme = "default", BaseUrl = "/docs/", SiteUrl = "https://example.test" };
            ApplicationTheme = applicationTheme ?? new ThemeManifest
            {
                Localization = new Dictionary<string, ThemeLocalization?>
                {
                    ["en-us"] = ThemeLocalization.English,
                    ["ko-kr"] = new() { TranslationUnavailable = "한국어 번역이 없습니다.", Draft = "초안", ScheduledOn = "{0} 공개 예정" },
                    ["ja-jp"] = new() { TranslationUnavailable = "日本語訳は現在利用できません。", Draft = "下書き", ScheduledOn = "{0}に公開予定" },
                },
            };
            var theme = new ThemeService(_paths, _files, site, ApplicationTheme, Substitute.For<ILogger<ThemeService>>());
            _provider = new ServiceCollection().AddLogging().AddSingleton<IThemeService>(theme).BuildServiceProvider();
            var renderer = new ComponentRenderer(_provider.GetRequiredService<IServiceScopeFactory>(), _provider.GetRequiredService<ILoggerFactory>());
            var plugins = Substitute.For<IPluginRunner>();
            plugins.Manifests.Returns([]);
            plugins.RunPreMarkdownAsync(Arg.Any<ContentDocument>(), Arg.Any<CancellationToken>()).Returns(call => call.ArgAt<ContentDocument>(0));
            plugins.RunPostMarkdownAsync(Arg.Any<ContentDocument>(), Arg.Any<CancellationToken>()).Returns(call => call.ArgAt<ContentDocument>(0));
            plugins.RunPostHtmlAsync(Arg.Any<string>(), Arg.Any<ContentDocument>(), Arg.Any<CancellationToken>()).Returns(call => call.ArgAt<string>(0));
            _generator = new StaticSiteGenerator(new ContentLoader(_paths, _files, site, Substitute.For<ILogger<ContentLoader>>()),
                new MarkdownService(), plugins, theme, renderer, _paths, _files, site, Substitute.For<ILogger<StaticSiteGenerator>>());
        }

        public ThemeManifest ApplicationTheme { get; }
        public void Add(string path, string metadata, string markdown = "Body") =>
            _files.AddFile(Path.Combine(_paths.GetContentsRoot(), path), new MockFileData($"---\n{metadata}\n---\n{markdown}"));
        public bool Exists(string path) => _files.File.Exists(Path.Combine(_output, path));
        public IHtmlDocument Html(string path) => new HtmlParser().ParseDocument(_files.File.ReadAllText(Path.Combine(_output, path)));
        public Task Build(bool preview) => _generator.BuildAsync<MainLayout, IndexView, PostView, PageView, NotFoundView, TagListView, TagView>(
            _output, preview, Xunit.TestContext.Current.CancellationToken);
        public void Dispose() => _provider.Dispose();
    }
}
