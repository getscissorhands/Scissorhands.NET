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
    /// Gets the actual document language, which can differ from the requested locale.
    /// </summary>
    public string? ContentLocale { get; init; }

    /// <summary>
    /// Gets whether this individual document substitutes primary content for a translation.
    /// </summary>
    public bool IsFallback { get; init; }

    /// <summary>
    /// Gets the configured, plain-text notice for a fallback document.
    /// </summary>
    public string? FallbackMessage { get; init; }

    /// <summary>
    /// Gets the absolute canonical URL for an individual document, or null for a collection.
    /// </summary>
    public string? CanonicalUrl { get; init; }

    /// <summary>
    /// Gets absolute URLs for the published primary document and real translations only.
    /// </summary>
    public IReadOnlyDictionary<string, string> AlternateLanguageUrls { get; init; }
        = System.Collections.ObjectModel.ReadOnlyDictionary<string, string>.Empty;

    /// <summary>
    /// Gets generated, base-relative switch destinations keyed by locale, including fallbacks.
    /// Labels and display order belong to the theme, not these routing values.
    /// </summary>
    public IReadOnlyDictionary<string, string> SwitchLanguageUrls { get; init; }
        = System.Collections.ObjectModel.ReadOnlyDictionary<string, string>.Empty;

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
