# ScissorHands.NET

A .NET 10 static site generator that turns Markdown into HTML using Razor themes and optional plugins.

[![ScissorHands.Core NuGet](https://img.shields.io/nuget/vpre/ScissorHands.Core.svg?label=ScissorHands.Core)](https://www.nuget.org/packages/ScissorHands.Core)
[![ScissorHands.Plugin NuGet](https://img.shields.io/nuget/vpre/ScissorHands.Plugin.svg?label=ScissorHands.Plugin)](https://www.nuget.org/packages/ScissorHands.Plugin)
[![ScissorHands.Theme NuGet](https://img.shields.io/nuget/vpre/ScissorHands.Theme.svg?label=ScissorHands.Theme)](https://www.nuget.org/packages/ScissorHands.Theme)
[![ScissorHands.Web NuGet](https://img.shields.io/nuget/vpre/ScissorHands.Web.svg?label=ScissorHands.Web)](https://www.nuget.org/packages/ScissorHands.Web)

## Try the sample

Install the [repository's .NET SDK](global.json), clone this repository, and run from its root:

```bash
cd sample
dotnet run -- --preview
```

Stop preview with Ctrl+C, then run `dotnet run -- --build` to generate `dist`. Preview includes unpublished content; deploy only build output.

## Packages

| Package | Purpose |
| --- | --- |
| [Web](src/ScissorHands.Web/README.md) | Site generation, preview, and the built-in theme |
| [Core](src/ScissorHands.Core/README.md) | Shared models and contracts |
| [Theme](src/ScissorHands.Theme/README.md) | Theme authoring |
| [Plugin](src/ScissorHands.Plugin/README.md) | Plugin authoring |

## Documentation and compatibility

- [Documentation website](https://getscissorhands.app/docs/)
- [vNext guide](docs/website-documentation.md): quickstart, configuration, authoring, and APIs
- [Sample](sample/README.md)

vNext includes breaking changes; review the [migration guide](docs/website-documentation.md#upgrading-to-vnext) before upgrading.

## Support and license

[Report an issue](https://github.com/getscissorhands/Scissorhands.NET/issues). ScissorHands.NET is licensed under the [MIT License](LICENSE).
