using System.Reflection;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using ScissorHands.Theme;

namespace ScissorHands.Web.Renderers;

/// <summary>
/// This represents the component renderer entity.
/// </summary>
/// <param name="scopeFactory"><see cref="IServiceScopeFactory"/> instance.</param>
/// <param name="loggerFactory"><see cref="ILoggerFactory"/> instance.</param>
public sealed class ComponentRenderer(IServiceScopeFactory scopeFactory, ILoggerFactory loggerFactory) : IComponentRenderer
{
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));
    private readonly ILoggerFactory _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));

    // Lazy initialization of cascading parameter names discovered via reflection
    private static readonly Lazy<HashSet<string>> _cascadingParameterNames = new(() =>
    {
        var parameterNames = new HashSet<string>(StringComparer.Ordinal);

        // Get all view base classes from the ScissorHands.Theme assembly
        var viewBaseTypes = new[]
        {
            typeof(PageViewBase),
            typeof(PostViewBase),
            typeof(IndexViewBase),
            typeof(NotFoundViewBase)
        };

        foreach (var type in viewBaseTypes)
        {
            // Find all properties with CascadingParameter attribute
            var cascadingProperties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.GetCustomAttribute<CascadingParameterAttribute>() != null);

            foreach (var property in cascadingProperties)
            {
                parameterNames.Add(property.Name);
            }
        }

        return parameterNames;
    });

    /// <inheritdoc />
    public async Task<string> RenderAsync<TComponent>(Type layoutType, IDictionary<string, object?> parameters, CancellationToken cancellationToken = default)
        where TComponent : IComponent
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var scope = _scopeFactory.CreateScope();
        var renderer = new HtmlRenderer(scope.ServiceProvider, _loggerFactory);

        #pragma warning disable ASP0006
        var layoutParams = new Dictionary<string, object?>(parameters)
        {
            ["Body"] = (RenderFragment)(builder =>
            {
                builder.OpenComponent<TComponent>(0);
                var seq = 1;

                foreach (var kvp in parameters)
                {
                    if (_cascadingParameterNames.Value.Contains(kvp.Key))
                    {
                        continue;
                    }

                    builder.AddAttribute(seq++, kvp.Key, kvp.Value);
                }
                builder.CloseComponent();
            })
        };
        #pragma warning restore ASP0006

        var parameterView = ParameterView.FromDictionary(layoutParams);
        var html = await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var root = await renderer.RenderComponentAsync(layoutType, parameterView);
            return root.ToHtmlString();
        });

        return html;
    }
}
