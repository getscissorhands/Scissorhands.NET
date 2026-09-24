using System.Collections.ObjectModel;

using Microsoft.AspNetCore.Components;

using ScissorHands.Core.Models;

namespace ScissorHands.Theme.Components;

/// <summary>
/// Supplies document SEO data for theme-owned head markup.
/// </summary>
public abstract class LocalizationMetadataBase : ComponentBase
{
    /// <summary>
    /// Gets the engine-prepared document context.
    /// </summary>
    [CascadingParameter]
    public LocaleContext? LocaleContext { get; set; }

    /// <summary>
    /// Gets the canonical URL, or null when document SEO does not apply.
    /// </summary>
    protected string? CanonicalUrl => LocaleContext?.CanonicalUrl;

    /// <summary>
    /// Gets published primary/real-translation URLs, without fallback alternatives.
    /// </summary>
    protected IReadOnlyDictionary<string, string> AlternateLanguageUrls =>
        LocaleContext?.AlternateLanguageUrls ?? ReadOnlyDictionary<string, string>.Empty;
}
