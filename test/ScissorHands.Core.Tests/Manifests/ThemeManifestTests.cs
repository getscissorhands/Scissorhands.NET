using ScissorHands.Core.Manifests;

namespace ScissorHands.Core.Tests.Manifests;

public class ThemeManifestTests
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

    [Fact]
    public void Given_ThemeCollections_When_SourceListsMutate_Then_ManifestShouldRetainSnapshots()
    {
        var stylesheets = new List<string> { "/theme.css" };
        var scripts = new List<string> { "/theme.js" };
        var manifest = new ThemeManifest
        {
            Stylesheets = stylesheets,
            Scripts = scripts,
        };

        stylesheets.Add("/later.css");
        scripts.Clear();

        manifest.Stylesheets.ShouldBe(["/theme.css"]);
        manifest.Scripts.ShouldBe(["/theme.js"]);
    }
}
