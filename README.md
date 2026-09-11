# ScissorHands.NET

A Blazor-based static site generator

## Prerequisites

- [.NET 10+ SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Visual Studio 2026](https://visualstudio.microsoft.com/downloads/) or [VS Code](https://code.visualstudio.com/download) + [C# Dev Kit](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit)

## Getting Started

1. Create a new empty web app.

    ```bash
    dotnet new web -n MyScissorHandsApp
    ```

1. Add the NuGet package.

    ```bash
    dotnet add ./MyScissorHandsApp package ScissorHands.Web --prerelease
    ```

   > Currently, ScissorHands is public preview. Therefore, add the `--prerelease` option.

1. Open `Program.cs` and add the following codes.

    ```csharp
    using ScissorHands.Web;

    var app = new ScissorHandsApplicationBuilder(args)
                  .Build();
    await app.RunAsync();
    ```

   The theme is discovered automatically from the `Site:Theme` slug in `appsettings.json`. Theme Razor components should share a namespace whose normalized suffix matches the theme slug, such as `ScissorHands.Theme.MinimalBlog` for `minimal-blog`.

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

> **NOTE**: For more details to run a ScissorHands.NET app, visit the [Quickstart](https://getscissorhands.app/docs/quickstart/) page.

## Sample Application

The [`samples/ScissorHands.Sample`](./samples/ScissorHands.Sample) project references the engine projects directly and uses the built-in default theme. It can be used to preview local engine changes without publishing packages:

```bash
cd samples/ScissorHands.Sample
dotnet run
```

## vNext Compatibility

This release contains three source and binary breaking public-member changes:

- `ThemeManifest.Stylesheets` is now `IReadOnlyList<string>`.
- `ThemeManifest.Scripts` is now `IReadOnlyList<string>`.
- `PluginManifest.Options` is now `IReadOnlyDictionary<string, object?>`.

The collections are defensively copied during initialization. Existing object initializers continue to work, but themes and plugins must no longer mutate manifest collections after construction.

The existing one-argument `IThemeService` methods remain temporarily supported but are marked obsolete for removal in the next major version. Cancellation-aware overloads are used by the generator.

Plugin hooks now honor optional, stage-scoped `DependsOn` declarations rather than assembly discovery, registration, or manifest order. Existing plugins remain compatible, but plugins that relied on an incidental execution order must declare their dependencies. Ready plugins are selected by ordinal case-insensitive name for deterministic output. See the [plugin guide](src/ScissorHands.Plugin/README.md#plugin-dependencies).

## Issues?

If you find any issues, please [report them](../../issues).
