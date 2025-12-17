namespace ScissorHands.Core.Services;

/// <summary>
/// This provides the interface for markdown service.
/// </summary>
public interface IMarkdownService
{
    /// <summary>
    /// Converts the specified markdown to HTML.
    /// </summary>
    /// <param name="markdown">Markdown text.</param>
    /// <param name="trim">Indicates whether to trim the starting and ending paragraph elements from the resulting HTML.</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/> value.</param>
    /// <returns>Returns the HTML string converted from the markdown.</returns>
    Task<string> ToHtmlAsync(string markdown, bool? trim = false, CancellationToken cancellationToken = default);
}
