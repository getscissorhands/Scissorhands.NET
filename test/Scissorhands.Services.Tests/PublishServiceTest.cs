using System;
using System.Threading.Tasks;

using Aliencube.Scissorhands.Services.Exceptions;
using Aliencube.Scissorhands.Services.Helpers;
using Aliencube.Scissorhands.Services.Tests.Fixtures;

using FluentAssertions;

using Moq;

using Xunit;

namespace Aliencube.Scissorhands.Services.Tests
{
    /// <summary>
    /// This represents the test entity for the <see cref="PublishService"/> class.
    /// </summary>
    public class PublishServiceTest : IClassFixture<PublishServiceFixture>
    {
        private readonly Mock<IFileHelper> _fileHelper;
        private readonly IPublishService _service;

        /// <summary>
        /// Initializes a new instance of the <see cref="PublishServiceTest"/> class.
        /// </summary>
        /// <param name="fixture"><see cref="PublishServiceFixture"/> instance.</param>
        public PublishServiceTest(PublishServiceFixture fixture)
        {
            this._fileHelper = fixture.FileHelper;
            this._service = fixture.PublishService;
        }

        /// <summary>
        /// Tests whether the method should throw an exception or not.
        /// </summary>
        [Fact]
        public void Given_NullMarkdown_ShouldThrow_ArgumentNullException()
        {
            Func<Task> func = async () => { var result = await this._service.PublishMarkdownAsync(null).ConfigureAwait(false); };
            func.ShouldThrow<ArgumentNullException>();
        }

        /// <summary>
        /// Tests whether the method should throw an exception or not.
        /// </summary>
        [Fact]
        public void Given_FalseWritingSync_ShouldThrow_PublishFailedException()
        {
            this._fileHelper.Setup(p => p.WriteAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(false));

            Func<Task> func = async () => { var result = await this._service.PublishMarkdownAsync("**Hello World**").ConfigureAwait(false); };
            func.ShouldThrow<PublishFailedException>();
        }

        /// <summary>
        /// Tests whether the method should return value or not.
        /// </summary>
        /// <param name="markdownpath">File path.</param>
        [Theory]
        [InlineData("~/Posts/markdown.md")]
        public async void Given_Markdown_ShouldReturn_Filepath(string markdownpath)
        {
            this._fileHelper.Setup(p => p.WriteAsync(It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult(true));

            var result = await this._service.PublishMarkdownAsync("**Hello World**").ConfigureAwait(false);
            result.Should().Be(markdownpath);
        }
    }
}