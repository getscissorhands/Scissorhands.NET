using ScissorHands.Core.Options;

namespace ScissorHands.Core.Tests.Options;

public class CommandOptionsTests
{
    [Theory]
    [InlineData(new[] { "--preview" }, CommandMode.Preview)]
    [InlineData(new[] { "--PREVIEW" }, CommandMode.Preview)]
    [InlineData(new[] { "--build" }, CommandMode.Build)]
    [InlineData(new[] { "--BUILD" }, CommandMode.Build)]
    [InlineData(new[] { "--help" }, CommandMode.Help)]
    [InlineData(new[] { "--HELP" }, CommandMode.Help)]
    [InlineData(new[] { "--unknown" }, CommandMode.Unknown)]
    public void Given_SingleArgument_When_Parse_Invoked_Then_It_Should_ReturnExpectedMode(string[] args, CommandMode expected)
    {
        // Arrange

        // Act
        var result = CommandOptions.Parse(args);

        // Assert
        result.ShouldNotBeNull();
        result.Mode.ShouldBe(expected);
    }

    [Fact]
    public void Given_EmptyArguments_When_Parse_Invoked_Then_It_Should_ReturnUnknownMode()
    {
        // Arrange
        var args = Array.Empty<string>();

        // Act
        var result = CommandOptions.Parse(args);

        // Assert
        result.Mode.ShouldBe(CommandMode.Unknown);
    }

    [Fact]
    public void Given_MultipleArgumentsWithPreviewAndBuild_When_Parse_Invoked_Then_It_Should_PrioritisePreview()
    {
        // Arrange
        var args = new[] { "--build", "--preview" };

        // Act
        var result = CommandOptions.Parse(args);

        // Assert
        result.Mode.ShouldBe(CommandMode.Preview);
    }

    [Fact]
    public void Given_MultipleArgumentsWithBuildAndHelp_When_Parse_Invoked_Then_It_Should_PrioritiseBuild()
    {
        // Arrange
        var args = new[] { "--help", "--build" };

        // Act
        var result = CommandOptions.Parse(args);

        // Assert
        result.Mode.ShouldBe(CommandMode.Build);
    }

    [Fact]
    public void Given_MultipleArgumentsWithHelpOnlyAmongKnownFlags_When_Parse_Invoked_Then_It_Should_ReturnHelp()
    {
        // Arrange
        var args = new[] { "--something", "--help", "--else" };

        // Act
        var result = CommandOptions.Parse(args);

        // Assert
        result.Mode.ShouldBe(CommandMode.Help);
    }
}
