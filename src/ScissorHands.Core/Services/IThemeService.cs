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
    /// Loads the theme manifest.
    /// </summary>
    /// <param name="themeSlug">Theme slug.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Returns the loaded <see cref="ThemeManifest"/>.</returns>
    async Task<ThemeManifest> LoadManifestAsync(string themeSlug, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await LoadManifestAsync(themeSlug);
    }

    /// <summary>
    /// Copies the assets of the specified theme to the destination.
    /// </summary>
    /// <param name="themeSlug">Theme slug.</param>
    /// <param name="destination">Destination path to copy the assets.</param>
    Task CopyAssetsAsync(string themeSlug, string destination);

    /// <summary>
    /// Copies the assets of the specified theme to the destination including stylesheets, JavaScripts and images.
    /// </summary>
    /// <param name="themeSlug">Theme slug.</param>
    /// <param name="destination">Destination path to copy the assets.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    async Task CopyAssetsAsync(string themeSlug, string destination, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await CopyAssetsAsync(themeSlug, destination);
    }
}
