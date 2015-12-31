using System;

using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.ViewFeatures;
using Microsoft.Extensions.PlatformAbstractions;

using Moq;

using Scissorhands.Helpers;
using Scissorhands.Models.Settings;
using Scissorhands.Services;
using Scissorhands.WebApp.Controllers;

namespace Scissorhands.WebApp.Tests.Fixtures
{
    /// <summary>
    /// This represents the fixture entity for the <see cref="PostController"/> class.
    /// </summary>
    public class PostControllerFixture : BaseControllerFixture<PostController>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PostControllerFixture"/> class.
        /// </summary>
        public PostControllerFixture()
        {
            this.DefaultThemeName = "default";

            this.WebAppSettings = new Mock<WebAppSettings>();
            this.WebAppSettings.SetupGet(p => p.Theme).Returns(this.DefaultThemeName);

            this.MarkdownHelper = new Mock<IMarkdownHelper>();

            this.PublishService = new Mock<IPublishService>();

            this.Controller = new PostController(this.WebAppSettings.Object, this.MarkdownHelper.Object, this.PublishService.Object);

            this.ApplicationEnvironment = new Mock<IApplicationEnvironment>();

            this.UrlHelper = new Mock<IUrlHelper>();

            this.RequestServices = new Mock<IServiceProvider>();

            this.HttpContext = new Mock<HttpContext>();

            this.ActionContext = new ActionContext { HttpContext = this.HttpContext.Object };
            this.TempData = new Mock<ITempDataDictionary>();
        }

        /// <summary>
        /// Gets or sets the theme name.
        /// </summary>
        public string DefaultThemeName { get; set; }

        /// <summary>
        /// Gets the <see cref="Mock{WebAppSettings}"/> instance.
        /// </summary>
        public Mock<WebAppSettings> WebAppSettings { get; }

        /// <summary>
        /// Gets the <see cref="Mock{IMarkdownHelper}"/> instance.
        /// </summary>
        public Mock<IMarkdownHelper> MarkdownHelper { get; }

        /// <summary>
        /// Gets the <see cref="Mock{IPublishService}"/> instance.
        /// </summary>
        public Mock<IPublishService> PublishService { get; }

        /// <summary>
        /// Gets the <see cref="Mock{IApplicationEnvironment}"/> instance.
        /// </summary>
        public Mock<IApplicationEnvironment> ApplicationEnvironment { get; }
    }
}