using System;
using System.Linq;

using Microsoft.AspNet.Mvc;

using Scissorhands.Exceptions;
using Scissorhands.Extensions;
using Scissorhands.Helpers;
using Scissorhands.Models.Settings;
using Scissorhands.ViewModels.Post;

namespace Scissorhands.WebApp.Controllers
{
    /// <summary>
    /// This represents the base controller entity. This MUST be inherited.
    /// </summary>
    public abstract class BaseController : Controller
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseController"/> class.
        /// </summary>
        /// <param name="metadata"><see cref="SiteMetadataSettings"/> instance.</param>
        /// <param name="requestHelper"><see cref="IHttpRequestHelper"/> instance.</param>
        protected BaseController(SiteMetadataSettings metadata, IHttpRequestHelper requestHelper)
        {
            if (metadata == null)
            {
                throw new ArgumentNullException(nameof(metadata));
            }

            this.Metadata = metadata;

            if (requestHelper == null)
            {
                throw new ArgumentNullException(nameof(requestHelper));
            }

            this.RequestHelper = requestHelper;
        }

        /// <summary>
        /// Gets the <see cref="SiteMetadataSettings"/> instance.
        /// </summary>
        protected SiteMetadataSettings Metadata { get; }

        /// <summary>
        /// Gets the <see cref="IHttpRequestHelper"/> instance.
        /// </summary>
        protected IHttpRequestHelper RequestHelper { get; }

        /// <summary>
        /// Gets the <see cref="Author"/> instance corresponding to the name provided.
        /// </summary>
        /// <param name="name">Name of the author.</param>
        /// <returns>Returns the <see cref="Author"/> instance.</returns>
        protected Author GetAuthor(string name)
        {
            var author = this.Metadata
                             .Authors
                             .SingleOrDefault(p => p.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
            return author;
        }

        /// <summary>
        /// Gets the default author name.
        /// </summary>
        /// <returns>Returns the default author name.</returns>
        protected string GetDefaultAuthorName()
        {
            var author = this.Metadata.Authors.FirstOrDefault(p => p.IsDefault);
            if (author == null)
            {
                throw new AuthorNotFoundException("Author not found");
            }

            return author.Name;
        }

        /// <summary>
        /// Gets the <see cref="PageMetadataSettings"/> instance.
        /// </summary>
        /// <param name="model"><see cref="PostFormViewModel"/> instance.</param>
        /// <param name="publishMode"><see cref="PublishMode"/> value.</param>
        /// <returns>Returns the <see cref="PageMetadataSettings"/> instance.</returns>
        protected PageMetadataSettings GetPageMetadata(PostFormViewModel model, PublishMode publishMode)
        {
            var page = new PageMetadataSettings
                           {
                               SiteTitle = this.Metadata.Title,
                               Title = model.Title,
                               Excerpt = model.Excerpt,
                               Author = this.GetAuthor(model.Author),
                               Date = model.DatePublished.ToString(this.Metadata.DateTimeFormat),
                               BaseUrl = this.RequestHelper.GetBaseUri(this.Request, publishMode).TrimTrailingSlash(),
                               Url = $"{this.RequestHelper.GetSlugPrefix(this.Request, publishMode)}/{model.Slug}.html",
                               HeaderNavigationLinks = this.Metadata.HeaderNavigationLinks.OrDefault(),
                           };
            return page;
        }
    }
}