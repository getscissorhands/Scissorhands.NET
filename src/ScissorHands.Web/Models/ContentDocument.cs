namespace ScissorHands.Web.Models;

public sealed class ContentDocument
{
    public string SourcePath { get; init; } = string.Empty;
    public ContentKind Kind { get; init; }
    public ContentMetadata Metadata { get; init; } = new();
    public string Markdown { get; init; } = string.Empty;
    public string Html { get; set; } = string.Empty;
}
