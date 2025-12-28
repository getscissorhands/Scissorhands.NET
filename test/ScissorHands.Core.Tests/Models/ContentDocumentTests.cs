namespace ScissorHands.Core.Tests.Models;

using ScissorHands.Core.Models;

public sealed class ContentDocumentTests
{
    [Fact]
    public void Given_DefaultContentDocument_When_Constructed_Then_It_Should_HaveNonNullDefaults()
    {
        // Arrange

        // Act
        var document = new ContentDocument();

        // Assert
        document.ShouldNotBeNull();
        document.SourcePath.ShouldNotBeNull();
        document.Markdown.ShouldNotBeNull();
        document.Html.ShouldNotBeNull();

        document.Metadata.ShouldNotBeNull();
        document.Metadata.Title.ShouldNotBeNull();
        document.Metadata.Slug.ShouldNotBeNull();
        document.Metadata.Tags.ShouldNotBeNull();
        document.Metadata.Tags.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(ContentKind.Undefined)]
    [InlineData(ContentKind.Post)]
    [InlineData(ContentKind.Page)]
    public void Given_ContentKind_When_Assigned_Then_It_Should_RoundTrip(ContentKind kind)
    {
        // Arrange
        var document = new ContentDocument
        {
            Kind = kind
        };

        // Act
        var actual = document.Kind;

        // Assert
        actual.ShouldBe(kind);
    }
}
