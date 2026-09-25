using System.Globalization;
using System.IO.Abstractions.TestingHelpers;

using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;
using ScissorHands.Web.Generators;
using ScissorHands.Web.Loaders;
using ScissorHands.Web.Renderers;
using ScissorHands.Web.Runners;
using ScissorHands.Web.Services;
using ScissorHands.Web.Tests.TestDoubles;

namespace ScissorHands.Web.Tests.Generators;

public class StaticSiteGeneratorPublicationTests
{
    [Theory]
    [InlineData("UTC", "2026-09-25", "2026-09-24T23:59:59.9999999Z", false)]
    [InlineData("UTC", "2026-09-25", "2026-09-25T00:00:00Z", true)]
    [InlineData("UTC", "2026-09-25", "2026-09-25T00:00:00.0000001Z", true)]
    [InlineData("Asia/Seoul", "2026-09-25", "2026-09-24T14:59:59.9999999Z", false)]
    [InlineData("Asia/Seoul", "2026-09-25", "2026-09-24T15:00:00Z", true)]
    [InlineData("Asia/Seoul", "2026-09-25T09:00:00", "2026-09-25T00:00:00Z", true)]
    [InlineData("Asia/Seoul", "2026-09-25T09:00:00.0000001", "2026-09-25T00:00:00Z", false)]
    [InlineData("Asia/Seoul", "2026-09-25T09:00:00Z", "2026-09-25T00:00:00Z", false)]
    [InlineData("UTC", "2026-09-25T09:00:00+09:00", "2026-09-25T00:00:00Z", true)]
    [InlineData("America/New_York", "2026-07-01", "2026-07-01T04:00:00Z", true)]
    [InlineData("America/New_York", "2026-01-01", "2026-01-01T04:59:59Z", false)]
    public async Task Given_PublicationBoundary_When_Built_Then_It_Should_UseTheConfiguredInstant(
        string zone, string published, string reference, bool eligible)
    {
        using var fixture = new Fixture(new SiteManifest { TimeZone = zone });
        fixture.Clock.UtcNow = DateTimeOffset.Parse(reference, CultureInfo.InvariantCulture);
        fixture.Add("posts/post.md", $"title: Post\npublished: {published}\ntags: [topic]");

        await fixture.Build(false);

        fixture.Exists("post/index.html").ShouldBe(eligible);
        fixture.Exists("tags/topic/index.html").ShouldBe(eligible);
        using var home = fixture.Html("index.html");
        home.QuerySelectorAll(".post-link").Count().ShouldBe(eligible ? 1 : 0);
        fixture.Clock.Reads.ShouldBe(1);
    }

    [Theory]
    [InlineData(false, "/")]
    [InlineData(false, "/docs/")]
    [InlineData(true, "/")]
    [InlineData(true, "/docs/")]
    public async Task Given_DraftAndScheduledContent_When_Previewed_Then_It_Should_ShowBothStatusesAndPreviewNavigation(
        bool localized, string baseUrl)
    {
        using var fixture = new Fixture(Site(localized, baseUrl));
        fixture.Add("posts/post.md", "title: Draft post\ndraft: true\npublished: 2026-09-25T14:00:00Z\ntags: [topic]");
        fixture.Add("pages/01-draft.md", "title: Draft page\nslug: draft\ndraft: true\npublished: 2099-01-01\nshow_in_navigation: true\ntags: [topic]");
        fixture.Add("pages/02-next.md", "title: Next\nslug: next\nshow_in_navigation: true");
        fixture.Add("pages/hidden.md", "title: Hidden\ndraft: true");
        fixture.Add("pages/hidden/child.md", "title: Hidden child\nshow_in_navigation: true");
        fixture.Add("pages/not-found.md", "title: Draft 404\nslug: 404.html\ndraft: true", "# Custom draft 404");

        await fixture.Build(true);

        foreach (var prefix in localized ? new[] { "", "ko-kr/" } : [""])
        {
            using var post = fixture.Html(prefix + "post/index.html");
            post.QuerySelector("article")!.TextContent.ShouldContain("Draft");
            post.QuerySelector("article")!.TextContent.ShouldContain("Scheduled on 2026-09-25");
            using var page = fixture.Html(prefix + "draft/index.html");
            page.QuerySelector("article")!.TextContent.ShouldContain("Draft");
            page.QuerySelector("article")!.TextContent.ShouldNotContain("Scheduled");
            page.QuerySelector(".page-navigation-next")!.GetAttribute("href").ShouldBe(prefix + "next");
            using var next = fixture.Html(prefix + "next/index.html");
            next.QuerySelector(".page-navigation-previous")!.GetAttribute("href").ShouldBe(prefix + "draft");
            using var home = fixture.Html(prefix + "index.html");
            home.QuerySelector(".post-list li")!.TextContent.ShouldContain("Draft");
            home.QuerySelector(".post-list li")!.TextContent.ShouldContain("Scheduled on 2026-09-25");
            home.QuerySelectorAll(".site-header nav a").Select(a => a.TextContent).ShouldBe(["Home", "Draft page", "Next", "Tags"]);
            using var tag = fixture.Html(prefix + "tags/topic/index.html");
            tag.QuerySelector(".post-list li")!.TextContent.ShouldContain("Scheduled on 2026-09-25");
            tag.QuerySelector(".page-list li")!.TextContent.ShouldContain("Draft");
            fixture.Exists(prefix + "hidden/index.html").ShouldBeTrue();
            fixture.Exists(prefix + "hidden/child/index.html").ShouldBeTrue();
        }
        using var notFound = fixture.Html("404.html");
        notFound.Body!.TextContent.ShouldNotContain("Custom draft 404");
        notFound.QuerySelector("main")!.TextContent.ShouldNotContain("Draft");

        await fixture.Build(false);

        fixture.Exists("post/index.html").ShouldBeFalse();
        fixture.Exists("draft/index.html").ShouldBeFalse();
        fixture.Exists("ko-kr/post/index.html").ShouldBeFalse();
        fixture.Exists("ko-kr/draft/index.html").ShouldBeFalse();
        fixture.Exists("tags/topic/index.html").ShouldBeFalse();
        fixture.Exists("ko-kr/tags/topic/index.html").ShouldBeFalse();
        using var production = fixture.Html("index.html");
        production.QuerySelector(".post-link").ShouldBeNull();
        production.QuerySelector("a[href='draft']").ShouldBeNull();
    }

    [Theory]
    [InlineData(false, false, false, false)]
    [InlineData(false, true, false, false)]
    [InlineData(true, false, false, false)]
    [InlineData(true, true, false, false)]
    [InlineData(false, false, false, true)]
    [InlineData(false, false, true, false)]
    [InlineData(false, false, true, true)]
    [InlineData(true, false, false, true)]
    [InlineData(false, true, true, false)]
    [InlineData(true, true, true, true)]
    public async Task Given_PairedPostStatuses_When_Generated_Then_It_Should_SelectAndLabelEachModeConsistently(
        bool primaryDraft, bool translatedDraft, bool primaryFuture, bool translatedFuture)
    {
        using var fixture = new Fixture(Site(true, "/docs/"));
        fixture.Add("posts/post.md",
            $"title: Primary\ndraft: {primaryDraft}\npublished: 2026-09-25T{(primaryFuture ? "14" : "10")}:00:00Z\ntags: [primary]",
            "# Primary body");
        fixture.Add("posts/ko-kr/post.md",
            $"title: Translation\ndraft: {translatedDraft}\npublished: 2026-09-25T{(translatedFuture ? "14" : "10")}:00:00Z\ntags: [translated]",
            "# Translation body");

        await fixture.Build(true);

        using (var translated = fixture.Html("ko-kr/post/index.html"))
        {
            translated.QuerySelector("[data-localization-fallback]").ShouldBeNull();
            translated.QuerySelector("article")!.TextContent.ShouldContain("Translation body");
            translated.QuerySelector("article")!.TextContent.Contains("Draft", StringComparison.Ordinal).ShouldBe(primaryDraft || translatedDraft);
            translated.QuerySelector("article")!.TextContent.Contains("Scheduled on 2026-09-25", StringComparison.Ordinal)
                .ShouldBe(primaryFuture || translatedFuture);
        }
        var previewDocument = fixture.Processed.Last(d => d.Metadata.Slug == "ko-kr/post");
        previewDocument.Metadata.Draft.ShouldBe(translatedDraft);
        previewDocument.PublicationStatus.IsDraft.ShouldBe(primaryDraft || translatedDraft);
        previewDocument.PublicationStatus.IsScheduled.ShouldBe(primaryFuture || translatedFuture);
        fixture.Exists("ko-kr/tags/translated/index.html").ShouldBeTrue();
        fixture.Exists("ko-kr/tags/primary/index.html").ShouldBeFalse();

        await fixture.Build(false);

        var primaryEligible = !primaryDraft && !primaryFuture;
        fixture.Exists("post/index.html").ShouldBe(primaryEligible);
        fixture.Exists("ko-kr/post/index.html").ShouldBe(primaryEligible);
        if (primaryEligible)
        {
            var fallback = translatedDraft || translatedFuture;
            using var translated = fixture.Html("ko-kr/post/index.html");
            (translated.QuerySelector("[data-localization-fallback]") is not null).ShouldBe(fallback);
            translated.QuerySelector("article")!.TextContent.ShouldContain(fallback ? "Primary body" : "Translation body");
            translated.QuerySelectorAll("link[rel='alternate']").Select(a => a.GetAttribute("hreflang"))
                .ShouldBe(fallback ? ["en-us"] : ["en-us", "ko-kr"]);
            fixture.Exists("ko-kr/tags/translated/index.html").ShouldBe(!fallback);
            fixture.Exists("ko-kr/tags/primary/index.html").ShouldBe(fallback);
        }
        else
        {
            fixture.Exists("ko-kr/tags/index.html").ShouldBeFalse();
            using var home = fixture.Html("ko-kr/index.html");
            home.QuerySelector(".post-link").ShouldBeNull();
        }
    }

    [Fact]
    public async Task Given_AuthoredDateAcrossUtcMidnight_When_Previewed_Then_It_Should_PreserveBadgeRoutesAndPairing()
    {
        using var fixture = new Fixture(new SiteManifest
        {
            TimeZone = "Asia/Seoul",
            Locale = "en-us",
            UseDateInPostUrl = true,
            LocalizationFallbackMessages = new Dictionary<string, string?> { ["ko-kr"] = "Missing translation" },
        });
        fixture.Clock.UtcNow = new DateTimeOffset(2026, 9, 24, 14, 0, 0, TimeSpan.Zero);
        fixture.Add("posts/post.md", "published: 2026-09-25");
        fixture.Add("posts/ko-kr/post.md", "published: 2026-09-25T00:30:00+09:00");

        await fixture.Build(true);

        foreach (var route in new[] { "2026/09/25/post", "ko-kr/2026/09/25/post" })
        {
            using var html = fixture.Html(route + "/index.html");
            html.QuerySelector("article")!.TextContent.ShouldContain("Scheduled on 2026-09-25");
            html.QuerySelector("article")!.TextContent.ShouldNotContain("Scheduled on 2026-09-24");
        }
    }

    [Fact]
    public async Task Given_ClockAdvancingDuringGeneration_When_Previewed_Then_It_Should_KeepOneStatusSnapshot()
    {
        using var fixture = new Fixture();
        fixture.Add("posts/first.md", "published: 2026-09-25T13:00:00Z");
        fixture.Add("posts/second.md", "published: 2026-09-25T13:00:00Z");
        fixture.Plugins.RunPreMarkdownAsync(Arg.Any<ContentDocument>(), Arg.Any<CancellationToken>()).Returns(call =>
        {
            fixture.Clock.UtcNow = fixture.Clock.UtcNow.AddDays(1);
            var source = call.ArgAt<ContentDocument>(0);
            return new ContentDocument { Kind = source.Kind, Metadata = source.Metadata, Markdown = source.Markdown };
        });

        await fixture.Build(true);

        fixture.Clock.Reads.ShouldBe(1);
        foreach (var route in new[] { "first", "second" })
        {
            using var html = fixture.Html(route + "/index.html");
            html.QuerySelector("article")!.TextContent.ShouldContain("Scheduled on 2026-09-25");
        }
        using var home = fixture.Html("index.html");
        home.QuerySelectorAll(".post-list li").ShouldAllBe(entry => entry.TextContent.Contains("Scheduled on 2026-09-25"));
    }

    [Fact]
    public async Task Given_RescheduledPublishedPost_When_Regenerated_Then_It_Should_RemoveOwnedOutputAndReferences()
    {
        using var fixture = new Fixture(Site(true, "/"));
        fixture.Add("posts/post.md", "title: Primary\npublished: 2026-09-25T10:00:00Z\ntags: [topic]");
        await fixture.Build(false);
        fixture.Exists("post/index.html").ShouldBeTrue();
        fixture.Exists("ko-kr/post/index.html").ShouldBeTrue();
        fixture.Files.AddFile(Path.Combine(fixture.Destination, "unmanaged.txt"), new MockFileData("Keep"));
        fixture.Add("posts/post.md", "title: Primary\npublished: 2026-09-25T14:00:00Z\ntags: [topic]");

        await fixture.Build(false);

        foreach (var route in new[] { "post/index.html", "ko-kr/post/index.html", "tags/index.html", "ko-kr/tags/topic/index.html" })
        {
            fixture.Exists(route).ShouldBeFalse();
        }
        fixture.Files.File.ReadAllText(Path.Combine(fixture.Destination, "unmanaged.txt")).ShouldBe("Keep");
        using var home = fixture.Html("index.html");
        home.QuerySelector(".post-link").ShouldBeNull();

        fixture.Clock.UtcNow = new DateTimeOffset(2026, 9, 25, 14, 0, 0, TimeSpan.Zero);
        await fixture.Build(false);
        fixture.Exists("post/index.html").ShouldBeTrue();
    }

    [Theory]
    [InlineData("remove")]
    [InlineData("alter")]
    [InlineData("hide")]
    [InlineData("move")]
    public async Task Given_PostHtmlPluginChangesBadge_When_Previewed_Then_It_Should_FailBeforeWritingTheDocument(string change)
    {
        using var fixture = new Fixture();
        fixture.Add("posts/post.md", "draft: true\npublished: 2099-01-01");
        fixture.Plugins.RunPostHtmlAsync(Arg.Any<string>(), Arg.Any<ContentDocument>(), Arg.Any<CancellationToken>()).Returns(call =>
        {
            var rendered = call.ArgAt<string>(0);
            if (call.ArgAt<ContentDocument>(1).Metadata.Slug != "post")
            {
                return rendered;
            }
            using var html = new HtmlParser().ParseDocument(rendered);
            var badge = html.QuerySelector("[data-publication-badge='draft']")!;
            switch (change)
            {
                case "remove":
                    badge.Remove();
                    break;
                case "alter":
                    badge.TextContent = "Published";
                    break;
                case "hide":
                    badge.SetAttribute("hidden", "");
                    break;
                case "move":
                    html.QuerySelector("article")!.AppendChild(badge);
                    break;
            }
            return html.DocumentElement.OuterHtml;
        });

        var error = await Should.ThrowAsync<InvalidDataException>(() => fixture.Build(true));

        error.Message.ShouldContain("Publication");
        error.Message.ShouldContain("post");
        fixture.Exists("post/index.html").ShouldBeFalse();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Given_OptionalDatesAndCustom404_When_Generated_Then_It_Should_PreserveTheirBehavior(bool preview)
    {
        using var fixture = new Fixture();
        fixture.Add("posts/undated.md", "title: Undated");
        fixture.Add("pages/future.md", "title: Future page\npublished: 2099-01-01");
        fixture.Add("pages/not-found.md", "slug: 404.html\npublished: 2099-01-01", "# Custom 404");
        await fixture.Build(preview);

        fixture.Exists("undated/index.html").ShouldBeTrue();
        using var page = fixture.Html("future/index.html");
        page.QuerySelector("article")!.TextContent.ShouldNotContain("Scheduled");
        using var missing = fixture.Html("404.html");
        missing.Body!.TextContent.ShouldContain("Custom 404");
        missing.Body.TextContent.ShouldNotContain("Scheduled");
    }

    private static SiteManifest Site(bool localized, string baseUrl) => new()
    {
        Locale = localized ? "en-us" : null,
        SiteUrl = "https://example.test",
        BaseUrl = baseUrl,
        LocalizationFallbackMessages = new Dictionary<string, string?> { ["ko-kr"] = "Korean translation unavailable." },
    };

    private sealed class Fixture : IDisposable
    {
        private readonly ServiceProvider _provider;
        private readonly TestAppPaths _paths;
        private readonly StaticSiteGenerator _generator;

        public Fixture(SiteManifest? site = null)
        {
            var root = Path.Combine(Path.GetPathRoot(Environment.CurrentDirectory)!, "publication-tests");
            Destination = Path.Combine(root, "output");
            _paths = new TestAppPaths(root, Path.Combine(root, "contents"), Path.Combine(root, "themes"));
            var options = site ?? new SiteManifest();
            var theme = Substitute.For<ScissorHands.Core.Services.IThemeService>();
            theme.LoadManifestAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(new ThemeManifest { Slug = "default" });
            _provider = new ServiceCollection().AddLogging().AddSingleton(theme).BuildServiceProvider();
            var renderer = new ComponentRenderer(_provider.GetRequiredService<IServiceScopeFactory>(), _provider.GetRequiredService<ILoggerFactory>());
            Plugins.Manifests.Returns([]);
            Plugins.RunPreMarkdownAsync(Arg.Any<ContentDocument>(), Arg.Any<CancellationToken>()).Returns(call => call.ArgAt<ContentDocument>(0));
            Plugins.RunPostMarkdownAsync(Arg.Any<ContentDocument>(), Arg.Any<CancellationToken>()).Returns(call => call.ArgAt<ContentDocument>(0));
            Plugins.RunPostHtmlAsync(Arg.Any<string>(), Arg.Any<ContentDocument>(), Arg.Any<CancellationToken>()).Returns(call =>
            {
                Processed.Add(call.ArgAt<ContentDocument>(1));
                return call.ArgAt<string>(0);
            });
            _generator = new StaticSiteGenerator(
                new ContentLoader(_paths, Files, options, Substitute.For<ILogger<ContentLoader>>()),
                new MarkdownService(), Plugins, theme, renderer, _paths, Files, options,
                Substitute.For<ILogger<StaticSiteGenerator>>(), Clock);
        }

        public MockFileSystem Files { get; } = new();
        public IPluginRunner Plugins { get; } = Substitute.For<IPluginRunner>();
        public List<ContentDocument> Processed { get; } = [];
        public TestClock Clock { get; } = new();
        public string Destination { get; }
        public void Add(string path, string metadata, string markdown = "# Body") =>
            Files.AddFile(Path.Combine(_paths.GetContentsRoot(), path), new MockFileData($"---\n{metadata}\n---\n{markdown}"));
        public bool Exists(string path) => Files.File.Exists(Path.Combine(Destination, path));
        public IHtmlDocument Html(string path) => new HtmlParser().ParseDocument(Files.File.ReadAllText(Path.Combine(Destination, path)));
        public Task Build(bool preview) =>
            _generator.BuildAsync<MainLayout, IndexView, PostView, PageView, NotFoundView, TagListView, TagView>(
                Destination, preview, Xunit.TestContext.Current.CancellationToken);
        public void Dispose() => _provider.Dispose();
    }

    private sealed class TestClock : TimeProvider
    {
        public DateTimeOffset UtcNow { get; set; } = new(2026, 9, 25, 12, 0, 0, TimeSpan.Zero);
        public int Reads { get; private set; }
        public override DateTimeOffset GetUtcNow()
        {
            Reads++;
            return UtcNow;
        }
    }
}
