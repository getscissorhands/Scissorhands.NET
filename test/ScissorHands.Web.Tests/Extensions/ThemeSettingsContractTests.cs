using System.Text;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using ScissorHands.Core.Manifests;
using ScissorHands.Web.Extensions;

namespace ScissorHands.Web.Tests.Extensions;

public class ThemeSettingsContractTests
{
    [Theory]
    [InlineData("{}")]
    [InlineData("""{"Theme":{}}""")]
    [InlineData("""{"Theme":{"HeroImages":null}}""")]
    [InlineData("""{"Theme":{"HeroImages":[]}}""")]
    public void Given_EmptyThemeConfiguration_When_Bound_Then_It_Should_ExposeEmptySettings(string json)
    {
        using var provider = Bind(json);

        var settings = provider.GetRequiredService<ThemeSettings>();
        settings.Localization.ShouldBeEmpty();
        settings.HeroImages.ShouldBeEmpty();
    }

    [Theory]
    [InlineData("""{"Source":"/images/hero.jpg","Alt":"A landscape"}""", "/images/hero.jpg", "A landscape")]
    [InlineData("""{"Source":"images/hero.jpg?v=1#top","Alt":"A landscape"}""", "images/hero.jpg?v=1#top", "A landscape")]
    [InlineData("""{"Source":"https://cdn.example.test/hero.jpg?v=1#top","Alt":""}""", "https://cdn.example.test/hero.jpg?v=1#top", "")]
    public void Given_OneHeroImage_When_Bound_Then_It_Should_ExposeImageAndAlt(string image, string source, string alt)
    {
        using var provider = Bind("{\"Theme\":{\"HeroImages\":[" + image + "]}}");

        provider.GetRequiredService<ThemeSettings>().HeroImages.ShouldBe(
            [new ThemeHeroImage { Source = source, Alt = alt }]);
    }

    [Fact]
    public void Given_LocalizationAndSeveralImages_When_Bound_Then_It_Should_KeepBothContractsSeparateFromPackageMetadata()
    {
        using var provider = Bind("""
            {
              "Site": { "Theme": "custom" },
              "Theme": {
                "Name": "Ignored application name",
                "Localization": { "en": { "Draft": "Draft" } },
                "HeroImages": [
                  { "Source": "/images/first.svg", "Alt": "First" },
                  { "Source": "http://cdn.example.test/second.png", "Alt": "" }
                ]
              }
            }
            """);

        var settings = provider.GetRequiredService<ThemeSettings>();
        var manifest = provider.GetRequiredService<ThemeManifest>();
        settings.HeroImages.Select(image => image.Source).ShouldBe(
            ["/images/first.svg", "http://cdn.example.test/second.png"]);
        settings.HeroImages.Select(image => image.Alt).ShouldBe(["First", ""]);
        settings.Localization["en"]!.Draft.ShouldBe("Draft");
        manifest.Localization["en"]!.Draft.ShouldBe("Draft");
        manifest.Localization.ShouldNotBeSameAs(settings.Localization);
        manifest.Name.ShouldBeEmpty();
        manifest.Slug.ShouldBeEmpty();
    }

    [Fact]
    public void Given_LegacySiteHeroImage_When_Bound_Then_It_Should_NotPopulateThemeImages()
    {
        using var provider = Bind("""{"Site":{"HeroImage":"/images/legacy.jpg"}}""");

        provider.GetRequiredService<ThemeSettings>().HeroImages.ShouldBeEmpty();
    }

    [Theory]
    [InlineData("""{"Theme":{"HeroImages":"hero.jpg"}}""", "Theme:HeroImages")]
    [InlineData("""{"Theme":{"HeroImages":{ "first":{"Source":"hero.jpg","Alt":""} }}}""", "Theme:HeroImages:first")]
    [InlineData("""{"Theme":{"HeroImages":[null]}}""", "Theme:HeroImages:0:Source")]
    [InlineData("""{"Theme":{"HeroImages":["hero.jpg"]}}""", "Theme:HeroImages:0")]
    [InlineData("""{"Theme":{"HeroImages":[{"Alt":""}]}}""", "Theme:HeroImages:0:Source")]
    [InlineData("""{"Theme":{"HeroImages":[{"Source":"hero.jpg"}]}}""", "Theme:HeroImages:0:Alt")]
    [InlineData("""{"Theme":{"HeroImages":[{"Source":" ","Alt":""}]}}""", "Theme:HeroImages:0:Source")]
    [InlineData("""{"Theme":{"HeroImages":[{"Source":"hero.jpg","Alt":null}]}}""", "Theme:HeroImages:0:Alt")]
    [InlineData("""{"Theme":"not-an-object"}""", "Theme")]
    public void Given_InvalidHeroImageShape_When_Bound_Then_It_Should_FailWithConfigurationPath(string json, string path)
    {
        var exception = Should.Throw<InvalidDataException>(() => Bind(json));

        exception.Message.ShouldContain(path);
    }

    [Fact]
    public void Given_NonContiguousImageIndices_When_Bound_Then_It_Should_RejectTheArray()
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Theme:HeroImages:1:Source"] = "images/hero.jpg",
            ["Theme:HeroImages:1:Alt"] = "A landscape",
        }).Build();

        var exception = Should.Throw<InvalidDataException>(() => new ServiceCollection().AddConfigurations(config));

        exception.Message.ShouldContain("Theme:HeroImages:1");
    }

    [Theory]
    [InlineData("javascript:alert(1)")]
    [InlineData("data:image/svg+xml;base64,PHN2Zz4=")]
    [InlineData("//cdn.example.test/hero.jpg")]
    [InlineData("../secret.jpg")]
    [InlineData("/images/../secret.jpg")]
    [InlineData("/images/%2e%2e/secret.jpg")]
    [InlineData("images/%2fsecret.jpg")]
    [InlineData(@"images\hero.jpg")]
    [InlineData("https://user:pass@cdn.example.test/hero.jpg")]
    [InlineData(" hero.jpg ")]
    public void Given_UnsafeHeroImageSource_When_Bound_Then_It_Should_RejectIt(string source)
    {
        var json = "{\"Theme\":{\"HeroImages\":[{\"Source\":"
            + System.Text.Json.JsonSerializer.Serialize(source) + ",\"Alt\":\"\"}]}}";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        var config = new ConfigurationBuilder().AddJsonStream(stream).Build();

        var exception = Should.Throw<InvalidDataException>(() => new ServiceCollection().AddConfigurations(config));

        exception.Message.ShouldContain("Theme:HeroImages:0:Source");
    }

    private static ServiceProvider Bind(string json)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        var config = new ConfigurationBuilder().AddJsonStream(stream).Build();
        return new ServiceCollection().AddConfigurations(config).BuildServiceProvider();
    }
}
