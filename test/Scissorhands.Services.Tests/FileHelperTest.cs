using Aliencube.Scissorhands.Services.Helpers;
using Aliencube.Scissorhands.Services.Tests.Fixtures;

using FluentAssertions;

using Xunit;

namespace Aliencube.Scissorhands.Services.Tests
{
    /// <summary>
    /// This represents the test entity for the <see cref="FileHelper"/> class.
    /// </summary>
    public class FileHelperTest : IClassFixture<FileHelperFixture>
    {
        private readonly IFileHelper _helper;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileHelperTest"/> class.
        /// </summary>
        /// <param name="fixture"><see cref="FileHelperFixture"/> instance.</param>
        public FileHelperTest(FileHelperFixture fixture)
        {
            this._helper = fixture.FileHelper;
        }

        /// <summary>
        /// Tests whether the given file path returns null or not.
        /// </summary>
        /// <param name="filepath">Fully qualified file path.</param>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async void Given_Filepath_Should_Return_NullRead(string filepath)
        {
            var result = await this._helper.ReadAsync(filepath).ConfigureAwait(false);
            result.Should().BeNull();
        }

        /// <summary>
        /// Tests whether the given file path returns content or not.
        /// </summary>
        [Fact]
        public async void Given_Filepath_Should_Return_Content()
        {
            var filepath = "project.json";
            var result = await this._helper.ReadAsync(filepath).ConfigureAwait(false);
            result.Should().StartWithEquivalent("{");
        }
    }
}