using ScissorHands.Core.Manifests;
using ScissorHands.Plugin;

namespace ScissorHands.Web.Runners;

internal static class PluginDependencyResolver
{
    internal static IReadOnlyDictionary<PluginStage, IReadOnlyList<IContentPlugin>> Resolve(
        IReadOnlyList<IContentPlugin> plugins,
        IReadOnlyDictionary<string, PluginManifest> manifests)
    {
        var installed = plugins.ToDictionary(plugin => plugin.Name, StringComparer.OrdinalIgnoreCase);
        var enabled = plugins.Where(plugin => manifests.ContainsKey(plugin.Name))
                             .ToDictionary(plugin => plugin.Name, StringComparer.OrdinalIgnoreCase);
        var dependenciesByName = new Dictionary<string, PluginDependency[]>(StringComparer.OrdinalIgnoreCase);
        foreach (var plugin in enabled.Values)
        {
            var dependencies = plugin is IContentPluginDependencies declaration
                ? declaration.DependsOn?.ToArray()
                    ?? throw new InvalidOperationException($"Plugin '{plugin.Name}' must return a non-null DependsOn list.")
                : [];
            foreach (var dependency in dependencies)
            {
                if (dependency is null)
                {
                    throw new InvalidOperationException($"Plugin '{plugin.Name}' has a null dependency declaration.");
                }

                if (!Enum.IsDefined(dependency.Stage))
                {
                    throw new InvalidOperationException(
                        $"Plugin '{plugin.Name}' declares dependency '{dependency.Name}' with invalid stage '{dependency.Stage}'.");
                }

                if (string.IsNullOrWhiteSpace(dependency.Name))
                {
                    throw new InvalidOperationException(
                        $"Plugin '{plugin.Name}' has a dependency with an empty name in stage '{dependency.Stage}'.");
                }

                if (string.Equals(plugin.Name, dependency.Name, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException(
                        $"Plugin '{plugin.Name}' cannot depend on itself in stage '{dependency.Stage}'.");
                }

                if (!installed.ContainsKey(dependency.Name))
                {
                    throw new InvalidOperationException(
                        $"Plugin '{plugin.Name}' requires plugin '{dependency.Name}' in stage '{dependency.Stage}', but it is not installed.");
                }

                if (!enabled.ContainsKey(dependency.Name))
                {
                    throw new InvalidOperationException(
                        $"Plugin '{plugin.Name}' requires plugin '{dependency.Name}' in stage '{dependency.Stage}', but it is not enabled. Add its manifest to the Plugins configuration.");
                }
            }

            dependenciesByName.Add(plugin.Name, dependencies);
        }

        return Enum.GetValues<PluginStage>()
                   .ToDictionary(stage => stage, stage => ResolveStage(stage, enabled, dependenciesByName));
    }

    private static IReadOnlyList<IContentPlugin> ResolveStage(
        PluginStage stage,
        IReadOnlyDictionary<string, IContentPlugin> enabled,
        IReadOnlyDictionary<string, PluginDependency[]> dependenciesByName)
    {
        var remainingDependencies = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var dependents = enabled.Keys.ToDictionary(name => name, _ => new List<string>(), StringComparer.OrdinalIgnoreCase);
        foreach (var name in enabled.Keys)
        {
            var requirements = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var dependency in dependenciesByName[name].Where(dependency => dependency.Stage == stage))
            {
                if (!requirements.Add(dependency.Name))
                {
                    throw new InvalidOperationException(
                        $"Plugin '{name}' declares dependency '{dependency.Name}' more than once in stage '{stage}'.");
                }

                dependents[dependency.Name].Add(name);
            }

            remainingDependencies.Add(name, requirements.Count);
        }

        var ready = new SortedSet<string>(
            remainingDependencies.Where(pair => pair.Value == 0).Select(pair => pair.Key),
            StringComparer.OrdinalIgnoreCase);
        var ordered = new List<IContentPlugin>(enabled.Count);
        while (ready.Count > 0)
        {
            var name = ready.First();
            ready.Remove(name);
            ordered.Add(enabled[name]);
            foreach (var dependent in dependents[name])
            {
                remainingDependencies[dependent]--;
                if (remainingDependencies[dependent] == 0)
                {
                    ready.Add(dependent);
                }
            }
        }

        if (ordered.Count != enabled.Count)
        {
            var unresolved = remainingDependencies.Where(pair => pair.Value > 0)
                                                  .Select(pair => pair.Key)
                                                  .Order(StringComparer.OrdinalIgnoreCase);
            throw new InvalidOperationException(
                $"Plugin dependency cycle detected in stage '{stage}'. Unresolved plugins: {string.Join(", ", unresolved)}.");
        }

        return ordered;
    }
}
