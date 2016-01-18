﻿using System;
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
            this.SlugPrefix = "http://localhost:5080/posts/2016/01/01";
            this.Authors = new List<Author>() { new Author() { Name = "Joe Bloggs", IsDefault = true } };

            this.SiteMetadataSettings = new Mock<SiteMetadataSettings>();
            this.SiteMetadataSettings.SetupGet(p => p.Theme).Returns(this.DefaultThemeName);
            this.SiteMetadataSettings.SetupGet(p => p.Authors).Returns(this.Authors);

            this.RequestHelper = new Mock<IHttpRequestHelper>();
            this.RequestHelper.Setup(p => p.GetSlugPrefix(It.IsAny<HttpRequest>(), It.IsAny<PageType?>())).Returns(this.SlugPrefix);

            this.MarkdownHelper = new Mock<IMarkdownHelper>();

            this.ThemeService = new Mock<IThemeService>();

            this.PublishService = new Mock<IPublishService>();

            this.Controller = new PostController(this.SiteMetadataSettings.Object, this.RequestHelper.Object, this.MarkdownHelper.Object, this.ThemeService.Object, this.PublishService.Object);

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
        /// Gets or sets the slug prefix.
        /// </summary>
        public string SlugPrefix { get; set; }

        /// <summary>
        /// Gets or sets the list of <see cref="Author"/> instances.
        /// </summary>
        public List<Author> Authors { get; set; }

        /// <summary>
        /// Gets the <see cref="Mock{SiteMetadataSettings}"/> instance.
        /// </summary>
        public Mock<SiteMetadataSettings> SiteMetadataSettings { get; }

        /// <summary>
        /// Gets the <see cref="Mock{IHttpRequestHelper}"/> instance.
        /// </summary>
        public Mock<IHttpRequestHelper> RequestHelper { get; }

        /// <summary>
        /// Gets the <see cref="Mock{IMarkdownHelper}"/> instance.
        /// </summary>
        public Mock<IMarkdownHelper> MarkdownHelper { get; }

        /// <summary>
        /// Gets the <see cref="Mock{ThemeService}"/>.
        /// </summary>
        public Mock<IThemeService> ThemeService { get; }

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