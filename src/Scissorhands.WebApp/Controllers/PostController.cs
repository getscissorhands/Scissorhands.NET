using System;

using Aliencube.Scissorhands.Models;
using Aliencube.Scissorhands.Services;
using Aliencube.Scissorhands.WebApp.ViewModels.Post;

using Microsoft.AspNet.Mvc;

namespace Aliencube.Scissorhands.WebApp.Controllers
{
    /// <summary>
    /// This represents the controller entity for post.
    /// </summary>
    [Route("post")]
    public class PostController : Controller
    {
        private readonly WebAppSettings _settings;
        private readonly IMarkdownService _markdownService;

        /// <summary>
        /// Initializes a new instance of the <see cref="PostController"/> class.
        /// </summary>
        /// <param name="settings"><see cref="WebAppSettings"/> instance.</param>
        /// <param name="markdownService"><see cref="IMarkdownService"/> instance.</param>
        public PostController(WebAppSettings settings, IMarkdownService markdownService)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            this._settings = settings;

            if (markdownService == null)
            {
                throw new ArgumentNullException(nameof(markdownService));
            }

            this._markdownService = markdownService;
        }

        /// <summary>
        /// Processes /post/index.
        /// </summary>
        /// <returns>Returns the view model.</returns>
        public IActionResult Index()
        {
            return this.RedirectToRoute("write");
        }

        /// <summary>
        /// Processes /post/write.
        /// </summary>
        /// <returns>Returns the view model.</returns>
        [Route("write", Name = "write")]
        public IActionResult Write()
        {
            var vm = new PostFormViewModel();
            return this.View(vm);
        }

        /// <summary>
        /// Processes /post/edit.
        /// </summary>
        /// <returns>Returns the view model.</returns>
        [Route("edit")]
        public IActionResult Edit()
        {
            return this.View();
        }

        /// <summary>
        /// Processes /post/preview.
        /// </summary>
        /// <param name="model"><see cref="PostFormViewModel"/> instance.</param>
        /// <returns>Returns the view model.</returns>
        [Route("preview")]
        [HttpPost]
        public IActionResult Preview(PostFormViewModel model)
        {
            var markdown = model.Body;
            var html = this._markdownService.Parse(model.Body);

            var vm = new PostViewViewModel() { Theme = this._settings.Theme, Markdown = markdown, Html = html };
            return this.View(vm);
        }

        /// <summary>
        /// Processes /post/publish.
        /// </summary>
        /// <param name="model"><see cref="PostFormViewModel"/> instance.</param>
        /// <returns>Returns the view model.</returns>
        [Route("publish")]
        [HttpPost]
        public IActionResult Publish(PostFormViewModel model)
        {
            var vm = model;
            return this.View(vm);
        }

        /// <summary>
        /// Processes /post/build.
        /// </summary>
        /// <returns>Returns the view model.</returns>
        [Route("build")]
        public IActionResult Build()
        {
            return this.View();
        }

        /// <summary>
        /// Processes /post/view.
        /// </summary>
        /// <returns>Returns the view model.</returns>
        [Route("view")]
        public IActionResult PostView()
        {
            return this.View();
        }
    }
}