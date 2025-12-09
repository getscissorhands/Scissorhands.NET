namespace ScissorHands.Web.Models;

public sealed class ThemeManifest
{
    public string Name { get; init; } = string.Empty;
    public string Version { get; init; } = "0.1.0";
    public string? Description { get; init; }
    public string Slug { get; init; } = string.Empty;
    public List<string> Stylesheets { get; init; } = [];
    public List<string> Scripts { get; init; } = [];
}
