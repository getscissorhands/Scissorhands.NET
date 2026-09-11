namespace ScissorHands.Plugin;

/// <summary>
/// Declares a required plugin whose hook must run first in the specified stage.
/// </summary>
/// <param name="PluginId">The dependency's lowercase kebab-case plugin ID, matched ordinally.</param>
/// <param name="Stage">The stage in which the dependency must run before the declaring plugin.</param>
public sealed record PluginDependency(string PluginId, PluginStage Stage);
