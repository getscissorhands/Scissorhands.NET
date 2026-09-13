using ScissorHands.Core.Models;
using ScissorHands.Web.Navigation;

namespace ScissorHands.Web.Tests.Navigation;

public class PageReadingOrderTests
{
    [Fact]
    public void Given_MixedFilesAndDirectories_When_Built_Then_It_Should_VisitIndexesFirstAndDirectoriesDepthFirst()
    {
        var pages = new[]
        {
            Page("A last", "last", @"parent\03-child-2.md"),
            Page("B grandchild", "grandchild", @"parent\02-group\01-grandchild.md"),
            Page("C child", "zulu", @"parent\01-child.md"),
            Page("D parent", "parent-page", @"parent\INDEX.md"),
            Page("E group landing", "group-page", @"parent\02-group\index.md"),
            Page("F root landing", "root-page", "index.md"),
        };

        var tree = Build(pages);

        tree.Select(node => node.Url).ShouldBe(["root-page", "parent-page", "zulu", "group-page", "grandchild", "last"]);
        Build(pages.Reverse().ToArray()).Select(node => node.Url).ShouldBe(tree.Select(node => node.Url));
        pages[0].SourcePath.ShouldBe(@"parent\03-child-2.md");
        pages[2].Metadata.Slug.ShouldBe("zulu");
    }

    [Fact]
    public void Given_OrdinalFileNames_When_Built_Then_It_Should_Not_UseNaturalOrTitleOrdering()
    {
        var tree = Build([
            Page("A", "two", "2-two.md"),
            Page("Z", "ten", "10-ten.md"),
            Page("B", "upper", "A-page.md"),
            Page("Y", "lower", "a-page.md"),
        ]);

        tree.Select(node => node.Url).ShouldBe(["ten", "two", "upper", "lower"]);
    }

    [Fact]
    public void Given_SourceLessPages_When_Built_Then_It_Should_AppendThemInOrdinalTitleAndSlugOrder()
    {
        var tree = Build([
            Page("Alpha", "b"),
            Page("Zebra", "z"),
            Page("Alpha", "a"),
            Page("ZZ file", "file", "01-file.md"),
        ]);

        tree.Select(node => node.Url).ShouldBe(["file", "a", "b", "z"]);
    }

    [Fact]
    public void Given_SlugGroupsDifferentFromSources_When_Built_Then_It_Should_RankGroupsWithoutRegroupingPages()
    {
        var tree = Build([
            Page("A last", "alpha/last", "03-last.md"),
            Page("B middle", "middle", "02-middle.md"),
            Page("Z first", "zulu/group/first", "01-first.md"),
        ]);

        tree.Select(node => node.Path).ShouldBe(["zulu", "middle", "alpha"]);
        var group = tree[0].Children.ShouldHaveSingleItem();
        group.Path.ShouldBe("zulu/group");
        group.Url.ShouldBeNull();
        group.Children.ShouldHaveSingleItem().Url.ShouldBe("zulu/group/first");
    }

    [Fact]
    public void Given_RealParentWithLaterSource_When_Built_Then_It_Should_UseItsEarliestDescendantRank()
    {
        var tree = Build([
            Page("Parent", "parent", "99-parent.md"),
            Page("Other", "other", "02-other.md"),
            Page("Child", "parent/child", "01-child.md"),
        ]);

        tree.Select(node => node.Url).ShouldBe(["parent", "other"]);
        tree[0].Children.ShouldHaveSingleItem().Url.ShouldBe("parent/child");
    }

    [Theory]
    [InlineData(" ")]
    [InlineData("../escape.md")]
    [InlineData("folder/../escape.md")]
    [InlineData(@"folder\.\page.md")]
    [InlineData("folder/")]
    [InlineData("bad\0name.md")]
    public void Given_InvalidSuppliedPath_When_Built_Then_It_Should_NotTreatItAsSourceLess(string source)
    {
        var exception = Should.Throw<InvalidDataException>(() => Build([Page("Page", "page", source)]));

        exception.Message.ShouldContain("Page source path");
    }

    [Fact]
    public void Given_FullyQualifiedPaths_When_Built_Then_It_Should_OrderRelativeComponentsWithoutReadingFiles()
    {
        var root = Path.Combine(Path.GetPathRoot(Environment.CurrentDirectory)!, "not-read", "pages");
        var tree = Build([
            Page("Second", "second", Path.Combine(root, "02-second.md")),
            Page("First", "first", Path.Combine(root, "01-first.md")),
        ]);

        tree.Select(node => node.Url).ShouldBe(["first", "second"]);
    }

    private static IReadOnlyList<NavigationNode> Build(IReadOnlyList<ContentDocument> pages)
        => NavigationTreeBuilder.Build(pages, cancellationToken: Xunit.TestContext.Current.CancellationToken);

    private static ContentDocument Page(string title, string slug, string source = "") => new()
    {
        SourcePath = source,
        Kind = ContentKind.Page,
        Metadata = new ContentMetadata { Title = title, Slug = slug, ShowInNavigation = true },
    };
}
