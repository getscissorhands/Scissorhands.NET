# ScissorHands.Theme

[![NuGet](https://img.shields.io/nuget/vpre/ScissorHands.Theme.svg)](https://www.nuget.org/packages/ScissorHands.Theme)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/getscissorhands/ScissorHands.NET/blob/vnext/LICENSE)

`ScissorHands.Theme` provides Razor component base types for themes in the .NET 10 ScissorHands.NET static site generator.

Install this package when authoring a theme. Applications that only consume a theme should install [`ScissorHands.Web`](https://www.nuget.org/packages/ScissorHands.Web).

## Install

```bash
dotnet add package ScissorHands.Theme --prerelease
```

## Documentation

- [Theme authoring and examples](https://github.com/getscissorhands/Scissorhands.NET/blob/vnext/docs/website-documentation.md#theme-authoring)
- [Publication status contract](https://github.com/getscissorhands/Scissorhands.NET/blob/vnext/docs/website-documentation.md#publication-status-theme-contract) - required Draft/Scheduled badges; missing badges fail preview generation
- [Locale rendering contract](https://github.com/getscissorhands/Scissorhands.NET/blob/vnext/docs/website-documentation.md#locale-render-context) - required for fallback notices and theme-owned localization markup
- [Theme localization catalog](https://github.com/getscissorhands/Scissorhands.NET/blob/vnext/docs/website-documentation.md#locale-specific-sites) - configured messages on the effective theme manifest, separate from locale declarations
- [vNext migration guide](https://github.com/getscissorhands/Scissorhands.NET/blob/vnext/docs/website-documentation.md#upgrading-to-vnext) - review before upgrading; custom themes must supply all seven theme roles
- [Theme template](https://github.com/getscissorhands/theme-template)

## License

ScissorHands.NET is licensed under the [MIT License](https://github.com/getscissorhands/ScissorHands.NET/blob/vnext/LICENSE).
