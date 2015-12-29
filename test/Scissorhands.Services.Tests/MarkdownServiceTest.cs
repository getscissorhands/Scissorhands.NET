using System;
using System.Threading.Tasks;

using Aliencube.Scissorhands.Services.Helpers;
using Aliencube.Scissorhands.Services.Tests.Fixtures;

using FluentAssertions;

using Moq;

using Xunit;

namespace Aliencube.Scissorhands.Services.Tests
{
    /// <summary>
    /// This represents the test entity for the <see cref="MarkdownService"/> class.
    /// </summary>
    public class MarkdownServiceTest : IClassFixture<MarkdownServiceFixture>
    {
        private readonly Mock<IFileHelper> _helper;
        private readonly IMarkdownService _service;

        private string _markdown;

        /// <summary>
        /// Initializes a new instance of the <see cref="MarkdownServiceTest"/> class.
        /// </summary>
        /// <param name="fixture"><see cref="MarkdownServiceFixture"/> instance.</param>
        public MarkdownServiceTest(MarkdownServiceFixture fixture)
        {
            this._helper = fixture.FileHelper;
            this._service = fixture.MarkdownService;
        }

        /// <summary>
        /// Tests whether constructor should throw an exception or not.
        /// </summary>
        [Fact]
        public void Given_NullParameter_Constructor_ShouldThrow_ArgumentNullException()
        {
            Action action = () => { var service = new MarkdownService(null); };
            action.ShouldThrow<ArgumentNullException>();
        }

        /// <summary>
        /// Tests whether constructor should NOT throw an exception or not.
        /// </summary>
        [Fact]
        public void Given_Parameters_Constructor_ShouldThrow_NoException()
        {
            Action action = () => { var service = new MarkdownService(this._helper.Object); };
            action.ShouldNotThrow<Exception>();
        }

        /// <summary>
        /// Tests whether the given markdown returns null or not.
        /// </summary>
        /// <param name="markdown">Markdown string.</param>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void Given_Markdown_Parse_ShouldReturn_NullConverted(string markdown)
        {
            var parsed = this._service.Parse(markdown);
            parsed.Should().BeNull();
        }

        /// <summary>
        /// Tests whether the given markdown returns parsed HTML or not.
        /// </summary>
        [Fact]
        public void Given_Markdown_Parse_Should_Return_ParsedHtml()
        {
            this._markdown = "**Hello World**";
            var parsed = this._service.Parse(this._markdown);
            parsed.Should().ContainEquivalentOf("<strong>Hello World</strong>");
        }

        /// <summary>
        /// Tests whether the given file path returns null or not.
        /// </summary>
        /// <param name="filepath">Fully qualified file path.</param>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async void Given_NullOrEmptyFilepath_ConvertAsync_ShouldReturn_NullConverted(string filepath)
        {
            var converted = await this._service.ConvertAsync(filepath).ConfigureAwait(false);
            converted.Should().BeNull();
        }

        /// <summary>
        /// Tests whether the given file path returns null or not.
        /// </summary>
        /// <param name="filepath">Fully qualified file path.</param>
        [Theory]
        [InlineData("blank.md")]
        public async void Given_Filepath_ConvertAsync_ShouldReturn_NullConverted(string filepath)
        {
            this._markdown = null;
            this._helper.Setup(p => p.ReadAsync(It.IsAny<string>())).Returns(Task.FromResult(this._markdown));
            var converted = await this._service.ConvertAsync(filepath).ConfigureAwait(false);
            converted.Should().BeNull();
        }

        /// <summary>
        /// Tests whether the given markdown returns parsed HTML or not.
        /// </summary>
        /// <param name="filepath">Fully qualified file path.</param>
        [Theory]
        [InlineData("hello-world.md")]
        public async void Given_Filepath_ConvertAsync_ShouldReturn_ParsedHtml(string filepath)
        {
            this._markdown = "**Hello World**";
            this._helper.Setup(p => p.ReadAsync(It.IsAny<string>())).Returns(Task.FromResult(this._markdown));
            var converted = await this._service.ConvertAsync(filepath).ConfigureAwait(false);
            converted.Should().ContainEquivalentOf("<strong>Hello World</strong>");
        }
    }
}