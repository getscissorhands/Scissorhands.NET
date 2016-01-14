using System;

namespace Scissorhands.Helpers
{
    /// <summary>
    /// This provides interfaces to the Markdown helper class.
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
}