using System;

using Scissorhands.Models.Settings;

namespace Scissorhands.Helpers
{
    /// <summary>
    /// This represents the helper entity for themes.
    /// </summary>
    public class ThemeHelper : IThemeHelper
    {
        private readonly ISiteMetadataSettings _metadata;

        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="ThemeHelper"/> class.
        /// </summary>
        /// <param name="metadata"><see cref="ISiteMetadataSettings"/> instance.</param>
        public ThemeHelper(ISiteMetadataSettings metadata)
        {
            if (metadata == null)
            {
                throw new ArgumentNullException(nameof(metadata));
            }

            this._metadata = metadata;
        }

        /// <summary>
        /// Gets the path for head partial view.
        /// </summary>
        /// <param name="themeName">Theme name.</param>
        /// <returns>Returns the path for head partial view.</returns>
        public string GetHeadPartialViewPath(string themeName = null)
        {
            var theme = themeName;
            if (string.IsNullOrWhiteSpace(theme))
            {
                theme = this._metadata.Theme;
            }

            var head = $"~/Themes/{theme}/shared/_head.cshtml";
            return head;
        }

        /// <summary>
        /// Gets the path for header partial view.
        /// </summary>
        /// <param name="themeName">Theme name.</param>
        /// <returns>Returns the path for header partial view.</returns>
        public string GetHeaderPartialViewPath(string themeName = null)
        {
            var theme = themeName;
            if (string.IsNullOrWhiteSpace(theme))
            {
                theme = this._metadata.Theme;
            }

            var header = $"~/Themes/{theme}/shared/_header.cshtml";
            return header;
        }

        /// <summary>
        /// Gets the path for post partial view.
        /// </summary>
        /// <param name="themeName">Theme name.</param>
        /// <returns>Returns the path for post partial view.</returns>
        public string GetPostPartialViewPath(string themeName = null)
        {
            var theme = themeName;
            if (string.IsNullOrWhiteSpace(theme))
            {
                theme = this._metadata.Theme;
            }

            var post = $"~/Themes/{theme}/post/post.cshtml";
            return post;
        }

        /// <summary>
        /// Gets the path for footer partial view.
        /// </summary>
        /// <param name="themeName">Theme name.</param>
        /// <returns>Returns the path for footer partial view.</returns>
        public string GetFooterPartialViewPath(string themeName = null)
        {
            var theme = themeName;
            if (string.IsNullOrWhiteSpace(theme))
            {
                theme = this._metadata.Theme;
            }

            var footer = $"~/Themes/{theme}/shared/_footer.cshtml";
            return footer;
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