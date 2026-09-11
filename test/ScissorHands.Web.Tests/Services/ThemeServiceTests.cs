using System.IO.Abstractions.TestingHelpers;

using Microsoft.Extensions.Logging;

using ScissorHands.Core.Manifests;
using ScissorHands.Web.Services;
using ScissorHands.Web.Tests.TestDoubles;

namespace ScissorHands.Web.Tests.Services;

public class ThemeServiceTests
{
    [Fact]
    public async Task Given_MissingConfiguredThemeManifest_When_LoadManifestAsyncInvoked_Then_It_Should_ReportMissingFile()
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
        var exception = await Should.ThrowAsync<FileNotFoundException>(() =>
            service.LoadManifestAsync("missing", CancellationToken.None));

        // Assert
        exception.FileName.ShouldEndWith(fileSystem.Path.Combine("missing", "theme.json"));
    }

    [Fact]
    public async Task Given_MissingDefaultThemeManifest_When_LoadManifestAsyncInvoked_Then_It_Should_ReturnBuiltInManifest()
    {
        var fileSystem = new MockFileSystem();
        var root = fileSystem.Path.GetPathRoot(Environment.CurrentDirectory) ?? fileSystem.Path.DirectorySeparatorChar.ToString();
        var baseRoot = fileSystem.Path.Combine(root, "base");
        var contentsRoot = fileSystem.Path.Combine(baseRoot, "contents");
        var themesRoot = fileSystem.Path.Combine(baseRoot, "themes");
        var service = new ThemeService(
            new TestAppPaths(baseRoot, contentsRoot, themesRoot),
            fileSystem,
            new SiteManifest(),
            Substitute.For<ILogger<ThemeService>>());

        var manifest = await service.LoadManifestAsync("default", CancellationToken.None);

        manifest.Name.ShouldBe("Default");
        manifest.Slug.ShouldBe("default");
    }

    [Fact]
    public async Task Given_InvalidThemeManifestJson_When_LoadManifestAsyncInvoked_Then_It_Should_ReportInvalidData()
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
        var exception = await Should.ThrowAsync<InvalidDataException>(() =>
            service.LoadManifestAsync("bad", CancellationToken.None));

        // Assert
        exception.Message.ShouldContain("invalid JSON");
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
        var manifest = await service.LoadManifestAsync("minimal", CancellationToken.None);

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
        await service.CopyAssetsAsync(themeSlug, destination, CancellationToken.None);

        // Assert
        var targetRoot = fileSystem.Path.Combine(destination, ThemeManifest.THEME_DIRECTORY, themeSlug);
        fileSystem.File.Exists(fileSystem.Path.Combine(targetRoot, "assets", "site.css")).ShouldBeTrue();
        fileSystem.File.Exists(fileSystem.Path.Combine(targetRoot, "logo.png")).ShouldBeTrue();
        fileSystem.File.Exists(fileSystem.Path.Combine(targetRoot, "manifest.json")).ShouldBeTrue();
        fileSystem.File.Exists(fileSystem.Path.Combine(targetRoot, "ignored.txt")).ShouldBeFalse();
    }

    [Fact]
    public async Task Given_CancelledToken_When_LoadManifestAsyncInvoked_Then_It_Should_PropagateCancellation()
    {
        var fileSystem = new MockFileSystem();
        var root = fileSystem.Path.GetPathRoot(Environment.CurrentDirectory) ?? fileSystem.Path.DirectorySeparatorChar.ToString();
        var baseRoot = fileSystem.Path.Combine(root, "base");
        var contentsRoot = fileSystem.Path.Combine(baseRoot, "contents");
        var themesRoot = fileSystem.Path.Combine(baseRoot, "themes");
        fileSystem.AddFile(fileSystem.Path.Combine(themesRoot, "minimal", "theme.json"), new MockFileData("{}"));

        var service = new ThemeService(
            new TestAppPaths(baseRoot, contentsRoot, themesRoot),
            fileSystem,
            new SiteManifest(),
            Substitute.For<ILogger<ThemeService>>());
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Should.ThrowAsync<OperationCanceledException>(() =>
            service.LoadManifestAsync("minimal", cancellation.Token));
    }

    [Fact]
    public async Task Given_BundledDefaultThemeAssets_When_ProjectThemeFolderMissing_Then_It_Should_CopyBundledAssets()
    {
        var fileSystem = new MockFileSystem();
        var root = fileSystem.Path.GetPathRoot(Environment.CurrentDirectory) ?? fileSystem.Path.DirectorySeparatorChar.ToString();
        var baseRoot = fileSystem.Path.Combine(root, "base");
        var contentsRoot = fileSystem.Path.Combine(baseRoot, "contents");
        var themesRoot = fileSystem.Path.Combine(baseRoot, "themes");
        var bundledFavicon = fileSystem.Path.Combine(
            AppContext.BaseDirectory,
            ThemeManifest.THEME_DIRECTORY,
            "default",
            "favicon.ico");
        fileSystem.AddFile(bundledFavicon, new MockFileData([1, 2, 3]));
        var destination = fileSystem.Path.Combine(root, "output");
        var service = new ThemeService(
            new TestAppPaths(baseRoot, contentsRoot, themesRoot),
            fileSystem,
            new SiteManifest(),
            Substitute.For<ILogger<ThemeService>>());

        await service.CopyAssetsAsync("default", destination, CancellationToken.None);

        fileSystem.File.Exists(
            fileSystem.Path.Combine(destination, ThemeManifest.THEME_DIRECTORY, "default", "favicon.ico"))
            .ShouldBeTrue();
    }
}
