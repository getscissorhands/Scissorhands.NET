using ScissorHands.Core.Validation;

namespace ScissorHands.Core.Tests.Validation;

public class PluginIdValidatorTests
{
    [Theory]
    [InlineData("a")]
    [InlineData("plugin")]
    [InlineData("heading-ids")]
    [InlineData("syntax-highlighting-v2")]
    [InlineData("2fa")]
    [InlineData("123")]
    public void Given_KebabCaseId_When_Validated_Then_It_Should_AcceptTheId(string id)
    {
        Should.NotThrow(() => PluginIdValidator.Validate(id, "Test declaration"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("Plugin")]
    [InlineData("somePlugin")]
    [InlineData("plugin name")]
    [InlineData("plugin_name")]
    [InlineData("plugin--name")]
    [InlineData("-plugin")]
    [InlineData("plugin-")]
    [InlineData("plugin.")]
    [InlineData("plugin/name")]
    [InlineData("plugin\\name")]
    [InlineData("plugin\n")]
    [InlineData("plugin\r\n")]
    [InlineData("plugin\0")]
    [InlineData("caf\u00e9")]
    public void Given_InvalidId_When_Validated_Then_It_Should_ReportAnErrorWithoutNormalizing(string? id)
    {
        var exception = Should.Throw<InvalidOperationException>(() => PluginIdValidator.Validate(id, "Test declaration"));

        exception.Message.ShouldContain("Test declaration");
        exception.Message.ShouldContain("invalid plugin ID");
        exception.Message.ShouldContain("lowercase ASCII kebab-case");
    }
}
