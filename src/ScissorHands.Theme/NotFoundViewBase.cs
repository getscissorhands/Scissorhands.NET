using Microsoft.AspNetCore.Components;

using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;

namespace ScissorHands.Theme;

/// <summary>
/// This represents the base class entity for the not found (404) view component.
/// </summary>
public abstract class NotFoundViewBase : ComponentBase
{
    /// <summary>
    /// Gets or sets the <see cref="ContentDocument"/> instance.
    /// If a page document with the slug <c>404.html</c> exists, it will be provided here.
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
