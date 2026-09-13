using Microsoft.AspNetCore.Components;

using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;
using ScissorHands.Core.Services;

namespace ScissorHands.Theme;

/// <summary>
/// This represents the base class entity for the main layout component.
/// </summary>
public abstract class MainLayoutBase : LayoutComponentBase
{
    /// <summary>
    /// Gets or sets the page title calculated.
    /// </summary>
    protected string? PageTitle { get; set; }

    /// <summary>
    /// Gets or sets the page description calculated.
    /// </summary>
    protected string? PageDescription { get; set; }

    /// <summary>
    /// Gets or sets the page locale calculated.
    /// </summary>
    protected string? PageLocale { get; set; }

    /// <summary>
    /// Gets or sets the list of <see cref="ContentDocument"/> instances.
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
    /// Gets or sets the <see cref="ContentDocument"/> instance.
    /// </summary>
    [Parameter]
    public ContentDocument? Document { get; set; }

    /// <summary>
    /// Gets or sets the list of <see cref="PluginManifest"/> instances.
    /// </summary>
    [Parameter]
    public IEnumerable<PluginManifest>? Plugins { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="IThemeService"/> instance.
    /// </summary>
    [Inject]
    public IThemeService? ThemeService { get; init; }

    /// <summary>
    /// Gets or sets the <see cref="ThemeManifest"/> instance.
    /// </summary>
    [Parameter]
    public ThemeManifest? Theme { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="SiteManifest"/> instance.
    /// </summary>
    [Parameter]
    public SiteManifest? Site { get; init; }

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        PageTitle = CalculatePageTitle();
        PageDescription = CalculatePageDescription();
        PageLocale = CalculatePageLocale();
    }

    /// <summary>
    /// Gets a base-relative URL for a path within the current theme.
    /// </summary>
    /// <param name="path">The theme-relative path, optionally prefixed with slashes.</param>
    /// <returns>The URL relative to the site's base URL.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="path"/> is null.</exception>
    /// <exception cref="InvalidOperationException"><see cref="Theme"/> has not been supplied.</exception>
    protected string GetThemeUrl(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        var theme = Theme ?? throw new InvalidOperationException("A theme must be supplied before getting a theme URL.");

        return $"{ThemeManifest.THEME_DIRECTORY}/{theme.Slug.Trim('/')}/{path.TrimStart('/')}";
    }

    /// <summary>
    /// Calculates the page title based on the site and document title.
    /// </summary>
    /// <returns>Returns the page title calculated.</returns>
    protected virtual string CalculatePageTitle()
    {
        var title = Site?.Title;
        if (Document is not null && string.IsNullOrWhiteSpace(Document.Metadata.Title) == false)
        {
            title = $"{Document.Metadata.Title} | {title}";
        }

        return title ?? string.Empty;
    }

    /// <summary>
    /// Calculates the page description based on the site and document description.
    /// </summary>
    /// <returns>Returns the page description calculated.</returns>
    protected virtual string CalculatePageDescription()
    {
        var description = Site?.Description ?? string.Empty;

        if (Document is not null && string.IsNullOrWhiteSpace(Document.Metadata.Description) == false)
        {
            description = Document.Metadata.Description;
        }

        return description ?? string.Empty;
    }

    /// <summary>
    /// Calculates the page locale based on the site and document locale.
    /// </summary>
    /// <returns>Returns the page locale calculated.</returns>
    protected virtual string CalculatePageLocale()
    {
        var locale = Site?.Locale ?? string.Empty;

        if (Document is not null && string.IsNullOrWhiteSpace(Document.Metadata.Locale) == false)
        {
            locale = Document.Metadata.Locale;
        }

        return locale.ToLowerInvariant() ?? string.Empty;
    }
}
