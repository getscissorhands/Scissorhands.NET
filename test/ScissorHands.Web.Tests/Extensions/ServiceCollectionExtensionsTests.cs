using System.IO.Abstractions;
using System.Reflection;
using System.Text;

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
    public async Task Given_JsonConfigurationAndPluginDependency_When_ResolvedRunnerInvoked_Then_It_Should_UseDependencyOrder()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("""
            {
              "Plugins": [
                { "Name": "alpha", "Options": { "Suffix": "2" } },
                { "Name": "Zulu", "Options": { "Suffix": "1" } }
              ]
            }
            """));
        var config = new ConfigurationBuilder().AddJsonStream(stream).Build();
        var first = Substitute.For<IContentPlugin>();
        first.Name.Returns("Zulu");
        var second = Substitute.For<IContentPlugin, IContentPluginDependencies>();
        second.Name.Returns("Alpha");
        ((IContentPluginDependencies)second).DependsOn.Returns([new PluginDependency("zulu", PluginStage.PreMarkdown)]);
        foreach (var plugin in new[] { first, second })
        {
            plugin.PreMarkdownAsync(Arg.Any<ContentDocument>(), Arg.Any<PluginManifest>(), Arg.Any<SiteManifest>(), Arg.Any<CancellationToken>())
                .Returns(call => new ContentDocument
                {
                    Markdown = call.Arg<ContentDocument>().Markdown + call.Arg<PluginManifest>().Options!["Suffix"],
                });
        }
        var services = new ServiceCollection();
        services.AddConfigurations(config);
        services.AddServices(config, []);
        services.AddSingleton(second);
        services.AddSingleton(first);
        using var provider = services.BuildServiceProvider();
        var runner = provider.GetRequiredService<IPluginRunner>();

        var result = await runner.RunPreMarkdownAsync(new ContentDocument { Markdown = "source" }, Xunit.TestContext.Current.CancellationToken);

        runner.Manifests.Select(manifest => manifest.Name).ShouldBe(["alpha", "Zulu"]);
        result.Markdown.ShouldBe("source12");
    }

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
