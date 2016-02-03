using System;

using FluentAssertions;

using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.Routing;

using Moq;

using Scissorhands.Models.Settings;
using Scissorhands.Services.Tests.Fixtures;

using Xunit;

namespace Scissorhands.Services.Tests
{
    /// <summary>
    /// This represents the test entity for the <see cref="ThemeService"/> class.
    /// </summary>
    public class ThemeServiceTest : IClassFixture<ThemeServiceFixture>
    {
        private readonly string _defaultThemeName;
        private readonly Mock<ISiteMetadataSettings> _metadata;
        private readonly IThemeService _service;
        private readonly RouteData _routeData;

        private ViewContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="ThemeServiceTest"/> class.
        /// </summary>
        /// <param name="fixture"><see cref="ThemeServiceFixture"/> instance.</param>
        public ThemeServiceTest(ThemeServiceFixture fixture)
        {
            this._defaultThemeName = fixture.DefaultThemeName;
            this._metadata = fixture.SiteMetadataSettings;
            this._service = fixture.ThemeService;

            this._routeData = new RouteData();
        }

        /// <summary>
        /// Tests whether the constructor should throw an exception or not.
        /// </summary>
        [Fact]
        public void Given_NullParameter_Constructor_ShouldThrow_ArgumentNullException()
        {
            Action action = () => { var service = new ThemeService(null); };
            action.ShouldThrow<ArgumentNullException>();
        }

        /// <summary>
        /// Tests whether the constructor should NOT throw an exception or not.
        /// </summary>
        [Fact]
        public void Given_Parameter_Constructor_ShouldThrow_NoException()
        {
            Action action = () => { var service = new ThemeService(this._metadata.Object); };
            action.ShouldNotThrow<Exception>();
        }

        /// <summary>
        /// Tests whether the method should return value or not.
        /// </summary>
        /// <param name="controllerName">Controller name.</param>
        /// <param name="actionName">Action name.</param>
        [Theory]
        [InlineData("", "")]
        public void Given_EmptyController_GetLayout_ShouldReturn_DefaultLayout(string controllerName, string actionName)
        {
            this.SetViewContext(controllerName, actionName);

            var layout = this._service.GetLayout(this._context);
            layout.Should().BeEquivalentTo("~/Views/Shared/_Layout.cshtml");
        }

        /// <summary>
        /// Tests whether the method should return value or not.
        /// </summary>
        /// <param name="controllerName">Controller name.</param>
        /// <param name="actionName">Action name.</param>
        [Theory]
        [InlineData("home", "")]
        public void Given_DifferentController_GetLayout_ShouldReturn_DefaultLayout(string controllerName, string actionName)
        {
            this.SetViewContext(controllerName, actionName);

            var layout = this._service.GetLayout(this._context);
            layout.Should().BeEquivalentTo("~/Views/Shared/_Layout.cshtml");
        }

        /// <summary>
        /// Tests whether the method should return value or not.
        /// </summary>
        /// <param name="controllerName">Controller name.</param>
        /// <param name="actionName">Action name.</param>
        [Theory]
        [InlineData("post", "test")]
        public void Given_DifferentAction_GetLayout_ShouldReturn_DefaultLayout(string controllerName, string actionName)
        {
            this.SetViewContext(controllerName, actionName);

            var layout = this._service.GetLayout(this._context);
            layout.Should().BeEquivalentTo("~/Views/Shared/_Layout.cshtml");
        }

        /// <summary>
        /// Tests whether the method should return value or not.
        /// </summary>
        /// <param name="controllerName">Controller name.</param>
        /// <param name="actionName">Action name.</param>
        [Theory]
        [InlineData("post", "")]
        public void Given_EmptyAction_GetLayout_ShouldReturn_ThemeLayout(string controllerName, string actionName)
        {
            this.SetViewContext(controllerName, actionName);

            var layout = this._service.GetLayout(this._context);
            layout.Should().BeEquivalentTo($"~/Themes/{this._defaultThemeName}/shared/_layout.cshtml");
        }

        /// <summary>
        /// Tests whether the method should return value or not.
        /// </summary>
        /// <param name="controllerName">Controller name.</param>
        /// <param name="actionName">Action name.</param>
        [Theory]
        [InlineData("post", "preview")]
        public void Given_ViewContext_GetLayout_ShouldReturn_ThemeLayout(string controllerName, string actionName)
        {
            this.SetViewContext(controllerName, actionName);

            var layout = this._service.GetLayout(this._context);
            layout.Should().BeEquivalentTo($"~/Themes/{this._defaultThemeName}/shared/_layout.cshtml");
        }

        private void SetViewContext(string controllerName, string actionName)
        {
            this._routeData.Values.Add("controller", controllerName);
            this._routeData.Values.Add("action", actionName);

            this._context = new ViewContext { RouteData = this._routeData };
        }
    }
}