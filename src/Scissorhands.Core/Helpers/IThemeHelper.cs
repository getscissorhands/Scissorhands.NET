using System;

namespace Scissorhands.Helpers
{
    /// <summary>
    /// This provides interfaces to the theme helper class.
    /// </summary>
    public interface IThemeHelper : IDisposable
    {
        /// <summary>
        /// Gets the path for head partial view.
        /// </summary>
        /// <param name="themeName">Theme name.</param>
        /// <returns>Returns the path for head partial view.</returns>
        string GetHeadPartialViewPath(string themeName = null);

        /// <summary>
        /// Gets the path for header partial view.
        /// </summary>
        /// <param name="themeName">Theme name.</param>
        /// <returns>Returns the path for header partial view.</returns>
        string GetHeaderPartialViewPath(string themeName = null);

        /// <summary>
        /// Gets the path for post partial view.
        /// </summary>
        /// <param name="themeName">Theme name.</param>
        /// <returns>Returns the path for post partial view.</returns>
        string GetPostPartialViewPath(string themeName = null);

        /// <summary>
        /// Gets the path for footer partial view.
        /// </summary>
        /// <param name="themeName">Theme name.</param>
        /// <returns>Returns the path for footer partial view.</returns>
        string GetFooterPartialViewPath(string themeName = null);
    }
}