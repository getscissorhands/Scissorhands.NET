using Microsoft.AspNetCore.Components;

using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;

namespace ScissorHands.Theme;

public class IndexViewBase : ComponentBase
{
    [Parameter]
    public IEnumerable<ContentDocument> Documents { get; set; } = [];

    [Parameter]
    public ThemeManifest? Theme { get; set; }

    [Parameter]
    public SiteManifest? Site { get; set; }
}
