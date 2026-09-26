using System.Globalization;

using AngleSharp.Html.Parser;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;
using ScissorHands.Core.Services;
using ScissorHands.Theme;
using ScissorHands.Theme.Components;
using ScissorHands.Web.Publication;
using ScissorHands.Web.Renderers;

namespace ScissorHands.Web.Tests.Renderers;

public class ComponentRendererPublicationTests
{
    [Fact]
    public async Task Given_CustomThemeOwnedBadgeMarkup_When_Rendered_Then_It_Should_SatisfyThePublicationContract()
    {
        using var fixture = new Fixture();
        var document = Prepared(scheduled: true);

        var rendered = await fixture.Renderer.RenderAsync<CustomView>(
            typeof(MainLayout), fixture.Parameters(document), Xunit.TestContext.Current.CancellationToken);

        using var html = new HtmlParser().ParseDocument(rendered);
        html.QuerySelectorAll("article .custom-status em").Select(badge => badge.TextContent)
            .ShouldBe(["Entwurf", "Geplant am 02.01.2099"]);
        html.QuerySelector("[data-publication-badge='scheduled']")!.GetAttribute("data-publication-date").ShouldBe("2099-01-02");
        html.QuerySelector("article > :first-child")!.ClassName.ShouldBe("custom-status");
        html.QuerySelector("article h1")!.TextContent.ShouldBe("Content");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Given_InheritanceOrSourceLookalikeWithoutReceipt_When_RenderAsync_Invoked_Then_It_Should_RejectMissingDelivery(bool lookalike)
    {
        // Arrange
        using var fixture = new Fixture();
        var document = Prepared();
        document.Html = lookalike
            ? "<article data-publication-content='post'><span data-publication-badge='draft' data-publication-route='post' data-publication-placement='detail'>Draft</span></article>"
            : "<article><h1>No badges</h1></article>";

        // Act
        var exception = await Should.ThrowAsync<InvalidDataException>(() => fixture.Renderer.RenderAsync<LookalikeView>(
            typeof(MainLayout), fixture.Parameters(document), Xunit.TestContext.Current.CancellationToken));

        // Assert
        exception.Message.ShouldContain("post");
        exception.Message.ShouldContain("'draft'");
        exception.Message.ShouldContain("PublicationBadgeBase");
        exception.Message.ShouldContain("lookalike");
    }

    [Fact]
    public async Task Given_OneOfTwoFragmentsOmitted_When_RenderAsync_Invoked_Then_It_Should_RejectMissingScheduledReceipt()
    {
        // Arrange
        using var fixture = new Fixture();
        var document = Prepared(scheduled: true);

        // Act
        var exception = await Should.ThrowAsync<InvalidDataException>(() => fixture.Renderer.RenderAsync<PartialView>(
            typeof(MainLayout), fixture.Parameters(document), Xunit.TestContext.Current.CancellationToken));

        // Assert
        exception.Message.ShouldContain("'scheduled'");
        exception.Message.ShouldContain("post");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Given_ListEntryWithoutReceipt_When_RenderAsync_Invoked_Then_It_Should_RejectEachMissingEntry(bool tag)
    {
        // Arrange
        using var fixture = new Fixture();
        var parameters = fixture.Parameters(new ContentDocument());
        parameters[tag ? "TaggedPages" : "Documents"] = new[] { Prepared() };

        // Act
        var exception = tag
            ? await Should.ThrowAsync<InvalidDataException>(() => fixture.Renderer.RenderAsync<EmptyTagView>(
                typeof(MainLayout), parameters, Xunit.TestContext.Current.CancellationToken))
            : await Should.ThrowAsync<InvalidDataException>(() => fixture.Renderer.RenderAsync<EmptyIndexView>(
                typeof(MainLayout), parameters, Xunit.TestContext.Current.CancellationToken));

        // Assert
        exception.Message.ShouldContain("post");
        exception.Message.ShouldContain("Listing");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Given_UnflaggedLegacyTheme_When_RenderAsync_Invoked_Then_It_Should_RemainCompatible(bool preview)
    {
        // Arrange
        using var fixture = new Fixture();
        var document = new ContentDocument { Html = "<h1>Legacy theme</h1>" };
        var parameters = fixture.Parameters(document, preview);

        // Act
        var html = await fixture.Renderer.RenderAsync<LookalikeView>(
            typeof(MainLayout), parameters, Xunit.TestContext.Current.CancellationToken);

        // Assert
        html.ShouldContain("Legacy theme");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Given_RenderedBadgesRemovedAfterRendering_When_Validate_Invoked_Then_It_Should_RejectPostHtmlPluginRemoval(bool listing)
    {
        // Arrange
        using var fixture = new Fixture();
        var document = Prepared(scheduled: true);
        var parameters = fixture.Parameters(listing ? new ContentDocument() : document);
        parameters["Documents"] = new[] { document };
        var html = listing
            ? await fixture.Renderer.RenderAsync<IndexView>(typeof(MainLayout), parameters, Xunit.TestContext.Current.CancellationToken)
            : await fixture.Renderer.RenderAsync<PostView>(typeof(MainLayout), parameters, Xunit.TestContext.Current.CancellationToken);
        using var parsed = new HtmlParser().ParseDocument(html);
        parsed.QuerySelector("[data-publication-badge='scheduled']")!.Remove();
        var postHookHtml = parsed.DocumentElement.OuterHtml;

        // Act
        var exception = Should.Throw<InvalidDataException>(() => PublicationBadgeValidator.Validate(
            postHookHtml, [document], listing, true, "output", Xunit.TestContext.Current.CancellationToken));

        // Assert
        exception.Message.ShouldContain("scheduled");
        exception.Message.ShouldContain("post-HTML plugins");
    }

    [Fact]
    public async Task Given_SourceBadgeInBuild_When_RenderAsync_Invoked_Then_It_Should_RejectUnexpectedStatusMarkup()
    {
        // Arrange
        using var fixture = new Fixture();
        var document = new ContentDocument { Html = "<span data-publication-badge='draft'>Draft</span>" };

        // Act
        var exception = await Should.ThrowAsync<InvalidDataException>(() => fixture.Renderer.RenderAsync<PostView>(
            typeof(MainLayout), fixture.Parameters(document, false), Xunit.TestContext.Current.CancellationToken));

        // Assert
        exception.Message.ShouldContain("Unexpected");
    }

    private static ContentDocument Prepared(bool scheduled = false) => new()
    {
        Metadata = new ContentMetadata { Slug = "post", Title = "Post" },
        Html = "<h1>Content</h1>",
        PublicationStatus = new PublicationStatus
        {
            Route = "post",
            IsDraft = true,
            ScheduledDate = scheduled ? new DateOnly(2099, 1, 2) : null,
        },
    };

    private sealed class Fixture : IDisposable
    {
        private readonly ServiceProvider _provider;

        public Fixture()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton(Substitute.For<IThemeService>());
            _provider = services.BuildServiceProvider();
            Renderer = new ComponentRenderer(
                _provider.GetRequiredService<IServiceScopeFactory>(), _provider.GetRequiredService<ILoggerFactory>());
        }

        public ComponentRenderer Renderer { get; }

        public Dictionary<string, object?> Parameters(ContentDocument document, bool preview = true) => new()
        {
            ["Site"] = new SiteManifest { IsPreview = preview },
            ["Theme"] = new ThemeManifest(),
            ["Document"] = document,
        };

        public void Dispose() => _provider.Dispose();
    }

    private sealed class LookalikeView : PageViewBase
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenComponent<EmptyBadges>(0);
            builder.CloseComponent();
            builder.AddMarkupContent(1, Document?.Html);
        }
    }

    private sealed class EmptyBadges : PublicationBadgeBase;
    private sealed class EmptyTagView : TagViewBase;
    private sealed class EmptyIndexView : IndexViewBase;

    private sealed class CustomView : PageViewBase
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "article");
            builder.AddMultipleAttributes(1, PublicationBadgeBase.GetRegionAttributes(Document, PublicationBadgePlacement.Detail));
            builder.OpenComponent<CustomBadges>(2);
            builder.CloseComponent();
            builder.AddMarkupContent(3, Document?.Html);
            builder.CloseElement();
        }
    }

    private sealed class CustomBadges : PublicationBadgeBase
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "section");
            builder.AddAttribute(1, "class", "custom-status");
            foreach (var badge in Badges)
            {
                var label = badge.ScheduledDate is { } date
                    ? $"Geplant am {date.ToString("dd/MM/yyyy", CultureInfo.GetCultureInfo("de-DE"))}"
                    : "Entwurf";
                builder.OpenElement(2, "em");
                builder.AddMultipleAttributes(3, badge.Attributes);
                builder.AddContent(4, badge.RenderContent(label));
                builder.CloseElement();
            }
            builder.CloseElement();
        }
    }

    private sealed class PartialView : PageViewBase
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenComponent<PartialBadges>(0);
            builder.CloseComponent();
        }
    }

    private sealed class PartialBadges : PublicationBadgeBase
    {
        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            foreach (var badge in Badges.Where(badge => badge.Kind == "draft"))
            {
                builder.OpenElement(0, "span");
                builder.AddMultipleAttributes(1, badge.Attributes);
                builder.AddContent(2, badge.RenderContent("Draft"));
                builder.CloseElement();
            }
        }
    }
}
