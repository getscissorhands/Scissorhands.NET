using System;

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
        private readonly SiteMetadataSettings _metadata;
        private readonly IThemeHelper _themeHelper;

        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewModelService"/> class.
        /// </summary>
        /// <param name="metadata"><see cref="SiteMetadataSettings"/> instance.</param>
        /// <param name="themeHelper"><see cref="IThemeHelper"/> instance.</param>
        public ViewModelService(SiteMetadataSettings metadata, IThemeHelper themeHelper)
        {
            if (metadata == null)
            {
                throw new ArgumentNullException(nameof(metadata));
            }

            this._metadata = metadata;

            if (themeHelper == null)
            {
                throw new ArgumentNullException(nameof(themeHelper));
            }

            this._themeHelper = themeHelper;
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