using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;
using ScissorHands.Plugin;

namespace ScissorHands.Web.Runners;

/// <summary>
/// This represents the plugin runner entity.
/// </summary>
/// <param name="manifests">List of <see cref="PluginManifest"/> instances.</param>
/// <param name="plugins">List of <see cref="IContentPlugin"/> instances.</param>
public sealed class PluginRunner(IEnumerable<PluginManifest> manifests, IEnumerable<IContentPlugin> plugins) : IPluginRunner
{
    /// <inheritdoc />
    public IReadOnlyList<PluginManifest> Manifests { get; init; } = [.. manifests ?? throw new ArgumentNullException(nameof(manifests))];

    /// <inheritdoc />
    public IReadOnlyList<IContentPlugin> Plugins { get; init; } = [.. plugins ?? throw new ArgumentNullException(nameof(plugins))];

    /// <inheritdoc />
    public async Task<ContentDocument> RunPreMarkdownAsync(ContentDocument document, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var current = document;
        foreach (var plugin in Plugins)
        {
            var manifest = Manifests.SingleOrDefault(m => m.Name == plugin.Name);
            if (manifest == null)
            {
                continue;
            }

            current = await plugin.PreMarkdownAsync(current, manifest, cancellationToken);
        }

        return current;
    }

    /// <inheritdoc />
    public async Task<ContentDocument> RunPostMarkdownAsync(ContentDocument document, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var current = document;
        foreach (var plugin in Plugins)
        {
            var manifest = Manifests.SingleOrDefault(m => m.Name == plugin.Name);
            if (manifest == null)
            {
                continue;
            }

            current = await plugin.PostMarkdownAsync(current, manifest, cancellationToken);
        }

        return current;
    }

    /// <inheritdoc />
    public async Task<string> RunPostHtmlAsync(string html, ContentDocument document, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var current = html;
        foreach (var plugin in Plugins)
        {
            var manifest = Manifests.SingleOrDefault(m => m.Name == plugin.Name);
            if (manifest == null)
            {
                continue;
            }

            current = await plugin.PostHtmlAsync(current, document, manifest, cancellationToken);
        }

        return current;
    }
}
