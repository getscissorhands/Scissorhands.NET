using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using Microsoft.AspNet.Mvc;

using Scissorhands.Helpers;
using Scissorhands.Models.Requests;
using Scissorhands.Models.Responses;
using Scissorhands.Models.Settings;
using Scissorhands.Services;
using Scissorhands.ViewModels.Post;

namespace Scissorhands.WebApp.Controllers
{
    /// <summary>
    /// This represents the controller entity for post.
    /// </summary>
    [Route("admin/post")]
    public class PostController : BaseController
    {
        private readonly IMarkdownHelper _markdownHelper;
        private readonly IThemeService _themeService;
        private readonly IPublishService _publishService;

        /// <summary>
        /// Initializes a new instance of the <see cref="PostController"/> class.
        /// </summary>
        /// <param name="metadata"><see cref="SiteMetadataSettings"/> instance.</param>
        /// <param name="requestHelper"><see cref="IHttpRequestHelper"/> instance.</param>
        /// <param name="markdownHelper"><see cref="IMarkdownHelper"/> instance.</param>
        /// <param name="themeService"><see cref="IThemeService"/> instance.</param>
        /// <param name="publishService"><see cref="IPublishService"/> instance.</param>
        public PostController(SiteMetadataSettings metadata, IHttpRequestHelper requestHelper, IMarkdownHelper markdownHelper, IThemeService themeService, IPublishService publishService)
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

            model.DatePublished = DateTime.Now;

            var vm = new PostPreviewViewModel()
            {
                Theme = this.Metadata.Theme,
                HeadPartialViewPath = this._themeService.GetHeadPartialViewPath(this.Metadata.Theme),
                HeaderPartialViewPath = this._themeService.GetHeaderPartialViewPath(this.Metadata.Theme),
                PostPartialViewPath = this._themeService.GetPostPartialViewPath(this.Metadata.Theme),
                FooterPartialViewPath = this._themeService.GetFooterPartialViewPath(this.Metadata.Theme),
                Page = this.GetPageMetadata(model, PublishMode.Preview),
            };

            var parsedHtml = this._markdownHelper.Parse(model.Body);
            vm.Html = parsedHtml;

            return this.View(vm);
        }

        /// <summary>
        /// Processes /admin/post/preview/html API request.
        /// </summary>
        /// <param name="request"><see cref="MarkdownPreviewRequest"/> instance.</param>
        /// <returns>Returns the <see cref="MarkdownPreviewResponse"/> object.</returns>
        [Route("preview/html")]
        [HttpPost]
        public IActionResult WritePreview([FromBody] MarkdownPreviewRequest request)
        {
            var parsedHtml = this._markdownHelper.Parse(request.Value);
            var response = new MarkdownPreviewResponse() { Value = parsedHtml };
            return new JsonResult(response);
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

            model.DatePublished = DateTime.Now;

            var vm = new PostPublishViewModel
                         {
                             Theme = this.Metadata.Theme,
                             HeadPartialViewPath = this._themeService.GetHeadPartialViewPath(this.Metadata.Theme),
                             HeaderPartialViewPath = this._themeService.GetHeaderPartialViewPath(this.Metadata.Theme),
                             PostPartialViewPath = this._themeService.GetPostPartialViewPath(this.Metadata.Theme),
                             FooterPartialViewPath = this._themeService.GetFooterPartialViewPath(this.Metadata.Theme),
                             Page = this.GetPageMetadata(model, PublishMode.Publish),
                         };

            var publishedpath = await this._publishService.PublishPostAsync(model, this.Request).ConfigureAwait(false);
            vm.MarkdownPath = publishedpath.Markdown;
            vm.HtmlPath = publishedpath.Html;

            return this.View(vm);
        }

        /// <summary>
        /// Processes /admin/post/publish/html.
        /// </summary>
        /// <param name="model"><see cref="PostFormViewModel"/> instance.</param>
        /// <returns>Returns the view model.</returns>
        [Route("publish/html")]
        [HttpPost]
        public IActionResult PublishHtml([FromBody] PostFormViewModel model)
        {
            if (model == null)
            {
                return new HttpStatusCodeResult((int)HttpStatusCode.BadRequest);
            }

            var vm = new PostParseViewModel()
                         {
                             Theme = this.Metadata.Theme,
                             HeadPartialViewPath = this._themeService.GetHeadPartialViewPath(this.Metadata.Theme),
                             HeaderPartialViewPath = this._themeService.GetHeaderPartialViewPath(this.Metadata.Theme),
                             PostPartialViewPath = this._themeService.GetPostPartialViewPath(this.Metadata.Theme),
                             FooterPartialViewPath = this._themeService.GetFooterPartialViewPath(this.Metadata.Theme),
                             Page = this.GetPageMetadata(model, PublishMode.Publish),
                         };

            var parsedHtml = this._markdownHelper.Parse(model.Body);
            vm.Html = parsedHtml;

            return this.View(vm);
        }
    }
}