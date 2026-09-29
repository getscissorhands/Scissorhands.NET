using Microsoft.AspNetCore.Components;

using ScissorHands.Core.Models;

namespace ScissorHands.Web;

public partial class NavigationItems : ComponentBase
{
    /// <summary>
    /// Gets or sets the engine-prepared navigation nodes for this level.
    /// </summary>
    [Parameter]
    public IReadOnlyList<NavigationNode> Nodes { get; set; } = [];
}
