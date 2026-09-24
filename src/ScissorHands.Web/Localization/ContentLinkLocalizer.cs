using AngleSharp.Html.Parser;

using ScissorHands.Core.Urls;

namespace ScissorHands.Web.Localization;

/// <summary>
/// Localizes known document links against the same site base used by rendered HTML.
/// This is not a URL sanitizer and does not infer targets from file extensions.
/// </summary>
internal sealed class ContentLinkLocalizer
{
    private readonly Uri _baseUri;
    private readonly HashSet<string> _origins = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _targets = new(StringComparer.Ordinal);

    public ContentLinkLocalizer(string siteUrl, string baseUrl, IEnumerable<(string PrimaryRoute, string LocalizedRoute)> routes,
        string? configuredSiteUrl = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _baseUri = new Uri(siteUrl.TrimEnd('/') + baseUrl, UriKind.Absolute);
        _origins.Add(_baseUri.GetLeftPart(UriPartial.Authority));
        if (configuredSiteUrl is not null)
        {
            var configured = new Uri(configuredSiteUrl, UriKind.Absolute);
            _origins.Add(configured.GetLeftPart(UriPartial.Authority));
        }
        foreach (var (primary, localized) in routes)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var source = new Uri(_baseUri, ContentUrlHelper.GetContentUrl(primary)).AbsolutePath.TrimEnd('/');
            var target = new Uri(_baseUri, ContentUrlHelper.GetContentUrl(localized)).AbsolutePath.TrimEnd('/');
            _targets.Add(source, target);
            _targets.Add(source + "/", target + "/");
            _targets.Add(source + "/index.html", target + "/index.html");
        }
    }

    public string Localize(string html, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var parser = new HtmlParser();
        using var document = parser.ParseDocument(string.Empty);
        var body = document.Body!;
        foreach (var node in parser.ParseFragment(html, body).ToArray())
        {
            body.AppendChild(node);
        }

        var changed = false;
        foreach (var anchor in body.QuerySelectorAll("a[href]"))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (anchor.HasAttribute("download")
                || string.Equals(anchor.GetAttribute("data-localize"), "false", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }
            var href = anchor.GetAttribute("href")!;
            var localized = LocalizeUrl(href);
            if (localized != href)
            {
                anchor.SetAttribute("href", localized);
                changed = true;
            }
        }
        cancellationToken.ThrowIfCancellationRequested();
        return changed ? body.InnerHtml : html;
    }

    private string LocalizeUrl(string href)
    {
        var suffixIndex = href.IndexOfAny(['?', '#']);
        var path = suffixIndex < 0 ? href : href[..suffixIndex];
        var schemeEnd = path.IndexOf(':');
        if (schemeEnd > 0 && Uri.CheckSchemeName(path[..schemeEnd])
            && !path[(schemeEnd + 1)..].StartsWith("//", StringComparison.Ordinal))
        {
            return href;
        }
        if (path.Length == 0 || path.Any(char.IsWhiteSpace) || path.Contains('\\')
            || !Uri.TryCreate(_baseUri, path, out var target)
            || !_origins.Contains(target.GetLeftPart(UriPartial.Authority))
            || target.UserInfo.Length != 0
            || !_targets.TryGetValue(target.AbsolutePath, out var localizedPath))
        {
            return href;
        }

        var suffix = suffixIndex < 0 ? string.Empty : href[suffixIndex..];
        if (path.StartsWith("//", StringComparison.Ordinal))
        {
            var pathStart = path.IndexOf('/', 2);
            return (pathStart < 0 ? path : path[..pathStart]) + localizedPath + suffix;
        }
        if (path.StartsWith('/'))
        {
            return localizedPath + suffix;
        }
        if (Uri.TryCreate(path, UriKind.Absolute, out _))
        {
            var pathStart = path.IndexOf('/', path.IndexOf("://", StringComparison.Ordinal) + 3);
            return (pathStart < 0 ? path : path[..pathStart]) + localizedPath + suffix;
        }
        return localizedPath[_baseUri.AbsolutePath.Length..] + suffix;
    }
}
