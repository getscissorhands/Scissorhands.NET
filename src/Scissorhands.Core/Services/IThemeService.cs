using System;

using Microsoft.AspNet.Mvc.Rendering;

namespace Scissorhands.Services
{
    /// <summary>
    /// This provides interfaces to the theme service class.
    /// </summary>
    public interface IThemeService : IDisposable
    {
        /// <summary>
        /// Gets the layout path for view.
        /// </summary>
        /// <param name="viewContext"><see cref="ViewContext"/> instance.</param>
        /// <returns>Returns the layout path for view.</returns>
        string GetLayout(ViewContext viewContext);

        /// <summary>
        /// Gets the head path for view.
        /// </summary>
        /// <param name="themeName">Theme name.</param>
        /// <returns>Returns the post path for view.</returns>
        string GetHead(string themeName = null);

        /// <summary>
        /// Gets the header path for view.
        /// </summary>
        /// <param name="themeName">Theme name.</param>
        /// <returns>Returns the post path for view.</returns>
        string GetHeader(string themeName = null);

        /// <summary>
        /// Gets the post path for view.
        /// </summary>
        /// <param name="themeName">Theme name.</param>
        /// <returns>Returns the post path for view.</returns>
        string GetPost(string themeName = null);

        /// <summary>
        /// Gets the footer path for view.
        /// </summary>
        /// <param name="themeName">Theme name.</param>
        /// <returns>Returns the post path for view.</returns>
        string GetFooter(string themeName = null);
    }
}