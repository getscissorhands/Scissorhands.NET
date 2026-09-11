# ScissorHands.Plugin

[![NuGet](https://img.shields.io/nuget/vpre/ScissorHands.Plugin.svg)](https://www.nuget.org/packages/ScissorHands.Plugin)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/getscissorhands/ScissorHands.NET/blob/vnext/LICENSE)

`ScissorHands.Plugin` provides the contracts and Razor component base type for extending the ScissorHands.NET content generation pipeline.

Install this package when authoring a plugin. Applications that only consume plugins should install [`ScissorHands.Web`](https://www.nuget.org/packages/ScissorHands.Web).

## Install

```bash
dotnet add package ScissorHands.Plugin --prerelease
```

ScissorHands.NET currently targets .NET 10.

## Create a content plugin

Derive from `ContentPlugin` and override only the stages your plugin needs:

```csharp
using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;
using ScissorHands.Plugin;

public sealed class ReadingTimePlugin : ContentPlugin
{
    public override string Name => "Reading Time";

    public override Task<ContentDocument> PostMarkdownAsync(
        ContentDocument document,
        PluginManifest plugin,
        SiteManifest site,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var words = document.Markdown
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Length;

        document.Html = $"<p>{Math.Max(1, words / 200)} min read</p>{document.Html}";
        return Task.FromResult(document);
    }
}
```

For complete control, implement `IContentPlugin` directly.

## Plugin stages

Plugins can participate in three ordered stages:

- `PreMarkdownAsync`: transform the document before Markdown conversion.
- `PostMarkdownAsync`: transform the document after `Html` is populated.
- `PostHtmlAsync`: transform the final HTML after Razor rendering.

The output from each plugin becomes the input to the next configured plugin.

## Configure a plugin

Install the plugin assembly in the application and add its manifest to `appsettings.json`:

```json
{
  "Plugins": [
    {
      "Name": "Reading Time",
      "Options": {
        "WordsPerMinute": 200
      }
    }
  ]
}
```

Plugin names are matched case-insensitively and must be non-empty and unique. A configured manifest that does not match an installed plugin fails startup. Installed plugins without a manifest remain disabled.

`PluginManifest.Options` is nullable and exposed as `IReadOnlyDictionary<string, object?>`. Treat it as immutable input and validate the type and value of every option your plugin consumes.

## Create a Razor plugin component

Derive from `PluginComponentBase` when a theme needs to render plugin output:

```razor
@inherits PluginComponentBase

@if (Plugin is not null)
{
    <meta name="example-plugin" content="@Name" />
}
```

Set the component's `Name` parameter to the plugin manifest name. The base type provides the current document, document collection, plugin manifests, theme, and site through cascading parameters.

## Preview behavior

`SiteManifest.IsPreview` is set before plugin hooks and Razor rendering. Use it to suppress production-only side effects when appropriate.

Internal URLs emitted by a plugin should be relative to `SiteManifest.BaseUrl` rather than hardcoded to the domain root.

## Learn more

- [Plugin documentation](https://getscissorhands.app/docs/plugins/)
- [Official plugins](https://github.com/getscissorhands/plugins)
- [ScissorHands.Web package](https://www.nuget.org/packages/ScissorHands.Web)
- [Source code](https://github.com/getscissorhands/ScissorHands.NET)

## License

ScissorHands.NET is licensed under the [MIT License](https://github.com/getscissorhands/ScissorHands.NET/blob/vnext/LICENSE).
