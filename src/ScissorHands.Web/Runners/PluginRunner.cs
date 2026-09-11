using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;
using ScissorHands.Core.Validation;
using ScissorHands.Plugin;

namespace ScissorHands.Web.Runners;

/// <summary>
/// This represents the plugin runner entity.
/// </summary>
/// <param name="manifests">List of <see cref="PluginManifest"/> instances.</param>
/// <param name="plugins">List of <see cref="IContentPlugin"/> instances.</param>
/// <param name="site"><see cref="SiteManifest"/> instance.</param>
public sealed class PluginRunner : IPluginRunner
{
    private readonly IReadOnlyDictionary<string, PluginManifest> _manifestsById;
    private readonly IReadOnlyDictionary<PluginStage, IReadOnlyList<IContentPlugin>> _pluginsByStage;
    private readonly SiteManifest _site;

    /// <summary>
    /// Initializes a new instance of the <see cref="PluginRunner"/> class.
    /// </summary>
    public PluginRunner(IEnumerable<PluginManifest> manifests, IEnumerable<IContentPlugin> plugins, SiteManifest site)
    {
        ArgumentNullException.ThrowIfNull(manifests);
        ArgumentNullException.ThrowIfNull(plugins);

        _site = site ?? throw new ArgumentNullException(nameof(site));
        Manifests = [.. manifests];
        Plugins = [.. plugins];
        _manifestsById = CreateManifestLookup(Manifests);
        ValidatePluginConfiguration(Plugins, _manifestsById);
        _pluginsByStage = PluginDependencyResolver.Resolve(Plugins, _manifestsById);
    }

    /// <inheritdoc />
    public IReadOnlyList<PluginManifest> Manifests { get; }

    /// <inheritdoc />
    public IReadOnlyList<IContentPlugin> Plugins { get; }

    /// <inheritdoc />
    public async Task<ContentDocument> RunPreMarkdownAsync(ContentDocument document, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var current = document;
        foreach (var plugin in _pluginsByStage[PluginStage.PreMarkdown])
        {
            var manifest = _manifestsById[plugin.Id];
            current = await plugin.PreMarkdownAsync(current, manifest, _site, cancellationToken);
        }

        return current;
    }

    /// <inheritdoc />
    public async Task<ContentDocument> RunPostMarkdownAsync(ContentDocument document, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var current = document;
        foreach (var plugin in _pluginsByStage[PluginStage.PostMarkdown])
        {
            var manifest = _manifestsById[plugin.Id];
            current = await plugin.PostMarkdownAsync(current, manifest, _site, cancellationToken);
        }

        return current;
    }

    /// <inheritdoc />
    public async Task<string> RunPostHtmlAsync(string html, ContentDocument document, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var current = html;
        foreach (var plugin in _pluginsByStage[PluginStage.PostHtml])
        {
            var manifest = _manifestsById[plugin.Id];
            current = await plugin.PostHtmlAsync(current, document, manifest, _site, cancellationToken);
        }

        return current;
    }

    private static IReadOnlyDictionary<string, PluginManifest> CreateManifestLookup(IEnumerable<PluginManifest> manifests)
    {
        var result = new Dictionary<string, PluginManifest>(StringComparer.Ordinal);
        foreach (var manifest in manifests)
        {
            PluginIdValidator.Validate(manifest.Id, "Configured plugin manifest");
            if (!result.TryAdd(manifest.Id, manifest))
            {
                throw new InvalidOperationException($"Plugin manifest ID '{manifest.Id}' is configured more than once.");
            }
        }

        return result;
    }

    private static void ValidatePluginConfiguration(
        IEnumerable<IContentPlugin> plugins,
        IReadOnlyDictionary<string, PluginManifest> manifests)
    {
        var pluginIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var plugin in plugins)
        {
            PluginIdValidator.Validate(plugin.Id, $"Installed plugin '{plugin.Name}'");
            if (string.IsNullOrWhiteSpace(plugin.Name))
            {
                throw new InvalidOperationException($"Installed plugin '{plugin.Id}' must have a non-empty display name.");
            }

            if (!pluginIds.Add(plugin.Id))
            {
                throw new InvalidOperationException($"Installed plugin ID '{plugin.Id}' is not unique.");
            }
        }

        foreach (var manifestId in manifests.Keys)
        {
            if (!pluginIds.Contains(manifestId))
            {
                throw new InvalidOperationException(
                    $"Plugin manifest ID '{manifestId}' does not match any installed plugin.");
            }
        }
    }
}
