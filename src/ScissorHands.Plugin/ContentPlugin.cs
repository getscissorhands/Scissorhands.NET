using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;

namespace ScissorHands.Plugin;

public abstract class ContentPlugin : IContentPlugin
{
    public abstract string Name { get; }

    public virtual Task<ContentDocument> PreMarkdownAsync(ContentDocument document, PluginManifest? manifest, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(document);
    }

    public virtual Task<ContentDocument> PostMarkdownAsync(ContentDocument document, PluginManifest? manifest, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(document);
    }

    public virtual Task<string> PostHtmlAsync(string html, ContentDocument document, PluginManifest? manifest, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(html);
    }
}