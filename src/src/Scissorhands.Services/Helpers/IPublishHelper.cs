using System;
using System.Threading.Tasks;

using Aliencube.Scissorhands.Services.Models;

namespace Aliencube.Scissorhands.Services.Helpers
{
    /// <summary>
    /// This provides interfaces to the <see cref="PublishHelper" /> class.
    /// </summary>
    public interface IPublishHelper : IDisposable
    {
        /// <summary>
        /// Reads the file
        /// </summary>
        /// <param name="filepath">
        /// The filepath.
        /// </param>
        /// <returns>
        /// Returns the contents.
        /// </returns>
        string Read(string filepath);

        /// <summary>
        /// Reads the file
        /// </summary>
        /// <param name="filepath">
        /// The filepath.
        /// </param>
        /// <returns>
        /// Returns the contents.
        /// </returns>
        Task<string> ReadAsync(string filepath);

        /// <summary>
        /// Creates the publish directory.
        /// </summary>
        /// <param name="publishDirectory">
        /// The publish directory.
        /// </param>
        void CreatePublishDirectory(string publishDirectory);

        /// <summary>
        /// Writes the content to the designated path.
        /// </summary>
        /// <param name="publishpath">
        /// The publish path.
        /// </param>
        /// <param name="content">
        /// The content.
        /// </param>
        void Write(string publishpath, string content);

        /// <summary>
        /// Writes the content to the designated path.
        /// </summary>
        /// <param name="publishpath">
        /// The publish path.
        /// </param>
        /// <param name="content">
        /// The content.
        /// </param>
        /// <returns>
        /// Returns the <see cref="Task" />.
        /// </returns>
        Task WriteAsync(string publishpath, string content);

        /// <summary>
        /// Sets the layout.
        /// </summary>
        /// <param name="layout">
        /// The master template.
        /// </param>
        void SetLayout(string layout);

        /// <summary>
        /// Compiles post model with template.
        /// </summary>
        /// <param name="template">
        /// Template string.
        /// </param>
        /// <param name="templateType">
        /// The <see cref="TemplateType" /> value.
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
        string Compile<T>(string template, TemplateType templateType, T model) where T : BasePageModel;

        /// <summary>
        /// Compiles post model with template.
        /// </summary>
        /// <param name="template">
        /// Template string.
        /// </param>
        /// <param name="templateType">
        /// The <see cref="TemplateType" /> value.
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
        Task<string> CompileAsync<T>(string template, TemplateType templateType, T model) where T : BasePageModel;
    }
}