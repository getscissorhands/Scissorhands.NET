using System.Globalization;

using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;

namespace ScissorHands.Web.Tests.Themes;

public class DefaultPublicationLocalizationTests
{
    [Theory]
    [InlineData("en-us", "Draft", "Scheduled on 2026-09-26")]
    [InlineData("ko-kr", "초안", "2026-09-26 공개 예정")]
    [InlineData("ja-jp", "下書き", "2026-09-26に公開予定")]
    public void Given_RequestedLocale_When_BadgesRendered_Then_It_Should_UseCatalogInsteadOfContentLanguage(
        string locale, string draft, string scheduled)
    {
        using var context = new BunitContext();
        var beforeCulture = CultureInfo.CurrentCulture;
        var beforeUiCulture = CultureInfo.CurrentUICulture;
        var document = new ContentDocument
        {
            Metadata = new ContentMetadata { Locale = "en-us" },
            PublicationStatus = new PublicationStatus { Route = "post", IsDraft = true, ScheduledDate = new DateOnly(2026, 9, 26) },
        };

        var rendered = context.Render<PublicationBadges>(parameters => parameters
            .AddCascadingValue(new SiteManifest { Locales = ["en-us", "ja-jp", "ko-kr"], IsPreview = true })
            .AddCascadingValue(new LocaleContext { Locale = locale, ContentLocale = "en-us", IsFallback = locale != "en-us" })
            .AddCascadingValue(Catalog())
            .AddCascadingValue(document));

        rendered.Find("[data-publication-badge='draft']").TextContent.ShouldBe(draft);
        rendered.Find("[data-publication-badge='scheduled']").TextContent.ShouldBe(scheduled);
        rendered.Find("[data-publication-badge='scheduled']").GetAttribute("data-publication-date").ShouldBe("2026-09-26");
        CultureInfo.CurrentCulture.ShouldBeSameAs(beforeCulture);
        CultureInfo.CurrentUICulture.ShouldBeSameAs(beforeUiCulture);
    }

    [Theory]
    [InlineData(true, "초안")]
    [InlineData(false, "Draft")]
    public void Given_NoRenderLocale_When_BadgesRendered_Then_It_Should_UsePrimaryOrEnglishWithoutInferringCatalogLocales(bool declared, string expected)
    {
        using var context = new BunitContext();
        var rendered = context.Render<PublicationBadges>(parameters => parameters
            .AddCascadingValue(new SiteManifest { Locales = declared ? ["ko-kr"] : [], IsPreview = true })
            .AddCascadingValue(Catalog())
            .AddCascadingValue(new ContentDocument { PublicationStatus = new PublicationStatus { Route = "post", IsDraft = true } }));

        rendered.Find("[data-publication-badge='draft']").TextContent.ShouldBe(expected);
    }

    [Fact]
    public void Given_UnvalidatedMissingCatalog_When_BadgesRenderedDirectly_Then_It_Should_ReportTheLocale()
    {
        using var context = new BunitContext();

        var error = Should.Throw<InvalidOperationException>(() => context.Render<PublicationBadges>(parameters => parameters
            .AddCascadingValue(new SiteManifest { Locales = ["ko-kr"], IsPreview = true })
            .AddCascadingValue(new ThemeManifest())
            .AddCascadingValue(new ContentDocument { PublicationStatus = new PublicationStatus { Route = "post", IsDraft = true } })));

        error.Message.ShouldContain("Theme:Localization:ko-kr");
    }

    [Fact]
    public void Given_ConfiguredLabelMarkup_When_BadgesRendered_Then_It_Should_EncodeTheText()
    {
        using var context = new BunitContext();
        const string label = "<img src=x onerror=alert(1)> 초안";
        var rendered = context.Render<PublicationBadges>(parameters => parameters
            .AddCascadingValue(new SiteManifest { Locales = ["ko-kr"], IsPreview = true })
            .AddCascadingValue(new ThemeManifest
            {
                Localization = new Dictionary<string, ThemeLocalization?>
                {
                    ["ko-kr"] = new() { TranslationUnavailable = "Unavailable", Draft = label, ScheduledOn = "{0} 공개 예정" },
                },
            })
            .AddCascadingValue(new ContentDocument { PublicationStatus = new PublicationStatus { Route = "post", IsDraft = true } }));

        rendered.FindAll("img").ShouldBeEmpty();
        rendered.Find("[data-publication-badge='draft']").TextContent.ShouldBe(label);
    }

    private static ThemeManifest Catalog() => new()
    {
        Localization = new Dictionary<string, ThemeLocalization?>
        {
            ["en-us"] = ThemeLocalization.English,
            ["ko-kr"] = new() { TranslationUnavailable = "한국어 번역 없음", Draft = "초안", ScheduledOn = "{0} 공개 예정" },
            ["ja-jp"] = new() { TranslationUnavailable = "日本語訳なし", Draft = "下書き", ScheduledOn = "{0}に公開予定" },
        },
    };
}
