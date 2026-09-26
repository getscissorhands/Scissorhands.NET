using System.IO.Abstractions.TestingHelpers;
using System.Text.Json;

using Microsoft.Extensions.Logging;

using ScissorHands.Core.Manifests;
using ScissorHands.Web.Localization;
using ScissorHands.Web.Services;
using ScissorHands.Web.Tests.TestDoubles;

namespace ScissorHands.Web.Tests.Services;

public class ThemeManifestLocalizationTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Given_LoadedThemeAndLocaleConfiguration_When_ApplyTo_Invoked_Then_It_Should_ReturnFreshNormalizedManifestWithoutMutation(bool enabled)
    {
        // Arrange
        var site = new SiteManifest { Locales = enabled ? ["en-us", "ko-kr"] : [] };
        var package = new ThemeManifest
        {
            Name = "Custom service theme",
            Slug = "custom",
            Version = "2.3.4",
            Description = "Custom service description",
            Stylesheets = ["/first.css", "/second.css"],
            Scripts = ["/custom.js"],
            Localization = new Dictionary<string, ThemeLocalization?>
            {
                ["EN_US"] = ThemeLocalization.English with { Draft = "Configured draft" },
                ["KO_kr"] = ThemeLocalization.English with { Draft = "초안" },
                ["ja-jp"] = null,
            },
        };
        var locales = LocaleConfiguration.Create(site, package);

        // Act
        var effective = locales.ApplyTo(package);

        // Assert
        effective.ShouldNotBeSameAs(package);
        effective.Name.ShouldBe(package.Name);
        effective.Slug.ShouldBe(package.Slug);
        effective.Version.ShouldBe(package.Version);
        effective.Description.ShouldBe(package.Description);
        effective.Stylesheets.ShouldBe(["/first.css", "/second.css"]);
        effective.Scripts.ShouldBe(["/custom.js"]);
        effective.Stylesheets.ShouldNotBeSameAs(package.Stylesheets);
        effective.Scripts.ShouldNotBeSameAs(package.Scripts);
        effective.Localization.ShouldNotBeSameAs(package.Localization);
        effective.Localization.ShouldNotBeSameAs(locales.Localization);
        if (enabled)
        {
            effective.Localization.Keys.ShouldBe(["en-us", "ko-kr"]);
            effective.Localization["en-us"]!.Draft.ShouldBe("Configured draft");
            effective.Localization["ko-kr"]!.Draft.ShouldBe("초안");
        }
        else
        {
            effective.Localization.Keys.ShouldBe(["en"]);
            effective.Localization["en"].ShouldBe(ThemeLocalization.English);
        }
        package.Localization.Keys.ShouldBe(["EN_US", "KO_kr", "ja-jp"]);
        package.Localization["EN_US"]!.Draft.ShouldBe("Configured draft");
        site.IsLocalizationEnabled.ShouldBe(enabled);
    }

    [Fact]
    public void Given_NullPackage_When_ApplyTo_Invoked_Then_It_Should_RejectTheMissingManifest()
    {
        // Arrange
        var locales = LocaleConfiguration.Create(new SiteManifest(), new ThemeManifest());

        // Act
        var exception = Should.Throw<ArgumentNullException>(() => locales.ApplyTo(null!));

        // Assert
        exception.ParamName.ShouldBe("package");
    }

    [Fact]
    public void Given_MutableManifestInputs_When_Initialized_Invoked_Then_It_Should_RetainReadOnlySnapshots()
    {
        // Arrange
        var inventory = new List<string> { "en", "ko" };
        var catalog = new Dictionary<string, ThemeLocalization?>
        {
            ["en"] = ThemeLocalization.English,
            ["ko"] = ThemeLocalization.English with { Draft = "초안" },
            ["ja"] = null,
        };
        var stylesheets = new List<string> { "/package.css" };
        var scripts = new List<string> { "/package.js" };
        var site = new SiteManifest { Locales = inventory };
        var theme = new ThemeManifest { Localization = catalog, Stylesheets = stylesheets, Scripts = scripts };
        var locales = LocaleConfiguration.Create(site, theme);

        // Act
        inventory.Clear();
        catalog.Clear();
        stylesheets.Clear();
        scripts.Clear();

        // Assert
        site.Locales.ShouldBe(["en", "ko"]);
        site.IsLocalizationEnabled.ShouldBeTrue();
        theme.Localization.Count.ShouldBe(3);
        theme.Localization["ja"].ShouldBeNull();
        theme.Localization["ko"]!.Draft.ShouldBe("초안");
        theme.Stylesheets.ShouldBe(["/package.css"]);
        theme.Scripts.ShouldBe(["/package.js"]);
        locales.AdditionalLocales.ShouldBe(["ko"]);
        locales.Localization.Keys.ShouldBe(["en", "ko"]);
        Should.Throw<NotSupportedException>(() => ((IList<string>)site.Locales).Add("ja"));
        Should.Throw<NotSupportedException>(() => ((IDictionary<string, ThemeLocalization?>)theme.Localization).Clear());
        Should.Throw<NotSupportedException>(() => ((IList<string>)locales.AdditionalLocales).Clear());
        Should.Throw<NotSupportedException>(() => ((IDictionary<string, ThemeLocalization?>)locales.Localization).Clear());
    }

    [Fact]
    public void Given_NullCollections_When_Initialized_Invoked_Then_It_Should_ExposeEmptySnapshots()
    {
        // Arrange
        IReadOnlyList<string> inventory = null!;
        IReadOnlyDictionary<string, ThemeLocalization?> catalog = null!;

        // Act
        var site = new SiteManifest { Locales = inventory };
        var theme = new ThemeManifest { Localization = catalog };

        // Assert
        site.Locales.ShouldBeEmpty();
        site.IsLocalizationEnabled.ShouldBeFalse();
        theme.Localization.ShouldBeEmpty();
        new ThemeManifest().Localization.ShouldBeEmpty();
    }

    [Fact]
    public void Given_UnconfiguredMessages_When_Initialized_Invoked_Then_It_Should_KeepDefaultsExplicitAndImmutable()
    {
        // Arrange
        var english = ThemeLocalization.English;

        // Act
        var configured = new ThemeLocalization();
        var changed = english with { Draft = "Changed" };

        // Assert
        configured.TranslationUnavailable.ShouldBeNull();
        configured.Draft.ShouldBeNull();
        configured.ScheduledOn.ShouldBeNull();
        english.TranslationUnavailable.ShouldNotBeNullOrWhiteSpace();
        english.Draft.ShouldBe("Draft");
        english.ScheduledOn.ShouldBe("Scheduled on {0}");
        ThemeLocalization.English.ShouldBeSameAs(english);
        changed.Draft.ShouldBe("Changed");
        english.Draft.ShouldBe("Draft");
    }

    [Fact]
    public void Given_JsonManifest_When_Deserialize_Invoked_Then_It_Should_BindNullableCatalogEntries()
    {
        // Arrange
        const string json = """
            {
              "Name": "Package", "Slug": "package",
              "Localization": {
                "en": { "Draft": "Configured" },
                "ko": null
              }
            }
            """;

        // Act
        var manifest = JsonSerializer.Deserialize<ThemeManifest>(json);

        // Assert
        manifest.ShouldNotBeNull();
        manifest.Localization["en"]!.Draft.ShouldBe("Configured");
        manifest.Localization["en"]!.TranslationUnavailable.ShouldBeNull();
        manifest.Localization["en"]!.ScheduledOn.ShouldBeNull();
        manifest.Localization["ko"].ShouldBeNull();
    }

    [Fact]
    public async Task Given_ApplicationCatalogAndPackage_When_LoadManifestAsync_Invoked_Then_It_Should_PreservePackageMetadataAndAssets()
    {
        // Arrange
        var site = new SiteManifest { Theme = "package", Locales = ["en", "ko"] };
        var application = new ThemeManifest
        {
            Name = "Ignored application name",
            Slug = "ignored",
            Version = "99",
            Description = "Ignored",
            Stylesheets = ["/ignored.css"],
            Scripts = ["/ignored.js"],
            Localization = new Dictionary<string, ThemeLocalization?>
            {
                ["EN"] = ThemeLocalization.English with { Draft = "Application draft" },
                ["ko"] = ThemeLocalization.English with { Draft = "초안" },
                ["ja"] = new(),
            },
        };
        var package = """
            {
              "NAME": "Package name", "SLUG": "package", "Version": "2.3.4",
              "description": "Package description",
              "stylesheets": ["/package.css", "/other.css"], "scripts": ["/package.js"],
              "localization": { "en": { "Draft": "Package draft" } }
            }
            """;
        var service = CreateService(site, application, package);

        // Act
        var effective = await service.LoadManifestAsync("package", Xunit.TestContext.Current.CancellationToken);

        // Assert
        effective.Name.ShouldBe("Package name");
        effective.Slug.ShouldBe("package");
        effective.Version.ShouldBe("2.3.4");
        effective.Description.ShouldBe("Package description");
        effective.Stylesheets.ShouldBe(["/package.css", "/other.css"]);
        effective.Scripts.ShouldBe(["/package.js"]);
        effective.Localization.Keys.ShouldBe(["en", "ko"]);
        effective.Localization["en"]!.Draft.ShouldBe("Application draft");
        effective.Localization["ko"]!.Draft.ShouldBe("초안");
        effective.ShouldNotBeSameAs(application);
        effective.Localization.ShouldNotBeSameAs(application.Localization);
        application.Name.ShouldBe("Ignored application name");
        application.Localization.Keys.ShouldBe(["EN", "ko", "ja"]);
        site.Locales.ShouldBe(["en", "ko"]);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Given_PackageMessagesButNoApplicationCatalog_When_LoadManifestAsync_Invoked_Then_It_Should_RejectMissingConfiguration(bool preview)
    {
        // Arrange
        var site = new SiteManifest { Locales = ["en"], IsPreview = preview };
        var package = JsonSerializer.Serialize(new ThemeManifest
        {
            Name = "Package",
            Slug = "package",
            Localization = new Dictionary<string, ThemeLocalization?> { ["en"] = ThemeLocalization.English },
        });
        var service = CreateService(site, new ThemeManifest(), package);

        // Act
        var exception = await Should.ThrowAsync<InvalidDataException>(() =>
            service.LoadManifestAsync("package", Xunit.TestContext.Current.CancellationToken));

        // Assert
        exception.Message.ShouldContain("Theme:Localization:en");
    }

    [Theory]
    [InlineData("Draft")]
    [InlineData("ScheduledOn")]
    [InlineData("TranslationUnavailable")]
    public async Task Given_IncompleteApplicationMessages_When_LoadManifestAsync_Invoked_Then_It_Should_NotMergePackageDefaults(string field)
    {
        // Arrange
        var messages = field switch
        {
            "Draft" => ThemeLocalization.English with { Draft = null },
            "ScheduledOn" => ThemeLocalization.English with { ScheduledOn = null },
            _ => ThemeLocalization.English with { TranslationUnavailable = null },
        };
        var application = new ThemeManifest
        {
            Localization = new Dictionary<string, ThemeLocalization?> { ["en"] = messages },
        };
        var package = JsonSerializer.Serialize(new ThemeManifest
        {
            Name = "Package",
            Slug = "package",
            Localization = new Dictionary<string, ThemeLocalization?> { ["en"] = ThemeLocalization.English },
        });
        var service = CreateService(new SiteManifest { Locales = ["en"] }, application, package);

        // Act
        var exception = await Should.ThrowAsync<InvalidDataException>(() =>
            service.LoadManifestAsync("package", Xunit.TestContext.Current.CancellationToken));

        // Assert
        exception.Message.ShouldContain($"Theme:Localization:en:{field}");
    }

    [Theory]
    [InlineData("null")]
    [InlineData("42")]
    [InlineData("\"not a catalog\"")]
    [InlineData("""{"en":{"Draft":false}}""")]
    public async Task Given_MalformedPackageCatalog_When_LoadManifestAsync_Invoked_Then_It_Should_IgnoreItEntirely(string catalog)
    {
        // Arrange
        var application = new ThemeManifest
        {
            Localization = new Dictionary<string, ThemeLocalization?> { ["en"] = ThemeLocalization.English },
        };
        var package = $$"""{"name":"Package","slug":"package","LOCALIZATION":{{catalog}}}""";
        var service = CreateService(new SiteManifest { Locales = ["en"] }, application, package);

        // Act
        var effective = await service.LoadManifestAsync("package", Xunit.TestContext.Current.CancellationToken);

        // Assert
        effective.Localization["en"].ShouldBe(ThemeLocalization.English);
        effective.Name.ShouldBe("Package");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Given_DisabledLocalization_When_LoadManifestAsync_Invoked_Then_It_Should_UseEnglishWithoutActivatingLocales(bool hasPackage)
    {
        // Arrange
        var site = new SiteManifest();
        var application = new ThemeManifest
        {
            Localization = new Dictionary<string, ThemeLocalization?>
            {
                ["en"] = new() { Draft = "Not the disabled default", ScheduledOn = "{1}" },
                ["ko"] = null,
            },
        };
        var package = hasPackage ? """{"name":"Package","slug":"package","Localization":{"en":null}}""" : null;
        var service = CreateService(site, application, package);

        // Act
        var effective = await service.LoadManifestAsync(hasPackage ? "package" : "default", Xunit.TestContext.Current.CancellationToken);
        var locales = LocaleConfiguration.Create(site, effective);

        // Assert
        effective.Localization.Keys.ShouldBe(["en"]);
        effective.Localization["en"].ShouldBe(ThemeLocalization.English);
        site.IsLocalizationEnabled.ShouldBeFalse();
        site.Locales.ShouldBeEmpty();
        locales.Primary.ShouldBeNull();
        locales.AdditionalLocales.ShouldBeEmpty();
        locales.GetDirectoryLocale("en").ShouldBeNull();
        application.Localization["en"]!.Draft.ShouldBe("Not the disabled default");
    }

    [Fact]
    public async Task Given_ApplicationCatalogWithoutPackageFile_When_LoadManifestAsync_Invoked_Then_It_Should_ComposeBuiltInMetadata()
    {
        // Arrange
        var application = new ThemeManifest
        {
            Localization = new Dictionary<string, ThemeLocalization?>
            {
                ["ko"] = ThemeLocalization.English with { Draft = "초안" },
            },
        };
        var service = CreateService(new SiteManifest { Locales = ["ko"] }, application, null);

        // Act
        var effective = await service.LoadManifestAsync("default", Xunit.TestContext.Current.CancellationToken);

        // Assert
        effective.Name.ShouldBe("Default");
        effective.Slug.ShouldBe("default");
        effective.Localization.Keys.ShouldBe(["ko"]);
        effective.Localization["ko"]!.Draft.ShouldBe("초안");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Given_InvalidActiveTemplate_When_LoadManifestAsync_Invoked_Then_It_Should_RejectBeforeRendering(bool preview)
    {
        // Arrange
        var application = new ThemeManifest
        {
            Localization = new Dictionary<string, ThemeLocalization?>
            {
                ["en"] = ThemeLocalization.English with { ScheduledOn = "Scheduled {{0}}" },
            },
        };
        var service = CreateService(new SiteManifest { Locales = ["en"], IsPreview = preview }, application, null);

        // Act
        var exception = await Should.ThrowAsync<InvalidDataException>(() =>
            service.LoadManifestAsync("default", Xunit.TestContext.Current.CancellationToken));

        // Assert
        exception.Message.ShouldContain("Theme:Localization:en:ScheduledOn");
    }

    [Fact]
    public async Task Given_ExistingFourArgumentConstructor_When_LoadManifestAsync_Invoked_Then_It_Should_PreserveDisabledModeSupport()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var root = fileSystem.Path.Combine(fileSystem.Path.GetPathRoot(Environment.CurrentDirectory)!, "legacy-theme-test");
        var paths = new TestAppPaths(root, fileSystem.Path.Combine(root, "contents"), fileSystem.Path.Combine(root, "themes"));
        var service = new ThemeService(paths, fileSystem, new SiteManifest(), Substitute.For<ILogger<ThemeService>>());

        // Act
        var effective = await service.LoadManifestAsync("default", Xunit.TestContext.Current.CancellationToken);

        // Assert
        effective.Localization["en"].ShouldBe(ThemeLocalization.English);
    }

    private static ThemeService CreateService(SiteManifest site, ThemeManifest application, string? package)
    {
        var fileSystem = new MockFileSystem();
        var root = fileSystem.Path.Combine(fileSystem.Path.GetPathRoot(Environment.CurrentDirectory)!, "theme-catalog-test");
        var themes = fileSystem.Path.Combine(root, "themes");
        var paths = new TestAppPaths(root, fileSystem.Path.Combine(root, "contents"), themes);
        if (package is not null)
        {
            fileSystem.AddFile(fileSystem.Path.Combine(themes, "package", "theme.json"), new MockFileData(package));
        }

        return new ThemeService(paths, fileSystem, site, application, Substitute.For<ILogger<ThemeService>>());
    }
}
