using System.Reflection;
using System.Text;

using ScissorHands.Theme;
using ScissorHands.Web.Abstractions;

namespace ScissorHands.Web.Services;

/// <summary>
/// Resolves theme components from loaded application assemblies.
/// </summary>
public sealed class ThemeComponentResolver(IAssemblyCatalog assemblyCatalog) : IThemeComponentResolver
{
    private const string DEFAULT_THEME_SLUG = "default";

    private readonly IAssemblyCatalog _assemblyCatalog = assemblyCatalog ?? throw new ArgumentNullException(nameof(assemblyCatalog));

    /// <inheritdoc />
    public ThemeComponentSet Resolve(string themeSlug)
    {
        var candidates = DiscoverCandidates();
        if (candidates.Count == 0)
        {
            throw new InvalidOperationException(
                "No complete ScissorHands theme was found. A theme must provide MainLayout, IndexView, PostView, PageView, and NotFoundView components.");
        }

        var normalizedSlug = Normalize(themeSlug);
        if (string.IsNullOrEmpty(normalizedSlug) || string.Equals(normalizedSlug, DEFAULT_THEME_SLUG, StringComparison.Ordinal))
        {
            return candidates.SingleOrDefault(candidate => candidate.IsBuiltIn)?.Components
                   ?? throw CreateResolutionException(themeSlug, candidates);
        }

        var matchingCandidates = candidates
            .Where(candidate => Normalize(candidate.Namespace).EndsWith(normalizedSlug, StringComparison.Ordinal))
            .ToList();

        if (matchingCandidates.Count == 1)
        {
            return matchingCandidates[0].Components;
        }

        throw CreateResolutionException(themeSlug, candidates);
    }

    private IReadOnlyList<ThemeCandidate> DiscoverCandidates()
    {
        var componentTypes = _assemblyCatalog.GetAssemblies()
            .SelectMany(GetLoadableTypes)
            .Where(type => type is { IsAbstract: false, Namespace: not null })
            .Where(IsThemeComponent)
            .GroupBy(type => type.Namespace!, StringComparer.Ordinal)
            .Select(CreateCandidate)
            .Where(candidate => candidate is not null)
            .Cast<ThemeCandidate>()
            .ToList();

        return componentTypes;
    }

    private static ThemeCandidate? CreateCandidate(IGrouping<string, Type> group)
    {
        var mainLayout = FindSingle<MainLayoutBase>(group);
        var indexView = FindSingle<IndexViewBase>(group);
        var postView = FindSingle<PostViewBase>(group);
        var pageView = FindSingle<PageViewBase>(group);
        var notFoundView = FindSingle<NotFoundViewBase>(group);

        if (mainLayout is null || indexView is null || postView is null || pageView is null || notFoundView is null)
        {
            return null;
        }

        var tagListView = FindSingle<TagListViewBase>(group) ?? typeof(ScissorHands.Web.TagListView);
        var tagView = FindSingle<TagViewBase>(group) ?? typeof(ScissorHands.Web.TagView);
        var components = new ThemeComponentSet(mainLayout, indexView, postView, pageView, notFoundView, tagListView, tagView);

        return new ThemeCandidate(
            group.Key,
            string.Equals(group.Key, typeof(ScissorHands.Web.MainLayout).Namespace, StringComparison.Ordinal),
            components);
    }

    private static Type? FindSingle<TBase>(IEnumerable<Type> types)
    {
        var matches = types.Where(type => typeof(TBase).IsAssignableFrom(type)).Take(2).ToList();
        return matches.Count == 1 ? matches[0] : null;
    }

    private static bool IsThemeComponent(Type type)
    {
        return typeof(MainLayoutBase).IsAssignableFrom(type)
               || typeof(IndexViewBase).IsAssignableFrom(type)
               || typeof(PostViewBase).IsAssignableFrom(type)
               || typeof(PageViewBase).IsAssignableFrom(type)
               || typeof(NotFoundViewBase).IsAssignableFrom(type)
               || typeof(TagListViewBase).IsAssignableFrom(type)
               || typeof(TagViewBase).IsAssignableFrom(type);
    }

    private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.OfType<Type>();
        }
    }

    private static InvalidOperationException CreateResolutionException(string themeSlug, IEnumerable<ThemeCandidate> candidates)
    {
        var namespaces = string.Join(", ", candidates.Select(candidate => candidate.Namespace).OrderBy(value => value, StringComparer.Ordinal));
        return new InvalidOperationException(
            $"Unable to resolve the configured theme '{themeSlug}'. Discovered theme namespaces: {namespaces}. " +
            "Use a namespace ending with the normalized theme slug, or configure layouts explicitly with AddLayouts(...).");
    }

    private static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var builder = new StringBuilder(value.Length);
        foreach (var character in value)
        {
            if (char.IsLetterOrDigit(character))
            {
                builder.Append(char.ToLowerInvariant(character));
            }
        }

        return builder.ToString();
    }

    private sealed record ThemeCandidate(string Namespace, bool IsBuiltIn, ThemeComponentSet Components);
}
