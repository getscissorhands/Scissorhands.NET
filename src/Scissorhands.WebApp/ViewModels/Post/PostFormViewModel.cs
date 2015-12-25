using System.ComponentModel.DataAnnotations;

namespace Aliencube.Scissorhands.WebApp.ViewModels.Post
{
    /// <summary>
    /// This represents the view model entity for /post/write.
    /// </summary>
    public class PostFormViewModel
    {
        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        [Required]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the slug.
        /// </summary>
        [Required]
        public string Slug { get; set; }

        /// <summary>
        /// Gets or sets the body.
        /// </summary>
        [Required]
        public string Body { get; set; }
    }
}