namespace ScissorHands.Core.Manifests;

public sealed class ThemeManifest
{
    public const string ThemeDirectory = "themes";

    public string Name { get; init; } = string.Empty;
    public string Version { get; init; } = "1.0.0";
    public string? Description { get; init; }
    public string Slug { get; init; } = string.Empty;
    public List<string> Stylesheets { get; init; } = [];
    public List<string> Scripts { get; init; } = [];
}
