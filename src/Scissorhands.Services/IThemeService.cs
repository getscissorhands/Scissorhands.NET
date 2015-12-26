using System;

using Microsoft.AspNet.Mvc.Rendering;

namespace Aliencube.Scissorhands.Services
{
    /// <summary>
    /// This provides interfaces to the <see cref="ThemeService"/> class.
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
        /// Gets the post path for view.
        /// </summary>
        /// <param name="themeName">Theme name.</param>
        /// <returns>Returns the post path for view.</returns>
        string GetPost(string themeName = null);
    }
}