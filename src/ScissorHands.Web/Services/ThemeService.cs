using System.IO.Abstractions;
using System.Text.Json;

using Microsoft.Extensions.Logging;

using ScissorHands.Core.Manifests;
using ScissorHands.Core.Services;
using ScissorHands.Web.Abstractions;

namespace ScissorHands.Web.Services;

/// <summary>
/// This represents the service entity for theme.
/// </summary>
/// <param name="paths"><see cref="IAppPaths"/> instance.</param>
/// <param name="fileSystem"><see cref="IFileSystem"/> instance.</param>
/// <param name="site"><see cref="SiteManifest"/> instance.</param>
/// <param name="logger"><see cref="ILogger{T}"/> instance.</param>
public sealed class ThemeService(IAppPaths paths, IFileSystem fileSystem, SiteManifest site, ILogger<ThemeService> logger) : IThemeService
{
    private readonly IAppPaths _paths = paths ?? throw new ArgumentNullException(nameof(paths));
    private readonly IFileSystem _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    private readonly SiteManifest _site = site ?? throw new ArgumentNullException(nameof(site));
    private readonly ILogger<ThemeService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <inheritdoc />
    [Obsolete("Use LoadManifestAsync(string, CancellationToken). This overload will be removed in the next major version.")]
    public Task<ThemeManifest> LoadManifestAsync(string themeSlug)
        => LoadManifestAsync(themeSlug, CancellationToken.None);

    /// <inheritdoc />
    public async Task<ThemeManifest> LoadManifestAsync(string themeSlug, CancellationToken cancellationToken = default)
    {
        var manifestPath = _fileSystem.Path.Combine(_paths.GetThemesRoot(), themeSlug, "theme.json");
        if (!_fileSystem.File.Exists(manifestPath))
        {
            if (string.IsNullOrWhiteSpace(themeSlug)
                || string.Equals(themeSlug, "default", StringComparison.OrdinalIgnoreCase))
            {
                return new ThemeManifest { Name = "Default", Slug = "default" };
            }

            throw new FileNotFoundException($"Theme manifest was not found at '{manifestPath}'.", manifestPath);
        }

        try
        {
            var json = await _fileSystem.File.ReadAllTextAsync(manifestPath, cancellationToken);
            var manifest = JsonSerializer.Deserialize<ThemeManifest>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? throw new InvalidDataException($"Theme manifest '{manifestPath}' is empty.");

            if (string.IsNullOrWhiteSpace(manifest.Name) || string.IsNullOrWhiteSpace(manifest.Slug))
            {
                throw new InvalidDataException($"Theme manifest '{manifestPath}' must define non-empty name and slug values.");
            }

            if (!string.Equals(manifest.Slug, themeSlug, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException(
                    $"Theme manifest slug '{manifest.Slug}' does not match configured theme '{themeSlug}'.");
            }

            return manifest;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (JsonException ex)
        {
            throw new InvalidDataException($"Theme manifest '{manifestPath}' contains invalid JSON.", ex);
        }
    }

    /// <inheritdoc />
    [Obsolete("Use CopyAssetsAsync(string, string, CancellationToken). This overload will be removed in the next major version.")]
    public Task CopyAssetsAsync(string themeSlug, string destination)
        => CopyAssetsAsync(themeSlug, destination, CancellationToken.None);

    /// <inheritdoc />
    public Task CopyAssetsAsync(string themeSlug, string destination, CancellationToken cancellationToken = default)
    {
        var themeRoot = _fileSystem.Path.Combine(_paths.GetThemesRoot(), themeSlug);
        var targetRoot = _fileSystem.Path.Combine(destination, ThemeManifest.THEME_DIRECTORY, themeSlug);

        if (!_fileSystem.Directory.Exists(themeRoot))
        {
            _logger.LogWarning("Theme folder not found at {Path}", themeRoot);
            return Task.CompletedTask;
        }

        var sourceAssets = _fileSystem.Path.Combine(themeRoot, "assets");
        var targetAssets = _fileSystem.Path.Combine(targetRoot, "assets");
        if (_fileSystem.Directory.Exists(sourceAssets))
        {
            CopyDirectory(sourceAssets, targetAssets, cancellationToken);
        }
        else
        {
            _logger.LogWarning("Theme assets not found at {Path}", sourceAssets);
        }

        var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".png", ".jpg", ".jpeg", ".gif", ".webp", ".ico"
        };

        foreach (var file in _fileSystem.Directory.EnumerateFiles(themeRoot, "*", SearchOption.AllDirectories)
                     .Where(path => allowedExtensions.Contains(_fileSystem.Path.GetExtension(path))))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var relative = _fileSystem.Path.GetRelativePath(themeRoot, file);
            var destinationPath = _fileSystem.Path.Combine(targetRoot, relative);
            _fileSystem.Directory.CreateDirectory(_fileSystem.Path.GetDirectoryName(destinationPath)!);
            _fileSystem.File.Copy(file, destinationPath, overwrite: true);
        }

        var allowedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "manifest.json"
        };

        foreach (var file in _fileSystem.Directory.EnumerateFiles(themeRoot, "*", SearchOption.AllDirectories)
                     .Where(path => allowedFiles.Contains(_fileSystem.Path.GetFileName(path))))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var relative = _fileSystem.Path.GetRelativePath(themeRoot, file);
            var destinationPath = _fileSystem.Path.Combine(targetRoot, relative);
            _fileSystem.Directory.CreateDirectory(_fileSystem.Path.GetDirectoryName(destinationPath)!);
            _fileSystem.File.Copy(file, destinationPath, overwrite: true);
        }

        return Task.CompletedTask;
    }

    private void CopyDirectory(string sourceDir, string destinationDir, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _fileSystem.Directory.CreateDirectory(destinationDir);

        foreach (var file in _fileSystem.Directory.GetFiles(sourceDir, "*", SearchOption.TopDirectoryOnly))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var destFile = _fileSystem.Path.Combine(destinationDir, _fileSystem.Path.GetFileName(file));
            _fileSystem.File.Copy(file, destFile, overwrite: true);
        }

        foreach (var directory in _fileSystem.Directory.GetDirectories(sourceDir, "*", SearchOption.TopDirectoryOnly))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var name = _fileSystem.Path.GetFileName(directory);
            CopyDirectory(directory, _fileSystem.Path.Combine(destinationDir, name), cancellationToken);
        }
    }
}
