using Microsoft.Extensions.Logging;

using ScissorHands.Core.Manifests;
using ScissorHands.Web.Services;
using ScissorHands.Web.Tests.TestDoubles;

using System.IO.Abstractions.TestingHelpers;

namespace ScissorHands.Web.Tests.Services;

public class ThemeServiceTests
{
    [Fact]
    public async Task Given_MissingThemeManifest_When_LoadManifestAsync_Invoked_Then_It_Should_ReturnDefaultThemeManifest()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var root = fileSystem.Path.GetPathRoot(Environment.CurrentDirectory) ?? fileSystem.Path.DirectorySeparatorChar.ToString();
        var baseRoot = fileSystem.Path.Combine(root, "base");
        var contentsRoot = fileSystem.Path.Combine(baseRoot, "contents");
        var themesRoot = fileSystem.Path.Combine(baseRoot, "themes");

        var site = new SiteManifest();
        var paths = new TestAppPaths(basePath: baseRoot, contentsRoot, themesRoot);
        var logger = Substitute.For<ILogger<ThemeService>>();

        var service = new ThemeService(paths, fileSystem, site, logger);

        // Act
        var manifest = await service.LoadManifestAsync("missing");

        // Assert
        manifest.ShouldNotBeNull();
        manifest.Name.ShouldBe("Default");
        manifest.Slug.ShouldBe("default");
    }

    [Fact]
    public async Task Given_InvalidThemeManifestJson_When_LoadManifestAsync_Invoked_Then_It_Should_ReturnDefaultThemeManifest()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var root = fileSystem.Path.GetPathRoot(Environment.CurrentDirectory) ?? fileSystem.Path.DirectorySeparatorChar.ToString();
        var baseRoot = fileSystem.Path.Combine(root, "base");
        var contentsRoot = fileSystem.Path.Combine(baseRoot, "contents");
        var themesRoot = fileSystem.Path.Combine(baseRoot, "themes");

        var themeRoot = fileSystem.Path.Combine(themesRoot, "bad");
        fileSystem.AddDirectory(themeRoot);
        fileSystem.AddFile(fileSystem.Path.Combine(themeRoot, "theme.json"), new MockFileData("{ not-json"));

        var site = new SiteManifest();
        var paths = new TestAppPaths(basePath: baseRoot, contentsRoot, themesRoot);
        var logger = Substitute.For<ILogger<ThemeService>>();

        var service = new ThemeService(paths, fileSystem, site, logger);

        // Act
        var manifest = await service.LoadManifestAsync("bad");

        // Assert
        manifest.Name.ShouldBe("Default");
        manifest.Slug.ShouldBe("default");
    }

    [Fact]
    public async Task Given_ValidThemeManifestJson_When_LoadManifestAsync_Invoked_Then_It_Should_DeserializeCaseInsensitively()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var root = fileSystem.Path.GetPathRoot(Environment.CurrentDirectory) ?? fileSystem.Path.DirectorySeparatorChar.ToString();
        var baseRoot = fileSystem.Path.Combine(root, "base");
        var contentsRoot = fileSystem.Path.Combine(baseRoot, "contents");
        var themesRoot = fileSystem.Path.Combine(baseRoot, "themes");

        var themeRoot = fileSystem.Path.Combine(themesRoot, "minimal");
        fileSystem.AddDirectory(themeRoot);

        var json = "{\n  \"NAME\": \"Minimal Blog\",\n  \"SLUG\": \"minimal\",\n  \"version\": \"9.9.9\"\n}";
        fileSystem.AddFile(fileSystem.Path.Combine(themeRoot, "theme.json"), new MockFileData(json));

        var site = new SiteManifest();
        var paths = new TestAppPaths(basePath: baseRoot, contentsRoot, themesRoot);
        var logger = Substitute.For<ILogger<ThemeService>>();

        var service = new ThemeService(paths, fileSystem, site, logger);

        // Act
        var manifest = await service.LoadManifestAsync("minimal");

        // Assert
        manifest.Name.ShouldBe("Minimal Blog");
        manifest.Slug.ShouldBe("minimal");
        manifest.Version.ShouldBe("9.9.9");
    }

    [Fact]
    public async Task Given_ThemeAssets_When_CopyAssetsAsync_Invoked_Then_It_Should_CopyAssetsAndAllowedFiles()
    {
        // Arrange
        var fileSystem = new MockFileSystem();
        var root = fileSystem.Path.GetPathRoot(Environment.CurrentDirectory) ?? fileSystem.Path.DirectorySeparatorChar.ToString();
        var baseRoot = fileSystem.Path.Combine(root, "base");
        var contentsRoot = fileSystem.Path.Combine(baseRoot, "contents");
        var themesRoot = fileSystem.Path.Combine(baseRoot, "themes");

        var themeSlug = "minimal";
        var themeRoot = fileSystem.Path.Combine(themesRoot, themeSlug);

        fileSystem.AddDirectory(themeRoot);
        fileSystem.AddDirectory(fileSystem.Path.Combine(themeRoot, "assets"));

        fileSystem.AddFile(fileSystem.Path.Combine(themeRoot, "assets", "site.css"), new MockFileData("body{}"));
        fileSystem.AddFile(fileSystem.Path.Combine(themeRoot, "logo.png"), new MockFileData(new byte[] { 1, 2, 3 }));
        fileSystem.AddFile(fileSystem.Path.Combine(themeRoot, "manifest.json"), new MockFileData("{}"));
        fileSystem.AddFile(fileSystem.Path.Combine(themeRoot, "ignored.txt"), new MockFileData("no"));

        var destination = fileSystem.Path.Combine(root, "out");

        var site = new SiteManifest();
        var paths = new TestAppPaths(basePath: baseRoot, contentsRoot, themesRoot);
        var logger = Substitute.For<ILogger<ThemeService>>();

        var service = new ThemeService(paths, fileSystem, site, logger);

        // Act
        await service.CopyAssetsAsync(themeSlug, destination);

        // Assert
        var targetRoot = fileSystem.Path.Combine(destination, ThemeManifest.THEME_DIRECTORY, themeSlug);
        fileSystem.File.Exists(fileSystem.Path.Combine(targetRoot, "assets", "site.css")).ShouldBeTrue();
        fileSystem.File.Exists(fileSystem.Path.Combine(targetRoot, "logo.png")).ShouldBeTrue();
        fileSystem.File.Exists(fileSystem.Path.Combine(targetRoot, "manifest.json")).ShouldBeTrue();
        fileSystem.File.Exists(fileSystem.Path.Combine(targetRoot, "ignored.txt")).ShouldBeFalse();
    }
}
