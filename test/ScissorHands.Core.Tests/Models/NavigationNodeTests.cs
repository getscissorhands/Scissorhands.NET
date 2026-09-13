using ScissorHands.Core.Models;

namespace ScissorHands.Core.Tests.Models;

public class NavigationNodeTests
{
    [Fact]
    public void Given_DefaultNode_When_Constructed_Then_It_Should_HaveNonNullDefaults()
    {
        var node = new NavigationNode();

        node.Title.ShouldBeEmpty();
        node.Path.ShouldBeEmpty();
        node.Url.ShouldBeNull();
        node.Children.ShouldBeEmpty();
    }

    [Fact]
    public void Given_Children_When_SourceCollectionChanges_Then_It_Should_RetainAnImmutableSnapshot()
    {
        var child = new NavigationNode { Title = "Child", Path = "parent/child", Url = "parent/child" };
        var children = new List<NavigationNode> { child };
        var node = new NavigationNode { Title = "Parent", Path = "parent", Children = children };

        children.Clear();

        node.Children.ShouldBe([child]);
        var mutableView = node.Children.ShouldBeAssignableTo<IList<NavigationNode>>();
        Should.Throw<NotSupportedException>(() => mutableView.Add(new NavigationNode()));
    }

    [Fact]
    public void Given_NullChildren_When_Initialized_Then_It_Should_UseAnEmptyCollection()
    {
        new NavigationNode { Children = null! }.Children.ShouldBeEmpty();
    }
}
