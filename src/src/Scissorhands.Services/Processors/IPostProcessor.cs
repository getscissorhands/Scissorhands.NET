using System.Collections.Generic;
using System.Threading.Tasks;

using Aliencube.Scissorhands.Services.Configs;
using Aliencube.Scissorhands.Services.Models;

namespace Aliencube.Scissorhands.Services.Processors
{
    /// <summary>
    /// This provides interfaces to the <see cref="PostProcessor" /> class.
    /// </summary>
    public interface IPostProcessor : IProcessor
    {
        /// <summary>
        /// Gets the theme corresponding to the theme name.
        /// </summary>
        /// <param name="themeName">
        /// The theme name.
        /// </param>
        /// <returns>
        /// Returns the <see cref="Theme" />.
        /// </returns>
        Theme GetTheme(string themeName);

            /// <summary>
        /// Gets the razor template.
        /// </summary>
        /// <param name="themeName">
        /// The theme name.
        /// </param>
        /// <param name="templateType">
        /// The <see cref="TemplateType" /> value.
        /// </param>
        /// <returns>
        /// Returns the razor template.
        /// </returns>
        string GetTemplate(string themeName, TemplateType templateType);

        /// <summary>
        /// Gets the razor template.
        /// </summary>
        /// <param name="themeName">
        /// The theme name.
        /// </param>
        /// <param name="templateType">
        /// The <see cref="TemplateType" /> value.
        /// </param>
        /// <returns>
        /// Returns the razor template.
        /// </returns>
        Task<string> GetTemplateAsync(string themeName, TemplateType templateType);

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
        /// Gets the list of post paths.
        /// </summary>
        /// <param name="postpath">
        /// The postpath for a single post.
        /// </param>
        /// <returns>
        /// Returns the list of post paths.
        /// </returns>
        IEnumerable<string> GetPostPaths(string postpath = null);

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
    }
}