using System.Collections.ObjectModel;

namespace ScissorHands.Core.Manifests;

/// <summary>
/// Represents application settings in the top-level Theme configuration section.
/// These settings are independent of the selected theme's package manifest.
/// </summary>
public sealed class ThemeSettings
{
    private IReadOnlyDictionary<string, ThemeLocalization?> _localization
        = ReadOnlyDictionary<string, ThemeLocalization?>.Empty;
    private IReadOnlyList<ThemeHeroImage> _heroImages = Array.Empty<ThemeHeroImage>();

    /// <summary>
    /// Gets the application-supplied theme messages, keyed by locale.
    /// </summary>
    public IReadOnlyDictionary<string, ThemeLocalization?> Localization
    {
        get => _localization;
        init => _localization = value is null
            ? ReadOnlyDictionary<string, ThemeLocalization?>.Empty
            : new ReadOnlyDictionary<string, ThemeLocalization?>(
                new Dictionary<string, ThemeLocalization?>(value, StringComparer.Ordinal));
    }

    /// <summary>
    /// Gets the optional, ordered site-wide images available to themes.
    /// Themes decide whether and how to render them.
    /// </summary>
    public IReadOnlyList<ThemeHeroImage> HeroImages
    {
        get => _heroImages;
        init => _heroImages = Array.AsReadOnly((value ?? Array.Empty<ThemeHeroImage>()).ToArray());
    }
}
