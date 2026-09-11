using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace ScissorHands.Core.Validation;

/// <summary>
/// Validates the stable identifiers used by plugins, manifests, dependencies, and components.
/// </summary>
public static partial class PluginIdValidator
{
    /// <summary>
    /// Requires a non-empty lowercase ASCII kebab-case ID without normalizing the input.
    /// </summary>
    /// <param name="id">The plugin ID to validate.</param>
    /// <param name="context">The configuration or declaration being validated.</param>
    public static void Validate([NotNull] string? id, string context)
    {
        if (id is null || !ValidIdPattern().IsMatch(id))
        {
            throw new InvalidOperationException(
                $"{context} has invalid plugin ID '{id ?? "<missing>"}'. Use lowercase ASCII kebab-case: letters or digits separated by single hyphens (for example, 'heading-ids').");
        }
    }

    [GeneratedRegex(@"\A[a-z0-9]+(?:-[a-z0-9]+)*\z")]
    private static partial Regex ValidIdPattern();
}
