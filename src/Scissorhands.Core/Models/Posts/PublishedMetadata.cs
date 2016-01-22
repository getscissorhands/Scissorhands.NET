using System;
using System.Collections.Generic;

namespace Scissorhands.Models.Posts
{
    /// <summary>
    /// This represents the entity for published metadata.
    /// </summary>
    public class PublishedMetadata
    {
        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the slug.
        /// </summary>
        public string Slug { get; set; }

        /// <summary>
        /// Gets or sets the author.
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// Gets or sets the date published.
        /// </summary>
        public DateTime DatePublished { get; set; }

        /// <summary>
        /// Gets or sets the list of tags.
        /// </summary>
        public List<string> Tags { get; set; }
    }
}