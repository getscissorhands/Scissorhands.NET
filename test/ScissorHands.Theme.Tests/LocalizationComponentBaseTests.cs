using System.Reflection;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

using ScissorHands.Core.Models;
using ScissorHands.Theme.Components;

namespace ScissorHands.Theme.Tests;

public class LocalizationComponentBaseTests
{
    [Theory]
    [InlineData(typeof(LanguageSwitcherBase))]
    [InlineData(typeof(LocalizationMetadataBase))]
    [InlineData(typeof(LocalizationFallbackBannerBase))]
    public void Given_LocalizationBase_When_Inspected_Then_It_Should_Not_PrescribeMarkup(Type type)
    {
        type.IsAbstract.ShouldBeTrue();
        type.Namespace.ShouldBe("ScissorHands.Theme.Components");
        type.GetMethod("BuildRenderTree", BindingFlags.Instance | BindingFlags.NonPublic)!
            .DeclaringType.ShouldBe(typeof(ComponentBase));
        type.GetProperty("LocaleContext")!.GetCustomAttribute<CascadingParameterAttribute>().ShouldNotBeNull();
    }

    [Fact]
    public void Given_CustomSwitcher_When_Rendered_Then_It_Should_ReusePreparedLinksWithItsOwnLabelsAndMarkup()
    {
        using var context = new BunitContext();
        var cut = context.Render<CustomSwitcher>(parameters => parameters.AddCascadingValue(new LocaleContext
        {
            Locale = "ko-kr",
            ContentLocale = "en-us",
            SwitchLanguageUrls = new Dictionary<string, string>
            {
                ["en-us"] = "about/",
                ["ko-kr"] = "ko-kr/about/",
            },
        }));

        cut.FindAll("nav, ul").ShouldBeEmpty();
        cut.FindAll(".custom-choice").Select(link => link.TextContent).ShouldBe(["EN-US", "KO-KR"]);
        cut.FindAll(".custom-choice").Select(link => link.GetAttribute("href")).ShouldBe(["about/", "ko-kr/about/"]);
        cut.Find("[aria-current='true']").TextContent.ShouldBe("KO-KR");
    }

    [Fact]
    public void Given_MetadataContext_When_Rendered_Then_It_Should_ExposePreparedSeoWithoutEmittingMarkup()
    {
        using var context = new BunitContext();
        var urls = new Dictionary<string, string> { ["en-us"] = "https://example.test/about/" };
        var cut = context.Render<MetadataProbe>(parameters => parameters.AddCascadingValue(new LocaleContext
        {
            CanonicalUrl = "https://example.test/about/",
            AlternateLanguageUrls = urls,
        }));

        cut.Markup.ShouldBeEmpty();
        cut.Instance.PreparedCanonical.ShouldBe("https://example.test/about/");
        cut.Instance.PreparedAlternatives.ShouldBeSameAs(urls);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Given_CustomBanner_When_Rendered_Then_It_Should_ProvideEncodedTextAndAttributesWithoutFixingItsWrapper(bool fallback)
    {
        using var context = new BunitContext();
        var receipt = new LocalizationFallbackBannerBase.RenderReceipt();
        var cut = context.Render<CustomBanner>(parameters => parameters
            .AddCascadingValue(receipt)
            .AddCascadingValue(new LocaleContext
            {
                Locale = "ko-kr",
                IsFallback = fallback,
                FallbackMessage = "<script>not markup</script> & text",
            }));

        receipt.Rendered.ShouldBe(fallback);
        if (!fallback)
        {
            cut.Markup.ShouldBeEmpty();
            return;
        }
        var message = cut.Find("section[data-localization-fallback]");
        message.GetAttribute("lang").ShouldBe("ko-kr");
        message.TextContent.ShouldBe("<script>not markup</script> & text");
        message.Children.ShouldHaveSingleItem().LocalName.ShouldBe("strong");
        cut.FindAll("aside, script").ShouldBeEmpty();
    }

    [Fact]
    public void Given_InheritanceWithoutMessageRendering_When_Rendered_Then_It_Should_NotClaimBannerDelivery()
    {
        using var context = new BunitContext();
        var receipt = new LocalizationFallbackBannerBase.RenderReceipt();
        var cut = context.Render<EmptyBanner>(parameters => parameters
            .AddCascadingValue(receipt)
            .AddCascadingValue(new LocaleContext { Locale = "ko-kr", IsFallback = true, FallbackMessage = "Unavailable" }));

        cut.Markup.ShouldBeEmpty();
        receipt.Rendered.ShouldBeFalse();
    }

    public sealed class CustomSwitcher : LanguageSwitcherBase
    {
        protected override string GetNativeLabel(string locale, IEnumerable<string> locales) => locale.ToUpperInvariant();

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            foreach (var link in Links)
            {
                builder.OpenElement(0, "a");
                builder.AddAttribute(1, "class", "custom-choice");
                builder.AddAttribute(2, "href", link.Url);
                builder.AddAttribute(3, "aria-current", link.IsCurrent ? "true" : null);
                builder.AddContent(4, link.Label);
                builder.CloseElement();
            }
        }
    }

    public sealed class MetadataProbe : LocalizationMetadataBase
    {
        public string? PreparedCanonical => CanonicalUrl;
        public IReadOnlyDictionary<string, string> PreparedAlternatives => AlternateLanguageUrls;
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
            builder.OpenElement(2, "strong");
            builder.AddContent(3, FallbackMessageContent);
            builder.CloseElement();
            builder.CloseElement();
        }
    }

    public sealed class EmptyBanner : LocalizationFallbackBannerBase;
}
