# ScissorHands.Web

[![NuGet](https://img.shields.io/nuget/vpre/ScissorHands.Web.svg)](https://www.nuget.org/packages/ScissorHands.Web)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/getscissorhands/ScissorHands.NET/blob/vnext/LICENSE)

`ScissorHands.Web` turns Markdown into static HTML using Razor themes and optional plugins. It includes a default theme and local preview server. Requires .NET 10.

## Quickstart

Create an empty ASP.NET Core application:

```bash
dotnet new web -n MyScissorHandsApp
cd MyScissorHandsApp
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

```bash
dotnet run -- --preview
```

Open the logged URL and refresh after edits. Stop preview with Ctrl+C, then run `dotnet run -- --build` to generate `dist` for static hosting.

Preview includes drafts and scheduled posts; do not deploy `preview/`. Builds exclude drafts and future-scheduled posts using the configured publication timezone (UTC by default).

Declaring `Site.Locales` requires complete messages in `Theme.Localization`; review the [locale configuration and migration guidance](https://github.com/getscissorhands/Scissorhands.NET/blob/vnext/docs/website-documentation.md#locale-routing-migration) when upgrading.

## Documentation

- [Configuration, content and authoring guides](https://github.com/getscissorhands/Scissorhands.NET/blob/vnext/docs/website-documentation.md)
- [Preview and build](https://github.com/getscissorhands/Scissorhands.NET/blob/vnext/docs/website-documentation.md#preview-and-build)
- [vNext migration guide](https://github.com/getscissorhands/Scissorhands.NET/blob/vnext/docs/website-documentation.md#upgrading-to-vnext) - review before upgrading; vNext includes breaking changes

## License

ScissorHands.NET is licensed under the [MIT License](https://github.com/getscissorhands/ScissorHands.NET/blob/vnext/LICENSE).

The built-in theme is adapted from [PlainPage](https://github.com/ChurchTao/PlainPage). See the [third-party notices](https://github.com/getscissorhands/Scissorhands.NET/blob/vnext/src/ScissorHands.Web/THIRD-PARTY-NOTICES.md).
