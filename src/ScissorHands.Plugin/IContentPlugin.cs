using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;

namespace ScissorHands.Plugin;

public interface IContentPlugin
{
    string Name { get; }
    Task<ContentDocument> PreMarkdownAsync(ContentDocument document, PluginManifest? manifest, CancellationToken cancellationToken = default);
    Task<ContentDocument> PostMarkdownAsync(ContentDocument document, PluginManifest? manifest, CancellationToken cancellationToken = default);
    Task<string> PostHtmlAsync(string html, ContentDocument document, PluginManifest? manifest, CancellationToken cancellationToken = default);
}
