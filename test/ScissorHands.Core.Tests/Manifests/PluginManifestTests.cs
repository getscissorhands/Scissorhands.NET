using ScissorHands.Core.Manifests;

namespace ScissorHands.Core.Tests.Manifests;

public sealed class PluginManifestTests
{
    [Fact]
    public void Given_DefaultPluginManifest_When_Constructed_Then_It_Should_HaveNullOptionalProperties()
    {
        // Arrange

        // Act
        var manifest = new PluginManifest();

        // Assert
        manifest.ShouldNotBeNull();
        manifest.Name.ShouldBeNull();
        manifest.Options.ShouldBeNull();
    }

    [Fact]
    public void Given_PluginManifestWithOptions_When_Constructed_Then_It_Should_RetainOptions()
    {
        // Arrange
        var options = new Dictionary<string, object?>
        {
            ["enabled"] = true,
            ["trackingId"] = "UA-123",
        };

        // Act
        var manifest = new PluginManifest
        {
            Name = "TestPlugin",
            Options = options
        };

        // Assert
        manifest.Name.ShouldBe("TestPlugin");
        manifest.Options.ShouldNotBeNull();
        manifest.Options["enabled"].ShouldBe(true);
        manifest.Options["trackingId"].ShouldBe("UA-123");
    }
}
