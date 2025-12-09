namespace ScissorHands.Web.Models;

public sealed record ContentMetadata
{
    public string Title { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? Author { get; init; }
    public string? HeroImage { get; init; }
    public DateTimeOffset? Published { get; init; }
    public IEnumerable<string> Tags { get; init; } = Array.Empty<string>();
    public bool Draft { get; init; }
}
