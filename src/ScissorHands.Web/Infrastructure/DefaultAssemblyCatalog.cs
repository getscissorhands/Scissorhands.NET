using System.Reflection;

using ScissorHands.Web.Abstractions;

namespace ScissorHands.Web.Infrastructure;

/// <summary>
/// This represents the default assembly catalog entity.
/// </summary>
public sealed class DefaultAssemblyCatalog : IAssemblyCatalog
{
    /// <inheritdoc />
    public IReadOnlyCollection<Assembly> GetAssemblies()
    {
        var fromAppDomain = AppDomain.CurrentDomain.GetAssemblies();
        var loadedAssemblies = fromAppDomain
            .Where(assembly => !assembly.IsDynamic)
            .GroupBy(assembly => assembly.GetName().Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key!, group => group.First(), StringComparer.OrdinalIgnoreCase);

        foreach (var path in Directory.GetFiles(AppContext.BaseDirectory, "*.dll"))
        {
            try
            {
                var assemblyName = AssemblyName.GetAssemblyName(path);
                if (!loadedAssemblies.ContainsKey(assemblyName.Name!))
                {
                    loadedAssemblies[assemblyName.Name!] = Assembly.Load(assemblyName);
                }
            }
            catch (BadImageFormatException)
            {
                // Native binaries in the output directory are not plugin assemblies.
            }
            catch (FileLoadException)
            {
                // Ignore assemblies that cannot be loaded in the current context.
            }
            catch (FileNotFoundException)
            {
                // A dependency may disappear while the application is starting.
            }
        }

        return [.. loadedAssemblies.Values];
    }
}
