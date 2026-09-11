using System.Globalization;
using System.IO.Abstractions;

using Microsoft.Extensions.Logging;

using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;
using ScissorHands.Web.Abstractions;

using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ScissorHands.Web.Loaders;

/// <summary>
/// This represents the content loader entity.
/// </summary>
/// <param name="paths"><see cref="IAppPaths"/> instance.</param>
/// <param name="fileSystem"><see cref="IFileSystem"/> instance.</param>
/// <param name="options"><see cref="SiteManifest"/> instance.</param>
/// <param name="logger"><see cref="ILogger{T}"/> instance.</param>
public sealed class ContentLoader(IAppPaths paths, IFileSystem fileSystem, SiteManifest options, ILogger<ContentLoader> logger) : IContentLoader
{
    private const string POST_DIRECTORY = "posts";
    private const string PAGE_DIRECTORY = "pages";
    private static readonly HashSet<string> SupportedMetadataKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "title",
        "slug",
        "description",
        "locale",
        "author",
        "twitter_handle",
        "hero_image",
        "draft",
        "tags",
        "published",
    };

    private readonly IAppPaths _paths = paths ?? throw new ArgumentNullException(nameof(paths));
    private readonly IFileSystem _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    private readonly SiteManifest _options = options ?? throw new ArgumentNullException(nameof(options));
    private readonly ILogger<ContentLoader> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IDeserializer _deserializer = new DeserializerBuilder()
                                                       .WithNamingConvention(CamelCaseNamingConvention.Instance)
                                                       .IgnoreUnmatchedProperties()
                                                       .Build();

    /// <inheritdoc />
    public async Task<IEnumerable<ContentDocument>> LoadAsync(CancellationToken cancellationToken)
    {
        var documents = new List<ContentDocument>();
        documents.AddRange(await LoadFromDirectoryAsync(ContentKind.Post, POST_DIRECTORY, cancellationToken));
        documents.AddRange(await LoadFromDirectoryAsync(ContentKind.Page, PAGE_DIRECTORY, cancellationToken));
        return documents;
    }

    private async Task<IReadOnlyList<ContentDocument>> LoadFromDirectoryAsync(ContentKind kind, string directory, CancellationToken cancellationToken)
    {
        var result = new List<ContentDocument>();
        var root = _fileSystem.Path.Combine(_paths.GetContentsRoot(), directory);

        if (!_fileSystem.Directory.Exists(root))
        {
            _logger.LogWarning("Content directory {Directory} not found at {Path}", directory, root);
            return result;
        }

        foreach (var file in _fileSystem.Directory.EnumerateFiles(root, "*.md", SearchOption.AllDirectories))
        {
            var text = await _fileSystem.File.ReadAllTextAsync(file, cancellationToken);
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
        if (string.Equals(firstLine?.Trim(), "---", StringComparison.OrdinalIgnoreCase) == false)
        {
            return (new ContentMetadata { Title = _fileSystem.Path.GetFileNameWithoutExtension(sourcePath), Slug = string.Empty }, text);
        }

        var yamlLines = new List<string>();
        string? line;
        while ((line = reader.ReadLine()) is not null)
        {
            if (string.Equals(line.Trim(), "---", StringComparison.OrdinalIgnoreCase) == true)
            {
                break;
            }
            yamlLines.Add(line);
        }

        if (line is null)
        {
            throw new InvalidDataException($"Frontmatter in '{sourcePath}' is missing its closing delimiter.");
        }

        var markdownBody = reader.ReadToEnd();
        var yaml = string.Join(Environment.NewLine, yamlLines);

        try
        {
            var map = _deserializer.Deserialize<Dictionary<string, object>>(yaml) ?? [];
            var unsupportedKeys = map.Keys.Where(key => !SupportedMetadataKeys.Contains(key)).ToList();
            if (unsupportedKeys.Count > 0)
            {
                throw new InvalidDataException(
                    $"Unsupported frontmatter field(s) in '{sourcePath}': {string.Join(", ", unsupportedKeys)}.");
            }

            var title = map.TryGetValue("title", out var titleValue) ? Convert.ToString(titleValue, CultureInfo.InvariantCulture) ?? string.Empty : _fileSystem.Path.GetFileNameWithoutExtension(sourcePath);
            var slug = map.TryGetValue("slug", out var slugValue) ? Convert.ToString(slugValue, CultureInfo.InvariantCulture) ?? string.Empty : string.Empty;
            var description = map.TryGetValue("description", out var descValue) ? Convert.ToString(descValue, CultureInfo.InvariantCulture) : default;
            var locale = map.TryGetValue("locale", out var localeValue) ? Convert.ToString(localeValue, CultureInfo.InvariantCulture) : default;
            var author = map.TryGetValue("author", out var authorValue) ? Convert.ToString(authorValue, CultureInfo.InvariantCulture) : default;
            var twitterHandle = map.TryGetValue("twitter_handle", out var twitterValue) ? Convert.ToString(twitterValue, CultureInfo.InvariantCulture) : default;
            var heroImage = map.TryGetValue("hero_image", out var heroImageValue) ? Convert.ToString(heroImageValue, CultureInfo.InvariantCulture) : default;
            var draft = false;
            if (map.TryGetValue("draft", out var draftValue)
                && !bool.TryParse(Convert.ToString(draftValue, CultureInfo.InvariantCulture), out draft))
            {
                throw new InvalidDataException($"Frontmatter field 'draft' in '{sourcePath}' must be true or false.");
            }

            var tags = map.TryGetValue("tags", out var tagsValue) ? ToTags(tagsValue, sourcePath) : [];
            DateTimeOffset? published = null;

            if (map.TryGetValue("published", out var publishedValue))
            {
                if (!DateTimeOffset.TryParse(
                        Convert.ToString(publishedValue, CultureInfo.InvariantCulture),
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeUniversal,
                        out var parsedPublished))
                {
                    throw new InvalidDataException(
                        $"Frontmatter field 'published' in '{sourcePath}' must be a valid date and time.");
                }

                published = parsedPublished;
            }

            return (new ContentMetadata
            {
                Title = title,
                Slug = slug,
                Description = description,
                Locale = locale,
                Author = author,
                TwitterHandle = twitterHandle,
                HeroImage = heroImage,
                Tags = tags,
                Published = published,
                Draft = draft,
            }, markdownBody.Trim());
        }
        catch (YamlException ex)
        {
            throw new InvalidDataException($"Failed to parse frontmatter in '{sourcePath}'.", ex);
        }
    }

    private ContentMetadata ApplySlug(ContentMetadata metadata, ContentKind kind, string file, string root)
    {
        var slug = string.IsNullOrWhiteSpace(metadata.Slug)
            ? InferSlugFromFile(file, root)
            : metadata.Slug.Trim('/');

        var effectiveLocale = string.IsNullOrWhiteSpace(metadata.Locale) ? _options.Locale : metadata.Locale;

        if (kind == ContentKind.Post && _options.UseDateInPostUrl)
        {
            if (metadata.Published is { } published)
            {
                slug = string.Concat(published.ToString("yyyy/MM/dd"), "/", slug);
            }
            else
            {
                _logger.LogWarning("UseDateInPostUrl is enabled but no published date was found for {Path}; using slug without date", file);
            }
        }

        if (_options.UseLocaleInUrl)
        {
            var localeSegment = ToLocaleSegment(effectiveLocale);
            if (!string.IsNullOrWhiteSpace(localeSegment))
            {
                slug = slug.Trim('/');
                if (!string.IsNullOrWhiteSpace(slug)
                    && !slug.Equals("404.html", StringComparison.OrdinalIgnoreCase)
                    && !slug.Equals(localeSegment, StringComparison.OrdinalIgnoreCase)
                    && !slug.StartsWith(localeSegment + "/", StringComparison.OrdinalIgnoreCase))
                {
                    slug = string.Concat(localeSegment, "/", slug);
                }
            }
        }

        return metadata with { Slug = slug, Locale = effectiveLocale };
    }

    private static string ToLocaleSegment(string? locale)
    {
        if (string.IsNullOrWhiteSpace(locale))
        {
            return string.Empty;
        }

        return locale.Trim()
                     .Replace('_', '-')
                     .Replace('/', '-')
                     .ToLowerInvariant();
    }

    private static IEnumerable<string> ToTags(object value, string sourcePath)
    {
        return value switch
        {
            IEnumerable<object> enumerable => [.. enumerable.Select(v => Convert.ToString(v) ?? string.Empty).Where(s => !string.IsNullOrWhiteSpace(s))],
            string csv => csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
            _ => throw new InvalidDataException(
                $"Frontmatter field 'tags' in '{sourcePath}' must be a YAML list or comma-separated string."),
        };
    }

    private static string InferSlugFromFile(string path, string root)
    {
        var relative = Path.GetRelativePath(root, path);
        var withoutExtension = Path.Combine(Path.GetDirectoryName(relative) ?? string.Empty, Path.GetFileNameWithoutExtension(relative));
        return withoutExtension.Replace(Path.DirectorySeparatorChar, '/');
    }
}
