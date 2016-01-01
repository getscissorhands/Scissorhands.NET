namespace Scissorhands.WebApp.ViewModels.Post
{
    /// <summary>
    /// This represents the view model entity for post preview.
    /// </summary>
    public class PostViewViewModel
    {
        /// <summary>
        /// Gets or sets the theme.
        /// </summary>
        public string Theme { get; set; }

        /// <summary>
        /// Gets or sets the markdown.
        /// </summary>
        public string Markdown { get; set; }

        /// <summary>
        /// Gets or sets the HTML converted from markdown.
        /// </summary>
        public string Html { get; set; }
    }
}