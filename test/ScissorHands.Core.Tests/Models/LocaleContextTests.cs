using System.Runtime.CompilerServices;

using ScissorHands.Core.Models;

namespace ScissorHands.Core.Tests.Models;

public class LocaleContextTests
{
    [Fact]
    public void Given_DefaultContext_When_Construct_Invoked_Then_It_Should_HaveUnlocalizedDefaults()
    {
        // Arrange

        // Act
        var context = new LocaleContext();

        // Assert
        context.Locale.ShouldBeEmpty();
        context.Route.ShouldBeEmpty();
        context.HomeUrl.ShouldBe(".");
        context.TagIndexUrl.ShouldBeNull();
        context.SwitchLanguageUrls.ShouldBeEmpty();
        context.ShouldBe(new LocaleContext());
    }

    [Theory]
    [InlineData(nameof(LocaleContext.Locale))]
    [InlineData(nameof(LocaleContext.Route))]
    [InlineData(nameof(LocaleContext.HomeUrl))]
    [InlineData(nameof(LocaleContext.TagIndexUrl))]
    public void Given_LocaleContextContract_When_GetProperty_Invoked_Then_It_Should_BeSealedAndInitOnly(string propertyName)
    {
        // Arrange
        var type = typeof(LocaleContext);

        // Act
        var property = type.GetProperty(propertyName);

        // Assert
        type.IsSealed.ShouldBeTrue();
        property.ShouldNotBeNull();
        property.PropertyType.ShouldBe(typeof(string));
        property.SetMethod.ShouldNotBeNull();
        property.SetMethod.ReturnParameter.GetRequiredCustomModifiers().ShouldContain(typeof(IsExternalInit));
    }

    [Fact]
    public void Given_PreparedContext_When_Copy_Invoked_Then_It_Should_LeaveTheOriginalSnapshotUnchanged()
    {
        // Arrange
        var original = new LocaleContext
        {
            Locale = "ko-kr",
            Route = "ko-kr/tags/C#",
            HomeUrl = "ko-kr/",
            TagIndexUrl = "ko-kr/tags",
        };

        // Act
        var copy = original with { Locale = "en-us", Route = "en-us", HomeUrl = "en-us/", TagIndexUrl = null };

        // Assert
        original.Locale.ShouldBe("ko-kr");
        original.Route.ShouldBe("ko-kr/tags/C#");
        original.HomeUrl.ShouldBe("ko-kr/");
        original.TagIndexUrl.ShouldBe("ko-kr/tags");
        copy.Locale.ShouldBe("en-us");
        copy.Route.ShouldBe("en-us");
        copy.HomeUrl.ShouldBe("en-us/");
        copy.TagIndexUrl.ShouldBeNull();
        copy.ShouldNotBe(original);
    }

    [Theory]
    [InlineData(".", " C# ", "tags/c%23")]
    [InlineData(".", " Topic/Subtopic ", "tags/topic%2Fsubtopic")]
    [InlineData("ko-kr/", " C# ", "ko-kr/tags/c%23")]
    [InlineData("ko-kr/", " Topic/Subtopic ", "ko-kr/tags/topic%2Fsubtopic")]
    [InlineData("fr%20ca/", "C#", "fr%20ca/tags/c%23")]
    [InlineData("fr%20ca/", "a%2Fb", "fr%20ca/tags/a%252fb")]
    public void Given_PreparedHomeAndRawTag_When_GetTagUrl_Invoked_Then_It_Should_PreserveHomeEscapingAndFormatTagOnce(
        string homeUrl, string tag, string expected)
    {
        // Arrange
        var context = new LocaleContext { HomeUrl = homeUrl };

        // Act
        var url = context.GetTagUrl(tag);

        // Assert
        url.ShouldBe(expected);
        new Uri(new Uri("https://example.com/site/"), url).AbsoluteUri.ShouldBe($"https://example.com/site/{expected}");
    }

    [Theory]
    [InlineData(".", null)]
    [InlineData("ko-kr/", null)]
    [InlineData(".", "")]
    [InlineData("ko-kr/", "")]
    [InlineData(".", " ")]
    [InlineData("ko-kr/", " ")]
    [InlineData(".", " . ")]
    [InlineData("ko-kr/", " . ")]
    [InlineData(".", "..")]
    [InlineData("ko-kr/", "..")]
    public void Given_InvalidRawTag_When_GetTagUrl_Invoked_Then_It_Should_PreserveSharedValidation(string homeUrl, string? tag)
    {
        // Arrange
        var context = new LocaleContext { HomeUrl = homeUrl };

        // Act
        var exception = Record.Exception(() => context.GetTagUrl(tag!));

        // Assert
        exception.ShouldBeOfType(tag is null ? typeof(ArgumentNullException) : typeof(ArgumentException));
        ((ArgumentException)exception).ParamName.ShouldBe("tag");
    }
}
