using System.Runtime.CompilerServices;

using ScissorHands.Core.Models;

namespace ScissorHands.Core.Tests.Models;

public class PageNavigationTests
{
    [Fact]
    public void Given_DefaultNavigation_When_Construct_Invoked_Then_It_Should_HaveNoNeighbors()
    {
        // Arrange

        // Act
        var navigation = new PageNavigation();

        // Assert
        navigation.Previous.ShouldBeNull();
        navigation.Next.ShouldBeNull();
        navigation.ShouldBe(new PageNavigation());
    }

    [Fact]
    public void Given_DefaultLink_When_Construct_Invoked_Then_It_Should_HaveEmptyStrings()
    {
        // Arrange

        // Act
        var link = new PageNavigationLink();

        // Assert
        link.Title.ShouldBeEmpty();
        link.Url.ShouldBeEmpty();
        link.ShouldBe(new PageNavigationLink());
    }

    [Fact]
    public void Given_NavigationSnapshot_When_Copy_Invoked_Then_It_Should_LeaveOriginalLinksUnchanged()
    {
        // Arrange
        var previous = new PageNavigationLink { Title = "Previous", Url = "guides/previous" };
        var next = new PageNavigationLink { Title = "Next", Url = "guides/next" };
        var navigation = new PageNavigation { Previous = previous, Next = next };

        // Act
        var updated = navigation with
        {
            Previous = previous with { Title = "Changed", Url = "changed" },
            Next = null,
        };

        // Assert
        navigation.Previous.ShouldBeSameAs(previous);
        navigation.Next.ShouldBeSameAs(next);
        previous.Title.ShouldBe("Previous");
        previous.Url.ShouldBe("guides/previous");
        next.Title.ShouldBe("Next");
        next.Url.ShouldBe("guides/next");
        updated.Previous.ShouldBe(new PageNavigationLink { Title = "Changed", Url = "changed" });
        updated.Next.ShouldBeNull();
    }

    [Theory]
    [InlineData(typeof(PageNavigation), nameof(PageNavigation.Previous))]
    [InlineData(typeof(PageNavigation), nameof(PageNavigation.Next))]
    [InlineData(typeof(PageNavigationLink), nameof(PageNavigationLink.Title))]
    [InlineData(typeof(PageNavigationLink), nameof(PageNavigationLink.Url))]
    public void Given_NavigationContract_When_GetProperty_Invoked_Then_It_Should_BeSealedAndInitOnly(Type type, string propertyName)
    {
        // Arrange

        // Act
        var property = type.GetProperty(propertyName);

        // Assert
        type.IsSealed.ShouldBeTrue();
        property.ShouldNotBeNull();
        property.SetMethod.ShouldNotBeNull();
        property.SetMethod.ReturnParameter.GetRequiredCustomModifiers().ShouldContain(typeof(IsExternalInit));
    }
}
