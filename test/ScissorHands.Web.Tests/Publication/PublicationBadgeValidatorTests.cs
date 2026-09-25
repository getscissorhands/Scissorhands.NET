using ScissorHands.Core.Models;
using ScissorHands.Web.Publication;

namespace ScissorHands.Web.Tests.Publication;

public class PublicationBadgeValidatorTests
{
    [Theory]
    [InlineData("<strong>Draft</strong>")]
    [InlineData("<span hidden>Decoration</span>Draft")]
    [InlineData("<span aria-hidden='true'>Decoration</span><b>Draft</b>")]
    public void Given_VisibleFormattedBadge_When_Validate_Invoked_Then_It_Should_AcceptThemeOwnedMarkup(string text)
    {
        // Arrange
        var html = Detail(Badge(text));

        // Act
        var exception = Record.Exception(() => Validate(html));

        // Assert
        exception.ShouldBeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("<span>Draft</span>")]
    [InlineData("<span data-publication-badge='draft'>Draft</span>")]
    [InlineData("<span data-publication-badge='draft' data-publication-route='other' data-publication-placement='detail'>Draft</span>")]
    [InlineData("<span data-publication-badge='draft' data-publication-route='post' data-publication-placement='listing'>Draft</span>")]
    public void Given_MissingOrWrongBadge_When_Validate_Invoked_Then_It_Should_FailWithActionableRouteContext(string badge)
    {
        // Arrange
        var html = Detail(badge);

        // Act
        var exception = Should.Throw<InvalidDataException>(() => Validate(html));

        // Assert
        exception.Message.ShouldContain("rendered route 'rendered'");
        exception.Message.ShouldContain("post");
        exception.Message.ShouldContain("PublicationBadgeBase");
        exception.Message.ShouldContain("post-HTML plugins");
    }

    [Theory]
    [InlineData("<span hidden>Draft</span>")]
    [InlineData("Dr<span aria-hidden='TRUE'>aft</span>")]
    [InlineData("<span hidden><span aria-hidden='false'>Draft</span></span>")]
    [InlineData("<script>Draft</script>")]
    [InlineData("<style>Draft</style>")]
    [InlineData("<template>Draft</template>")]
    [InlineData("<noscript>Draft</noscript>")]
    [InlineData("Draft!")]
    public void Given_AlteredOrHiddenRequiredText_When_Validate_Invoked_Then_It_Should_RejectTheBadge(string text)
    {
        // Arrange
        var html = Detail(Badge(text));

        // Act
        var exception = Record.Exception(() => Validate(html));

        // Assert
        exception.ShouldBeOfType<InvalidDataException>();
    }

    [Theory]
    [InlineData("hidden")]
    [InlineData("aria-hidden='true'")]
    [InlineData("inert")]
    public void Given_HiddenAncestorOrBadge_When_Validate_Invoked_Then_It_Should_NotCountItsExposedText(string attribute)
    {
        // Arrange
        var ancestor = $"<div {attribute}>{Detail(Badge())}</div>";
        var badge = Detail(Badge().Replace("<span ", $"<span {attribute} ", StringComparison.Ordinal));

        // Act
        var ancestorException = Record.Exception(() => Validate(ancestor));
        var badgeException = Record.Exception(() => Validate(badge));

        // Assert
        ancestorException.ShouldBeOfType<InvalidDataException>();
        badgeException.ShouldBeOfType<InvalidDataException>();
    }

    [Theory]
    [InlineData("<h1>Content first</h1>")]
    [InlineData("<img src='hero.png'>")]
    [InlineData("<time>2099-01-02</time>")]
    [InlineData("Authored text first")]
    public void Given_ContentPrecedingDetailBadges_When_Validate_Invoked_Then_It_Should_RejectLatePlacement(string prefix)
    {
        // Arrange
        var html = Detail(prefix + Badge());

        // Act
        var exception = Should.Throw<InvalidDataException>(() => Validate(html));

        // Assert
        exception.Message.ShouldContain("beginning");
    }

    [Theory]
    [InlineData("nav")]
    [InlineData("header")]
    [InlineData("footer")]
    [InlineData("aside")]
    public void Given_BadgeMovedToNonContentRegion_When_Validate_Invoked_Then_It_Should_RejectMisplacement(string element)
    {
        // Arrange
        var html = $"<{element}>{Detail(Badge())}</{element}>";

        // Act
        var exception = Record.Exception(() => Validate(html));

        // Assert
        exception.ShouldBeOfType<InvalidDataException>();
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void Given_TwoRequiredStatusesAndOneMissing_When_Validate_Invoked_Then_It_Should_IdentifyTheOmittedBadge(bool draft, bool scheduled)
    {
        // Arrange
        var html = Detail((draft ? Badge() : "") + (scheduled ? Badge("Scheduled on 2099-01-02", "scheduled") : ""));
        var document = Document("post", scheduled: true);

        // Act
        var exception = Should.Throw<InvalidDataException>(() => PublicationBadgeValidator.Validate(
            html, [document], false, true, "rendered", Xunit.TestContext.Current.CancellationToken));

        // Assert
        exception.Message.ShouldContain(draft ? "Scheduled on 2099-01-02" : "Draft");
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Given_UnexpectedOrDuplicateBadge_When_Validate_Invoked_Then_It_Should_RejectIt(bool preview, bool duplicate)
    {
        // Arrange
        var html = Detail(Badge() + (duplicate ? Badge() : ""));
        var documents = duplicate ? new[] { Document("post") } : [];

        // Act
        var exception = Record.Exception(() => PublicationBadgeValidator.Validate(
            html, documents, false, preview, "rendered", Xunit.TestContext.Current.CancellationToken));

        // Assert
        exception.ShouldBeOfType<InvalidDataException>();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Given_TwoListingEntries_When_Validate_Invoked_Then_It_Should_RequireBadgesInTheirOwnEntry(bool swap)
    {
        // Arrange
        var first = Badge(placement: "listing");
        var second = Badge(route: "second", placement: "listing");
        var html = $"<main><ul><li data-publication-entry='post'><a href='post'>Post</a>{(swap ? second : first)}</li>"
            + $"<li data-publication-entry='second'><a href='second'>Second</a>{(swap ? first : second)}</li></ul></main>";

        // Act
        var exception = Record.Exception(() => PublicationBadgeValidator.Validate(
            html, [Document("post"), Document("second")], true, true, "tags/topic", Xunit.TestContext.Current.CancellationToken));

        // Assert
        if (swap)
        {
            exception.ShouldBeOfType<InvalidDataException>();
        }
        else
        {
            exception.ShouldBeNull();
        }
    }

    [Fact]
    public void Given_CancelledGeneration_When_Validate_Invoked_Then_It_Should_HonorCancellation()
    {
        // Arrange
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        // Act
        var exception = Record.Exception(() => PublicationBadgeValidator.Validate("", [], false, false, "route", cancellation.Token));

        // Assert
        exception.ShouldBeOfType<OperationCanceledException>();
    }

    private static ContentDocument Document(string route, bool scheduled = false) => new()
    {
        PublicationStatus = new PublicationStatus
        {
            Route = route,
            IsDraft = true,
            ScheduledDate = scheduled ? new DateOnly(2099, 1, 2) : null,
        },
    };

    private static string Detail(string badges) =>
        $"<main><article data-publication-content='post'><div>{badges}</div><h1>Post content</h1></article></main>";

    private static string Badge(string text = "Draft", string kind = "draft", string route = "post", string placement = "detail") =>
        $"<span data-publication-badge='{kind}' data-publication-route='{route}' data-publication-placement='{placement}'>{text}</span>";

    private static void Validate(string html) => PublicationBadgeValidator.Validate(
        html, [Document("post")], false, true, "rendered", Xunit.TestContext.Current.CancellationToken);
}
