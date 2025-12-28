using ScissorHands.Core.Manifests;

namespace ScissorHands.Core.Tests.Manifests;

public class SiteManifestTests
{
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
