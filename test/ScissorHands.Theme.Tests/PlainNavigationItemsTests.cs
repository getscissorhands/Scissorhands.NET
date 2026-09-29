using ScissorHands.Core.Models;
using ScissorHands.Theme.Components;

namespace ScissorHands.Theme.Tests;

public class PlainNavigationItemsTests
{
    [Fact]
    public void Given_PreparedTree_When_Rendered_Then_It_Should_PreserveOrderAndRenderGroupsWithoutLinks()
    {
        using var context = new BunitContext();
        var nodes = new[]
        {
            new NavigationNode
            {
                Title = "Guides",
                Path = "guides",
                Children =
                [
                    new() { Title = "Getting started", Path = "guides/start", Url = "guides/start" },
                    new()
                    {
                        Title = "Advanced", Path = "guides/advanced",
                        Children = [new() { Title = "Plugins", Path = "guides/advanced/plugins", Url = "guides/advanced/plugins" }],
                    },
                ],
            },
            new NavigationNode { Title = "About", Path = "about", Url = "about" },
        };

        var cut = context.Render<PlainNavigationItems>(parameters => parameters.Add(p => p.Nodes, nodes));

        cut.FindAll("li").Count.ShouldBe(5);
        cut.FindAll("li > span").Select(element => element.TextContent).ShouldBe(["Guides", "Advanced"]);
        cut.FindAll("li > a").Select(element => element.TextContent).ShouldBe(["Getting started", "Plugins", "About"]);
        cut.FindAll("li > a").Select(element => element.GetAttribute("href"))
            .ShouldBe(["guides/start", "guides/advanced/plugins", "about"]);
        cut.FindAll("li > ul").Count.ShouldBe(2);
        cut.FindAll("button").ShouldBeEmpty();
    }

    [Fact]
    public void Given_UntrustedTitleAndUrl_When_Rendered_Then_It_Should_EncodeMarkupAndAttributes()
    {
        using var context = new BunitContext();
        const string title = "<img src=x onerror=alert(1)>";
        const string url = "page?x=\" onclick=\"alert(1)";
        var cut = context.Render<PlainNavigationItems>(parameters => parameters.Add(p => p.Nodes,
            [new NavigationNode { Title = title, Path = "page", Url = url }]));

        cut.Find("a").TextContent.ShouldBe(title);
        cut.Find("a").GetAttribute("href").ShouldBe(url);
        cut.Find("a").HasAttribute("onclick").ShouldBeFalse();
        cut.FindAll("img").ShouldBeEmpty();
    }

    [Theory]
    [InlineData("javascript:alert(1)")]
    [InlineData("https://example.com")]
    [InlineData("//example.com")]
    [InlineData("\\example.com")]
    [InlineData("java\nscript:alert(1)")]
    [InlineData("")]
    public void Given_NonRelativeUrl_When_Rendered_Then_It_Should_RejectUnsafeNavigation(string url)
    {
        using var context = new BunitContext();

        var error = Should.Throw<InvalidOperationException>(() => context.Render<PlainNavigationItems>(parameters =>
            parameters.Add(p => p.Nodes, [new NavigationNode { Title = "Page", Path = "page", Url = url }])));

        error.Message.ShouldContain("base-relative navigation URL");
    }

    [Fact]
    public void Given_NoNodes_When_Rendered_Then_It_Should_EmitNoListItems()
    {
        using var context = new BunitContext();

        var cut = context.Render<PlainNavigationItems>();

        cut.FindAll("li").ShouldBeEmpty();
        cut.FindAll("nav").ShouldBeEmpty();
        cut.FindAll("ul").ShouldBeEmpty();
    }
}
