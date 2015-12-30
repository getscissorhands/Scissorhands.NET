using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Aliencube.Scissorhands.Models;
using Aliencube.Scissorhands.Services;
using Aliencube.Scissorhands.ViewModels.Post;

using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.PlatformAbstractions;

using Newtonsoft.Json;

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
        private readonly IPublishService _publishService;

        /// <summary>
        /// Initializes a new instance of the <see cref="PostController"/> class.
        /// </summary>
        /// <param name="settings"><see cref="WebAppSettings"/> instance.</param>
        /// <param name="markdownService"><see cref="IMarkdownService"/> instance.</param>
        /// <param name="publishService"><see cref="IPublishService"/> instance.</param>
        public PostController(WebAppSettings settings, IMarkdownService markdownService, IPublishService publishService)
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

            if (publishService == null)
            {
                throw new ArgumentNullException(nameof(publishService));
            }

            this._publishService = publishService;
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
            if (model == null)
            {
                return new HttpStatusCodeResult((int)HttpStatusCode.BadRequest);
            }

            var vm = new PostViewViewModel() { Theme = this._settings.Theme };

            var parsedHtml = this._markdownService.Parse(model.Body);
            vm.Html = parsedHtml;

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
            if (model == null)
            {
                return new HttpStatusCodeResult((int)HttpStatusCode.BadRequest);
            }

            var vm = new PostPublishViewModel() { Theme = this._settings.Theme, Markdown = model.Body };

            var parsedHtml = this._markdownService.Parse(model.Body);
            vm.Html = parsedHtml;

            var appEnv = this.Resolver.GetService(typeof(IApplicationEnvironment)) as IApplicationEnvironment;

            var markdownpath = await this._publishService.PublishMarkdownAsync(vm.Markdown, this.Resolver).ConfigureAwait(false);
            vm.Markdownpath = markdownpath;

            string html;
            using (var client = new HttpClient())
            using (var content = new StringContent(JsonConvert.SerializeObject(vm), Encoding.UTF8))
            {
                client.BaseAddress = new Uri(string.Join("://", this.Request.IsHttps ? "https" : "http", this.Request.Host.Value));
                content.Headers.ContentType.MediaType = "application/json";
                content.Headers.ContentType.CharSet = "utf-8";
                var response = await client.PostAsync("/admin/post/publish/html", content).ConfigureAwait(false);
                html = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }

            var postpath = await this._publishService.PublishPostAsync(html, this.Resolver).ConfigureAwait(false);
            vm.Postpath = postpath;

            return this.View(vm);
        }

        [Route("publish/html")]
        [HttpPost]
        public async Task<IActionResult> PublishHtml([FromBody] PostPublishViewModel model)
        {
            if (model == null)
            {
                return new HttpStatusCodeResult((int)HttpStatusCode.BadRequest);
            }

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