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
        /// Gets or sets the path of the Markdown file.
        /// </summary>
        public string MarkdownPath { get; set; }

        /// <summary>
        /// Gets or sets the path of the HTML post file.
        /// </summary>
        public string HtmlPath { get; set; }
    }
}