using System.Globalization;

using Microsoft.Extensions.Logging;

using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;

using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ScissorHands.Web.Services;

public sealed class ContentLoader(SiteManifest options, ILogger<ContentLoader> logger)
{
    private readonly SiteManifest _options = options ?? throw new ArgumentNullException(nameof(options));
    private readonly ILogger<ContentLoader> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly string _basePath = Directory.GetCurrentDirectory();
    private readonly IDeserializer _deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();

    public async Task<IReadOnlyList<ContentDocument>> LoadAsync(CancellationToken cancellationToken)
    {
        var documents = new List<ContentDocument>();
        documents.AddRange(await LoadFromFolderAsync(ContentKind.Post, "posts", cancellationToken));
        documents.AddRange(await LoadFromFolderAsync(ContentKind.Page, "pages", cancellationToken));
        return documents;
    }

    private async Task<IReadOnlyList<ContentDocument>> LoadFromFolderAsync(ContentKind kind, string folder, CancellationToken cancellationToken)
    {
        var result = new List<ContentDocument>();
        var root = Path.Combine(_basePath, _options.ContentRoot, folder);

        if (!Directory.Exists(root))
        {
            _logger.LogWarning("Content folder {Folder} not found at {Path}", folder, root);
            return result;
        }

        foreach (var file in Directory.EnumerateFiles(root, "*.md", SearchOption.AllDirectories))
        {
            var text = await File.ReadAllTextAsync(file, cancellationToken);
            var (metadata, markdown) = ParseFrontMatter(text, file);
            metadata = ApplySlug(metadata, kind, file, root);

            var document = new ContentDocument
            {
                SourcePath = file,
                Kind = kind,
                Metadata = metadata,
                Markdown = markdown
            };

            if (document.Metadata.Draft)
            {
                _logger.LogInformation("Skipping draft content at {Path}", file);
                continue;
            }

            result.Add(document);
        }

        return result;
    }

    private (ContentMetadata metadata, string markdown) ParseFrontMatter(string text, string sourcePath)
    {
        using var reader = new StringReader(text);
        var firstLine = reader.ReadLine();
        if (!string.Equals(firstLine?.Trim(), "---", StringComparison.Ordinal))
        {
            return (new ContentMetadata { Title = Path.GetFileNameWithoutExtension(sourcePath), Slug = string.Empty }, text);
        }

        var yamlLines = new List<string>();
        string? line;
        while ((line = reader.ReadLine()) is not null)
        {
            if (string.Equals(line.Trim(), "---", StringComparison.Ordinal))
            {
                break;
            }
            yamlLines.Add(line);
        }

        var markdownBody = reader.ReadToEnd();
        var yaml = string.Join(Environment.NewLine, yamlLines);

        try
        {
            var map = _deserializer.Deserialize<Dictionary<string, object>>(yaml) ?? new();
            var title = map.TryGetValue("title", out var titleValue) ? Convert.ToString(titleValue, CultureInfo.InvariantCulture) ?? string.Empty : Path.GetFileNameWithoutExtension(sourcePath);
            var slug = map.TryGetValue("slug", out var slugValue) ? Convert.ToString(slugValue, CultureInfo.InvariantCulture) ?? string.Empty : string.Empty;
            var description = map.TryGetValue("description", out var descValue) ? Convert.ToString(descValue, CultureInfo.InvariantCulture) : null;
            var author = map.TryGetValue("author", out var authorValue) ? Convert.ToString(authorValue, CultureInfo.InvariantCulture) : null;
            var heroImage = map.TryGetValue("hero", out var heroImageValue) ? Convert.ToString(heroImageValue, CultureInfo.InvariantCulture) : null;
            var draft = map.TryGetValue("draft", out var draftValue) && bool.TryParse(Convert.ToString(draftValue, CultureInfo.InvariantCulture), out var parsedDraft) && parsedDraft;
            var tags = map.TryGetValue("tags", out var tagsValue) ? ToTags(tagsValue) : Array.Empty<string>();
            DateTimeOffset? published = null;

            if (map.TryGetValue("published", out var publishedValue) && DateTimeOffset.TryParse(Convert.ToString(publishedValue, CultureInfo.InvariantCulture), out var parsedPublished))
            {
                published = parsedPublished;
            }

            return (new ContentMetadata
            {
                Title = title,
                Slug = slug,
                Description = description,
                Author = author,
                HeroImage = heroImage,
                Tags = tags,
                Published = published,
                Draft = draft,
            }, markdownBody.Trim());
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse frontmatter for {Path}", sourcePath);
            return (new ContentMetadata { Title = Path.GetFileNameWithoutExtension(sourcePath), Slug = string.Empty }, text);
        }
    }

    private ContentMetadata ApplySlug(ContentMetadata metadata, ContentKind kind, string file, string root)
    {
        var slug = string.IsNullOrWhiteSpace(metadata.Slug)
            ? InferSlugFromFile(file, root)
            : metadata.Slug.Trim('/');

        if (kind == ContentKind.Post && _options.IncludeDateInPostUrl)
        {
            if (metadata.Published is { } published)
            {
                slug = string.Concat(published.ToString("yyyy/MM/dd"), "/", slug);
            }
            else
            {
                _logger.LogWarning("IncludeDateInPostUrl is enabled but no published date was found for {Path}; using slug without date", file);
            }
        }

        return metadata with { Slug = slug };
    }

    private static IEnumerable<string> ToTags(object value)
    {
        return value switch
        {
            IEnumerable<object> enumerable => enumerable.Select(v => Convert.ToString(v) ?? string.Empty).Where(s => !string.IsNullOrWhiteSpace(s)).ToArray(),
            string csv => csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
            _ => Array.Empty<string>()
        };
    }

    private static string InferSlugFromFile(string path, string root)
    {
        var relative = Path.GetRelativePath(root, path);
        var withoutExtension = Path.Combine(Path.GetDirectoryName(relative) ?? string.Empty, Path.GetFileNameWithoutExtension(relative));
        return withoutExtension.Replace(Path.DirectorySeparatorChar, '/');
    }
}
