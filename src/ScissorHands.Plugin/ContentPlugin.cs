using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;

namespace ScissorHands.Plugin;

/// <summary>
/// This represents the base entity for content plugins.
/// </summary>
public abstract class ContentPlugin : IContentPlugin, IContentPluginDependencies
{
    /// <inheritdoc />
    public abstract string Id { get; }

    /// <inheritdoc />
    public abstract string Name { get; }

    /// <inheritdoc />
    public virtual IReadOnlyList<PluginDependency> DependsOn => [];

    /// <inheritdoc />
    public virtual Task<ContentDocument> PreMarkdownAsync(ContentDocument document, PluginManifest plugin, SiteManifest site, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(document);
    }

    /// <inheritdoc />
    public virtual Task<ContentDocument> PostMarkdownAsync(ContentDocument document, PluginManifest plugin, SiteManifest site, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(document);
    }

    /// <inheritdoc />
    public virtual Task<string> PostHtmlAsync(string html, ContentDocument document, PluginManifest plugin, SiteManifest site, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(html);
    }
}
