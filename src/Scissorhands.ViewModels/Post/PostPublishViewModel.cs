namespace Aliencube.Scissorhands.ViewModels.Post
{
    /// <summary>
    /// This represents the view model entity for post publish.
    /// </summary>
    public class PostPublishViewModel
    {
        /// <summary>
        /// Gets or sets the theme.
        /// </summary>
        public string Theme { get; set; }

        /// <summary>
        /// Gets or sets the HTML converted from markdown.
        /// </summary>
        public string Html { get; set; }

        /// <summary>
        /// Gets or sets the path of the markdown file.
        /// </summary>
        public string Markdownpath { get; set; }

        /// <summary>
        /// Gets or sets the path of the HTML post file.
        /// </summary>
        public string Postpath { get; set; }
    }
}