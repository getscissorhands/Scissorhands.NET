using ScissorHands.Core.Manifests;
using ScissorHands.Core.Validation;
using ScissorHands.Plugin;

namespace ScissorHands.Web.Runners;

internal static class PluginDependencyResolver
{
    internal static IReadOnlyDictionary<PluginStage, IReadOnlyList<IContentPlugin>> Resolve(
        IReadOnlyList<IContentPlugin> plugins,
        IReadOnlyDictionary<string, PluginManifest> manifests)
    {
        var installed = plugins.ToDictionary(plugin => plugin.Id, StringComparer.Ordinal);
        var enabled = plugins.Where(plugin => manifests.ContainsKey(plugin.Id))
                             .ToDictionary(plugin => plugin.Id, StringComparer.Ordinal);
        var dependenciesById = new Dictionary<string, PluginDependency[]>(StringComparer.Ordinal);
        foreach (var plugin in enabled.Values)
        {
            var dependencies = plugin is IContentPluginDependencies declaration
                ? declaration.DependsOn?.ToArray()
                    ?? throw new InvalidOperationException($"Plugin '{plugin.Id}' must return a non-null DependsOn list.")
                : [];
            foreach (var dependency in dependencies)
            {
                if (dependency is null)
                {
                    throw new InvalidOperationException($"Plugin '{plugin.Id}' has a null dependency declaration.");
                }

                if (!Enum.IsDefined(dependency.Stage))
                {
                    throw new InvalidOperationException(
                        $"Plugin '{plugin.Id}' declares dependency '{dependency.PluginId}' with invalid stage '{dependency.Stage}'.");
                }

                PluginIdValidator.Validate(dependency.PluginId, $"Plugin '{plugin.Id}' dependency in stage '{dependency.Stage}'");
                if (string.Equals(plugin.Id, dependency.PluginId, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException(
                        $"Plugin '{plugin.Id}' cannot depend on itself in stage '{dependency.Stage}'.");
                }

                if (!installed.ContainsKey(dependency.PluginId))
                {
                    throw new InvalidOperationException(
                        $"Plugin '{plugin.Id}' requires plugin '{dependency.PluginId}' in stage '{dependency.Stage}', but it is not installed.");
                }

                if (!enabled.ContainsKey(dependency.PluginId))
                {
                    throw new InvalidOperationException(
                        $"Plugin '{plugin.Id}' requires plugin '{dependency.PluginId}' in stage '{dependency.Stage}', but it is not enabled. Add its manifest to the Plugins configuration.");
                }
            }

            dependenciesById.Add(plugin.Id, dependencies);
        }

        return Enum.GetValues<PluginStage>()
                   .ToDictionary(stage => stage, stage => ResolveStage(stage, enabled, dependenciesById));
    }

    private static IReadOnlyList<IContentPlugin> ResolveStage(
        PluginStage stage,
        IReadOnlyDictionary<string, IContentPlugin> enabled,
        IReadOnlyDictionary<string, PluginDependency[]> dependenciesById)
    {
        var remainingDependencies = new Dictionary<string, int>(StringComparer.Ordinal);
        var dependents = enabled.Keys.ToDictionary(id => id, _ => new List<string>(), StringComparer.Ordinal);
        foreach (var id in enabled.Keys)
        {
            var requirements = new HashSet<string>(StringComparer.Ordinal);
            foreach (var dependency in dependenciesById[id].Where(dependency => dependency.Stage == stage))
            {
                if (!requirements.Add(dependency.PluginId))
                {
                    throw new InvalidOperationException(
                        $"Plugin '{id}' declares dependency '{dependency.PluginId}' more than once in stage '{stage}'.");
                }

                dependents[dependency.PluginId].Add(id);
            }

            remainingDependencies.Add(id, requirements.Count);
        }

        var ready = new SortedSet<string>(
            remainingDependencies.Where(pair => pair.Value == 0).Select(pair => pair.Key),
            StringComparer.Ordinal);
        var ordered = new List<IContentPlugin>(enabled.Count);
        while (ready.Count > 0)
        {
            var id = ready.First();
            ready.Remove(id);
            ordered.Add(enabled[id]);
            foreach (var dependent in dependents[id])
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
                                                  .Order(StringComparer.Ordinal);
            throw new InvalidOperationException(
                $"Plugin dependency cycle detected in stage '{stage}'. Unresolved plugins: {string.Join(", ", unresolved)}.");
        }

        return ordered;
    }
}
