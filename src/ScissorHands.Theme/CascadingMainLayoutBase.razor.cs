using Microsoft.AspNetCore.Components;

using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;

namespace ScissorHands.Theme;

public partial class CascadingMainLayoutBase : ComponentBase
{
    /// <summary>
    /// Gets or sets the child content.
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the collection of <see cref="ContentDocument"/> instances.
    /// </summary>
    [Parameter]
    public IEnumerable<ContentDocument>? Documents { get; set; }

    /// <summary>
    /// Gets or sets the dictionary of tags and their associated documents.
    /// Used for tag list view.
    /// </summary>
    [Parameter]
    public IDictionary<string, (IEnumerable<ContentDocument> Posts, IEnumerable<ContentDocument> Pages)>? TaggedDocuments { get; set; }

    /// <summary>
    /// Gets or sets the current tag name.
    /// Used for individual tag view.
    /// </summary>
    [Parameter]
    public string? Tag { get; set; }

    /// <summary>
    /// Gets or sets the posts for the current tag.
    /// Used for individual tag view.
    /// </summary>
    [Parameter]
    public IEnumerable<ContentDocument>? TaggedPosts { get; set; }

    /// <summary>
    /// Gets or sets the pages for the current tag.
    /// Used for individual tag view.
    /// </summary>
    [Parameter]
    public IEnumerable<ContentDocument>? TaggedPages { get; set; }

    /// <summary>
    /// Gets or sets the current <see cref="ContentDocument"/> instance.
    /// </summary>
    [Parameter]
    public ContentDocument? Document { get; set; }

    /// <summary>
    /// Gets or sets the engine-prepared previous and next links for the current page.
    /// </summary>
    [Parameter]
    public PageNavigation PageNavigation { get; set; } = new();

    /// <summary>
    /// Gets or sets the optional engine-prepared locale context for the current render.
    /// </summary>
    [Parameter]
    public LocaleContext? LocaleContext { get; set; }

    /// <summary>
    /// Gets or sets the collection of <see cref="PluginManifest"/> instances.
    /// </summary>
    [Parameter]
    public IEnumerable<PluginManifest>? Plugins { get; set; }

    /// <summary>
    /// Gets or sets the current <see cref="ThemeManifest"/> instance.
    /// </summary>
    [Parameter]
    public ThemeManifest? Theme { get; set; }

    /// <summary>
    /// Gets or sets the validated application theme settings.
    /// </summary>
    [Parameter]
    public ThemeSettings? ThemeSettings { get; set; }

    /// <summary>
    /// Gets or sets the current <see cref="SiteManifest"/> instance.
    /// </summary>
    [Parameter]
    public SiteManifest? Site { get; set; }
}
