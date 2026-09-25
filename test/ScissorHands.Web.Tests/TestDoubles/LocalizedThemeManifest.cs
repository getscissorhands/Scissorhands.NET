using ScissorHands.Core.Manifests;

namespace ScissorHands.Web.Tests.TestDoubles;

internal static class LocalizedThemeManifest
{
    public static ThemeManifest Create(IEnumerable<string> locales, string? translationUnavailable = "Translation unavailable.") => new()
    {
        Slug = "default",
        Localization = locales.ToDictionary(
            locale => locale.Replace('_', '-').ToLowerInvariant(),
            _ => (ThemeLocalization?)new ThemeLocalization
            {
                TranslationUnavailable = translationUnavailable,
                Draft = "Draft",
                ScheduledOn = "Scheduled on {0}",
            },
            StringComparer.Ordinal),
    };
}
