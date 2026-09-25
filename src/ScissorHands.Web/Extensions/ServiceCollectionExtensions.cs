using System.IO.Abstractions;
using System.Reflection;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using ScissorHands.Core.Manifests;
using ScissorHands.Core.Services;
using ScissorHands.Plugin;
using ScissorHands.Web.Abstractions;
using ScissorHands.Web.Generators;
using ScissorHands.Web.Infrastructure;
using ScissorHands.Web.Loaders;
using ScissorHands.Web.Localization;
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
        var siteSection = config.GetSection(SITE_SETTINGS_SECTION_NAME);
        ValidateSiteConfiguration(siteSection);
        SiteManifest? siteManifest = siteSection.Get<SiteManifest>();
        siteManifest ??= new();
        _ = LocaleConfiguration.Create(siteManifest);
        siteManifest.Generator += ";v" + GetPackageVersion();

        services.AddSingleton(siteManifest);

        // Only the top-level application catalog is configuration. Package identity and
        // assets still come from the theme selected by Site:Theme.
        var localizationSection = config.GetSection("Theme:Localization");
        var localizationEntries = localizationSection.GetChildren().ToArray();
        // Some providers flatten empty objects/arrays to a childless empty-string
        // marker. An authored empty string is indistinguishable through IConfiguration.
        if (localizationSection.Value is not null
            && (localizationSection.Value.Length > 0 || localizationEntries.Length > 0))
        {
            throw new InvalidDataException("Theme:Localization must be a locale-keyed object.");
        }
        var localization = new Dictionary<string, ThemeLocalization?>(StringComparer.Ordinal);
        foreach (var entry in localizationEntries)
        {
            // Retain unusable entries as null so active-locale validation reports their
            // configuration path; unused catalog entries are not validated or activated.
            localization.Add(entry.Key, entry.Value is null ? entry.Get<ThemeLocalization>() : null);
        }
        services.AddSingleton(new ThemeManifest { Localization = localization });

        IEnumerable<PluginManifest>? pluginManifests = config.GetSection(PLUGIN_SETTINGS_SECTION_NAME).Get<List<PluginManifest>>();

        services.AddSingleton(pluginManifests ?? []);

        return services;
    }

    private static void ValidateSiteConfiguration(IConfigurationSection site)
    {
        foreach (var child in site.GetChildren())
        {
            if (string.Equals(child.Key, "Locale", StringComparison.OrdinalIgnoreCase)
                || string.Equals(child.Key, "LocalizationFallbackMessages", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidDataException(
                    $"{child.Path} is no longer supported. Remove Site:Locale and Site:LocalizationFallbackMessages; migrate to the ordered Site:Locales array (primary first) and Theme:Localization:<locale> with TranslationUnavailable, Draft, and ScheduledOn.");
            }
        }

        var theme = site.GetSection(nameof(SiteManifest.Theme));
        if (theme.GetChildren().Any())
        {
            throw new InvalidDataException(
                "Site:Theme must remain a string slug, for example \"Theme\": \"default\", not a nested object or array. Keep the slug in Site:Theme and move messages to the top-level Theme:Localization:<locale> catalog.");
        }

        var locales = site.GetSection(nameof(SiteManifest.Locales));
        var entries = locales.GetChildren().ToArray();
        // Some JSON providers represent [] as a childless empty-string marker.
        // IConfiguration cannot distinguish it from an authored ""; accept both.
        // With no items to bind, SiteManifest retains its empty locale snapshot.
        if (locales.Value is not null
            && (locales.Value.Length > 0 || entries.Length > 0))
        {
            throw new InvalidDataException("Site:Locales must be an ordered array of locale strings, not a scalar. Use [] or null to disable localization.");
        }

        for (var index = 0; index < entries.Length; index++)
        {
            var entry = entries[index];
            if (entry.Key != index.ToString(System.Globalization.CultureInfo.InvariantCulture)
                || entry.GetChildren().Any()
                || string.IsNullOrWhiteSpace(entry.Value))
            {
                throw new InvalidDataException(
                    $"Invalid {entry.Path}: Site:Locales must be an ordered array of nonblank locale strings with contiguous zero-based indices; null or object entries are not allowed.");
            }
        }
    }

    /// <summary>
    /// Adds dependencies to the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/> instance.</param>
    /// <param name="config"><see cref="IConfiguration"/> instance.</param>
    /// <param name="pluginAssemblies">Optional override assemblies for plugin scanning (useful for deterministic tests).</param>
    /// <returns>Returns the <see cref="IServiceCollection"/> instance.</returns>
    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration config, IEnumerable<Assembly>? pluginAssemblies = null)
    {
        services.AddSingleton<IFileSystem, FileSystem>();
        services.AddSingleton<IAppPaths, CurrentDirectoryAppPaths>();
        services.AddSingleton<IAssemblyCatalog, DefaultAssemblyCatalog>();
        services.AddSingleton<IContentWatcherFactory, ContentWatcherFactory>();
        services.TryAddSingleton(TimeProvider.System);

        services.AddSingleton<IContentLoader, ContentLoader>();
        services.AddSingleton<IMarkdownService, MarkdownService>();

        var assemblies = pluginAssemblies?.ToArray()
                   ?? [.. new DefaultAssemblyCatalog().GetAssemblies()];

        var siteManifest = config.GetSection(SITE_SETTINGS_SECTION_NAME).Get<SiteManifest>();
        if (siteManifest?.Debug == true)
        {
            foreach (var assembly in assemblies)
            {
                Console.WriteLine($"Loaded Assembly: {assembly.FullName}");
            }
        }

        services.Scan(scan => scan.FromAssemblies(assemblies)
                                  .AddClasses(c => c.AssignableTo<IContentPlugin>())
                                  .As<IContentPlugin>()
                                  .WithSingletonLifetime());

        services.AddSingleton<IPluginRunner, PluginRunner>();
        services.AddSingleton<IThemeService, ThemeService>();
        services.AddSingleton<IThemeComponentResolver, ThemeComponentResolver>();
        services.AddSingleton<IComponentRenderer, ComponentRenderer>();
        services.AddSingleton<IStaticSiteGenerator, StaticSiteGenerator>();

        return services;
    }

    private static string GetPackageVersion()
    {
        // var type = typeof(ServiceCollectionExtensions);
        // var attribute = type.Assembly
        //                     .GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        // var version = attribute?.InformationalVersion;

        // return version
        //     ?? typeof(ServiceCollectionExtensions).Assembly.GetName().Version?.ToString()
        //     ?? "unknown";

        var version = typeof(ServiceCollectionExtensions).Assembly.GetName().Version?.ToString();

        return version ?? "unknown";
    }
}
