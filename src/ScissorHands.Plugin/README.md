# ScissorHands.Plugin

[![NuGet](https://img.shields.io/nuget/vpre/ScissorHands.Plugin.svg)](https://www.nuget.org/packages/ScissorHands.Plugin)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/getscissorhands/ScissorHands.NET/blob/vnext/LICENSE)

`ScissorHands.Plugin` provides content hooks and Razor plugin components for the .NET 10 ScissorHands.NET generator.

Install this package when authoring a plugin. Applications that only consume plugins should install [`ScissorHands.Web`](https://www.nuget.org/packages/ScissorHands.Web).

## Install

Bash:

```bash
dotnet add package ScissorHands.Plugin --prerelease
```

PowerShell:

```powershell
dotnet add package ScissorHands.Plugin --prerelease
```

## Documentation

- [Plugin authoring and configuration](https://github.com/getscissorhands/Scissorhands.NET/blob/vnext/docs/website-documentation.md#plugin-authoring)
- [vNext migration guide](https://github.com/getscissorhands/Scissorhands.NET/blob/vnext/docs/website-documentation.md#upgrading-to-vnext) - review before upgrading; name-only plugin identity is no longer supported
- [Official plugins](https://github.com/getscissorhands/plugins)

## License

ScissorHands.NET is licensed under the [MIT License](https://github.com/getscissorhands/ScissorHands.NET/blob/vnext/LICENSE).
