namespace ScissorHands.Plugin;

/// <summary>
/// Declares a required plugin whose hook must run first in the specified stage.
/// </summary>
/// <param name="Name">The dependency's plugin name, matched case-insensitively.</param>
/// <param name="Stage">The stage in which the dependency must run before the declaring plugin.</param>
public sealed record PluginDependency(string Name, PluginStage Stage);
