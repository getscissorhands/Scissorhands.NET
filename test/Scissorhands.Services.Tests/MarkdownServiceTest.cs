using Aliencube.Scissorhands.Services.Tests.Fixtures;

using FluentAssertions;

using Xunit;

namespace Aliencube.Scissorhands.Services.Tests
{
    /// <summary>
    /// This represents the test entity for the <see cref="MarkdownService"/> class.
    /// </summary>
    public class MarkdownServiceTest : IClassFixture<MarkdownServiceFixture>
    {
        private readonly IMarkdownService _service;

        /// <summary>
        /// Initializes a new instance of the <see cref="MarkdownServiceTest"/> class.
        /// </summary>
        /// <param name="fixture"><see cref="MarkdownServiceFixture"/> instance.</param>
        public MarkdownServiceTest(MarkdownServiceFixture fixture)
        {
            this._service = fixture.MarkdownService;
        }

        /// <summary>
        /// Tests whether the given file path returns null or not.
        /// </summary>
        /// <param name="filepath">Fully qualified file path.</param>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async void Given_Filepath_Should_Return_Null(string filepath)
        {
            var result = await this._service.ReadAsync(filepath);
            result.Should().BeNull();
        }

        /// <summary>
        /// Tests whether the given file path returns content or not.
        /// </summary>
        [Fact]
        public async void Given_Filepath_Should_Return_Content()
        {
            var filepath = "project.json";
            var result = await this._service.ReadAsync(filepath);
            result.Should().StartWithEquivalent("{");
        }
    }
}