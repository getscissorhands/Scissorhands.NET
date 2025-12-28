using Microsoft.AspNetCore.Components;

using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;

namespace ScissorHands.Plugin;

/// <summary>
/// This represents the base component entity for content plugin.
/// </summary>
public class PluginComponentBase : ComponentBase
{
    /// <summary>
    /// Gets or sets the <see cref="PluginManifest"/> instance.
    /// </summary>
    protected PluginManifest? Plugin { get; set; }

    /// <summary>
    /// Gets or sets the plugin name.
    /// </summary>
    [Parameter]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the cascaded list of <see cref="ContentDocument"/> instances.
    /// </summary>
    [CascadingParameter]
    protected IEnumerable<ContentDocument>? Documents { get; set; }

    /// <summary>
    /// Gets or sets the cascaded <see cref="ContentDocument"/> instance.
    /// </summary>
    [CascadingParameter]
    protected ContentDocument? Document { get; set; }

    /// <summary>
    /// Gets or sets the cascaded list of <see cref="PluginManifest"/> instances.
    /// </summary>
    [CascadingParameter]
    protected IEnumerable<PluginManifest>? Plugins { get; set; }

    /// <summary>
    /// Gets or sets the cascaded <see cref="ThemeManifest"/> instance.
    /// </summary>
    [CascadingParameter]
    protected ThemeManifest? Theme { get; set; }

    /// <summary>
    /// Gets or sets the cascaded <see cref="SiteManifest"/> instance.
    /// </summary>
    [CascadingParameter]
    protected SiteManifest? Site { get; set; }

    /// <inheritdoc />
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        Plugin = Plugins?.SingleOrDefault(p => p.Name!.Equals(Name, StringComparison.OrdinalIgnoreCase));
    }
}
