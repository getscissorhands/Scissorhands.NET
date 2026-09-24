using System.Reflection;

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;
using ScissorHands.Core.Services;

namespace ScissorHands.Theme.Tests;

public class LocaleContextTests
{
    [Theory]
    [InlineData(typeof(MainLayoutBase), typeof(ParameterAttribute))]
    [InlineData(typeof(CascadingMainLayoutBase), typeof(ParameterAttribute))]
    [InlineData(typeof(IndexViewBase), typeof(CascadingParameterAttribute))]
    [InlineData(typeof(PostViewBase), typeof(CascadingParameterAttribute))]
    [InlineData(typeof(PageViewBase), typeof(CascadingParameterAttribute))]
    [InlineData(typeof(NotFoundViewBase), typeof(CascadingParameterAttribute))]
    [InlineData(typeof(TagListViewBase), typeof(CascadingParameterAttribute))]
    [InlineData(typeof(TagViewBase), typeof(CascadingParameterAttribute))]
    public void Given_ComponentContract_When_GetProperty_Invoked_Then_It_Should_ExposeOptionalTypedLocaleContext(Type type, Type attributeType)
    {
        // Arrange

        // Act
        var property = type.GetProperty(nameof(LocaleContext));

        // Assert
        property.ShouldNotBeNull();
        property.PropertyType.ShouldBe(typeof(LocaleContext));
        new NullabilityInfoContext().Create(property).ReadState.ShouldBe(NullabilityState.Nullable);
        property.GetCustomAttribute(attributeType).ShouldNotBeNull();
        property.GetCustomAttribute<CascadingParameterAttribute>()?.Name.ShouldBeNull();
        if (type != typeof(MainLayoutBase))
        {
            type.GetProperty(nameof(MainLayoutBase.NavigationPages)).ShouldBeNull();
            type.GetProperty(nameof(MainLayoutBase.NavigationTree)).ShouldBeNull();
        }
    }

    [Fact]
    public void Given_ComponentsWithoutContext_When_Construct_Invoked_Then_It_Should_KeepLocaleContextOptional()
    {
        // Arrange

        // Act
        LocaleContext?[] contexts =
        [
            new LayoutProbe().LocaleContext,
            new CascadingMainLayoutBase().LocaleContext,
            new IndexProbe().LocaleContext,
            new PostProbe().LocaleContext,
            new PageProbe().LocaleContext,
            new NotFoundProbe().LocaleContext,
            new TagListProbe().LocaleContext,
            new TagProbe().LocaleContext,
        ];

        // Assert
        contexts.ShouldAllBe(context => context == null);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Given_OptionalContext_When_Render_Invoked_Then_It_Should_CascadeToEveryViewWithoutLeakingNavigation(bool supplyContext)
    {
        // Arrange
        using var context = new BunitContext();
        var locale = supplyContext ? new LocaleContext { Locale = "ko-kr", Route = "ko-kr/about", HomeUrl = "ko-kr/" } : null;
        var document = new ContentDocument { Metadata = new ContentMetadata { Title = "Current" } };
        var documents = new[] { document };
        var site = new SiteManifest { Locale = "en-US" };
        var navigation = new PageNavigation { Next = new PageNavigationLink { Title = "Next", Url = "ko-kr/next" } };

        // Act
        var cut = context.Render<CascadingMainLayoutBase>(parameters => parameters
            .Add(p => p.LocaleContext, locale)
            .Add(p => p.Document, document)
            .Add(p => p.Documents, documents)
            .Add(p => p.Site, site)
            .Add(p => p.PageNavigation, navigation)
            .AddChildContent<IndexProbe>()
            .AddChildContent<PostProbe>()
            .AddChildContent<PageProbe>()
            .AddChildContent<NotFoundProbe>()
            .AddChildContent<TagListProbe>()
            .AddChildContent<TagProbe>());

        // Assert
        cut.FindComponent<CascadingValue<LocaleContext>>().Instance.Value.ShouldBeSameAs(locale);
        cut.FindComponent<IndexProbe>().Instance.LocaleContext.ShouldBeSameAs(locale);
        cut.FindComponent<PostProbe>().Instance.LocaleContext.ShouldBeSameAs(locale);
        cut.FindComponent<NotFoundProbe>().Instance.LocaleContext.ShouldBeSameAs(locale);
        cut.FindComponent<TagListProbe>().Instance.LocaleContext.ShouldBeSameAs(locale);
        cut.FindComponent<TagProbe>().Instance.LocaleContext.ShouldBeSameAs(locale);
        var page = cut.FindComponent<PageProbe>().Instance;
        page.LocaleContext.ShouldBeSameAs(locale);
        page.Document.ShouldBeSameAs(document);
        page.Site.ShouldBeSameAs(site);
        page.PageNavigation.ShouldBeSameAs(navigation);
        page.NavigationPages.ShouldBeNull();
        page.NavigationTree.ShouldBeNull();
        site.Locale.ShouldBe("en-US");
        cut.FindComponent<IndexProbe>().Instance.Documents.ShouldBeSameAs(documents);
    }

    [Fact]
    public void Given_OuterLocaleContext_When_Render_Invoked_Then_It_Should_IsolateAndUpdateTheInnerTypedCascade()
    {
        // Arrange
        using var context = new BunitContext();
        var outer = new LocaleContext { Locale = "en-us", HomeUrl = "en-us/" };
        var inner = new LocaleContext { Locale = "ko-kr", HomeUrl = "ko-kr/" };
        var cut = context.Render<CascadingMainLayoutBase>(parameters => parameters
            .AddCascadingValue(outer)
            .AddChildContent<PageProbe>());

        // Act
        var omittedValue = cut.FindComponent<PageProbe>().Instance.LocaleContext;
        cut.Render(parameters => parameters.Add(p => p.LocaleContext, inner));
        var suppliedValue = cut.FindComponent<PageProbe>().Instance.LocaleContext;
        cut.Render(parameters => parameters.Add(p => p.LocaleContext, null));

        // Assert
        omittedValue.ShouldBeNull();
        suppliedValue.ShouldBeSameAs(inner);
        cut.FindComponent<PageProbe>().Instance.LocaleContext.ShouldBeNull();
        outer.Locale.ShouldBe("en-us");
    }

    [Theory]
    [InlineData(null, "ko-KR", "en-US", "ko-kr")]
    [InlineData(null, " ", "en-US", "en-us")]
    [InlineData(null, null, "en-US", "en-us")]
    [InlineData(null, null, null, "")]
    [InlineData(null, "ko-KR", null, "")]
    [InlineData(null, "ko-KR", "", "")]
    [InlineData(null, null, "   ", "")]
    [InlineData("fr-ca", "ko-KR", null, "")]
    [InlineData("fr-ca", "ko-KR", "   ", "")]
    [InlineData("fr-ca", "ko-KR", "en-US", "fr-ca")]
    [InlineData("", "ko-KR", "en-US", "")]
    public void Given_LocaleSources_When_CalculatePageLocale_Invoked_Then_It_Should_RequireConfiguredSiteLocale(
        string? locale, string? documentLocale, string? siteLocale, string expected)
    {
        // Arrange
        using var context = new BunitContext();
        context.Services.AddSingleton(Substitute.For<IThemeService>());
        var site = siteLocale is null ? null : new SiteManifest { Locale = siteLocale };
        var document = documentLocale is null ? null : new ContentDocument { Metadata = new ContentMetadata { Locale = documentLocale } };

        // Act
        var cut = context.Render<LayoutProbe>(parameters => parameters
            .Add(p => p.Site, site)
            .Add(p => p.Document, document)
            .Add(p => p.LocaleContext, locale is null ? null : new LocaleContext { Locale = locale }));

        // Assert
        cut.Instance.CalculatedLocale.ShouldBe(expected);
        site?.Locale.ShouldBe(siteLocale);
        document?.Metadata.Locale.ShouldBe(documentLocale);
    }

    [Theory]
    [InlineData(false, ".", "tags")]
    [InlineData(true, "ko-kr/", null)]
    [InlineData(true, "fr%20ca/", "fr%20ca/tags")]
    [InlineData(true, ".", "")]
    public void Given_OptionalLocaleContext_When_LayoutUrlHelpers_Invoked_Then_It_Should_PreservePreparedTargetsAndLegacyDefaults(
        bool supplyContext, string homeUrl, string? tagIndexUrl)
    {
        // Arrange
        using var context = new BunitContext();
        context.Services.AddSingleton(Substitute.For<IThemeService>());
        var cut = context.Render<LayoutProbe>(parameters => parameters
            .Add(p => p.LocaleContext, supplyContext ? new LocaleContext { HomeUrl = homeUrl, TagIndexUrl = tagIndexUrl } : null));

        // Act
        var home = cut.Instance.HomeUrl();
        var tags = cut.Instance.TagIndexUrl();

        // Assert
        home.ShouldBe(homeUrl);
        tags.ShouldBe(tagIndexUrl);
    }

    [Theory]
    [InlineData(null, " C# ", "tags/c%23")]
    [InlineData(null, " Topic/Subtopic ", "tags/topic%2Fsubtopic")]
    [InlineData(".", " C# ", "tags/c%23")]
    [InlineData("ko-kr/", " C# ", "ko-kr/tags/c%23")]
    [InlineData("fr%20ca/", " Topic/Subtopic ", "fr%20ca/tags/topic%2Fsubtopic")]
    public void Given_OptionalLocaleAndRawTag_When_GetTagUrl_Invoked_Then_It_Should_UseContextOrSharedFormatting(
        string? homeUrl, string tag, string expected)
    {
        // Arrange
        using var context = new BunitContext();
        var cut = context.Render<CascadingMainLayoutBase>(parameters => parameters
            .Add(p => p.LocaleContext, homeUrl is null ? null : new LocaleContext { HomeUrl = homeUrl })
            .AddChildContent<PostProbe>()
            .AddChildContent<PageProbe>()
            .AddChildContent<TagListProbe>());

        // Act
        var urls = new[]
        {
            cut.FindComponent<PostProbe>().Instance.TagUrl(tag),
            cut.FindComponent<PageProbe>().Instance.TagUrl(tag),
            cut.FindComponent<TagListProbe>().Instance.TagUrl(tag),
        };

        // Assert
        urls.ShouldAllBe(url => url == expected);
    }

    [Theory]
    [InlineData(false, null)]
    [InlineData(true, null)]
    [InlineData(false, "")]
    [InlineData(true, "")]
    [InlineData(false, " ")]
    [InlineData(true, " ")]
    [InlineData(false, " . ")]
    [InlineData(true, " . ")]
    [InlineData(false, "..")]
    [InlineData(true, "..")]
    public void Given_InvalidTagAndOptionalLocale_When_GetTagUrl_Invoked_Then_It_Should_PreserveHelperExceptions(bool supplyContext, string? tag)
    {
        // Arrange
        using var context = new BunitContext();
        var cut = context.Render<CascadingMainLayoutBase>(parameters => parameters
            .Add(p => p.LocaleContext, supplyContext ? new LocaleContext { HomeUrl = "ko-kr/" } : null)
            .AddChildContent<PostProbe>()
            .AddChildContent<PageProbe>()
            .AddChildContent<TagListProbe>());

        // Act
        var exceptions = new[]
        {
            Record.Exception(() => cut.FindComponent<PostProbe>().Instance.TagUrl(tag!)),
            Record.Exception(() => cut.FindComponent<PageProbe>().Instance.TagUrl(tag!)),
            Record.Exception(() => cut.FindComponent<TagListProbe>().Instance.TagUrl(tag!)),
        };

        // Assert
        foreach (var exception in exceptions)
        {
            exception.ShouldBeOfType(tag is null ? typeof(ArgumentNullException) : typeof(ArgumentException));
            ((ArgumentException)exception).ParamName.ShouldBe("tag");
        }
    }

    public sealed class LayoutProbe : MainLayoutBase
    {
        public string? CalculatedLocale => PageLocale;
        public string HomeUrl() => GetHomeUrl();
        public string? TagIndexUrl() => GetTagIndexUrl();
    }

    public sealed class IndexProbe : IndexViewBase;

    public sealed class PostProbe : PostViewBase
    {
        public string TagUrl(string tag) => GetTagUrl(tag);
    }

    public sealed class PageProbe : PageViewBase
    {
        [CascadingParameter]
        public IReadOnlyList<ContentDocument>? NavigationPages { get; set; }

        [CascadingParameter]
        public IReadOnlyList<NavigationNode>? NavigationTree { get; set; }

        public string TagUrl(string tag) => GetTagUrl(tag);
    }

    public sealed class NotFoundProbe : NotFoundViewBase;

    public sealed class TagListProbe : TagListViewBase
    {
        public string TagUrl(string tag) => GetTagUrl(tag);
    }

    public sealed class TagProbe : TagViewBase;
}
