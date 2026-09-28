namespace ScissorHands.Core.Manifests;

/// <summary>
/// Represents an optional site-wide image supplied to the selected theme.
/// </summary>
public sealed record ThemeHeroImage
{
    /// <summary>
    /// Gets a site-relative image path or an absolute HTTP(S) URL.
    /// </summary>
    public string Source { get; init; } = string.Empty;

    /// <summary>
    /// Gets alternative text; an empty value marks a decorative image.
    /// </summary>
    public string Alt { get; init; } = string.Empty;
}
