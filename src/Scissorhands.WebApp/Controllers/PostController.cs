using System;
using System.IO;
using System.Threading.Tasks;

using Aliencube.Scissorhands.Models;
using Aliencube.Scissorhands.Services;
using Aliencube.Scissorhands.WebApp.ViewModels.Post;

using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.Mvc.ViewEngines;
using Microsoft.AspNet.Mvc.ViewFeatures;

namespace Aliencube.Scissorhands.WebApp.Controllers
{
    /// <summary>
    /// This represents the controller entity for post.
    /// </summary>
    [Route("admin/post")]
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
        public async Task<IActionResult> Publish(PostFormViewModel model)
        {
            var markdown = model.Body;
            var html = this._markdownService.Parse(model.Body);

            var vm = new PostViewViewModel() { Theme = this._settings.Theme, Markdown = markdown, Html = html };
            var result = await this.RenderPartialViewToString(vm).ConfigureAwait(false);
            return this.View(model);
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <remarks>http://stackoverflow.com/questions/31905624/where-are-the-controllercontext-and-viewengines-properties-in-mvc-6-controller#31906578</remarks>
        private async Task<string> RenderPartialViewToString(object model)
        {
            var viewName = "Build";

            this.ViewData.Model = model;

            string result = null;
            using (var writer = new StringWriter())
            {
                var viewEngine = this.Resolver.GetService(typeof(ICompositeViewEngine)) as ICompositeViewEngine;
                if (viewEngine == null)
                {
                    return null;
                }

                var viewResult = viewEngine.FindPartialView(this.ActionContext, viewName);
                var viewContext = new ViewContext(this.ActionContext, viewResult.View, this.ViewData, this.TempData, writer, new HtmlHelperOptions());

                await viewResult.View.RenderAsync(viewContext).ConfigureAwait(false);

                result = writer.GetStringBuilder().ToString();
            }

            return result;
        }
    }
}