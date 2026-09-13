using ScissorHands.Core.Manifests;

namespace ScissorHands.Theme.Tests;

public class UrlHelperTests
{
    [Theory]
    [InlineData(" /en-us/about & team/ ", "en-us/about%20%26%20team")]
    [InlineData(@"posts\C#", "posts/C%23")]
    [InlineData("", ".")]
    public void Given_ContentSlug_When_ViewHelpersInvoked_Then_It_Should_UseSharedEscaping(string slug, string expected)
    {
        new UrlLayout().ContentUrl(slug).ShouldBe(expected);
        new UrlIndexView().ContentUrl(slug).ShouldBe(expected);
        new UrlTagView().ContentUrl(slug).ShouldBe(expected);
    }

    [Theory]
    [InlineData("../child")]
    [InlineData(@"parent\.\child")]
    public void Given_RelativePathSegment_When_ViewHelpersInvoked_Then_It_Should_RejectTheSlug(string slug)
    {
        Should.Throw<ArgumentException>(() => new UrlLayout().ContentUrl(slug)).ParamName.ShouldBe("slug");
        Should.Throw<ArgumentException>(() => new UrlIndexView().ContentUrl(slug)).ParamName.ShouldBe("slug");
        Should.Throw<ArgumentException>(() => new UrlTagView().ContentUrl(slug)).ParamName.ShouldBe("slug");
    }

    [Fact]
    public void Given_NullSlug_When_ViewHelpersInvoked_Then_It_Should_ThrowArgumentNullException()
    {
        Should.Throw<ArgumentNullException>(() => new UrlLayout().ContentUrl(null!)).ParamName.ShouldBe("slug");
        Should.Throw<ArgumentNullException>(() => new UrlIndexView().ContentUrl(null!)).ParamName.ShouldBe("slug");
        Should.Throw<ArgumentNullException>(() => new UrlTagView().ContentUrl(null!)).ParamName.ShouldBe("slug");
    }

    [Theory]
    [InlineData("  C#  ", "tags/c%23")]
    [InlineData(" Topic/Subtopic ", "tags/topic%2Fsubtopic")]
    public void Given_Tag_When_ViewHelpersInvoked_Then_It_Should_UseSharedNormalization(string tag, string expected)
    {
        new UrlPostView().TagUrl(tag).ShouldBe(expected);
        new UrlPageView().TagUrl(tag).ShouldBe(expected);
        new UrlTagListView().TagUrl(tag).ShouldBe(expected);
    }

    [Theory]
    [InlineData(" ")]
    [InlineData(" . ")]
    [InlineData("..")]
    public void Given_InvalidTag_When_ViewHelpersInvoked_Then_It_Should_RejectTheTag(string tag)
    {
        Should.Throw<ArgumentException>(() => new UrlPostView().TagUrl(tag)).ParamName.ShouldBe("tag");
        Should.Throw<ArgumentException>(() => new UrlPageView().TagUrl(tag)).ParamName.ShouldBe("tag");
        Should.Throw<ArgumentException>(() => new UrlTagListView().TagUrl(tag)).ParamName.ShouldBe("tag");
    }

    [Fact]
    public void Given_NullTag_When_ViewHelpersInvoked_Then_It_Should_ThrowArgumentNullException()
    {
        Should.Throw<ArgumentNullException>(() => new UrlPostView().TagUrl(null!)).ParamName.ShouldBe("tag");
        Should.Throw<ArgumentNullException>(() => new UrlPageView().TagUrl(null!)).ParamName.ShouldBe("tag");
        Should.Throw<ArgumentNullException>(() => new UrlTagListView().TagUrl(null!)).ParamName.ShouldBe("tag");
    }

    [Theory]
    [InlineData("/images/hero%20image.png?size=2#focus", "images/hero%20image.png?size=2#focus")]
    [InlineData("https://cdn.example.com/hero%20image.png?size=2#focus", "https://cdn.example.com/hero%20image.png?size=2#focus")]
    public void Given_ImagePath_When_PostHelperInvoked_Then_It_Should_PreserveUrlParts(string path, string expected)
    {
        new UrlPostView().ImageUrl(path).ShouldBe(expected);
    }

    [Fact]
    public void Given_NullImagePath_When_PostHelperInvoked_Then_It_Should_ThrowArgumentNullException()
    {
        Should.Throw<ArgumentNullException>(() => new UrlPostView().ImageUrl(null!)).ParamName.ShouldBe("path");
    }

    [Fact]
    public void Given_ThemeAsset_When_LayoutHelperInvoked_Then_It_Should_PreserveConcatenation()
    {
        // Arrange
        var layout = new UrlLayout(new ThemeManifest { Slug = "//minimal//" });

        // Act
        var result = layout.ThemeUrl("//assets/theme%20dark.css?v=1#dark");

        // Assert
        result.ShouldBe("themes/minimal/assets/theme%20dark.css?v=1#dark");
    }

    [Fact]
    public void Given_MissingTheme_When_LayoutHelperInvoked_Then_It_Should_PreserveTheException()
    {
        var exception = Should.Throw<InvalidOperationException>(() => new UrlLayout().ThemeUrl("assets/theme.css"));

        exception.Message.ShouldBe("A theme must be supplied before getting a theme URL.");
    }

    [Fact]
    public void Given_NullPathAndMissingTheme_When_LayoutHelperInvoked_Then_It_Should_ValidatePathFirst()
    {
        var exception = Should.Throw<ArgumentNullException>(() => new UrlLayout().ThemeUrl(null!));

        exception.ParamName.ShouldBe("path");
    }

    private sealed class UrlLayout : MainLayoutBase
    {
        public UrlLayout(ThemeManifest? theme = null)
        {
            Theme = theme;
        }

        public string ContentUrl(string slug) => GetContentUrl(slug);

        public string ThemeUrl(string path) => GetThemeUrl(path);
    }

    private sealed class UrlIndexView : IndexViewBase
    {
        public string ContentUrl(string slug) => GetContentUrl(slug);
    }

    private sealed class UrlTagView : TagViewBase
    {
        public string ContentUrl(string slug) => GetContentUrl(slug);
    }

    private sealed class UrlPostView : PostViewBase
    {
        public string TagUrl(string tag) => GetTagUrl(tag);

        public string ImageUrl(string path) => GetImageUrl(path);
    }

    private sealed class UrlPageView : PageViewBase
    {
        public string TagUrl(string tag) => GetTagUrl(tag);
    }

    private sealed class UrlTagListView : TagListViewBase
    {
        public string TagUrl(string tag) => GetTagUrl(tag);
    }
}
