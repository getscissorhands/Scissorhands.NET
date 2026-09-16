# ScissorHands.Web

[![NuGet](https://img.shields.io/nuget/vpre/ScissorHands.Web.svg)](https://www.nuget.org/packages/ScissorHands.Web)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/getscissorhands/ScissorHands.NET/blob/vnext/LICENSE)

`ScissorHands.Web` turns Markdown into static HTML using Razor themes and optional plugins. It includes a default theme and local preview server. Requires .NET 10.

## Quickstart

Create an empty ASP.NET Core application:

Bash:

```bash
dotnet new web -n MyScissorHandsApp
cd MyScissorHandsApp
dotnet add package ScissorHands.Web --prerelease
```

PowerShell:

```powershell
dotnet new web -n MyScissorHandsApp
Set-Location MyScissorHandsApp
dotnet add package ScissorHands.Web --prerelease
```

Replace `Program.cs` with:

```csharp
using ScissorHands.Web;

var app = new ScissorHandsApplicationBuilder(args).Build();
await app.RunAsync();
```

The default settings select the built-in theme. Create `contents/posts/hello.md`:

```markdown
---
title: Hello, ScissorHands
tags: [static-site]
---

# Hello, ScissorHands
```

Run from the application directory:

Bash:

```bash
dotnet run -- --preview
```

PowerShell:

```powershell
dotnet run -- --preview
```

Open the logged URL and refresh after edits. Stop preview with Ctrl+C, then run `dotnet run -- --build` to generate `dist` for static hosting.

## Documentation

- [Configuration, content and authoring guides](https://github.com/getscissorhands/Scissorhands.NET/blob/vnext/docs/website-documentation.md)
- [Preview and build](https://github.com/getscissorhands/Scissorhands.NET/blob/vnext/docs/website-documentation.md#preview-and-build)
- [vNext migration guide](https://github.com/getscissorhands/Scissorhands.NET/blob/vnext/docs/website-documentation.md#upgrading-to-vnext) - review before upgrading; vNext includes breaking changes

## License

ScissorHands.NET is licensed under the [MIT License](https://github.com/getscissorhands/ScissorHands.NET/blob/vnext/LICENSE).

The built-in theme is adapted from [PlainPage](https://github.com/ChurchTao/PlainPage). See the [third-party notices](https://github.com/getscissorhands/Scissorhands.NET/blob/vnext/src/ScissorHands.Web/THIRD-PARTY-NOTICES.md).
