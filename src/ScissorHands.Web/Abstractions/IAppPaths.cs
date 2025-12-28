namespace ScissorHands.Web.Abstractions;

/// <summary>
/// Provides application path resolution.
/// </summary>
public interface IAppPaths
{
    /// <summary>
    /// Gets the base path of the application.
    /// </summary>
    string BasePath { get; }

    /// <summary>
    /// Gets the contents root path.
    /// </summary>
    /// <returns>Returns the full path to the contents root directory.</returns>
    string GetContentsRoot();

    /// <summary>
    /// Gets the themes root path.
    /// </summary>
    /// <returns>Returns the full path to the themes root directory.</returns>
    string GetThemesRoot();
}
