using System;
using System.Collections.Generic;

namespace Scissorhands.Models.Settings
{
    /// <summary>
    /// This represents the metadata entity used for post/page.
    /// </summary>
    public class PageMetadataSettings
    {
        /// <summary>
        /// Gets or sets the title of the post/page.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the excerpt of the post/page.
        /// </summary>
        public string Excerpt { get; set; }

        /// <summary>
        /// Gets or sets the author of the post/page.
        /// </summary>
        public Author Author { get; set; }

        /// <summary>
        /// Gets or sets the date of the post/page when it was written.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Gets or sets the base URL of the post/page.
        /// </summary>
        public string BaseUrl { get; set; }

        /// <summary>
        /// Gets or sets the URL of the post/page.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the list of <see cref="NavigationLink"/> objects.
        /// </summary>
        public List<NavigationLink> HeaderNavigationLinks { get; set; }
    }
}