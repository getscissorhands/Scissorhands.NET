using Aliencube.Scissorhands.Models;
using Aliencube.Scissorhands.Services.Tests.Fixtures;

using FluentAssertions;

using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.Routing;

using Moq;

using Xunit;

namespace Aliencube.Scissorhands.Services.Tests
{
    /// <summary>
    /// This represents the test entity for the <see cref="ThemeService"/> class.
    /// </summary>
    public class ThemeServiceTest : IClassFixture<ThemeServiceFixture>
    {
        private readonly Mock<WebAppSettings> _settings;
        private readonly IThemeService _service;

        private readonly RouteData _routeData;
        private readonly string _defaultThemeName;

        /// <summary>
        /// Initializes a new instance of the <see cref="ThemeServiceTest"/> class.
        /// </summary>
        /// <param name="fixture"><see cref="ThemeServiceFixture"/> instance.</param>
        public ThemeServiceTest(ThemeServiceFixture fixture)
        {
            this._settings = fixture.WebAppSettings;
            this._service = fixture.ThemeService;

            this._routeData = new RouteData();

            this._defaultThemeName = "default";
            this._settings.SetupGet(p => p.Theme).Returns(this._defaultThemeName);
        }

        [Theory]
        [InlineData("", "")]
        public void Given_EmptyController_Should_Return_DefaultLayout(string controllerName, string actionName)
        {
            this._routeData.Values.Add("controller", controllerName);
            this._routeData.Values.Add("action", actionName);

            var context = new ViewContext { RouteData = this._routeData };

            var layout = this._service.GetLayout(context);
            layout.Should().BeEquivalentTo("~/Views/Shared/_Layout.cshtml");
        }

        [Theory]
        [InlineData("home", "")]
        public void Given_DifferentController_Should_Return_DefaultLayout(string controllerName, string actionName)
        {
            this._routeData.Values.Add("controller", controllerName);
            this._routeData.Values.Add("action", actionName);

            var context = new ViewContext { RouteData = this._routeData };

            var layout = this._service.GetLayout(context);
            layout.Should().BeEquivalentTo("~/Views/Shared/_Layout.cshtml");
        }

        [Theory]
        [InlineData("post", "test")]
        public void Given_DifferentAction_Should_Return_DefaultLayout(string controllerName, string actionName)
        {
            this._routeData.Values.Add("controller", controllerName);
            this._routeData.Values.Add("action", actionName);

            var context = new ViewContext { RouteData = this._routeData };

            var layout = this._service.GetLayout(context);
            layout.Should().BeEquivalentTo("~/Views/Shared/_Layout.cshtml");
        }

        [Theory]
        [InlineData("post", "")]
        public void Given_EmptyAction_Should_Return_ThemeLayout(string controllerName, string actionName)
        {
            this._routeData.Values.Add("controller", controllerName);
            this._routeData.Values.Add("action", actionName);

            var context = new ViewContext { RouteData = this._routeData };

            var layout = this._service.GetLayout(context);
            layout.Should().BeEquivalentTo($"~/Themes/{this._defaultThemeName}/Shared/_Layout.cshtml");
        }

        [Theory]
        [InlineData("post", "preview")]
        public void Given_ViewContext_Should_Return_ThemeLayout(string controllerName, string actionName)
        {
            this._routeData.Values.Add("controller", controllerName);
            this._routeData.Values.Add("action", actionName);

            var context = new ViewContext { RouteData = this._routeData };

            var layout = this._service.GetLayout(context);
            layout.Should().BeEquivalentTo($"~/Themes/{this._defaultThemeName}/Shared/_Layout.cshtml");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void Given_NullThemeName_Should_Return_DefaultThemePostPath(string themeName)
        {
            var postpath = this._service.GetPost(themeName);
            postpath.Should().BeEquivalentTo($"~/themes/{this._defaultThemeName}/post/post.cshtml");
        }

        [Theory]
        [InlineData("test")]
        public void Given_ThemeName_Should_Return_ThemePostPath(string themeName)
        {
            var postpath = this._service.GetPost(themeName);
            postpath.Should().BeEquivalentTo($"~/themes/{themeName}/post/post.cshtml");
        }
    }
}