using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;

using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;
using ScissorHands.Core.Services;

namespace ScissorHands.Theme.Tests;

public class MainLayoutBaseTests
{
    [Fact]
    public async Task Given_SiteAndThemeService_When_OnInitializedAsync_Invoked_Then_It_Should_SetPageTitleDescription_And_LoadThemeManifest()
    {
        // Arrange
        using var context = new BunitContext();

        var expectedTheme = new ThemeManifest { Name = "Expected", Slug = "minimal" };

        var themeService = Substitute.For<IThemeService>();
        themeService
            .LoadManifestAsync("minimal")
            .Returns(Task.FromResult(expectedTheme));

        context.Services.AddSingleton(themeService);

        var site = new SiteManifest
        {
            Title = "My Site",
            Description = "My Description",
            Theme = "minimal",
        };

        var oldTheme = new ThemeManifest { Name = "Old", Slug = "old" };

        // Act
        var cut = context.Renderer.Render<TestMainLayout>(parameters => parameters
            .Add(p => p.Site, site)
            .Add(p => p.Theme, oldTheme));

        // Assert
        cut.WaitForAssertion(() =>
        {
            themeService.Received(1).LoadManifestAsync("minimal");
            cut.Instance.ExposedPageTitle.ShouldBe("My Site");
            cut.Instance.ExposedPageDescription.ShouldBe("My Description");
            cut.Instance.Theme.ShouldBeSameAs(expectedTheme);
        });

        await Task.CompletedTask;
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
        themeService
            .LoadManifestAsync(Arg.Any<string>())
            .Returns(Task.FromResult(new ThemeManifest()));

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
        themeService
            .LoadManifestAsync(Arg.Any<string>())
            .Returns(Task.FromResult(new ThemeManifest()));

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
        themeService
            .LoadManifestAsync(Arg.Any<string>())
            .Returns(Task.FromResult(new ThemeManifest()));

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

    public string InvokeCalculatePageTitle() => CalculatePageTitle();

    public string InvokeCalculatePageDescription() => CalculatePageDescription();

    public Task InvokeOnInitializedAsync() => base.OnInitializedAsync();

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        // Intentionally empty: these tests focus on base behavior, not markup.
    }
}

