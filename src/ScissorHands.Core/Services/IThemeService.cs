using ScissorHands.Core.Manifests;

namespace ScissorHands.Core.Services;

/// <summary>
/// This provides the interface for theme service.
/// </summary>
public interface IThemeService
{
    /// <summary>
    /// Loads the theme manifest.
    /// </summary>
    /// <param name="themeSlug">Theme slug.</param>
    /// <returns>Returns the loaded <see cref="ThemeManifest"/>.</returns>
    Task<ThemeManifest> LoadManifestAsync(string themeSlug);

    /// <summary>
    /// Copies the assets of the specified theme to the destination including stylesheets, JavaScripts and images.
    /// </summary>
    /// <param name="themeSlug">Theme slug.</param>
    /// <param name="destination">Destination path to copy the assets.</param>
    Task CopyAssetsAsync(string themeSlug, string destination);
}
