using Microsoft.AspNetCore.Components;

namespace ScissorHands.Web.Renderers;

/// <summary>
/// This provides the interface for component renderer.
/// </summary>
public interface IComponentRenderer
{
    /// <summary>
    /// Renders the specified component to HTML string.
    /// </summary>
    /// <typeparam name="TComponent">Type of the component to render.</typeparam>
    /// <param name="layoutType">Type of the layout component.</param>
    /// <param name="parameters">Parameters to pass to the component.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Returns the rendered HTML string.</returns>
    Task<string> RenderAsync<TComponent>(Type layoutType, IDictionary<string, object?> parameters, CancellationToken cancellationToken = default)
        where TComponent : IComponent;
}
