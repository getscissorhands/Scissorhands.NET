using ScissorHands.Core.Manifests;

namespace ScissorHands.Core.Tests.Manifests;

public class SiteManifestTests
{
    [Theory]
    [InlineData("/", "/")]
    [InlineData("/docs", "/docs/")]
    [InlineData("/docs/", "/docs/")]
    [InlineData("/manual/docs", "/manual/docs/")]
    [InlineData("/manual/docs/", "/manual/docs/")]
    public void Given_BasePath_When_ManifestInitialized_Then_It_Should_NormalizeTrailingSlash(string baseUrl, string expected)
    {
        var manifest = new SiteManifest { BaseUrl = baseUrl };

        manifest.BaseUrl.ShouldBe(expected);
        new SiteManifest { BaseUrl = manifest.BaseUrl }.BaseUrl.ShouldBe(expected);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("docs")]
    [InlineData("docs/")]
    [InlineData("https://example.test/docs")]
    [InlineData("//example.test/docs")]
    [InlineData("/docs?lang=en")]
    [InlineData("/docs#intro")]
    [InlineData("/docs/?lang=en")]
    [InlineData("/docs\\child")]
    [InlineData(" /docs")]
    [InlineData("/docs//")]
    public void Given_OtherBaseUrlForm_When_ManifestInitialized_Then_It_Should_PreserveTheValue(string? baseUrl)
    {
        var manifest = new SiteManifest { BaseUrl = baseUrl! };

        manifest.BaseUrl.ShouldBe(baseUrl);
    }

    [Fact]
    public void Given_DefaultSiteManifest_When_Constructed_Then_It_Should_HaveReasonableDefaults()
    {
        // Arrange

        // Act
        var manifest = new SiteManifest();

        // Assert
        manifest.ShouldNotBeNull();

        manifest.Generator.ShouldNotBeNullOrWhiteSpace();
        manifest.Title.ShouldNotBeNullOrWhiteSpace();
        manifest.Description.ShouldNotBeNullOrWhiteSpace();
        manifest.Locale.ShouldNotBeNullOrWhiteSpace();
        manifest.Author.ShouldNotBeNullOrWhiteSpace();

        manifest.BaseUrl.ShouldNotBeNullOrWhiteSpace();
        manifest.BaseUrl.StartsWith('/').ShouldBeTrue();

        manifest.SiteUrl.ShouldNotBeNullOrWhiteSpace();
        Uri.TryCreate(manifest.SiteUrl, UriKind.Absolute, out _).ShouldBeTrue();

        manifest.HeroImage.ShouldNotBeNullOrWhiteSpace();
        Uri.TryCreate(manifest.HeroImage, UriKind.Absolute, out _).ShouldBeTrue();
    }

    [Fact]
    public void Given_SiteManifestDirectoryConstants_When_Referenced_Then_It_Should_BeNonEmpty()
    {
        // Arrange

        // Act
        var contents = SiteManifest.CONTENTS_DIRECTORY;
        var build = SiteManifest.BUILD_OUTPUT_DIRECTORY;
        var preview = SiteManifest.PREVIEW_OUTPUT_DIRECTORY;

        // Assert
        contents.ShouldNotBeNullOrWhiteSpace();
        build.ShouldNotBeNullOrWhiteSpace();
        preview.ShouldNotBeNullOrWhiteSpace();
    }
}
