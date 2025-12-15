using Microsoft.AspNetCore.Components;

using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;

namespace ScissorHands.Plugin;

public class PluginComponentBase : ComponentBase
{
    [Parameter]
    public ContentDocument Document { get; set; } = new();

    [Parameter]
    public ThemeManifest? Theme { get; set; }

    [Parameter]
    public PluginManifest? Plugin { get; set; }

    [Parameter]
    public SiteManifest? Site { get; set; }
}
