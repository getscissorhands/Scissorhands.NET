using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;

using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;
using ScissorHands.Core.Services;

namespace ScissorHands.Theme.Tests;

public class MainLayoutBaseTests
{
    [Theory]
    [InlineData("minimal", "assets/theme.css", "themes/minimal/assets/theme.css")]
    [InlineData("minimal", "/assets/theme.css", "themes/minimal/assets/theme.css")]
    [InlineData("/minimal/", "assets/theme.css", "themes/minimal/assets/theme.css")]
    [InlineData("///minimal///", "///assets/theme.css", "themes/minimal/assets/theme.css")]
    [InlineData("minimal", "", "themes/minimal/")]
    [InlineData("minimal", "/", "themes/minimal/")]
    [InlineData("minimal", "/assets/theme.css?v=1#theme", "themes/minimal/assets/theme.css?v=1#theme")]
    public void Given_ThemeAndPath_When_GetThemeUrl_Invoked_Then_It_Should_ReturnBaseRelativeUrl(string slug, string path, string expected)
    {
        // Arrange
        using var context = new BunitContext();
        context.Services.AddSingleton(Substitute.For<IThemeService>());

        var cut = context.Renderer.Render<TestMainLayout>(parameters => parameters
            .Add(p => p.Theme, new ThemeManifest { Slug = slug }));

        // Act
        var result = cut.Instance.InvokeGetThemeUrl(path);

        // Assert
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("/", "https://example.com/themes/minimal/assets/theme.css")]
    [InlineData("/docs/", "https://example.com/docs/themes/minimal/assets/theme.css")]
    public void Given_SiteBaseUrl_When_GetThemeUrl_Invoked_Then_It_Should_ResolveUnderSiteBaseUrl(string baseUrl, string expected)
    {
        // Arrange
        using var context = new BunitContext();
        context.Services.AddSingleton(Substitute.For<IThemeService>());

        var site = new SiteManifest { BaseUrl = baseUrl };
        var cut = context.Renderer.Render<TestMainLayout>(parameters => parameters
            .Add(p => p.Site, site)
            .Add(p => p.Theme, new ThemeManifest { Slug = "minimal" }));

        // Act
        var result = cut.Instance.InvokeGetThemeUrl("/assets/theme.css");
        var resolved = new Uri(new Uri($"https://example.com{site.BaseUrl}"), result);

        // Assert
        result.ShouldBe("themes/minimal/assets/theme.css");
        resolved.AbsoluteUri.ShouldBe(expected);
    }

    [Fact]
    public void Given_NullPath_When_GetThemeUrl_Invoked_Then_It_Should_ThrowArgumentNullException()
    {
        // Arrange
        using var context = new BunitContext();
        context.Services.AddSingleton(Substitute.For<IThemeService>());

        var cut = context.Renderer.Render<TestMainLayout>(parameters => parameters
            .Add(p => p.Theme, new ThemeManifest { Slug = "minimal" }));

        // Act
        var exception = Should.Throw<ArgumentNullException>(() => cut.Instance.InvokeGetThemeUrl(null!));

        // Assert
        exception.ParamName.ShouldBe("path");
    }

    [Fact]
    public void Given_MissingTheme_When_GetThemeUrl_Invoked_Then_It_Should_ThrowInvalidOperationException()
    {
        // Arrange
        var component = new TestMainLayout();

        // Act
        var exception = Should.Throw<InvalidOperationException>(() => component.InvokeGetThemeUrl("assets/theme.css"));

        // Assert
        exception.Message.ShouldBe("A theme must be supplied before getting a theme URL.");
    }

    [Fact]
    public void Given_SiteAndTheme_When_Rendered_Then_It_Should_SetPageMetadata_And_UseProvidedTheme()
    {
        // Arrange
        using var context = new BunitContext();

        var expectedTheme = new ThemeManifest { Name = "Expected", Slug = "minimal" };

        var themeService = Substitute.For<IThemeService>();
        context.Services.AddSingleton(themeService);

        var site = new SiteManifest
        {
            Title = "My Site",
            Description = "My Description",
            Theme = "minimal",
        };

        // Act
        var cut = context.Renderer.Render<TestMainLayout>(parameters => parameters
            .Add(p => p.Site, site)
            .Add(p => p.Theme, expectedTheme));

        // Assert
        cut.Instance.ExposedPageTitle.ShouldBe("My Site");
        cut.Instance.ExposedPageDescription.ShouldBe("My Description");
        cut.Instance.ExposedPageLocale.ShouldBe("en-us");
        cut.Instance.Theme.ShouldBeSameAs(expectedTheme);
        themeService.DidNotReceiveWithAnyArgs().LoadManifestAsync(default!, Xunit.TestContext.Current.CancellationToken);
    }

    [Fact]
    public void Given_UpdatedDocument_When_ComponentRerendered_Then_It_Should_RecalculatePageMetadata()
    {
        using var context = new BunitContext();
        context.Services.AddSingleton(Substitute.For<IThemeService>());

        var site = new SiteManifest { Title = "My Site", Description = "Site Description" };
        var first = new ContentDocument
        {
            Metadata = new ContentMetadata { Title = "First", Description = "First Description", Locale = "en-US" }
        };
        var second = new ContentDocument
        {
            Metadata = new ContentMetadata { Title = "Second", Description = "Second Description", Locale = "ko-KR" }
        };

        var cut = context.Renderer.Render<TestMainLayout>(parameters => parameters
            .Add(p => p.Site, site)
            .Add(p => p.Document, first));

        cut.Render(parameters => parameters
            .Add(p => p.Site, site)
            .Add(p => p.Document, second));

        cut.Instance.ExposedPageTitle.ShouldBe("Second | My Site");
        cut.Instance.ExposedPageDescription.ShouldBe("Second Description");
        cut.Instance.ExposedPageLocale.ShouldBe("ko-kr");
    }

    [Theory]
    [InlineData("Post Title", "My Site", "Post Title | My Site")]
    [InlineData(" ", "My Site", "My Site")]
    [InlineData("", "My Site", "My Site")]
    public void Given_DocumentTitle_When_CalculatePageTitle_Invoked_Then_It_Should_ReturnExpectedTitle(string documentTitle, string siteTitle, string expected)
    {
        // Arrange
        using var context = new BunitContext();

        var themeService = Substitute.For<IThemeService>();
        context.Services.AddSingleton(themeService);

        var site = new SiteManifest { Title = siteTitle, Theme = "minimal" };

        var document = new ContentDocument
        {
            Metadata = new ContentMetadata { Title = documentTitle }
        };

        var cut = context.Renderer.Render<TestMainLayout>(parameters => parameters
            .Add(p => p.Site, site)
            .Add(p => p.Document, document));

        // Act
        var result = cut.Instance.InvokeCalculatePageTitle();

        // Assert
        result.ShouldBe(expected);
    }

    [Fact]
    public void Given_NullDocument_When_CalculatePageTitle_Invoked_Then_It_Should_ReturnSiteTitle()
    {
        // Arrange
        using var context = new BunitContext();

        var themeService = Substitute.For<IThemeService>();
        context.Services.AddSingleton(themeService);

        var site = new SiteManifest { Title = "My Site", Theme = "minimal" };

        var cut = context.Renderer.Render<TestMainLayout>(parameters => parameters
            .Add(p => p.Site, site));

        // Act
        var result = cut.Instance.InvokeCalculatePageTitle();

        // Assert
        result.ShouldBe("My Site");
    }

    [Theory]
    [InlineData("Doc Description", "Site Description", "Doc Description")]
    [InlineData(" ", "Site Description", "Site Description")]
    [InlineData("", "Site Description", "Site Description")]
    public void Given_DocumentDescription_When_CalculatePageDescription_Invoked_Then_It_Should_ReturnExpectedDescription(string documentDescription, string siteDescription, string expected)
    {
        // Arrange
        using var context = new BunitContext();

        var themeService = Substitute.For<IThemeService>();
        context.Services.AddSingleton(themeService);

        var site = new SiteManifest { Description = siteDescription, Theme = "minimal" };

        var document = new ContentDocument
        {
            Metadata = new ContentMetadata { Description = documentDescription }
        };

        var cut = context.Renderer.Render<TestMainLayout>(parameters => parameters
            .Add(p => p.Site, site)
            .Add(p => p.Document, document));

        // Act
        var result = cut.Instance.InvokeCalculatePageDescription();

        // Assert
        result.ShouldBe(expected);
    }

    [Fact]
    public void Given_NullSite_When_CalculatePageTitleAndDescription_Invoked_Then_It_Should_ReturnEmptyStrings()
    {
        // Arrange
        var component = new TestMainLayout();

        // Act
        var title = component.InvokeCalculatePageTitle();
        var description = component.InvokeCalculatePageDescription();

        // Assert
        title.ShouldBe(string.Empty);
        description.ShouldBe(string.Empty);
    }
}

internal class TestMainLayout : MainLayoutBase
{
    public string? ExposedPageTitle => PageTitle;
    public string? ExposedPageDescription => PageDescription;
    public string? ExposedPageLocale => PageLocale;

    public string InvokeGetThemeUrl(string path) => GetThemeUrl(path);

    public string InvokeCalculatePageTitle() => CalculatePageTitle();

    public string InvokeCalculatePageDescription() => CalculatePageDescription();

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        // Intentionally empty: these tests focus on base behavior, not markup.
    }
}
