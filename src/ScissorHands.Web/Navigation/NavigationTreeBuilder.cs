using System.Globalization;

using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;
using ScissorHands.Core.Urls;

namespace ScissorHands.Web.Navigation;

/// <summary>
/// Builds theme-independent navigation from pages that have passed the engine's visibility filtering.
/// </summary>
public static class NavigationTreeBuilder
{
    /// <summary>
    /// Builds a source-ordered, read-only hierarchy with non-clickable nodes for missing ancestors.
    /// Pages without source paths follow file-backed pages in ordinal title and slug order.
    /// </summary>
    /// <param name="navigationPages">The visible pages with their resolved slugs.</param>
    /// <param name="site">Site settings used to distinguish locale prefixes from page groups.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The ordered root nodes.</returns>
    public static IReadOnlyList<NavigationNode> Build(
        IReadOnlyList<ContentDocument> navigationPages,
        SiteManifest? site = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(navigationPages);
        return BuildOrdered(PageReadingOrder.Order(navigationPages, cancellationToken: cancellationToken), site, cancellationToken);
    }

    internal static IReadOnlyList<NavigationNode> BuildOrdered(
        IReadOnlyList<ContentDocument> navigationPages,
        SiteManifest? site,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var items = new Dictionary<string, (string Title, string? Url, int Rank)>(StringComparer.Ordinal);
        for (var rank = 0; rank < navigationPages.Count; rank++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var document = navigationPages[rank];
            var path = ContentUrlHelper.GetContentUrl(document.Metadata.Slug);
            items.Add(path, (document.Metadata.Title, path, rank));
        }

        foreach (var path in items.Keys.ToArray())
        {
            var parentPath = GetParentPath(path);
            while (parentPath.Length > 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var segment = Uri.UnescapeDataString(parentPath[(parentPath.LastIndexOf('/') + 1)..]);
                var title = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(segment.Replace('-', ' ').Replace('_', ' '));
                var rank = items[path].Rank;
                if (items.TryGetValue(parentPath, out var parent))
                {
                    items[parentPath] = (parent.Title, parent.Url, Math.Min(parent.Rank, rank));
                }
                else
                {
                    items.Add(parentPath, (title, null, rank));
                }
                parentPath = GetParentPath(parentPath);
            }
        }

        if (site?.IsLocalizationEnabled == true)
        {
            foreach (var locale in site.LocalizationFallbackMessages.Keys)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var localeSegment = ContentUrlHelper.GetLocaleSegment(locale);
                if (localeSegment.Length == 0)
                {
                    continue;
                }

                var localePath = ContentUrlHelper.GetContentUrl(localeSegment);
                if (items.TryGetValue(localePath, out var item) && item.Url is null)
                {
                    items.Remove(localePath);
                }
            }
        }

        var children = items.Keys.ToLookup(path =>
        {
            var parentPath = GetParentPath(path);
            return items.ContainsKey(parentPath) ? parentPath : string.Empty;
        }, StringComparer.Ordinal);

        return BuildChildren(string.Empty);

        IReadOnlyList<NavigationNode> BuildChildren(string parentPath)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return children[parentPath]
                .OrderBy(path => items[path].Rank)
                .ThenBy(path => path, StringComparer.Ordinal)
                .Select(path => new NavigationNode
                {
                    Title = items[path].Title,
                    Path = path,
                    Url = items[path].Url,
                    Children = BuildChildren(path),
                })
                .ToList()
                .AsReadOnly();
        }
    }

    private static string GetParentPath(string path)
    {
        var separator = path.LastIndexOf('/');
        return separator < 0 ? string.Empty : path[..separator];
    }
}
