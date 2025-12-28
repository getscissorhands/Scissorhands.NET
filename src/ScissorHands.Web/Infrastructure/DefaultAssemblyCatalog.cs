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
        var fromBaseDirectory = Directory.GetFiles(AppContext.BaseDirectory, "*.dll")
                                         .Select(Assembly.LoadFrom)
                                         .ToArray();

        var fromAppDomain = AppDomain.CurrentDomain.GetAssemblies();

        return [.. fromBaseDirectory
                   .Union(fromAppDomain)
                   .Distinct()];
    }
}
