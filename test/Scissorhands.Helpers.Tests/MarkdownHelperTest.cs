using FluentAssertions;

using Scissorhands.Helpers.Tests.Fixtures;

using Xunit;

namespace Scissorhands.Helpers.Tests
{
    /// <summary>
    /// This represents the test entity for the <see cref="MarkdownHelper"/> class.
    /// </summary>
    public class MarkdownHelperTest : IClassFixture<MarkdownHelperFixture>
    {
        private readonly IMarkdownHelper _helper;

        private string _markdown;

        /// <summary>
        /// Initializes a new instance of the <see cref="MarkdownHelperTest"/> class.
        /// </summary>
        /// <param name="fixture"><see cref="MarkdownHelperFixture"/> instance.</param>
        public MarkdownHelperTest(MarkdownHelperFixture fixture)
        {
            this._helper = fixture.MarkdownHelper;
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
            var parsed = this._helper.Parse(markdown);
            parsed.Should().BeNull();
        }

        /// <summary>
        /// Tests whether the given markdown returns parsed HTML or not.
        /// </summary>
        [Fact]
        public void Given_Markdown_Parse_Should_Return_ParsedHtml()
        {
            this._markdown = "**Hello World**";
            var parsed = this._helper.Parse(this._markdown);
            parsed.Should().ContainEquivalentOf("<strong>Hello World</strong>");
        }
    }
}