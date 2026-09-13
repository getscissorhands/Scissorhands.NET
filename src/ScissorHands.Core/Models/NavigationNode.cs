namespace ScissorHands.Core.Models;

/// <summary>
/// Represents a theme-independent node in the site's navigation hierarchy.
/// </summary>
public sealed record NavigationNode
{
    private IReadOnlyList<NavigationNode> _children = Array.Empty<NavigationNode>();

    /// <summary>
    /// Gets the display title, rendered as text by the theme.
    /// </summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// Gets the escaped, base-relative path that identifies the node.
    /// </summary>
    public string Path { get; init; } = string.Empty;

    /// <summary>
    /// Gets the base-relative page URL, or null for a non-clickable group.
    /// </summary>
    public string? Url { get; init; }

    /// <summary>
    /// Gets the ordered children, snapshotted during initialization.
    /// </summary>
    public IReadOnlyList<NavigationNode> Children
    {
        get => _children;
        init => _children = Array.AsReadOnly((value ?? Array.Empty<NavigationNode>()).ToArray());
    }
}
