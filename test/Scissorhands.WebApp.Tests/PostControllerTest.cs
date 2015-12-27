using Aliencube.Scissorhands.Services;
using Aliencube.Scissorhands.WebApp.Controllers;
using Aliencube.Scissorhands.WebApp.Tests.Fixtures;
using Aliencube.Scissorhands.WebApp.ViewModels.Post;

using FluentAssertions;

using Microsoft.AspNet.Mvc;

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
        private readonly Mock<IMarkdownService> _markdownService;
        private readonly PostController _controller;

        /// <summary>
        /// Initializes a new instance of the <see cref="PostControllerTest"/> class.
        /// </summary>
        /// <param name="fixture"><see cref="PostControllerFixture"/> instance.</param>
        public PostControllerTest(PostControllerFixture fixture)
        {
            this._defaultThemeName = fixture.DefaultThemeName;
            this._markdownService = fixture.MarkdownService;
            this._controller = fixture.Controller;
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
        /// Tests whether the action should return <see cref="ViewResult"/> instance or not.
        /// </summary>
        /// <param name="markdown">String value in Markdown format.</param>
        /// <param name="html">String value in HTML format.</param>
        [Theory]
        [InlineData("**Hello World", "<p>Joe Bloggs</p>")]
        public void Given_Preview_WithModel_ShouldReturn_ViewResult(string markdown, string html)
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
    }
}