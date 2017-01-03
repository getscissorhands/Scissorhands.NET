using FluentAssertions;

using Scissorhands.ConsoleApp.Tests.Fixtures;

using Xunit;

namespace Scissorhands.ConsoleApp.Tests
{
    /// <summary>
    /// This represents the test entity for the <see cref="CommandOptionsBuilder"/> class.
    /// </summary>
    public class CommandOptionsBuilderTest : IClassFixture<CommandOptionsBuilderFixture>
    {
        private readonly ICommandOptionsBuilder _builder;

        /// <summary>
        /// Initialises a new instance of the <see cref="CommandOptionsBuilderTest"/> class.
        /// </summary>
        /// <param name="fixture"><see cref="CommandOptionsBuilderFixture"/> instance.</param>
        public CommandOptionsBuilderTest(CommandOptionsBuilderFixture fixture)
        {
            this._builder = fixture.CommandOptionsBuilder;
        }

        /// <summary>
        /// Tests whether the given method should return empty result or not.
        /// </summary>
        [Fact]
        public async void Given_NullParameter_GetDefaultOptionsAsyc_ShouldReturn_EmptyOptions()
        {
            var options = await this._builder.GetDefaultOptionsAsyc(null).ConfigureAwait(false);

            options.Should().NotBeNull();
            options.Source.Should().BeNullOrWhiteSpace();
            options.Theme.Should().BeNullOrWhiteSpace();
            options.OutputDirectory.Should().BeNullOrWhiteSpace();
        }

        /// <summary>
        /// Tests whether the given method should return empty result or not.
        /// </summary>
        /// <param name="filepath">File path of <c>appsettings.json</c>.</param>
        [Theory]
        [InlineData("config.json")]
        public async void Given_InvalidParameter_GetDefaultOptionsAsyc_ShouldReturn_EmptyOptions(string filepath)
        {
            var options = await this._builder.GetDefaultOptionsAsyc(filepath).ConfigureAwait(false);

            options.Should().NotBeNull();
            options.Source.Should().BeNullOrWhiteSpace();
            options.Theme.Should().BeNullOrWhiteSpace();
            options.OutputDirectory.Should().BeNullOrWhiteSpace();
        }

        /// <summary>
        /// Tests whether the given method should return empty result or not.
        /// </summary>
        /// <param name="filepath">File path of <c>appsettings.json</c>.</param>
        /// <param name="source">Source directory value.</param>
        /// <param name="theme">Theme name.</param>
        /// <param name="outputDirectory">Output directory value.</param>
        [Theory]
        [InlineData("appsettings.json", "markdowns", "basic", "posts")]
        public async void Given_ValidParameter_GetDefaultOptionsAsyc_ShouldReturn_Options(string filepath, string source, string theme, string outputDirectory)
        {
            var options = await this._builder.GetDefaultOptionsAsyc(filepath).ConfigureAwait(false);

            options.Should().NotBeNull();
            options.Source.Should().BeEquivalentTo(source);
            options.Theme.Should().BeEquivalentTo(theme);
            options.OutputDirectory.Should().BeEquivalentTo(outputDirectory);
        }
    }
}