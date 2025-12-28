using ScissorHands.Web.Watchers;

namespace ScissorHands.Web.Abstractions;

/// <summary>
/// Provides factory methods to create filesystem watchers.
/// </summary>
public interface IContentWatcherFactory
{
    /// <summary>
    /// Creates a new instance of <see cref="ContentWatcher"/> class.
    /// </summary>
    /// <param name="contentPath">Full path to the content directory.</param>
    /// <param name="themePath">Full path to the theme directory.</param>
    /// <param name="debounce">Time span to debounce file system events.</param>
    /// <param name="onChange">Callback function to invoke when a change is detected.</param>
    /// <returns>A new instance of <see cref="ContentWatcher"/>.</returns>
    ContentWatcher Create(string contentPath, string themePath, TimeSpan debounce, Func<Task> onChange);
}
