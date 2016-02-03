using System;
using System.Threading.Tasks;

using FluentAssertions;

using Moq;

using Scissorhands.Helpers;
using Scissorhands.Services.Tests.Fixtures;

using Xunit;

namespace Scissorhands.Services.Tests
{
    /// <summary>
    /// This represents the test entity for the <see cref="MarkdownService"/> class.
    /// </summary>
    public class MarkdownServiceTest : IClassFixture<MarkdownServiceFixture>
    {
        private readonly Mock<IMarkdownHelper> _markdownHelper;
        private readonly Mock<IFileHelper> _fileHelper;
        private readonly IMarkdownService _service;

        private string _markdown;
        private string _parsed;

        /// <summary>
        /// Initializes a new instance of the <see cref="MarkdownServiceTest"/> class.
        /// </summary>
        /// <param name="fixture"><see cref="MarkdownServiceFixture"/> instance.</param>
        public MarkdownServiceTest(MarkdownServiceFixture fixture)
        {
            this._markdownHelper = fixture.MarkdownHelper;
            this._fileHelper = fixture.FileHelper;
            this._service = fixture.MarkdownService;
        }

        /// <summary>
        /// Tests whether constructor should throw an exception or not.
        /// </summary>
        [Fact]
        public void Given_NullParameter_Constructor_ShouldThrow_ArgumentNullException()
        {
            Action action1 = () => { var service = new MarkdownService(null, this._fileHelper.Object); };
            action1.ShouldThrow<ArgumentNullException>();

            Action action2 = () => { var service = new MarkdownService(this._markdownHelper.Object, null); };
            action2.ShouldThrow<ArgumentNullException>();
        }

        /// <summary>
        /// Tests whether constructor should NOT throw an exception or not.
        /// </summary>
        [Fact]
        public void Given_Parameters_Constructor_ShouldThrow_NoException()
        {
            Action action = () => { var service = new MarkdownService(this._markdownHelper.Object, this._fileHelper.Object); };
            action.ShouldNotThrow<Exception>();
        }

        /// <summary>
        /// Tests whether the Parse method should return <c>null</c> or not.
        /// </summary>
        /// <param name="markdown">Markdown value.</param>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void Given_NullParameter_Parse_ShouldReturn_Null(string markdown)
        {
            var result = this._service.Parse(markdown);
            result.Should().BeNullOrWhiteSpace();
        }

        /// <summary>
        /// Tests whether the Parse method should return parsed HTML or not.
        /// </summary>
        /// <param name="markdown">Markdown value.</param>
        /// <param name="html">HTML value.</param>
        [Theory]
        [InlineData("**Hello World**", "<strong>Hello World</strong>")]
        public void Given_NullParameter_Parse_ShouldReturn_Html(string markdown, string html)
        {
            this._markdownHelper.Setup(p => p.Parse(It.IsAny<string>())).Returns(html);

            var result = this._service.Parse(markdown);
            result.Should().BeEquivalentTo(html);
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
            this._fileHelper.Setup(p => p.ReadAsync(It.IsAny<string>())).Returns(Task.FromResult(this._markdown));
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
            this._parsed = "<strong>Hello World</strong>";

            this._fileHelper.Setup(p => p.ReadAsync(It.IsAny<string>())).Returns(Task.FromResult(this._markdown));
            this._markdownHelper.Setup(p => p.Parse(It.IsAny<string>())).Returns(this._parsed);

            var converted = await this._service.ConvertAsync(filepath).ConfigureAwait(false);
            converted.Should().ContainEquivalentOf(this._parsed);
        }
    }
}