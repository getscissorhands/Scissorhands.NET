using Microsoft.AspNetCore.Components;

using ScissorHands.Core.Models;

namespace ScissorHands.Theme.Components;

/// <summary>
/// Renders the prepared navigation hierarchy as an unstyled list.
/// Supply the surrounding navigation landmark and list in the theme layout.
/// </summary>
public partial class PlainNavigationItems : ComponentBase
{
    /// <summary>
    /// Gets or sets the prepared nodes for this level of navigation.
    /// </summary>
    [Parameter]
    public IReadOnlyList<NavigationNode> Nodes { get; set; } = [];

    private static string RequireRelativeUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url) || url[0] == '/' || char.IsWhiteSpace(url[0])
            || url.Contains(':') || url.Contains('\\') || url.Any(char.IsControl))
        {
            throw new InvalidOperationException("PlainNavigationItems requires a nonempty base-relative navigation URL.");
        }

        return url;
    }
}
