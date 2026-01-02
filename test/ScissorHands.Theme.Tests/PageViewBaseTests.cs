namespace ScissorHands.Theme.Tests;

public class PageViewBaseTests
{
    [Fact]
    public void Given_PageViewBase_When_Constructed_Then_It_Should_HaveNullDocument()
    {
        // Arrange

        // Act
        var view = new TestPageView();

        // Assert
        view.Document.ShouldBeNull();
    }
}

internal class TestPageView : PageViewBase
{
}

