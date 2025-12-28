using ScissorHands.Core.Manifests;

namespace ScissorHands.Core.Tests.Manifests;

public sealed class ThemeManifestTests
{
    [Fact]
    public void Given_DefaultThemeManifest_When_Constructed_Then_It_Should_HaveExpectedDefaults()
    {
        // Arrange

        // Act
        var manifest = new ThemeManifest();

        // Assert
        manifest.ShouldNotBeNull();

        manifest.Name.ShouldNotBeNull();
        manifest.Slug.ShouldNotBeNull();
        manifest.Version.ShouldBe("1.0.0");

        manifest.Stylesheets.ShouldNotBeNull();
        manifest.Stylesheets.ShouldBeEmpty();

        manifest.Scripts.ShouldNotBeNull();
        manifest.Scripts.ShouldBeEmpty();
    }

    [Fact]
    public void Given_ThemeDirectoryConstant_When_Referenced_Then_It_Should_BeThemes()
    {
        // Arrange

        // Act
        var dir = ThemeManifest.THEME_DIRECTORY;

        // Assert
        dir.ShouldBe("themes");
    }
}
