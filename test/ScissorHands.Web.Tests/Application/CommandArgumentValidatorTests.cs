using ScissorHands.Core.Options;
using ScissorHands.Web.Application;

namespace ScissorHands.Web.Tests.Application;

public class CommandArgumentValidatorTests
{
    [Theory]
    [InlineData(new[] { "--help" }, CommandMode.Help, true, false)]
    [InlineData(new[] { "--build" }, CommandMode.Build, false, false)]
    [InlineData(new[] { "--preview" }, CommandMode.Preview, false, false)]
    [InlineData(new string[0], CommandMode.Unknown, false, true)]
    public void Given_Args_When_Validate_Invoked_Then_It_Should_ReturnExpectedResult(string[] args, CommandMode expectedMode, bool expectedIsHelp, bool expectedIsError)
    {
        // Arrange

        // Act
        var result = CommandArgumentValidator.Validate(args);

        // Assert
        result.ShouldNotBeNull();
        result.Mode.ShouldBe(expectedMode);
        result.IsHelp.ShouldBe(expectedIsHelp);
        result.IsError.ShouldBe(expectedIsError);
    }
}
