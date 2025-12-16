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
    /// <param name="themeName">Name of the theme.</param>
    /// <returns>Returns the loaded <see cref="ThemeManifest"/>.</returns>
    ThemeManifest LoadManifest(string themeName);

    /// <summary>
    /// Copies the assets of the specified theme to the destination including stylesheets, JavaScripts and images.
    /// </summary>
    /// <param name="themeSlug">Theme slug.</param>
    /// <param name="destination">Destination path to copy the assets.</param>
    void CopyAssets(string themeSlug, string destination);
}
