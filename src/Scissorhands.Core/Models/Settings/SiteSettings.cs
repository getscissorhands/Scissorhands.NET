using System.Collections.Generic;

using Newtonsoft.Json;

namespace Scissorhands.Models.Settings
{
    /// <summary>
    /// This represents the settings entity for site content.
    /// </summary>
    public class SiteSettings
    {
        /// <summary>
        /// Gets or sets the title of the website.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the description of the website.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the base URL of the website. eg) http://getscissorhands.net
        /// </summary>
        [JsonIgnore]
        public string BaseUrl { get; set; }

        /// <summary>
        /// Gets or sets the base path of the website. Trailing slash is optional. eg) /blog or /blog/
        /// </summary>
        [JsonIgnore]
        public string BasePath { get; set; }

        /// <summary>
        /// Gets or sets the list of <see cref="Author"/> objects.
        /// </summary>
        [JsonIgnore]
        public List<Author> Authors { get; set; }

        /// <summary>
        /// Gets or sets the list of <see cref="FeedType"/> values.
        /// </summary>
        [JsonIgnore]
        public List<FeedType> FeedTypes { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="SocialMedia"/> object.
        /// </summary>
        public SocialMedia SocialMedia { get; set; }
    }
}