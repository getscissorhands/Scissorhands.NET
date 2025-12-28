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
    /// Gets or sets the list of <see cref="ContentDocument"/> instances.
    /// </summary>
    [Parameter]
    public IEnumerable<ContentDocument>? Documents { get; set; }

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
    protected override async Task OnInitializedAsync()
    {
        PageTitle = CalculatePageTitle();
        PageDescription = CalculatePageDescription();

        Theme = await ThemeService!.LoadManifestAsync(Site!.Theme);
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
}
