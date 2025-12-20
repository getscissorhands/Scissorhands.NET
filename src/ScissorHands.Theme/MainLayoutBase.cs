using Microsoft.AspNetCore.Components;

using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;
using ScissorHands.Core.Services;

namespace ScissorHands.Theme;

/// <summary>
/// This represents the base class entity for the main layout component.
/// </summary>
public class MainLayoutBase : LayoutComponentBase
{
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
        Theme = ThemeService!.LoadManifest(Site!.Theme);

        await Task.CompletedTask;
    }
}
