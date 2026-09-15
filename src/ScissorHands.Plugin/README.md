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

Start with a `ContentPlugin` subclass that supplies a stable ID and display name:

```csharp
using ScissorHands.Plugin;

public sealed class ExamplePlugin : ContentPlugin
{
    public override string Id => "example-plugin";
    public override string Name => "Example Plugin";
}
```

This scaffold passes content through unchanged. Override `PreMarkdownAsync`, `PostMarkdownAsync`, or `PostHtmlAsync` to add transformations. IDs must be unique lowercase ASCII kebab-case; names are display-only.

## Configure a plugin

Install the plugin assembly in the application and add its manifest to `appsettings.json`:

```json
{
  "Plugins": [
    {
      "Id": "example-plugin"
    }
  ]
}
```

The ID must match the installed plugin exactly. Installed plugins without a manifest remain disabled. Declare ordering requirements through stage-scoped `DependsOn`, not manifest position.

vNext does not support name-only plugin configuration or Razor selection. Review the migration reference before upgrading older plugins.

## Learn more

- [vNext plugin guide (website handoff)](https://github.com/getscissorhands/Scissorhands.NET/blob/vnext/docs/website-documentation.md#plugin-authoring)
- [vNext migration reference](https://github.com/getscissorhands/Scissorhands.NET/blob/vnext/docs/website-documentation.md#upgrading-to-vnext)
- [Plugin documentation](https://getscissorhands.app/docs/plugins/)
- [Official plugins](https://github.com/getscissorhands/plugins)
- [ScissorHands.Web package](https://www.nuget.org/packages/ScissorHands.Web)
- [Source code](https://github.com/getscissorhands/ScissorHands.NET)

## License

ScissorHands.NET is licensed under the [MIT License](https://github.com/getscissorhands/ScissorHands.NET/blob/vnext/LICENSE).
