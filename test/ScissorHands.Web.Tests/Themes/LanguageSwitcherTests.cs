using System.Globalization;

using Microsoft.Extensions.DependencyInjection;

using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;
using ScissorHands.Core.Services;

namespace ScissorHands.Web.Tests.Themes;

public class LanguageSwitcherTests
{
    [Fact]
    public void Given_FallbackContext_When_Rendered_Then_It_Should_ShowNativeNamesAndTheRequestedLanguage()
    {
        using var context = new BunitContext();
        var locale = new LocaleContext
        {
            Locale = "ko-kr",
            ContentLocale = "en-us",
            IsFallback = true,
            SwitchLanguageUrls = new Dictionary<string, string>
            {
                ["en-us"] = "about/",
                ["ko-kr"] = "ko-kr/about/",
                ["ja-jp"] = "ja-jp/about/",
            },
        };

        var cut = context.Render<LanguageSwitcher>(parameters => parameters.AddCascadingValue(locale));

        cut.Find(".language-switcher-list").GetAttribute("aria-label").ShouldBe("Language");
        var toggle = cut.Find(".language-switcher-toggle");
        toggle.GetAttribute("aria-controls").ShouldBe("language-switcher-list");
        toggle.GetAttribute("aria-expanded").ShouldBe("false");
        toggle.HasAttribute("hidden").ShouldBeTrue();
        var links = cut.FindAll("a");
        links.Select(link => link.TextContent).ShouldBe(["English", "한국어", "日本語"]);
        links.Select(link => link.GetAttribute("href")).ShouldBe(locale.SwitchLanguageUrls.Values);
        cut.Find("a[aria-current='true']").GetAttribute("lang").ShouldBe("ko-kr");
        links.ShouldAllBe(link => link.GetAttribute("hreflang") == link.GetAttribute("lang"));
        links.ShouldAllBe(link => link.GetAttribute("tabindex") == "0");
        cut.FindAll("script, select").ShouldBeEmpty();
    }

    [Fact]
    public void Given_RegionalVariants_When_Rendered_Then_It_Should_DisambiguateNativeLabels()
    {
        using var context = new BunitContext();
        var cut = context.Render<LanguageSwitcher>(parameters => parameters.AddCascadingValue(new LocaleContext
        {
            Locale = "en-us",
            SwitchLanguageUrls = new Dictionary<string, string> { ["en-us"] = ".", ["en-gb"] = "en-gb/" },
        }));

        cut.FindAll("a").Select(a => a.TextContent).ShouldBe(
            [CultureInfo.GetCultureInfo("en-us").NativeName, CultureInfo.GetCultureInfo("en-gb").NativeName]);
        cut.FindAll("a").Select(a => a.TextContent).Distinct().Count().ShouldBe(2);
    }

    [Fact]
    public void Given_ThemeOverrides_When_Rendered_Then_It_Should_ChangePresentationWithoutChangingDestinations()
    {
        using var context = new BunitContext();
        var urls = new Dictionary<string, string> { ["en-us"] = "about/", ["ko-kr"] = "ko-kr/about/", ["ja-jp"] = "ja-jp/about/" };
        var cut = context.Render<LanguageSwitcher>(parameters => parameters
            .AddCascadingValue(new LocaleContext { Locale = "en-us", SwitchLanguageUrls = urls })
            .Add(p => p.Labels, new Dictionary<string, string> { ["ko-kr"] = "KO <b>한국어</b>" })
            .Add(p => p.LocaleOrder, new[] { "ko-kr", "ja-jp" })
            .Add(p => p.Class, "custom-switcher")
            .Add(p => p.AriaLabel, "Choose a language"));

        cut.Find(".language-switcher").ClassList.ShouldContain("custom-switcher");
        cut.Find(".language-switcher-list").GetAttribute("aria-label").ShouldBe("Choose a language");
        cut.Find(".language-switcher-toggle").TextContent.ShouldContain("Choose a language");
        cut.FindAll("a").Select(a => a.GetAttribute("href")).ShouldBe(["ko-kr/about/", "ja-jp/about/", "about/"]);
        cut.FindAll("a").First().TextContent.ShouldBe("KO <b>한국어</b>");
        cut.FindAll("b").ShouldBeEmpty();
        urls.Keys.ShouldBe(["en-us", "ko-kr", "ja-jp"]);
    }

    [Fact]
    public void Given_DefaultLayout_When_Rendered_Then_It_Should_PlaceTheSwitcherInsideTheTopNavigation()
    {
        using var context = new BunitContext();
        context.Services.AddSingleton(Substitute.For<IThemeService>());

        var cut = context.Render<MainLayout>(parameters => parameters
            .Add(p => p.Site, new SiteManifest { Locales = ["en-us", "ko-kr"] })
            .Add(p => p.Theme, new ThemeManifest())
            .Add(p => p.LocaleContext, new LocaleContext
            {
                Locale = "en-us",
                SwitchLanguageUrls = new Dictionary<string, string> { ["en-us"] = ".", ["ko-kr"] = "ko-kr/" },
            }));

        cut.Find(".site-header nav .language-switcher").ShouldNotBeNull();
        cut.FindAll(".site-header + .language-switcher").ShouldBeEmpty();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Given_NoAlternativeLanguages_When_Rendered_Then_It_Should_OmitTheSwitcher(bool supplyPrimary)
    {
        using var context = new BunitContext();
        var cut = context.Render<LanguageSwitcher>(parameters =>
        {
            if (supplyPrimary)
            {
                parameters.AddCascadingValue(new LocaleContext
                {
                    Locale = "en-us",
                    SwitchLanguageUrls = new Dictionary<string, string> { ["en-us"] = "." },
                });
            }
        });
        cut.Markup.ShouldBeEmpty();
    }

    [Fact]
    public void Given_BlankOverride_When_Rendered_Then_It_Should_RejectAnUnlabelledLink()
    {
        using var context = new BunitContext();
        Should.Throw<InvalidOperationException>(() => context.Render<LanguageSwitcher>(parameters => parameters
            .AddCascadingValue(new LocaleContext { SwitchLanguageUrls = new Dictionary<string, string> { ["en-us"] = "." } })
            .Add(p => p.Labels, new Dictionary<string, string> { ["en-us"] = " " })));
    }

    [Fact]
    public void Given_BlankNavigationLabel_When_Rendered_Then_It_Should_RejectAnUnlabelledSwitcher()
    {
        using var context = new BunitContext();
        Should.Throw<InvalidOperationException>(() => context.Render<LanguageSwitcher>(parameters => parameters
            .AddCascadingValue(new LocaleContext
            {
                SwitchLanguageUrls = new Dictionary<string, string> { ["en-us"] = ".", ["ko-kr"] = "ko-kr/" },
            })
            .Add(p => p.AriaLabel, " ")));
    }
}
