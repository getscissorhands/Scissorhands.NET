namespace Scissorhands.ViewModels.Post
{
    /// <summary>
    /// This represents the view model entity for post parse.
    /// </summary>
    public class PostParseViewModel : PostViewModel
    {
        /// <summary>
        /// Gets or sets the HTML converted from markdown.
        /// </summary>
        public string Html { get; set; }
    }
}