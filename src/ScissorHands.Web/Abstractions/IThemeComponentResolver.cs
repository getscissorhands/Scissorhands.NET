using ScissorHands.Theme;

namespace ScissorHands.Web.Abstractions;

/// <summary>
/// Resolves the Razor component family for a configured theme.
/// </summary>
public interface IThemeComponentResolver
{
    /// <summary>
    /// Resolves the component family for the given theme slug.
    /// </summary>
    /// <param name="themeSlug">Configured theme slug.</param>
    /// <returns>The resolved theme components.</returns>
    ThemeComponentSet Resolve(string themeSlug);
}
