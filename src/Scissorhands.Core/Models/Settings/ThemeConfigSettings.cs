using System.Collections.Generic;

namespace Scissorhands.Models.Settings
{
    /// <summary>
    /// This represents the settings entity for site content.
    /// </summary>
    public class ThemeConfigSettings
    {
        /// <summary>
        /// Gets or sets the title of the theme.
        /// </summary>
        public virtual string Title { get; set; }

        /// <summary>
        /// Gets or sets the description of the theme.
        /// </summary>
        public virtual string Description { get; set; }

        /// <summary>
        /// Gets or sets the list of authors of the theme.
        /// </summary>
        public virtual List<Author> Authors { get; set; }

        /// <summary>
        /// Gets or sets the URL where the theme is maintained.
        /// </summary>
        public virtual string ProjectUrl { get; set; }

        /// <summary>
        /// Gets or sets the URL that defines the license of the theme.
        /// </summary>
        public virtual string LicenseUrl { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="SocialMedia"/> object.
        /// </summary>
        public virtual SocialMedia SocialMedia { get; set; }
    }
}