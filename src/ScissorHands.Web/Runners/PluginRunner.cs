using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;
using ScissorHands.Plugin;

namespace ScissorHands.Web.Runners;

public interface IPluginRunner
{
    Task<ContentDocument> RunPreMarkdownAsync(ContentDocument document, CancellationToken cancellationToken);

    Task<ContentDocument> RunPostMarkdownAsync(ContentDocument document, CancellationToken cancellationToken);

    Task<string> RunPostHtmlAsync(string html, ContentDocument document, CancellationToken cancellationToken);
}

public sealed class PluginRunner(IEnumerable<PluginManifest> manifests, IEnumerable<IContentPlugin> plugins) : IPluginRunner
{
    private readonly IReadOnlyList<PluginManifest> _manifests = [.. manifests ?? throw new ArgumentNullException(nameof(manifests))];
    private readonly IReadOnlyList<IContentPlugin> _plugins = [.. plugins ?? throw new ArgumentNullException(nameof(plugins))];

    public async Task<ContentDocument> RunPreMarkdownAsync(ContentDocument document, CancellationToken cancellationToken)
    {
        var current = document;
        foreach (var plugin in _plugins)
        {
            var manifest = _manifests.SingleOrDefault(m => m.Name == plugin.Name);
            current = await plugin.PreMarkdownAsync(current, manifest, cancellationToken);
        }

        return current;
    }

    public async Task<ContentDocument> RunPostMarkdownAsync(ContentDocument document, CancellationToken cancellationToken)
    {
        var current = document;
        foreach (var plugin in _plugins)
        {
            var manifest = _manifests.SingleOrDefault(m => m.Name == plugin.Name);
            current = await plugin.PostMarkdownAsync(current, manifest, cancellationToken);
        }

        return current;
    }

    public async Task<string> RunPostHtmlAsync(string html, ContentDocument document, CancellationToken cancellationToken)
    {
        var current = html;
        foreach (var plugin in _plugins)
        {
            var manifest = _manifests.SingleOrDefault(m => m.Name == plugin.Name);
            current = await plugin.PostHtmlAsync(current, document, manifest, cancellationToken);
        }

        return current;
    }
}
