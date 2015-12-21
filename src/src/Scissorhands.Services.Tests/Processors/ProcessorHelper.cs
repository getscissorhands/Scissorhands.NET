using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Aliencube.Scissorhands.Services.Processors;

using MarkdownDeep;

namespace Aliencube.Scissorhands.Services.Tests.Processors
{
    /// <summary>
    /// This represents the helper entity for classes inheriting the <see cref="BaseProcessor" /> class.
    /// </summary>
    public static class ProcessorHelper
    {
        /// <summary>
        /// Reads the file.
        /// </summary>
        /// <param name="filepath">
        /// The filepath.
        /// </param>
        /// <returns>
        /// Returns the string contents from the file.
        /// </returns>
        public static string ReadFile(string filepath)
        {
            using (var stream = new FileStream(filepath, FileMode.Open, FileAccess.Read))
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                var contents = reader.ReadToEnd();
                return contents;
            }
        }

        /// <summary>
        /// Reads the file.
        /// </summary>
        /// <param name="filepath">
        /// The filepath.
        /// </param>
        /// <returns>
        /// Returns the string contents from the file.
        /// </returns>
        public static async Task<string> ReadFileAsync(string filepath)
        {
            using (var stream = new FileStream(filepath, FileMode.Open, FileAccess.Read))
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                var contents = await reader.ReadToEndAsync();
                return contents;
            }
        }

        /// <summary>
        /// Converts the Markdown to HTML.
        /// </summary>
        /// <param name="markdown">
        /// The Markdown string.
        /// </param>
        /// <returns>
        /// Returns the HTML converted string.
        /// </returns>
        public static string ConvertMarkdownToHtml(string markdown)
        {
            var md = new Markdown() { ExtraMode = true, SafeMode = false };
            var converted = md.Transform(markdown);
            return converted;
        }
    }
}
