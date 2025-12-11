using Microsoft.Extensions.Configuration;

using Microsoft.Extensions.DependencyInjection;

using ScissorHands.Core.Manifests;
using ScissorHands.Core.Services;
using ScissorHands.Web.Generators;
using ScissorHands.Web.Loaders;
using ScissorHands.Web.Renderers;
using ScissorHands.Web.Runners;
using ScissorHands.Web.Services;

namespace ScissorHands.Web.Extensions;

public static class ServiceCollectionExtensions
{
    private const string SITE_SETTINGS_SECTION_NAME = "Site";
    private const string PLUGIN_SETTINGS_SECTION_NAME = "Plugins";

    public static IServiceCollection AddConfigurations(this IServiceCollection services, IConfiguration config)
    {
        var siteManifest = config.GetSection(SITE_SETTINGS_SECTION_NAME).Get<SiteManifest>();
        var pluginManifests = config.GetSection(PLUGIN_SETTINGS_SECTION_NAME).Get<List<PluginManifest>>();
        services.AddSingleton(siteManifest!);
        services.AddSingleton(pluginManifests ?? []);

        return services;
    }

    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddSingleton<IContentLoader, ContentLoader>();
        services.AddSingleton<IMarkdownService, MarkdownService>();
        services.AddSingleton<IPluginLoader, PluginLoader>();
        services.AddSingleton<IPluginRunner, PluginRunner>();
        services.AddSingleton<IThemeService, ThemeService>();
        services.AddSingleton<IComponentRenderer, ComponentRenderer>();
        services.AddSingleton<IStaticSiteGenerator, StaticSiteGenerator>();

        return services;
    }
}
