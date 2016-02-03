using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Rendering;

using Scissorhands.Models.Settings;

namespace Scissorhands.Services
{
    /// <summary>
    /// This represents the service entity for themes.
    /// </summary>
    public class ThemeService : IThemeService
    {
        private readonly ISiteMetadataSettings _metadata;
        private readonly IDictionary<string, List<string>> _controllers;

        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="ThemeService"/> class.
        /// </summary>
        /// <param name="metadata"><see cref="ISiteMetadataSettings"/> instance.</param>
        public ThemeService(ISiteMetadataSettings metadata)
        {
            if (metadata == null)
            {
                throw new ArgumentNullException(nameof(metadata));
            }

            this._metadata = metadata;

            this._controllers = new Dictionary<string, List<string>>
                                    {
                                        { "Post", new List<string>() { "Preview", "PublishHtml" } },
                                    };
        }

        /// <summary>
        /// Gets the layout path for view.
        /// </summary>
        /// <param name="viewContext"><see cref="ViewContext"/> instance.</param>
        /// <returns>Returns the layout path for view.</returns>
        public string GetLayout(ViewContext viewContext)
        {
            var themepath = this.GetThemePath(viewContext);
            var layout = $"{themepath}/Shared/_Layout.cshtml";

            return themepath.StartsWith("~/Themes") ? layout.ToLowerInvariant() : layout;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (this._disposed)
            {
                return;
            }

            this._disposed = true;
        }

        private string GetThemePath(ActionContext context)
        {
            var controller = context.RouteData.Values["controller"].ToString();
            var action = context.RouteData.Values["action"].ToString();

            var themepath = "~/Views";
            if (this.IsThemeRequired(controller, action))
            {
                themepath = $"~/Themes/{this._metadata.Theme}";
            }

            return themepath;
        }

        private bool IsThemeRequired(string controllerName, string actionName = null)
        {
            if (string.IsNullOrWhiteSpace(controllerName))
            {
                return false;
            }

            var controllerExists = this._controllers
                                       .Select(p => p.Key)
                                       .ToList()
                                       .Exists(p => p.Equals(controllerName, StringComparison.CurrentCultureIgnoreCase));
            if (!controllerExists)
            {
                return false;
            }

            // No action means all actions on the controller are theme required.
            if (string.IsNullOrWhiteSpace(actionName))
            {
                return true;
            }

            var actionExists = this._controllers
                                   .Single(p => p.Key.Equals(controllerName, StringComparison.CurrentCultureIgnoreCase))
                                   .Value
                                   .Exists(p => p.Equals(actionName, StringComparison.CurrentCultureIgnoreCase));
            return actionExists;
        }
    }
}