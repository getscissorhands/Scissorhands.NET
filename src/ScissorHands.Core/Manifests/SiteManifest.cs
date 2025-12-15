namespace ScissorHands.Core.Manifests;

public sealed class SiteManifest
{
    public string Generator { get; set; } = "ScissorHands.NET";
    public string Title { get; init; } = "ScissorHands.NET";
    public string Description { get; set; } = "A Blazor-powered static site generator.";
    public string Author { get; init; } = "The ScissorHands Team";
    public string Theme { get; init; } = string.Empty;
    public string ContentRoot { get; init; } = "contents";
    public string Output { get; init; } = "dist";
    public string PreviewOutput { get; init; } = "preview";
    public string BaseUrl { get; init; } = "/";
    public bool IncludeDateInPostUrl { get; init; } = true;
}
