using System.Globalization;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;
using ScissorHands.Theme.Components;

namespace ScissorHands.Theme.Tests;

public class PublicationBadgeBaseTests
{
    [Theory]
    [InlineData(true, true, true, 2)]
    [InlineData(true, true, false, 1)]
    [InlineData(true, false, true, 1)]
    [InlineData(true, false, false, 0)]
    [InlineData(false, true, true, 0)]
    public void Given_PreparedStatus_When_Render_Invoked_Then_It_Should_RenderOnlyPreviewBadgesAndRecordEachFragment(
        bool preview, bool draft, bool scheduled, int count)
    {
        // Arrange
        using var context = new BunitContext();
        var receipt = new PublicationBadgeBase.RenderReceipt();
        var document = new ContentDocument
        {
            Metadata = new ContentMetadata { Draft = true, Slug = "changed-by-hook" },
            PublicationStatus = new PublicationStatus
            {
                Route = "original",
                IsDraft = draft,
                ScheduledDate = scheduled ? new DateOnly(2099, 1, 2) : null,
            },
        };

        // Act
        var cut = context.Render<CustomBadges>(parameters => parameters
            .AddCascadingValue(new SiteManifest { IsPreview = preview })
            .AddCascadingValue(document)
            .AddCascadingValue(receipt));

        // Assert
        cut.FindAll("strong").Count.ShouldBe(count);
        receipt.WasRendered("original", PublicationBadgePlacement.Detail, "draft").ShouldBe(preview && draft);
        receipt.WasRendered("original", PublicationBadgePlacement.Detail, "scheduled").ShouldBe(preview && scheduled);
        if (preview && draft)
        {
            cut.Find("[data-publication-badge='draft']").TextContent.ShouldBe("Draft");
        }
        if (preview && scheduled)
        {
            cut.Find("[data-publication-badge='scheduled']").TextContent.ShouldBe("Scheduled on 2099-01-02");
            cut.Find("[data-publication-badge='scheduled']").GetAttribute("data-publication-date").ShouldBe("2099-01-02");
            receipt.GetRenderedText("original", PublicationBadgePlacement.Detail, "scheduled").ShouldBe("Scheduled on 2099-01-02");
        }
        cut.Markup.ShouldNotContain("changed-by-hook");
    }

    [Theory]
    [InlineData("en-US", "MMM dd, yyyy", "Sep 26, 2026")]
    [InlineData("en-GB", "dd/MM/yyyy", "26/09/2026")]
    [InlineData("fr-FR", "dd MMMM yyyy", "26 septembre 2026")]
    public void Given_ThemeOwnedLabelsAndCulture_When_Rendered_Then_It_Should_PreserveStructuredStatusAndAuthoredDate(
        string culture, string format, string expectedDate)
    {
        using var context = new BunitContext();
        var receipt = new PublicationBadgeBase.RenderReceipt();
        var document = new ContentDocument
        {
            PublicationStatus = new PublicationStatus { Route = "post", IsDraft = true, ScheduledDate = new DateOnly(2026, 9, 26) },
        };

        var cut = context.Render<CustomBadges>(parameters => parameters
            .AddCascadingValue(new SiteManifest { IsPreview = true })
            .AddCascadingValue(document)
            .AddCascadingValue(receipt)
            .Add(component => component.DraftLabel, "Brouillon")
            .Add(component => component.ScheduledPrefix, "Publication le")
            .Add(component => component.Culture, CultureInfo.GetCultureInfo(culture))
            .Add(component => component.DateFormat, format));

        cut.Find("[data-publication-badge='draft']").TextContent.ShouldBe("Brouillon");
        var scheduled = cut.Find("[data-publication-badge='scheduled']");
        scheduled.TextContent.ShouldBe($"Publication le {expectedDate}");
        scheduled.GetAttribute("data-publication-date").ShouldBe("2026-09-26");
        receipt.GetRenderedText("post", PublicationBadgePlacement.Detail, "scheduled").ShouldBe($"Publication le {expectedDate}");
        document.PublicationStatus.ScheduledDate.ShouldBe(new DateOnly(2026, 9, 26));
    }

    [Fact]
    public void Given_ThemeLabelContainingHtml_When_Rendered_Then_It_Should_EncodeAndRecordTheLabel()
    {
        using var context = new BunitContext();
        var receipt = new PublicationBadgeBase.RenderReceipt();
        const string label = "<script>alert('label')</script> & Draft";

        var cut = context.Render<CustomBadges>(parameters => parameters
            .AddCascadingValue(new SiteManifest { IsPreview = true })
            .AddCascadingValue(new ContentDocument { PublicationStatus = new PublicationStatus { Route = "post", IsDraft = true } })
            .AddCascadingValue(receipt)
            .Add(component => component.DraftLabel, label));

        cut.FindAll("script").ShouldBeEmpty();
        cut.Find("strong").TextContent.ShouldBe(label);
        receipt.GetRenderedText("post", PublicationBadgePlacement.Detail, "draft").ShouldBe(label);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Given_EmptyThemeLabel_When_Rendered_Then_It_Should_RejectMissingPresentation(string label)
    {
        using var context = new BunitContext();

        Should.Throw<ArgumentException>(() => context.Render<CustomBadges>(parameters => parameters
            .AddCascadingValue(new SiteManifest { IsPreview = true })
            .AddCascadingValue(new ContentDocument { PublicationStatus = new PublicationStatus { Route = "post", IsDraft = true } })
            .Add(component => component.DraftLabel, label)));
    }

    [Fact]
    public void Given_ExplicitListingContent_When_Render_Invoked_Then_It_Should_OverrideTheCascadingDetailAndEncodeAttributes()
    {
        // Arrange
        using var context = new BunitContext();
        var receipt = new PublicationBadgeBase.RenderReceipt();
        const string route = "entry\"><script>bad</script>";
        var entry = new ContentDocument { PublicationStatus = new PublicationStatus { Route = route, IsDraft = true } };
        var detail = new ContentDocument { PublicationStatus = new PublicationStatus { Route = "detail", IsDraft = true } };

        // Act
        var cut = context.Render<CustomBadges>(parameters => parameters
            .AddCascadingValue(new SiteManifest { IsPreview = true })
            .AddCascadingValue(detail)
            .AddCascadingValue(receipt)
            .Add(component => component.Content, entry)
            .Add(component => component.Placement, PublicationBadgePlacement.Listing));

        // Assert
        cut.Find("strong").GetAttribute("data-publication-route").ShouldBe(route);
        cut.Find("strong").GetAttribute("data-publication-placement").ShouldBe("listing");
        cut.FindAll("script").ShouldBeEmpty();
        receipt.WasRendered(route, PublicationBadgePlacement.Listing, "draft").ShouldBeTrue();
        receipt.WasRendered("detail", PublicationBadgePlacement.Detail, "draft").ShouldBeFalse();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Given_InheritanceOrTextWithoutFragment_When_Render_Invoked_Then_It_Should_NotRecordDelivery(bool lookalike)
    {
        // Arrange
        using var context = new BunitContext();
        var receipt = new PublicationBadgeBase.RenderReceipt();
        var document = new ContentDocument { PublicationStatus = new PublicationStatus { Route = "draft", IsDraft = true } };

        // Act
        context.Render<UndeliveredBadges>(parameters => parameters
            .AddCascadingValue(new SiteManifest { IsPreview = true })
            .AddCascadingValue(document)
            .AddCascadingValue(receipt)
            .Add(component => component.Lookalike, lookalike));

        // Assert
        receipt.WasRendered("draft", PublicationBadgePlacement.Detail, "draft").ShouldBeFalse();
    }

    [Theory]
    [InlineData(PublicationBadgePlacement.Detail, "data-publication-content")]
    [InlineData(PublicationBadgePlacement.Listing, "data-publication-entry")]
    public void Given_PreparedDocument_When_GetRegionAttributes_Invoked_Then_It_Should_UseStableRouteWithoutRequiringStyles(
        PublicationBadgePlacement placement, string attribute)
    {
        // Arrange
        var document = new ContentDocument
        {
            Metadata = new ContentMetadata { Slug = "changed" },
            PublicationStatus = new PublicationStatus { Route = "original", IsDraft = true },
        };

        // Act
        var attributes = PublicationBadgeBase.GetRegionAttributes(document, placement);

        // Assert
        attributes.ShouldHaveSingleItem().Key.ShouldBe(attribute);
        attributes[attribute].ShouldBe("original");
        PublicationBadgeBase.GetRegionAttributes(new ContentDocument(), placement).ShouldBeEmpty();
        PublicationBadgeBase.GetRegionAttributes(null, placement).ShouldBeEmpty();
    }

    public sealed class CustomBadges : PublicationBadgeBase
    {
        [Parameter]
        public string DraftLabel { get; set; } = "Draft";

        [Parameter]
        public string ScheduledPrefix { get; set; } = "Scheduled on";

        [Parameter]
        public string DateFormat { get; set; } = "yyyy-MM-dd";

        [Parameter]
        public CultureInfo Culture { get; set; } = CultureInfo.InvariantCulture;

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            foreach (var badge in Badges)
            {
                var label = badge.ScheduledDate is { } date
                    ? $"{ScheduledPrefix} {date.ToString(DateFormat, Culture)}"
                    : DraftLabel;
                builder.OpenElement(0, "strong");
                builder.AddMultipleAttributes(1, badge.Attributes);
                builder.AddContent(2, badge.RenderContent(label));
                builder.CloseElement();
            }
        }
    }

    public sealed class UndeliveredBadges : PublicationBadgeBase
    {
        [Microsoft.AspNetCore.Components.Parameter]
        public bool Lookalike { get; set; }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            if (!Lookalike)
            {
                return;
            }
            foreach (var badge in Badges)
            {
                builder.OpenElement(0, "span");
                builder.AddMultipleAttributes(1, badge.Attributes);
                builder.AddContent(2, "Lookalike label");
                builder.CloseElement();
            }
        }
    }
}
