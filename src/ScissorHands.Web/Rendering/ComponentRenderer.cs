using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ScissorHands.Web.Rendering;

public sealed class ComponentRenderer
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILoggerFactory _loggerFactory;

    public ComponentRenderer(IServiceScopeFactory scopeFactory, ILoggerFactory loggerFactory)
    {
        _scopeFactory = scopeFactory;
        _loggerFactory = loggerFactory;
    }

    public async Task<string> RenderAsync<TComponent>(Type layoutType, IDictionary<string, object?> parameters, CancellationToken cancellationToken = default)
        where TComponent : IComponent
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var scope = _scopeFactory.CreateScope();
        var renderer = new HtmlRenderer(scope.ServiceProvider, _loggerFactory);

        #pragma warning disable ASP0006
        var layoutParams = new Dictionary<string, object?>
        {
            ["Layout"] = layoutType,
            ["ChildContent"] = (RenderFragment)(builder =>
            {
                builder.OpenComponent<TComponent>(0);
                var seq = 1;
                foreach (var kvp in parameters)
                {
                    builder.AddAttribute(seq, kvp.Key, kvp.Value);
                    seq += 1;
                }
                builder.CloseComponent();
            })
        };
        #pragma warning restore ASP0006

        var parameterView = ParameterView.FromDictionary(layoutParams);
        var html = await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var result = await renderer.RenderComponentAsync<LayoutView>(parameterView);
            return result.ToHtmlString();
        });

        return html;
    }
}
