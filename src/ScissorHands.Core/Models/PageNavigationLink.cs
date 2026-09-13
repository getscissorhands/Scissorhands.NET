namespace ScissorHands.Core.Models;

/// <summary>
/// Represents an immutable link to a page in the reading sequence.
/// </summary>
public sealed record PageNavigationLink
{
    /// <summary>
    /// Gets the display title, rendered as text by the theme.
    /// </summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// Gets the engine-formatted, base-relative page URL.
    /// </summary>
    public string Url { get; init; } = string.Empty;
}
