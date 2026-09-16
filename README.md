# ScissorHands.NET

A .NET 10 static site generator that turns Markdown into HTML using Razor themes and optional plugins.

[![ScissorHands.Core NuGet](https://img.shields.io/nuget/vpre/ScissorHands.Core.svg?label=ScissorHands.Core)](https://www.nuget.org/packages/ScissorHands.Core)
[![ScissorHands.Plugin NuGet](https://img.shields.io/nuget/vpre/ScissorHands.Plugin.svg?label=ScissorHands.Plugin)](https://www.nuget.org/packages/ScissorHands.Plugin)
[![ScissorHands.Theme NuGet](https://img.shields.io/nuget/vpre/ScissorHands.Theme.svg?label=ScissorHands.Theme)](https://www.nuget.org/packages/ScissorHands.Theme)
[![ScissorHands.Web NuGet](https://img.shields.io/nuget/vpre/ScissorHands.Web.svg?label=ScissorHands.Web)](https://www.nuget.org/packages/ScissorHands.Web)

## Try the sample

Install the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0), clone this repository, and run from `samples/ScissorHands.Sample`:

```bash
dotnet run -- --preview
```

Stop preview with Ctrl+C. Run `dotnet run -- --build` to generate `dist` for static hosting. See the [sample guide](samples/ScissorHands.Sample/README.md), or use [ScissorHands.Web](src/ScissorHands.Web/README.md) to create your own application.

## Packages

| Package | Purpose |
| --- | --- |
| [Web](src/ScissorHands.Web/README.md) | Application composition, generation, preview, and the built-in theme |
| [Core](src/ScissorHands.Core/README.md) | Shared models, manifests, and contracts |
| [Theme](src/ScissorHands.Theme/README.md) | Base types for theme authors |
| [Plugin](src/ScissorHands.Plugin/README.md) | Content hooks and plugin component contracts |

## Documentation and compatibility

- [Documentation website](https://getscissorhands.app/docs/)
- [vNext reference](docs/website-documentation.md): configuration, content, locales, themes, plugins, and APIs
- [Upgrading to vNext](docs/website-documentation.md#upgrading-to-vnext)

vNext includes breaking changes; review the migration guide before upgrading.

## Support and license

[Report an issue](https://github.com/getscissorhands/Scissorhands.NET/issues). ScissorHands.NET is licensed under the [MIT License](LICENSE).
