namespace Scissorhands.ViewModels.Post
{
    /// <summary>
    /// This represents the view model entity for post publish.
    /// </summary>
    public class PostPublishViewModel : PostViewModel
    {
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