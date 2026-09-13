using ScissorHands.Core.Models;

namespace ScissorHands.Web.Navigation;

internal static class PageReadingOrder
{
    internal static IReadOnlyList<ContentDocument> Order(
        IReadOnlyList<ContentDocument> pages,
        string? pagesRoot = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(pages);
        cancellationToken.ThrowIfCancellationRequested();

        var files = new List<(ContentDocument Document, string[] Segments)>();
        var sourceLess = new List<ContentDocument>();
        foreach (var page in pages)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (string.IsNullOrEmpty(page.SourcePath))
            {
                sourceLess.Add(page);
            }
            else
            {
                files.Add((page, GetSourceSegments(page.SourcePath, pagesRoot)));
            }
        }

        var ordered = files
            .OrderBy(item => item.Segments, SourcePathComparer.Instance)
            .Select(item => item.Document)
            .Concat(sourceLess
                .OrderBy(page => page.Metadata.Title, StringComparer.Ordinal)
                .ThenBy(page => page.Metadata.Slug, StringComparer.Ordinal));
        var result = new List<ContentDocument>(pages.Count);
        foreach (var page in ordered)
        {
            cancellationToken.ThrowIfCancellationRequested();
            result.Add(page);
        }

        cancellationToken.ThrowIfCancellationRequested();
        return result.AsReadOnly();
    }

    private static string[] GetSourceSegments(string sourcePath, string? pagesRoot)
    {
        var path = sourcePath.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);
        if (string.IsNullOrWhiteSpace(path)
            || path.EndsWith(Path.DirectorySeparatorChar)
            || path.Split(Path.DirectorySeparatorChar).Any(segment => segment is "." or ".."))
        {
            throw new InvalidDataException($"Page source path '{sourcePath}' must identify a file without relative path segments.");
        }

        try
        {
            if (pagesRoot is not null)
            {
                var root = Path.GetFullPath(pagesRoot);
                path = Path.GetRelativePath(root, Path.GetFullPath(path, root));
                if (Path.IsPathRooted(path)
                    || path is "." or ".."
                    || path.StartsWith($"..{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
                {
                    throw new InvalidDataException($"Page source path '{sourcePath}' must remain within the pages root '{root}'.");
                }
            }
            else if (Path.IsPathFullyQualified(path))
            {
                path = Path.GetFullPath(path);
                path = path[Path.GetPathRoot(path)!.Length..];
            }
            else if (Path.IsPathRooted(path))
            {
                throw new InvalidDataException($"Page source path '{sourcePath}' must be relative or fully qualified.");
            }

            var segments = path.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);
            var invalidCharacters = Path.GetInvalidFileNameChars();
            if (segments.Length == 0 || segments.Any(segment => segment.IndexOfAny(invalidCharacters) >= 0))
            {
                throw new InvalidDataException($"Page source path '{sourcePath}' contains an invalid filename.");
            }

            return segments;
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            throw new InvalidDataException($"Page source path '{sourcePath}' is invalid.", ex);
        }
    }

    private sealed class SourcePathComparer : IComparer<string[]>
    {
        public static SourcePathComparer Instance { get; } = new();

        public int Compare(string[]? left, string[]? right)
        {
            ArgumentNullException.ThrowIfNull(left);
            ArgumentNullException.ThrowIfNull(right);

            for (var index = 0; index < Math.Min(left.Length, right.Length); index++)
            {
                var leftIsIndex = index == left.Length - 1 && left[index].Equals("index.md", StringComparison.OrdinalIgnoreCase);
                var rightIsIndex = index == right.Length - 1 && right[index].Equals("index.md", StringComparison.OrdinalIgnoreCase);
                if (leftIsIndex != rightIsIndex)
                {
                    return leftIsIndex ? -1 : 1;
                }

                var comparison = StringComparer.Ordinal.Compare(left[index], right[index]);
                if (comparison != 0)
                {
                    return comparison;
                }
            }

            return left.Length.CompareTo(right.Length);
        }
    }
}
