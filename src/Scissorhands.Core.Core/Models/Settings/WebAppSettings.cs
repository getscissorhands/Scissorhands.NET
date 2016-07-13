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
        /// Gets or sets the directory path where Markdown files are stored.
        /// </summary>
        public virtual string MarkdownPath { get; set; }

        /// <summary>
        /// Gets or sets the directory path where HTML posts are stored.
        /// </summary>
        public virtual string HtmlPath { get; set; }
    }
}