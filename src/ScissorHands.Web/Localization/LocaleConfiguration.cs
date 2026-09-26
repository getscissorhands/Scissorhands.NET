using System.Collections.ObjectModel;
using System.Text;
using System.Text.RegularExpressions;

using ScissorHands.Core.Manifests;
using ScissorHands.Core.Urls;

namespace ScissorHands.Web.Localization;

internal sealed partial class LocaleConfiguration
{
    private static readonly IReadOnlyDictionary<string, ThemeLocalization?> EnglishLocalization
        = new ReadOnlyDictionary<string, ThemeLocalization?>(
            new Dictionary<string, ThemeLocalization?> { ["en"] = ThemeLocalization.English });

    private LocaleConfiguration(
        string? primary,
        IReadOnlyList<string> additionalLocales,
        IReadOnlyDictionary<string, ThemeLocalization?> localization)
    {
        Primary = primary;
        AdditionalLocales = additionalLocales;
        Localization = localization;
    }

    public string? Primary { get; }
    public IReadOnlyList<string> AdditionalLocales { get; }
    public IReadOnlyDictionary<string, ThemeLocalization?> Localization { get; }

    // Content discovery needs only the locale inventory, not theme messages.
    public static LocaleConfiguration Create(SiteManifest site)
    {
        ArgumentNullException.ThrowIfNull(site);
        if (!site.IsLocalizationEnabled)
        {
            return new(null, Array.Empty<string>(), EnglishLocalization);
        }

        var primary = ValidateLocale(site.Locales[0], "Site:Locales:0");
        var format = LocalePattern().Match(primary);
        var locales = new HashSet<string>(StringComparer.Ordinal) { primary };
        var additional = new List<string>();
        for (var index = 1; index < site.Locales.Count; index++)
        {
            var field = $"Site:Locales:{index}";
            var locale = ValidateLocale(site.Locales[index], field);
            var candidate = LocalePattern().Match(locale);
            if (candidate.Groups["script"].Success != format.Groups["script"].Success
                || candidate.Groups["region"].Success != format.Groups["region"].Success)
            {
                throw new InvalidDataException(
                    $"{field}: locale '{site.Locales[index]}' must use the same script/region structure as Site:Locales:0 '{site.Locales[0]}'.");
            }
            if (!locales.Add(locale))
            {
                throw new InvalidDataException(
                    $"Duplicate locale '{site.Locales[index]}' in {field}: normalized Site:Locales entries must be unique.");
            }
            additional.Add(locale);
        }

        return new(primary, additional.AsReadOnly(), ReadOnlyDictionary<string, ThemeLocalization?>.Empty);
    }

    public static LocaleConfiguration Create(SiteManifest site, ThemeManifest theme)
    {
        ArgumentNullException.ThrowIfNull(theme);
        var inventory = Create(site);
        if (inventory.Primary is null)
        {
            return inventory;
        }

        var active = new HashSet<string>(inventory.AdditionalLocales, StringComparer.Ordinal) { inventory.Primary };
        var configured = new Dictionary<string, ThemeLocalization?>(StringComparer.Ordinal);
        foreach (var (key, value) in theme.Localization)
        {
            var locale = ContentUrlHelper.GetLocaleSegment(key);
            // Catalog entries never declare routes or activate otherwise undeclared locales.
            if (active.Contains(locale) && !configured.TryAdd(locale, value))
            {
                throw new InvalidDataException(
                    $"Duplicate locale '{key}' in Theme:Localization:{key}: normalized active catalog keys must be unique.");
            }
        }

        var localization = new Dictionary<string, ThemeLocalization?>(StringComparer.Ordinal);
        foreach (var locale in new[] { inventory.Primary }.Concat(inventory.AdditionalLocales))
        {
            var path = $"Theme:Localization:{locale}";
            if (!configured.TryGetValue(locale, out var messages) || messages is null)
            {
                throw new InvalidDataException(
                    $"{path} must supply TranslationUnavailable, Draft, and ScheduledOn for declared locale '{locale}'.");
            }

            RequireMessage(messages.TranslationUnavailable, $"{path}:TranslationUnavailable");
            RequireMessage(messages.Draft, $"{path}:Draft");
            RequireMessage(messages.ScheduledOn, $"{path}:ScheduledOn");
            try
            {
                var format = CompositeFormat.Parse(messages.ScheduledOn!);
                if (format.MinimumArgumentCount != 1)
                {
                    throw new FormatException("The template must reference argument zero and no other argument index.");
                }
            }
            catch (FormatException exception)
            {
                throw new InvalidDataException(
                    $"{path}:ScheduledOn must be a valid composite format containing a real '{{0}}' argument and no unsupported argument indices; escaped '{{{{0}}}}' alone is not a placeholder.",
                    exception);
            }

            localization.Add(locale, messages);
        }

        return new(inventory.Primary, inventory.AdditionalLocales,
            new ReadOnlyDictionary<string, ThemeLocalization?>(localization));
    }

    /// <summary>
    /// Creates an effective theme with the package identity and assets and this locale catalog.
    /// For localized sites, use <see cref="Create(SiteManifest, ThemeManifest)"/> before applying.
    /// </summary>
    public ThemeManifest ApplyTo(ThemeManifest package)
    {
        ArgumentNullException.ThrowIfNull(package);
        return new ThemeManifest
        {
            Name = package.Name,
            Version = package.Version,
            Description = package.Description,
            Slug = package.Slug,
            Stylesheets = package.Stylesheets,
            Scripts = package.Scripts,
            Localization = Localization,
        };
    }

    public string? GetDirectoryLocale(string directory)
    {
        var normalized = ContentUrlHelper.GetLocaleSegment(directory);
        return AdditionalLocales.Contains(normalized, StringComparer.Ordinal) ? normalized : null;
    }

    public string GetFallbackMessage(string locale)
    {
        var normalized = ContentUrlHelper.GetLocaleSegment(locale);
        Localization.TryGetValue(normalized, out var messages);
        var message = messages?.TranslationUnavailable;
        if (string.IsNullOrWhiteSpace(message))
        {
            throw new InvalidDataException(
                $"Theme:Localization:{normalized}:TranslationUnavailable requires a validated nonblank message. Create the locale configuration with the effective theme catalog before rendering.");
        }
        return message;
    }

    private static void RequireMessage(string? value, string path)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidDataException($"{path} must contain a nonblank message.");
        }
    }

    private static string ValidateLocale(string? value, string field)
    {
        var locale = ContentUrlHelper.GetLocaleSegment(value);
        if (!LocalePattern().IsMatch(locale))
        {
            throw new InvalidDataException(
                $"Invalid {field}: '{value}'. Use a safe language, language-region, or language-script-region locale such as 'en', 'en-us', or 'zh-hant-tw'.");
        }
        return locale;
    }

    [GeneratedRegex(@"\A[a-z]{2,3}(?:-(?<script>[a-z]{4}))?(?:-(?<region>[a-z]{2}|[0-9]{3}))?\z", RegexOptions.CultureInvariant)]
    private static partial Regex LocalePattern();
}
