using ScissorHands.Core.Models;

namespace ScissorHands.Web.Tests.Themes;

public class DefaultViewUrlTests
{
    [Theory]
    [InlineData("  Mixed Case  ", "tags/mixed%20case")]
    [InlineData(" C# ", "tags/c%23")]
    [InlineData("Topic/Subtopic", "tags/topic%2Fsubtopic")]
    [InlineData("q?x=1&y=2#z", "tags/q%3Fx%3D1%26y%3D2%23z")]
    [InlineData("<img src=x>", "tags/%3Cimg%20src%3Dx%3E")]
    public void Given_Tag_When_DefaultViewsRendered_Then_It_Should_UseConsistentBaseRelativeUrls(string tag, string expectedHref)
    {
        // Arrange
        using var context = new BunitContext();
        var document = new ContentDocument
        {
            Metadata = new ContentMetadata { Tags = [tag] },
        };
        var taggedDocuments = new Dictionary<string, (IEnumerable<ContentDocument> Posts, IEnumerable<ContentDocument> Pages)>
        {
            [tag] = ([document], []),
        };

        // Act
        var post = context.Render<PostView>(parameters => parameters.AddCascadingValue(document));
        var page = context.Render<PageView>(parameters => parameters.AddCascadingValue(document));
        var tagList = context.Render<TagListView>(parameters => parameters.AddCascadingValue("TaggedDocuments", taggedDocuments));

        // Assert
        foreach (var link in new[] { post.Find(".tag-list a"), page.Find(".tag-list a"), tagList.Find(".tag-card") })
        {
            var href = link.GetAttribute("href");
            href.ShouldBe(expectedHref);
            link.TextContent.ShouldContain($"#{tag}");
            new Uri(new Uri("https://example.com/"), href!).AbsoluteUri.ShouldBe($"https://example.com/{expectedHref}");
            new Uri(new Uri("https://example.com/site/"), href!).AbsoluteUri.ShouldBe($"https://example.com/site/{expectedHref}");
        }

        post.FindAll("img").ShouldBeEmpty();
        page.FindAll("img").ShouldBeEmpty();
        tagList.FindAll("img").ShouldBeEmpty();
    }

    [Theory]
    [InlineData(" ")]
    [InlineData(" . ")]
    [InlineData("..")]
    public void Given_InvalidTag_When_DefaultViewsRendered_Then_It_Should_RejectTheTag(string tag)
    {
        // Arrange
        using var context = new BunitContext();
        var document = new ContentDocument
        {
            Metadata = new ContentMetadata { Tags = [tag] },
        };
        var taggedDocuments = new Dictionary<string, (IEnumerable<ContentDocument> Posts, IEnumerable<ContentDocument> Pages)>
        {
            [tag] = ([document], []),
        };

        // Act / Assert
        Should.Throw<ArgumentException>(() => context.Render<PostView>(parameters => parameters.AddCascadingValue(document)))
            .ParamName.ShouldBe("tag");
        Should.Throw<ArgumentException>(() => context.Render<PageView>(parameters => parameters.AddCascadingValue(document)))
            .ParamName.ShouldBe("tag");
        Should.Throw<ArgumentException>(() => context.Render<TagListView>(parameters => parameters.AddCascadingValue("TaggedDocuments", taggedDocuments)))
            .ParamName.ShouldBe("tag");
    }

    [Theory]
    [InlineData(" /en-us/guides/about & team/ ", "en-us/guides/about%20%26%20team")]
    [InlineData(@"posts\C#\intro", "posts/C%23/intro")]
    [InlineData("posts/already%20encoded", "posts/already%2520encoded")]
    [InlineData("", ".")]
    public void Given_ContentSlug_When_DefaultListsRendered_Then_It_Should_EscapeBaseRelativeUrls(string slug, string expectedHref)
    {
        // Arrange
        using var context = new BunitContext();
        var postDocument = new ContentDocument
        {
            Kind = ContentKind.Post,
            Metadata = new ContentMetadata { Slug = slug, Title = "Post" },
        };
        var pageDocument = new ContentDocument
        {
            Kind = ContentKind.Page,
            Metadata = new ContentMetadata { Slug = slug, Title = "Page" },
        };

        // Act
        var index = context.Render<IndexView>(parameters => parameters
            .AddCascadingValue<IEnumerable<ContentDocument>>([postDocument]));
        var tag = context.Render<TagView>(parameters => parameters
            .AddCascadingValue<IEnumerable<ContentDocument>>("TaggedPosts", [postDocument])
            .AddCascadingValue<IEnumerable<ContentDocument>>("TaggedPages", [pageDocument]));

        // Assert
        foreach (var link in new[] { index.Find(".post-link"), tag.Find(".post-link"), tag.Find(".page-list a") })
        {
            var href = link.GetAttribute("href");
            href.ShouldBe(expectedHref);
            var expectedPath = expectedHref == "." ? string.Empty : expectedHref;
            new Uri(new Uri("https://example.com/site/"), href!).AbsoluteUri.ShouldBe($"https://example.com/site/{expectedPath}");
        }
    }

    [Theory]
    [InlineData("../outside")]
    [InlineData("parent/./child")]
    public void Given_RelativePathSegment_When_DefaultListsRendered_Then_It_Should_RejectTheSlug(string slug)
    {
        // Arrange
        using var context = new BunitContext();
        var document = new ContentDocument
        {
            Metadata = new ContentMetadata { Slug = slug },
        };

        // Act / Assert
        Should.Throw<ArgumentException>(() => context.Render<IndexView>(parameters => parameters
            .AddCascadingValue<IEnumerable<ContentDocument>>([document]))).ParamName.ShouldBe("slug");
        Should.Throw<ArgumentException>(() => context.Render<TagView>(parameters => parameters
            .AddCascadingValue<IEnumerable<ContentDocument>>("TaggedPosts", [document]))).ParamName.ShouldBe("slug");
        Should.Throw<ArgumentException>(() => context.Render<TagView>(parameters => parameters
            .AddCascadingValue<IEnumerable<ContentDocument>>("TaggedPages", [document]))).ParamName.ShouldBe("slug");
    }

    [Theory]
    [InlineData("images/hero.png", "images/hero.png", "https://example.com/site/images/hero.png")]
    [InlineData("///images/hero%20image.png?v=1&size=2#focus", "images/hero%20image.png?v=1&size=2#focus", "https://example.com/site/images/hero%20image.png?v=1&size=2#focus")]
    [InlineData("https://cdn.example.com/hero%20image.png?v=1&size=2#focus", "https://cdn.example.com/hero%20image.png?v=1&size=2#focus", "https://cdn.example.com/hero%20image.png?v=1&size=2#focus")]
    [InlineData("http://cdn.example.com/hero%20image.png?v=1#focus", "http://cdn.example.com/hero%20image.png?v=1#focus", "http://cdn.example.com/hero%20image.png?v=1#focus")]
    public void Given_HeroImage_When_PostRendered_Then_It_Should_PreserveImageUrlSemantics(
        string imagePath, string expectedSrc, string expectedResolvedUrl)
    {
        // Arrange
        using var context = new BunitContext();
        var document = new ContentDocument
        {
            Metadata = new ContentMetadata { HeroImage = imagePath },
        };

        // Act
        var cut = context.Render<PostView>(parameters => parameters.AddCascadingValue(document));

        // Assert
        var image = cut.Find("img.hero-image");
        var src = image.GetAttribute("src");
        src.ShouldBe(expectedSrc);
        image.GetAttribute("alt").ShouldBe(string.Empty);
        new Uri(new Uri("https://example.com/site/"), src!).AbsoluteUri.ShouldBe(expectedResolvedUrl);
    }
}
