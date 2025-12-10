namespace ScissorHands.Core.Models;

/// <summary>
/// This represents the content document entity. It can be either blog post or page.
/// </summary>
public sealed class ContentDocument
{
    /// <summary>
    /// Gets the source path of the content document.
    /// </summary>
    public string SourcePath { get; init; } = string.Empty;

    /// <summary>
    /// Gets the <see cref="ContentKind"/> value.
    /// </summary>
    public ContentKind Kind { get; init; }

    /// <summary>
    /// Gets the <see cref="ContentMetadata"/> instance.
    /// </summary>
    public ContentMetadata Metadata { get; init; } = new();

    /// <summary>
    /// Gets the markdown content.
    /// </summary>
    public string Markdown { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the HTML content.
    /// </summary>
    public string Html { get; set; } = string.Empty;
}
