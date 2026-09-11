using System.IO.Abstractions;
using System.Reflection;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;
using ScissorHands.Core.Services;
using ScissorHands.Plugin;
using ScissorHands.Web.Abstractions;
using ScissorHands.Web.Extensions;
using ScissorHands.Web.Generators;
using ScissorHands.Web.Loaders;
using ScissorHands.Web.Renderers;
using ScissorHands.Web.Runners;

namespace ScissorHands.Web.Tests.Extensions;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void Given_PluginOptions_When_AddConfigurationsInvoked_Then_It_Should_BindReadOnlyOptions()
    {
        var services = new ServiceCollection();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Plugins:0:Name"] = "Example",
                ["Plugins:0:Options:Enabled"] = "true",
            })
            .Build();

        services.AddConfigurations(config);

        using var provider = services.BuildServiceProvider();
        var manifest = provider.GetRequiredService<IEnumerable<PluginManifest>>().Single();

        manifest.Name.ShouldBe("Example");
        manifest.Options.ShouldNotBeNull();
        manifest.Options.ShouldContainKey("Enabled");
    }

    [Fact]
    public void Given_ServiceCollection_When_AddServicesInvokedWithOverrideAssemblies_Then_It_Should_RegisterExpectedServicesAndPlugins()
    {
        // Arrange
        var services = new ServiceCollection();

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Site:Theme"] = "minimal",
                ["Site:Title"] = "My Site"
            })
            .Build();

        IEnumerable<Assembly> pluginAssemblies = new[] { typeof(DummyContentPlugin).Assembly };

        // Act
        services.AddServices(config, pluginAssemblies);

        using var provider = services.BuildServiceProvider();

        // Assert (core registrations)
        services.Any(d => d.ServiceType == typeof(IAppPaths)).ShouldBeTrue();
        services.Any(d => d.ServiceType == typeof(IFileSystem)).ShouldBeTrue();
        services.Any(d => d.ServiceType == typeof(IAssemblyCatalog)).ShouldBeTrue();
        services.Any(d => d.ServiceType == typeof(IContentWatcherFactory)).ShouldBeTrue();

        services.Any(d => d.ServiceType == typeof(IContentLoader)).ShouldBeTrue();
        services.Any(d => d.ServiceType == typeof(IMarkdownService)).ShouldBeTrue();
        services.Any(d => d.ServiceType == typeof(IPluginRunner)).ShouldBeTrue();
        services.Any(d => d.ServiceType == typeof(IThemeService)).ShouldBeTrue();
        services.Any(d => d.ServiceType == typeof(IComponentRenderer)).ShouldBeTrue();
        services.Any(d => d.ServiceType == typeof(IStaticSiteGenerator)).ShouldBeTrue();

        // Assert (plugin scanning)
        provider.GetServices<IContentPlugin>().Any(p => p is DummyContentPlugin).ShouldBeTrue();
    }

}

public sealed class DummyContentPlugin : IContentPlugin
{
    public string Name => "Dummy";

    public Task<ContentDocument> PreMarkdownAsync(ContentDocument document, PluginManifest plugin, SiteManifest site, CancellationToken cancellationToken = default)
        => Task.FromResult(document);

    public Task<ContentDocument> PostMarkdownAsync(ContentDocument document, PluginManifest plugin, SiteManifest site, CancellationToken cancellationToken = default)
        => Task.FromResult(document);

    public Task<string> PostHtmlAsync(string html, ContentDocument document, PluginManifest plugin, SiteManifest site, CancellationToken cancellationToken = default)
        => Task.FromResult(html);
}
