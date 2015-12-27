using Aliencube.Scissorhands.Models;
using Aliencube.Scissorhands.Services;
using Aliencube.Scissorhands.WebApp.Controllers;

using Moq;

namespace Aliencube.Scissorhands.WebApp.Tests.Fixtures
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

            this.MarkdownService = new Mock<IMarkdownService>();

            this.PublishService = new Mock<IPublishService>();

            this.BuildService = new Mock<IBuildService>();

            this.Controller = new PostController(this.WebAppSettings.Object, this.MarkdownService.Object, this.PublishService.Object, this.BuildService.Object);
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
        /// Gets the <see cref="Mock{IMarkdownService}"/> instance.
        /// </summary>
        public Mock<IMarkdownService> MarkdownService { get; }

        /// <summary>
        /// Gets the <see cref="Mock{IPublishService}"/> instance.
        /// </summary>
        public Mock<IPublishService> PublishService { get; }

        /// <summary>
        /// Gets the <see cref="Mock{IBuildService}"/> instance.
        /// </summary>
        public Mock<IBuildService> BuildService { get; }
    }
}