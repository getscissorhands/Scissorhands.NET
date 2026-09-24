using System.Globalization;

using Microsoft.AspNetCore.Components;

using ScissorHands.Core.Models;

namespace ScissorHands.Theme.Components;

/// <summary>
/// Prepares language-switch data without prescribing theme markup.
/// </summary>
public abstract class LanguageSwitcherBase : ComponentBase
{
    /// <summary>
    /// Gets the current language and engine-prepared generated destinations.
    /// </summary>
    [CascadingParameter]
    public LocaleContext? LocaleContext { get; set; }

    /// <summary>
    /// Gets optional plain-text label overrides, keyed by the normalized locale.
    /// </summary>
    [Parameter]
    public IReadOnlyDictionary<string, string>? Labels { get; set; }

    /// <summary>
    /// Gets locales to display first, followed by remaining destinations in engine order.
    /// </summary>
    [Parameter]
    public IReadOnlyList<string>? LocaleOrder { get; set; }

    /// <summary>
    /// Gets the theme-localizable accessible navigation label.
    /// </summary>
    [Parameter]
    public string AriaLabel { get; set; } = "Language";

    /// <summary>
    /// Gets optional theme CSS classes.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    /// <summary>
    /// Gets prepared destinations, labels, and requested-language selection.
    /// </summary>
    protected IReadOnlyList<LanguageLink> Links { get; private set; } = Array.Empty<LanguageLink>();

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        Links = Array.Empty<LanguageLink>();
        if (LocaleContext is null)
        {
            return;
        }

        var destinations = LocaleContext.SwitchLanguageUrls;
        if (destinations.Count > 1 && string.IsNullOrWhiteSpace(AriaLabel))
        {
            throw new InvalidOperationException("LanguageSwitcher requires a nonblank AriaLabel.");
        }
        var order = LocaleOrder?.Select((locale, index) => (locale, index))
            .ToDictionary(item => item.locale, item => item.index, StringComparer.Ordinal);
        var links = new List<LanguageLink>(destinations.Count);
        foreach (var (locale, url) in destinations.OrderBy(link =>
            order is not null && order.TryGetValue(link.Key, out var rank) ? rank : int.MaxValue))
        {
            var label = Labels is not null && Labels.TryGetValue(locale, out var customLabel)
                ? customLabel
                : GetNativeLabel(locale, destinations.Keys);
            if (string.IsNullOrWhiteSpace(label))
            {
                throw new InvalidOperationException($"LanguageSwitcher requires a nonblank label for '{locale}'.");
            }
            links.Add(new LanguageLink(locale, url, label, locale == LocaleContext.Locale));
        }
        Links = links.AsReadOnly();
    }

    /// <summary>
    /// Resolves a default native label; themes may override this without changing destinations.
    /// </summary>
    protected virtual string GetNativeLabel(string locale, IEnumerable<string> locales)
    {
        var language = locale.Split('-')[0];
        var disambiguate = locales.Count(candidate => candidate.Split('-')[0] == language) > 1;
        return CultureInfo.GetCultureInfo(disambiguate ? locale : language).NativeName;
    }

    /// <summary>
    /// Represents one engine-prepared destination with theme-prepared display data.
    /// </summary>
    public sealed record LanguageLink(string Locale, string Url, string Label, bool IsCurrent);
}
