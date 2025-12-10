using System.Text;

using Microsoft.Extensions.Logging;

using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;
using ScissorHands.Core.Services;
using ScissorHands.Web.Rendering;

namespace ScissorHands.Web.Services;

public sealed class StaticSiteGenerator(
        ContentLoader contentLoader,
        MarkdownService markdownService,
        PluginRunner pluginRunner,
        IThemeService themeService,
        ComponentRenderer renderer,
        SiteManifest options,
        ILogger<StaticSiteGenerator> logger)
{
    private readonly ContentLoader _contentLoader = contentLoader ?? throw new ArgumentNullException(nameof(contentLoader));
    private readonly MarkdownService _markdownService = markdownService ?? throw new ArgumentNullException(nameof(markdownService));
    private readonly PluginRunner _pluginRunner = pluginRunner ?? throw new ArgumentNullException(nameof(pluginRunner));
    private readonly IThemeService _themeService = themeService ?? throw new ArgumentNullException(nameof(themeService));
    private readonly ComponentRenderer _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
    private readonly SiteManifest _options = options ?? throw new ArgumentNullException(nameof(options));
    private readonly ILogger<StaticSiteGenerator> _logger = logger ?? throw new ArgumentNullException(nameof(logger));


    public async Task BuildAsync<TMainLayout, TIndexView, TPostView, TPageView>(string destination, bool preview, CancellationToken cancellationToken)
        where TMainLayout : ScissorHands.Theme.MainLayoutBase
        where TIndexView : ScissorHands.Theme.IndexViewBase
        where TPostView : ScissorHands.Theme.PostViewBase
        where TPageView : ScissorHands.Theme.PageViewBase
    {
        Directory.CreateDirectory(destination);
        _logger.LogInformation("Starting static site build to {Destination} (preview: {Preview})", destination, preview);

        var theme = _themeService.LoadManifest(_options.Theme);
        var documents = await _contentLoader.LoadAsync(cancellationToken);

        var layoutType = typeof(TMainLayout);
        await RenderIndexAsync<TIndexView>(documents, theme, destination, layoutType, cancellationToken);

        foreach (var document in documents)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var preProcessed = await _pluginRunner.RunPreMarkdownAsync(document, cancellationToken);
            var html = await _markdownService.ToHtmlAsync(preProcessed.Markdown, cancellationToken);
            preProcessed.Html = html;
            var postMarkdown = await _pluginRunner.RunPostMarkdownAsync(preProcessed, cancellationToken);

            var parameters = new Dictionary<string, object?>
            {
                ["Document"] = postMarkdown,
                ["Theme"] = theme,
                ["Site"] = _options
            };

            var rendered = postMarkdown.Kind switch
            {
                ContentKind.Page => await _renderer.RenderAsync<TPageView>(layoutType, parameters, cancellationToken),
                _ => await _renderer.RenderAsync<TPostView>(layoutType, parameters, cancellationToken)
            };

            var finalHtml = await _pluginRunner.RunPostHtmlAsync(rendered, postMarkdown, cancellationToken);
            var outputPath = ResolveOutputPath(destination, postMarkdown.Metadata.Slug);
            Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
            await File.WriteAllTextAsync(outputPath, finalHtml, Encoding.UTF8, cancellationToken);
            _logger.LogInformation("Wrote {OutputPath}", outputPath);
        }

        CopyContentAssets(destination);
        _themeService.CopyAssets(_options.Theme, destination);
    }

    private async Task RenderIndexAsync<TIndexView>(IEnumerable<ContentDocument> documents, ThemeManifest theme, string destination, Type layoutType, CancellationToken cancellationToken)
        where TIndexView : ScissorHands.Theme.IndexViewBase
    {
        var posts = documents
            .Where(d => d.Kind == ContentKind.Post)
            .OrderByDescending(d => d.Metadata.Published ?? DateTimeOffset.MinValue)
            .ToList();

        var parameters = new Dictionary<string, object?>
        {
            ["Documents"] = posts,
            ["Theme"] = theme,
            ["Site"] = _options
        };

        var rendered = await _renderer.RenderAsync<TIndexView>(layoutType, parameters, cancellationToken);
        var finalHtml = await _pluginRunner.RunPostHtmlAsync(rendered, new ContentDocument
        {
            Kind = ContentKind.Page,
            Metadata = new ContentMetadata { Title = _options.Title, Slug = string.Empty },
            Markdown = string.Empty,
            Html = rendered
        }, cancellationToken);

        var outputPath = ResolveOutputPath(destination, string.Empty);
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
        await File.WriteAllTextAsync(outputPath, finalHtml, Encoding.UTF8, cancellationToken);
        _logger.LogInformation("Wrote {OutputPath}", outputPath);
    }

    private static string ResolveOutputPath(string root, string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            return Path.Combine(root, "index.html");
        }

        var safeSlug = slug.Trim('/');
        return Path.Combine(root, safeSlug, "index.html");
    }

    private void CopyContentAssets(string destination)
    {
        var source = Path.Combine(Directory.GetCurrentDirectory(), _options.ContentRoot, "images");
        var target = Path.Combine(destination, "images");

        if (!Directory.Exists(source))
        {
            _logger.LogWarning("No content images found at {Path}", source);
            return;
        }

        CopyDirectory(source, target);
    }

    private static void CopyDirectory(string sourceDir, string destinationDir)
    {
        Directory.CreateDirectory(destinationDir);

        foreach (var file in Directory.GetFiles(sourceDir, "*", SearchOption.TopDirectoryOnly))
        {
            var destFile = Path.Combine(destinationDir, Path.GetFileName(file));
            File.Copy(file, destFile, overwrite: true);
        }

        foreach (var directory in Directory.GetDirectories(sourceDir, "*", SearchOption.TopDirectoryOnly))
        {
            var name = Path.GetFileName(directory);
            CopyDirectory(directory, Path.Combine(destinationDir, name));
        }
    }
}
