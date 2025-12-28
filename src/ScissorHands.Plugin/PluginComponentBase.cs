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
    /// Gets or sets the <see cref="PluginManifest"/> instance.
    /// </summary>
    [Parameter]
    public IEnumerable<PluginManifest>? Plugins { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="ThemeManifest"/> instance.
    /// </summary>
    [Parameter]
    public ThemeManifest? Theme { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="SiteManifest"/> instance.
    /// </summary>
    [Parameter]
    public SiteManifest? Site { get; set; }

    /// <inheritdoc />
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        Plugin = Plugins?.SingleOrDefault(p => p.Name!.Equals(Name, StringComparison.OrdinalIgnoreCase));
    }
}
