using System.IO.Abstractions;
using System.Text;

using Microsoft.Extensions.Logging;

using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;
using ScissorHands.Core.Services;
using ScissorHands.Web.Abstractions;
using ScissorHands.Web.Loaders;
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
        _fileSystem.Directory.CreateDirectory(destination);
        _logger.LogInformation("Starting static site build to {Destination} (preview: {Preview})", destination, preview);

        _options.IsPreview = preview;
        _options.DescriptionInHtml = await _markdownService.ToHtmlAsync(_options.Description, trim: true, cancellationToken: cancellationToken);
        var plugins = _pluginRunner.Manifests;
        var theme = await _themeService.LoadManifestAsync(_options.Theme, cancellationToken);
        var documents = (await _contentLoader.LoadAsync(cancellationToken)).ToList();
        ValidateOutputRoutes(destination, documents);

        var layoutType = typeof(TMainLayout);
        await RenderIndexAsync<TIndexView>(documents, plugins, theme, destination, layoutType, cancellationToken);

        var notFoundDocument = documents.SingleOrDefault(d => d.Kind == ContentKind.Page && string.Equals(d.Metadata.Slug, PAGE_NOT_FOUND_SLUG, StringComparison.OrdinalIgnoreCase));
        await RenderNotFoundAsync<TNotFoundView>(notFoundDocument, plugins, theme, destination, layoutType, cancellationToken);

        foreach (var document in documents.Where(d => IsNotFoundPage(d) == false))
        {
            await RenderDocumentAsync<TPostView, TPageView>(document, plugins, theme, destination, layoutType, cancellationToken);
        }

        await RenderTagPagesAsync<TTagListView, TTagView>(documents, plugins, theme, destination, layoutType, cancellationToken);

        CopyContentAssets(destination);
        await _themeService.CopyAssetsAsync(_options.Theme, destination, cancellationToken);
    }

    private async Task RenderIndexAsync<TIndexView>(IEnumerable<ContentDocument> documents, IEnumerable<PluginManifest> plugins, ThemeManifest theme, string destination, Type layoutType, CancellationToken cancellationToken)
        where TIndexView : ScissorHands.Theme.IndexViewBase
    {
        var posts = documents
            .Where(d => d.Kind == ContentKind.Post)
            .OrderByDescending(d => d.Metadata.Published ?? DateTimeOffset.MinValue)
            .ToList();

        var parameters = CreateBaseParameters(plugins, theme);
        parameters["Documents"] = posts;

        var rendered = await _renderer.RenderAsync<TIndexView>(layoutType, parameters, cancellationToken);
        var indexDocument = new ContentDocument
        {
            Kind = ContentKind.Page,
            Metadata = new ContentMetadata { Title = _options.Title, Slug = string.Empty },
            Markdown = string.Empty,
            Html = rendered
        };

        var outputPath = ResolveOutputPath(destination, string.Empty);
        await WriteRenderedHtmlAsync(outputPath, rendered, indexDocument, cancellationToken);
    }

    private async Task RenderNotFoundAsync<TNotFoundView>(ContentDocument? notFoundDocument, IEnumerable<PluginManifest> plugins, ThemeManifest theme, string destination, Type layoutType, CancellationToken cancellationToken)
        where TNotFoundView : ScissorHands.Theme.NotFoundViewBase
    {
        ContentDocument documentToRender;
        if (notFoundDocument is null)
        {
            documentToRender = new ContentDocument
            {
                Kind = ContentKind.Page,
                Metadata = new ContentMetadata { Title = "404 - Not Found", Description = "Page not found", Slug = PAGE_NOT_FOUND_SLUG },
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

        var parameters = CreateBaseParameters(plugins, theme);
        parameters["Document"] = documentToRender;

        var rendered = await _renderer.RenderAsync<TNotFoundView>(layoutType, parameters, cancellationToken);

        var outputPath = _fileSystem.Path.Combine(destination, PAGE_NOT_FOUND_SLUG);
        await WriteRenderedHtmlAsync(outputPath, rendered, documentToRender, cancellationToken);
    }

    private async Task RenderDocumentAsync<TPostView, TPageView>(ContentDocument document, IEnumerable<PluginManifest> plugins, ThemeManifest theme, string destination, Type layoutType, CancellationToken cancellationToken)
        where TPostView : ScissorHands.Theme.PostViewBase
        where TPageView : ScissorHands.Theme.PageViewBase
    {
        cancellationToken.ThrowIfCancellationRequested();

        var postMarkdown = await ConvertMarkdownToHtmlAsync(document, cancellationToken);

        var parameters = CreateBaseParameters(plugins, theme);
        parameters["Document"] = postMarkdown;

        var rendered = postMarkdown.Kind switch
        {
            ContentKind.Page => await _renderer.RenderAsync<TPageView>(layoutType, parameters, cancellationToken),
            _ => await _renderer.RenderAsync<TPostView>(layoutType, parameters, cancellationToken)
        };
        var outputPath = ResolveOutputPath(destination, postMarkdown.Metadata.Slug);

        await WriteRenderedHtmlAsync(outputPath, rendered, postMarkdown, cancellationToken);
    }

    private async Task RenderTagPagesAsync<TTagListView, TTagView>(IEnumerable<ContentDocument> documents, IEnumerable<PluginManifest> plugins, ThemeManifest theme, string destination, Type layoutType, CancellationToken cancellationToken)
        where TTagListView : ScissorHands.Theme.TagListViewBase
        where TTagView : ScissorHands.Theme.TagViewBase
    {
        // Build the tag dictionary: for each tag, group posts (sorted by published date descending) and pages (sorted by title ascending)
        var taggedDocuments = documents
            .Where(d => d.Metadata.Tags.Any() && IsNotFoundPage(d) == false)
            .SelectMany(d => d.Metadata.Tags.Select(tag => (Tag: tag.ToLowerInvariant(), Document: d)))
            .GroupBy(x => x.Tag)
            .ToDictionary(
                g => g.Key,
                g =>
                {
                    var posts = g
                        .Where(x => x.Document.Kind == ContentKind.Post)
                        .Select(x => x.Document)
                        .OrderByDescending(d => d.Metadata.Published ?? DateTimeOffset.MinValue)
                        .ToList()
                        .AsEnumerable();

                    var pages = g
                        .Where(x => x.Document.Kind == ContentKind.Page)
                        .Select(x => x.Document)
                        .OrderBy(d => d.Metadata.Title)
                        .ToList()
                        .AsEnumerable();

                    return (Posts: posts, Pages: pages);
                });

        if (taggedDocuments.Count == 0)
        {
            _logger.LogInformation("No tags found in content documents; skipping tag pages");
            return;
        }

        // Render the tag list page at /tags
        await RenderTagListPageAsync<TTagListView>(taggedDocuments, plugins, theme, destination, layoutType, cancellationToken);

        // Render individual tag pages at /tags/{tag}
        foreach (var tagEntry in taggedDocuments)
        {
            await RenderTagPageAsync<TTagView>(tagEntry.Key, tagEntry.Value.Posts, tagEntry.Value.Pages, plugins, theme, destination, layoutType, cancellationToken);
        }
    }

    private async Task RenderTagListPageAsync<TTagListView>(IDictionary<string, (IEnumerable<ContentDocument> Posts, IEnumerable<ContentDocument> Pages)> taggedDocuments, IEnumerable<PluginManifest> plugins, ThemeManifest theme, string destination, Type layoutType, CancellationToken cancellationToken)
        where TTagListView : ScissorHands.Theme.TagListViewBase
    {
        var parameters = CreateBaseParameters(plugins, theme);
        parameters["TaggedDocuments"] = taggedDocuments;

        var rendered = await _renderer.RenderAsync<TTagListView>(layoutType, parameters, cancellationToken);
        var tagListDocument = new ContentDocument
        {
            Kind = ContentKind.Page,
            Metadata = new ContentMetadata { Title = "Tags", Slug = "tags" },
            Markdown = string.Empty,
            Html = rendered
        };
        var outputPath = ResolveOutputPath(destination, "tags");
        await WriteRenderedHtmlAsync(outputPath, rendered, tagListDocument, cancellationToken);
    }

    private async Task RenderTagPageAsync<TTagView>(string tag, IEnumerable<ContentDocument> posts, IEnumerable<ContentDocument> pages, IEnumerable<PluginManifest> plugins, ThemeManifest theme, string destination, Type layoutType, CancellationToken cancellationToken)
        where TTagView : ScissorHands.Theme.TagViewBase
    {
        var parameters = CreateBaseParameters(plugins, theme);
        parameters["Tag"] = tag;
        parameters["TaggedPosts"] = posts;
        parameters["TaggedPages"] = pages;

        var rendered = await _renderer.RenderAsync<TTagView>(layoutType, parameters, cancellationToken);
        var tagDocument = new ContentDocument
        {
            Kind = ContentKind.Page,
            Metadata = new ContentMetadata { Title = $"Tag: {tag}", Slug = $"tags/{ToTagSlug(tag)}" },
            Markdown = string.Empty,
            Html = rendered
        };
        var outputPath = ResolveOutputPath(destination, $"tags/{ToTagSlug(tag)}");
        await WriteRenderedHtmlAsync(outputPath, rendered, tagDocument, cancellationToken);
    }

    private Dictionary<string, object?> CreateBaseParameters(IEnumerable<PluginManifest> plugins, ThemeManifest theme)
    {
        return new Dictionary<string, object?>
        {
            ["Plugins"] = plugins,
            ["Theme"] = theme,
            ["Site"] = _options
        };
    }

    private async Task WriteRenderedHtmlAsync(string outputPath, string renderedHtml, ContentDocument document, CancellationToken cancellationToken)
    {
        var finalHtml = await _pluginRunner.RunPostHtmlAsync(renderedHtml, document, cancellationToken);

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

    private void CopyContentAssets(string destination)
    {
        var source = _fileSystem.Path.Combine(_paths.GetContentsRoot(), "images");
        var target = _fileSystem.Path.Combine(destination, "images");

        if (!_fileSystem.Directory.Exists(source))
        {
            _logger.LogWarning("No content images found at {Path}", source);
            return;
        }

        CopyDirectory(source, target);
    }

    private void CopyDirectory(string sourceDir, string destinationDir)
    {
        _fileSystem.Directory.CreateDirectory(destinationDir);

        foreach (var file in _fileSystem.Directory.GetFiles(sourceDir, "*", SearchOption.TopDirectoryOnly))
        {
            var destFile = _fileSystem.Path.Combine(destinationDir, _fileSystem.Path.GetFileName(file));
            _fileSystem.File.Copy(file, destFile, overwrite: true);
        }

        foreach (var directory in _fileSystem.Directory.GetDirectories(sourceDir, "*", SearchOption.TopDirectoryOnly))
        {
            var name = _fileSystem.Path.GetFileName(directory);
            CopyDirectory(directory, _fileSystem.Path.Combine(destinationDir, name));
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

    private static void ValidateOutputRoutes(string destination, IEnumerable<ContentDocument> documents)
    {
        var routes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        AddRoute(string.Empty, "site index");
        AddOutputPath(Path.Combine(Path.GetFullPath(destination), PAGE_NOT_FOUND_SLUG), "not-found page");

        var taggedDocuments = false;
        var tags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var document in documents)
        {
            if (!IsNotFoundPage(document))
            {
                AddRoute(document.Metadata.Slug, document.SourcePath);
            }

            foreach (var tag in document.Metadata.Tags)
            {
                taggedDocuments = true;
                tags.Add(tag);
            }
        }

        if (taggedDocuments)
        {
            AddRoute("tags", "tag index");
            foreach (var tag in tags)
            {
                AddRoute($"tags/{ToTagSlug(tag)}", $"tag '{tag}'");
            }
        }

        void AddRoute(string route, string owner)
            => AddOutputPath(ResolveOutputPath(destination, route), owner);

        void AddOutputPath(string outputPath, string owner)
        {
            var fullPath = Path.GetFullPath(outputPath);
            if (routes.TryGetValue(fullPath, out var existingOwner))
            {
                throw new InvalidDataException(
                    $"Output collision at '{fullPath}' between '{existingOwner}' and '{owner}'.");
            }

            var separator = Path.DirectorySeparatorChar.ToString();
            var conflictingRoute = routes.FirstOrDefault(route =>
                fullPath.StartsWith(route.Key + separator, StringComparison.OrdinalIgnoreCase)
                || route.Key.StartsWith(fullPath + separator, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrEmpty(conflictingRoute.Key))
            {
                throw new InvalidDataException(
                    $"Output collision between file '{conflictingRoute.Key}' from '{conflictingRoute.Value}' and '{fullPath}' from '{owner}'.");
            }

            routes.Add(fullPath, owner);
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

    private static string ToTagSlug(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
        {
            throw new InvalidDataException("Tags cannot be empty.");
        }

        return Uri.EscapeDataString(tag.Trim().ToLowerInvariant());
    }
}
