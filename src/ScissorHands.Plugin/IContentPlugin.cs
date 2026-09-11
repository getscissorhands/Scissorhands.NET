using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;

namespace ScissorHands.Plugin;

/// <summary>
/// This provides an interface for content plugins.
/// </summary>
public interface IContentPlugin
{
    /// <summary>
    /// Gets the stable, unique lowercase kebab-case ID of the plugin.
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Gets the display name of the plugin. This does not identify the plugin.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Invokes before processing markdown.
    /// </summary>
    /// <param name="document"><see cref="ContentDocument"/> instance.</param>
    /// <param name="plugin"><see cref="PluginManifest"/> instance.</param>
    /// <param name="site"><see cref="SiteManifest"/> instance.</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/> value.</param>
    /// <returns>Returns the updated <see cref="ContentDocument"/> instance.</returns>
    Task<ContentDocument> PreMarkdownAsync(ContentDocument document, PluginManifest plugin, SiteManifest site, CancellationToken cancellationToken = default);

    /// <summary>
    /// Invokes after processing markdown.
    /// </summary>
    /// <param name="document"><see cref="ContentDocument"/> instance.</param>
    /// <param name="plugin"><see cref="PluginManifest"/> instance.</param>
    /// <param name="site"><see cref="SiteManifest"/> instance.</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/> value.</param>
    /// <returns>Returns the updated <see cref="ContentDocument"/> instance.</returns>
    Task<ContentDocument> PostMarkdownAsync(ContentDocument document, PluginManifest plugin, SiteManifest site, CancellationToken cancellationToken = default);

    /// <summary>
    /// Invokes after processing HTML.
    /// </summary>
    /// <param name="html">HTML content.</param>
    /// <param name="document"><see cref="ContentDocument"/> instance.</param>
    /// <param name="plugin"><see cref="PluginManifest"/> instance.</param>
    /// <param name="site"><see cref="SiteManifest"/> instance.</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/> value.</param>
    /// <returns>Returns the updated HTML.</returns>
    Task<string> PostHtmlAsync(string html, ContentDocument document, PluginManifest plugin, SiteManifest site, CancellationToken cancellationToken = default);
}
