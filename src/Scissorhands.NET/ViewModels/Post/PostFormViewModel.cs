namespace Scissorhands.WebApp.ViewModels.Post
{
    /// <summary>
    /// This represents the view model entity for post form.
    /// </summary>
    public class PostFormViewModel
    {
        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the slug prefix.
        /// </summary>
        public string SlugPrefix { get; set; }

        /// <summary>
        /// Gets or sets the slug.
        /// </summary>
        public string Slug { get; set; }

        /// <summary>
        /// Gets or sets the author.
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// Gets or sets the body.
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// Gets or sets the comma delimited list of tags.
        /// </summary>
        public string Tags { get; set; }
    }
}