using System.Reflection;

using Microsoft.Extensions.Configuration;

using Microsoft.Extensions.DependencyInjection;

using ScissorHands.Core.Manifests;
using ScissorHands.Core.Services;
using ScissorHands.Plugin;
using ScissorHands.Web.Generators;
using ScissorHands.Web.Loaders;
using ScissorHands.Web.Renderers;
using ScissorHands.Web.Runners;
using ScissorHands.Web.Services;

namespace ScissorHands.Web.Extensions;

/// <summary>
/// This represents the extensions entity for <see cref="IServiceCollection"/>
/// </summary>
public static class ServiceCollectionExtensions
{
    private const string SITE_SETTINGS_SECTION_NAME = "Site";
    private const string PLUGIN_SETTINGS_SECTION_NAME = "Plugins";

    /// <summary>
    /// Adds the configurations from appsettings.json
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/> instance.</param>
    /// <param name="config"><see cref="IConfiguration"/> instance.</param>
    /// <returns>Returns the <see cref="IServiceCollection"/> instance.</returns>
    public static IServiceCollection AddConfigurations(this IServiceCollection services, IConfiguration config)
    {
        SiteManifest? siteManifest = config.GetSection(SITE_SETTINGS_SECTION_NAME).Get<SiteManifest>();
        siteManifest!.Generator += ";v" + GetPackageVersion();

        IEnumerable<PluginManifest>? pluginManifests = config.GetSection(PLUGIN_SETTINGS_SECTION_NAME).Get<List<PluginManifest>>();

        services.AddSingleton(siteManifest!);
        services.AddSingleton(pluginManifests ?? []);

        return services;
    }

    /// <summary>
    /// Adds dependencies to the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/> instance.</param>
    /// <returns>Returns the <see cref="IServiceCollection"/> instance.</returns>
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddSingleton<IContentLoader, ContentLoader>();
        services.AddSingleton<IMarkdownService, MarkdownService>();

        var assemblies = Directory.GetFiles(AppContext.BaseDirectory, "*.dll")
                                  .Select(Assembly.LoadFrom)
                                  .ToArray()
                                  .Union(AppDomain.CurrentDomain.GetAssemblies())
                                  .ToArray();
        services.Scan(scan => scan.FromAssemblies(assemblies)
                                  .AddClasses(c => c.AssignableTo<IContentPlugin>())
                                  .As<IContentPlugin>()
                                  .WithSingletonLifetime());

        services.AddSingleton<IPluginRunner, PluginRunner>();
        services.AddSingleton<IThemeService, ThemeService>();
        services.AddSingleton<IComponentRenderer, ComponentRenderer>();
        services.AddSingleton<IStaticSiteGenerator, StaticSiteGenerator>();

        return services;
    }

    private static string GetPackageVersion()
    {
        var type = typeof(ServiceCollectionExtensions);
        var attribute = type.Assembly
                            .GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        var version = attribute?.InformationalVersion;

        return version
            ?? typeof(ServiceCollectionExtensions).Assembly.GetName().Version?.ToString()
            ?? "unknown";
    }
}
