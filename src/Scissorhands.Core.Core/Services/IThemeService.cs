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
    }
}