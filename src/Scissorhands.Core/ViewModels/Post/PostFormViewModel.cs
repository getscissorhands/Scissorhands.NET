namespace Scissorhands.ViewModels.Post
{
    /// <summary>
    /// This represents the view model entity for post form.
    /// </summary>
    public class PostFormViewModel
    {
        /// <summary>
        /// Gets or sets the title of the post/page.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the slug prefix of the post/page.
        /// </summary>
        public string SlugPrefix { get; set; }

        /// <summary>
        /// Gets or sets the slug of the post/page.
        /// </summary>
        public string Slug { get; set; }

        /// <summary>
        /// Gets or sets the author of the post/page.
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// Gets or sets the excerpt of the post/page.
        /// </summary>
        public string Excerpt { get; set; }

        /// <summary>
        /// Gets or sets the body of the post/page.
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// Gets or sets the comma delimited list of tags of the post/page.
        /// </summary>
        public string Tags { get; set; }
    }
}