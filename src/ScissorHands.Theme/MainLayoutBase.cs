using Microsoft.AspNetCore.Components;

using ScissorHands.Core.Manifests;
using ScissorHands.Core.Services;

namespace ScissorHands.Theme;

public class MainLayoutBase : LayoutComponentBase
{
    protected ThemeManifest? Theme { get; set; }

    [Inject]
    public IThemeService? ThemeService { get; init; }

    [Inject]
    public SiteManifest? Site { get; init; }

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        Theme = ThemeService!.LoadManifest(Site!.Theme);
    }
}
