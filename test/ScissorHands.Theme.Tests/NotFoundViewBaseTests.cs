namespace ScissorHands.Theme.Tests;

public class NotFoundViewBaseTests
{
    [Fact]
    public void Given_NotFoundViewBase_When_Constructed_Then_It_Should_HaveNonNullDocument()
    {
        // Arrange

        // Act
        var view = new TestNotFoundView();

        // Assert
        view.Document.ShouldNotBeNull();
    }
}

internal class TestNotFoundView : NotFoundViewBase
{
}
