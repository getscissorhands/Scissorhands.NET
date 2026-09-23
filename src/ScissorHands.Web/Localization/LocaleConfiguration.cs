using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

using ScissorHands.Core.Manifests;
using ScissorHands.Core.Urls;

namespace ScissorHands.Web.Localization;

internal sealed partial class LocaleConfiguration
{
    private LocaleConfiguration(string? primary, IReadOnlyDictionary<string, string?> messages)
    {
        Primary = primary;
        Messages = messages;
    }

    public string? Primary { get; }
    public IReadOnlyDictionary<string, string?> Messages { get; }

    public static LocaleConfiguration Create(SiteManifest site)
    {
        if (!site.IsLocalizationEnabled)
        {
            return new(null, ReadOnlyDictionary<string, string?>.Empty);
        }

        var primary = ValidateLocale(site.Locale, "Site:Locale");
        var format = LocalePattern().Match(primary);
        var messages = new Dictionary<string, string?>(StringComparer.Ordinal);
        foreach (var (key, value) in site.LocalizationFallbackMessages)
        {
            var locale = ValidateLocale(key, $"Site:LocalizationFallbackMessages:{key}");
            var candidate = LocalePattern().Match(locale);
            if (candidate.Groups["script"].Success != format.Groups["script"].Success
                || candidate.Groups["region"].Success != format.Groups["region"].Success)
            {
                throw new InvalidDataException(
                    $"Locale '{key}' must use the same format as Site:Locale '{site.Locale}'.");
            }
            if (locale == primary || !messages.TryAdd(locale, value))
            {
                throw new InvalidDataException(
                    $"Duplicate locale '{key}' in Site:LocalizationFallbackMessages: normalized locales must be unique and different from Site:Locale.");
            }
        }
        return new(primary, new ReadOnlyDictionary<string, string?>(messages));
    }

    public string? GetDirectoryLocale(string directory)
    {
        var normalized = ContentUrlHelper.GetLocaleSegment(directory);
        return Messages.ContainsKey(normalized) ? normalized : null;
    }

    public string GetFallbackMessage(string locale)
    {
        if (!Messages.TryGetValue(locale, out var message) || string.IsNullOrWhiteSpace(message))
        {
            throw new InvalidDataException(
                $"Site:LocalizationFallbackMessages:{locale} must contain a nonblank message because this locale requires primary-content fallback.");
        }
        return message;
    }

    private static string ValidateLocale(string? value, string field)
    {
        var locale = ContentUrlHelper.GetLocaleSegment(value);
        if (!LocalePattern().IsMatch(locale))
        {
            throw new InvalidDataException(
                $"Invalid {field}: '{value}'. Use a safe language or language-region locale such as 'en' or 'en-us'.");
        }
        return locale;
    }

    [GeneratedRegex(@"\A[a-z]{2,3}(?:-(?<script>[a-z]{4}))?(?:-(?<region>[a-z]{2}|[0-9]{3}))?\z", RegexOptions.CultureInvariant)]
    private static partial Regex LocalePattern();
}
