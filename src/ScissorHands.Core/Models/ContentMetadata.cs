namespace ScissorHands.Core.Models;

/// <summary>
/// This represents the metadata record of a content document.
/// </summary>
public sealed record ContentMetadata
{
    /// <summary>
    /// Gets the title of the content.
    /// </summary>
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// Gets the slug of the content.
    /// </summary>
    public string Slug { get; init; } = string.Empty;

    /// <summary>
    /// Gets the description of the content.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets the author of the content.
    /// </summary>
    public string? Author { get; init; }

    /// <summary>
    /// Gets the hero image path of the content.
    /// </summary>
    public string? HeroImage { get; init; }

    /// <summary>
    /// Gets the published date of the content.
    /// </summary>
    public DateTimeOffset? Published { get; init; }

    /// <summary>
    /// Gets the tags of the content.
    /// </summary>
    public IEnumerable<string> Tags { get; init; } = [];

    /// <summary>
    /// Gets a value indicating whether the content is a draft or not.
    /// </summary>
    public bool Draft { get; init; }
}
