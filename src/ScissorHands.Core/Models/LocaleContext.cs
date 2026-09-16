using ScissorHands.Core.Urls;

namespace ScissorHands.Core.Models;

/// <summary>
/// Represents the engine-prepared locale and base-relative URLs for the current render.
/// </summary>
public sealed record LocaleContext
{
    /// <summary>
    /// Gets the canonical normalized locale.
    /// </summary>
    public string Locale { get; init; } = string.Empty;

    /// <summary>
    /// Gets the resolved current raw route, including the metadata slug of synthetic pages.
    /// </summary>
    public string Route { get; init; } = string.Empty;

    /// <summary>
    /// Gets the already escaped base-relative home URL, with a trailing slash when localized.
    /// </summary>
    public string HomeUrl { get; init; } = ".";

    /// <summary>
    /// Gets the already escaped base-relative tag index URL, or null when the active locale has no tags.
    /// </summary>
    public string? TagIndexUrl { get; init; }

    /// <summary>
    /// Gets a base-relative URL for a raw tag within the current locale.
    /// </summary>
    /// <param name="tag">The raw tag name.</param>
    /// <returns>The shared tag URL prefixed with the already escaped locale home URL, when localized.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tag"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="tag"/> is empty, whitespace, <c>.</c>, or <c>..</c> after trimming.</exception>
    public string GetTagUrl(string tag)
    {
        var tagUrl = ContentUrlHelper.GetTagUrl(tag);

        return HomeUrl == "." ? tagUrl : $"{HomeUrl.TrimEnd('/')}/{tagUrl}";
    }
}
