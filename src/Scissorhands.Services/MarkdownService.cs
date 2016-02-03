using System;
using System.Threading.Tasks;

using Scissorhands.Helpers;

namespace Scissorhands.Services
{
    /// <summary>
    /// This represents the service entity for markdown.
    /// </summary>
    public class MarkdownService : IMarkdownService
    {
        private readonly IMarkdownHelper _markdownHelper;
        private readonly IFileHelper _fileHelper;

        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="MarkdownService"/> class.
        /// </summary>
        /// <param name="markdownHelper"><see cref="IMarkdownHelper"/> instance.</param>
        /// <param name="fileHelper"><see cref="IFileHelper"/> instance.</param>
        public MarkdownService(IMarkdownHelper markdownHelper, IFileHelper fileHelper)
        {
            if (markdownHelper == null)
            {
                throw new ArgumentNullException(nameof(markdownHelper));
            }

            this._markdownHelper = markdownHelper;

            if (fileHelper == null)
            {
                throw new ArgumentNullException(nameof(fileHelper));
            }

            this._fileHelper = fileHelper;
        }

        /// <summary>
        /// Parses the Markdown string to HTML string.
        /// </summary>
        /// <param name="markdown">Markdown string.</param>
        /// <returns>Returns HTML string parsed.</returns>
        public string Parse(string markdown)
        {
            if (string.IsNullOrWhiteSpace(markdown))
            {
                return null;
            }

            var parsed = this._markdownHelper.Parse(markdown);
            return parsed;
        }

        /// <summary>
        /// Converts the markdown file to HTML string.
        /// </summary>
        /// <param name="filepath">Fully qualified file path.</param>
        /// <returns>Returns HTML string converted from the markdown file.</returns>
        public async Task<string> ConvertAsync(string filepath)
        {
            if (string.IsNullOrWhiteSpace(filepath))
            {
                return null;
            }

            var markdown = await this._fileHelper.ReadAsync(filepath).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(markdown))
            {
                return null;
            }

            var parsed = this.Parse(markdown);
            return await Task.FromResult(parsed).ConfigureAwait(false);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (this._disposed)
            {
                return;
            }

            this._disposed = true;
        }
    }
}