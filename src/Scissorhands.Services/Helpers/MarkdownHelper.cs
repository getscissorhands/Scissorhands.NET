using System;

using CommonMark;

namespace Aliencube.Scissorhands.Services.Helpers
{
    /// <summary>
    /// This provides interfaces to the <see cref="MarkdownHelper"/> class.
    /// </summary>
    public interface IMarkdownHelper : IDisposable
    {
        /// <summary>
        /// Parses the Markdown string to HTML string.
        /// </summary>
        /// <param name="markdown">Markdown string.</param>
        /// <returns>Returns HTML string parsed.</returns>
        string Parse(string markdown);
    }

    /// <summary>
    /// This represents the helper entity for Markdown.
    /// </summary>
    public class MarkdownHelper : IMarkdownHelper
    {
        private bool _disposed;

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

            var parsed = CommonMarkConverter.Convert(markdown);
            return parsed;
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
