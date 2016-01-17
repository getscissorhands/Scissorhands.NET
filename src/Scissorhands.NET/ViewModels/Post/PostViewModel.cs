using Scissorhands.Models.Settings;

namespace Scissorhands.WebApp.ViewModels.Post
{
    /// <summary>
    /// This represents the view model entity for post. This MUST be inherited.
    /// </summary>
    public abstract class PostViewModel
    {
        /// <summary>
        /// Gets or sets the theme.
        /// </summary>
        public string Theme { get; set; }

        /// <summary>
        /// Gets or sets the file path of the head partial view.
        /// </summary>
        public string HeadPartialViewPath { get; set; }

        /// <summary>
        /// Gets or sets the file path of the header partial view.
        /// </summary>
        public string HeaderPartialViewPath { get; set; }

        /// <summary>
        /// Gets or sets the file path of the header partial view.
        /// </summary>
        public string PostPartialViewPath { get; set; }

        /// <summary>
        /// Gets or sets the file path of the footer partial view.
        /// </summary>
        public string FooterPartialViewPath { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="PageMetadataSettings"/> object.
        /// </summary>
        public PageMetadataSettings Page { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="SiteMetadataSettings"/> object.
        /// </summary>
        public SiteMetadataSettings Site { get; set; }
    }
}