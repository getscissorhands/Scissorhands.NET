using System;
using System.Threading.Tasks;

using Microsoft.AspNet.Http;
using Microsoft.Extensions.PlatformAbstractions;

using Scissorhands.Models.Posts;

namespace Scissorhands.Services
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
        /// Gets the published HTML content.
        /// </summary>
        /// <param name="markdown">Content in Markdown format.</param>
        /// <param name="request"><see cref="HttpRequest"/> instance.</param>
        /// <returns>Returns the published HTML content.</returns>
        Task<string> GetPublishedHtmlAsync(string markdown, HttpRequest request);

        /// <summary>
        /// Publishes the post as a file.
        /// </summary>
        /// <param name="markdown">Content in Markdown format.</param>
        /// <param name="env"><see cref="IApplicationEnvironment"/> instance.</param>
        /// <param name="request"><see cref="HttpRequest"/> instance.</param>
        /// <returns>Returns the <see cref="PublishedPostPath"/> instance containing paths for published files.</returns>
        Task<PublishedPostPath> PublishPostAsync(string markdown, IApplicationEnvironment env, HttpRequest request);
    }
}