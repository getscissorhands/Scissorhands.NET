using ScissorHands.Core.Models;

namespace ScissorHands.Plugin;

public abstract class ContentPlugin : IContentPlugin
{
    public virtual Task<ContentDocument> PreMarkdownAsync(ContentDocument document, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(document);
    }

    public virtual Task<ContentDocument> PostMarkdownAsync(ContentDocument document, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(document);
    }

    public virtual Task<string> PostHtmlAsync(string html, ContentDocument document, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(html);
    }
}