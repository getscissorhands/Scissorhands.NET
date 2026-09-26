using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;

using AngleSharp.Dom;
using AngleSharp.Html.Parser;

using ScissorHands.Core.Models;

namespace ScissorHands.Web.Publication;

/// <summary>
/// Validates theme-owned publication markup, including the final output of post-HTML plugins.
/// </summary>
internal static class PublicationBadgeValidator
{
    private const string BadgeSelector = "[data-publication-badge]";
    private const string RegionSelector = "[data-publication-content], [data-publication-entry]";
    private const string ForbiddenSelector = "nav, header, footer, aside, [role='navigation'], script, style, template, noscript";

    internal static IReadOnlyDictionary<(string Route, string Kind), string> Validate(
        string html, IEnumerable<ContentDocument> documents, bool isListing, bool preview, string route,
        CancellationToken cancellationToken, IReadOnlyDictionary<(string Route, string Kind), string>? expectedLabels = null)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var expected = new Dictionary<(string Route, string Kind), DateOnly?>();
        foreach (var document in documents)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var status = document.PublicationStatus;
            if (!preview || !status.HasBadges)
            {
                continue;
            }
            if (string.IsNullOrWhiteSpace(status.Route))
            {
                throw Failure(route, "A prepared publication status has no stable document route.");
            }
            if (status.IsDraft)
            {
                expected[(status.Route, "draft")] = null;
            }
            if (status.ScheduledDate is { } date)
            {
                expected[(status.Route, "scheduled")] = date;
            }
        }

        using var parsed = new HtmlParser().ParseDocument(html);
        var badges = parsed.QuerySelectorAll(BadgeSelector);
        var regions = parsed.QuerySelectorAll(RegionSelector);
        var placement = isListing ? "listing" : "detail";
        var regionAttribute = isListing ? "data-publication-entry" : "data-publication-content";
        var seen = new HashSet<(string Route, string Kind)>();
        var labels = new Dictionary<(string Route, string Kind), string>();
        foreach (var badge in badges)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var badgeRoute = badge.GetAttribute("data-publication-route") ?? string.Empty;
            var kind = badge.GetAttribute("data-publication-badge") ?? string.Empty;
            var key = (badgeRoute, kind);
            if (!expected.TryGetValue(key, out var date) || !seen.Add(key))
            {
                throw Failure(route, $"Unexpected or duplicate '{kind}' badge for document '{badgeRoute}'.");
            }
            if (badge.GetAttribute("data-publication-placement") != placement)
            {
                throw Failure(route, $"The '{kind}' badge for document '{badgeRoute}' has the wrong placement; expected '{placement}'.");
            }
            if (badge.GetAttribute("data-publication-date") != date?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))
            {
                throw Failure(route, $"The '{kind}' badge for document '{badgeRoute}' has missing or incorrect machine-readable publication date metadata.");
            }

            var matchingRegions = regions.Where(region => region.GetAttribute(regionAttribute) == badgeRoute).ToArray();
            var region = matchingRegions.Length == 1 ? matchingRegions[0] : null;
            if (region is null || badge.ParentElement?.Closest(RegionSelector) != region
                || region.Closest("main, [role='main']") is null
                || region.Closest(ForbiddenSelector) is not null
                || badge.Closest(ForbiddenSelector) is not null
                || badge.QuerySelector("script, style, template, noscript") is not null
                || region.ParentElement?.Closest(RegionSelector) is not null
                || (!isListing && region.LocalName != "article")
                || badge.HasAttribute("data-publication-content") || badge.HasAttribute("data-publication-entry")
                || region.HasAttribute(isListing ? "data-publication-content" : "data-publication-entry"))
            {
                throw Failure(route, $"The '{kind}' badge for document '{badgeRoute}' must belong to its unique '{regionAttribute}' region inside main content, outside navigation.");
            }
            var text = GetExposedText(badge, cancellationToken).Trim();
            if (text.Length == 0 || (expectedLabels is not null
                && (!expectedLabels.TryGetValue(key, out var label) || text != label.Trim())))
            {
                throw Failure(route, $"The '{kind}' badge for document '{badgeRoute}' must expose the complete nonempty theme-provided label without hidden or altered required text.");
            }
            labels.Add(key, text);
        }

        foreach (var key in expected.Keys)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!seen.Contains(key))
            {
                throw Failure(route, $"Missing '{key.Kind}' badge for document '{key.Route}'.");
            }
        }
        if (!isListing && expected.Count > 0)
        {
            foreach (var documentRoute in expected.Keys.Select(key => key.Route).Distinct(StringComparer.Ordinal))
            {
                cancellationToken.ThrowIfCancellationRequested();
                var region = regions.Single(element => element.GetAttribute(regionAttribute) == documentRoute);
                var count = expected.Keys.Count(key => key.Route == documentRoute);
                if (!BeginsWithBadges(region, count, cancellationToken))
                {
                    throw Failure(route, $"Publication badges for document '{documentRoute}' must appear at the beginning of its article, before headings, images, dates, or authored content.");
                }
            }
        }
        return new ReadOnlyDictionary<(string Route, string Kind), string>(labels);
    }

    private static bool BeginsWithBadges(IElement region, int requiredCount, CancellationToken cancellationToken)
    {
        var pending = new Stack<INode>(region.ChildNodes.Reverse());
        var count = 0;
        while (pending.TryPop(out var node))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (node is IText text && !string.IsNullOrWhiteSpace(text.Data))
            {
                return false;
            }
            if (node is not IElement element || IsHidden(element))
            {
                continue;
            }
            if (element.HasAttribute("data-publication-badge"))
            {
                if (++count == requiredCount)
                {
                    return true;
                }
                continue;
            }
            // Themes may wrap or format the initial badge group, but not put article content before it.
            if (element.LocalName is not ("div" or "span" or "section" or "p" or "strong" or "b" or "em" or "i"))
            {
                return false;
            }
            foreach (var child in element.ChildNodes.Reverse())
            {
                pending.Push(child);
            }
        }
        return false;
    }

    private static string GetExposedText(IElement badge, CancellationToken cancellationToken)
    {
        for (var ancestor = badge.ParentElement; ancestor is not null; ancestor = ancestor.ParentElement)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (IsHidden(ancestor))
            {
                return string.Empty;
            }
        }

        var result = new StringBuilder();
        var pending = new Stack<INode>();
        pending.Push(badge);
        while (pending.TryPop(out var node))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (node is IElement element && IsHidden(element))
            {
                continue;
            }
            if (node is IText text)
            {
                result.Append(text.Data);
            }
            else
            {
                foreach (var child in node.ChildNodes.Reverse())
                {
                    pending.Push(child);
                }
            }
        }
        return result.ToString();
    }

    private static bool IsHidden(IElement element) =>
        element.HasAttribute("hidden") || element.HasAttribute("inert")
        || string.Equals(element.GetAttribute("aria-hidden")?.Trim(), "true", StringComparison.OrdinalIgnoreCase);

    private static InvalidDataException Failure(string route, string reason) => new(
        $"Publication badge validation failed for rendered route '{route}': {reason} "
        + "Update the theme to render every PublicationBadgeBase badge using RenderContent(themeLabel) and its Attributes "
        + "inside GetRegionAttributes(document, placement), and ensure post-HTML plugins preserve the badges and their placement.");
}
