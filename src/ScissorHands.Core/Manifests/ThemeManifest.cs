using System.Collections.ObjectModel;

namespace ScissorHands.Core.Manifests;

/// <summary>
/// This represents the manifest entity for theme.
/// </summary>
public sealed class ThemeManifest
{
    private IReadOnlyList<string> _stylesheets = Array.Empty<string>();
    private IReadOnlyList<string> _scripts = Array.Empty<string>();
    private IReadOnlyDictionary<string, ThemeLocalization?> _localization
        = ReadOnlyDictionary<string, ThemeLocalization?>.Empty;

    /// <summary>
    /// Defines the theme directory name.
    /// </summary>
    public const string THEME_DIRECTORY = "themes";

    /// <summary>
    /// Gets the name of the theme.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets the version of the theme.
    /// </summary>
    public string Version { get; init; } = "1.0.0";

    /// <summary>
    /// Gets the description of the theme.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets the slug of the theme.
    /// </summary>
    public string Slug { get; init; } = string.Empty;

    /// <summary>
    /// Gets the list of CSS stylesheets included in the theme.
    /// </summary>
    public IReadOnlyList<string> Stylesheets
    {
        get => _stylesheets;
        init => _stylesheets = Array.AsReadOnly((value ?? Array.Empty<string>()).ToArray());
    }

    /// <summary>
    /// Gets the list of JavaScript files included in the theme.
    /// </summary>
    public IReadOnlyList<string> Scripts
    {
        get => _scripts;
        init => _scripts = Array.AsReadOnly((value ?? Array.Empty<string>()).ToArray());
    }

    /// <summary>
    /// Gets the application-supplied theme messages, keyed by locale.
    /// The collection is snapshotted; null entries are retained for contextual validation.
    /// </summary>
    public IReadOnlyDictionary<string, ThemeLocalization?> Localization
    {
        get => _localization;
        init => _localization = value is null
            ? ReadOnlyDictionary<string, ThemeLocalization?>.Empty
            : new ReadOnlyDictionary<string, ThemeLocalization?>(
                new Dictionary<string, ThemeLocalization?>(value, StringComparer.Ordinal));
    }
}
