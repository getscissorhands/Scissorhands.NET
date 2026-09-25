namespace ScissorHands.Core.Models;

/// <summary>
/// Represents preview-only publication status prepared before document hooks.
/// </summary>
public sealed record PublicationStatus
{
    /// <summary>
    /// Gets the original generated route identifying this document's badges.
    /// </summary>
    public string Route { get; init; } = string.Empty;

    /// <summary>
    /// Gets whether this document or its primary is a draft.
    /// </summary>
    public bool IsDraft { get; init; }

    /// <summary>
    /// Gets the authored calendar date when this post or its primary is future-scheduled.
    /// </summary>
    public DateOnly? ScheduledDate { get; init; }

    /// <summary>
    /// Gets whether this post requires a scheduled badge.
    /// </summary>
    public bool IsScheduled => ScheduledDate.HasValue;

    /// <summary>
    /// Gets whether any publication badges are required.
    /// </summary>
    public bool HasBadges => IsDraft || IsScheduled;
}
