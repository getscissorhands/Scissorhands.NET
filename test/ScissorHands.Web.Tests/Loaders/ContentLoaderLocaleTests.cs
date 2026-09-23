using System.IO.Abstractions.TestingHelpers;

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
            Locale = primary,
            LocalizationFallbackMessages = new Dictionary<string, string?> { [additional] = "Unavailable" },
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
            Locale = "en-us",
            LocalizationFallbackMessages = new Dictionary<string, string?> { ["ko-kr"] = "One", ["ko_KR"] = "Two" },
        });
        var error = await Should.ThrowAsync<InvalidDataException>(fixture.Load);
        error.Message.ShouldContain("Duplicate locale");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Given_DisabledLocalization_When_Loaded_Then_It_Should_IgnoreDictionaryAndTreatFoldersAsOrdinary(string? locale)
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Site:Locale"] = locale,
            ["Site:LocalizationFallbackMessages:../invalid"] = null,
            ["Site:LocalizationFallbackMessages:ko-kr"] = "Unavailable",
        }).Build();
        var site = configuration.GetSection("Site").Get<SiteManifest>()!;
        site.IsLocalizationEnabled.ShouldBeFalse();
        var fixture = new Fixture(site);
        fixture.Add("pages/ko-kr/about.md");
        fixture.Add("pages/it/about.md");

        var documents = await fixture.Load();

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
    [InlineData(false, false, 2)]
    [InlineData(false, true, 1)]
    [InlineData(true, false, 0)]
    [InlineData(true, true, 0)]
    public async Task Given_PrimaryAndTranslationDrafts_When_Loaded_Then_It_Should_ApplyThePrimaryGate(
        bool primaryDraft, bool translatedDraft, int count)
    {
        var fixture = new Fixture();
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
                    Locale = "en-us",
                    UseDateInPostUrl = true,
                    LocalizationFallbackMessages = new Dictionary<string, string?> { ["ko-kr"] = "Unavailable" },
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
