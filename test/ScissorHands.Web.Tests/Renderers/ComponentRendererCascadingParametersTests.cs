using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;
using ScissorHands.Theme;
using ScissorHands.Web.Renderers;

namespace ScissorHands.Web.Tests.Renderers;

public class ComponentRendererCascadingParametersTests
{
    [Fact]
    public async Task Given_ComponentRenderer_When_RenderingWithLayout_Then_It_Should_PassCascadingValuesViaLayout_NotAsComponentAttributes()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();

        using var provider = services.BuildServiceProvider();

        var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();
        var loggerFactory = provider.GetRequiredService<ILoggerFactory>();

        var renderer = new ComponentRenderer(scopeFactory, loggerFactory);

        var document = new ContentDocument
        {
            Metadata = new ContentMetadata { Title = "Hello", Slug = "hello" }
        };
        var plugins = new List<PluginManifest> { new() };
        var theme = new ThemeManifest { Name = "test", Slug = "test" };
        var site = new SiteManifest();

        var parameters = new Dictionary<string, object?>
        {
            ["Document"] = document,
            ["Plugins"] = plugins,
            ["Theme"] = theme,
            ["Site"] = site,
            ["Extra"] = "extra"
        };

        // Act
        var html = await renderer.RenderAsync<TestCascadingPageView>(typeof(TestCascadingLayout), parameters);

        // Assert
        html.ShouldContain("extra");
        html.ShouldContain("Hello");
    }

    private sealed class TestCascadingLayout : LayoutComponentBase
    {
        [Parameter]
        public IEnumerable<ContentDocument>? Documents { get; set; }

        [Parameter]
        public ContentDocument? Document { get; set; }

        [Parameter]
        public IEnumerable<PluginManifest>? Plugins { get; set; }

        [Parameter]
        public ThemeManifest? Theme { get; set; }

        [Parameter]
        public SiteManifest? Site { get; set; }

        [Parameter]
        public string? Extra { get; set; }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenComponent<CascadingMainLayoutBase>(0);
            builder.AddAttribute(1, nameof(CascadingMainLayoutBase.Documents), Documents);
            builder.AddAttribute(2, nameof(CascadingMainLayoutBase.Document), Document);
            builder.AddAttribute(3, nameof(CascadingMainLayoutBase.Plugins), Plugins);
            builder.AddAttribute(4, nameof(CascadingMainLayoutBase.Theme), Theme);
            builder.AddAttribute(5, nameof(CascadingMainLayoutBase.Site), Site);
            builder.AddAttribute(6, nameof(CascadingMainLayoutBase.ChildContent), (RenderFragment)(b => b.AddContent(0, Body)));
            builder.CloseComponent();
        }
    }

    private sealed class TestCascadingPageView : PageViewBase
    {
        [Parameter]
        public string? Extra { get; set; }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "div");
            builder.AddContent(1, $"{Extra}|{Document?.Metadata.Title}");
            builder.CloseElement();
        }
    }
}
