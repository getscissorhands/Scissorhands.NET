using System.IO.Abstractions.TestingHelpers;

using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;
using ScissorHands.Core.Services;
using ScissorHands.Theme;
using ScissorHands.Theme.Components;
using ScissorHands.Web.Generators;
using ScissorHands.Web.Loaders;
using ScissorHands.Web.Renderers;
using ScissorHands.Web.Runners;
using ScissorHands.Web.Services;
using ScissorHands.Web.Tests.TestDoubles;

namespace ScissorHands.Web.Tests.Generators;

public class StaticSiteGeneratorLocaleTests
{
    [Theory]
    [InlineData(false, "/")]
    [InlineData(true, "/")]
    [InlineData(false, "/docs")]
    [InlineData(true, "/docs/")]
    [InlineData(false, "/manual/docs/")]
    public async Task Given_TranslationsAndFallbacks_When_Generated_Then_It_Should_PreservePrimaryRoutesAndScopeTheSite(bool preview, string baseUrl)
    {
        using var fixture = new Fixture(baseUrl);
        fixture.Add("pages/01-about.md", "title: English about\nslug: about\nshow_in_navigation: true\ntags: [primary]");
        fixture.Add("pages/02-next.md", "title: English next\nslug: next\nshow_in_navigation: true");
        fixture.Add("pages/ko-kr/01-about.md", "title: Korean about\nslug: about\nshow_in_navigation: true\ntags: [translated]");
        fixture.Add("posts/post.md", "title: English post\npublished: 2026-09-01\ntags: [shared]");
        fixture.Add("posts/ko-kr/post.md", "title: Korean post\npublished: 2026-09-01\ntags: [shared]");
        fixture.Add("pages/not-found.md", "title: Missing\nslug: 404.html");

        await fixture.Build(preview);

        using var primary = fixture.Html("index.html");
        primary.QuerySelector("meta[http-equiv='refresh']").ShouldBeNull();
        primary.QuerySelector(".site-title")!.GetAttribute("href").ShouldBe(".");
        primary.QuerySelectorAll(".post-link").Select(link => link.TextContent).ShouldBe(["English post"]);
        fixture.Exists("en-us/index.html").ShouldBeFalse();
        using var korean = fixture.Html("ko-kr/index.html");
        korean.QuerySelectorAll(".post-link").Select(link => link.TextContent).ShouldBe(["Korean post"]);
        korean.QuerySelector(".site-title")!.GetAttribute("href").ShouldBe("ko-kr/");
        korean.QuerySelector("[data-localization-fallback]").ShouldBeNull();
        korean.QuerySelector("link[rel='canonical']").ShouldBeNull();
        using var fallback = fixture.Html("ko-kr/next/index.html");
        fallback.QuerySelector("[data-localization-fallback]")!.TextContent.ShouldBe(Fixture.Notice);
        fallback.QuerySelector("[data-localization-fallback]")!.GetAttribute("lang").ShouldBe("ko-kr");
        fallback.QuerySelector("article")!.GetAttribute("lang").ShouldBe("en-us");
        fallback.QuerySelector(".page-navigation-previous")!.GetAttribute("href").ShouldBe("ko-kr/about");
        fallback.QuerySelector("article a")!.GetAttribute("href").ShouldBe(baseUrl == "/" ? "/ko-kr/about/" : "/about/");
        fallback.QuerySelector("link[rel='canonical']")!.GetAttribute("href")
            .ShouldBe("https://example.test" + fixture.Site.BaseUrl + "next/");
        fallback.QuerySelectorAll("link[rel='alternate']").Select(link => link.GetAttribute("hreflang")).ShouldBe(["en-us"]);
        using var translated = fixture.Html("ko-kr/about/index.html");
        translated.QuerySelector("[data-localization-fallback]").ShouldBeNull();
        translated.QuerySelector("article")!.GetAttribute("lang").ShouldBe("ko-kr");
        translated.QuerySelector("link[rel='canonical']")!.GetAttribute("href")
            .ShouldBe("https://example.test" + fixture.Site.BaseUrl + "ko-kr/about/");
        translated.QuerySelectorAll("link[rel='alternate']").Select(link => link.GetAttribute("hreflang"))
            .ShouldBe(["en-us", "ko-kr"]);
        using var original = fixture.Html("about/index.html");
        original.QuerySelectorAll("link[rel='alternate']").Select(link => link.OuterHtml)
            .ShouldBe(translated.QuerySelectorAll("link[rel='alternate']").Select(link => link.OuterHtml));
        fixture.Exists("tags/translated/index.html").ShouldBeFalse();
        fixture.Exists("ko-kr/tags/translated/index.html").ShouldBeTrue();
        fixture.Exists("ko-kr/tags/primary/index.html").ShouldBeFalse();
        using var missing = fixture.Html("404.html");
        missing.DocumentElement.GetAttribute("lang").ShouldBe("en-us");
        missing.QuerySelector("[data-localization-fallback]").ShouldBeNull();
        missing.QuerySelector("link[rel='canonical']").ShouldBeNull();
        foreach (var (path, targets) in new (string, string[])[]
        {
            ("about/index.html", ["about/", "ko-kr/about/"]),
            ("ko-kr/about/index.html", ["about/", "ko-kr/about/"]),
            ("ko-kr/next/index.html", ["next/", "ko-kr/next/"]),
            ("index.html", [".", "ko-kr/"]),
            ("ko-kr/index.html", [".", "ko-kr/"]),
            ("tags/index.html", ["tags/", "ko-kr/tags/"]),
            ("ko-kr/tags/index.html", ["tags/", "ko-kr/tags/"]),
            ("tags/primary/index.html", ["tags/primary/", "ko-kr/"]),
            ("ko-kr/tags/translated/index.html", [".", "ko-kr/tags/translated/"]),
            ("tags/shared/index.html", ["tags/shared/", "ko-kr/tags/shared/"]),
            ("404.html", [".", "ko-kr/"]),
        })
        {
            using var html = fixture.Html(path);
            html.QuerySelectorAll(".language-switcher a").Select(a => a.GetAttribute("href")).ShouldBe(targets, path);
        }
        fallback.QuerySelector(".language-switcher [aria-current='true']")!.GetAttribute("lang").ShouldBe("ko-kr");
        missing.QuerySelector("[data-localization-fallback]").ShouldBeNull();
        fixture.Exists("ko-kr/404.html/index.html").ShouldBeFalse();
        fixture.Site.IsPreview.ShouldBe(preview);
        fixture.Site.Locale.ShouldBe("en-us");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Given_PublicationChanges_When_Regenerated_Then_It_Should_ReplaceFallbackAndRemoveWithdrawnRoutes(bool preview)
    {
        using var fixture = new Fixture();
        fixture.Add("pages/about.md", "title: Primary\nshow_in_navigation: true\ntags: [topic]");
        await fixture.Build(preview);
        fixture.Html("ko-kr/about/index.html").QuerySelector("[data-localization-fallback]").ShouldNotBeNull();
        fixture.Add("pages/ko-kr/about.md", "title: Translation\nshow_in_navigation: true\ntags: [topic]");
        await fixture.Build(preview);
        using var translated = fixture.Html("ko-kr/about/index.html");
        translated.QuerySelector("[data-localization-fallback]").ShouldBeNull();
        fixture.Add("pages/ko-kr/about.md", "title: Draft\ndraft: true");
        await fixture.Build(preview);
        fixture.Html("ko-kr/about/index.html").QuerySelector("[data-localization-fallback]").ShouldNotBeNull();
        fixture.Add("pages/about.md", "title: Withdrawn\ndraft: true");
        await fixture.Build(preview);
        fixture.Exists("about/index.html").ShouldBeFalse();
        fixture.Exists("ko-kr/about/index.html").ShouldBeFalse();
        fixture.Exists("tags/topic/index.html").ShouldBeFalse();
        fixture.Exists("ko-kr/tags/topic/index.html").ShouldBeFalse();
        fixture.Add("pages/ko-kr/about.md", "title: Ready translation");
        fixture.Remove("pages/about.md");
        await fixture.Build(preview);
        fixture.Exists("ko-kr/about/index.html").ShouldBeFalse();
        using var home = fixture.Html("ko-kr/index.html");
        home.QuerySelectorAll(".site-header nav a").Select(link => link.TextContent).ShouldBe(["Home"]);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Given_MissingPrimary_When_Generated_Then_It_Should_NotPublishAdditionalLanguageOnlyContent(bool primaryDraft)
    {
        using var fixture = new Fixture();
        if (primaryDraft)
        {
            fixture.Add("pages/about.md", "draft: true");
        }
        fixture.Add("pages/ko-kr/about.md", "title: Korean only\ntags: [korean]");
        await fixture.Build();
        fixture.Exists("about/index.html").ShouldBeFalse();
        fixture.Exists("ko-kr/about/index.html").ShouldBeFalse();
        fixture.Exists("ko-kr/tags/index.html").ShouldBeFalse();
    }

    [Fact]
    public async Task Given_NoTranslationDirectory_When_Generated_Then_It_Should_EncodeTheConfiguredBanner()
    {
        using var fixture = new Fixture(message: "<script>alert('notice')</script> & translation");
        fixture.Add("pages/about.md", "title: About");
        await fixture.Build();
        using var html = fixture.Html("ko-kr/about/index.html");
        var banner = html.QuerySelector("[data-localization-fallback]")!;
        banner.TextContent.ShouldBe("<script>alert('notice')</script> & translation");
        banner.Children.ShouldBeEmpty();
        html.QuerySelector("main > :first-child").ShouldBeSameAs(banner);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public async Task Given_BlankMessage_When_FallbackNeeded_Then_It_Should_FailButAllowCompleteTranslations(string? message)
    {
        using var fixture = new Fixture(message: message);
        fixture.Add("pages/about.md");
        var error = await Should.ThrowAsync<InvalidDataException>(() => fixture.Build());
        error.Message.ShouldContain("Site:LocalizationFallbackMessages:ko-kr");
        fixture.Add("pages/ko-kr/about.md");
        await fixture.Build();
    }

    [Fact]
    public async Task Given_CustomThemeWithoutSharedBanner_When_Generated_Then_It_Should_Fail()
    {
        using var fixture = new Fixture();
        fixture.Add("pages/about.md");
        var error = await Should.ThrowAsync<InvalidDataException>(() => fixture.BuildWithMissingBanner());
        error.Message.ShouldContain("LocalizationFallbackBanner");
        fixture.Exists("ko-kr/about/index.html").ShouldBeFalse();
    }

    [Fact]
    public async Task Given_ThemeOwnedLocalizationMarkup_When_Generated_Then_It_Should_PreserveDataAndValidateItsCustomBanner()
    {
        using var fixture = new Fixture(message: "<b>Not translated</b>");
        fixture.Add("pages/about.md");

        await fixture.BuildWithCustomTheme<CustomBanner>();

        using var fallback = fixture.Html("ko-kr/about/index.html");
        var banner = fallback.QuerySelector("section[data-localization-fallback]")!;
        banner.TextContent.Trim().ShouldBe("<b>Not translated</b>");
        banner.GetAttribute("lang").ShouldBe("ko-kr");
        banner.QuerySelector("strong").ShouldNotBeNull();
        banner.QuerySelector("b").ShouldBeNull();
        fallback.QuerySelector(".localization-fallback").ShouldBeNull();
        fallback.QuerySelectorAll(".custom-switcher a").Select(a => a.GetAttribute("href")).ShouldBe(["about/", "ko-kr/about/"]);
        fallback.QuerySelector("head link[data-custom-metadata]")!.GetAttribute("href").ShouldBe("https://example.test/about/");
        using var primary = fixture.Html("about/index.html");
        primary.QuerySelector("[data-localization-fallback]").ShouldBeNull();
    }

    [Fact]
    public async Task Given_DerivedBannerWithoutMessageFragment_When_Generated_Then_It_Should_NotSatisfyTheReceipt()
    {
        using var fixture = new Fixture();
        fixture.Add("pages/about.md");

        var error = await Should.ThrowAsync<InvalidDataException>(() => fixture.BuildWithCustomTheme<LookalikeBanner>());

        error.Message.ShouldContain("FallbackMessageContent");
        fixture.Exists("ko-kr/about/index.html").ShouldBeFalse();
    }

    [Fact]
    public async Task Given_PostHtmlPluginRemovesBanner_When_Generated_Then_It_Should_Fail()
    {
        using var fixture = new Fixture();
        fixture.Add("pages/about.md");
        fixture.Plugins.RunPostHtmlAsync(Arg.Any<string>(), Arg.Any<ContentDocument>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                using var html = new HtmlParser().ParseDocument(call.ArgAt<string>(0));
                html.QuerySelector("[data-localization-fallback]")?.Remove();
                return html.DocumentElement.OuterHtml;
            });
        var error = await Should.ThrowAsync<InvalidDataException>(() => fixture.Build());
        error.Message.ShouldContain("LocalizationFallbackBanner");
    }

    [Theory]
    [InlineData("hidden")]
    [InlineData("aria-hidden")]
    [InlineData("script")]
    [InlineData("style")]
    [InlineData("template")]
    public async Task Given_NonVisibleBannerMarkup_When_Generated_Then_It_Should_RejectTheNotice(string mode)
    {
        using var fixture = new Fixture();
        fixture.Add("pages/about.md");
        fixture.Plugins.RunPostHtmlAsync(Arg.Any<string>(), Arg.Any<ContentDocument>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                using var html = new HtmlParser().ParseDocument(call.ArgAt<string>(0));
                var banner = html.QuerySelector("[data-localization-fallback]");
                if (banner is not null)
                {
                    if (mode is "hidden" or "aria-hidden")
                    {
                        banner.SetAttribute(mode, "true");
                    }
                    else
                    {
                        banner.InnerHtml = $"<{mode}>{Fixture.Notice}</{mode}>";
                    }
                }
                return html.DocumentElement.OuterHtml;
            });

        await Should.ThrowAsync<InvalidDataException>(() => fixture.BuildWithCustomTheme<CustomBanner>());
    }

    [Theory]
    [InlineData("")]
    [InlineData("tags")]
    [InlineData("tags/topic")]
    [InlineData("ko-kr")]
    [InlineData("ko-kr/unrelated")]
    [InlineData(".scissorhands-output.json")]
    public async Task Given_GeneratedRouteCollision_When_Generated_Then_It_Should_RejectTheConflict(string route)
    {
        using var fixture = new Fixture();
        fixture.Add("pages/conflict.md", $"slug: /{route}/\ntags: [topic]");
        var error = await Should.ThrowAsync<InvalidDataException>(() => fixture.Build());
        error.Message.ShouldContain("Output collision");
    }

    [Theory]
    [InlineData("https://example.com/")]
    [InlineData("//example.com/")]
    [InlineData("/../")]
    [InlineData("/%2e%2e/")]
    [InlineData("/%2fhost/")]
    [InlineData("/%zz/")]
    public async Task Given_UnsafeBaseUrl_When_Generated_Then_It_Should_Fail(string baseUrl)
    {
        using var fixture = new Fixture(baseUrl);
        var error = await Should.ThrowAsync<InvalidDataException>(() => fixture.Build());
        error.Message.ShouldContain("Site.BaseUrl");
    }

    [Theory]
    [InlineData("ko-kr")]
    [InlineData(".scissorhands-output.json")]
    public async Task Given_LinkedOutput_When_Generated_Then_It_Should_RejectTheLink(string path)
    {
        using var fixture = new Fixture();
        var linkedPath = Path.Combine(fixture.Destination, path);
        if (path.EndsWith(".json", StringComparison.Ordinal))
        {
            fixture.FileSystem.AddFile(linkedPath, new MockFileData("[]"));
        }
        else
        {
            fixture.FileSystem.AddDirectory(linkedPath);
        }
        fixture.FileSystem.File.SetAttributes(linkedPath, fixture.FileSystem.File.GetAttributes(linkedPath) | FileAttributes.ReparsePoint);
        var error = await Should.ThrowAsync<InvalidDataException>(() => fixture.Build());
        error.Message.ShouldContain("filesystem link");
    }

    [Fact]
    public async Task Given_UnsafeStaleOutputManifest_When_Generated_Then_It_Should_NotDeleteOutsideTheRoot()
    {
        using var fixture = new Fixture();
        fixture.FileSystem.AddFile(Path.Combine(fixture.Destination, ".scissorhands-output.json"), new MockFileData("[\"../outside.html\"]"));
        var outside = Path.Combine(fixture.Destination, "..", "outside.html");
        fixture.FileSystem.AddFile(outside, new MockFileData("Keep"));
        await Should.ThrowAsync<InvalidDataException>(() => fixture.Build());
        fixture.FileSystem.File.ReadAllText(outside).ShouldBe("Keep");
    }

    [Fact]
    public async Task Given_NewGeneratorInstance_When_Regenerated_Then_It_Should_RemoveOnlyPreviouslyOwnedPages()
    {
        using var fixture = new Fixture();
        fixture.Add("pages/about.md");
        await fixture.Build();
        fixture.Remove("pages/about.md");
        fixture.FileSystem.AddFile(Path.Combine(fixture.Destination, "unmanaged.html"), new MockFileData("Keep"));
        await fixture.BuildWithNewGenerator();
        fixture.Exists("about/index.html").ShouldBeFalse();
        fixture.Exists("ko-kr/about/index.html").ShouldBeFalse();
        fixture.Exists("unmanaged.html").ShouldBeTrue();
    }

    [Fact]
    public async Task Given_PluginChangesRouteToLocaleHome_When_Generated_Then_It_Should_NotOverwriteTheHomepage()
    {
        using var fixture = new Fixture();
        fixture.Add("pages/about.md");
        fixture.Plugins.RunPostMarkdownAsync(Arg.Any<ContentDocument>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var document = call.ArgAt<ContentDocument>(0);
                return new ContentDocument { Kind = document.Kind, Metadata = document.Metadata with { Slug = "ko-kr" } };
            });
        var error = await Should.ThrowAsync<InvalidDataException>(() => fixture.Build());
        error.Message.ShouldContain("Output collision");
        using var home = fixture.Html("ko-kr/index.html");
        home.Title.ShouldBe("Site");
    }

    [Fact]
    public async Task Given_PluginReplacesMetadata_When_Generated_Then_It_Should_PreserveRequestedAndActualLocaleSnapshots()
    {
        using var fixture = new Fixture();
        fixture.Add("pages/about.md");
        fixture.Plugins.RunPreMarkdownAsync(Arg.Any<ContentDocument>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var document = call.ArgAt<ContentDocument>(0);
                return new ContentDocument
                {
                    Kind = document.Kind,
                    Metadata = document.Metadata with { Title = "Plugin title", Locale = "ja-jp" },
                    Markdown = document.Markdown,
                };
            });
        await fixture.Build();
        using var fallback = fixture.Html("ko-kr/about/index.html");
        fallback.Title.ShouldBe("Plugin title | Site");
        fallback.DocumentElement.GetAttribute("lang").ShouldBe("ko-kr");
        fallback.QuerySelector("article")!.GetAttribute("lang").ShouldBe("en-us");
        fixture.Exists("ja-jp/index.html").ShouldBeFalse();
        await fixture.Plugins.Received(2).RunPreMarkdownAsync(Arg.Any<ContentDocument>(), Arg.Any<CancellationToken>());
        await fixture.Plugins.Received(2).RunPostMarkdownAsync(Arg.Any<ContentDocument>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Given_ContentAssetClaimsDocumentOutput_When_Generated_Then_It_Should_NotOverwriteTheDocument()
    {
        using var fixture = new Fixture();
        fixture.Add("pages/about.md", "slug: images/about");
        fixture.FileSystem.AddFile(Path.Combine(Path.GetDirectoryName(fixture.Destination)!, "contents", "images", "about", "index.html"),
            new MockFileData("Asset"));
        var error = await Should.ThrowAsync<InvalidDataException>(() => fixture.Build());
        error.Message.ShouldContain("Output collision");
        fixture.FileSystem.File.ReadAllText(Path.Combine(fixture.Destination, "images", "about", "index.html")).ShouldNotBe("Asset");
    }

    [Fact]
    public async Task Given_StaleOutputReplacedWithLink_When_Regenerated_Then_It_Should_RejectCleanupThroughTheLink()
    {
        using var fixture = new Fixture();
        fixture.Add("pages/about.md");
        await fixture.Build();
        fixture.Remove("pages/about.md");
        var path = Path.Combine(fixture.Destination, "ko-kr", "about");
        fixture.FileSystem.File.SetAttributes(path, FileAttributes.Directory | FileAttributes.ReparsePoint);
        var error = await Should.ThrowAsync<InvalidDataException>(() => fixture.Build());
        error.Message.ShouldContain("filesystem link");
        fixture.Exists("ko-kr/about/index.html").ShouldBeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("en-us")]
    public async Task Given_RemovedLocaleDeclaration_When_Regenerated_Then_It_Should_ReclassifyFilesAndRemoveSyntheticFallbacks(string? primary)
    {
        using var fixture = new Fixture();
        fixture.Add("pages/about.md");
        fixture.Add("pages/ko-kr/standalone.md");
        await fixture.Build();
        fixture.Exists("ko-kr/about/index.html").ShouldBeTrue();
        fixture.Exists("ko-kr/standalone/index.html").ShouldBeFalse();

        await fixture.BuildWithSite(new SiteManifest { Locale = primary });

        fixture.Exists("about/index.html").ShouldBeTrue();
        fixture.Exists("ko-kr/about/index.html").ShouldBeFalse();
        fixture.Exists("ko-kr/standalone/index.html").ShouldBeTrue();
        fixture.Exists("ko-kr/index.html").ShouldBeFalse();
        using var primaryHtml = fixture.Html("about/index.html");
        primaryHtml.QuerySelector(".language-switcher").ShouldBeNull();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Given_MissingTagIndexInAnotherLocale_When_Generated_Then_TheSwitcherShould_LinkToItsHomepage(bool primaryTagged)
    {
        using var fixture = new Fixture();
        fixture.Add("pages/about.md", primaryTagged ? "tags: [topic]" : "");
        fixture.Add("pages/ko-kr/about.md", primaryTagged ? "" : "tags: [topic]");
        await fixture.Build();
        var prefix = primaryTagged ? "" : "ko-kr/";
        using var html = fixture.Html(prefix + "tags/index.html");
        html.QuerySelectorAll(".language-switcher a").Select(a => a.GetAttribute("href"))
            .ShouldBe(primaryTagged ? ["tags/", "ko-kr/"] : [".", "ko-kr/tags/"]);
        fixture.Exists((primaryTagged ? "ko-kr/" : "") + "tags/index.html").ShouldBeFalse();
    }

    [Theory]
    [InlineData(false, "/", false)]
    [InlineData(true, "/", true)]
    [InlineData(false, "/docs/", true)]
    [InlineData(true, "/docs/", false)]
    public async Task Given_AuthoredMarkdown_When_Generated_Then_It_Should_LocalizeOnlyAvailableContentLinks(
        bool preview, string baseUrl, bool hasTranslation)
    {
        using var fixture = new Fixture(baseUrl, realMarkdown: true);
        fixture.Add("pages/target.md");
        fixture.Add("pages/draft.md", "draft: true");
        var markdown = $$"""
            [Target](target/?q=a%2Fb&x=1#heading)
            [Root]({{baseUrl}}target/)
            [Primary](target/?q=a%2Fb&x=1#heading){data-localize="false"}
            [Korean](ko-kr/target/)
            [Missing](missing/)
            [Draft](draft/)
            [External](https://other.test/target/?q=1#heading)
            [Image](images/sample.svg)
            """;
        fixture.Add("pages/source.md", markdown: markdown);
        if (hasTranslation)
        {
            fixture.Add("pages/ko-kr/source.md", markdown: markdown);
        }

        await fixture.Build(preview);

        using var localized = fixture.Html("ko-kr/source/index.html");
        var links = localized.QuerySelectorAll("article a").ToDictionary(a => a.TextContent, a => a.GetAttribute("href"));
        links["Target"].ShouldBe("ko-kr/target/?q=a%2Fb&x=1#heading");
        links["Root"].ShouldBe(baseUrl + "ko-kr/target/");
        links["Primary"].ShouldBe("target/?q=a%2Fb&x=1#heading");
        links["Korean"].ShouldBe("ko-kr/target/");
        links["Missing"].ShouldBe("missing/");
        links["Draft"].ShouldBe("draft/");
        links["External"].ShouldBe("https://other.test/target/?q=1#heading");
        links["Image"].ShouldBe("images/sample.svg");
        (localized.QuerySelector("[data-localization-fallback]") is null).ShouldBe(hasTranslation);
        using var primary = fixture.Html("source/index.html");
        primary.QuerySelector("article a")!.GetAttribute("href").ShouldBe("target/?q=a%2Fb&x=1#heading");
    }

    private sealed class Fixture : IDisposable
    {
        public const string Notice = "Korean translation is not available.";
        private readonly ServiceProvider _provider;
        private readonly TestAppPaths _paths;
        private readonly IThemeService _theme;
        private readonly IMarkdownService _markdown;
        private readonly IComponentRenderer _renderer;
        private readonly StaticSiteGenerator _generator;

        public Fixture(string baseUrl = "/", string? message = Notice, bool realMarkdown = false)
        {
            var root = Path.Combine(Path.GetPathRoot(Environment.CurrentDirectory)!, "locale-test");
            Destination = Path.Combine(root, "output");
            _paths = new TestAppPaths(root, Path.Combine(root, "contents"), Path.Combine(root, "themes"));
            Site = new SiteManifest
            {
                Title = "Site",
                Locale = "en-us",
                BaseUrl = baseUrl,
                SiteUrl = "https://example.test",
                LocalizationFallbackMessages = new Dictionary<string, string?> { ["ko-kr"] = message },
            };
            if (realMarkdown)
            {
                _markdown = new MarkdownService();
            }
            else
            {
                _markdown = Substitute.For<IMarkdownService>();
                _markdown.ToHtmlAsync(Arg.Any<string>(), Arg.Any<bool?>(), Arg.Any<CancellationToken>())
                    .Returns("<p>Body <a href=\"/about/\">About</a></p>");
            }
            _theme = Substitute.For<IThemeService>();
            _theme.LoadManifestAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(new ThemeManifest { Slug = "default" });
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton(_theme);
            _provider = services.BuildServiceProvider();
            _renderer = new ComponentRenderer(_provider.GetRequiredService<IServiceScopeFactory>(), _provider.GetRequiredService<ILoggerFactory>());
            Plugins.Manifests.Returns([]);
            Plugins.RunPreMarkdownAsync(Arg.Any<ContentDocument>(), Arg.Any<CancellationToken>()).Returns(call => call.ArgAt<ContentDocument>(0));
            Plugins.RunPostMarkdownAsync(Arg.Any<ContentDocument>(), Arg.Any<CancellationToken>()).Returns(call => call.ArgAt<ContentDocument>(0));
            Plugins.RunPostHtmlAsync(Arg.Any<string>(), Arg.Any<ContentDocument>(), Arg.Any<CancellationToken>()).Returns(call => call.ArgAt<string>(0));
            _generator = CreateGenerator();
        }

        private StaticSiteGenerator CreateGenerator(SiteManifest? site = null) => new(
            new ContentLoader(_paths, FileSystem, site ?? Site, Substitute.For<ILogger<ContentLoader>>()),
            _markdown, Plugins, _theme, _renderer, _paths, FileSystem, site ?? Site, Substitute.For<ILogger<StaticSiteGenerator>>());
        public MockFileSystem FileSystem { get; } = new();
        public SiteManifest Site { get; }
        public string Destination { get; }
        public IPluginRunner Plugins { get; } = Substitute.For<IPluginRunner>();
        public void Add(string path, string metadata = "", string markdown = "Body") =>
            FileSystem.AddFile(Path.Combine(_paths.GetContentsRoot(), path), new MockFileData($"---\n{metadata}\n---\n{markdown}"));
        public void Remove(string path) => FileSystem.File.Delete(Path.Combine(_paths.GetContentsRoot(), path));
        public Task Build(bool preview = false) =>
            _generator.BuildAsync<MainLayout, IndexView, PostView, PageView, NotFoundView, TagListView, TagView>(
                Destination, preview, Xunit.TestContext.Current.CancellationToken);
        public Task BuildWithNewGenerator() =>
            CreateGenerator().BuildAsync<MainLayout, IndexView, PostView, PageView, NotFoundView, TagListView, TagView>(
                Destination, true, Xunit.TestContext.Current.CancellationToken);
        public Task BuildWithSite(SiteManifest site) =>
            CreateGenerator(site).BuildAsync<MainLayout, IndexView, PostView, PageView, NotFoundView, TagListView, TagView>(
                Destination, true, Xunit.TestContext.Current.CancellationToken);
        public Task BuildWithMissingBanner() =>
            _generator.BuildAsync<MissingBannerLayout, IndexView, PostView, PageView, NotFoundView, TagListView, TagView>(
                Destination, false, Xunit.TestContext.Current.CancellationToken);
        public Task BuildWithCustomTheme<TBanner>() where TBanner : ComponentBase =>
            _generator.BuildAsync<CustomLayout<TBanner>, IndexView, PostView, PageView, NotFoundView, TagListView, TagView>(
                Destination, false, Xunit.TestContext.Current.CancellationToken);
        public bool Exists(string path) => FileSystem.File.Exists(Path.Combine(Destination, path));
        public IHtmlDocument Html(string path) => new HtmlParser().ParseDocument(FileSystem.File.ReadAllText(Path.Combine(Destination, path)));
        public void Dispose() => _provider.Dispose();
    }

    public sealed class MissingBannerLayout : MainLayoutBase
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "aside");
            builder.AddAttribute(1, "data-localization-fallback", LocaleContext?.Locale);
            builder.AddAttribute(2, "lang", LocaleContext?.Locale);
            builder.AddContent(3, LocaleContext?.FallbackMessage);
            builder.CloseElement();
        }
    }

    public sealed class CustomLayout<TBanner> : MainLayoutBase where TBanner : ComponentBase
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenComponent<CascadingMainLayoutBase>(0);
            builder.AddAttribute(1, nameof(CascadingMainLayoutBase.LocaleContext), LocaleContext);
            builder.AddAttribute(2, nameof(CascadingMainLayoutBase.Document), Document);
            builder.AddAttribute(3, nameof(CascadingMainLayoutBase.Site), Site);
            builder.AddAttribute(4, nameof(CascadingMainLayoutBase.Theme), Theme);
            builder.AddAttribute(5, nameof(CascadingMainLayoutBase.ChildContent), (RenderFragment)(content =>
            {
                content.OpenElement(0, "html");
                content.OpenElement(1, "head");
                content.OpenComponent<CustomMetadata>(2);
                content.CloseComponent();
                content.CloseElement();
                content.OpenElement(3, "body");
                content.OpenComponent<CustomSwitcher>(4);
                content.CloseComponent();
                content.OpenComponent<TBanner>(5);
                content.CloseComponent();
                content.AddContent(6, Body);
                content.CloseElement();
                content.CloseElement();
            }));
            builder.CloseComponent();
        }
    }

    public sealed class CustomBanner : LocalizationFallbackBannerBase
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            if (!IsFallback)
            {
                return;
            }
            builder.OpenElement(0, "section");
            builder.AddMultipleAttributes(1, BannerAttributes);
            builder.AddAttribute(2, "role", "note");
            builder.AddContent(3, "\n  ");
            builder.OpenElement(4, "strong");
            builder.AddContent(5, FallbackMessageContent);
            builder.CloseElement();
            builder.AddContent(6, "\n");
            builder.CloseElement();
        }
    }

    public sealed class LookalikeBanner : LocalizationFallbackBannerBase
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            if (!IsFallback)
            {
                return;
            }
            builder.OpenElement(0, "section");
            builder.AddMultipleAttributes(1, BannerAttributes);
            builder.AddContent(2, FallbackMessage);
            builder.CloseElement();
        }
    }

    public sealed class CustomMetadata : LocalizationMetadataBase
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            if (CanonicalUrl is null)
            {
                return;
            }
            builder.OpenElement(0, "link");
            builder.AddAttribute(1, "rel", "canonical");
            builder.AddAttribute(2, "href", CanonicalUrl);
            builder.AddAttribute(3, "data-custom-metadata", true);
            builder.CloseElement();
            foreach (var (locale, url) in AlternateLanguageUrls)
            {
                builder.OpenElement(4, "link");
                builder.AddAttribute(5, "rel", "alternate");
                builder.AddAttribute(6, "hreflang", locale);
                builder.AddAttribute(7, "href", url);
                builder.CloseElement();
            }
        }
    }

    public sealed class CustomSwitcher : LanguageSwitcherBase
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "class", "custom-switcher");
            foreach (var link in Links)
            {
                builder.OpenElement(2, "a");
                builder.AddAttribute(3, "href", link.Url);
                builder.AddContent(4, link.Label);
                builder.CloseElement();
            }
            builder.CloseElement();
        }
    }
}
