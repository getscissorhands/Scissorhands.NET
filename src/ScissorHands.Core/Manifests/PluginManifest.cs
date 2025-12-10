namespace ScissorHands.Core.Manifests;

public sealed class PluginManifest
{
    public string? Assembly { get; init; }
    public string? Type { get; init; }
    public Dictionary<string, object>? Settings { get; init; }
}
