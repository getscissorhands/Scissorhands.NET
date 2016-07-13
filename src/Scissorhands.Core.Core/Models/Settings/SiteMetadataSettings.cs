using System;
using System.Collections.Generic;
using System.Linq;

using Scissorhands.Exceptions;

namespace Scissorhands.Models.Settings
{
    /// <summary>
    /// This represents the settings entity for site content.
    /// </summary>
    public class SiteMetadataSettings : ISiteMetadataSettings
    {
        private bool _disposed;

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
        public string BaseUrl { get; set; }

        /// <summary>
        /// Gets or sets the base path of the website. Trailing slash is optional. eg) /blog or /blog/
        /// </summary>
        public string BasePath { get; set; }

        /// <summary>
        /// Gets or sets the date/time format.
        /// </summary>
        public string DateTimeFormat { get; set; }

        /// <summary>
        /// Gets or sets the list of <see cref="Author"/> objects.
        /// </summary>
        public List<Author> Authors { get; set; }

        /// <summary>
        /// Gets or sets the list of <see cref="FeedType"/> values.
        /// </summary>
        public List<FeedType> FeedTypes { get; set; }

        /// <summary>
        /// Gets or sets the theme.
        /// </summary>
        public string Theme { get; set; }

        /// <summary>
        /// Gets or sets the list of navigation links used in the header section.
        /// </summary>
        public List<NavigationLink> HeaderNavigationLinks { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="SocialMedia"/> object.
        /// </summary>
        public SocialMedia SocialMedia { get; set; }

        /// <summary>
        /// Gets the <see cref="Author"/> instance corresponding to the name provided.
        /// </summary>
        /// <param name="name">Name of the author.</param>
        /// <returns>Returns the <see cref="Author"/> instance.</returns>
        public Author GetAuthor(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            var author = this.Authors
                             .SingleOrDefault(p => p.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
            return author;
        }

        /// <summary>
        /// Gets the default author name.
        /// </summary>
        /// <returns>Returns the default author name.</returns>
        public string GetDefaultAuthorName()
        {
            var author = this.Authors.FirstOrDefault(p => p.IsDefault);
            if (author == null)
            {
                throw new AuthorNotFoundException("Author not found");
            }

            return author.Name;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (this._disposed)
            {
                return;
            }

            this._disposed = true;
        }
    }
}