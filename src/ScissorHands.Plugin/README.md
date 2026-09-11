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

Derive from `ContentPlugin`, provide its stable `Id` and display `Name`, and override only the stages your plugin needs:

```csharp
using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;
using ScissorHands.Plugin;

public sealed class ReadingTimePlugin : ContentPlugin
{
    public override string Id => "reading-time";

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

## Plugin identity

`Id` is the stable identifier used by configuration, dependency declarations, and Razor component selection. It must be unique among installed plugins and use lowercase ASCII kebab-case: one or more letters or digits, optionally separated into segments by single hyphens. Examples include `reading-time`, `heading-ids`, and `syntax-highlighting-v2`.

Empty IDs, uppercase letters, whitespace, underscores, non-ASCII characters, and leading, trailing, or repeated hyphens are rejected. IDs are matched with ordinal comparison; the engine does not trim, lowercase, or derive them from names.

`IContentPlugin.Name` remains a non-empty display name, while `PluginManifest.Name` is optional display metadata. Display names do not need to be unique and are never used for matching or execution ordering. Keep an ID stable when changing a display name; changing an ID requires updating every reference.

## Plugin stages

Plugins can participate in three ordered stages:

- `PreMarkdownAsync`: transform the document before Markdown conversion.
- `PostMarkdownAsync`: transform the document after `Html` is populated.
- `PostHtmlAsync`: transform the final HTML after Razor rendering.

The output from each plugin becomes the input to the next enabled plugin. Execution order is determined by optional stage-scoped dependencies, not manifest, assembly discovery, or dependency injection registration order.

## Plugin dependencies

Dependency declarations are optional. Plugins without declarations have no ordering requirements and must not rely on another plugin having already run in the same stage.

When a hook consumes another plugin's output, override `DependsOn` in your `ContentPlugin` subclass. For example, a table-of-contents plugin that needs heading IDs in the post-Markdown stage declares:

```csharp
public override IReadOnlyList<PluginDependency> DependsOn =>
[
    new("heading-ids", PluginStage.PostMarkdown),
];
```

`PluginDependency.PluginId` is the target plugin's `Id`, not its display name. `PluginStage` supports `PreMarkdown`, `PostMarkdown`, and `PostHtml`. A declaration means that the target must be installed and enabled, and its hook must run before the declaring plugin's hook in that stage. Declare the same target separately for each stage that requires it.

`ContentPlugin.DependsOn` defaults to an empty list. Plugins implementing `IContentPlugin` directly can opt in by also implementing `IContentPluginDependencies`; dependency metadata is optional even though `Id` is required on every plugin. Declarations belong to plugin code, not `PluginManifest.Options` or a new JSON setting.

The engine resolves transitive dependencies separately for each stage, without changing the Markdown/Razor stage boundaries. Cross-stage dependencies cannot be expressed. Among plugins whose requirements are satisfied, the engine selects the next by ordinal ID ordering for deterministic output. Manifest position does not affect execution.

The runner snapshots declarations at construction and rejects missing or disabled dependencies, invalid IDs, unknown stages, self-dependencies, duplicate declarations within a stage, and cycles before any hooks run. Diagnostics identify the affected plugin ID and stage where applicable. A disabled plugin's declarations are ignored; dependencies are never installed or enabled automatically.

If a plugin previously relied on discovery or registration order, declare its actual dependencies instead. Unrelated plugins still execute sequentially, but must not rely on the tie-breaking order as a substitute for a dependency.

## Configure a plugin

Install the plugin assembly in the application and add its manifest to `appsettings.json`:

```json
{
  "Plugins": [
    {
      "Id": "reading-time",
      "Name": "Reading Time",
      "Options": {
        "WordsPerMinute": 200
      }
    }
  ]
}
```

`Id` is required and must match an installed plugin's ID exactly. `Name` is optional display metadata. Duplicate manifest IDs and manifests without a matching installed plugin fail startup. Installed plugins without a manifest remain disabled.

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

Set the component's required `Id` parameter to the plugin manifest ID, such as `Id="reading-time"`. Its optional `Name` parameter is display text only, as used in the example above. The base type provides the current document, document collection, plugin manifests, theme, and site through cascading parameters.

Invalid component IDs, invalid manifest IDs, and duplicate manifest IDs fail rendering. A valid component ID without a configured manifest leaves `Plugin` null, allowing the component to omit disabled plugin output.

## Migrating from name-based identity

This is a breaking source, binary, and configuration change. There is no name-based compatibility fallback or automatic slug generation.

1. Add `Id` to every `IContentPlugin` implementation, or override it in every `ContentPlugin` subclass. Choose an explicit, permanent kebab-case ID; keep `Name` for display.
2. Add that `Id` to each entry in the `Plugins` array in `appsettings.json`. Existing `Name` values may remain as optional labels, but no longer enable or identify plugins.
3. Replace dependency display names with target IDs. `PluginDependency.Name` has become `PluginDependency.PluginId`; update named constructor arguments and property access as well as string values.
4. Change Razor plugin selectors from `Name="Reading Time"` to `Id="reading-time"`. `Name` may still be supplied for display, but providing it alone is an error.
5. Rebuild plugin and consuming theme/application assemblies against the new packages, and deploy them with the updated configuration. Declare any previously implicit execution dependencies through `DependsOn`.

For example, a plugin's identity and a declaration targeting it become:

```csharp
public override string Id => "heading-ids";
public override string Name => "Heading IDs";
```

```csharp
new PluginDependency(PluginId: "heading-ids", Stage: PluginStage.PostMarkdown)
```

The matching configuration entry is `{ "Id": "heading-ids", "Name": "Heading IDs" }`. Renaming the display label later does not require changing dependencies or component selectors.

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
