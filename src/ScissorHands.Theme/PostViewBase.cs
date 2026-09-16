using Microsoft.AspNetCore.Components;

using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;
using ScissorHands.Core.Urls;

namespace ScissorHands.Theme;

/// <summary>
/// This represents the base class entity for the post view component.
/// </summary>
public abstract class PostViewBase : ComponentBase
{
    /// <summary>
    /// Gets or sets the optional engine-prepared locale context for the current render.
    /// </summary>
    [CascadingParameter]
    public LocaleContext? LocaleContext { get; set; }

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

    /// <inheritdoc cref="ContentUrlHelper.GetTagUrl(string)" />
    protected string GetTagUrl(string tag) => LocaleContext is null ? ContentUrlHelper.GetTagUrl(tag) : LocaleContext.GetTagUrl(tag);

    /// <inheritdoc cref="ContentUrlHelper.GetImageUrl(string)" />
    protected string GetImageUrl(string path) => ContentUrlHelper.GetImageUrl(path);
}
