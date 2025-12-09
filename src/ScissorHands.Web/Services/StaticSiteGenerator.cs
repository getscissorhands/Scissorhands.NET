using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ScissorHands.Web.Models;
using ScissorHands.Web.Rendering;

namespace ScissorHands.Web.Services;

public sealed class StaticSiteGenerator
{
    private readonly ContentLoader _contentLoader;
    private readonly MarkdownService _markdownService;
    private readonly PluginRunner _pluginRunner;
    private readonly ThemeService _themeService;
    private readonly ComponentRenderer _renderer;
    private readonly SiteOptions _options;
    private readonly ILogger<StaticSiteGenerator> _logger;

    public StaticSiteGenerator(
        ContentLoader contentLoader,
        MarkdownService markdownService,
        PluginRunner pluginRunner,
        ThemeService themeService,
        ComponentRenderer renderer,
        IOptions<SiteOptions> options,
        ILogger<StaticSiteGenerator> logger)
    {
        _contentLoader = contentLoader;
        _markdownService = markdownService;
        _pluginRunner = pluginRunner;
        _themeService = themeService;
        _renderer = renderer;
        _options = options.Value;
        _logger = logger;
    }

    public async Task BuildAsync(string destination, bool preview, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(destination);
        _logger.LogInformation("Starting static site build to {Destination} (preview: {Preview})", destination, preview);

        var theme = _themeService.LoadManifest(_options.Theme);
        var documents = await _contentLoader.LoadAsync(cancellationToken);

        var layoutType = typeof(Themes.MinimalBlog.MainLayout);
        await RenderIndexAsync(documents, theme, destination, layoutType, cancellationToken);

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
                ContentKind.Page => await _renderer.RenderAsync<Themes.MinimalBlog.PageView>(layoutType, parameters, cancellationToken),
                _ => await _renderer.RenderAsync<Themes.MinimalBlog.PostView>(layoutType, parameters, cancellationToken)
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

    private async Task RenderIndexAsync(IEnumerable<ContentDocument> documents, ThemeManifest theme, string destination, Type layoutType, CancellationToken cancellationToken)
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

        var rendered = await _renderer.RenderAsync<Themes.MinimalBlog.IndexView>(layoutType, parameters, cancellationToken);
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
