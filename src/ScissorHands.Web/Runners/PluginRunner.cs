using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;
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
    private readonly IReadOnlyDictionary<string, PluginManifest> _manifestsByName;
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
        _manifestsByName = CreateManifestLookup(Manifests);
        ValidatePluginConfiguration(Plugins, _manifestsByName);
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
        foreach (var plugin in Plugins)
        {
            if (!_manifestsByName.TryGetValue(plugin.Name, out var manifest))
            {
                continue;
            }

            current = await plugin.PreMarkdownAsync(current, manifest, _site, cancellationToken);
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
            if (!_manifestsByName.TryGetValue(plugin.Name, out var manifest))
            {
                continue;
            }

            current = await plugin.PostMarkdownAsync(current, manifest, _site, cancellationToken);
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
            if (!_manifestsByName.TryGetValue(plugin.Name, out var manifest))
            {
                continue;
            }

            current = await plugin.PostHtmlAsync(current, document, manifest, _site, cancellationToken);
        }

        return current;
    }

    private static IReadOnlyDictionary<string, PluginManifest> CreateManifestLookup(IEnumerable<PluginManifest> manifests)
    {
        var result = new Dictionary<string, PluginManifest>(StringComparer.OrdinalIgnoreCase);
        foreach (var manifest in manifests)
        {
            if (string.IsNullOrWhiteSpace(manifest.Name))
            {
                throw new InvalidOperationException("Every configured plugin manifest must have a non-empty name.");
            }

            if (!result.TryAdd(manifest.Name, manifest))
            {
                throw new InvalidOperationException($"Plugin manifest '{manifest.Name}' is configured more than once.");
            }
        }

        return result;
    }

    private static void ValidatePluginConfiguration(
        IEnumerable<IContentPlugin> plugins,
        IReadOnlyDictionary<string, PluginManifest> manifests)
    {
        var pluginNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var plugin in plugins)
        {
            if (string.IsNullOrWhiteSpace(plugin.Name))
            {
                throw new InvalidOperationException("Every installed plugin must have a non-empty name.");
            }

            if (!pluginNames.Add(plugin.Name))
            {
                throw new InvalidOperationException($"Installed plugin name '{plugin.Name}' is not unique.");
            }
        }

        foreach (var manifestName in manifests.Keys)
        {
            if (!pluginNames.Contains(manifestName))
            {
                throw new InvalidOperationException(
                    $"Plugin manifest '{manifestName}' does not match any installed plugin.");
            }
        }
    }
}
