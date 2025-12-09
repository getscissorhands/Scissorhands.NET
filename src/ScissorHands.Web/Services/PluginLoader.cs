using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ScissorHands.Web.Models;
using ScissorHands.Web.Plugins;

namespace ScissorHands.Web.Services;

public sealed class PluginLoader
{
    private readonly ILogger<PluginLoader> _logger;
    private readonly IServiceProvider _services;
    private readonly IReadOnlyList<PluginDefinition> _definitions;

    public PluginLoader(IOptions<List<PluginDefinition>> pluginOptions, IServiceProvider services, ILogger<PluginLoader> logger)
    {
        _services = services;
        _logger = logger;
        _definitions = pluginOptions.Value ?? new List<PluginDefinition>();
    }

    public IReadOnlyList<IContentPlugin> Load()
    {
        var plugins = new List<IContentPlugin>();

        foreach (var definition in _definitions)
        {
            if (string.IsNullOrWhiteSpace(definition.Assembly) || string.IsNullOrWhiteSpace(definition.Type))
            {
                _logger.LogDebug("Skipping plugin with missing assembly/type information");
                continue;
            }

            try
            {
                var assembly = Assembly.Load(new AssemblyName(definition.Assembly));
                var pluginType = assembly.GetType(definition.Type, throwOnError: false, ignoreCase: false);

                if (pluginType is null || !typeof(IContentPlugin).IsAssignableFrom(pluginType))
                {
                    _logger.LogWarning("Plugin type {PluginType} could not be loaded or does not implement IContentPlugin", definition.Type);
                    continue;
                }

                if (ActivatorUtilities.CreateInstance(_services, pluginType) is IContentPlugin plugin)
                {
                    plugins.Add(plugin);
                }
                else
                {
                    _logger.LogWarning("Plugin {PluginType} could not be instantiated", definition.Type);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load plugin {PluginType} from {Assembly}", definition.Type, definition.Assembly);
            }
        }

        return plugins;
    }
}
