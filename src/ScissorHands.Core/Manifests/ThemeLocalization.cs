namespace ScissorHands.Core.Manifests;

/// <summary>
/// Represents immutable, plain-text theme messages for one locale.
/// Configured entries must explicitly supply every field.
/// </summary>
public sealed record ThemeLocalization
{
    /// <summary>
    /// Gets the notice shown when primary content replaces an unavailable translation.
    /// </summary>
    public string? TranslationUnavailable { get; init; }

    /// <summary>
    /// Gets the draft-status label.
    /// </summary>
    public string? Draft { get; init; }

    /// <summary>
    /// Gets the scheduled-status composite format, with the publication date at argument zero.
    /// </summary>
    public string? ScheduledOn { get; init; }

    /// <summary>
    /// Gets the English messages used only when localization is disabled.
    /// </summary>
    public static ThemeLocalization English { get; } = new()
    {
        TranslationUnavailable = "This page is not available in the requested language.",
        Draft = "Draft",
        ScheduledOn = "Scheduled on {0}",
    };
}
