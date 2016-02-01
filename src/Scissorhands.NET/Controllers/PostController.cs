using System;

using Microsoft.AspNet.Mvc;

using Scissorhands.Helpers;
using Scissorhands.Models.Settings;
using Scissorhands.Services;
using Scissorhands.ViewModels.Post;

namespace Scissorhands.WebApp.Controllers
{
    /// <summary>
    /// This represents the controller entity for post.
    /// </summary>
    [Route("admin/post")]
    public partial class PostController : BaseController
    {
        private readonly IMarkdownHelper _markdownHelper;
        private readonly IThemeService _themeService;
        private readonly IViewModelService _viewModelService;
        private readonly IPublishService _publishService;

        /// <summary>
        /// Initializes a new instance of the <see cref="PostController"/> class.
        /// </summary>
        /// <param name="metadata"><see cref="SiteMetadataSettings"/> instance.</param>
        /// <param name="requestHelper"><see cref="IHttpRequestHelper"/> instance.</param>
        /// <param name="markdownHelper"><see cref="IMarkdownHelper"/> instance.</param>
        /// <param name="themeService"><see cref="IThemeService"/> instance.</param>
        /// <param name="viewModelService"><see cref="IViewModelService"/> instance.</param>
        /// <param name="publishService"><see cref="IPublishService"/> instance.</param>
        public PostController(SiteMetadataSettings metadata, IHttpRequestHelper requestHelper, IMarkdownHelper markdownHelper, IThemeService themeService, IViewModelService viewModelService, IPublishService publishService)
            : base(metadata, requestHelper)
        {
            if (markdownHelper == null)
            {
                throw new ArgumentNullException(nameof(markdownHelper));
            }

            this._markdownHelper = markdownHelper;

            if (themeService == null)
            {
                throw new ArgumentNullException(nameof(themeService));
            }

            this._themeService = themeService;

            if (viewModelService == null)
            {
                throw new ArgumentNullException(nameof(viewModelService));
            }

            this._viewModelService = viewModelService;

            if (publishService == null)
            {
                throw new ArgumentNullException(nameof(publishService));
            }

            this._publishService = publishService;
        }

        /// <summary>
        /// Processes /admin/post/index.
        /// </summary>
        /// <returns>Returns the view model.</returns>
        public IActionResult Index()
        {
            return this.RedirectToRoute("write");
        }

        /// <summary>
        /// Processes /admin/post/write.
        /// </summary>
        /// <returns>Returns the view model.</returns>
        [Route("write", Name = "write")]
        public IActionResult Write()
        {
            var vm = new PostFormViewModel()
                         {
                             SlugPrefix = this.RequestHelper.GetSlugPrefix(this.Request),
                             Author = this.GetDefaultAuthorName(),
                         };

            return this.View(vm);
        }

        /// <summary>
        /// Processes /admin/post/edit.
        /// </summary>
        /// <returns>Returns the view model.</returns>
        [Route("edit")]
        public IActionResult Edit()
        {
            return this.View();
        }
    }
}