using System;
using System.Threading.Tasks;

using Aliencube.Scissorhands.ViewModels.Post;

using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.ViewFeatures;
using Microsoft.Extensions.PlatformAbstractions;

namespace Aliencube.Scissorhands.Services
{
    /// <summary>
    /// This provides interfaces to the <see cref="PublishService"/> class.
    /// </summary>
    public interface IPublishService : IDisposable
    {
        /// <summary>
        /// Publishes the markdown as a file.
        /// </summary>
        /// <param name="markdown">Content in Markdown format.</param>
        /// <param name="env"><see cref="IApplicationEnvironment"/> instance.</param>
        /// <returns>Returns the Markdown file path in a virtual path format.</returns>
        Task<string> PublishMarkdownAsync(string markdown, IApplicationEnvironment env);

        /// <summary>
        /// Publishes the HTML post as a file.
        /// </summary>
        /// <param name="html">Content in HTML format.</param>
        /// <param name="env"><see cref="IApplicationEnvironment"/> instance.</param>
        /// <returns>Returns the HTML file path.</returns>
        Task<string> PublishHtmlAsync(string html, IApplicationEnvironment env);

        /// <summary>
        /// Publishes the post as a file.
        /// </summary>
        /// <param name="markdown">Content in Markdown format.</param>
        /// <param name="env"><see cref="IApplicationEnvironment"/> instance.</param>
        /// <returns>Returns the HTML file path.</returns>
        Task<string> PublishPostAsync(string markdown, IApplicationEnvironment env);

        /// <summary>
        /// Gets the post HTML to be published.
        /// </summary>
        /// <param name="resolver"><see cref="IServiceProvider"/> instance.</param>
        /// <param name="actionContext"><see cref="ActionContext"/> instance.</param>
        /// <param name="viewModel"><see cref="PostPublishViewModel"/> instance.</param>
        /// <param name="viewData"><see cref="ViewDataDictionary"/> instance.</param>
        /// <param name="tempData"><see cref="ITempDataDictionary"/> instance.</param>
        /// <returns>Returns HTML to be published.</returns>
        Task<string> GetPostHtmlAsync(IServiceProvider resolver, ActionContext actionContext, PostPublishViewModel viewModel, ViewDataDictionary viewData, ITempDataDictionary tempData);
    }
}