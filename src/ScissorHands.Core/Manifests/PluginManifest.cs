using System.Collections.ObjectModel;

namespace ScissorHands.Core.Manifests;

/// <summary>
/// This represents the manifest entity for plugins.
/// </summary>
public sealed class PluginManifest
{
    private IReadOnlyDictionary<string, object?>? _options;

    /// <summary>
    /// Gets the required lowercase kebab-case plugin ID. Validated before the manifest is used.
    /// </summary>
    public string? Id { get; init; }

    /// <summary>
    /// Gets the optional display name. This does not identify the plugin.
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Gets the list of options for the plugin.
    /// </summary>
    public IReadOnlyDictionary<string, object?>? Options
    {
        get => _options;
        init => _options = value is null
            ? null
            : new ReadOnlyDictionary<string, object?>(new Dictionary<string, object?>(value, StringComparer.Ordinal));
    }
}
