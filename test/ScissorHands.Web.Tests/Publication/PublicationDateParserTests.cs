using System.Globalization;

using ScissorHands.Web.Publication;

namespace ScissorHands.Web.Tests.Publication;

[Collection("NonParallel")]
public class PublicationDateParserTests
{
    private const string SourcePath = "contents/posts/scheduled.md";

    [Theory]
    [InlineData("UTC", 0, 0)]
    [InlineData("Asia/Seoul", 9, 9)]
    [InlineData("America/New_York", -5, -4)]
    public void Given_SupportedTimeZone_When_ResolveTimeZone_Invoked_Then_It_Should_ResolvePlatformIndependentSeasonalOffsets(
        string id, int winterHours, int summerHours)
    {
        // Arrange
        var winter = new DateTime(2026, 1, 15, 12, 0, 0, DateTimeKind.Unspecified);
        var summer = new DateTime(2026, 7, 15, 12, 0, 0, DateTimeKind.Unspecified);

        // Act
        var timeZone = PublicationDateParser.ResolveTimeZone(id);

        // Assert
        timeZone.GetUtcOffset(winter).ShouldBe(TimeSpan.FromHours(winterHours));
        timeZone.GetUtcOffset(summer).ShouldBe(TimeSpan.FromHours(summerHours));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t\r\n")]
    [InlineData("Unknown/Zone")]
    [InlineData(" Asia/Seoul ")]
    public void Given_InvalidTimeZone_When_ResolveTimeZone_Invoked_Then_It_Should_RejectWithConfigurationGuidance(string? id)
    {
        // Arrange

        // Act
        var exception = Should.Throw<InvalidDataException>(() => PublicationDateParser.ResolveTimeZone(id));

        // Assert
        exception.Message.ShouldContain("Site:TimeZone");
        exception.Message.ShouldContain("UTC");
        exception.Message.ShouldContain("Asia/Seoul");
        exception.Message.ShouldContain("America/New_York");
    }

    [Theory]
    [InlineData("2026-09-25", "UTC", "2026-09-25T00:00:00.0000000+00:00")]
    [InlineData("2026-09-25", "Asia/Seoul", "2026-09-25T00:00:00.0000000+09:00")]
    [InlineData("2026-01-15", "America/New_York", "2026-01-15T00:00:00.0000000-05:00")]
    [InlineData("2026-07-15", "America/New_York", "2026-07-15T00:00:00.0000000-04:00")]
    [InlineData("2026-09-25T12:34:56", "Asia/Seoul", "2026-09-25T12:34:56.0000000+09:00")]
    [InlineData("2026-09-25T12:34", "Asia/Seoul", "2026-09-25T12:34:00.0000000+09:00")]
    [InlineData("2026-09-25 12:34:56", "Asia/Seoul", "2026-09-25T12:34:56.0000000+09:00")]
    [InlineData("2026-09-25T12:34:56.1234567", "Asia/Seoul", "2026-09-25T12:34:56.1234567+09:00")]
    [InlineData(" 2026-09-25 12:34:56.123 ", "Asia/Seoul", "2026-09-25T12:34:56.1230000+09:00")]
    [InlineData("2026-01-15T12:34:56", "America/New_York", "2026-01-15T12:34:56.0000000-05:00")]
    [InlineData("2026-07-15T12:34:56", "America/New_York", "2026-07-15T12:34:56.0000000-04:00")]
    [InlineData("09/25/2026 12:34:56", "Asia/Seoul", "2026-09-25T12:34:56.0000000+09:00")]
    [InlineData("2026/09/25 12:34:56", "Asia/Seoul", "2026-09-25T12:34:56.0000000+09:00")]
    [InlineData("September 25, 2026 12:34:56", "Asia/Seoul", "2026-09-25T12:34:56.0000000+09:00")]
    [InlineData("25 Sep 2026 12:34:56", "Asia/Seoul", "2026-09-25T12:34:56.0000000+09:00")]
    [InlineData("25-Sep-2026 12:34:56", "Asia/Seoul", "2026-09-25T12:34:56.0000000+09:00")]
    [InlineData("2024-02-29", "UTC", "2024-02-29T00:00:00.0000000+00:00")]
    public void Given_OffsetFreeCompleteDate_When_Parse_Invoked_Then_It_Should_UseConfiguredTimeZoneAndPreserveCalendarFields(
        string value, string id, string expected)
    {
        // Arrange
        var timeZone = PublicationDateParser.ResolveTimeZone(id);

        // Act
        var result = PublicationDateParser.Parse(value, timeZone, SourcePath);

        // Assert
        result.ToString("O", CultureInfo.InvariantCulture).ShouldBe(expected);
    }

    [Theory]
    [InlineData("2026-09-25T00:15:00Z", "2026-09-25T00:15:00.0000000+00:00")]
    [InlineData("2026-09-25T00:15:00.1234567Z", "2026-09-25T00:15:00.1234567+00:00")]
    [InlineData("2026-09-25T00:15:00+09:00", "2026-09-25T00:15:00.0000000+09:00")]
    [InlineData("2026-09-25T00:15:00+0900", "2026-09-25T00:15:00.0000000+09:00")]
    [InlineData("2026-09-25T00:15:00+09", "2026-09-25T00:15:00.0000000+09:00")]
    [InlineData("2026-09-25T23:15:00-04:00", "2026-09-25T23:15:00.0000000-04:00")]
    [InlineData("2026-09-25T23:15:00-0400", "2026-09-25T23:15:00.0000000-04:00")]
    [InlineData("2026-09-25T23:15:00-04", "2026-09-25T23:15:00.0000000-04:00")]
    [InlineData("2026-09-25T00:15:00.123+05:30", "2026-09-25T00:15:00.1230000+05:30")]
    [InlineData("2026-09-25T00:15:00.1234567-0330", "2026-09-25T00:15:00.1234567-03:30")]
    [InlineData("2026-09-25T00:15:00+00:00", "2026-09-25T00:15:00.0000000+00:00")]
    [InlineData("2026-09-25T00:15:00-00:00", "2026-09-25T00:15:00.0000000+00:00")]
    [InlineData("2026-09-25T00:15:00+0000", "2026-09-25T00:15:00.0000000+00:00")]
    [InlineData("2026-09-25T00:15:00-0000", "2026-09-25T00:15:00.0000000+00:00")]
    [InlineData("2026-09-25T00:15:00+00", "2026-09-25T00:15:00.0000000+00:00")]
    [InlineData("2026-09-25T00:15:00-00", "2026-09-25T00:15:00.0000000+00:00")]
    [InlineData("Fri, 25 Sep 2026 12:34:56 GMT", "2026-09-25T12:34:56.0000000+00:00")]
    [InlineData("25 Sep 2026 12:34:56 +0900", "2026-09-25T12:34:56.0000000+09:00")]
    public void Given_ExplicitOffset_When_Parse_Invoked_Then_It_Should_PreserveAuthoredDateAndOffset(
        string value, string expected)
    {
        // Arrange
        var timeZone = PublicationDateParser.ResolveTimeZone("America/New_York");

        // Act
        var result = PublicationDateParser.Parse(value, timeZone, SourcePath);

        // Assert
        result.ToString("O", CultureInfo.InvariantCulture).ShouldBe(expected);
    }

    [Theory]
    [InlineData("2026-03-08T02:00:00", "does not exist")]
    [InlineData("2026-03-08T02:30:00", "does not exist")]
    [InlineData("2026-03-08T02:59:59.9999999", "does not exist")]
    [InlineData("2026-11-01T01:00:00", "ambiguous")]
    [InlineData("2026-11-01T01:30:00", "ambiguous")]
    [InlineData("2026-11-01T01:59:59.9999999", "ambiguous")]
    public void Given_GapOrOverlapLocalTime_When_Parse_Invoked_Then_It_Should_RequireExplicitOffset(
        string value, string reason)
    {
        // Arrange
        var timeZone = PublicationDateParser.ResolveTimeZone("America/New_York");

        // Act
        var exception = Should.Throw<InvalidDataException>(() => PublicationDateParser.Parse(value, timeZone, SourcePath));

        // Assert
        exception.Message.ShouldContain("published");
        exception.Message.ShouldContain(SourcePath);
        exception.Message.ShouldContain(value);
        exception.Message.ShouldContain("America/New_York");
        exception.Message.ShouldContain(reason);
        exception.Message.ShouldContain("explicit offset");
    }

    [Theory]
    [InlineData("2026-03-08T01:59:59", -5)]
    [InlineData("2026-03-08T03:00:00", -4)]
    [InlineData("2026-11-01T00:59:59", -4)]
    [InlineData("2026-11-01T02:00:00", -5)]
    [InlineData("2026-03-08T02:30:00-05:00", -5)]
    [InlineData("2026-03-08T02:30:00Z", 0)]
    [InlineData("2026-11-01T01:30:00-04:00", -4)]
    [InlineData("2026-11-01T01:30:00-05:00", -5)]
    [InlineData("2026-11-01T01:30:00+00:00", 0)]
    public void Given_ValidTransitionBoundaryOrExplicitOffset_When_Parse_Invoked_Then_It_Should_ReturnSpecifiedInstant(
        string value, int expectedOffsetHours)
    {
        // Arrange
        var timeZone = PublicationDateParser.ResolveTimeZone("America/New_York");

        // Act
        var result = PublicationDateParser.Parse(value, timeZone, SourcePath);

        // Assert
        result.Offset.ShouldBe(TimeSpan.FromHours(expectedOffsetHours));
        result.DateTime.ToString("yyyy-MM-dd'T'HH:mm:ss", CultureInfo.InvariantCulture).ShouldBe(value[..19]);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" \t\r\n")]
    [InlineData("not a date")]
    [InlineData("2026-02-29")]
    [InlineData("2026-09-31")]
    [InlineData("2026-13-01")]
    [InlineData("2026-09-25T24:00:00")]
    [InlineData("2026-09-25T12:60:00")]
    [InlineData("2026-09-25T12:00:60")]
    [InlineData("2026-09-25T12:00:00+14:01")]
    [InlineData("2026-09-25T12:00:00-15:00")]
    [InlineData("2026-09-25T12:00:00+09:99")]
    [InlineData("2026-09-25T12:00:00+09:00junk")]
    [InlineData("2026-09-25T12:00:00 America/New_York")]
    [InlineData("2026-09-25T12:00:00Z+09:00")]
    [InlineData("2026-09-25T12:00:00+")]
    [InlineData("2026-09-25 trailing text")]
    [InlineData("2026-09")]
    [InlineData("09/25")]
    [InlineData("September 25")]
    [InlineData("September 2026")]
    [InlineData("12:34:56")]
    [InlineData("12:34:56Z")]
    [InlineData("09/25/26")]
    [InlineData("25/09/2026")]
    [InlineData("25 septembre 2026")]
    [InlineData("0000-01-01")]
    [InlineData("10000-01-01")]
    [InlineData("0001-01-01T00:00:00+00:01")]
    [InlineData("9999-12-31T23:59:59.9999999-00:01")]
    public void Given_InvalidOrIncompleteDate_When_Parse_Invoked_Then_It_Should_RejectWithSourceAndFormatGuidance(string? value)
    {
        // Arrange
        var timeZone = TimeZoneInfo.Utc;

        // Act
        var exception = Should.Throw<InvalidDataException>(() => PublicationDateParser.Parse(value, timeZone, SourcePath));

        // Assert
        exception.Message.ShouldContain("published");
        exception.Message.ShouldContain(SourcePath);
        exception.Message.ShouldContain("complete date");
        exception.Message.ShouldContain("explicit offset");
    }

    [Theory]
    [InlineData("0001-01-01T00:00:00", 1)]
    [InlineData("9999-12-31T23:59:59.9999999", -1)]
    public void Given_ZoneOffsetCausingUtcOverflow_When_Parse_Invoked_Then_It_Should_RejectWithSourceContext(
        string value, int offsetHours)
    {
        // Arrange
        var timeZone = TimeZoneInfo.CreateCustomTimeZone("Fixed", TimeSpan.FromHours(offsetHours), "Fixed", "Fixed");

        // Act
        var exception = Should.Throw<InvalidDataException>(() => PublicationDateParser.Parse(value, timeZone, SourcePath));

        // Assert
        exception.Message.ShouldContain("published");
        exception.Message.ShouldContain(SourcePath);
        exception.Message.ShouldContain("supported range");
        exception.Message.ShouldContain("Fixed");
        exception.InnerException.ShouldBeAssignableTo<ArgumentException>();
    }

    [Theory]
    [InlineData("0001-01-01", "0001-01-01T00:00:00.0000000+00:00")]
    [InlineData("9999-12-31T23:59:59.9999999", "9999-12-31T23:59:59.9999999+00:00")]
    [InlineData("0001-01-01T14:00:00+14:00", "0001-01-01T14:00:00.0000000+14:00")]
    [InlineData("9999-12-31T09:59:59.9999999-14:00", "9999-12-31T09:59:59.9999999-14:00")]
    public void Given_RepresentableRangeBoundary_When_Parse_Invoked_Then_It_Should_PreserveFullPrecision(
        string value, string expected)
    {
        // Arrange
        var timeZone = TimeZoneInfo.Utc;

        // Act
        var result = PublicationDateParser.Parse(value, timeZone, SourcePath);

        // Assert
        result.ToString("O", CultureInfo.InvariantCulture).ShouldBe(expected);
    }

    [Theory]
    [InlineData("en-US")]
    [InlineData("fr-FR")]
    [InlineData("ko-KR")]
    [InlineData("ar-SA")]
    public void Given_NonInvariantCurrentCulture_When_Parse_Invoked_Then_It_Should_UseInvariantCalendarAndSyntax(string cultureName)
    {
        // Arrange
        var originalCulture = CultureInfo.CurrentCulture;
        var timeZone = PublicationDateParser.ResolveTimeZone("Asia/Seoul");

        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(cultureName);

            // Act
            var result = PublicationDateParser.Parse("09/25/2026 12:34:56", timeZone, SourcePath);
            Action parseLocalizedDate = () => PublicationDateParser.Parse("25 septembre 2026", timeZone, SourcePath);

            // Assert
            result.ToString("O", CultureInfo.InvariantCulture).ShouldBe("2026-09-25T12:34:56.0000000+09:00");
            Should.Throw<InvalidDataException>(parseLocalizedDate);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }
}
