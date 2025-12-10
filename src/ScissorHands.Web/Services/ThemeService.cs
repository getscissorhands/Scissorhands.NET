using System.Text.Json;
using Microsoft.Extensions.Logging;

using ScissorHands.Core.Manifests;
using ScissorHands.Core.Services;

namespace ScissorHands.Web.Services;

public sealed class ThemeService(ILogger<ThemeService> logger) : IThemeService
{
    private readonly ILogger<ThemeService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly string _basePath = Directory.GetCurrentDirectory();

    public ThemeManifest LoadManifest(string themeName)
    {
        var manifestPath = Path.Combine(_basePath, "themes", themeName, "theme.json");
        if (!File.Exists(manifestPath))
        {
            _logger.LogWarning("Theme manifest not found at {Path}", manifestPath);
            return new ThemeManifest { Name = themeName };
        }

        try
        {
            var json = File.ReadAllText(manifestPath);
            return JsonSerializer.Deserialize<ThemeManifest>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                   ?? new ThemeManifest { Name = themeName };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read theme manifest {Path}", manifestPath);
            return new ThemeManifest { Name = themeName };
        }
    }

    public void CopyAssets(string themeName, string destination)
    {
        var themeRoot = Path.Combine(_basePath, "themes", themeName);
        var targetRoot = Path.Combine(destination, "themes", themeName);

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
