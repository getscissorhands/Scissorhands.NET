using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;
using ScissorHands.Plugin;

namespace ScissorHands.Web.Runners;

/// <summary>
/// This provides an interface to <see cref="PluginRunner"/>.
public interface IPluginRunner
{
    /// <summary>
    /// Gets the list of <see cref="PluginManifest"/> instances.
    IReadOnlyList<PluginManifest> Manifests { get; }

    /// <summary>
    /// Gets the list of <see cref="IContentPlugin"/> instances.
    /// </summary>
    IReadOnlyList<IContentPlugin> Plugins { get; }

    /// <summary>
    /// Invokes before processing markdown.
    /// </summary>
    /// <param name="document"><see cref="ContentDocument"/> instance.</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/> value.</param>
    /// <returns>Returns the updated <see cref="ContentDocument"/> instance.</returns>
    Task<ContentDocument> RunPreMarkdownAsync(ContentDocument document, CancellationToken cancellationToken);

    /// <summary>
    /// Invokes after processing markdown.
    /// </summary>
    /// <param name="document"><see cref="ContentDocument"/> instance.</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/> value.</param>
    /// <returns>Returns the updated <see cref="ContentDocument"/> instance.</returns>
    Task<ContentDocument> RunPostMarkdownAsync(ContentDocument document, CancellationToken cancellationToken);

    /// <summary>
    /// Invokes after processing HTML.
    /// </summary>
    /// <param name="html">HTML content.</param>
    /// <param name="document"><see cref="ContentDocument"/> instance.</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/> value.</param>
    /// <returns>Returns the updated HTML.</returns>
    Task<string> RunPostHtmlAsync(string html, ContentDocument document, CancellationToken cancellationToken);
}
