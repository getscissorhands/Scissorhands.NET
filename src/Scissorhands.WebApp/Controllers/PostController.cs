using System;
using System.Net;
using System.Threading.Tasks;

using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.PlatformAbstractions;

using Scissorhands.Helpers;
using Scissorhands.Models.Posts;
using Scissorhands.Models.Settings;
using Scissorhands.Services;
using Scissorhands.WebApp.ViewModels.Post;

namespace Scissorhands.WebApp.Controllers
{
    /// <summary>
    /// This represents the controller entity for post.
    /// </summary>
    [Route("admin/post")]
    public class PostController : Controller
    {
        private readonly WebAppSettings _settings;
        private readonly IMarkdownHelper _markdownHelper;
        private readonly IThemeService _themeService;
        private readonly IPublishService _publishService;

        /// <summary>
        /// Initializes a new instance of the <see cref="PostController"/> class.
        /// </summary>
        /// <param name="settings"><see cref="WebAppSettings"/> instance.</param>
        /// <param name="markdownHelper"><see cref="IMarkdownHelper"/> instance.</param>
        /// <param name="themeService"><see cref="IThemeService"/> instance.</param>
        /// <param name="publishService"><see cref="IPublishService"/> instance.</param>
        public PostController(WebAppSettings settings, IMarkdownHelper markdownHelper, IThemeService themeService, IPublishService publishService)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            this._settings = settings;

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
            var vm = new PostFormViewModel();
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

        /// <summary>
        /// Processes /admin/post/preview.
        /// </summary>
        /// <param name="model"><see cref="PostFormViewModel"/> instance.</param>
        /// <returns>Returns the view model.</returns>
        [Route("preview")]
        [HttpPost]
        public IActionResult Preview(PostFormViewModel model)
        {
            if (model == null)
            {
                return new HttpStatusCodeResult((int)HttpStatusCode.BadRequest);
            }

            var vm = new PostPreviewViewModel()
                         {
                             Theme = this._settings.Theme,
                             HeadPartialViewPath = this._themeService.GetHeadPartialViewPath(this._settings.Theme),
                             HeaderPartialViewPath = this._themeService.GetHeaderPartialViewPath(this._settings.Theme),
                             PostPartialViewPath = this._themeService.GetPostPartialViewPath(this._settings.Theme),
                             FooterPartialViewPath = this._themeService.GetFooterPartialViewPath(this._settings.Theme),
                         };

            var parsedHtml = this._markdownHelper.Parse(model.Body);
            vm.Html = parsedHtml;

            return this.View(vm);
        }

        /// <summary>
        /// Processes /admin/post/publish.
        /// </summary>
        /// <param name="model"><see cref="PostFormViewModel"/> instance.</param>
        /// <returns>Returns the view model.</returns>
        [Route("publish")]
        [HttpPost]
        public async Task<IActionResult> Publish(PostFormViewModel model)
        {
            if (model == null)
            {
                return new HttpStatusCodeResult((int)HttpStatusCode.BadRequest);
            }

            var vm = new PostPublishViewModel
                         {
                             Theme = this._settings.Theme,
                             HeadPartialViewPath = this._themeService.GetHeadPartialViewPath(this._settings.Theme),
                             HeaderPartialViewPath = this._themeService.GetHeaderPartialViewPath(this._settings.Theme),
                             PostPartialViewPath = this._themeService.GetPostPartialViewPath(this._settings.Theme),
                             FooterPartialViewPath = this._themeService.GetFooterPartialViewPath(this._settings.Theme),
                         };

            var env = this.Resolver.GetService(typeof(IApplicationEnvironment)) as IApplicationEnvironment;

            var publishedpath = await this._publishService.PublishPostAsync(model.Body, env, this.Request).ConfigureAwait(false);
            vm.MarkdownPath = publishedpath.Markdown;
            vm.HtmlPath = publishedpath.Html;

            return this.View(vm);
        }

        /// <summary>
        /// Processes /admin/post/publish/html.
        /// </summary>
        /// <param name="body"><see cref="PublishedContent"/> instance read from request body.</param>
        /// <returns>Returns the view model.</returns>
        [Route("publish/html")]
        [HttpPost]
        public IActionResult PublishHtml([FromBody] PublishedContent body)
        {
            if (body == null)
            {
                return new HttpStatusCodeResult((int)HttpStatusCode.BadRequest);
            }

            var vm = body;
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