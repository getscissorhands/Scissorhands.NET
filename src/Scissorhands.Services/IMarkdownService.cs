using System;
using System.Threading.Tasks;

namespace Aliencube.Scissorhands.Services
{
    /// <summary>
    /// This provides interfaces to the <see cref="MarkdownService"/> class.
    /// </summary>
    public interface IMarkdownService : IDisposable
    {
        /// <summary>
        /// Reads file and return the content as string.
        /// </summary>
        /// <param name="filepath">Fully qualified file path.</param>
        /// <returns>Returns the content as string.</returns>
        Task<string> ReadAsync(string filepath);

        /// <summary>
        /// Parses the markdown string to HTML string.
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