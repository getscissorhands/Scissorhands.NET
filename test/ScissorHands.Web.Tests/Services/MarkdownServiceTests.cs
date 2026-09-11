using ScissorHands.Web.Services;

namespace ScissorHands.Web.Tests.Services;

public class MarkdownServiceTests
{
    [Fact]
    public async Task Given_SingleParagraph_When_TrimRequested_Then_It_Should_RemoveOnlyOuterParagraph()
    {
        var service = new MarkdownService();

        var html = await service.ToHtmlAsync("This is **important**.", trim: true, cancellationToken: Xunit.TestContext.Current.CancellationToken);

        html.ShouldBe("This is <strong>important</strong>.");
    }

    [Fact]
    public async Task Given_MultipleParagraphs_When_TrimRequested_Then_It_Should_PreserveParagraphElements()
    {
        var service = new MarkdownService();

        var html = await service.ToHtmlAsync("First paragraph.\n\nSecond paragraph.", trim: true, cancellationToken: Xunit.TestContext.Current.CancellationToken);

        html.ShouldBe("<p>First paragraph.</p>\n<p>Second paragraph.</p>\n");
    }
}
