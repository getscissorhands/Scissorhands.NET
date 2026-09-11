using Microsoft.AspNetCore.Components.Rendering;

using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;

namespace ScissorHands.Plugin.Tests;

public class PluginComponentBaseTests
{
    [Fact]
    public void Given_CascadingValues_When_ComponentRendered_Then_It_Should_BindAllCascadingParameters()
    {
        // Arrange
        using var context = new BunitContext();

        var documents = new[]
        {
            new ContentDocument { SourcePath = "a.md", Kind = ContentKind.Post },
            new ContentDocument { SourcePath = "b.md", Kind = ContentKind.Page },
        };

        var document = new ContentDocument { SourcePath = "current.md", Kind = ContentKind.Post };
        var plugins = new[] { new PluginManifest { Name = "Test" } };
        var theme = new ThemeManifest { Name = "Minimal", Slug = "minimal" };
        var site = new SiteManifest();

        // Act
        var cut = context.Renderer.Render<TestPluginComponent>(parameters => parameters
            .Add(p => p.Name, "Test")
            .AddCascadingValue(documents)
            .AddCascadingValue(document)
            .AddCascadingValue(plugins)
            .AddCascadingValue(theme)
            .AddCascadingValue(site));

        // Assert
        cut.Instance.BoundDocuments.ShouldBeSameAs(documents);
        cut.Instance.BoundDocument.ShouldBeSameAs(document);
        cut.Instance.BoundPlugins.ShouldBeSameAs(plugins);
        cut.Instance.BoundPlugin.ShouldBeSameAs(plugins[0]);
        cut.Instance.BoundTheme.ShouldBeSameAs(theme);
        cut.Instance.BoundSite.ShouldBeSameAs(site);
    }

    [Fact]
    public void Given_UpdatedPluginName_When_ComponentRerendered_Then_It_Should_UpdateSelectedPlugin()
    {
        using var context = new BunitContext();
        var plugins = new[]
        {
            new PluginManifest { Name = "First" },
            new PluginManifest { Name = "Second" },
            new PluginManifest(),
        };

        var cut = context.Renderer.Render<TestPluginComponent>(parameters => parameters
            .Add(p => p.Name, "First")
            .AddCascadingValue(plugins));

        cut.Instance.BoundPlugin.ShouldBeSameAs(plugins[0]);

        cut.Render(parameters => parameters
            .Add(p => p.Name, "Second"));

        cut.Instance.BoundPlugin.ShouldBeSameAs(plugins[1]);
    }
}

internal class TestPluginComponent : PluginComponentBase
{
    public IEnumerable<ContentDocument>? BoundDocuments => Documents;
    public ContentDocument? BoundDocument => Document;
    public IEnumerable<PluginManifest>? BoundPlugins => Plugins;
    public PluginManifest? BoundPlugin => Plugin;
    public ThemeManifest? BoundTheme => Theme;
    public SiteManifest? BoundSite => Site;

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        // Intentionally empty: we only care about parameter binding.
    }
}
