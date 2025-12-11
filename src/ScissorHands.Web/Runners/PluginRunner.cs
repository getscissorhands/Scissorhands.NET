using ScissorHands.Core.Models;
using ScissorHands.Plugin;
using ScissorHands.Web.Loaders;

namespace ScissorHands.Web.Runners;

public interface IPluginRunner
{
    Task<ContentDocument> RunPreMarkdownAsync(ContentDocument document, CancellationToken cancellationToken);

    Task<ContentDocument> RunPostMarkdownAsync(ContentDocument document, CancellationToken cancellationToken);

    Task<string> RunPostHtmlAsync(string html, ContentDocument document, CancellationToken cancellationToken);
}

public sealed class PluginRunner(IPluginLoader loader) : IPluginRunner
{
    private readonly IReadOnlyList<IContentPlugin> _plugins = (loader ?? throw new ArgumentNullException(nameof(loader))).Load();

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
