namespace ScissorHands.Web.Models;

public sealed class SiteOptions
{
    public string Title { get; init; } = "ScissorHands Blog";
    public string Description { get; init; } = "A Blazor-powered static blog with live preview.";
    public string Author { get; init; } = "Your Name";
    public string Theme { get; init; } = "MinimalBlog";
    public string ContentRoot { get; init; } = "contents";
    public string Output { get; init; } = "dist";
    public string PreviewOutput { get; init; } = "preview";
    public string BaseUrl { get; init; } = "/";
    public bool IncludeDateInPostUrl { get; init; } = true;
}
