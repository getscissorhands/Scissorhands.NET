using System.Globalization;
using System.IO.Abstractions;

using Microsoft.Extensions.Logging;

using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;
using ScissorHands.Core.Urls;
using ScissorHands.Web.Abstractions;
using ScissorHands.Web.Localization;

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
        "author",
        "twitter_handle",
        "hero_image",
        "draft",
        "show_in_navigation",
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
        cancellationToken.ThrowIfCancellationRequested();
        var locales = LocaleConfiguration.Create(_options);
        var documents = new List<ContentDocument>();
        documents.AddRange(await LoadFromDirectoryAsync(ContentKind.Post, POST_DIRECTORY, locales, cancellationToken));
        documents.AddRange(await LoadFromDirectoryAsync(ContentKind.Page, PAGE_DIRECTORY, locales, cancellationToken));
        return documents;
    }

    private async Task<IReadOnlyList<ContentDocument>> LoadFromDirectoryAsync(
        ContentKind kind, string directory, LocaleConfiguration locales, CancellationToken cancellationToken)
    {
        var entries = new List<(string Identity, string? Locale, string Slug, ContentDocument Document)>();
        var identities = new HashSet<(string Identity, string? Locale)>();
        var root = _fileSystem.Path.GetFullPath(_fileSystem.Path.Combine(_paths.GetContentsRoot(), directory));

        if (!_fileSystem.Directory.Exists(root))
        {
            _logger.LogWarning("Content directory {Directory} not found at {Path}", directory, root);
            return [];
        }

        foreach (var file in EnumerateContentFiles(root, cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var relative = _fileSystem.Path.GetRelativePath(root, file).Replace('\\', '/');
            var separator = relative.IndexOf('/');
            var folder = separator < 0 ? string.Empty : relative[..separator];
            var locale = locales.GetDirectoryLocale(folder);
            if (locales.Primary is not null && separator >= 0
                && ContentUrlHelper.GetLocaleSegment(folder) == locales.Primary)
            {
                throw new InvalidDataException(
                    $"Primary content '{file}' must be moved out of the '{folder}' locale directory into '{root}', preserving its intended slug.");
            }
            var identity = locale is null ? relative : relative[(separator + 1)..];
            var text = await _fileSystem.File.ReadAllTextAsync(file, cancellationToken);
            var (metadata, markdown) = ParseFrontMatter(text, file);
            var slug = string.IsNullOrWhiteSpace(metadata.Slug)
                ? InferSlugFromFile(kind, identity)
                : metadata.Slug.Trim().Trim('/');
            if (locale is not null && !string.IsNullOrWhiteSpace(metadata.Slug))
            {
                if (slug.Equals(locale, StringComparison.OrdinalIgnoreCase))
                {
                    slug = string.Empty;
                }
                else if (slug.StartsWith(locale + "/", StringComparison.OrdinalIgnoreCase))
                {
                    slug = slug[(locale.Length + 1)..];
                }
            }
            slug = string.Join('/', slug.Replace('\\', '/').Split('/', StringSplitOptions.RemoveEmptyEntries));
            try
            {
                _ = ContentUrlHelper.GetContentUrl(slug);
            }
            catch (ArgumentException exception)
            {
                throw new InvalidDataException($"Invalid slug '{slug}' in '{file}'.", exception);
            }
            metadata = ApplySlug(metadata with { Slug = slug, Locale = locale ?? locales.Primary }, kind, file, locale);

            var document = new ContentDocument
            {
                SourcePath = file,
                Kind = kind,
                Metadata = metadata,
                Markdown = markdown
            };

            if (!identities.Add((identity, locale)))
            {
                throw new InvalidDataException($"Duplicate translation identity '{identity}' for locale '{locale}' at '{file}'.");
            }
            entries.Add((identity, locale, slug, document));
        }

        var primary = entries.Where(entry => entry.Locale is null).ToDictionary(entry => entry.Identity, StringComparer.Ordinal);
        var result = new List<ContentDocument>();
        foreach (var entry in entries)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (entry.Locale is not null)
            {
                if (!primary.TryGetValue(entry.Identity, out var original))
                {
                    _logger.LogInformation("Skipping translation {Path} because its primary document is missing", entry.Document.SourcePath);
                    continue;
                }
                if (!string.Equals(original.Slug, entry.Slug, StringComparison.Ordinal))
                {
                    throw new InvalidDataException(
                        $"Paired slugs must match: '{original.Document.SourcePath}' has '{original.Slug}', but '{entry.Document.SourcePath}' has '{entry.Slug}'.");
                }
                if (kind == ContentKind.Post
                    && (original.Document.Metadata.Published is not { } originalDate
                        || entry.Document.Metadata.Published is not { } translatedDate
                        || originalDate.Date != translatedDate.Date))
                {
                    throw new InvalidDataException(
                        $"Paired posts '{original.Document.SourcePath}' (published: {original.Document.Metadata.Published:O}) and '{entry.Document.SourcePath}' (published: {entry.Document.Metadata.Published:O}) must both declare published values with the same written calendar date.");
                }
                if (original.Document.Metadata.Draft || original.Slug.Equals("404.html", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
            }
            if (entry.Document.Metadata.Draft)
            {
                _logger.LogInformation("Skipping draft content at {Path}", entry.Document.SourcePath);
                continue;
            }
            result.Add(entry.Document);
        }
        return result;
    }

    private IEnumerable<string> EnumerateContentFiles(string directory, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        RejectLink(directory);
        foreach (var file in _fileSystem.Directory.EnumerateFiles(directory, "*.md"))
        {
            cancellationToken.ThrowIfCancellationRequested();
            RejectLink(file);
            yield return file;
        }
        foreach (var child in _fileSystem.Directory.EnumerateDirectories(directory))
        {
            foreach (var file in EnumerateContentFiles(child, cancellationToken))
            {
                yield return file;
            }
        }
    }

    private void RejectLink(string path)
    {
        var current = _fileSystem.Path.GetFullPath(path);
        while (current is not null)
        {
            if ((_fileSystem.File.GetAttributes(current) & FileAttributes.ReparsePoint) != 0)
            {
                throw new InvalidDataException($"Content path '{path}' crosses a filesystem link at '{current}'.");
            }
            if (current == _fileSystem.Path.GetFullPath(_paths.BasePath))
            {
                break;
            }
            current = _fileSystem.Path.GetDirectoryName(current);
        }
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
            if (map.Keys.Any(key => key.Equals("locale", StringComparison.OrdinalIgnoreCase)))
            {
                throw new InvalidDataException(
                    $"Frontmatter 'locale' is no longer supported in '{sourcePath}'. Remove it; keep primary content directly under pages/posts and put translations in a configured locale directory.");
            }
            var unsupportedKeys = map.Keys.Where(key => !SupportedMetadataKeys.Contains(key)).ToList();
            if (unsupportedKeys.Count > 0)
            {
                throw new InvalidDataException(
                    $"Unsupported frontmatter field(s) in '{sourcePath}': {string.Join(", ", unsupportedKeys)}.");
            }

            var title = map.TryGetValue("title", out var titleValue) ? Convert.ToString(titleValue, CultureInfo.InvariantCulture) ?? string.Empty : _fileSystem.Path.GetFileNameWithoutExtension(sourcePath);
            var slug = map.TryGetValue("slug", out var slugValue) ? Convert.ToString(slugValue, CultureInfo.InvariantCulture) ?? string.Empty : string.Empty;
            var description = map.TryGetValue("description", out var descValue) ? Convert.ToString(descValue, CultureInfo.InvariantCulture) : default;
            var author = map.TryGetValue("author", out var authorValue) ? Convert.ToString(authorValue, CultureInfo.InvariantCulture) : default;
            var twitterHandle = map.TryGetValue("twitter_handle", out var twitterValue) ? Convert.ToString(twitterValue, CultureInfo.InvariantCulture) : default;
            var heroImage = map.TryGetValue("hero_image", out var heroImageValue) ? Convert.ToString(heroImageValue, CultureInfo.InvariantCulture) : default;
            var draft = ReadBooleanMetadata(map, "draft", sourcePath);
            var showInNavigation = ReadBooleanMetadata(map, "show_in_navigation", sourcePath);

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
                Author = author,
                TwitterHandle = twitterHandle,
                HeroImage = heroImage,
                Tags = tags,
                Published = published,
                Draft = draft,
                ShowInNavigation = showInNavigation,
            }, markdownBody.Trim());
        }
        catch (YamlException ex)
        {
            throw new InvalidDataException($"Failed to parse frontmatter in '{sourcePath}'.", ex);
        }
    }

    private static bool ReadBooleanMetadata(IReadOnlyDictionary<string, object> map, string field, string sourcePath)
    {
        if (!map.TryGetValue(field, out var value))
        {
            return false;
        }

        if (!bool.TryParse(Convert.ToString(value, CultureInfo.InvariantCulture), out var result))
        {
            throw new InvalidDataException($"Frontmatter field '{field}' in '{sourcePath}' must be true or false.");
        }

        return result;
    }

    private ContentMetadata ApplySlug(ContentMetadata metadata, ContentKind kind, string file, string? locale)
    {
        var slug = metadata.Slug;

        if (kind == ContentKind.Post && _options.UseDateInPostUrl)
        {
            if (metadata.Published is { } published)
            {
                slug = string.Concat(published.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture), "/", slug);
            }
            else
            {
                _logger.LogWarning("UseDateInPostUrl is enabled but no published date was found for {Path}; using slug without date", file);
            }
        }

        if (locale is not null)
        {
            slug = string.IsNullOrEmpty(slug) ? locale : string.Concat(locale, "/", slug);
        }

        return metadata with { Slug = slug };
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

    private static string InferSlugFromFile(ContentKind kind, string relative)
    {
        var directory = Path.GetDirectoryName(relative) ?? string.Empty;
        var name = Path.GetFileNameWithoutExtension(relative);
        var withoutExtension = kind == ContentKind.Page
                               && directory.Length > 0
                               && name.Equals("index", StringComparison.OrdinalIgnoreCase)
            ? directory
            : Path.Combine(directory, name);
        return withoutExtension.Replace(Path.DirectorySeparatorChar, '/');
    }
}
