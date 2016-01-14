namespace Scissorhands.WebApp.ViewModels.Post
{
    /// <summary>
    /// This represents the view model entity for post preview.
    /// </summary>
    public class PostPreviewViewModel : PostViewModel
    {
        /// <summary>
        /// Gets or sets the HTML converted from markdown.
        /// </summary>
        public string Html { get; set; }
    }
}