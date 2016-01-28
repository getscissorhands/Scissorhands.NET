using System;
using System.Net;

using Microsoft.AspNet.Mvc;

using Scissorhands.Models.Requests;
using Scissorhands.Models.Responses;
using Scissorhands.Models.Settings;
using Scissorhands.ViewModels.Post;

namespace Scissorhands.WebApp.Controllers
{
    /// <summary>
    /// This represents the controller entity for post.
    /// </summary>
    public partial class PostController
    {
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
        public IActionResult PreviewHtml([FromBody] MarkdownPreviewRequest request)
        {
            var parsedHtml = this._markdownHelper.Parse(request.Value);
            var response = new MarkdownPreviewResponse() { Value = parsedHtml };
            return new JsonResult(response);
        }
    }
}