using System.Text;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using ScissorHands.Core.Manifests;
using ScissorHands.Web.Extensions;
using ScissorHands.Web.Localization;

namespace ScissorHands.Web.Tests.Localization;

public class LocaleCatalogContractTests
{
    [Theory]
    [InlineData("{}")]
    [InlineData("""{"Site":{}}""")]
    [InlineData("""{"Site":{"Locales":null}}""")]
    [InlineData("""{"Site":{"Locales":[]}}""")]
    [InlineData("""{"Site":{"Locales":""}}""")]
    [InlineData("""{"Site":{"Locales":[]},"Theme":{"Localization":null}}""")]
    [InlineData("""{"Theme":{"Localization":{}}}""")]
    [InlineData("""{"Site":{"Locales":[]},"Theme":{"Localization":{}}}""")]
    [InlineData("""{"Site":{"Locales":[]},"Theme":{"Localization":""}}""")]
    public void Given_DisabledJsonInventory_When_AddConfigurations_Invoked_Then_It_Should_KeepLocalizationDisabled(string json)
    {
        // Arrange
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        var config = new ConfigurationBuilder().AddJsonStream(stream).Build();
        var services = new ServiceCollection();

        // Act
        services.AddConfigurations(config);
        using var provider = services.BuildServiceProvider();
        var site = provider.GetRequiredService<SiteManifest>();
        var theme = provider.GetRequiredService<ThemeManifest>();
        var locales = LocaleConfiguration.Create(site, theme);

        // Assert
        site.Locales.ShouldBeEmpty();
        site.IsLocalizationEnabled.ShouldBeFalse();
        theme.Localization.ShouldBeEmpty();
        locales.Primary.ShouldBeNull();
        locales.AdditionalLocales.ShouldBeEmpty();
        locales.GetDirectoryLocale("en").ShouldBeNull();
        locales.Localization.Keys.ShouldBe(["en"]);
        locales.Localization["en"].ShouldBe(ThemeLocalization.English);
    }

    [Theory]
    [InlineData("""[" EN_us "]""", "en-us", "")]
    [InlineData("""[" EN_us ","ko_KR","ja-JP"]""", "en-us", "ko-kr,ja-jp")]
    [InlineData("""["zh-Hant-TW","sr-Latn-RS"]""", "zh-hant-tw", "sr-latn-rs")]
    [InlineData("""["es-419","en-US"]""", "es-419", "en-us")]
    [InlineData("""["en","ko","ja"]""", "en", "ko,ja")]
    public void Given_OrderedJsonInventory_When_AddConfigurations_Invoked_Then_It_Should_NormalizeWithoutReordering(
        string array, string primary, string additional)
    {
        // Arrange
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("{\"Site\":{\"Locales\":" + array + "}}"));
        var config = new ConfigurationBuilder().AddJsonStream(stream).Build();
        var services = new ServiceCollection();

        // Act
        services.AddConfigurations(config);
        using var provider = services.BuildServiceProvider();
        var site = provider.GetRequiredService<SiteManifest>();
        var locales = LocaleConfiguration.Create(site);

        // Assert
        site.IsLocalizationEnabled.ShouldBeTrue();
        locales.Primary.ShouldBe(primary);
        locales.AdditionalLocales.ShouldBe(additional.Split(',', StringSplitOptions.RemoveEmptyEntries));
        locales.GetDirectoryLocale(primary).ShouldBeNull();
        locales.Localization.ShouldBeEmpty();
        foreach (var locale in locales.AdditionalLocales)
        {
            locales.GetDirectoryLocale(locale.ToUpperInvariant().Replace('-', '_')).ShouldBe(locale);
        }
    }

    [Theory]
    [InlineData("null", "Site:Locales:0")]
    [InlineData("\"\"", "Site:Locales:0")]
    [InlineData("\"   \"", "Site:Locales:0")]
    [InlineData("{}", "Site:Locales:0")]
    [InlineData("[]", "Site:Locales:0")]
    [InlineData("true", "Site:Locales:0")]
    [InlineData("42", "Site:Locales:0")]
    [InlineData("\"en\",null,\"ko\"", "Site:Locales:1")]
    [InlineData("\"en\",{},\"ko\"", "Site:Locales:1")]
    [InlineData("\"en\",\"EN\"", "Site:Locales:1")]
    [InlineData("\"en-US\",\" en_us \"", "Site:Locales:1")]
    [InlineData("\"en-US\",\"ko\"", "Site:Locales:1")]
    [InlineData("\"zh-Hant-TW\",\"en-US\"", "Site:Locales:1")]
    [InlineData("\"../en\"", "Site:Locales:0")]
    [InlineData("\"en?x\"", "Site:Locales:0")]
    public void Given_InvalidJsonArrayItem_When_AddConfigurations_Invoked_Then_It_Should_RejectWithoutSkipping(
        string items, string path)
    {
        // Arrange
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("{\"Site\":{\"Locales\":[" + items + "]}}"));
        var config = new ConfigurationBuilder().AddJsonStream(stream).Build();

        // Act
        var exception = Should.Throw<InvalidDataException>(() => new ServiceCollection().AddConfigurations(config));

        // Assert
        exception.Message.ShouldContain(path);
    }

    [Theory]
    [InlineData("\"en-us\"")]
    [InlineData("\" \"")]
    [InlineData("false")]
    [InlineData("123")]
    [InlineData("""{"primary":"en"}""")]
    public void Given_ScalarOrNamedInventory_When_AddConfigurations_Invoked_Then_It_Should_ExplainTheArrayContract(string value)
    {
        // Arrange
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("{\"Site\":{\"Locales\":" + value + "}}"));
        var config = new ConfigurationBuilder().AddJsonStream(stream).Build();

        // Act
        var exception = Should.Throw<InvalidDataException>(() => new ServiceCollection().AddConfigurations(config));

        // Assert
        exception.Message.ShouldContain("Site:Locales");
        exception.Message.ShouldContain("array");
    }

    [Theory]
    [InlineData("Site:Locales", "Site:Locales:0", "en")]
    [InlineData("Theme:Localization", "Theme:Localization:en:Draft", "Draft")]
    public void Given_EmptyMarkerWithChildren_When_AddConfigurations_Invoked_Then_It_Should_RejectTheMixedShape(
        string path, string childPath, string childValue)
    {
        // Arrange
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            [path] = string.Empty,
            [childPath] = childValue,
        }).Build();

        // Act
        var exception = Should.Throw<InvalidDataException>(() => new ServiceCollection().AddConfigurations(config));

        // Assert
        exception.Message.ShouldContain(path);
    }

    [Theory]
    [InlineData("Locale", "null")]
    [InlineData("Locale", "\"\"")]
    [InlineData("Locale", "\"en-us\"")]
    [InlineData("LocalizationFallbackMessages", "null")]
    [InlineData("LocalizationFallbackMessages", "{}")]
    [InlineData("LocalizationFallbackMessages", """{"ko":"Notice"}""")]
    public void Given_LegacyConfiguration_When_AddConfigurations_Invoked_Then_It_Should_ExplainMigrationEvenForNull(
        string key, string value)
    {
        // Arrange
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("{\"Site\":{\"" + key + "\":" + value + "}}"));
        var config = new ConfigurationBuilder().AddJsonStream(stream).Build();

        // Act
        var exception = Should.Throw<InvalidDataException>(() => new ServiceCollection().AddConfigurations(config));

        // Assert
        exception.Message.ShouldContain($"Site:{key}");
        exception.Message.ShouldContain("Site:Locales");
        exception.Message.ShouldContain("Theme:Localization");
        exception.Message.ShouldContain("primary first");
    }

    [Theory]
    [InlineData("""{"Slug":"default"}""")]
    [InlineData("""{"Localization":{"en":{"Draft":"Draft"}}}""")]
    [InlineData("""{"Localization":{}}""")]
    [InlineData("""{"Localization":null}""")]
    [InlineData("""["default"]""")]
    public void Given_NestedSiteTheme_When_AddConfigurations_Invoked_Then_It_Should_ExplainTheStringSlugAndTopLevelCatalog(string value)
    {
        // Arrange
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("{\"Site\":{\"Theme\":" + value + "}}"));
        var config = new ConfigurationBuilder().AddJsonStream(stream).Build();

        // Act
        var exception = Should.Throw<InvalidDataException>(() => new ServiceCollection().AddConfigurations(config));

        // Assert
        exception.Message.ShouldContain("Site:Theme");
        exception.Message.ShouldContain("string slug");
        exception.Message.ShouldContain("top-level Theme:Localization");
    }

    [Fact]
    public void Given_ApplicationThemeSection_When_AddConfigurations_Invoked_Then_It_Should_BindOnlyCatalogAsSingleton()
    {
        // Arrange
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("""
            {
              "Site": { "Theme": "package", "Locales": [" EN_us ", "ko-KR"] },
              "Theme": {
                "Name": "Not package metadata", "Slug": "ignored", "Version": "99",
                "Description": "ignored", "Stylesheets": ["/ignored.css"], "Scripts": ["/ignored.js"],
                "Localization": {
                  "EN_US": { "TranslationUnavailable": "Unavailable", "Draft": "Draft", "ScheduledOn": "Scheduled on {0}" },
                  "ko-KR": { "TranslationUnavailable": "번역 없음", "Draft": "초안", "ScheduledOn": "{0}에 게시 예정" },
                  "ja-JP": null,
                  "fr-FR": "unused malformed entry"
                }
              }
            }
            """));
        var config = new ConfigurationBuilder().AddJsonStream(stream).Build();
        var services = new ServiceCollection();

        // Act
        services.AddConfigurations(config);
        using var provider = services.BuildServiceProvider();
        var site = provider.GetRequiredService<SiteManifest>();
        var theme = provider.GetRequiredService<ThemeManifest>();
        var locales = LocaleConfiguration.Create(site, theme);

        // Assert
        site.Theme.ShouldBe("package");
        site.Locales.ShouldBe([" EN_us ", "ko-KR"]);
        theme.ShouldBeSameAs(provider.GetRequiredService<ThemeManifest>());
        theme.Name.ShouldBeEmpty();
        theme.Slug.ShouldBeEmpty();
        theme.Version.ShouldBe("1.0.0");
        theme.Description.ShouldBeNull();
        theme.Stylesheets.ShouldBeEmpty();
        theme.Scripts.ShouldBeEmpty();
        theme.Localization["ja-JP"].ShouldBeNull();
        theme.Localization["fr-FR"].ShouldBeNull();
        locales.Localization.Keys.ShouldBe(["en-us", "ko-kr"]);
        locales.GetFallbackMessage("KO_kr").ShouldBe("번역 없음");
        locales.GetDirectoryLocale("ja-jp").ShouldBeNull();
        config["Theme:Slug"].ShouldBe("ignored");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("../en")]
    [InlineData("en\\us")]
    [InlineData("en-us#fragment")]
    [InlineData("english")]
    public void Given_InvalidDirectInventory_When_Create_Invoked_Then_It_Should_RejectTheItem(string? locale)
    {
        // Arrange
        var site = new SiteManifest { Locales = [locale!] };

        // Act
        var exception = Should.Throw<InvalidDataException>(() => LocaleConfiguration.Create(site));

        // Assert
        site.IsLocalizationEnabled.ShouldBeTrue();
        exception.Message.ShouldContain("Site:Locales:0");
    }

    [Theory]
    [InlineData("en-US", " EN_us ", "Duplicate")]
    [InlineData("en-US", "en/US", "Duplicate")]
    [InlineData("en", "ko-KR", "structure")]
    [InlineData("zh-Hant", "en-US", "structure")]
    [InlineData("zh-Hant-TW", "en-US", "structure")]
    public void Given_DuplicateOrMismatchedDirectInventory_When_Create_Invoked_Then_It_Should_ExplainTheInvalidLocale(
        string primary, string additional, string reason)
    {
        // Arrange
        var site = new SiteManifest { Locales = [primary, additional] };

        // Act
        var exception = Should.Throw<InvalidDataException>(() => LocaleConfiguration.Create(site));

        // Assert
        exception.Message.ShouldContain("Site:Locales:1");
        exception.Message.ShouldContain(reason);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Given_MissingPrimaryCatalog_When_Create_Invoked_Then_It_Should_FailInEitherMode(bool preview)
    {
        // Arrange
        var site = new SiteManifest { Locales = ["en"], IsPreview = preview };

        // Act
        var exception = Should.Throw<InvalidDataException>(() => LocaleConfiguration.Create(site, new ThemeManifest()));

        // Assert
        exception.Message.ShouldContain("Theme:Localization:en");
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("""{"en":null}""")]
    [InlineData("""{"en":"not an entry"}""")]
    [InlineData("""{"en":{}}""")]
    [InlineData("""{"en":{"Draft":"Draft","ScheduledOn":"Scheduled {0}"}}""")]
    [InlineData("""{"en":{"TranslationUnavailable":"Notice","ScheduledOn":"Scheduled {0}"}}""")]
    [InlineData("""{"en":{"TranslationUnavailable":"Notice","Draft":"Draft"}}""")]
    public void Given_MissingConfiguredEntryOrField_When_Create_Invoked_Then_It_Should_NotSupplyEnglishDefaults(string catalog)
    {
        // Arrange
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(
            "{\"Site\":{\"Locales\":[\"en\"]},\"Theme\":{\"Localization\":" + catalog + "}}"));
        var config = new ConfigurationBuilder().AddJsonStream(stream).Build();
        var services = new ServiceCollection().AddConfigurations(config);
        using var provider = services.BuildServiceProvider();
        var site = provider.GetRequiredService<SiteManifest>();
        var theme = provider.GetRequiredService<ThemeManifest>();

        // Act
        var exception = Should.Throw<InvalidDataException>(() => LocaleConfiguration.Create(site, theme));

        // Assert
        exception.Message.ShouldContain("Theme:Localization:en");
    }

    [Theory]
    [InlineData("TranslationUnavailable", null)]
    [InlineData("TranslationUnavailable", "")]
    [InlineData("TranslationUnavailable", " \t")]
    [InlineData("Draft", null)]
    [InlineData("Draft", "")]
    [InlineData("Draft", " \t")]
    [InlineData("ScheduledOn", null)]
    [InlineData("ScheduledOn", "")]
    [InlineData("ScheduledOn", " \t")]
    public void Given_BlankActiveMessage_When_Create_Invoked_Then_It_Should_ReportTheLocaleAndField(string field, string? value)
    {
        // Arrange
        var messages = field switch
        {
            "TranslationUnavailable" => ThemeLocalization.English with { TranslationUnavailable = value },
            "Draft" => ThemeLocalization.English with { Draft = value },
            _ => ThemeLocalization.English with { ScheduledOn = value },
        };
        var site = new SiteManifest { Locales = ["en", "ko"] };
        var theme = new ThemeManifest
        {
            Localization = new Dictionary<string, ThemeLocalization?>
            {
                ["en"] = ThemeLocalization.English,
                ["ko"] = messages,
            },
        };

        // Act
        var exception = Should.Throw<InvalidDataException>(() => LocaleConfiguration.Create(site, theme));

        // Assert
        exception.Message.ShouldContain($"Theme:Localization:ko:{field}");
    }

    [Theory]
    [InlineData("Scheduled soon")]
    [InlineData("Scheduled {{0}}")]
    [InlineData("Scheduled {{{{0}}}}")]
    [InlineData("Scheduled {")]
    [InlineData("Scheduled }")]
    [InlineData("Scheduled {date}")]
    [InlineData("Scheduled {1}")]
    [InlineData("Scheduled {0} {1}")]
    [InlineData("Scheduled {-1}")]
    [InlineData("Scheduled {0,invalid}")]
    public void Given_InvalidScheduledTemplate_When_Create_Invoked_Then_It_Should_ReportTheTemplatePath(string template)
    {
        // Arrange
        var site = new SiteManifest { Locales = ["en"] };
        var theme = CreateTheme("en", ThemeLocalization.English with { ScheduledOn = template });

        // Act
        var exception = Should.Throw<InvalidDataException>(() => LocaleConfiguration.Create(site, theme));

        // Assert
        exception.Message.ShouldContain("Theme:Localization:en:ScheduledOn");
        exception.InnerException.ShouldBeOfType<FormatException>();
    }

    [Theory]
    [InlineData("Scheduled on {0}")]
    [InlineData("{0}에 게시 예정")]
    [InlineData("On {0}, scheduled")]
    [InlineData("{0} / {0}")]
    [InlineData("{{notice}} {{{0}}}")]
    [InlineData("Scheduled {0,12}")]
    [InlineData("Scheduled {0:yyyy-MM-dd}")]
    public void Given_ValidScheduledTemplate_When_Create_Invoked_Then_It_Should_PreserveLocalizedFormatting(string template)
    {
        // Arrange
        var site = new SiteManifest { Locales = ["ko"] };
        var theme = CreateTheme("KO", ThemeLocalization.English with { ScheduledOn = template });

        // Act
        var locales = LocaleConfiguration.Create(site, theme);

        // Assert
        locales.Localization["ko"]!.ScheduledOn.ShouldBe(template);
    }

    [Fact]
    public void Given_ExtraInvalidCatalogEntries_When_Create_Invoked_Then_It_Should_OnlyUseDeclaredLocales()
    {
        // Arrange
        var site = new SiteManifest { Locales = ["en", "ko"] };
        var theme = new ThemeManifest
        {
            Localization = new Dictionary<string, ThemeLocalization?>
            {
                ["EN"] = ThemeLocalization.English,
                ["KO"] = ThemeLocalization.English with { TranslationUnavailable = "번역 없음" },
                ["ja"] = null,
                ["../unsafe"] = new() { ScheduledOn = "{9}" },
            },
        };

        // Act
        var locales = LocaleConfiguration.Create(site, theme);

        // Assert
        locales.Primary.ShouldBe("en");
        locales.AdditionalLocales.ShouldBe(["ko"]);
        locales.Localization.Keys.ShouldBe(["en", "ko"]);
        locales.GetDirectoryLocale("en").ShouldBeNull();
        locales.GetDirectoryLocale("ja").ShouldBeNull();
        locales.GetFallbackMessage("ko").ShouldBe("번역 없음");
        theme.Localization.Count.ShouldBe(4);
    }

    [Fact]
    public void Given_NormalizedDuplicateActiveCatalogKeys_When_Create_Invoked_Then_It_Should_ReportTheAmbiguity()
    {
        // Arrange
        var site = new SiteManifest { Locales = ["en-us"] };
        var theme = new ThemeManifest
        {
            Localization = new Dictionary<string, ThemeLocalization?>
            {
                ["en-US"] = ThemeLocalization.English,
                ["EN_us"] = ThemeLocalization.English,
            },
        };

        // Act
        var exception = Should.Throw<InvalidDataException>(() => LocaleConfiguration.Create(site, theme));

        // Assert
        exception.Message.ShouldContain("Theme:Localization");
        exception.Message.ShouldContain("Duplicate");
    }

    [Fact]
    public void Given_InventoryWithoutCatalog_When_GetFallbackMessage_Invoked_Then_It_Should_RequestCatalogValidation()
    {
        // Arrange
        var locales = LocaleConfiguration.Create(new SiteManifest { Locales = ["en", "ko"] });

        // Act
        var exception = Should.Throw<InvalidDataException>(() => locales.GetFallbackMessage("ko"));

        // Assert
        exception.Message.ShouldContain("Theme:Localization:ko:TranslationUnavailable");
    }

    private static ThemeManifest CreateTheme(string locale, ThemeLocalization messages)
        => new() { Localization = new Dictionary<string, ThemeLocalization?> { [locale] = messages } };
}
