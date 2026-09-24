using AngleSharp.Html.Parser;

using ScissorHands.Web.Localization;
using ScissorHands.Web.Services;

namespace ScissorHands.Web.Tests.Localization;

public class ContentLinkLocalizerTests
{
    [Theory]
    [InlineData("/", "/about/", "/ko-kr/about/")]
    [InlineData("/", "/about", "/ko-kr/about")]
    [InlineData("/", "/about/index.html", "/ko-kr/about/index.html")]
    [InlineData("/", "about/?q=a%2Fb&lang=en#team?details", "ko-kr/about/?q=a%2Fb&lang=en#team?details")]
    [InlineData("/docs/", "/docs/about/?q=a+b&x=%26#A%20B", "/docs/ko-kr/about/?q=a+b&x=%26#A%20B")]
    [InlineData("/docs/", "about/", "ko-kr/about/")]
    [InlineData("/docs/", "./about/", "ko-kr/about/")]
    [InlineData("/docs/", "https://EXAMPLE.test/docs/about/?a=1#x", "https://EXAMPLE.test/docs/ko-kr/about/?a=1#x")]
    [InlineData("/docs/", "//example.test/docs/about/", "//example.test/docs/ko-kr/about/")]
    [InlineData("/manual/docs/", "/manual/docs/about/", "/manual/docs/ko-kr/about/")]
    [InlineData("/", "/guides/start/#part", "/ko-kr/guides/start/#part")]
    [InlineData("/", "/2026/09/01/hello/", "/ko-kr/2026/09/01/hello/")]
    [InlineData("/", "/about%20%26%20team/?q=%2F#x", "/ko-kr/about%20%26%20team/?q=%2F#x")]
    public void Given_InternalDocumentLink_When_Localized_Then_It_Should_KeepUrlFormAndSuffix(
        string baseUrl, string href, string expected)
    {
        var localizer = Create(baseUrl);
        var input = $"<p><a href=\"{System.Net.WebUtility.HtmlEncode(href)}\">Link</a></p>";

        using var html = new HtmlParser().ParseDocument(localizer.Localize(input, Xunit.TestContext.Current.CancellationToken));

        html.QuerySelector("a")!.GetAttribute("href").ShouldBe(expected);
    }

    [Theory]
    [InlineData("/docs/", "/about/")]
    [InlineData("/docs/", "../about/")]
    [InlineData("/", "/missing/")]
    [InlineData("/", "/tags/")]
    [InlineData("/", "/")]
    [InlineData("/", "/404.html")]
    [InlineData("/", "/ko-kr/about/")]
    [InlineData("/", "/ja-jp/about/?q=1#team")]
    [InlineData("/", "https://external.test/about/?q=%26#team")]
    [InlineData("/", "//external.test/about/")]
    [InlineData("/", "http://example.test/about/")]
    [InlineData("/", "https://example.test:444/about/")]
    [InlineData("/", "https://user@example.test/about/")]
    [InlineData("/", "/images/photo.jpg")]
    [InlineData("/", "/files/guide.pdf?download=1")]
    [InlineData("/", "/themes/default/assets/theme.css")]
    [InlineData("/", "mailto:reader@example.test")]
    [InlineData("/", "javascript:void(0)")]
    [InlineData("/", "https:about/")]
    [InlineData("/", "https:/about/")]
    [InlineData("/", "#team")]
    [InlineData("/", "?q=1#team")]
    [InlineData("/", "")]
    public void Given_UnmappedOrExplicitLink_When_Localized_Then_It_Should_PreserveTheOriginalHtml(string baseUrl, string href)
    {
        var input = $"<p><a href='{href}'>Link</a></p>";
        Create(baseUrl).Localize(input, Xunit.TestContext.Current.CancellationToken).ShouldBe(input);
    }

    [Fact]
    public async Task Given_MarkdownOptOutAndReferenceLinks_When_Localized_Then_It_Should_HonorAttributesAndResources()
    {
        const string markdown = """
            [Automatic](/about/?q=1&x=%26#team)
            [Primary](/about/?q=1&x=%26#team){data-localize="false"}
            [Japanese](/ja-jp/about/)
            [External](https://external.test/about/?q=1&x=%26#team)
            [Resource](/files/guide.pdf)
            ![Image](/about/)
            [Reference][about]
            [Download](/about/){download=true}

            [about]: /about/#reference
            """;
        var html = await new MarkdownService().ToHtmlAsync(markdown, cancellationToken: Xunit.TestContext.Current.CancellationToken);

        using var document = new HtmlParser().ParseDocument(Create("/").Localize(html, Xunit.TestContext.Current.CancellationToken));
        var links = document.QuerySelectorAll("a").ToDictionary(a => a.TextContent, a => a.GetAttribute("href"));

        links["Automatic"].ShouldBe("/ko-kr/about/?q=1&x=%26#team");
        links["Primary"].ShouldBe("/about/?q=1&x=%26#team");
        links["Japanese"].ShouldBe("/ja-jp/about/");
        links["External"].ShouldBe("https://external.test/about/?q=1&x=%26#team");
        links["Resource"].ShouldBe("/files/guide.pdf");
        links["Reference"].ShouldBe("/ko-kr/about/#reference");
        links["Download"].ShouldBe("/about/");
        document.QuerySelector("img")!.GetAttribute("src").ShouldBe("/about/");
        document.QuerySelector("a[data-localize]")!.GetAttribute("data-localize").ShouldBe("false");
    }

    [Fact]
    public void Given_PreviewOrigin_When_Localized_Then_It_Should_AlsoRecognizeTheConfiguredPublicationOrigin()
    {
        var localizer = new ContentLinkLocalizer("http://127.0.0.1:5000", "/docs/",
            [("about", "ko-kr/about")], "https://example.test");
        const string input = "<a href='https://example.test/docs/about/'>Published</a><a href='http://127.0.0.1:5000/docs/about/'>Preview</a>";
        using var html = new HtmlParser().ParseDocument(localizer.Localize(input, Xunit.TestContext.Current.CancellationToken));
        html.QuerySelectorAll("a").Select(a => a.GetAttribute("href")).ShouldBe(
            ["https://example.test/docs/ko-kr/about/", "http://127.0.0.1:5000/docs/ko-kr/about/"]);
    }

    [Fact]
    public void Given_Cancellation_When_Localizing_Then_It_Should_Stop()
    {
        Should.Throw<OperationCanceledException>(() => Create("/").Localize("<a href='/about/'>About</a>", new CancellationToken(true)));
    }

    private static ContentLinkLocalizer Create(string baseUrl) => new("https://example.test", baseUrl,
        [("about", "ko-kr/about"), ("guides/start", "ko-kr/guides/start"),
         ("2026/09/01/hello", "ko-kr/2026/09/01/hello"), ("about & team", "ko-kr/about & team")]);
}
