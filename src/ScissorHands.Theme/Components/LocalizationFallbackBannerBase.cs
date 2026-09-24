using System.Collections.ObjectModel;

using Microsoft.AspNetCore.Components;

using ScissorHands.Core.Models;

namespace ScissorHands.Theme.Components;

/// <summary>
/// Supplies fallback notice data and encoded text rendering without prescribing banner markup.
/// </summary>
public abstract class LocalizationFallbackBannerBase : ComponentBase
{
    /// <summary>
    /// Gets the engine-prepared localization context.
    /// </summary>
    [CascadingParameter]
    public LocaleContext? LocaleContext { get; set; }

    /// <summary>
    /// Gets the renderer's per-render receipt for required fallback text.
    /// </summary>
    [CascadingParameter]
    public RenderReceipt? Receipt { get; set; }

    /// <summary>
    /// Gets whether the current document requires a notice.
    /// </summary>
    protected bool IsFallback => LocaleContext?.IsFallback == true;

    /// <summary>
    /// Gets the configured plain-text message.
    /// </summary>
    protected string? FallbackMessage => LocaleContext?.FallbackMessage;

    /// <summary>
    /// Gets the required marker and language attributes for the message-bearing element.
    /// </summary>
    protected IReadOnlyDictionary<string, object> BannerAttributes { get; private set; }
        = ReadOnlyDictionary<string, object>.Empty;

    /// <summary>
    /// Gets encoded message content. Rendering this fragment records delivery of the notice.
    /// </summary>
    protected RenderFragment FallbackMessageContent => builder =>
    {
        if (!IsFallback)
        {
            return;
        }
        builder.AddContent(0, FallbackMessage);
        if (Receipt is not null)
        {
            Receipt.Rendered = true;
        }
    };

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        BannerAttributes = IsFallback
            ? new ReadOnlyDictionary<string, object>(new Dictionary<string, object>
            {
                ["data-localization-fallback"] = LocaleContext!.Locale,
                ["lang"] = LocaleContext.Locale,
            })
            : ReadOnlyDictionary<string, object>.Empty;
    }

    /// <summary>
    /// Records actual rendering of encoded fallback text, not inheritance or a capability claim.
    /// </summary>
    public sealed class RenderReceipt
    {
        public bool Rendered { get; internal set; }
    }
}
