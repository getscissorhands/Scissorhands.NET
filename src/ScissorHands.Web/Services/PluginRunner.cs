using ScissorHands.Core.Models;
using ScissorHands.Plugin;

namespace ScissorHands.Web.Services;

public sealed class PluginRunner
{
    private readonly IReadOnlyList<IContentPlugin> _plugins;

    public PluginRunner(PluginLoader loader)
    {
        _plugins = loader.Load();
    }

    public async Task<ContentDocument> RunPreMarkdownAsync(ContentDocument document, CancellationToken cancellationToken)
    {
        var current = document;
        foreach (var plugin in _plugins)
        {
            current = await plugin.PreMarkdownAsync(current, cancellationToken);
        }

        return current;
    }

    public async Task<ContentDocument> RunPostMarkdownAsync(ContentDocument document, CancellationToken cancellationToken)
    {
        var current = document;
        foreach (var plugin in _plugins)
        {
            current = await plugin.PostMarkdownAsync(current, cancellationToken);
        }

        return current;
    }

    public async Task<string> RunPostHtmlAsync(string html, ContentDocument document, CancellationToken cancellationToken)
    {
        var current = html;
        foreach (var plugin in _plugins)
        {
            current = await plugin.PostHtmlAsync(current, document, cancellationToken);
        }

        return current;
    }
}
