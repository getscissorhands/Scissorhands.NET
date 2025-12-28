using Microsoft.AspNetCore.Components.Rendering;

using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;

namespace ScissorHands.Plugin.Tests;

public class PluginComponentBaseTests
{
    [Fact]
    public void Given_Parameters_When_ComponentRendered_Then_It_Should_BindAllParameters()
    {
        // Arrange
        using var context = new BunitContext();

        var documents = new[]
        {
            new ContentDocument { SourcePath = "a.md", Kind = ContentKind.Post },
            new ContentDocument { SourcePath = "b.md", Kind = ContentKind.Page },
        };

        var document = new ContentDocument { SourcePath = "current.md", Kind = ContentKind.Post };
        var plugin = new PluginManifest { Name = "Test" };
        var theme = new ThemeManifest { Name = "Minimal", Slug = "minimal" };
        var site = new SiteManifest();

        // Act
        var cut = context.Renderer.Render<TestPluginComponent>(parameters => parameters
            .Add(p => p.Documents, documents)
            .Add(p => p.Document, document)
            .Add(p => p.Plugin, plugin)
            .Add(p => p.Theme, theme)
            .Add(p => p.Site, site));

        // Assert
        cut.Instance.Documents.ShouldBeSameAs(documents);
        cut.Instance.Document.ShouldBeSameAs(document);
        cut.Instance.Plugin.ShouldBeSameAs(plugin);
        cut.Instance.Theme.ShouldBeSameAs(theme);
        cut.Instance.Site.ShouldBeSameAs(site);
    }
}

internal class TestPluginComponent : PluginComponentBase
{
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        // Intentionally empty: we only care about parameter binding.
    }
}

