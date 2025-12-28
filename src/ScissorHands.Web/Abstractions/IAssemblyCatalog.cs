using System.Reflection;

namespace ScissorHands.Web.Abstractions;

/// <summary>
/// Provides a deterministic set of assemblies for scanning.
/// </summary>
public interface IAssemblyCatalog
{
    /// <summary>
    /// Gets the list of assemblies to be scanned.
    /// </summary>
    /// <returns>Returns the list of assemblies to be scanned.</returns>
    IReadOnlyCollection<Assembly> GetAssemblies();
}
