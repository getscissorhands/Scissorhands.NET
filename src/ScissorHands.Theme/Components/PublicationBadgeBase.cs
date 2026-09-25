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
    /// Gets the prepared badges. Render each badge's Attributes and encoded Content fragment.
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
            badges.Add(CreateBadge("draft", "Draft", status.Route));
        }
        if (status.ScheduledDate is { } date)
        {
            badges.Add(CreateBadge("scheduled", $"Scheduled on {date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}", status.Route));
        }
        Badges = badges.AsReadOnly();
    }

    private Badge CreateBadge(string kind, string text, string route)
    {
        var placement = Placement;
        var receipt = Receipt;
        return new Badge(kind, text, new ReadOnlyDictionary<string, object>(new Dictionary<string, object>
        {
            ["data-publication-badge"] = kind,
            ["data-publication-route"] = route,
            ["data-publication-placement"] = placement == PublicationBadgePlacement.Listing ? "listing" : "detail",
        }), builder =>
        {
            builder.AddContent(0, text);
            receipt?.Record(route, placement, kind);
        });
    }

    /// <summary>
    /// Describes one encoded status label and its required structural attributes.
    /// </summary>
    public sealed class Badge
    {
        internal Badge(string kind, string text, IReadOnlyDictionary<string, object> attributes, RenderFragment content)
        {
            Kind = kind;
            Text = text;
            Attributes = attributes;
            Content = content;
        }

        public string Kind { get; }
        public string Text { get; }
        public IReadOnlyDictionary<string, object> Attributes { get; }
        public RenderFragment Content { get; }
    }

    /// <summary>
    /// Records delivery of each route's encoded fragments, not inheritance or lookalike markup.
    /// </summary>
    public sealed class RenderReceipt
    {
        private readonly HashSet<(string Route, PublicationBadgePlacement Placement, string Kind)> _rendered = [];

        public bool WasRendered(string route, PublicationBadgePlacement placement, string kind) =>
            _rendered.Contains((route, placement, kind));

        internal void Record(string route, PublicationBadgePlacement placement, string kind) =>
            _rendered.Add((route, placement, kind));
    }
}
