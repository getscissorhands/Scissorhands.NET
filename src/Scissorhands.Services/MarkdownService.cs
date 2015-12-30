using System;
using System.Threading.Tasks;

using Aliencube.Scissorhands.Services.Helpers;

using CommonMark;

namespace Aliencube.Scissorhands.Services
{
    /// <summary>
    /// This represents the service entity for markdown.
    /// </summary>
    public class MarkdownService : IMarkdownService
    {
        private readonly IFileHelper _fileHelper;

        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="MarkdownService"/> class.
        /// </summary>
        /// <param name="fileHelper"><see cref="IFileHelper"/> instance.</param>
        public MarkdownService(IFileHelper fileHelper)
        {
            if (fileHelper == null)
            {
                throw new ArgumentNullException(nameof(fileHelper));
            }

            this._fileHelper = fileHelper;
        }

        /// <summary>
        /// Parses the markdown string to HTML string.
        /// </summary>
        /// <param name="markdown">Markdown string.</param>
        /// <returns>Returns HTML string parsed.</returns>
        public string Parse(string markdown)
        {
            if (string.IsNullOrWhiteSpace(markdown))
            {
                return null;
            }

            var parsed = CommonMarkConverter.Convert(markdown);
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