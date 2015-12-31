namespace Aliencube.Scissorhands.Models.Posts
{
    /// <summary>
    /// This represents the entity that contains file paths for post - Markdown and HTML.
    /// </summary>
    public class PublishedPostPath
    {
        /// <summary>
        /// Gets or sets the file path of Markdown.
        /// </summary>
        public string Markdown { get; set; }

        /// <summary>
        /// Gets or sets the file path of HTML.
        /// </summary>
        public string Html { get; set; }
    }
}