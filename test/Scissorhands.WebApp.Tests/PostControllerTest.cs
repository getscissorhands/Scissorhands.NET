using System;
using System.Net;
using System.Threading.Tasks;

using Aliencube.Scissorhands.Models;
using Aliencube.Scissorhands.Services;
using Aliencube.Scissorhands.ViewModels.Post;
using Aliencube.Scissorhands.WebApp.Controllers;
using Aliencube.Scissorhands.WebApp.Tests.Fixtures;

using FluentAssertions;

using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.ViewFeatures;

using Moq;

using Xunit;

namespace Aliencube.Scissorhands.WebApp.Tests
{
    /// <summary>
    /// This represents the test entity for the <see cref="PostController"/> class.
    /// </summary>
    public class PostControllerTest : IClassFixture<PostControllerFixture>
    {
        private readonly string _defaultThemeName;
        private readonly Mock<WebAppSettings> _settings;
        private readonly Mock<IMarkdownService> _markdownService;
        private readonly Mock<IPublishService> _publishService;
        private readonly PostController _controller;

        /// <summary>
        /// Initializes a new instance of the <see cref="PostControllerTest"/> class.
        /// </summary>
        /// <param name="fixture"><see cref="PostControllerFixture"/> instance.</param>
        public PostControllerTest(PostControllerFixture fixture)
        {
            this._defaultThemeName = fixture.DefaultThemeName;
            this._settings = fixture.WebAppSettings;
            this._markdownService = fixture.MarkdownService;
            this._publishService = fixture.PublishService;
            this._controller = fixture.Controller;
        }

        /// <summary>
        /// Tests whether the constructor throws an exception or not.
        /// </summary>
        [Fact]
        public void Given_NullParameter_Constructor_ShouldThrow_ArgumentNullException()
        {
            Action action1 = () => { var controller = new PostController(null, this._markdownService.Object, this._publishService.Object); };
            action1.ShouldThrow<ArgumentNullException>();

            Action action2 = () => { var controller = new PostController(this._settings.Object, null, this._publishService.Object); };
            action2.ShouldThrow<ArgumentNullException>();

            Action action3 = () => { var controller = new PostController(this._settings.Object, this._markdownService.Object, null); };
            action3.ShouldThrow<ArgumentNullException>();
        }

        /// <summary>
        /// Tests whether the action should return <see cref="RedirectToRouteResult"/> instance or not.
        /// </summary>
        [Fact]
        public void Given_Index_ShouldReturn_RedirectToRouteResult()
        {
            var result = this._controller.Index() as RedirectToRouteResult;
            result.Should().NotBeNull();
            result.RouteName.Should().Be("write");
        }

        /// <summary>
        /// Tests whether the action should return <see cref="ViewResult"/> instance or not.
        /// </summary>
        [Fact]
        public void Given_Write_ShouldReturn_ViewResult()
        {
            var result = this._controller.Write() as ViewResult;
            result.Should().NotBeNull();

            var vm = result.ViewData.Model as PostFormViewModel;
            vm.Should().NotBeNull();
        }

        /// <summary>
        /// Tests whether the action should return <see cref="HttpStatusCodeResult"/> instance or not.
        /// </summary>
        [Fact]
        public void Given_NullParameter_Preview_ShouldReturn_BadRequest()
        {
            var result = this._controller.Preview(null) as HttpStatusCodeResult;
            result.Should().NotBeNull();
            result.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        }

        /// <summary>
        /// Tests whether the action should return <see cref="ViewResult"/> instance or not.
        /// </summary>
        /// <param name="markdown">String value in Markdown format.</param>
        /// <param name="html">String value in HTML format.</param>
        [Theory]
        [InlineData("**Hello World", "<p>Joe Bloggs</p>")]
        public void Given_Model_Preview_ShouldReturn_ViewResult(string markdown, string html)
        {
            this._markdownService.Setup(p => p.Parse(It.IsAny<string>())).Returns(html);

            var model = new PostFormViewModel() { Title = "Title", Slug = "slug", Body = markdown };

            var result = this._controller.Preview(model) as ViewResult;
            result.Should().NotBeNull();

            var vm = result.ViewData.Model as PostViewViewModel;
            vm.Should().NotBeNull();

            vm.Theme.Should().Be(this._defaultThemeName);
            vm.Markdown.Should().Be(markdown);
            vm.Html.Should().Be(html);
        }

        /// <summary>
        /// Tests whether the action should return <see cref="HttpStatusCodeResult"/> instance or not.
        /// </summary>
        [Fact]
        public async void Given_NullParameter_Publish_ShouldReturn_BadRequest()
        {
            var result = await this._controller.Publish(null).ConfigureAwait(false) as HttpStatusCodeResult;
            result.Should().NotBeNull();
            result.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
        }

        /// <summary>
        /// Tests whether the action should return <see cref="ViewResult"/> instance or not.
        /// </summary>
        /// <param name="markdown">String value in Markdown format.</param>
        /// <param name="html">String value in HTML format.</param>
        /// <param name="markdownpath">Path of the Markdown file.</param>
        /// <param name="htmlpath">Path of the HTML post file.</param>
        [Theory]
        [InlineData("**Hello World", "<p>Joe Bloggs</p>", "~/Posts/markdown.md", "/posts/post.html")]
        public async void Given_Model_Publish_ShouldReturn_ViewResult(string markdown, string html, string markdownpath, string htmlpath)
        {
            this._markdownService.Setup(p => p.Parse(It.IsAny<string>())).Returns(html);
            this._publishService.Setup(p => p.PublishMarkdownAsync(It.IsAny<string>(), It.IsAny<IServiceProvider>())).Returns(Task.FromResult(markdownpath));
            this._publishService.Setup(
                p =>
                p.GetPostHtmlAsync(
                    It.IsAny<IServiceProvider>(),
                    It.IsAny<ActionContext>(),
                    It.IsAny<PostPublishViewModel>(),
                    It.IsAny<ViewDataDictionary>(),
                    It.IsAny<ITempDataDictionary>())).Returns(Task.FromResult(html));
            this._publishService.Setup(p => p.PublishPostAsync(It.IsAny<string>(), It.IsAny<IServiceProvider>())).Returns(Task.FromResult(htmlpath));

            var model = new PostFormViewModel() { Title = "Title", Slug = "slug", Body = markdown };

            var result = await this._controller.Publish(model).ConfigureAwait(false) as ViewResult;
            result.Should().NotBeNull();

            var vm = result.ViewData.Model as PostPublishViewModel;
            vm.Should().NotBeNull();

            vm.Theme.Should().Be(this._defaultThemeName);
            vm.Markdownpath.Should().Be(markdownpath);
            vm.Postpath.Should().Be(htmlpath);
            vm.Html.Should().Be(html);
        }
    }
}