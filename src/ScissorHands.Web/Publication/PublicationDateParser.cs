using System.Globalization;
using System.Text.RegularExpressions;

namespace ScissorHands.Web.Publication;

/// <summary>
/// Interprets authored publication dates without consulting the machine's local time zone.
/// </summary>
internal static partial class PublicationDateParser
{
    /// <summary>
    /// Resolves an explicitly configured time zone. The manifest supplies the omitted-setting default.
    /// </summary>
    public static TimeZoneInfo ResolveTimeZone(string? id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new InvalidDataException(
                "Site:TimeZone must not be null, empty, or whitespace. Use 'UTC' or a time zone ID such as 'Asia/Seoul' or 'America/New_York'.");
        }

        try
        {
            // .NET supports IANA IDs on both Windows and Unix when the platform time zone data is available.
            return TimeZoneInfo.FindSystemTimeZoneById(id);
        }
        catch (Exception exception) when (exception is TimeZoneNotFoundException or InvalidTimeZoneException)
        {
            throw new InvalidDataException(
                $"Site:TimeZone '{id}' could not be resolved. Use 'UTC' or a valid time zone ID such as 'Asia/Seoul' or 'America/New_York', and ensure the system time zone data is installed.",
                exception);
        }
    }

    /// <summary>
    /// Parses a complete date, preserving explicit offsets and interpreting other values in the configured zone.
    /// </summary>
    public static DateTimeOffset Parse(string? value, TimeZoneInfo timeZone, string sourcePath)
    {
        ArgumentNullException.ThrowIfNull(timeZone);

        if (string.IsNullOrWhiteSpace(value))
        {
            throw InvalidDate(value, sourcePath, "A complete publication date is required.");
        }

        // Keep the invariant parser's full-date compatibility, but do not let it fill in the
        // current date/year for partial dates or time-only values.
        var date = CompleteDateRegex().Match(value);
        if (!date.Success
            || !DateTimeOffset.TryParse(
                value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeUniversal,
                out var parsed))
        {
            throw InvalidDate(value, sourcePath, "The value is not a valid complete date or date/time.");
        }

        // Date separators belong to the complete date, not to an offset. Inspect only the
        // remaining text, also supporting the invariant parser's time-before-date forms.
        if (parsed.Offset != TimeSpan.Zero
            || ExplicitOffsetRegex().IsMatch(value.AsSpan(0, date.Index))
            || ExplicitOffsetRegex().IsMatch(value.AsSpan(date.Index + date.Length)))
        {
            return parsed;
        }

        var local = DateTime.SpecifyKind(parsed.DateTime, DateTimeKind.Unspecified);
        if (timeZone.IsInvalidTime(local))
        {
            throw InvalidDate(value, sourcePath, $"The local time does not exist in Site:TimeZone '{timeZone.Id}' because of a clock change.");
        }

        if (timeZone.IsAmbiguousTime(local))
        {
            throw InvalidDate(value, sourcePath, $"The local time is ambiguous in Site:TimeZone '{timeZone.Id}' because of a clock change.");
        }

        try
        {
            return new DateTimeOffset(local, timeZone.GetUtcOffset(local));
        }
        catch (ArgumentException exception)
        {
            throw InvalidDate(value, sourcePath, $"The date is outside the supported range in Site:TimeZone '{timeZone.Id}'.", exception);
        }
    }

    private static InvalidDataException InvalidDate(string? value, string sourcePath, string reason, Exception? innerException = null)
    {
        return new InvalidDataException(
            $"Invalid 'published' value '{value ?? "<null>"}' in '{sourcePath}'. {reason} Use a complete date such as '2026-09-25', or a date/time with an explicit offset such as '2026-09-25T09:00:00+09:00' or '2026-09-25T00:00:00Z'.",
            innerException);
    }

    // A four-digit year avoids culture/calendar-dependent two-digit-year interpretation.
    // This is only a completeness guard: TryParse remains responsible for validating the
    // calendar date and accepting the original invariant date/time syntax.
    [GeneratedRegex(
        @"(?<![0-9])(?:[0-9]{4}\s*[-/.]\s*[0-9]{1,2}\s*[-/.]\s*[0-9]{1,2}|[0-9]{1,2}\s*[-/.]\s*[0-9]{1,2}\s*[-/.]\s*[0-9]{4}|[0-9]{1,2}[\s/-]+[A-Za-z]+\.?[\s,/-]+[0-9]{4}|[A-Za-z]+\.?[\s/-]+[0-9]{1,2}[\s,/-]+[0-9]{4}|[0-9]{4}[\s/-]+[A-Za-z]+\.?[\s,/-]+[0-9]{1,2})(?![0-9])",
        RegexOptions.CultureInvariant)]
    private static partial Regex CompleteDateRegex();

    [GeneratedRegex(@"(?<![A-Za-z])(?:Z|GMT)(?![A-Za-z])|[+-]\s*[0-9]", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase)]
    private static partial Regex ExplicitOffsetRegex();
}
