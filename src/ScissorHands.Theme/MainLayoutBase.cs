using Microsoft.AspNetCore.Components;

using ScissorHands.Core.Manifests;
using ScissorHands.Core.Services;

namespace ScissorHands.Theme;

/// <summary>
/// This represents the base class entity for the main layout component.
/// </summary>
public class MainLayoutBase : LayoutComponentBase
{
    /// <summary>
    /// Gets or sets the <see cref="ThemeManifest"/> instance.
    /// </summary>
    protected ThemeManifest? Theme { get; set; }

    /// <summary>
    /// Gets or sets the list of <see cref="PluginManifest"/> instances.
    /// </summary>
    [Inject]
    public IEnumerable<PluginManifest>? Plugins { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="IThemeService"/> instance.
    /// </summary>
    [Inject]
    public IThemeService? ThemeService { get; init; }

    /// <summary>
    /// Gets or sets the <see cref="SiteManifest"/> instance.
    /// </summary>
    [Inject]
    public SiteManifest? Site { get; init; }

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        Theme = ThemeService!.LoadManifest(Site!.Theme);
    }
}
