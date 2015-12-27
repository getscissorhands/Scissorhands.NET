using System;
using System.Threading.Tasks;

namespace Aliencube.Scissorhands.Services
{
    /// <summary>
    /// This provides interfaces to the <see cref="BuildService"/> class.
    /// </summary>
    public interface IBuildService : IDisposable
    {
        /// <summary>
        /// Builds the entire blog.
        /// </summary>
        /// <returns>Returns the <see cref="Task"/>.</returns>
        Task BuildAsync();

        /// <summary>
        /// Builds all index pages including pagination.
        /// </summary>
        /// <returns>Returns the <see cref="Task"/>.</returns>
        Task<string> BuildIndiciesAsync();

        /// <summary>
        /// Builds all posts.
        /// </summary>
        /// <returns>Returns the <see cref="Task"/>.</returns>
        Task<string> BuildPostsAsync();

        /// <summary>
        /// Publishes the markdown as a file.
        /// </summary>
        /// <param name="markdown">Content in Markdown format.</param>
        /// <returns>Returns the <see cref="Task"/>.</returns>
        Task<string> PublishMarkdownAsync(string markdown);

        /// <summary>
        /// Publishes the HTML post as a file.
        /// </summary>
        /// <param name="html">Content in HTML format.</param>
        /// <returns>Returns the <see cref="Task"/>.</returns>
        Task<string> PublishPostAsync(string html);
    }
}