namespace ScissorHands.Core.Manifests;

/// <summary>
/// This represents the manifest entity for theme.
/// </summary>
public sealed class ThemeManifest
{
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
    public List<string> Stylesheets { get; init; } = [];

    /// <summary>
    /// Gets the list of JavaScript files included in the theme.
    /// </summary>
    public List<string> Scripts { get; init; } = [];
}
