using Microsoft.AspNetCore.Components;

using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;
using ScissorHands.Core.Urls;

namespace ScissorHands.Theme;

/// <summary>
/// This represents the base class entity for the tag list view component.
/// This view displays all tags with their associated posts and pages.
/// </summary>
public abstract class TagListViewBase : ComponentBase
{
    /// <summary>
    /// Gets or sets the optional engine-prepared locale context for the current render.
    /// </summary>
    [CascadingParameter]
    public LocaleContext? LocaleContext { get; set; }

    /// <summary>
    /// Gets or sets the dictionary of tags and their associated documents.
    /// The key is the tag name, and the value is the tuple of posts and pages.
    /// </summary>
    [CascadingParameter(Name = "TaggedDocuments")]
    public IDictionary<string, (IEnumerable<ContentDocument> Posts, IEnumerable<ContentDocument> Pages)>? TaggedDocuments { get; set; }

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
}
