using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.Extensions.PlatformAbstractions;

using Moq;

using Scissorhands.Helpers;
using Scissorhands.Models.Settings;
using Scissorhands.Themes.Tests.Fixtures;

using Xunit;

namespace Scissorhands.Themes.Tests
{
    /// <summary>
    /// This represents the test entity for the <see cref="ThemeLoader"/> class.
    /// </summary>
    public class ThemeLoaderTest : IClassFixture<ThemeLoaderFixture>
    {
        private readonly string _defaultThemeName;
        private readonly string _baseUrl;
        private readonly string _bastPath;
        private readonly List<Author> _authors;
        private readonly List<FeedType> _feedTypes;
        private readonly Mock<WebAppSettings> _settings;
        private readonly Mock<IFileHelper> _fileHelper;
        private readonly Mock<IApplicationEnvironment> _env;
        private readonly IThemeLoader _themeLoader;

        /// <summary>
        /// Initializes a new instance of the <see cref="ThemeLoaderTest"/> class.
        /// </summary>
        /// <param name="fixture"><see cref="ThemeLoaderFixture"/> instance.</param>
        public ThemeLoaderTest(ThemeLoaderFixture fixture)
        {
            this._defaultThemeName = fixture.DefaultThemeName;
            this._baseUrl = fixture.BaseUrl;
            this._bastPath = fixture.BasePath;
            this._authors = fixture.Authors;
            this._feedTypes = fixture.FeedTypes;
            this._settings = fixture.WebAppSettings;
            this._fileHelper = fixture.FileHelper;
            this._env = fixture.ApplicationEnvironment;
            this._themeLoader = fixture.ThemeLoader;
        }

        /// <summary>
        /// Tests whether the constructor should throw an exception or not.
        /// </summary>
        [Fact]
        public void Given_NullParameter_Constructor_ShouldThrow_ArgumentNullException()
        {
            Action action1 = () => { var service = new ThemeLoader(null, this._fileHelper.Object); };
            action1.ShouldThrow<ArgumentNullException>();

            Action action2 = () => { var service = new ThemeLoader(this._settings.Object, null); };
            action2.ShouldThrow<ArgumentNullException>();
        }

        /// <summary>
        /// Tests whether the constructor should throw an exception or not.
        /// </summary>
        [Fact]
        public void Given_Parameter_Constructor_ShouldThrow_NoArgumentNullException()
        {
            Action action = () => { var service = new ThemeLoader(this._settings.Object, this._fileHelper.Object); };
            action.ShouldNotThrow<Exception>();
        }

        /// <summary>
        /// Tests whether LoadAsync should throw an exception or not.
        /// </summary>
        [Fact]
        public void Given_InvalidFilepath_LoadAsync_ShouldThrow_InvalidOperationException()
        {
            var basepath = "test";
            this._env.SetupGet(p => p.ApplicationBasePath).Returns(basepath);

            Func<Task> func = async () => { var result = await this._themeLoader.LoadAsync(this._env.Object).ConfigureAwait(false); };
            func.ShouldThrow<InvalidOperationException>();
        }

        /// <summary>
        /// Tests whether LoadAsync should throw an exception or not.
        /// </summary>
        [Fact]
        public void Given_NullConfig_LoadAsync_ShouldThrow_ArgumentNullException()
        {
            var basepath = string.Empty;
            var json = string.Empty;

            this._env.SetupGet(p => p.ApplicationBasePath).Returns(basepath);
            this._fileHelper.Setup(p => p.ReadAsync(It.IsAny<string>())).Returns(Task.FromResult(json));

            Func<Task> func = async () => { var result = await this._themeLoader.LoadAsync(this._env.Object).ConfigureAwait(false); };
            func.ShouldThrow<ArgumentNullException>();
        }

        /// <summary>
        /// Tests whether LoadAsync should return result or not.
        /// </summary>
        /// <param name="title">Title of theme.</param>
        [Theory]
        [InlineData("Hello World")]
        public async void Given_WebAppSettings_LoadAsync_ShouldReturn_Result(string title)
        {
            var basepath = string.Empty;
            var json = $"{{ \"title\": \"{title}\" }}";

            this._env.SetupGet(p => p.ApplicationBasePath).Returns(basepath);
            this._fileHelper.Setup(p => p.ReadAsync(It.IsAny<string>())).Returns(Task.FromResult(json));

            var config = await this._themeLoader.LoadAsync(this._env.Object).ConfigureAwait(false);
            config.Title.Should().BeEquivalentTo(title);
            config.BaseUrl.Should().BeEquivalentTo(this._baseUrl);
            config.BasePath.Should().BeEquivalentTo(this._bastPath);
            config.Authors.Should().HaveCount(this._authors.Count);
            config.Authors.First().Name.Should().BeEquivalentTo(this._authors.First().Name);
            config.FeedTypes.Count.Should().Be(this._feedTypes.Count);
            config.FeedTypes.First().ShouldBeEquivalentTo(this._feedTypes.First());
        }
    }
}