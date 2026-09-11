namespace ScissorHands.Plugin;

/// <summary>
/// Identifies a content plugin execution stage.
/// </summary>
public enum PluginStage
{
    /// <summary>
    /// Runs before Markdown conversion.
    /// </summary>
    PreMarkdown,

    /// <summary>
    /// Runs after Markdown conversion and before Razor rendering.
    /// </summary>
    PostMarkdown,

    /// <summary>
    /// Runs after Razor rendering.
    /// </summary>
    PostHtml,
}
