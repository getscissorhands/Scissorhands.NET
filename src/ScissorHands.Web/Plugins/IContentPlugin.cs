using ScissorHands.Web.Models;

namespace ScissorHands.Web.Plugins;

public interface IContentPlugin
{
    Task<ContentDocument> PreMarkdownAsync(ContentDocument document, CancellationToken cancellationToken = default);
    Task<ContentDocument> PostMarkdownAsync(ContentDocument document, CancellationToken cancellationToken = default);
    Task<string> PostHtmlAsync(string html, ContentDocument document, CancellationToken cancellationToken = default);
}
