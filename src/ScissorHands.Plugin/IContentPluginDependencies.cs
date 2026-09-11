namespace ScissorHands.Plugin;

/// <summary>
/// Provides optional dependency declarations for a content plugin.
/// </summary>
public interface IContentPluginDependencies
{
    /// <summary>
    /// Gets the stage-scoped dependencies. An empty list declares no ordering requirements.
    /// Each dependency must be installed and enabled.
    /// </summary>
    IReadOnlyList<PluginDependency> DependsOn { get; }
}
