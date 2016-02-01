using System;
using System.Net;
using System.Threading.Tasks;

using Microsoft.AspNet.Mvc;

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

            var vm = this._viewModelService.CreatePostPublishViewModel();
            vm.Page = this.GetPageMetadata(model, PublishMode.Publish);

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

            var vm = this._viewModelService.CreatePostParseViewModel();
            vm.Page = this.GetPageMetadata(model, PublishMode.Publish);

            var parsedHtml = this._markdownHelper.Parse(model.Body);
            vm.Html = parsedHtml;

            return this.View(vm);
        }
    }
}