using System;
using System.IO;
using System.Threading.Tasks;

using Aliencube.Scissorhands.Models;
using Aliencube.Scissorhands.Services.Exceptions;
using Aliencube.Scissorhands.Services.Extensions;
using Aliencube.Scissorhands.Services.Helpers;
using Aliencube.Scissorhands.ViewModels.Post;

using Microsoft.AspNet.Hosting;
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

        private readonly IHostingEnvironment _env;
        private readonly WebAppSettings _settings;
        private readonly IFileHelper _fileHelper;

        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="PublishService"/> class.
        /// </summary>
        /// <param name="env"><see cref="IHostingEnvironment"/> instance.</param>
        /// <param name="settings"><see cref="WebAppSettings"/> instance.</param>
        /// <param name="fileHelper"><see cref="IFileHelper"/> instance.</param>
        public PublishService(IHostingEnvironment env, WebAppSettings settings, IFileHelper fileHelper)
        {
            if (env == null)
            {
                throw new ArgumentNullException(nameof(env));
            }

            this._env = env;

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
        /// <returns>Returns the Markdown file path in a virtual path format.</returns>
        public async Task<string> PublishMarkdownAsync(string markdown, IServiceProvider provider)
        {
            if (string.IsNullOrWhiteSpace(markdown))
            {
                throw new ArgumentNullException(nameof(markdown));
            }

            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            var env = provider.GetService(typeof(IApplicationEnvironment)) as IApplicationEnvironment;
            if (env == null)
            {
                throw new InvalidOperationException("ApplicationEnvironment not set");
            }

            this.SetPostDirectory(this._settings.MarkdownPath, env);

            var markdownpath = $"{this._settings.MarkdownPath}/markdown.md";
            var filepath = Path.Combine(new[] { env.ApplicationBasePath, "wwwroot", TrimDirectoryPath(markdownpath) });

            var written = await this._fileHelper.WriteAsync(filepath, markdown).ConfigureAwait(false);
            if (!written)
            {
                throw new PublishFailedException("Markdown not published");
            }

            return markdownpath;
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
        /// <returns>Returns the HTML file path.</returns>
        public async Task<string> PublishPostAsync(string html, IServiceProvider provider)
        {
            if (string.IsNullOrWhiteSpace(html))
            {
                throw new ArgumentNullException(nameof(html));
            }

            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            var env = provider.GetService(typeof(IApplicationEnvironment)) as IApplicationEnvironment;
            if (env == null)
            {
                throw new InvalidOperationException("ApplicationEnvironment not set");
            }

            this.SetPostDirectory(this._settings.HtmlPath, env);

            var htmlpath = $"{this._settings.HtmlPath}/post.html";
            var filepath = Path.Combine(new[] { env.ApplicationBasePath, "wwwroot", TrimDirectoryPath(htmlpath) });

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

        private static string TrimDirectoryPath(string directorypath)
        {
            var path = directorypath.Replace('/', Path.DirectorySeparatorChar);
            if (path.StartsWith(Path.DirectorySeparatorChar))
            {
                path = path.Substring(1);
            }

            return path;
        }

        private void SetPostDirectory(string directorypath, IApplicationEnvironment env)
        {
            if (string.IsNullOrWhiteSpace(directorypath))
            {
                throw new ArgumentNullException(nameof(directorypath));
            }

            if (env == null)
            {
                throw new ArgumentNullException(nameof(env));
            }

            var trimmed = TrimDirectoryPath(directorypath);

            var combined =
                Path.Combine(
                    new[]
                        {
                            env.ApplicationBasePath,
                            "wwwroot",
                            trimmed,
                        });

            if (!Directory.Exists(combined))
            {
                Directory.CreateDirectory(combined);
            }
        }
    }
}