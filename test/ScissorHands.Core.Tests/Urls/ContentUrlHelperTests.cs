using ScissorHands.Core.Urls;

namespace ScissorHands.Core.Tests.Urls;

public class ContentUrlHelperTests
{
    [Theory]
    [InlineData("", ".")]
    [InlineData(" \t ", ".")]
    [InlineData(" /\\// ", ".")]
    [InlineData("  /en-US/guides/about & team/  ", "en-US/guides/about%20%26%20team")]
    [InlineData(@"guides\\C#\intro", "guides/C%23/intro")]
    [InlineData("nested//a?b#c", "nested/a%3Fb%23c")]
    [InlineData("café/한글", "caf%C3%A9/%ED%95%9C%EA%B8%80")]
    [InlineData("already%20encoded/%2e%2e", "already%2520encoded/%252e%252e")]
    [InlineData("parent/ child /file.html", "parent/%20child%20/file.html")]
    public void Given_ContentSlug_When_GetContentUrl_Invoked_Then_It_Should_EscapeEachSegment(string slug, string expected)
    {
        // Act
        var result = ContentUrlHelper.GetContentUrl(slug);

        // Assert
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData(".")]
    [InlineData(" .. ")]
    [InlineData("./child")]
    [InlineData("parent/../child")]
    [InlineData("parent/./child")]
    [InlineData(@"parent\..\child")]
    [InlineData("parent/.")]
    [InlineData("parent/..")]
    public void Given_RelativePathSegment_When_GetContentUrl_Invoked_Then_It_Should_RejectTheSlug(string slug)
    {
        // Act
        var exception = Should.Throw<ArgumentException>(() => ContentUrlHelper.GetContentUrl(slug));

        // Assert
        exception.ParamName.ShouldBe("slug");
        exception.Message.ShouldStartWith("Content slugs cannot contain relative path segments.");
    }

    [Fact]
    public void Given_NullSlug_When_GetContentUrl_Invoked_Then_It_Should_ThrowArgumentNullException()
    {
        var exception = Should.Throw<ArgumentNullException>(() => ContentUrlHelper.GetContentUrl(null!));

        exception.ParamName.ShouldBe("slug");
    }

    [Theory]
    [InlineData("minimal", "assets/theme.css", "themes/minimal/assets/theme.css")]
    [InlineData("//minimal//", "///assets/theme.css", "themes/minimal/assets/theme.css")]
    [InlineData("minimal", "/assets/theme%20dark.css?v=1#dark", "themes/minimal/assets/theme%20dark.css?v=1#dark")]
    [InlineData(" minimal ", " /assets/theme.css ", "themes/ minimal / /assets/theme.css ")]
    [InlineData("", "", "themes//")]
    public void Given_ThemeAsset_When_GetThemeUrl_Invoked_Then_It_Should_OnlyTrimSlashes(string themeSlug, string path, string expected)
    {
        // Act
        var result = ContentUrlHelper.GetThemeUrl(themeSlug, path);

        // Assert
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData(null, "assets/theme.css", "themeSlug")]
    [InlineData("minimal", null, "path")]
    public void Given_NullThemeArgument_When_GetThemeUrl_Invoked_Then_It_Should_ThrowArgumentNullException(
        string? themeSlug, string? path, string expectedParameter)
    {
        var exception = Should.Throw<ArgumentNullException>(() => ContentUrlHelper.GetThemeUrl(themeSlug!, path!));

        exception.ParamName.ShouldBe(expectedParameter);
    }

    [Theory]
    [InlineData("images/hero.png", "images/hero.png")]
    [InlineData("///images/hero.png", "images/hero.png")]
    [InlineData("/images/hero%20image.png?size=2&fit=cover#focus", "images/hero%20image.png?size=2&fit=cover#focus")]
    [InlineData("https://cdn.example.com/hero%20image.png?size=2&fit=cover#focus", "https://cdn.example.com/hero%20image.png?size=2&fit=cover#focus")]
    [InlineData("http://cdn.example.com/hero.png?size=2#focus", "http://cdn.example.com/hero.png?size=2#focus")]
    [InlineData("HTTPS://cdn.example.com/hero.png", "HTTPS://cdn.example.com/hero.png")]
    [InlineData("//cdn.example.com/hero.png", "cdn.example.com/hero.png")]
    [InlineData(" images/hero image.png ", " images/hero image.png ")]
    [InlineData(@"images\hero.png", @"images\hero.png")]
    [InlineData("data:image/png;base64,AA==", "data:image/png;base64,AA==")]
    [InlineData("", "")]
    [InlineData("/", "")]
    public void Given_ImagePath_When_GetImageUrl_Invoked_Then_It_Should_OnlyTrimLeadingSlashes(string path, string expected)
    {
        // Act
        var result = ContentUrlHelper.GetImageUrl(path);

        // Assert
        result.ShouldBe(expected);
    }

    [Fact]
    public void Given_NullImagePath_When_GetImageUrl_Invoked_Then_It_Should_ThrowArgumentNullException()
    {
        var exception = Should.Throw<ArgumentNullException>(() => ContentUrlHelper.GetImageUrl(null!));

        exception.ParamName.ShouldBe("path");
    }

    [Theory]
    [InlineData("  Mixed Case  ", "tags/mixed%20case")]
    [InlineData("C#", "tags/c%23")]
    [InlineData("topic/subtopic", "tags/topic%2Fsubtopic")]
    [InlineData(@"topic\subtopic", "tags/topic%5Csubtopic")]
    [InlineData("CAFÉ", "tags/caf%C3%A9")]
    [InlineData("q?x=1&y=2#z", "tags/q%3Fx%3D1%26y%3D2%23z")]
    [InlineData("already%20encoded", "tags/already%2520encoded")]
    [InlineData("../topic", "tags/..%2Ftopic")]
    public void Given_Tag_When_GetTagUrl_Invoked_Then_It_Should_NormalizeAndEscapeOneSegment(string tag, string expected)
    {
        // Act
        var result = ContentUrlHelper.GetTagUrl(tag);

        // Assert
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" \t\r\n ")]
    [InlineData(".")]
    [InlineData(" . ")]
    [InlineData("..")]
    [InlineData(" .. ")]
    public void Given_InvalidTag_When_GetTagUrl_Invoked_Then_It_Should_RejectTheTag(string tag)
    {
        var exception = Should.Throw<ArgumentException>(() => ContentUrlHelper.GetTagUrl(tag));

        exception.ParamName.ShouldBe("tag");
    }

    [Fact]
    public void Given_NullTag_When_GetTagUrl_Invoked_Then_It_Should_ThrowArgumentNullException()
    {
        var exception = Should.Throw<ArgumentNullException>(() => ContentUrlHelper.GetTagUrl(null!));

        exception.ParamName.ShouldBe("tag");
    }

    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData(" \t\r\n ", "")]
    [InlineData(" en-US ", "en-us")]
    [InlineData("PT_BR", "pt-br")]
    [InlineData(" ZH_Hant/TW ", "zh-hant-tw")]
    [InlineData(@"en\US", @"en\us")]
    [InlineData(" en US ", "en us")]
    public void Given_Locale_When_GetLocaleSegment_Invoked_Then_It_Should_PreserveLoaderNormalization(string? locale, string expected)
    {
        // Act
        var result = ContentUrlHelper.GetLocaleSegment(locale);

        // Assert
        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("https://example.com/")]
    [InlineData("https://example.com/site/")]
    public void Given_SiteBaseUrl_When_UrlsAreResolved_Then_It_Should_PreserveTheSubpath(string baseUrl)
    {
        // Arrange
        var siteBase = new Uri(baseUrl);

        // Act / Assert
        new Uri(siteBase, ContentUrlHelper.GetContentUrl("/en-us/about & team")).AbsoluteUri
            .ShouldBe($"{baseUrl}en-us/about%20%26%20team");
        new Uri(siteBase, ContentUrlHelper.GetContentUrl("")).AbsoluteUri.ShouldBe(baseUrl);
        new Uri(siteBase, ContentUrlHelper.GetThemeUrl("/minimal/", "/assets/theme.css")).AbsoluteUri
            .ShouldBe($"{baseUrl}themes/minimal/assets/theme.css");
        new Uri(siteBase, ContentUrlHelper.GetImageUrl("/images/hero%20image.png?v=1#focus")).AbsoluteUri
            .ShouldBe($"{baseUrl}images/hero%20image.png?v=1#focus");
        new Uri(siteBase, ContentUrlHelper.GetImageUrl("https://cdn.example.com/hero%20image.png?v=1#focus")).AbsoluteUri
            .ShouldBe("https://cdn.example.com/hero%20image.png?v=1#focus");
        new Uri(siteBase, ContentUrlHelper.GetTagUrl(" Topic/Subtopic ")).AbsoluteUri
            .ShouldBe($"{baseUrl}tags/topic%2Fsubtopic");
    }
}
