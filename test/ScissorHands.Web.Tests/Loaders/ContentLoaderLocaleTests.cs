using System.IO.Abstractions.TestingHelpers;
using System.Text;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;
using ScissorHands.Web.Loaders;
using ScissorHands.Web.Tests.TestDoubles;

namespace ScissorHands.Web.Tests.Loaders;

public class ContentLoaderLocaleTests
{
    [Theory]
    [InlineData("pages", "about.md", "about")]
    [InlineData("pages", "guides/index.md", "guides")]
    [InlineData("pages", "guides/start.md", "guides/start")]
    [InlineData("pages", "index.md", "index")]
    [InlineData("posts", "guides/index.md", "2026/09/01/guides/index")]
    public async Task Given_PairedDirectories_When_Loaded_Then_It_Should_InferLocaleWithoutPrefixingPrimary(
        string kind, string path, string route)
    {
        var fixture = new Fixture();
        fixture.Add($"{kind}/{path}", "title: Primary\npublished: 2026-09-01");
        fixture.Add($"{kind}/KO_kr/{path}", "title: Korean\npublished: 2026-09-01");

        var documents = await fixture.Load();

        documents.Count.ShouldBe(2);
        documents.Single(d => d.Metadata.Title == "Primary").Metadata.Slug.ShouldBe(route);
        var translated = documents.Single(d => d.Metadata.Title == "Korean");
        translated.Metadata.Slug.ShouldBe("ko-kr/" + route);
        translated.Metadata.Locale.ShouldBe("ko-kr");
    }

    [Theory]
    [InlineData("en", "ko", true)]
    [InlineData("en-us", "ko", false)]
    [InlineData("en", "ko-kr", false)]
    [InlineData("en_US", "ko_KR", true)]
    [InlineData("en-us", "../ko", false)]
    [InlineData("en-us", "en_US", false)]
    [InlineData("..", "ko-kr", false)]
    public async Task Given_DeclaredLocales_When_Loaded_Then_It_Should_ValidateFormatAndSafety(
        string primary, string additional, bool valid)
    {
        var fixture = new Fixture(new SiteManifest
        {
            Locales = [primary, additional],
        });
        if (valid)
        {
            (await fixture.Load()).ShouldBeEmpty();
        }
        else
        {
            await Should.ThrowAsync<InvalidDataException>(fixture.Load);
        }
    }

    [Fact]
    public async Task Given_DuplicateNormalizedLocales_When_Loaded_Then_It_Should_RejectAmbiguousConfiguration()
    {
        var fixture = new Fixture(new SiteManifest
        {
            Locales = ["en-us", "ko-kr", "ko_KR"],
        });
        var error = await Should.ThrowAsync<InvalidDataException>(fixture.Load);
        error.Message.ShouldContain("Duplicate locale");
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("""{"Site":{"Locales":null}}""")]
    [InlineData("""{"Site":{"Locales":[]}}""")]
    public async Task Given_DisabledLocalization_When_LoadAsync_Invoked_Then_It_Should_IgnoreThemeCatalogAndTreatFoldersAsOrdinary(string siteConfiguration)
    {
        // Arrange
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(siteConfiguration));
        var configuration = new ConfigurationBuilder().AddJsonStream(stream).AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Site:Title"] = "Nonlocalized site",
            ["Theme:Localization:../invalid:TranslationUnavailable"] = null,
            ["Theme:Localization:ko-kr:TranslationUnavailable"] = "Unavailable",
            ["Theme:Localization:ko-kr:Draft"] = "Draft",
            ["Theme:Localization:ko-kr:ScheduledOn"] = "Scheduled on {0}",
        }).Build();
        var site = configuration.GetSection("Site").Get<SiteManifest>()!;
        site.IsLocalizationEnabled.ShouldBeFalse();
        var fixture = new Fixture(site);
        fixture.Add("pages/ko-kr/about.md");
        fixture.Add("pages/it/about.md");

        // Act
        var documents = await fixture.Load();

        // Assert
        documents.Select(d => d.Metadata.Slug).ShouldBe(["it/about", "ko-kr/about"], ignoreOrder: true);
        documents.ShouldAllBe(d => d.Metadata.Locale == null);
    }

    [Fact]
    public async Task Given_UnconfiguredFolder_When_Loaded_Then_It_Should_BePrimaryContent()
    {
        var fixture = new Fixture();
        fixture.Add("pages/it/about.md");
        var document = (await fixture.Load()).ShouldHaveSingleItem();
        document.Metadata.Slug.ShouldBe("it/about");
        document.Metadata.Locale.ShouldBe("en-us");
    }

    [Theory]
    [InlineData("pages/en_US/about.md", "")]
    [InlineData("posts/en-us/hello.md", "")]
    [InlineData("pages/about.md", "locale: en-us")]
    [InlineData("pages/ko-kr/about.md", "locale: ja-jp")]
    [InlineData("pages/about.md", "slug: ../escape")]
    public async Task Given_InvalidAuthoring_When_Loaded_Then_It_Should_ReportTheFile(string path, string metadata)
    {
        var fixture = new Fixture();
        fixture.Add(path, metadata);
        var error = await Should.ThrowAsync<InvalidDataException>(fixture.Load);
        error.Message.ShouldContain(Path.GetFileName(path));
    }

    [Theory]
    [InlineData(false, false, false, 2)]
    [InlineData(false, false, true, 1)]
    [InlineData(false, true, false, 0)]
    [InlineData(false, true, true, 0)]
    [InlineData(true, false, false, 2)]
    [InlineData(true, false, true, 2)]
    [InlineData(true, true, false, 2)]
    [InlineData(true, true, true, 2)]
    public async Task Given_PrimaryAndTranslationDrafts_When_Loaded_Then_It_Should_ApplyThePrimaryGate(
        bool preview, bool primaryDraft, bool translatedDraft, int count)
    {
        var fixture = new Fixture(new SiteManifest
        {
            IsPreview = preview,
            Locales = ["en-us", "ko-kr"],
        });
        fixture.Add("pages/about.md", $"draft: {primaryDraft}");
        fixture.Add("pages/ko-kr/about.md", $"draft: {translatedDraft}");
        (await fixture.Load()).Count.ShouldBe(count);
    }

    [Theory]
    [InlineData("ko-kr/about.md")]
    [InlineData("ko-kr/different-name.md")]
    [InlineData("ko-kr/different/about.md")]
    public async Task Given_UnpairedTranslation_When_Loaded_Then_It_Should_NotPublishIndependently(string path)
    {
        var fixture = new Fixture();
        fixture.Add($"pages/{path}", "slug: about");
        (await fixture.Load()).ShouldBeEmpty();
    }

    [Theory]
    [InlineData("about", "about", true)]
    [InlineData("custom/about", "custom/about", true)]
    [InlineData("custom/about", "ko-kr/custom/about", true)]
    [InlineData("about", "different", false)]
    public async Task Given_PairedSlugs_When_Loaded_Then_It_Should_ValidateWithoutOverriding(
        string primarySlug, string translatedSlug, bool valid)
    {
        var fixture = new Fixture();
        fixture.Add("pages/about.md", $"slug: {primarySlug}");
        fixture.Add("pages/ko-kr/about.md", $"slug: {translatedSlug}");
        if (valid)
        {
            var documents = await fixture.Load();
            documents.Select(d => d.Metadata.Slug).ShouldContain("ko-kr/" + primarySlug);
        }
        else
        {
            var error = await Should.ThrowAsync<InvalidDataException>(fixture.Load);
            error.Message.ShouldContain("Paired slugs");
            error.Message.ShouldContain("different");
        }
    }

    [Theory]
    [InlineData("2026-09-01", "2026-09-01", true)]
    [InlineData("2026-09-01T09:00:00+09:00", "2026-09-01T12:00:00Z", true)]
    [InlineData("2026-09-01T00:30:00+09:00", "2026-08-31T15:30:00Z", false)]
    [InlineData(null, "2026-09-01", false)]
    [InlineData("2026-09-01", null, false)]
    [InlineData(null, null, false)]
    public async Task Given_PairedPosts_When_Loaded_Then_It_Should_RequireMatchingWrittenDates(
        string? primaryDate, string? translatedDate, bool valid)
    {
        var fixture = new Fixture();
        fixture.Add("posts/hello.md", primaryDate is null ? "" : $"published: {primaryDate}");
        fixture.Add("posts/ko-kr/hello.md", translatedDate is null ? "" : $"published: {translatedDate}");
        if (valid)
        {
            var documents = await fixture.Load();
            documents.Select(d => d.Metadata.Slug).ShouldBe(
                ["2026/09/01/hello", "ko-kr/2026/09/01/hello"], ignoreOrder: true);
        }
        else
        {
            var error = await Should.ThrowAsync<InvalidDataException>(fixture.Load);
            error.Message.ShouldContain("published");
            error.Message.ShouldContain("hello.md");
        }
    }

    [Theory]
    [InlineData("pages/linked")]
    [InlineData("pages/linked/about.md")]
    public async Task Given_LinkedInput_When_Loaded_Then_It_Should_NotTraverseIt(string linkedPath)
    {
        var fixture = new Fixture();
        fixture.Add("pages/linked/about.md");
        var path = Path.Combine(fixture.Contents, linkedPath);
        fixture.FileSystem.File.SetAttributes(path, fixture.FileSystem.File.GetAttributes(path) | FileAttributes.ReparsePoint);
        var error = await Should.ThrowAsync<InvalidDataException>(fixture.Load);
        error.Message.ShouldContain("filesystem link");
    }

    [Theory]
    [InlineData("2026-09-25", 9)]
    [InlineData("2026-09-25T09:00:00", 9)]
    [InlineData("2026-09-25T09:00:00Z", 0)]
    [InlineData("2026-09-25T09:00:00-04:00", -4)]
    public async Task Given_PostFrontmatter_When_Loaded_Then_It_Should_ResolveTheZoneWithoutShiftingTheWrittenDate(
        string published, int offsetHours)
    {
        var fixture = new Fixture(new SiteManifest { TimeZone = "Asia/Seoul", UseDateInPostUrl = true });
        fixture.Add("posts/post.md", $"published: {published}");

        var document = (await fixture.Load()).ShouldHaveSingleItem();

        document.Metadata.Published.ShouldNotBeNull();
        document.Metadata.Published.Value.Offset.ShouldBe(TimeSpan.FromHours(offsetHours));
        document.Metadata.Slug.ShouldBe("2026/09/25/post");
    }

    [Theory]
    [InlineData(false, "2026-03-08T02:30:00")]
    [InlineData(true, "2026-03-08T02:30:00")]
    [InlineData(false, "2026-11-01T01:30:00")]
    [InlineData(true, "2026-11-01T01:30:00")]
    public async Task Given_UnresolvedDaylightSavingTime_When_Loaded_Then_It_Should_ReportTheFieldAndSource(
        bool preview, string published)
    {
        var fixture = new Fixture(new SiteManifest { IsPreview = preview, TimeZone = "America/New_York" });
        fixture.Add("posts/post.md", $"published: {published}");

        var error = await Should.ThrowAsync<InvalidDataException>(fixture.Load);

        error.Message.ShouldContain("published");
        error.Message.ShouldContain("post.md");
    }

    [Fact]
    public async Task Given_PageInDaylightSavingGap_When_Loaded_Then_It_Should_PreserveUnschedulingPageBehavior()
    {
        var fixture = new Fixture(new SiteManifest { TimeZone = "America/New_York" });
        fixture.Add("pages/page.md", "published: 2026-03-08T02:30:00");

        var document = (await fixture.Load()).ShouldHaveSingleItem();

        document.Metadata.Published!.Value.Offset.ShouldBe(TimeSpan.Zero);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Given_InvalidTimeZoneWithoutContent_When_Loaded_Then_It_Should_FailConfigurationValidation(bool preview)
    {
        var fixture = new Fixture(new SiteManifest { IsPreview = preview, TimeZone = "not-a-time-zone" });

        var error = await Should.ThrowAsync<InvalidDataException>(fixture.Load);

        error.Message.ShouldContain("Site:TimeZone");
    }

    [Fact]
    public async Task Given_Cancellation_When_Loaded_Then_It_Should_NotEnumerateContent()
    {
        var fixture = new Fixture();
        await Should.ThrowAsync<OperationCanceledException>(() => fixture.Loader.LoadAsync(new CancellationToken(true)));
    }

    private sealed class Fixture
    {
        public Fixture(SiteManifest? site = null)
        {
            var root = Path.Combine(Path.GetPathRoot(Environment.CurrentDirectory)!, "loader-locales");
            Contents = Path.Combine(root, "contents");
            Loader = new ContentLoader(new TestAppPaths(root, Contents, Path.Combine(root, "themes")), FileSystem,
                site ?? new SiteManifest
                {
                    Locales = ["en-us", "ko-kr"],
                    UseDateInPostUrl = true,
                }, Substitute.For<ILogger<ContentLoader>>());
        }

        public MockFileSystem FileSystem { get; } = new();
        public string Contents { get; }
        public ContentLoader Loader { get; }
        public void Add(string path, string metadata = "") =>
            FileSystem.AddFile(Path.Combine(Contents, path), new MockFileData($"---\n{metadata}\n---\n# Content"));
        public async Task<List<ContentDocument>> Load() =>
            (await Loader.LoadAsync(Xunit.TestContext.Current.CancellationToken)).ToList();
    }
}
