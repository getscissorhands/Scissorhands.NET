using System;
using System.Collections.Generic;

namespace Scissorhands.Models.Settings
{
    /// <summary>
    /// This provides interfaces to the <see cref="SiteMetadataSettings"/> class.
    /// </summary>
    public interface ISiteMetadataSettings : IDisposable
    {
        /// <summary>
        /// Gets or sets the title of the website.
        /// </summary>
        string Title { get; set; }

        /// <summary>
        /// Gets or sets the description of the website.
        /// </summary>
        string Description { get; set; }

        /// <summary>
        /// Gets or sets the base URL of the website. eg) http://getscissorhands.net
        /// </summary>
        string BaseUrl { get; set; }

        /// <summary>
        /// Gets or sets the base path of the website. Trailing slash is optional. eg) /blog or /blog/
        /// </summary>
        string BasePath { get; set; }

        /// <summary>
        /// Gets or sets the date/time format.
        /// </summary>
        string DateTimeFormat { get; set; }

        /// <summary>
        /// Gets or sets the list of <see cref="Author"/> objects.
        /// </summary>
        List<Author> Authors { get; set; }

        /// <summary>
        /// Gets or sets the list of <see cref="FeedType"/> values.
        /// </summary>
        List<FeedType> FeedTypes { get; set; }

        /// <summary>
        /// Gets or sets the theme.
        /// </summary>
        string Theme { get; set; }

        /// <summary>
        /// Gets or sets the list of navigation links used in the header section.
        /// </summary>
        List<NavigationLink> HeaderNavigationLinks { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="SocialMedia"/> object.
        /// </summary>
        SocialMedia SocialMedia { get; set; }

        /// <summary>
        /// Gets the <see cref="Author"/> instance corresponding to the name provided.
        /// </summary>
        /// <param name="name">Name of the author.</param>
        /// <returns>Returns the <see cref="Author"/> instance.</returns>
        Author GetAuthor(string name);

        /// <summary>
        /// Gets the default author name.
        /// </summary>
        /// <returns>Returns the default author name.</returns>
        string GetDefaultAuthorName();
    }
}