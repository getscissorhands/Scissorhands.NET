using ScissorHands.Core.Manifests;

namespace ScissorHands.Core.Tests.Manifests;

public class ThemeSettingsTests
{
    [Fact]
    public void Given_DefaultSettings_When_Constructed_Then_It_Should_HaveNoImagesOrLocalization()
    {
        var settings = new ThemeSettings();

        settings.Localization.ShouldBeEmpty();
        settings.HeroImages.ShouldBeEmpty();
    }

    [Fact]
    public void Given_MutableCollections_When_SettingsInitialized_Then_It_Should_KeepReadOnlySnapshots()
    {
        var catalog = new Dictionary<string, ThemeLocalization?>
        {
            ["en"] = ThemeLocalization.English,
        };
        var images = new List<ThemeHeroImage>
        {
            new() { Source = "/images/hero.jpg", Alt = "A landscape" },
        };

        var settings = new ThemeSettings { Localization = catalog, HeroImages = images };
        catalog.Clear();
        images.Clear();

        settings.Localization.Keys.ShouldBe(["en"]);
        settings.HeroImages.ShouldHaveSingleItem().ShouldBe(new ThemeHeroImage
        {
            Source = "/images/hero.jpg",
            Alt = "A landscape",
        });
        Should.Throw<NotSupportedException>(() => ((IDictionary<string, ThemeLocalization?>)settings.Localization).Clear());
        Should.Throw<NotSupportedException>(() => ((IList<ThemeHeroImage>)settings.HeroImages).Clear());
    }

    [Fact]
    public void Given_NullCollections_When_SettingsInitialized_Then_It_Should_ExposeEmptySnapshots()
    {
        var settings = new ThemeSettings { Localization = null!, HeroImages = null! };

        settings.Localization.ShouldBeEmpty();
        settings.HeroImages.ShouldBeEmpty();
    }
}
