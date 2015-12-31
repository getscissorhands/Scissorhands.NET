using System;
using System.IO;
using System.Threading.Tasks;

using Aliencube.Scissorhands.Models.Settings;
using Aliencube.Scissorhands.Services.Exceptions;
using Aliencube.Scissorhands.Services.Helpers;
using Aliencube.Scissorhands.ViewModels.Post;

using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.Mvc.ViewEngines;
using Microsoft.AspNet.Mvc.ViewFeatures;
using Microsoft.Extensions.PlatformAbstractions;

namespace Aliencube.Scissorhands.Services
{
    /// <summary>
    /// This represents the service entity for publish.
    /// </summary>
    public class PublishService : IPublishService
    {
        private const string ViewName = "Post";

        private readonly WebAppSettings _settings;
        private readonly IFileHelper _fileHelper;

        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="PublishService"/> class.
        /// </summary>
        /// <param name="settings"><see cref="WebAppSettings"/> instance.</param>
        /// <param name="fileHelper"><see cref="IFileHelper"/> instance.</param>
        public PublishService(WebAppSettings settings, IFileHelper fileHelper)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            this._settings = settings;

            if (fileHelper == null)
            {
                throw new ArgumentNullException(nameof(fileHelper));
            }

            this._fileHelper = fileHelper;
        }

        /// <summary>
        /// Publishes the markdown as a file.
        /// </summary>
        /// <param name="markdown">Content in Markdown format.</param>
        /// <param name="env"><see cref="IApplicationEnvironment"/> instance.</param>
        /// <returns>Returns the Markdown file path in a virtual path format.</returns>
        public async Task<string> PublishMarkdownAsync(string markdown, IApplicationEnvironment env)
        {
            if (string.IsNullOrWhiteSpace(markdown))
            {
                throw new ArgumentNullException(nameof(markdown));
            }

            if (env == null)
            {
                throw new ArgumentNullException(nameof(env));
            }

            var filename = "markdown.md";
            var markdownpath = $"{this._settings.MarkdownPath}/{filename}";

            var filepath = this._fileHelper.GetDirectory(env, this._settings.MarkdownPath);
            filepath = Path.Combine(new[] { filepath, filename });

            var written = await this._fileHelper.WriteAsync(filepath, markdown).ConfigureAwait(false);
            if (!written)
            {
                throw new PublishFailedException("Markdown not published");
            }

            return markdownpath;
        }

        /// <summary>
        /// Publishes the HTML post as a file.
        /// </summary>
        /// <param name="html">Content in HTML format.</param>
        /// <param name="env"><see cref="IApplicationEnvironment"/> instance.</param>
        /// <returns>Returns the HTML file path.</returns>
        public async Task<string> PublishHtmlAsync(string html, IApplicationEnvironment env)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the post HTML to be published.
        /// </summary>
        /// <param name="resolver"><see cref="IServiceProvider"/> instance.</param>
        /// <param name="actionContext"><see cref="ActionContext"/> instance.</param>
        /// <param name="viewModel"><see cref="PostPublishViewModel"/> instance.</param>
        /// <param name="viewData"><see cref="ViewDataDictionary"/> instance.</param>
        /// <param name="tempData"><see cref="ITempDataDictionary"/> instance.</param>
        /// <returns>Returns HTML to be published.</returns>
        public async Task<string> GetPostHtmlAsync(IServiceProvider resolver, ActionContext actionContext, PostPublishViewModel viewModel, ViewDataDictionary viewData, ITempDataDictionary tempData)
        {
            if (resolver == null)
            {
                throw new ArgumentNullException(nameof(resolver));
            }

            if (actionContext == null)
            {
                throw new ArgumentNullException(nameof(actionContext));
            }

            if (viewModel == null)
            {
                throw new ArgumentNullException(nameof(viewModel));
            }

            if (viewData == null)
            {
                throw new ArgumentNullException(nameof(viewData));
            }

            if (tempData == null)
            {
                throw new ArgumentNullException(nameof(tempData));
            }

            using (var writer = new StringWriter())
            {
                viewData.Model = viewModel;

                var viewEngine = resolver.GetService(typeof(ICompositeViewEngine)) as ICompositeViewEngine;
                if (viewEngine == null)
                {
                    return null;
                }

                var viewResult = viewEngine.FindPartialView(actionContext, ViewName);
                if (viewResult == null)
                {
                    return null;
                }

                var viewContext = new ViewContext(actionContext, viewResult.View, viewData, tempData, writer, new HtmlHelperOptions());

                await viewResult.View.RenderAsync(viewContext).ConfigureAwait(false);

                var result = writer.GetStringBuilder().ToString();
                return result;
            }
        }

        /// <summary>
        /// Publishes the HTML post as a file.
        /// </summary>
        /// <param name="html">Content in HTML format.</param>
        /// <param name="env"><see cref="IApplicationEnvironment"/> instance.</param>
        /// <returns>Returns the HTML file path.</returns>
        public async Task<string> PublishPostAsync(string html, IApplicationEnvironment env)
        {
            if (string.IsNullOrWhiteSpace(html))
            {
                throw new ArgumentNullException(nameof(html));
            }

            if (env == null)
            {
                throw new ArgumentNullException(nameof(env));
            }

            var filename = "post.html";
            var htmlpath = $"{this._settings.HtmlPath}/{filename}";

            var filepath = this._fileHelper.GetDirectory(env, this._settings.HtmlPath);
            filepath = Path.Combine(new[] { filepath, filename });

            var written = await this._fileHelper.WriteAsync(filepath, html).ConfigureAwait(false);
            if (!written)
            {
                throw new PublishFailedException("Post not published");
            }

            return htmlpath;
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