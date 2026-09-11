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
        var plugins = new[] { new PluginManifest { Id = "test", Name = "Manifest display name" } };
        var theme = new ThemeManifest { Name = "Minimal", Slug = "minimal" };
        var site = new SiteManifest();

        // Act
        var cut = context.Renderer.Render<TestPluginComponent>(parameters => parameters
            .Add(p => p.Id, "test")
            .Add(p => p.Name, "Component display name")
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
    public void Given_UpdatedPluginId_When_ComponentRerendered_Then_It_Should_SelectByIdRegardlessOfDisplayName()
    {
        using var context = new BunitContext();
        var plugins = new[]
        {
            new PluginManifest { Id = "first", Name = "Shared display name" },
            new PluginManifest { Id = "second", Name = "Shared display name" },
            new PluginManifest { Id = "third" },
        };

        var cut = context.Renderer.Render<TestPluginComponent>(parameters => parameters
            .Add(p => p.Id, "first")
            .AddCascadingValue(plugins));

        cut.Instance.BoundPlugin.ShouldBeSameAs(plugins[0]);

        cut.Render(parameters => parameters
            .Add(p => p.Name, "Renamed display text"));

        cut.Instance.BoundPlugin.ShouldBeSameAs(plugins[0]);

        cut.Render(parameters => parameters
            .Add(p => p.Id, "second"));

        cut.Instance.BoundPlugin.ShouldBeSameAs(plugins[1]);

        cut.Render(parameters => parameters
            .Add(p => p.Id, "missing"));

        cut.Instance.BoundPlugin.ShouldBeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Test")]
    [InlineData("test_plugin")]
    [InlineData("test--plugin")]
    public void Given_InvalidComponentId_When_Rendered_Then_It_Should_ReportConfigurationError(string? id)
    {
        using var context = new BunitContext();

        var exception = Should.Throw<InvalidOperationException>(() =>
            context.Renderer.Render<TestPluginComponent>(parameters => parameters.Add(p => p.Id, id!)));

        exception.Message.ShouldContain("Plugin component");
        exception.Message.ShouldContain("invalid plugin ID");
    }

    [Fact]
    public void Given_NameOnlyComponent_When_Rendered_Then_It_Should_NotFallBackToName()
    {
        using var context = new BunitContext();
        var plugins = new[] { new PluginManifest { Id = "test", Name = "Test" } };

        var exception = Should.Throw<InvalidOperationException>(() =>
            context.Renderer.Render<TestPluginComponent>(parameters => parameters
                .Add(p => p.Name, "Test")
                .AddCascadingValue(plugins)));

        exception.Message.ShouldContain("Plugin component");
        exception.Message.ShouldContain("invalid plugin ID");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("Test")]
    [InlineData("test_plugin")]
    public void Given_InvalidManifestId_When_Rendered_Then_It_Should_ReportConfigurationError(string? id)
    {
        using var context = new BunitContext();
        var plugins = new[] { new PluginManifest { Id = id, Name = "test" } };

        var exception = Should.Throw<InvalidOperationException>(() =>
            context.Renderer.Render<TestPluginComponent>(parameters => parameters
                .Add(p => p.Id, "test")
                .AddCascadingValue(plugins)));

        exception.Message.ShouldContain("Configured plugin manifest");
        exception.Message.ShouldContain("invalid plugin ID");
    }

    [Fact]
    public void Given_DuplicateManifestIds_When_Rendered_Then_It_Should_ReportConfigurationError()
    {
        using var context = new BunitContext();
        var plugins = new[]
        {
            new PluginManifest { Id = "test", Name = "First" },
            new PluginManifest { Id = "test", Name = "Second" },
        };

        var exception = Should.Throw<InvalidOperationException>(() =>
            context.Renderer.Render<TestPluginComponent>(parameters => parameters
                .Add(p => p.Id, "test")
                .AddCascadingValue(plugins)));

        exception.Message.ShouldContain("Plugin manifest ID 'test'");
        exception.Message.ShouldContain("configured more than once");
    }

    [Fact]
    public void Given_ManifestNameMatchingComponentId_When_Rendered_Then_It_Should_NotSelectByName()
    {
        using var context = new BunitContext();
        var plugins = new[] { new PluginManifest { Id = "other", Name = "test" } };

        var cut = context.Renderer.Render<TestPluginComponent>(parameters => parameters
            .Add(p => p.Id, "test")
            .AddCascadingValue(plugins));

        cut.Instance.BoundPlugin.ShouldBeNull();
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
