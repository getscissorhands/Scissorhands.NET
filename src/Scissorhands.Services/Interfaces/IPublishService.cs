using System;
using System.Threading.Tasks;

using Aliencube.Scissorhands.Services.Models;

namespace Aliencube.Scissorhands.Services.Interfaces
{
    /// <summary>
    /// This provides interfaces to the <see cref="PublishService" /> class.
    /// </summary>
    public interface IPublishService : IDisposable
    {
        /// <summary>
        /// Processes posts.
        /// </summary>
        /// <param name="postpath">
        /// The filename of the post to process.
        /// </param>
        /// <returns>
        /// Returns <c>True</c>, if processed; otherwise returns <c>False</c>.
        /// </returns>
        bool Process(string postpath = null);

        /// <summary>
        /// Processes posts.
        /// </summary>
        /// <param name="postpath">
        /// The filename of the post to process.
        /// </param>
        /// <returns>
        /// Returns <c>True</c>, if processed; otherwise returns <c>False</c>.
        /// </returns>
        Task<bool> ProcessAsync(string postpath = null);

        /// <summary>
        /// Gets the razor template.
        /// </summary>
        /// <param name="themeName">
        /// The theme name.
        /// </param>
        /// <returns>
        /// Returns the razor template.
        /// </returns>
        string GetTemplate(string themeName);

        /// <summary>
        /// Gets the razor template.
        /// </summary>
        /// <param name="themeName">
        /// The theme name.
        /// </param>
        /// <returns>
        /// Returns the razor template.
        /// </returns>
        Task<string> GetTemplateAsync(string themeName);

        /// <summary>
        /// Gets the post from the given post file written in Markdown.
        /// </summary>
        /// <param name="postpath">
        /// The full path of the post.
        /// </param>
        /// <returns>
        /// Returns the HTML converted post.
        /// </returns>
        string GetPost(string postpath);

        /// <summary>
        /// Gets the post from the given post file written in Markdown.
        /// </summary>
        /// <param name="postpath">
        /// The full path of the post.
        /// </param>
        /// <returns>
        /// Returns the HTML converted post.
        /// </returns>
        Task<string> GetPostAsync(string postpath);

        /// <summary>
        /// Gets the page model for razor template.
        /// </summary>
        /// <param name="post">
        /// Post data.
        /// </param>
        /// <typeparam name="T">
        /// Type inheriting <see cref="BasePageModel" />.
        /// </typeparam>
        /// <returns>
        /// Returns the page model for razor template.
        /// </returns>
        T GetModel<T>(string post) where T : BasePageModel;

        /// <summary>
        /// Compiles post model with template.
        /// </summary>
        /// <param name="template">
        /// Template string.
        /// </param>
        /// <param name="model">
        /// The post model.
        /// </param>
        /// <typeparam name="T">
        /// Type inheriting the <see cref="BasePageModel" /> class.
        /// </typeparam>
        /// <returns>
        /// Returns the compiled string.
        /// </returns>
        string Compile<T>(string template, T model) where T : BasePageModel;

        /// <summary>
        /// Compiles post model with template.
        /// </summary>
        /// <param name="template">
        /// Template string.
        /// </param>
        /// <param name="model">
        /// The post model.
        /// </param>
        /// <typeparam name="T">
        /// Type inheriting the <see cref="BasePageModel" /> class.
        /// </typeparam>
        /// <returns>
        /// Returns the compiled string.
        /// </returns>
        Task<string> CompileAsync<T>(string template, T model) where T : BasePageModel;

        /// <summary>
        /// Publishes the blog posts.
        /// </summary>
        /// <param name="postpath">
        /// The full path of the post.
        /// </param>
        /// <param name="compiled">
        /// Template merged post data.
        /// </param>
        /// <returns>
        /// Returns <c>True</c>, if published; otherwise returns <c>False</c>.
        /// </returns>
        bool Publish(string postpath, string compiled);

        /// <summary>
        /// Publishes the blog posts.
        /// </summary>
        /// <param name="postpath">
        /// The full path of the post.
        /// </param>
        /// <param name="compiled">
        /// Template merged post data.
        /// </param>
        /// <returns>
        /// Returns <c>True</c>, if published; otherwise returns <c>False</c>.
        /// </returns>
        Task<bool> PublishAsync(string postpath, string compiled);
    }
}