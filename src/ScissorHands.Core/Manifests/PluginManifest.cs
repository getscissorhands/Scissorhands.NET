namespace ScissorHands.Core.Manifests;

public sealed class PluginManifest
{
    public string? Name { get; init; }
    public IDictionary<string, object>? Options { get; init; }
}
