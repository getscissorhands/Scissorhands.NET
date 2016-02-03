using System;

using Microsoft.AspNet.Http;

using Scissorhands.Extensions;
using Scissorhands.Helpers;
using Scissorhands.Models.Settings;
using Scissorhands.ViewModels.Post;

namespace Scissorhands.Services
{
    /// <summary>
    /// This represents the service entity for view models.
    /// </summary>
    public class ViewModelService : IViewModelService
    {
        private readonly ISiteMetadataSettings _metadata;
        private readonly IHttpRequestHelper _requestHelper;
        private readonly IThemeHelper _themeHelper;

        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewModelService"/> class.
        /// </summary>
        /// <param name="metadata"><see cref="ISiteMetadataSettings"/> instance.</param>
        /// <param name="requestHelper"><see cref="IHttpRequestHelper"/> instance.</param>
        /// <param name="themeHelper"><see cref="IThemeHelper"/> instance.</param>
        public ViewModelService(ISiteMetadataSettings metadata, IHttpRequestHelper requestHelper, IThemeHelper themeHelper)
        {
            if (metadata == null)
            {
                throw new ArgumentNullException(nameof(metadata));
            }

            this._metadata = metadata;

            if (requestHelper == null)
            {
                throw new ArgumentNullException(nameof(requestHelper));
            }

            this._requestHelper = requestHelper;

            if (themeHelper == null)
            {
                throw new ArgumentNullException(nameof(themeHelper));
            }

            this._themeHelper = themeHelper;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="PostFormViewModel"/> class.
        /// </summary>
        /// <param name="request"><see cref="HttpRequest"/> instance.</param>
        /// <returns>Returns the <see cref="PostFormViewModel"/> instance created.</returns>
        public PostFormViewModel CreatePostFormViewModel(HttpRequest request)
        {
            var vm = new PostFormViewModel()
                         {
                             SlugPrefix = this._requestHelper.GetSlugPrefix(request),
                             Author = this._metadata.GetDefaultAuthorName(),
                         };
            return vm;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="PostPreviewViewModel"/> class.
        /// </summary>
        /// <returns>Returns the <see cref="PostPreviewViewModel"/> instance created.</returns>
        public PostPreviewViewModel CreatePostPreviewViewModel()
        {
            var vm = new PostPreviewViewModel()
                         {
                             Theme = this._metadata.Theme,
                             HeadPartialViewPath = this._themeHelper.GetHeadPartialViewPath(this._metadata.Theme),
                             HeaderPartialViewPath = this._themeHelper.GetHeaderPartialViewPath(this._metadata.Theme),
                             PostPartialViewPath = this._themeHelper.GetPostPartialViewPath(this._metadata.Theme),
                             FooterPartialViewPath = this._themeHelper.GetFooterPartialViewPath(this._metadata.Theme),
                         };
            return vm;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="PostPublishViewModel"/> class.
        /// </summary>
        /// <returns>Returns the <see cref="PostPublishViewModel"/> instance created.</returns>
        public PostPublishViewModel CreatePostPublishViewModel()
        {
            var vm = new PostPublishViewModel
                         {
                             Theme = this._metadata.Theme,
                             HeadPartialViewPath = this._themeHelper.GetHeadPartialViewPath(this._metadata.Theme),
                             HeaderPartialViewPath = this._themeHelper.GetHeaderPartialViewPath(this._metadata.Theme),
                             PostPartialViewPath = this._themeHelper.GetPostPartialViewPath(this._metadata.Theme),
                             FooterPartialViewPath = this._themeHelper.GetFooterPartialViewPath(this._metadata.Theme),
                         };
            return vm;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="PostParseViewModel"/> class.
        /// </summary>
        /// <returns>Returns the <see cref="PostParseViewModel"/> instance created.</returns>
        public PostParseViewModel CreatePostParseViewModel()
        {
            var vm = new PostParseViewModel()
                         {
                             Theme = this._metadata.Theme,
                             HeadPartialViewPath = this._themeHelper.GetHeadPartialViewPath(this._metadata.Theme),
                             HeaderPartialViewPath = this._themeHelper.GetHeaderPartialViewPath(this._metadata.Theme),
                             PostPartialViewPath = this._themeHelper.GetPostPartialViewPath(this._metadata.Theme),
                             FooterPartialViewPath = this._themeHelper.GetFooterPartialViewPath(this._metadata.Theme),
                         };
            return vm;
        }

        /// <summary>
        /// Gets the <see cref="PageMetadataSettings"/> instance.
        /// </summary>
        /// <param name="model"><see cref="PostFormViewModel"/> instance.</param>
        /// <param name="request"><see cref="HttpRequest"/> instance.</param>
        /// <param name="publishMode"><see cref="PublishMode"/> value.</param>
        /// <returns>Returns the <see cref="PageMetadataSettings"/> instance.</returns>
        public PageMetadataSettings CreatePageMetadata(PostFormViewModel model, HttpRequest request, PublishMode publishMode)
        {
            var page = new PageMetadataSettings
                           {
                               SiteTitle = this._metadata.Title,
                               Title = model.Title,
                               Excerpt = model.Excerpt,
                               Author = this._metadata.GetAuthor(model.Author),
                               Date = model.DatePublished.ToString(this._metadata.DateTimeFormat),
                               BaseUrl = this._requestHelper.GetBaseUri(request, publishMode).TrimTrailingSlash(),
                               Url = $"{this._requestHelper.GetSlugPrefix(request, publishMode)}/{model.Slug}.html",
                               HeaderNavigationLinks = this._metadata.HeaderNavigationLinks.OrDefault(),
                           };
            return page;
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