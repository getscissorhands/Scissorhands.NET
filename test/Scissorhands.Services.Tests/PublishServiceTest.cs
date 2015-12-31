using System;
using System.IO;
using System.Threading.Tasks;

using Aliencube.Scissorhands.Models.Settings;
using Aliencube.Scissorhands.Services.Exceptions;
using Aliencube.Scissorhands.Services.Helpers;
using Aliencube.Scissorhands.Services.Tests.Fixtures;

using FluentAssertions;

using Microsoft.Extensions.PlatformAbstractions;

using Moq;

using Xunit;

namespace Aliencube.Scissorhands.Services.Tests
{
    /// <summary>
    /// This represents the test entity for the <see cref="PublishService"/> class.
    /// </summary>
    public class PublishServiceTest : IClassFixture<PublishServiceFixture>
    {
        private readonly Mock<WebAppSettings> _settings;
        private readonly Mock<IFileHelper> _fileHelper;
        private readonly IPublishService _service;

        private readonly Mock<IApplicationEnvironment> _env;
        private readonly string _filepath;

        /// <summary>
        /// Initializes a new instance of the <see cref="PublishServiceTest"/> class.
        /// </summary>
        /// <param name="fixture"><see cref="PublishServiceFixture"/> instance.</param>
        public PublishServiceTest(PublishServiceFixture fixture)
        {
            this._settings = fixture.WebAppSettings;
            this._fileHelper = fixture.FileHelper;
            this._service = fixture.PublishService;

            this._env = new Mock<IApplicationEnvironment>();
            this._filepath = "/home/scissorhands.net/wwwroot/posts".Replace('/', Path.DirectorySeparatorChar);
        }

        /// <summary>
        /// Tests whether constructor should throw an exception or not.
        /// </summary>
        [Fact]
        public void Given_NullParameter_Constructor_ShouldThrow_ArgumentNullException()
        {
            Action action1 = () => { var service = new PublishService(null, this._fileHelper.Object); };
            action1.ShouldThrow<ArgumentNullException>();

            Action action2 = () => { var service = new PublishService(this._settings.Object, null); };
            action2.ShouldThrow<ArgumentNullException>();
        }

        /// <summary>
        /// Tests whether constructor should NOT throw an exception or not.
        /// </summary>
        [Fact]
        public void Given_Parameters_Constructor_ShouldThrow_NoException()
        {
            Action action = () => { var service = new PublishService(this._settings.Object, this._fileHelper.Object); };
            action.ShouldNotThrow<Exception>();
        }

        /// <summary>
        /// Tests whether the method should throw an exception or not.
        /// </summary>
        [Fact]
        public void Given_NullMarkdown_PublishMarkdownAsync_ShouldThrow_ArgumentNullException()
        {
            Func<Task> func = async () => { var result = await this._service.PublishMarkdownAsync(null, this._env.Object).ConfigureAwait(false); };
            func.ShouldThrow<ArgumentNullException>();
        }

        /// <summary>
        /// Tests whether the method should throw an exception or not.
        /// </summary>
        [Fact]
        public void Given_FalseWritingSync_PublishMarkdownAsync_ShouldThrow_PublishFailedException()
        {
            this._fileHelper.Setup(p => p.GetDirectory(It.IsAny<IApplicationEnvironment>(), It.IsAny<string>())).Returns(this._filepath);
            this._fileHelper.Setup(p => p.WriteAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(false));

            Func<Task> func = async () => { var result = await this._service.PublishMarkdownAsync("**Hello World**", this._env.Object).ConfigureAwait(false); };
            func.ShouldThrow<PublishFailedException>();
        }

        /// <summary>
        /// Tests whether the method should return value or not.
        /// </summary>
        /// <param name="markdownpath">File path.</param>
        [Theory]
        [InlineData("/posts/markdown.md")]
        public async void Given_Markdown_PublishMarkdownAsync_ShouldReturn_Filepath(string markdownpath)
        {
            this._fileHelper.Setup(p => p.GetDirectory(It.IsAny<IApplicationEnvironment>(), It.IsAny<string>())).Returns(this._filepath);
            this._fileHelper.Setup(p => p.WriteAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(true));

            var result = await this._service.PublishMarkdownAsync("**Hello World**", this._env.Object).ConfigureAwait(false);
            result.Should().Be(markdownpath);
        }

        /// <summary>
        /// Tests whether the method should throw an exception or not.
        /// </summary>
        [Fact]
        public void Given_NullHtml_PublishPostAsync_ShouldThrow_ArgumentNullException()
        {
            Func<Task> func = async () => { var result = await this._service.PublishPostAsync(null, this._env.Object).ConfigureAwait(false); };
            func.ShouldThrow<ArgumentNullException>();
        }

        /// <summary>
        /// Tests whether the method should throw an exception or not.
        /// </summary>
        [Fact]
        public void Given_FalseWritingSync_PublishPostAsync_ShouldThrow_PublishFailedException()
        {
            this._fileHelper.Setup(p => p.GetDirectory(It.IsAny<IApplicationEnvironment>(), It.IsAny<string>())).Returns(this._filepath);
            this._fileHelper.Setup(p => p.WriteAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(false));

            Func<Task> func = async () => { var result = await this._service.PublishPostAsync("**Hello World**", this._env.Object).ConfigureAwait(false); };
            func.ShouldThrow<PublishFailedException>();
        }

        /// <summary>
        /// Tests whether the method should return value or not.
        /// </summary>
        /// <param name="htmlpath">File path.</param>
        [Theory]
        [InlineData("/posts/post.html")]
        public async void Given_Markdown_PublishPostAsync_ShouldReturn_Filepath(string htmlpath)
        {
            this._fileHelper.Setup(p => p.GetDirectory(It.IsAny<IApplicationEnvironment>(), It.IsAny<string>())).Returns(this._filepath);
            this._fileHelper.Setup(p => p.WriteAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(true));

            var result = await this._service.PublishPostAsync("<strong>Hello World</strong>", this._env.Object).ConfigureAwait(false);
            result.Should().Be(htmlpath);
        }
    }
}