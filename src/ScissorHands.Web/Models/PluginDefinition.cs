namespace ScissorHands.Web.Models;

public sealed class PluginDefinition
{
    public string? Assembly { get; init; }
    public string? Type { get; init; }
    public Dictionary<string, object>? Settings { get; init; }
}
