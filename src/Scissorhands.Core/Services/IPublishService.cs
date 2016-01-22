using System;
using System.Threading.Tasks;

using Microsoft.AspNet.Http;

using Scissorhands.Models.Posts;
using Scissorhands.ViewModels.Post;

namespace Scissorhands.Services
{
    /// <summary>
    /// This provides interfaces to the publish service class.
    /// </summary>
    public interface IPublishService : IDisposable
    {
        /// <summary>
        /// Applies metadata to the markdown body.
        /// </summary>
        /// <param name="model"><see cref="PostFormViewModel"/> instance.</param>
        /// <returns>Returns the markdown body with metadata applied.</returns>
        string ApplyMetadata(PostFormViewModel model);

        /// <summary>
        /// Publishes the markdown as a file.
        /// </summary>
        /// <param name="markdown">Content in Markdown format.</param>
        /// <returns>Returns the Markdown file path in a virtual path format.</returns>
        Task<string> PublishMarkdownAsync(string markdown);

        /// <summary>
        /// Publishes the HTML post as a file.
        /// </summary>
        /// <param name="html">Content in HTML format.</param>
        /// <returns>Returns the HTML file path.</returns>
        Task<string> PublishHtmlAsync(string html);

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
        /// <param name="model"><see cref="PostFormViewModel"/> instance.</param>
        /// <param name="request"><see cref="HttpRequest"/> instance.</param>
        /// <returns>Returns the <see cref="PublishedPostPath"/> instance containing paths for published files.</returns>
        Task<PublishedPostPath> PublishPostAsync(PostFormViewModel model, HttpRequest request);
    }
}