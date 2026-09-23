using System.Reflection;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;
using ScissorHands.Theme;
using ScissorHands.Web.Navigation;

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

        // Discover all types in the ScissorHands.Theme assembly that have cascading parameters
        var themeAssembly = typeof(PageViewBase).Assembly;

        try
        {
            var allTypes = themeAssembly.GetExportedTypes();

            foreach (var type in allTypes)
            {
                // Find all properties with CascadingParameter attribute
                var cascadingProperties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                    .Where(p => p.GetCustomAttribute<CascadingParameterAttribute>() is not null);

                foreach (var property in cascadingProperties)
                {
                    parameterNames.Add(property.Name);
                }
            }
        }
        catch (ReflectionTypeLoadException)
        {
            // If type loading fails, fall back to empty set
            // This should not happen in normal operation, but provides safety
        }

        return parameterNames;
    });

    /// <inheritdoc />
    public async Task<string> RenderAsync<TComponent>(Type layoutType, IDictionary<string, object?> parameters, CancellationToken cancellationToken = default)
        where TComponent : IComponent
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var scope = _scopeFactory.CreateScope();
        await using var renderer = new HtmlRenderer(scope.ServiceProvider, _loggerFactory);

#pragma warning disable ASP0006
        var layoutParams = new Dictionary<string, object?>(parameters)
        {
            ["Body"] = (RenderFragment)(builder =>
            {
                builder.OpenComponent<TComponent>(0);
                var seq = 1;

                foreach (var kvp in parameters)
                {
                    if (kvp.Key is nameof(MainLayoutBase.NavigationPages) or nameof(MainLayoutBase.NavigationTree)
                        || _cascadingParameterNames.Value.Contains(kvp.Key))
                    {
                        continue;
                    }

                    builder.AddAttribute(seq++, kvp.Key, kvp.Value);
                }
                builder.CloseComponent();
            })
        };
#pragma warning restore ASP0006

        if (typeof(MainLayoutBase).IsAssignableFrom(layoutType)
            && !layoutParams.ContainsKey(nameof(MainLayoutBase.NavigationTree))
            && layoutParams.TryGetValue(nameof(MainLayoutBase.NavigationPages), out var pagesValue)
            && pagesValue is IReadOnlyList<ContentDocument> pages)
        {
            var site = layoutParams.TryGetValue(nameof(MainLayoutBase.Site), out var siteValue) ? siteValue as SiteManifest : null;
            layoutParams[nameof(MainLayoutBase.NavigationTree)] = NavigationTreeBuilder.Build(pages, site, cancellationToken);
        }

        var parameterView = ParameterView.FromDictionary(layoutParams);
        var receipt = new LocalizationFallbackBanner.RenderReceipt();
        var html = await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var wrapper = ParameterView.FromDictionary(new Dictionary<string, object?>
            {
                ["Value"] = receipt,
                ["IsFixed"] = true,
                ["ChildContent"] = (RenderFragment)(builder =>
                {
                    builder.OpenComponent(0, layoutType);
                    foreach (var parameter in parameterView)
                    {
                        builder.AddAttribute(1, parameter.Name, parameter.Value);
                    }
                    builder.CloseComponent();
                }),
            });
            var root = await renderer.RenderComponentAsync<CascadingValue<LocalizationFallbackBanner.RenderReceipt>>(wrapper);
            return root.ToHtmlString();
        });

        if (parameters.TryGetValue(nameof(MainLayoutBase.LocaleContext), out var context)
            && context is LocaleContext { IsFallback: true } locale && !receipt.Rendered)
        {
            throw new InvalidDataException(
                $"Theme '{layoutType.FullName}' must render LocalizationFallbackBanner above fallback content for route '{locale.Route}'.");
        }
        return html;
    }
}
