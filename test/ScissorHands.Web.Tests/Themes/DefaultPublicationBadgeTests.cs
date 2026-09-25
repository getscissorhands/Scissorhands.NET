using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;

using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;
using ScissorHands.Core.Services;
using ScissorHands.Web.Publication;

namespace ScissorHands.Web.Tests.Themes;

public class DefaultPublicationBadgeTests
{
    [Theory]
    [InlineData(typeof(PageView))]
    [InlineData(typeof(PostView))]
    public void Given_DraftAndScheduledDetail_When_Render_Invoked_Then_It_Should_PlaceBothBadgesBeforeAllArticleContent(Type view)
    {
        // Arrange
        using var context = CreateContext();
        var document = Prepared("future", true, true);
        RenderFragment body = builder =>
        {
            builder.OpenComponent(0, view);
            builder.CloseComponent();
        };

        // Act
        var cut = context.Render<MainLayout>(parameters => parameters
            .Add(component => component.Site, new SiteManifest { IsPreview = true })
            .Add(component => component.Theme, new ThemeManifest())
            .Add(component => component.Document, document)
            .Add(component => component.PageNavigation, new PageNavigation
            {
                Next = new PageNavigationLink { Title = "Draft next page", Url = "next" },
            })
            .Add(component => component.Body, body));

        // Assert
        var article = cut.Find("article");
        article.GetAttribute("data-publication-content").ShouldBe("future");
        var badgeGroup = article.FirstElementChild!;
        badgeGroup.ClassList.ShouldContain("publication-badges");
        badgeGroup.QuerySelectorAll("[data-publication-badge]")
            .Select(badge => badge.TextContent).ShouldBe(["Draft", "Scheduled on 2099-01-02"]);
        cut.FindAll("nav [data-publication-badge]").ShouldBeEmpty();
        PublicationBadgeValidator.Validate(cut.Markup, [document], false, true, "future", Xunit.TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData(typeof(IndexView), 2)]
    [InlineData(typeof(TagView), 3)]
    public void Given_PreparedListingEntries_When_Render_Invoked_Then_It_Should_AssociateBadgesWithEachPostOrPage(Type view, int expectedCount)
    {
        // Arrange
        using var context = CreateContext();
        var post = Prepared("future-post", true, true);
        var page = Prepared("draft-page", true, false, ContentKind.Page);
        RenderFragment body = builder =>
        {
            builder.OpenComponent(0, view);
            builder.CloseComponent();
        };

        // Act
        var cut = context.Render<MainLayout>(parameters => parameters
            .Add(component => component.Site, new SiteManifest { IsPreview = true })
            .Add(component => component.Theme, new ThemeManifest())
            .Add(component => component.Document, new ContentDocument())
            .Add(component => component.Documents, new[] { post })
            .Add(component => component.TaggedPosts, new[] { post })
            .Add(component => component.TaggedPages, new[] { page })
            .Add(component => component.Tag, "topic")
            .Add(component => component.Body, body));

        // Assert
        cut.FindAll("[data-publication-badge]").Count.ShouldBe(expectedCount);
        var entry = cut.Find("[data-publication-entry='future-post']");
        entry.QuerySelector("a")!.GetAttribute("href").ShouldBe("future-post");
        entry.QuerySelectorAll("[data-publication-route='future-post']").Length.ShouldBe(2);
        if (view == typeof(TagView))
        {
            cut.Find("[data-publication-entry='draft-page'] [data-publication-badge]").TextContent.ShouldBe("Draft");
        }
        PublicationBadgeValidator.Validate(cut.Markup, view == typeof(TagView) ? [post, page] : [post],
            true, true, "listing", Xunit.TestContext.Current.CancellationToken);
    }

    [Theory]
    [InlineData(typeof(TagListView))]
    [InlineData(typeof(NotFoundView))]
    public void Given_NonDocumentCollectionOrNotFound_When_Render_Invoked_Then_It_Should_NotShowStatusBadges(Type view)
    {
        // Arrange
        using var context = CreateContext();
        var post = Prepared("draft", true, true);
        var tags = new Dictionary<string, (IEnumerable<ContentDocument> Posts, IEnumerable<ContentDocument> Pages)>
        {
            ["topic"] = ([post], []),
        };
        RenderFragment body = builder =>
        {
            builder.OpenComponent(0, view);
            builder.CloseComponent();
        };

        // Act
        var cut = context.Render<MainLayout>(parameters => parameters
            .Add(component => component.Site, new SiteManifest { IsPreview = true })
            .Add(component => component.Theme, new ThemeManifest())
            .Add(component => component.Document, new ContentDocument())
            .Add(component => component.Documents, new[] { post })
            .Add(component => component.TaggedDocuments, tags)
            .Add(component => component.Body, body));

        // Assert
        cut.FindAll("[data-publication-badge]").ShouldBeEmpty();
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public void Given_BuildOrUnflaggedContent_When_Render_Invoked_Then_It_Should_NotInferBadgesFromAuthoredMetadata(bool preview, bool flagged)
    {
        // Arrange
        using var context = new BunitContext();
        var document = Prepared("post", flagged, flagged);

        // Act
        var cut = context.Render<PostView>(parameters => parameters
            .AddCascadingValue(new SiteManifest { IsPreview = preview })
            .AddCascadingValue(document));

        // Assert
        cut.FindAll("[data-publication-badge]").ShouldBeEmpty();
        document.Metadata.Draft.ShouldBeTrue();
    }

    private static BunitContext CreateContext()
    {
        var context = new BunitContext();
        context.Services.AddSingleton(Substitute.For<IThemeService>());
        return context;
    }

    private static ContentDocument Prepared(string route, bool draft, bool scheduled, ContentKind kind = ContentKind.Post) => new()
    {
        Kind = kind,
        Metadata = new ContentMetadata
        {
            Slug = route,
            Draft = true,
            Title = "Entry <encoded>",
            HeroImage = "hero.png",
            Published = new DateTimeOffset(2099, 1, 2, 0, 0, 0, TimeSpan.Zero),
        },
        Html = "<h1>Actual content</h1><p>Body</p>",
        PublicationStatus = new PublicationStatus
        {
            Route = route,
            IsDraft = draft,
            ScheduledDate = scheduled ? new DateOnly(2099, 1, 2) : null,
        },
    };
}
