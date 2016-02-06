using System;

using Microsoft.AspNet.Mvc;

using Scissorhands.Services;

namespace Scissorhands.WebApp.Controllers
{
    /// <summary>
    /// This represents the controller entity for post.
    /// </summary>
    [Route("admin/post")]
    public partial class PostController : BaseController
    {
        private readonly IMarkdownService _markdownService;
        private readonly IViewModelService _viewModelService;
        private readonly IPublishService _publishService;

        /// <summary>
        /// Initializes a new instance of the <see cref="PostController"/> class.
        /// </summary>
        /// <param name="markdownService"><see cref="IMarkdownService"/> instance.</param>
        /// <param name="viewModelService"><see cref="IViewModelService"/> instance.</param>
        /// <param name="publishService"><see cref="IPublishService"/> instance.</param>
        public PostController(IMarkdownService markdownService, IViewModelService viewModelService, IPublishService publishService)
        {
            if (markdownService == null)
            {
                throw new ArgumentNullException(nameof(markdownService));
            }

            this._markdownService = markdownService;

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
            var vm = this._viewModelService.CreatePostFormViewModel(this.Request);

            return this.View(vm);
        }

        /// <summary>
        /// Processes /admin/post/edit.
        /// </summary>
        /// <param name="filename">Filename to edit.</param>
        /// <returns>Returns the view model.</returns>
        [Route("edit")]
        public IActionResult Edit(string filename)
        {
            return this.View();
        }

        /// <summary>
        /// Processes /admin/post/delete.
        /// </summary>
        /// <param name="filename">Filename to edit.</param>
        /// <returns>Returns the view model.</returns>
        [Route("delete")]
        public IActionResult Delete(string filename)
        {
            return this.View();
        }
    }
}