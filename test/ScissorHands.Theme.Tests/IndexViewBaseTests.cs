namespace ScissorHands.Theme.Tests;

public class IndexViewBaseTests
{
    [Fact]
    public void Given_IndexViewBase_When_Constructed_Then_It_Should_HaveNullDocuments()
    {
        // Arrange

        // Act
        var view = new TestIndexView();

        // Assert
        view.Documents.ShouldBeNull();
    }
}

internal class TestIndexView : IndexViewBase
{
}

