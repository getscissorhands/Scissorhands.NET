using System;
using System.IO;
using System.Threading.Tasks;

using Aliencube.Scissorhands.ViewModels.Post;

using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.Mvc.ViewEngines;
using Microsoft.AspNet.Mvc.ViewFeatures;

namespace Aliencube.Scissorhands.Services
{
    /// <summary>
    /// This represents the service entity for publish.
    /// </summary>
    public class PublishService : IPublishService
    {
        private const string ViewName = "Build";

        private bool _disposed;

        /// <summary>
        /// Gets the HTML to be published.
        /// </summary>
        /// <param name="resolver"><see cref="IServiceProvider"/> instance.</param>
        /// <param name="actionContext"><see cref="ActionContext"/> instance.</param>
        /// <param name="viewModel"><see cref="PostPublishViewModel"/> instance.</param>
        /// <param name="viewData"><see cref="ViewDataDictionary"/> instance.</param>
        /// <param name="tempData"><see cref="ITempDataDictionary"/> instance.</param>
        /// <returns>Returns HTML to be published.</returns>
        public async Task<string> GetPublishHtmlAsync(IServiceProvider resolver, ActionContext actionContext, PostPublishViewModel viewModel, ViewDataDictionary viewData, ITempDataDictionary tempData)
        {
            using (var writer = new StringWriter())
            {
                viewData.Model = viewModel;

                var viewEngine = resolver.GetService(typeof(ICompositeViewEngine)) as ICompositeViewEngine;
                if (viewEngine == null)
                {
                    return null;
                }

                var viewResult = viewEngine.FindPartialView(actionContext, ViewName);
                var viewContext = new ViewContext(actionContext, viewResult.View, viewData, tempData, writer, new HtmlHelperOptions());

                await viewResult.View.RenderAsync(viewContext).ConfigureAwait(false);

                var result = writer.GetStringBuilder().ToString();
                return result;
            }
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
    }
}