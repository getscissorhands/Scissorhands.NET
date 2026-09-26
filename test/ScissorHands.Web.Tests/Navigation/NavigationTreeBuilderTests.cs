using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;
using ScissorHands.Web.Navigation;

namespace ScissorHands.Web.Tests.Navigation;

public class NavigationTreeBuilderTests
{
    [Fact]
    public void Given_VisiblePages_When_Built_Then_It_Should_CreateReadOnlyMissingAncestorsWithoutChangingInput()
    {
        var page = Page("Child", "/parent/group/child/");
        var pages = new List<ContentDocument> { page };

        var tree = BuildTree(pages);

        var parent = tree.ShouldHaveSingleItem();
        parent.Title.ShouldBe("Parent");
        parent.Path.ShouldBe("parent");
        parent.Url.ShouldBeNull();
        var group = parent.Children.ShouldHaveSingleItem();
        group.Title.ShouldBe("Group");
        group.Url.ShouldBeNull();
        var child = group.Children.ShouldHaveSingleItem();
        child.Title.ShouldBe("Child");
        child.Url.ShouldBe("parent/group/child");
        pages.ShouldBe([page]);
        page.Metadata.Slug.ShouldBe("/parent/group/child/");
        pages.Clear();
        group.Children.ShouldHaveSingleItem();
        var mutableRoots = tree.ShouldBeAssignableTo<IList<NavigationNode>>();
        Should.Throw<NotSupportedException>(() => mutableRoots.Clear());
    }

    [Fact]
    public void Given_ExistingParent_When_Built_Then_It_Should_KeepItsTitleAndLinkInsteadOfAPlaceholder()
    {
        var tree = BuildTree([
            Page("Child", "parent/group/child"),
            Page("Custom parent title", "parent/group"),
        ]);

        var group = tree.ShouldHaveSingleItem().Children.ShouldHaveSingleItem();
        group.Title.ShouldBe("Custom parent title");
        group.Url.ShouldBe("parent/group");
        group.Children.ShouldHaveSingleItem().Url.ShouldBe("parent/group/child");
    }

    [Fact]
    public void Given_ChangedVisiblePages_When_Rebuilt_Then_It_Should_NotRetainEmptyGroups()
    {
        var parent = Page("Parent", "parent");
        var populated = BuildTree([parent, Page("Child", "parent/group/child")]);
        var empty = BuildTree([parent]);

        populated.ShouldHaveSingleItem().Children.ShouldHaveSingleItem();
        empty.ShouldHaveSingleItem().Children.ShouldBeEmpty();
        BuildTree([]).ShouldBeEmpty();
    }

    [Theory]
    [InlineData("docs", "docs-other/child")]
    [InlineData("en-us/docs", "ko-kr/docs/child")]
    [InlineData("docs", "docs%2Fother/child")]
    public void Given_DifferentPathSegments_When_Built_Then_It_Should_NotAttachUnrelatedChildren(string parentSlug, string childSlug)
    {
        var tree = BuildTree([Page("Parent", parentSlug), Page("Child", childSlug)]);

        var parent = Flatten(tree).Single(node => node.Title == "Parent");
        parent.Children.ShouldBeEmpty();
        Flatten(tree).Count(node => node.Title == "Child").ShouldBe(1);
    }

    [Theory]
    [InlineData("deployment-tools", "Deployment Tools", "parent/deployment-tools/child")]
    [InlineData("100% ready", "100% Ready", "parent/100%25%20ready/child")]
    [InlineData("<img src=x>", "<Img Src=X>", "parent/%3Cimg%20src%3Dx%3E/child")]
    public void Given_MissingParentSegment_When_Built_Then_It_Should_SeparateDisplayTextFromEscapedUrls(
        string segment,
        string title,
        string url)
    {
        var tree = BuildTree([Page("Child", $"parent/{segment}/child")]);

        var group = tree.ShouldHaveSingleItem().Children.ShouldHaveSingleItem();
        group.Title.ShouldBe(title);
        group.Url.ShouldBeNull();
        group.Children.ShouldHaveSingleItem().Url.ShouldBe(url);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Given_LocalePrefix_When_Built_Then_It_Should_KeepOnlyRealLocaleLandingPages(bool localePage)
    {
        var pages = new List<ContentDocument> { Page("Child", "en-us/docs/child", "en_US") };
        if (localePage)
        {
            pages.Add(Page("English", "en-us", "en_US"));
        }

        var tree = BuildTree(pages, new SiteManifest
        {
            Locales = ["ko-kr", "en-us"],
            BaseUrl = "/site/",
        });

        tree.ShouldHaveSingleItem().Title.ShouldBe(localePage ? "English" : "Docs");
        Flatten(tree).Where(node => node.Url is null).Select(node => node.Title).ShouldBe(["Docs"]);
        Flatten(tree).Single(node => node.Title == "Child").Url.ShouldBe("en-us/docs/child");
    }

    [Fact]
    public void Given_UnorderedPages_When_Built_Then_It_Should_OrderSiblingsByTitleAndPath()
    {
        var pages = new[]
        {
            Page("Same", "docs/b"), Page("Zebra", "docs/z"), Page("Same", "docs/a"),
            Page("Docs", "docs"), Page("A root", "first"),
        };

        var tree = BuildTree(pages);
        var reversed = BuildTree(pages.Reverse().ToArray());

        Flatten(tree).Select(node => node.Path).ShouldBe(["first", "docs", "docs/a", "docs/b", "docs/z"]);
        Flatten(reversed).Select(node => node.Path).ShouldBe(Flatten(tree).Select(node => node.Path));
    }

    [Fact]
    public void Given_CancelledToken_When_Built_Then_It_Should_PropagateCancellation()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        Should.Throw<OperationCanceledException>(() => NavigationTreeBuilder.Build([], cancellationToken: cancellation.Token));
    }

    [Fact]
    public void Given_NullPages_When_Built_Then_It_Should_RejectTheInput()
    {
        Should.Throw<ArgumentNullException>(() => BuildTree(null!)).ParamName.ShouldBe("navigationPages");
    }

    [Theory]
    [InlineData("../outside")]
    [InlineData("parent/../outside")]
    public void Given_UnsafeSlug_When_Built_Then_It_Should_RejectTheRoute(string slug)
    {
        Should.Throw<ArgumentException>(() => BuildTree([Page("Invalid", slug)]));
    }

    private static IReadOnlyList<NavigationNode> BuildTree(IReadOnlyList<ContentDocument> pages, SiteManifest? site = null)
        => NavigationTreeBuilder.Build(pages, site, Xunit.TestContext.Current.CancellationToken);

    private static IEnumerable<NavigationNode> Flatten(IEnumerable<NavigationNode> nodes)
    {
        foreach (var node in nodes)
        {
            yield return node;
            foreach (var child in Flatten(node.Children))
            {
                yield return child;
            }
        }
    }

    private static ContentDocument Page(string title, string slug, string? locale = null) => new()
    {
        Kind = ContentKind.Page,
        Metadata = new ContentMetadata { Title = title, Slug = slug, Locale = locale, ShowInNavigation = true },
    };
}
