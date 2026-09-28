using System.IO.Abstractions.TestingHelpers;
using System.Text.Json;

using Microsoft.Extensions.Logging;

using ScissorHands.Core.Manifests;
using ScissorHands.Web.Localization;
using ScissorHands.Web.Services;
using ScissorHands.Web.Tests.TestDoubles;

namespace ScissorHands.Web.Tests.Services;

public class ThemeSettingsLocalizationTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Given_ApplicationSettings_When_LocalesApplied_Then_It_Should_ReturnNormalizedSettingsWithoutMutation(bool enabled)
    {
        var site = new SiteManifest { Locales = enabled ? ["en-us", "ko-kr"] : [] };
        var settings = new ThemeSettings
        {
            Localization = new Dictionary<string, ThemeLocalization?>
            {
                ["EN_US"] = ThemeLocalization.English with { Draft = "Configured draft" },
                ["KO_kr"] = ThemeLocalization.English with { Draft = "초안" },
                ["ja-jp"] = null,
            },
            HeroImages = [new ThemeHeroImage { Source = "/images/hero.webp", Alt = "A landscape" }],
        };
        var locales = LocaleConfiguration.Create(site, settings);

        var effective = locales.ApplyTo(settings);

        effective.ShouldNotBeSameAs(settings);
        effective.Localization.ShouldNotBeSameAs(settings.Localization);
        effective.Localization.ShouldNotBeSameAs(locales.Localization);
        effective.HeroImages.ShouldNotBeSameAs(settings.HeroImages);
        effective.HeroImages.ShouldBe(settings.HeroImages);
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
        settings.Localization.Keys.ShouldBe(["EN_US", "KO_kr", "ja-jp"]);
        site.IsLocalizationEnabled.ShouldBe(enabled);
    }

    [Fact]
    public void Given_NullSettings_When_ApplyingLocaleCatalog_Then_It_Should_RejectThem()
    {
        var locales = LocaleConfiguration.Create(new SiteManifest(), new ThemeSettings());

        Should.Throw<ArgumentNullException>(() => locales.ApplyTo(null!)).ParamName.ShouldBe("settings");
    }

    [Fact]
    public void Given_JsonSettings_When_Deserialized_Then_It_Should_PreserveNullableCatalogEntries()
    {
        const string json = """
            {
              "Localization": {
                "en": { "Draft": "Configured" },
                "ko": null
              }
            }
            """;

        var settings = JsonSerializer.Deserialize<ThemeSettings>(json);

        settings.ShouldNotBeNull();
        settings.Localization["en"]!.Draft.ShouldBe("Configured");
        settings.Localization["en"]!.TranslationUnavailable.ShouldBeNull();
        settings.Localization["ko"].ShouldBeNull();
    }

    [Fact]
    public async Task Given_ApplicationSettingsAndPackage_When_Loaded_Then_It_Should_KeepPackageMetadataSeparate()
    {
        var site = new SiteManifest { Locales = ["en", "ko"] };
        var settings = new ThemeSettings
        {
            Localization = new Dictionary<string, ThemeLocalization?>
            {
                ["EN"] = ThemeLocalization.English with { Draft = "Application draft" },
                ["ko"] = ThemeLocalization.English with { Draft = "초안" },
                ["ja"] = null,
            },
        };
        var package = """
            {
              "NAME":"Package name","SLUG":"package","Version":"2.3.4",
              "description":"Package description",
              "stylesheets":["/package.css"],"scripts":["/package.js"]
            }
            """;
        var service = CreateService(package);

        var manifest = await service.LoadManifestAsync("package", Xunit.TestContext.Current.CancellationToken);
        var effective = LocaleConfiguration.Create(site, settings).ApplyTo(settings);

        manifest.Name.ShouldBe("Package name");
        manifest.Slug.ShouldBe("package");
        manifest.Version.ShouldBe("2.3.4");
        manifest.Description.ShouldBe("Package description");
        manifest.Stylesheets.ShouldBe(["/package.css"]);
        manifest.Scripts.ShouldBe(["/package.js"]);
        effective.Localization.Keys.ShouldBe(["en", "ko"]);
        effective.Localization["en"]!.Draft.ShouldBe("Application draft");
        effective.Localization["ko"]!.Draft.ShouldBe("초안");
        settings.Localization.Keys.ShouldBe(["EN", "ko", "ja"]);
    }

    [Theory]
    [InlineData("null")]
    [InlineData("42")]
    [InlineData("\"not a catalog\"")]
    [InlineData("""{"en":{"Draft":false}}""")]
    public async Task Given_LegacyPackageCatalog_When_Loaded_Then_It_Should_ExplainApplicationSettingsMigration(string catalog)
    {
        var package = $$"""{"name":"Package","slug":"package","LOCALIZATION":{{catalog}}}""";
        var service = CreateService(package);

        var exception = await Should.ThrowAsync<InvalidDataException>(() =>
            service.LoadManifestAsync("package", Xunit.TestContext.Current.CancellationToken));

        exception.Message.ShouldContain("Theme:Localization");
    }

    [Fact]
    public async Task Given_NoConfiguredLocales_When_LoadingDefaultTheme_Then_It_Should_UseEnglishWithoutDeclaringRoutes()
    {
        var service = CreateService(null);
        var site = new SiteManifest();
        var settings = new ThemeSettings
        {
            Localization = new Dictionary<string, ThemeLocalization?>
            {
                ["en"] = new() { Draft = "Not the disabled default", ScheduledOn = "{1}" },
                ["ko"] = null,
            },
        };

        var manifest = await service.LoadManifestAsync("default", Xunit.TestContext.Current.CancellationToken);
        var locales = LocaleConfiguration.Create(site, settings);
        var effective = locales.ApplyTo(settings);

        manifest.Name.ShouldBe("Default");
        manifest.Slug.ShouldBe("default");
        effective.Localization.Keys.ShouldBe(["en"]);
        effective.Localization["en"].ShouldBe(ThemeLocalization.English);
        locales.Primary.ShouldBeNull();
        locales.AdditionalLocales.ShouldBeEmpty();
        settings.Localization["en"]!.Draft.ShouldBe("Not the disabled default");
    }

    private static ThemeService CreateService(string? package)
    {
        var fileSystem = new MockFileSystem();
        var root = fileSystem.Path.Combine(fileSystem.Path.GetPathRoot(Environment.CurrentDirectory)!, "theme-catalog-test");
        var themes = fileSystem.Path.Combine(root, "themes");
        var paths = new TestAppPaths(root, fileSystem.Path.Combine(root, "contents"), themes);
        if (package is not null)
        {
            fileSystem.AddFile(fileSystem.Path.Combine(themes, "package", "theme.json"), new MockFileData(package));
        }

        return new ThemeService(paths, fileSystem, new SiteManifest(), Substitute.For<ILogger<ThemeService>>());
    }
}
