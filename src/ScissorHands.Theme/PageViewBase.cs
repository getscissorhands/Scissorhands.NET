using Microsoft.AspNetCore.Components;

using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;

namespace ScissorHands.Theme;

public class PageViewBase : ComponentBase
{
    [Parameter]
    public ContentDocument Document { get; set; } = new();

    [Parameter]
    public ThemeManifest Theme { get; set; } = new();

    [Parameter]
    public SiteManifest Site { get; set; } = new();
}
