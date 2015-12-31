namespace Aliencube.Scissorhands.Models.Posts
{
    /// <summary>
    /// This represents the entity of content to publish.
    /// </summary>
    public class PublishedContent
    {
        /// <summary>
        /// Gets or sets the Markdown content.
        /// </summary>
        public string Markdown { get; set; }

        /// <summary>
        /// Gets or sets the HTML content.
        /// </summary>
        public string Html { get; set; }
    }
}