using System.Collections.Generic;

namespace Scissorhands.Models.Settings
{
    /// <summary>
    /// This represents the entity for web app settings.
    /// </summary>
    public class WebAppSettings
    {
        /// <summary>
        /// Gets or sets the server.
        /// </summary>
        public virtual ServerType Server { get; set; }

        /// <summary>
        /// Gets or sets the base URL of the website. eg) http://getscissorhands.net
        /// </summary>
        public string BaseUrl { get; set; }

        /// <summary>
        /// Gets or sets the base path of the website. Trailing slash is optional. eg) /blog or /blog/
        /// </summary>
        public string BasePath { get; set; }

        /// <summary>
        /// Gets or sets the list of <see cref="Author"/> objects.
        /// </summary>
        public List<Author> Authors { get; set; }

        /// <summary>
        /// Gets or sets the list of <see cref="FeedType"/> values.
        /// </summary>
        public List<FeedType> FeedTypes { get; set; }

        /// <summary>
        /// Gets or sets the theme.
        /// </summary>
        public virtual string Theme { get; set; }

        /// <summary>
        /// Gets or sets the directory path where Markdown files are stored.
        /// </summary>
        public virtual string MarkdownPath { get; set; }

        /// <summary>
        /// Gets or sets the directory path where HTML posts are stored.
        /// </summary>
        public virtual string HtmlPath { get; set; }
    }
}