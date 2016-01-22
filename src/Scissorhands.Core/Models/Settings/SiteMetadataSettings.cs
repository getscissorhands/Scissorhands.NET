using System.Collections.Generic;

namespace Scissorhands.Models.Settings
{
    /// <summary>
    /// This represents the settings entity for site content.
    /// </summary>
    public class SiteMetadataSettings
    {
        /// <summary>
        /// Gets or sets the title of the website.
        /// </summary>
        public virtual string Title { get; set; }

        /// <summary>
        /// Gets or sets the description of the website.
        /// </summary>
        public virtual string Description { get; set; }

        /// <summary>
        /// Gets or sets the base URL of the website. eg) http://getscissorhands.net
        /// </summary>
        public virtual string BaseUrl { get; set; }

        /// <summary>
        /// Gets or sets the base path of the website. Trailing slash is optional. eg) /blog or /blog/
        /// </summary>
        public virtual string BasePath { get; set; }

        /// <summary>
        /// Gets or sets the date/time format.
        /// </summary>
        public virtual string DateTimeFormat { get; set; }

        /// <summary>
        /// Gets or sets the list of <see cref="Author"/> objects.
        /// </summary>
        public virtual List<Author> Authors { get; set; }

        /// <summary>
        /// Gets or sets the list of <see cref="FeedType"/> values.
        /// </summary>
        public virtual List<FeedType> FeedTypes { get; set; }

        /// <summary>
        /// Gets or sets the theme.
        /// </summary>
        public virtual string Theme { get; set; }

        /// <summary>
        /// Gets or sets the list of navigation links used in the header section.
        /// </summary>
        public virtual List<NavigationLink> HeaderNavigationLinks { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="SocialMedia"/> object.
        /// </summary>
        public virtual SocialMedia SocialMedia { get; set; }
    }
}