using ScissorHands.Core.Models;

namespace ScissorHands.Plugin;

public interface IContentPlugin
{
    Task<ContentDocument> PreMarkdownAsync(ContentDocument document, CancellationToken cancellationToken = default);
    Task<ContentDocument> PostMarkdownAsync(ContentDocument document, CancellationToken cancellationToken = default);
    Task<string> PostHtmlAsync(string html, ContentDocument document, CancellationToken cancellationToken = default);
}
