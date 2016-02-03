using System;
using System.Collections.Generic;

using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.ViewFeatures;
using Microsoft.Extensions.PlatformAbstractions;

using Moq;

using Scissorhands.Helpers;
using Scissorhands.Models.Settings;
using Scissorhands.Services;
using Scissorhands.ViewModels.Post;
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

            this.MarkdownService = new Mock<IMarkdownService>();

            this.ViewModelService = new Mock<IViewModelService>();

            this.PublishService = new Mock<IPublishService>();

            this.Controller = new PostController(this.MarkdownService.Object, this.ViewModelService.Object, this.PublishService.Object);

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
        /// Gets the <see cref="Mock{IMarkdownService}"/> instance.
        /// </summary>
        public Mock<IMarkdownService> MarkdownService { get; }

        /// <summary>
        /// Gets the <see cref="Mock{IViewModelService}"/> instance.
        /// </summary>
        public Mock<IViewModelService> ViewModelService { get; }

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