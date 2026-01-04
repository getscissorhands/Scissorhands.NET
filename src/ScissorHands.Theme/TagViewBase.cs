using Microsoft.AspNetCore.Components;

using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;

namespace ScissorHands.Theme;

/// <summary>
/// This represents the base class entity for the tag view component.
/// This view displays all posts and pages for a specific tag.
/// </summary>
public abstract class TagViewBase : ComponentBase
{
    /// <summary>
    /// Gets or sets the tag name.
    /// </summary>
    [CascadingParameter]
    public string? Tag { get; set; }

    /// <summary>
    /// Gets or sets the list of posts for this tag.
    /// </summary>
    [CascadingParameter]
    public IEnumerable<ContentDocument>? Posts { get; set; }

    /// <summary>
    /// Gets or sets the list of pages for this tag.
    /// </summary>
    [CascadingParameter]
    public IEnumerable<ContentDocument>? Pages { get; set; }

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
