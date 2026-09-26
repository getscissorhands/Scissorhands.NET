using ScissorHands.Core.Manifests;

namespace ScissorHands.Core.Tests.Manifests;

public class SiteManifestTimeZoneTests
{
    [Fact]
    public void Given_OmittedTimeZone_When_SiteManifest_Invoked_Then_It_Should_DefaultToUtc()
    {
        // Arrange

        // Act
        var manifest = new SiteManifest();

        // Assert
        manifest.TimeZone.ShouldBe("UTC");
    }

    [Theory]
    [InlineData("UTC")]
    [InlineData("Asia/Seoul")]
    [InlineData("America/New_York")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("Unknown/Zone")]
    public void Given_ExplicitTimeZone_When_SiteManifest_Invoked_Then_It_Should_PreserveValueForEngineValidation(string? timeZone)
    {
        // Arrange

        // Act
        var manifest = new SiteManifest { TimeZone = timeZone! };

        // Assert
        manifest.TimeZone.ShouldBe(timeZone);
    }
}
