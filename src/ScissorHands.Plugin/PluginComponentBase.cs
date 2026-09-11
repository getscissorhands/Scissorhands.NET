using Microsoft.AspNetCore.Components;

using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;
using ScissorHands.Core.Validation;

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
    /// Gets or sets the required lowercase kebab-case ID of the plugin to select.
    /// </summary>
    [Parameter]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets display text. This is not used to select the plugin.
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
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        PluginIdValidator.Validate(Id, "Plugin component");
        Plugin = null;
        if (Plugins is null)
        {
            return;
        }

        var ids = new HashSet<string>(StringComparer.Ordinal);
        foreach (var manifest in Plugins)
        {
            PluginIdValidator.Validate(manifest.Id, "Configured plugin manifest");
            if (!ids.Add(manifest.Id))
            {
                throw new InvalidOperationException($"Plugin manifest ID '{manifest.Id}' is configured more than once.");
            }

            if (string.Equals(manifest.Id, Id, StringComparison.Ordinal))
            {
                Plugin = manifest;
            }
        }
    }
}
