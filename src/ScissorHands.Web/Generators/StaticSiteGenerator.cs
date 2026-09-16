using System.IO.Abstractions;
using System.Text;
using System.Text.Encodings.Web;

using Microsoft.Extensions.Logging;

using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;
using ScissorHands.Core.Services;
using ScissorHands.Core.Urls;
using ScissorHands.Web.Abstractions;
using ScissorHands.Web.Loaders;
using ScissorHands.Web.Navigation;
using ScissorHands.Web.Renderers;
using ScissorHands.Web.Runners;

namespace ScissorHands.Web.Generators;

/// <summary>
/// This represents the static site generator entity.
/// </summary>
/// <param name="contentLoader"><see cref="IContentLoader"/> instance.</param>
/// <param name="markdownService"><see cref="IMarkdownService"/> instance.</param>
/// <param name="pluginRunner"><see cref="IPluginRunner"/> instance.</param>
/// <param name="themeService"><see cref="IThemeService"/> instance.</param>
/// <param name="renderer"><see cref="IComponentRenderer"/> instance.</param>
/// <param name="options"><see cref="SiteManifest"/> instance.</param>
/// <param name="logger"><see cref="ILogger{T}"/> instance.</param>
public sealed class StaticSiteGenerator(
        IContentLoader contentLoader,
        IMarkdownService markdownService,
        IPluginRunner pluginRunner,
        IThemeService themeService,
        IComponentRenderer renderer,
        IAppPaths paths,
        IFileSystem fileSystem,
        SiteManifest options,
        ILogger<StaticSiteGenerator> logger) : IStaticSiteGenerator
{
    private const string PAGE_NOT_FOUND_SLUG = "404.html";

    private readonly IContentLoader _contentLoader = contentLoader ?? throw new ArgumentNullException(nameof(contentLoader));
    private readonly IMarkdownService _markdownService = markdownService ?? throw new ArgumentNullException(nameof(markdownService));
    private readonly IPluginRunner _pluginRunner = pluginRunner ?? throw new ArgumentNullException(nameof(pluginRunner));
    private readonly IThemeService _themeService = themeService ?? throw new ArgumentNullException(nameof(themeService));
    private readonly IComponentRenderer _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
    private readonly IAppPaths _paths = paths ?? throw new ArgumentNullException(nameof(paths));
    private readonly IFileSystem _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    private readonly SiteManifest _options = options ?? throw new ArgumentNullException(nameof(options));
    private readonly ILogger<StaticSiteGenerator> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <inheritdoc />
    public async Task BuildAsync<TMainLayout, TIndexView, TPostView, TPageView, TNotFoundView, TTagListView, TTagView>(string destination, bool preview, CancellationToken cancellationToken)
        where TMainLayout : ScissorHands.Theme.MainLayoutBase
        where TIndexView : ScissorHands.Theme.IndexViewBase
        where TPostView : ScissorHands.Theme.PostViewBase
        where TPageView : ScissorHands.Theme.PageViewBase
        where TNotFoundView : ScissorHands.Theme.NotFoundViewBase
        where TTagListView : ScissorHands.Theme.TagListViewBase
        where TTagView : ScissorHands.Theme.TagViewBase
    {
        cancellationToken.ThrowIfCancellationRequested();
        _fileSystem.Directory.CreateDirectory(destination);
        _logger.LogInformation("Starting static site build to {Destination} (preview: {Preview})", destination, preview);

        _options.IsPreview = preview;
        _options.DescriptionInHtml = await _markdownService.ToHtmlAsync(_options.Description, trim: true, cancellationToken: cancellationToken);
        var plugins = _pluginRunner.Manifests;
        var theme = await _themeService.LoadManifestAsync(_options.Theme, cancellationToken);
        var documents = (await _contentLoader.LoadAsync(cancellationToken)).ToList();
        cancellationToken.ThrowIfCancellationRequested();
        var outputs = new OutputRoutes(destination, _fileSystem, _options.UseLocaleInUrl);
        var scopes = CreateScopes(documents, outputs, cancellationToken);
        var defaultScope = scopes[0];
        var notFoundDocument = documents.SingleOrDefault(IsNotFoundPage);
        var notFoundOwner = (object?)notFoundDocument ?? new object();
        if (_options.UseLocaleInUrl && notFoundDocument is not null
            && ResolveLocale(notFoundDocument.Metadata.Locale, $"locale in '{notFoundDocument.SourcePath}'") != defaultScope.Locale)
        {
            throw new InvalidDataException(
                $"The locale in '{notFoundDocument.SourcePath}' for the shared 404.html must match Site.Locale.");
        }

        var redirects = CreateRedirects(defaultScope);
        ValidateOutputRoutes(destination, scopes, redirects, notFoundOwner);
        var documentScopes = new Dictionary<ContentDocument, GenerationScope>(ReferenceEqualityComparer.Instance);
        var layoutType = typeof(TMainLayout);
        foreach (var scope in scopes)
        {
            cancellationToken.ThrowIfCancellationRequested();
            foreach (var document in scope.Documents)
            {
                documentScopes.Add(document, scope);
            }

            await RenderIndexAsync<TIndexView>(scope, plugins, theme, destination, layoutType, cancellationToken);
        }

        await RenderNotFoundAsync<TNotFoundView>(notFoundDocument, notFoundOwner, defaultScope, plugins, theme, destination, layoutType, cancellationToken);

        foreach (var document in documents.Where(d => !IsNotFoundPage(d) && documentScopes.ContainsKey(d)))
        {
            await RenderDocumentAsync<TPostView, TPageView>(document, documentScopes[document], plugins, theme, destination, layoutType, cancellationToken);
        }

        foreach (var scope in scopes)
        {
            await RenderTagPagesAsync<TTagListView, TTagView>(scope, plugins, theme, destination, layoutType, cancellationToken);
        }

        foreach (var redirect in redirects)
        {
            await RenderRedirectAsync(redirect, destination, outputs, cancellationToken);
        }

        CopyContentAssets(destination, outputs);
        await _themeService.CopyAssetsAsync(_options.Theme, destination, cancellationToken);
    }

    private IReadOnlyList<GenerationScope> CreateScopes(
        IReadOnlyList<ContentDocument> documents, OutputRoutes outputs, CancellationToken cancellationToken)
    {
        if (!_options.UseLocaleInUrl)
        {
            return [CreateScope(null, documents, outputs, cancellationToken)];
        }

        ValidateRedirectBaseUrl();
        var defaultLocale = ResolveLocale(_options.Locale, "Site.Locale");
        var groups = new Dictionary<string, List<ContentDocument>>(StringComparer.Ordinal)
        {
            [defaultLocale] = [],
        };
        foreach (var document in documents.Where(d => !d.Metadata.Draft && !IsNotFoundPage(d)))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var locale = ResolveLocale(document.Metadata.Locale, $"locale in '{document.SourcePath}'");
            if (!groups.TryGetValue(locale, out var group))
            {
                group = [];
                groups.Add(locale, group);
            }
            group.Add(document);
        }

        var scopes = new List<GenerationScope>
        {
            CreateScope(defaultLocale, groups[defaultLocale], outputs, cancellationToken),
        };
        foreach (var locale in groups.Keys.Where(key => key != defaultLocale).Order(StringComparer.Ordinal))
        {
            scopes.Add(CreateScope(locale, groups[locale], outputs, cancellationToken));
        }
        return scopes;
    }

    private GenerationScope CreateScope(
        string? locale, IReadOnlyList<ContentDocument> documents, OutputRoutes outputs, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var hiddenPageRoutes = documents
            .Where(d => d.Kind == ContentKind.Page && (!d.Metadata.ShowInNavigation || d.Metadata.Draft || IsNotFoundPage(d)))
            .Select(d => NormalizeRoute(d.Metadata.Slug))
            .ToHashSet(StringComparer.Ordinal);
        var visiblePages = documents
            .Where(d => d.Kind == ContentKind.Page && d.Metadata.ShowInNavigation && !d.Metadata.Draft && !IsNotFoundPage(d))
            .Where(d => !HasHiddenNavigationAncestor(d.Metadata.Slug, hiddenPageRoutes))
            .ToList();
        var navigationPages = PageReadingOrder.Order(
            visiblePages, _fileSystem.Path.Combine(_paths.GetContentsRoot(), "pages"), cancellationToken);
        var navigation = new NavigationContext(
            navigationPages,
            NavigationTreeBuilder.BuildOrdered(navigationPages, _options, cancellationToken),
            CreatePageNavigation(navigationPages, cancellationToken));

        var tags = new List<TagGroup>();
        foreach (var group in documents.Where(d => !IsNotFoundPage(d))
            .SelectMany(d => d.Metadata.Tags.Select(tag => (Tag: tag.ToLowerInvariant(), Document: d)))
            .GroupBy(item => item.Tag))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var route = PrefixLocale(locale, GetTagRoute(group.Key));
            tags.Add(new TagGroup(
                group.Key,
                CreateRouteDocument(route, locale),
                group.Where(item => item.Document.Kind == ContentKind.Post).Select(item => item.Document)
                    .OrderByDescending(d => d.Metadata.Published ?? DateTimeOffset.MinValue).ToList().AsReadOnly(),
                group.Where(item => item.Document.Kind == ContentKind.Page).Select(item => item.Document)
                    .OrderBy(d => d.Metadata.Title).ToList().AsReadOnly()));
        }

        return new GenerationScope(locale, documents, navigation, tags, outputs,
            CreateRouteDocument(locale ?? string.Empty, locale),
            CreateRouteDocument(PrefixLocale(locale, "tags"), locale));
    }

    private static ContentDocument CreateRouteDocument(string route, string? locale) => new()
    {
        Kind = ContentKind.Page,
        Metadata = new ContentMetadata { Slug = route, Locale = locale },
    };

    private static string PrefixLocale(string? locale, string route)
        => locale is null ? route : $"{locale}/{route}";

    private string ResolveLocale(string? locale, string source)
    {
        var normalized = ContentUrlHelper.GetLocaleSegment(string.IsNullOrWhiteSpace(locale) ? _options.Locale : locale);
        if (normalized.Length == 0 || normalized is "." or ".."
            || normalized.IndexOfAny(['\\', ':', '*', '?', '"', '<', '>', '|']) >= 0
            || normalized.Any(char.IsControl) || normalized.EndsWith('.'))
        {
            throw new InvalidDataException($"Invalid {source}: a non-empty, safe locale route segment is required.");
        }
        return normalized;
    }

    private void ValidateRedirectBaseUrl()
    {
        var baseUrl = _options.BaseUrl;
        if (string.IsNullOrEmpty(baseUrl) || !baseUrl.StartsWith('/') || baseUrl.StartsWith("//", StringComparison.Ordinal)
            || !baseUrl.EndsWith('/') || baseUrl.IndexOfAny(['\\', '?', '#']) >= 0
            || baseUrl.Any(char.IsWhiteSpace) || baseUrl.Any(char.IsControl))
        {
            throw new InvalidDataException("Site.BaseUrl must be a rooted path ending in '/' (for example '/blog/') for locale redirects.");
        }
        foreach (var segment in baseUrl.Split('/', StringSplitOptions.RemoveEmptyEntries))
        {
            for (var index = 0; index < segment.Length; index++)
            {
                if (segment[index] == '%'
                    && (index + 2 >= segment.Length || !Uri.IsHexDigit(segment[index + 1]) || !Uri.IsHexDigit(segment[index + 2])))
                {
                    throw new InvalidDataException("Site.BaseUrl contains an invalid percent escape for locale redirects.");
                }
            }
            var decoded = Uri.UnescapeDataString(segment);
            if (decoded is "." or ".." || decoded.IndexOfAny(['/', '\\', '?', '#']) >= 0 || decoded.Any(char.IsControl))
            {
                throw new InvalidDataException("Site.BaseUrl contains an unsafe path segment for locale redirects.");
            }
        }
    }

    private static IReadOnlyList<RedirectPage> CreateRedirects(GenerationScope scope)
    {
        if (scope.Locale is null)
        {
            return [];
        }

        var context = scope.CreateLocaleContext(string.Empty)!;
        var redirects = new List<RedirectPage>
        {
            new(CreateRouteDocument(string.Empty, scope.Locale), scope.IndexDocument, context.HomeUrl),
        };
        if (scope.Tags.Count > 0)
        {
            redirects.Add(new(CreateRouteDocument("tags", scope.Locale), scope.TagIndexDocument, context.TagIndexUrl! + "/"));
            redirects.AddRange(scope.Tags.Select(tag => new RedirectPage(
                CreateRouteDocument(GetTagRoute(tag.Tag), scope.Locale), tag.Document, context.GetTagUrl(tag.Tag) + "/")));
        }
        return redirects;
    }

    private static bool HasHiddenNavigationAncestor(string slug, HashSet<string> hiddenPageRoutes)
    {
        var route = NormalizeRoute(slug);
        var separator = route.LastIndexOf('/');
        while (separator >= 0)
        {
            route = route[..separator];
            if (hiddenPageRoutes.Contains(route))
            {
                return true;
            }

            separator = route.LastIndexOf('/');
        }

        return false;
    }

    private async Task RenderIndexAsync<TIndexView>(GenerationScope scope, IEnumerable<PluginManifest> plugins, ThemeManifest theme, string destination, Type layoutType, CancellationToken cancellationToken)
        where TIndexView : ScissorHands.Theme.IndexViewBase
    {
        var posts = scope.Documents
            .Where(d => d.Kind == ContentKind.Post)
            .OrderByDescending(d => d.Metadata.Published ?? DateTimeOffset.MinValue)
            .ToList();

        var parameters = CreateBaseParameters(plugins, theme, scope, scope.IndexDocument.Metadata.Slug);
        parameters["Documents"] = posts;
        if (scope.Locale is not null)
        {
            parameters["Document"] = scope.IndexDocument;
        }

        var rendered = await _renderer.RenderAsync<TIndexView>(layoutType, parameters, cancellationToken);
        var indexDocument = new ContentDocument
        {
            Kind = ContentKind.Page,
            Metadata = scope.IndexDocument.Metadata with { Title = _options.Title },
            Markdown = string.Empty,
            Html = rendered
        };

        var outputPath = ResolveOutputPath(destination, scope.IndexDocument.Metadata.Slug);
        await WriteRenderedHtmlAsync(outputPath, rendered, indexDocument, scope.Outputs, scope.IndexDocument, cancellationToken);
    }

    private async Task RenderNotFoundAsync<TNotFoundView>(ContentDocument? notFoundDocument, object owner, GenerationScope scope, IEnumerable<PluginManifest> plugins, ThemeManifest theme, string destination, Type layoutType, CancellationToken cancellationToken)
        where TNotFoundView : ScissorHands.Theme.NotFoundViewBase
    {
        ContentDocument documentToRender;
        if (notFoundDocument is null)
        {
            documentToRender = new ContentDocument
            {
                Kind = ContentKind.Page,
                Metadata = new ContentMetadata { Title = "404 - Not Found", Description = "Page not found", Slug = PAGE_NOT_FOUND_SLUG, Locale = scope.Locale },
                Markdown = string.Empty,
                Html = string.Empty
            };
        }
        else
        {
            documentToRender = await ConvertMarkdownToHtmlAsync(notFoundDocument, cancellationToken);

            if (string.IsNullOrWhiteSpace(documentToRender.Metadata.Title))
            {
                documentToRender = new ContentDocument
                {
                    SourcePath = documentToRender.SourcePath,
                    Kind = documentToRender.Kind,
                    Metadata = documentToRender.Metadata with { Title = "404 - Not Found" },
                    Markdown = documentToRender.Markdown,
                    Html = documentToRender.Html
                };
            }
        }

        var parameters = CreateBaseParameters(plugins, theme, scope, PAGE_NOT_FOUND_SLUG);
        parameters["Document"] = documentToRender;

        var rendered = await _renderer.RenderAsync<TNotFoundView>(layoutType, parameters, cancellationToken);

        var outputPath = _fileSystem.Path.Combine(destination, PAGE_NOT_FOUND_SLUG);
        await WriteRenderedHtmlAsync(outputPath, rendered, documentToRender, scope.Outputs, owner, cancellationToken);
    }

    private async Task RenderDocumentAsync<TPostView, TPageView>(ContentDocument document, GenerationScope scope, IEnumerable<PluginManifest> plugins, ThemeManifest theme, string destination, Type layoutType, CancellationToken cancellationToken)
        where TPostView : ScissorHands.Theme.PostViewBase
        where TPageView : ScissorHands.Theme.PageViewBase
    {
        cancellationToken.ThrowIfCancellationRequested();
        scope.Navigation.AdjacentPages.TryGetValue(document, out var pageNavigation);

        var postMarkdown = await ConvertMarkdownToHtmlAsync(document, cancellationToken);

        var parameters = CreateBaseParameters(plugins, theme, scope, document.Metadata.Slug);
        parameters["Document"] = postMarkdown;
        if (postMarkdown.Kind == ContentKind.Page && pageNavigation is not null)
        {
            parameters["PageNavigation"] = pageNavigation;
        }

        var rendered = postMarkdown.Kind switch
        {
            ContentKind.Page => await _renderer.RenderAsync<TPageView>(layoutType, parameters, cancellationToken),
            _ => await _renderer.RenderAsync<TPostView>(layoutType, parameters, cancellationToken)
        };
        var outputPath = ResolveOutputPath(destination, postMarkdown.Metadata.Slug);

        await WriteRenderedHtmlAsync(outputPath, rendered, postMarkdown, scope.Outputs, document, cancellationToken);
    }

    private async Task RenderTagPagesAsync<TTagListView, TTagView>(GenerationScope scope, IEnumerable<PluginManifest> plugins, ThemeManifest theme, string destination, Type layoutType, CancellationToken cancellationToken)
        where TTagListView : ScissorHands.Theme.TagListViewBase
        where TTagView : ScissorHands.Theme.TagViewBase
    {
        if (scope.Tags.Count == 0)
        {
            _logger.LogInformation("No tags found in content documents; skipping tag pages");
            return;
        }

        var taggedDocuments = scope.Tags.ToDictionary(
            tag => tag.Tag,
            tag => ((IEnumerable<ContentDocument>)tag.Posts, (IEnumerable<ContentDocument>)tag.Pages));
        await RenderTagListPageAsync<TTagListView>(taggedDocuments, scope, plugins, theme, destination, layoutType, cancellationToken);

        foreach (var tag in scope.Tags)
        {
            await RenderTagPageAsync<TTagView>(tag, scope, plugins, theme, destination, layoutType, cancellationToken);
        }
    }

    private async Task RenderTagListPageAsync<TTagListView>(IDictionary<string, (IEnumerable<ContentDocument> Posts, IEnumerable<ContentDocument> Pages)> taggedDocuments, GenerationScope scope, IEnumerable<PluginManifest> plugins, ThemeManifest theme, string destination, Type layoutType, CancellationToken cancellationToken)
        where TTagListView : ScissorHands.Theme.TagListViewBase
    {
        var routeDocument = scope.TagIndexDocument;
        var parameters = CreateBaseParameters(plugins, theme, scope, routeDocument.Metadata.Slug);
        parameters["Document"] = routeDocument;
        parameters["TaggedDocuments"] = taggedDocuments;

        var rendered = await _renderer.RenderAsync<TTagListView>(layoutType, parameters, cancellationToken);
        var tagListDocument = new ContentDocument
        {
            Kind = ContentKind.Page,
            Metadata = routeDocument.Metadata with { Title = "Tags" },
            Markdown = string.Empty,
            Html = rendered
        };
        var outputPath = ResolveOutputPath(destination, routeDocument.Metadata.Slug);
        await WriteRenderedHtmlAsync(outputPath, rendered, tagListDocument, scope.Outputs, routeDocument, cancellationToken);
    }

    private async Task RenderTagPageAsync<TTagView>(TagGroup tag, GenerationScope scope, IEnumerable<PluginManifest> plugins, ThemeManifest theme, string destination, Type layoutType, CancellationToken cancellationToken)
        where TTagView : ScissorHands.Theme.TagViewBase
    {
        var routeDocument = tag.Document;
        var tagRoute = routeDocument.Metadata.Slug;
        var parameters = CreateBaseParameters(plugins, theme, scope, tagRoute);
        parameters["Document"] = routeDocument;
        parameters["Tag"] = tag.Tag;
        parameters["TaggedPosts"] = tag.Posts;
        parameters["TaggedPages"] = tag.Pages;

        var rendered = await _renderer.RenderAsync<TTagView>(layoutType, parameters, cancellationToken);
        var tagDocument = new ContentDocument
        {
            Kind = ContentKind.Page,
            Metadata = routeDocument.Metadata with { Title = $"Tag: {tag.Tag}" },
            Markdown = string.Empty,
            Html = rendered
        };
        var outputPath = ResolveOutputPath(destination, tagRoute);
        await WriteRenderedHtmlAsync(outputPath, rendered, tagDocument, scope.Outputs, routeDocument, cancellationToken);
    }

    private Dictionary<string, object?> CreateBaseParameters(IEnumerable<PluginManifest> plugins, ThemeManifest theme, GenerationScope scope, string route)
    {
        var parameters = new Dictionary<string, object?>
        {
            ["Plugins"] = plugins,
            ["Theme"] = theme,
            ["Site"] = _options,
            ["NavigationPages"] = scope.Navigation.Pages,
            ["NavigationTree"] = scope.Navigation.Tree,
            ["PageNavigation"] = new PageNavigation(),
        };
        if (scope.Locale is not null)
        {
            parameters["LocaleContext"] = scope.CreateLocaleContext(route);
        }
        return parameters;
    }

    private static IReadOnlyDictionary<ContentDocument, PageNavigation> CreatePageNavigation(
        IReadOnlyList<ContentDocument> pages,
        CancellationToken cancellationToken)
    {
        var links = new List<PageNavigationLink>(pages.Count);
        foreach (var page in pages)
        {
            cancellationToken.ThrowIfCancellationRequested();
            links.Add(new PageNavigationLink
            {
                Title = page.Metadata.Title,
                Url = ContentUrlHelper.GetContentUrl(page.Metadata.Slug),
            });
        }

        var result = new Dictionary<ContentDocument, PageNavigation>(ReferenceEqualityComparer.Instance);
        for (var index = 0; index < pages.Count; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            result.Add(pages[index], new PageNavigation
            {
                Previous = index == 0 ? null : links[index - 1],
                Next = index + 1 == links.Count ? null : links[index + 1],
            });
        }

        return result;
    }

    private sealed record NavigationContext(
        IReadOnlyList<ContentDocument> Pages,
        IReadOnlyList<NavigationNode> Tree,
        IReadOnlyDictionary<ContentDocument, PageNavigation> AdjacentPages);

    private sealed record TagGroup(
        string Tag, ContentDocument Document, IReadOnlyList<ContentDocument> Posts, IReadOnlyList<ContentDocument> Pages);

    private sealed record RedirectPage(ContentDocument Document, ContentDocument Target, string TargetUrl);

    private sealed record GenerationScope(
        string? Locale,
        IReadOnlyList<ContentDocument> Documents,
        NavigationContext Navigation,
        IReadOnlyList<TagGroup> Tags,
        OutputRoutes Outputs,
        ContentDocument IndexDocument,
        ContentDocument TagIndexDocument)
    {
        public LocaleContext? CreateLocaleContext(string route)
        {
            if (Locale is null)
            {
                return null;
            }
            var homeUrl = ContentUrlHelper.GetContentUrl(Locale) + "/";
            return new LocaleContext
            {
                Locale = Locale,
                Route = route,
                HomeUrl = homeUrl,
                TagIndexUrl = Tags.Count == 0 ? null : homeUrl + "tags",
            };
        }
    }

    private async Task RenderRedirectAsync(RedirectPage redirect, string destination, OutputRoutes outputs, CancellationToken cancellationToken)
    {
        var target = _options.BaseUrl + redirect.TargetUrl;
        var encodedTarget = HtmlEncoder.Default.Encode(target);
        var encodedLocale = HtmlEncoder.Default.Encode(redirect.Document.Metadata.Locale!);
        var html = $"""
            <!DOCTYPE html>
            <html lang="{encodedLocale}">
            <head>
                <meta charset="utf-8">
                <meta http-equiv="refresh" content="0;url={encodedTarget}">
                <title>Redirect</title>
            </head>
            <body><p><a href="{encodedTarget}">Continue</a></p></body>
            </html>
            """;
        var document = new ContentDocument
        {
            Kind = ContentKind.Page,
            Metadata = redirect.Document.Metadata with { Title = "Redirect" },
            Html = html,
        };
        await WriteRenderedHtmlAsync(ResolveOutputPath(destination, document.Metadata.Slug),
            html, document, outputs, redirect.Document, cancellationToken);
    }

    private async Task WriteRenderedHtmlAsync(string outputPath, string renderedHtml, ContentDocument document, OutputRoutes outputs, object owner, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        outputs.Claim(outputPath, owner, $"rendered route '{document.Metadata.Slug}'");
        var finalHtml = await _pluginRunner.RunPostHtmlAsync(renderedHtml, document, cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();
        outputs.ValidateLinks(outputPath);
        _fileSystem.Directory.CreateDirectory(_fileSystem.Path.GetDirectoryName(outputPath)!);
        await _fileSystem.File.WriteAllTextAsync(outputPath, finalHtml, Encoding.UTF8, cancellationToken);
        _logger.LogInformation("Wrote {OutputPath}", outputPath);
    }

    private async Task<ContentDocument> ConvertMarkdownToHtmlAsync(ContentDocument document, CancellationToken cancellationToken)
    {
        var preProcessed = await _pluginRunner.RunPreMarkdownAsync(document, cancellationToken);
        var html = await _markdownService.ToHtmlAsync(preProcessed.Markdown, cancellationToken: cancellationToken);
        preProcessed.Html = html;

        return await _pluginRunner.RunPostMarkdownAsync(preProcessed, cancellationToken);
    }

    private void CopyContentAssets(string destination, OutputRoutes outputs)
    {
        var source = _fileSystem.Path.Combine(_paths.GetContentsRoot(), "images");
        var target = _fileSystem.Path.Combine(destination, "images");

        if (!_fileSystem.Directory.Exists(source))
        {
            _logger.LogWarning("No content images found at {Path}", source);
            return;
        }

        CopyDirectory(source, target, outputs);
    }

    private void CopyDirectory(string sourceDir, string destinationDir, OutputRoutes outputs)
    {
        outputs.ValidateLinks(destinationDir);
        _fileSystem.Directory.CreateDirectory(destinationDir);

        foreach (var file in _fileSystem.Directory.GetFiles(sourceDir, "*", SearchOption.TopDirectoryOnly))
        {
            var destFile = _fileSystem.Path.Combine(destinationDir, _fileSystem.Path.GetFileName(file));
            if (_options.UseLocaleInUrl)
            {
                outputs.Claim(destFile, new object(), $"content asset '{file}'");
            }
            _fileSystem.File.Copy(file, destFile, overwrite: true);
        }

        foreach (var directory in _fileSystem.Directory.GetDirectories(sourceDir, "*", SearchOption.TopDirectoryOnly))
        {
            var name = _fileSystem.Path.GetFileName(directory);
            CopyDirectory(directory, _fileSystem.Path.Combine(destinationDir, name), outputs);
        }
    }

    private static bool IsNotFoundPage(ContentDocument document)
    {
        return document.Kind == ContentKind.Page &&
               string.Equals(document.Metadata.Slug, PAGE_NOT_FOUND_SLUG, StringComparison.OrdinalIgnoreCase) == true;
    }

    private static string ResolveOutputPath(string root, string slug)
    {
        var fullRoot = Path.GetFullPath(root);
        if (string.IsNullOrWhiteSpace(slug))
        {
            return Path.Combine(fullRoot, "index.html");
        }

        var safeSlug = NormalizeRoute(slug);
        var outputPath = Path.GetFullPath(Path.Combine(fullRoot, safeSlug.Replace('/', Path.DirectorySeparatorChar), "index.html"));
        var rootPrefix = fullRoot.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        if (!outputPath.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidDataException($"Route '{slug}' resolves outside the output directory.");
        }

        return outputPath;
    }

    private static void ValidateOutputRoutes(
        string destination, IReadOnlyList<GenerationScope> scopes, IReadOnlyList<RedirectPage> redirects, object notFoundOwner)
    {
        var outputs = scopes[0].Outputs;
        outputs.Plan(Path.Combine(Path.GetFullPath(destination), PAGE_NOT_FOUND_SLUG), notFoundOwner, "not-found page");
        foreach (var scope in scopes)
        {
            AddRoute(scope.IndexDocument, "site index");
            foreach (var document in scope.Documents.Where(d => !IsNotFoundPage(d)))
            {
                AddRoute(document, string.IsNullOrEmpty(document.SourcePath) ? $"content '{document.Metadata.Slug}'" : document.SourcePath);
            }
            if (scope.Tags.Count > 0)
            {
                AddRoute(scope.TagIndexDocument, "tag index");
                foreach (var tag in scope.Tags)
                {
                    AddRoute(tag.Document, $"tag '{tag.Tag}'");
                }
            }
        }

        foreach (var redirect in redirects)
        {
            var redirectPath = ResolveOutputPath(destination, redirect.Document.Metadata.Slug);
            var targetPath = ResolveOutputPath(destination, redirect.Target.Metadata.Slug);
            if (redirectPath.Equals(targetPath, StringComparison.OrdinalIgnoreCase) || !outputs.Contains(targetPath))
            {
                throw new InvalidDataException($"Invalid redirect route '{redirect.Document.Metadata.Slug}': its target must be a different generated page.");
            }
            AddRoute(redirect.Document, $"redirect '{redirect.Document.Metadata.Slug}'");
        }

        void AddRoute(ContentDocument document, string description)
            => outputs.Plan(ResolveOutputPath(destination, document.Metadata.Slug), document, description);
    }

    private sealed class OutputRoutes(string root, IFileSystem fileSystem, bool validateLinks)
    {
        private readonly Dictionary<string, (object Owner, string Description)> _routes = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _written = new(StringComparer.OrdinalIgnoreCase);
        private readonly string _root = Path.GetFullPath(root);

        public bool Contains(string path) => _routes.ContainsKey(Path.GetFullPath(path));

        public void Plan(string outputPath, object owner, string description)
            => Register(outputPath, owner, description, false);

        public void Claim(string outputPath, object owner, string description)
        {
            var fullPath = Path.GetFullPath(outputPath);
            Register(fullPath, owner, description, true);
            if (!_written.Add(fullPath))
            {
                throw new InvalidDataException($"Output collision at '{fullPath}': the destination was already written.");
            }
        }

        private void Register(string outputPath, object owner, string description, bool existingClaim)
        {
            var fullPath = Path.GetFullPath(outputPath);
            ValidateLinks(fullPath);
            if (_routes.TryGetValue(fullPath, out var existing))
            {
                if (existingClaim && ReferenceEquals(owner, existing.Owner))
                {
                    return;
                }
                throw new InvalidDataException(
                    $"Output collision at '{fullPath}' between '{existing.Description}' and '{description}'.");
            }

            var separator = Path.DirectorySeparatorChar.ToString();
            var conflictingRoute = _routes.FirstOrDefault(route =>
                fullPath.StartsWith(route.Key + separator, StringComparison.OrdinalIgnoreCase)
                || route.Key.StartsWith(fullPath + separator, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrEmpty(conflictingRoute.Key))
            {
                throw new InvalidDataException(
                    $"Output collision between file '{conflictingRoute.Key}' from '{conflictingRoute.Value.Description}' and '{fullPath}' from '{description}'.");
            }

            _routes.Add(fullPath, (owner, description));
        }

        public void ValidateLinks(string outputPath)
        {
            if (!validateLinks)
            {
                return;
            }
            var path = Path.GetFullPath(outputPath);
            while (true)
            {
                if ((fileSystem.File.Exists(path) || fileSystem.Directory.Exists(path))
                    && (fileSystem.File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0)
                {
                    throw new InvalidDataException($"Generated output path '{outputPath}' crosses a filesystem link at '{path}'.");
                }
                if (path.Equals(_root, StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }
                path = Path.GetDirectoryName(path)
                    ?? throw new InvalidDataException($"Generated output path '{outputPath}' is outside its output root.");
            }
        }
    }

    private static string NormalizeRoute(string route)
    {
        var normalized = route.Trim().Trim('/').Replace('\\', '/');
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return string.Empty;
        }

        var segments = normalized.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Any(segment => segment is "." or ".."))
        {
            throw new InvalidDataException($"Route '{route}' contains a relative path segment.");
        }

        return string.Join('/', segments);
    }

    private static string GetTagRoute(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
        {
            throw new InvalidDataException("Tags cannot be empty.");
        }

        try
        {
            return ContentUrlHelper.GetTagUrl(tag);
        }
        catch (ArgumentException exception)
        {
            throw new InvalidDataException($"Invalid tag route for '{tag}'.", exception);
        }
    }
}
