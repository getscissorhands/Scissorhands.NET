using System;
using System.Threading.Tasks;

namespace Aliencube.Scissorhands.Services
{
    /// <summary>
    /// This provides interfaces to the <see cref="PublishService" /> class.
    /// </summary>
    public interface IPublishService : IDisposable
    {
        /// <summary>
        /// Publishes either a single post or entire blog posts.
        /// </summary>
        /// <param name="postpath">
        /// The post path written in Markdown.
        /// </param>
        void Publish(string postpath = null);

        /// <summary>
        /// Publishes either a single post or entire blog posts.
        /// </summary>
        /// <param name="postpath">
        /// The post path written in Markdown.
        /// </param>
        /// <returns>
        /// Returns the <see cref="Task" />.
        /// </returns>
        Task PublishAsync(string postpath = null);

        ///// <summary>
        ///// Processes the blog posts.
        ///// </summary>
        ///// <param name="postpath">
        ///// The full path of the post written in Markdown.
        ///// </param>
        ///// <param name="content">
        ///// Blog content converted in HTML.
        ///// </param>
        ///// <returns>
        ///// Returns <c>True</c>, if processed; otherwise returns <c>False</c>.
        ///// </returns>
        //bool Process(string postpath, string content);

        ///// <summary>
        ///// Processes the blog posts.
        ///// </summary>
        ///// <param name="postpath">
        ///// The full path of the post written in Markdown.
        ///// </param>
        ///// <param name="content">
        ///// Blog content converted in HTML.
        ///// </param>
        ///// <returns>
        ///// Returns <c>True</c>, if processed; otherwise returns <c>False</c>.
        ///// </returns>
        //Task<bool> ProcessAsync(string postpath, string content);
    }
}