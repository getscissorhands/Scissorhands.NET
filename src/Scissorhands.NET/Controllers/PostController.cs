using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.PlatformAbstractions;

using Scissorhands.Exceptions;
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
        private readonly SiteMetadataSettings _metadata;
        private readonly IMarkdownHelper _markdownHelper;
        private readonly IThemeService _themeService;
        private readonly IPublishService _publishService;

        /// <summary>
        /// Initializes a new instance of the <see cref="PostController"/> class.
        /// </summary>
        /// <param name="metadata"><see cref="SiteMetadataSettings"/> instance.</param>
        /// <param name="markdownHelper"><see cref="IMarkdownHelper"/> instance.</param>
        /// <param name="themeService"><see cref="IThemeService"/> instance.</param>
        /// <param name="publishService"><see cref="IPublishService"/> instance.</param>
        public PostController(SiteMetadataSettings metadata, IMarkdownHelper markdownHelper, IThemeService themeService, IPublishService publishService)
        {
            if (metadata == null)
            {
                throw new ArgumentNullException(nameof(metadata));
            }

            this._metadata = metadata;

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
                             SlugPrefix = this.GetBaseUrl() + this.GetBasePath(),
                             Author = this.GetAuthorName(),
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
        public async Task<IActionResult> Preview(PostFormViewModel model)
        {
            if (model == null)
            {
                return new HttpStatusCodeResult((int)HttpStatusCode.BadRequest);
            }

            var vm = new PostPreviewViewModel()
                         {
                             Theme = this._metadata.Theme,
                             HeadPartialViewPath = this._themeService.GetHeadPartialViewPath(this._metadata.Theme),
                             HeaderPartialViewPath = this._themeService.GetHeaderPartialViewPath(this._metadata.Theme),
                             PostPartialViewPath = this._themeService.GetPostPartialViewPath(this._metadata.Theme),
                             FooterPartialViewPath = this._themeService.GetFooterPartialViewPath(this._metadata.Theme),
                             Page = this.GetPageMetadata(model),
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
                             Theme = this._metadata.Theme,
                             HeadPartialViewPath = this._themeService.GetHeadPartialViewPath(this._metadata.Theme),
                             HeaderPartialViewPath = this._themeService.GetHeaderPartialViewPath(this._metadata.Theme),
                             PostPartialViewPath = this._themeService.GetPostPartialViewPath(this._metadata.Theme),
                             FooterPartialViewPath = this._themeService.GetFooterPartialViewPath(this._metadata.Theme),
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

        private string GetAuthorName()
        {
            var author = this._metadata.Authors.FirstOrDefault(p => p.IsDefault);
            if (author == null)
            {
                throw new AuthorNotFoundException("Author not found");
            }

            return author.Name;
        }

        private string GetBaseUrl()
        {
            var baseUrl = this._metadata.BaseUrl;
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                throw new InvalidSettingsException("BaseUrl has not been set");
            }

            return baseUrl;
        }

        private string GetBasePath()
        {
            var basepath = this._metadata.BasePath;
            var today = $"{DateTime.Today.ToString("yyyy/MM/dd")}";

            if (!string.IsNullOrWhiteSpace(basepath))
            {
                basepath += $"/{today}";
            }

            return basepath;
        }

        private PageMetadataSettings GetPageMetadata(PostFormViewModel model)
        {
            var page = new PageMetadataSettings
            {
                Title = model.Title,
                Excerpt = model.Excerpt,
                Author = this._metadata.Authors.Single(p => p.Name.Equals(model.Author, StringComparison.CurrentCultureIgnoreCase)),
                Date = DateTime.Today,
                BaseUrl = this._metadata.BaseUrl,
                Url = $"{model.SlugPrefix}/{model.Slug}.html",
                HeaderNavigationLinks = this._metadata.HeaderNavigationLinks,
            };

            return page;
        }
    }
}