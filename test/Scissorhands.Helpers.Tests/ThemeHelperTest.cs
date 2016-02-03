using System;

using FluentAssertions;

using Moq;

using Scissorhands.Helpers.Tests.Fixtures;
using Scissorhands.Models.Settings;

using Xunit;

namespace Scissorhands.Helpers.Tests
{
    /// <summary>
    /// This represents the test entity for the <see cref="ThemeHelper"/> class.
    /// </summary>
    public class ThemeHelperTest : IClassFixture<ThemeHelperFixture>
    {
        private const string HeadPath = "~/Themes/{0}/shared/_head.cshtml";
        private const string HeaderPath = "~/Themes/{0}/shared/_header.cshtml";
        private const string PostPath = "~/Themes/{0}/post/post.cshtml";
        private const string FooterPath = "~/Themes/{0}/shared/_footer.cshtml";

        private readonly string _defaultThemeName;
        private readonly Mock<ISiteMetadataSettings> _metadata;
        private readonly IThemeHelper _helper;

        /// <summary>
        /// Initializes a new instance of the <see cref="ThemeHelperTest"/> class.
        /// </summary>
        /// <param name="fixture"><see cref="ThemeHelperFixture"/> instance.</param>
        public ThemeHelperTest(ThemeHelperFixture fixture)
        {
            this._defaultThemeName = fixture.DefaultThemeName;
            this._metadata = fixture.SiteMetadataSettings;
            this._helper = fixture.ThemeHelper;
        }

        /// <summary>
        /// Tests whether the constructor should throw an exception or not.
        /// </summary>
        [Fact]
        public void Given_NullParameter_Constructor_ShouldThrow_ArgumentNullException()
        {
            Action action = () => { var helper = new ThemeHelper(null); };
            action.ShouldThrow<ArgumentNullException>();
        }

        /// <summary>
        /// Tests whether the constructor should NOT throw an exception or not.
        /// </summary>
        [Fact]
        public void Given_Parameter_Constructor_ShouldThrow_NoException()
        {
            Action action = () => { var helper = new ThemeHelper(this._metadata.Object); };
            action.ShouldNotThrow<Exception>();
        }

        /// <summary>
        /// Tests whether the GetHeadPartialViewPath method should return result or not.
        /// </summary>
        [Fact]
        public void Given_NullParameter_GetHeadPartialViewPath_ShouldReturn_ResultWith_DefaultTheme()
        {
            var result = this._helper.GetHeadPartialViewPath();
            result.Should().BeEquivalentTo(string.Format(HeadPath, this._defaultThemeName));
        }

        /// <summary>
        /// Tests whether the GetHeadPartialViewPath method should return result or not.
        /// </summary>
        /// <param name="themeName">Theme name.</param>
        [Theory]
        [InlineData("polar-bear")]
        public void Given_ThemeName_GetHeadPartialViewPath_ShouldReturn_ResultWith_ThemeName(string themeName)
        {
            var result = this._helper.GetHeadPartialViewPath(themeName);
            result.Should().BeEquivalentTo(string.Format(HeadPath, themeName));
        }

        /// <summary>
        /// Tests whether the GetHeaderPartialViewPath method should return result or not.
        /// </summary>
        [Fact]
        public void Given_NullParameter_GetHeaderPartialViewPath_ShouldReturn_ResultWith_DefaultTheme()
        {
            var result = this._helper.GetHeaderPartialViewPath();
            result.Should().BeEquivalentTo(string.Format(HeaderPath, this._defaultThemeName));
        }

        /// <summary>
        /// Tests whether the GetHeaderPartialViewPath method should return result or not.
        /// </summary>
        /// <param name="themeName">Theme name.</param>
        [Theory]
        [InlineData("polar-bear")]
        public void Given_ThemeName_GetHeaderPartialViewPath_ShouldReturn_ResultWith_ThemeName(string themeName)
        {
            var result = this._helper.GetHeaderPartialViewPath(themeName);
            result.Should().BeEquivalentTo(string.Format(HeaderPath, themeName));
        }

        /// <summary>
        /// Tests whether the GetPostPartialViewPath method should return result or not.
        /// </summary>
        [Fact]
        public void Given_NullParameter_GetPostPartialViewPath_ShouldReturn_ResultWith_DefaultTheme()
        {
            var result = this._helper.GetPostPartialViewPath();
            result.Should().BeEquivalentTo(string.Format(PostPath, this._defaultThemeName));
        }

        /// <summary>
        /// Tests whether the GetPostPartialViewPath method should return result or not.
        /// </summary>
        /// <param name="themeName">Theme name.</param>
        [Theory]
        [InlineData("polar-bear")]
        public void Given_ThemeName_GetPostPartialViewPath_ShouldReturn_ResultWith_ThemeName(string themeName)
        {
            var result = this._helper.GetPostPartialViewPath(themeName);
            result.Should().BeEquivalentTo(string.Format(PostPath, themeName));
        }

        /// <summary>
        /// Tests whether the GetFooterPartialViewPath method should return result or not.
        /// </summary>
        [Fact]
        public void Given_NullParameter_GetFooterPartialViewPath_ShouldReturn_ResultWith_DefaultTheme()
        {
            var result = this._helper.GetFooterPartialViewPath();
            result.Should().BeEquivalentTo(string.Format(FooterPath, this._defaultThemeName));
        }

        /// <summary>
        /// Tests whether the GetFooterPartialViewPath method should return result or not.
        /// </summary>
        /// <param name="themeName">Theme name.</param>
        [Theory]
        [InlineData("polar-bear")]
        public void Given_ThemeName_GetFooterPartialViewPath_ShouldReturn_ResultWith_ThemeName(string themeName)
        {
            var result = this._helper.GetFooterPartialViewPath(themeName);
            result.Should().BeEquivalentTo(string.Format(FooterPath, themeName));
        }
    }
}