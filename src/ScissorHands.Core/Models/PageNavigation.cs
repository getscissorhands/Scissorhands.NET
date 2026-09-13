namespace ScissorHands.Core.Models;

/// <summary>
/// Represents the engine-prepared neighbors of a page in the reading sequence.
/// </summary>
public sealed record PageNavigation
{
    /// <summary>
    /// Gets the previous page, or null when no previous page is available.
    /// </summary>
    public PageNavigationLink? Previous { get; init; }

    /// <summary>
    /// Gets the next page, or null when no next page is available.
    /// </summary>
    public PageNavigationLink? Next { get; init; }
}
