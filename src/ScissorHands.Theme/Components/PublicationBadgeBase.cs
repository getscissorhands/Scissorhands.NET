using System.Collections.ObjectModel;
using System.Globalization;

using Microsoft.AspNetCore.Components;

using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;

namespace ScissorHands.Theme.Components;

/// <summary>
/// Identifies the theme-owned region in which publication status is displayed.
/// </summary>
public enum PublicationBadgePlacement
{
    Detail,
    Listing,
}

/// <summary>
/// Supplies preview publication badges without prescribing theme markup or styles.
/// </summary>
public abstract class PublicationBadgeBase : ComponentBase
{
    [CascadingParameter]
    public ContentDocument? Document { get; set; }

    [CascadingParameter]
    public SiteManifest? Site { get; set; }

    [CascadingParameter]
    public RenderReceipt? Receipt { get; set; }

    /// <summary>
    /// Gets or sets an explicit listing document, overriding the cascading detail document.
    /// </summary>
    [Parameter]
    public ContentDocument? Content { get; set; }

    [Parameter]
    public PublicationBadgePlacement Placement { get; set; }

    /// <summary>
    /// Gets the prepared status data. Supply theme-owned text to each badge's RenderContent helper.
    /// </summary>
    protected IReadOnlyList<Badge> Badges { get; private set; } = [];

    /// <summary>
    /// Gets attributes for the article containing detail content, or the individual listing entry.
    /// Regions belong inside main content, outside navigation. Detail badges precede article content.
    /// </summary>
    public static IReadOnlyDictionary<string, object> GetRegionAttributes(
        ContentDocument? document, PublicationBadgePlacement placement)
    {
        if (document?.PublicationStatus.HasBadges != true)
        {
            return ReadOnlyDictionary<string, object>.Empty;
        }

        return new ReadOnlyDictionary<string, object>(new Dictionary<string, object>
        {
            [placement == PublicationBadgePlacement.Listing ? "data-publication-entry" : "data-publication-content"]
                = document.PublicationStatus.Route,
        });
    }

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        var status = (Content ?? Document)?.PublicationStatus;
        if (Site?.IsPreview != true || status?.HasBadges != true)
        {
            Badges = [];
            return;
        }

        var badges = new List<Badge>(2);
        if (status.IsDraft)
        {
            badges.Add(CreateBadge("draft", status.Route));
        }
        if (status.ScheduledDate is { } date)
        {
            badges.Add(CreateBadge("scheduled", status.Route, date));
        }
        Badges = badges.AsReadOnly();
    }

    private Badge CreateBadge(string kind, string route, DateOnly? scheduledDate = null)
    {
        var placement = Placement;
        var receipt = Receipt;
        var attributes = new Dictionary<string, object>
        {
            ["data-publication-badge"] = kind,
            ["data-publication-route"] = route,
            ["data-publication-placement"] = placement == PublicationBadgePlacement.Listing ? "listing" : "detail",
        };
        if (scheduledDate is { } date)
        {
            attributes["data-publication-date"] = date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }
        return new Badge(kind, scheduledDate, new ReadOnlyDictionary<string, object>(attributes), text => builder =>
        {
            builder.AddContent(0, text);
            receipt?.Record(route, placement, kind, text);
        });
    }

    /// <summary>
    /// Describes one status, its authored date, and its required machine-readable attributes.
    /// </summary>
    public sealed class Badge
    {
        private readonly Func<string, RenderFragment> _renderContent;

        internal Badge(string kind, DateOnly? scheduledDate, IReadOnlyDictionary<string, object> attributes,
            Func<string, RenderFragment> renderContent)
        {
            Kind = kind;
            ScheduledDate = scheduledDate;
            Attributes = attributes;
            _renderContent = renderContent;
        }

        public string Kind { get; }
        public DateOnly? ScheduledDate { get; }
        public IReadOnlyDictionary<string, object> Attributes { get; }

        /// <summary>
        /// Encodes a theme-owned label and records its delivery without prescribing wording or culture.
        /// </summary>
        public RenderFragment RenderContent(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentException($"A nonempty theme label is required for the '{Kind}' publication badge.", nameof(text));
            }
            return _renderContent(text);
        }
    }

    /// <summary>
    /// Records delivery of each route's encoded fragments, not inheritance or lookalike markup.
    /// </summary>
    public sealed class RenderReceipt
    {
        private readonly Dictionary<(string Route, PublicationBadgePlacement Placement, string Kind), string> _rendered = [];

        public bool WasRendered(string route, PublicationBadgePlacement placement, string kind) =>
            _rendered.ContainsKey((route, placement, kind));

        public string? GetRenderedText(string route, PublicationBadgePlacement placement, string kind) =>
            _rendered.GetValueOrDefault((route, placement, kind));

        internal void Record(string route, PublicationBadgePlacement placement, string kind, string text) =>
            _rendered[(route, placement, kind)] = text;
    }
}
