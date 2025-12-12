// using System.Reflection;

// using Microsoft.Extensions.DependencyInjection;
// using Microsoft.Extensions.Logging;

// using ScissorHands.Core.Manifests;
// using ScissorHands.Plugin;

// namespace ScissorHands.Web.Loaders;

// public interface IPluginLoader
// {
//     IReadOnlyList<IContentPlugin> Load();
// }

// public sealed class PluginLoader(List<PluginManifest> pluginOptions, IServiceProvider services, ILogger<PluginLoader> logger) : IPluginLoader
// {
//     private readonly IReadOnlyList<PluginManifest> _definitions = pluginOptions ?? throw new ArgumentNullException(nameof(pluginOptions));
//     private readonly IServiceProvider _services = services ?? throw new ArgumentNullException(nameof(services));
//     private readonly ILogger<PluginLoader> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

//     public IReadOnlyList<IContentPlugin> Load()
//     {
//         var plugins = new List<IContentPlugin>();

//         foreach (var definition in _definitions)
//         {
//             if (string.IsNullOrWhiteSpace(definition.Assembly) || string.IsNullOrWhiteSpace(definition.Type))
//             {
//                 _logger.LogDebug("Skipping plugin with missing assembly/type information");
//                 continue;
//             }

//             try
//             {
//                 var assembly = Assembly.Load(new AssemblyName(definition.Assembly));
//                 var pluginType = assembly.GetType(definition.Type, throwOnError: false, ignoreCase: false);

//                 if (pluginType is null || !typeof(IContentPlugin).IsAssignableFrom(pluginType))
//                 {
//                     _logger.LogWarning("Plugin type {PluginType} could not be loaded or does not implement IContentPlugin", definition.Type);
//                     continue;
//                 }

//                 if (ActivatorUtilities.CreateInstance(_services, pluginType) is IContentPlugin plugin)
//                 {
//                     plugins.Add(plugin);
//                 }
//                 else
//                 {
//                     _logger.LogWarning("Plugin {PluginType} could not be instantiated", definition.Type);
//                 }
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogWarning(ex, "Failed to load plugin {PluginType} from {Assembly}", definition.Type, definition.Assembly);
//             }
//         }

//         return plugins;
//     }
// }
