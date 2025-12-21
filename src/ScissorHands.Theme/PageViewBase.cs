using Microsoft.AspNetCore.Components;

using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;

namespace ScissorHands.Theme;

/// <summary>
/// This represents the base class entity for the page view component.
/// </summary>
public abstract class PageViewBase : ComponentBase
{
    /// <summary>
    /// Gets or sets the <see cref="ContentDocument"/> instance.
    /// </summary>
    [Parameter]
    public ContentDocument Document { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of <see cref="PluginManifest"/> instances.
    /// </summary>
    [Parameter]
    public IEnumerable<PluginManifest>? Plugins { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="ThemeManifest"/> instance.
    /// </summary>
    [Parameter]
    public ThemeManifest? Theme { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="SiteManifest"/> instance.
    /// </summary>
    [Parameter]
    public SiteManifest? Site { get; set; }
}
