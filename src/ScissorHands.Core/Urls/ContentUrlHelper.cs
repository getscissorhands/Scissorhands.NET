using ScissorHands.Core.Manifests;

namespace ScissorHands.Core.Urls;

/// <summary>
/// Provides shared URL formatting for content, theme assets, images, tags, and locales.
/// </summary>
public static class ContentUrlHelper
{
    /// <summary>
    /// Gets a base-relative URL for a content slug, escaping each path segment.
    /// </summary>
    /// <param name="slug">The content slug, with forward or backward slash separators.</param>
    /// <returns>The escaped URL relative to the site's base URL, or <c>.</c> for an empty slug.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="slug"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="slug"/> contains a literal <c>.</c> or <c>..</c> path segment.</exception>
    public static string GetContentUrl(string slug)
    {
        ArgumentNullException.ThrowIfNull(slug);

        var segments = slug.Trim().Split(['/', '\\'], StringSplitOptions.RemoveEmptyEntries);
        if (segments.Any(segment => segment is "." or ".."))
        {
            throw new ArgumentException("Content slugs cannot contain relative path segments.", nameof(slug));
        }

        return segments.Length == 0 ? "." : string.Join('/', segments.Select(Uri.EscapeDataString));
    }

    /// <summary>
    /// Gets a base-relative URL for a path within a theme.
    /// </summary>
    /// <param name="themeSlug">The theme slug, optionally surrounded by forward slashes.</param>
    /// <param name="path">The theme-relative path, optionally prefixed with forward slashes.</param>
    /// <returns>The URL under <c>themes/{themeSlug}/</c>, without additional escaping or normalization.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="themeSlug"/> or <paramref name="path"/> is null.</exception>
    public static string GetThemeUrl(string themeSlug, string path)
    {
        ArgumentNullException.ThrowIfNull(themeSlug);
        ArgumentNullException.ThrowIfNull(path);

        return $"{ThemeManifest.THEME_DIRECTORY}/{themeSlug.Trim('/')}/{path.TrimStart('/')}";
    }

    /// <summary>
    /// Gets an image URL by removing leading forward slashes only.
    /// </summary>
    /// <param name="path">The image path or URL.</param>
    /// <returns>The image URL, preserving absolute HTTP(S) URLs, query strings, fragments, and existing encoding.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="path"/> is null.</exception>
    public static string GetImageUrl(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        return path.TrimStart('/');
    }

    /// <summary>
    /// Gets a base-relative URL for a tag, trimming whitespace, lowercasing, and escaping the tag as one segment.
    /// </summary>
    /// <param name="tag">The tag name.</param>
    /// <returns>The URL under <c>tags/</c>, relative to the site's base URL.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="tag"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="tag"/> is empty, whitespace, <c>.</c>, or <c>..</c> after trimming.</exception>
    public static string GetTagUrl(string tag)
    {
        ArgumentNullException.ThrowIfNull(tag);

        var normalizedTag = tag.Trim().ToLowerInvariant();
        if (normalizedTag is "" or "." or "..")
        {
            throw new ArgumentException("Tags cannot be empty or relative path segments.", nameof(tag));
        }

        return $"tags/{Uri.EscapeDataString(normalizedTag)}";
    }

    /// <summary>
    /// Normalizes a locale for use as a content route segment.
    /// </summary>
    /// <param name="locale">The optional locale.</param>
    /// <returns>
    /// An empty string for null or whitespace; otherwise the trimmed, invariant-lowercase locale
    /// with underscores and forward slashes replaced by hyphens.
    /// </returns>
    public static string GetLocaleSegment(string? locale)
    {
        return string.IsNullOrWhiteSpace(locale)
            ? string.Empty
            : locale.Trim().Replace('_', '-').Replace('/', '-').ToLowerInvariant();
    }
}
