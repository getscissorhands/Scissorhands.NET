using ScissorHands.Core.Models;

namespace ScissorHands.Web.Tests.Themes;

public class DefaultPageViewTests
{
    [Fact]
    public void Given_TaggedPage_When_Rendered_Then_It_Should_ShowTagLinksBelowTheContent()
    {
        using var context = new BunitContext();
        var document = new ContentDocument
        {
            Kind = ContentKind.Page,
            Metadata = new ContentMetadata { Tags = ["sample", "hierarchy"] },
            Html = "<h1>Parent</h1><p>Page content.</p>",
        };

        var cut = context.Render<PageView>(parameters => parameters.AddCascadingValue(document));

        cut.Find("article > p").TextContent.ShouldBe("Page content.");
        var tags = cut.Find("ul.tag-list");
        tags.GetAttribute("aria-label").ShouldBe("Page tags");
        cut.Find("article").LastElementChild.ShouldBe(tags);
        tags.QuerySelectorAll("a").Select(link => link.TextContent).ShouldBe(["#sample", "#hierarchy"]);
        tags.QuerySelectorAll("a").Select(link => link.GetAttribute("href")).ShouldBe(["tags/sample", "tags/hierarchy"]);
    }

    [Theory]
    [InlineData("C#", "tags/c%23")]
    [InlineData("  Mixed Case  ", "tags/mixed%20case")]
    [InlineData("topic/subtopic", "tags/topic%2Fsubtopic")]
    [InlineData("<img src=x>", "tags/%3Cimg%20src%3Dx%3E")]
    public void Given_TagText_When_Rendered_Then_It_Should_EncodeLabelsAndNormalizeBaseRelativeUrls(string tag, string expectedHref)
    {
        using var context = new BunitContext();
        var document = new ContentDocument
        {
            Kind = ContentKind.Page,
            Metadata = new ContentMetadata { Tags = [tag] },
        };

        var cut = context.Render<PageView>(parameters => parameters.AddCascadingValue(document));

        var link = cut.Find(".tag-list a");
        link.TextContent.ShouldBe($"#{tag}");
        link.GetAttribute("href").ShouldBe(expectedHref);
        cut.FindAll("img").ShouldBeEmpty();
        new Uri(new Uri("https://example.com/"), expectedHref).AbsoluteUri.ShouldBe($"https://example.com/{expectedHref}");
        new Uri(new Uri("https://example.com/site/"), expectedHref).AbsoluteUri.ShouldBe($"https://example.com/site/{expectedHref}");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Given_NoPageTags_When_Rendered_Then_It_Should_OmitTheTagList(bool includeDocument)
    {
        using var context = new BunitContext();
        var cut = context.Render<PageView>(parameters =>
        {
            if (includeDocument)
            {
                parameters.AddCascadingValue(new ContentDocument { Html = "<p>Untagged page.</p>" });
            }
        });

        cut.FindAll(".tag-list").ShouldBeEmpty();
        if (includeDocument)
        {
            cut.Find("article > p").TextContent.ShouldBe("Untagged page.");
        }
    }
}
