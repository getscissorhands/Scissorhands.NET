# ScissorHands.NET

A Blazor-based static site generator

[![ScissorHands.Core NuGet](https://img.shields.io/nuget/vpre/ScissorHands.Core.svg?label=ScissorHands.Core)](https://www.nuget.org/packages/ScissorHands.Core)
[![ScissorHands.Plugin NuGet](https://img.shields.io/nuget/vpre/ScissorHands.Plugin.svg?label=ScissorHands.Plugin)](https://www.nuget.org/packages/ScissorHands.Plugin)
[![ScissorHands.Theme NuGet](https://img.shields.io/nuget/vpre/ScissorHands.Theme.svg?label=ScissorHands.Theme)](https://www.nuget.org/packages/ScissorHands.Theme)
[![ScissorHands.Web NuGet](https://img.shields.io/nuget/vpre/ScissorHands.Web.svg?label=ScissorHands.Web)](https://www.nuget.org/packages/ScissorHands.Web)

## Prerequisites

- [.NET 10+ SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Visual Studio 2026](https://visualstudio.microsoft.com/downloads/) or [VS Code](https://code.visualstudio.com/download) + [C# Dev Kit](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit)

## Getting Started

1. Create a new empty web app.

    ```bash
    dotnet new web -n MyScissorHandsApp
    ```

1. Add the NuGet package. Make sure to add the `--prerelease` option because it's currently in public preview.

    ```bash
    dotnet add ./MyScissorHandsApp package ScissorHands.Web --prerelease
    ```

1. Open `Program.cs` and replace the existing codes with the following:

    ```csharp
    using ScissorHands.Web;

    var app = new ScissorHandsApplicationBuilder(args)
                  .Build();

    await app.RunAsync();
    ```

1. Build the app.

    ```bash
    dotnet build
    ```

1. Run the app for preview.

    ```bash
    dotnet run -- --preview
    ```

1. Run the app to build static contents.

    ```bash
    dotnet run -- --build
    ```

1. Add a GitHub Actions workflow to publish the app to [GitHub Pages](https://docs.github.com/pages/quickstart) or any static page hosting services.

> [!NOTE]
> For more details to run a ScissorHands.NET app, visit the [Quickstart](https://getscissorhands.app/docs/quickstart) page.

## Engine Preview

The [`samples/ScissorHands.Sample`](./samples/ScissorHands.Sample) project references the engine projects directly and uses the built-in default theme. Use this preview app to check how the engine works, without building a new app.

```bash
cd samples/ScissorHands.Sample
dotnet run -- --preview
```

## vNext Compatibility

This release includes the following source and binary breaking public-member changes:

- `ThemeManifest.Stylesheets` is now `IReadOnlyList<string>`.
- `ThemeManifest.Scripts` is now `IReadOnlyList<string>`.
- `PluginManifest.Options` is now `IReadOnlyDictionary<string, object?>`.
- Plugin implementations must provide `IContentPlugin.Id` or override `ContentPlugin.Id`.
- `PluginDependency` identifies its target through `PluginId`, not `Name`.

The collections are defensively copied during initialization. Existing object initializers continue to work, but themes and plugins must no longer mutate manifest collections after construction.

The existing one-argument `IThemeService` methods remain temporarily supported but are marked obsolete for removal in the next major version. Cancellation-aware overloads are used by the generator.

Plugin identity is now a required, stable lowercase kebab-case ID such as `heading-ids`. Add `Id` to every configured plugin manifest, reference IDs in dependency declarations, and select Razor plugin components with their `Id` parameter. `Name` is display-only and need not be unique. Missing or invalid IDs are rejected without name fallback or automatic normalization. Rebuild plugin assemblies and deploy them with the updated configuration and themes. See the [plugin ID migration guide](src/ScissorHands.Plugin/README.md#migrating-from-name-based-identity).

Plugin hooks honor optional, stage-scoped `DependsOn` declarations rather than assembly discovery, registration, or manifest order. Plugins that relied on an incidental execution order must declare their dependencies. Ready plugins are selected by ordinal ID ordering for deterministic output. See the [plugin dependency guide](src/ScissorHands.Plugin/README.md#plugin-dependencies).

## Issues?

If you find any issues, please [report them](../../issues).
