namespace ScissorHands.Theme.Tests;

public class PostViewBaseTests
{
    [Fact]
    public void Given_PostViewBase_When_Constructed_Then_It_Should_HaveNonNullDocument()
    {
        // Arrange

        // Act
        var view = new TestPostView();

        // Assert
        view.Document.ShouldNotBeNull();
    }
}

internal class TestPostView : PostViewBase
{
}
