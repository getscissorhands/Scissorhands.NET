using System.Text.Json;

using Microsoft.Extensions.Logging;

using ScissorHands.Core.Manifests;
using ScissorHands.Core.Services;

namespace ScissorHands.Web.Services;

/// <summary>
/// This represents the service entity for theme.
/// </summary>
/// <param name="site"><see cref="SiteManifest"/> instance.</param>
/// <param name="logger"><see cref="ILogger{T}"/> instance.</param>
public sealed class ThemeService(SiteManifest site, ILogger<ThemeService> logger) : IThemeService
{
    private readonly SiteManifest _site = site ?? throw new ArgumentNullException(nameof(site));
    private readonly ILogger<ThemeService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly string _basePath = Directory.GetCurrentDirectory();

    /// <inheritdoc />
    public ThemeManifest LoadManifest(string themeSlug)
    {
        var manifestPath = Path.Combine(_basePath, "themes", themeSlug, "theme.json");
        if (!File.Exists(manifestPath))
        {
            _logger.LogWarning("Theme manifest not found at {Path}", manifestPath);

            return new ThemeManifest { Name = themeSlug };
        }

        try
        {
            var json = File.ReadAllText(manifestPath);
            return JsonSerializer.Deserialize<ThemeManifest>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                   ?? new ThemeManifest { Name = themeSlug };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read theme manifest {Path}", manifestPath);
            return new ThemeManifest { Name = themeSlug };
        }
    }

    public void CopyAssets(string themeSlug, string destination)
    {
        var themeRoot = Path.Combine(_basePath, "themes", themeSlug);
        var targetRoot = Path.Combine(destination, "themes", themeSlug);

        if (!Directory.Exists(themeRoot))
        {
            _logger.LogWarning("Theme folder not found at {Path}", themeRoot);
            return;
        }

        var sourceAssets = Path.Combine(themeRoot, "assets");
        var targetAssets = Path.Combine(targetRoot, "assets");
        if (Directory.Exists(sourceAssets))
        {
            CopyDirectory(sourceAssets, targetAssets);
        }
        else
        {
            _logger.LogWarning("Theme assets not found at {Path}", sourceAssets);
        }

        var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".png", ".jpg", ".jpeg", ".gif", ".webp", ".ico"
        };

        foreach (var file in Directory.EnumerateFiles(themeRoot, "*", SearchOption.AllDirectories)
                     .Where(path => allowedExtensions.Contains(Path.GetExtension(path))))
        {
            var relative = Path.GetRelativePath(themeRoot, file);
            var destinationPath = Path.Combine(targetRoot, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
            File.Copy(file, destinationPath, overwrite: true);
        }

        var allowedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "manifest.json"
        };

        foreach (var file in Directory.EnumerateFiles(themeRoot, "*", SearchOption.AllDirectories)
                     .Where(path => allowedFiles.Contains(Path.GetFileName(path))))
        {
            var relative = Path.GetRelativePath(themeRoot, file);
            var destinationPath = Path.Combine(targetRoot, relative);
            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
            File.Copy(file, destinationPath, overwrite: true);
        }
    }

    private static void CopyDirectory(string sourceDir, string destinationDir)
    {
        Directory.CreateDirectory(destinationDir);

        foreach (var file in Directory.GetFiles(sourceDir, "*", SearchOption.TopDirectoryOnly))
        {
            var destFile = Path.Combine(destinationDir, Path.GetFileName(file));
            File.Copy(file, destFile, overwrite: true);
        }

        foreach (var directory in Directory.GetDirectories(sourceDir, "*", SearchOption.TopDirectoryOnly))
        {
            var name = Path.GetFileName(directory);
            CopyDirectory(directory, Path.Combine(destinationDir, name));
        }
    }
}
