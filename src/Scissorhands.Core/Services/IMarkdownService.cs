using System;
using System.Threading.Tasks;

namespace Scissorhands.Services
{
    /// <summary>
    /// This provides interfaces to the markdown service class.
    /// </summary>
    public interface IMarkdownService : IDisposable
    {
        /// <summary>
        /// Parses the Markdown string to HTML string.
        /// </summary>
        /// <param name="markdown">Markdown string.</param>
        /// <returns>Returns HTML string parsed.</returns>
        string Parse(string markdown);

        /// <summary>
        /// Converts the markdown file to HTML string.
        /// </summary>
        /// <param name="filepath">Fully qualified file path.</param>
        /// <returns>Returns HTML string converted from the markdown file.</returns>
        Task<string> ConvertAsync(string filepath);
    }
}