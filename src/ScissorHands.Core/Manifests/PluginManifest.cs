namespace ScissorHands.Core.Manifests;

/// <summary>
/// This represents the manifest entity for plugins.
/// </summary>
public sealed class PluginManifest
{
    /// <summary>
    /// Gets the name of the plugin.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Gets the list of options for the plugin.
    /// </summary>
    public IDictionary<string, object>? Options { get; init; }
}
