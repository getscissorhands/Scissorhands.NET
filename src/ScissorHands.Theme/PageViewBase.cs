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
    [CascadingParameter]
    public ContentDocument? Document { get; set; }

    /// <summary>
    /// Gets or sets the list of <see cref="PluginManifest"/> instances.
    /// </summary>
    [CascadingParameter]
    public IEnumerable<PluginManifest>? Plugins { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="ThemeManifest"/> instance.
    /// </summary>
    [CascadingParameter]
    public ThemeManifest? Theme { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="SiteManifest"/> instance.
    /// </summary>
    [CascadingParameter]
    public SiteManifest? Site { get; set; }
}
