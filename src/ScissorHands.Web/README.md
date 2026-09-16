# ScissorHands.Web

[![NuGet](https://img.shields.io/nuget/vpre/ScissorHands.Web.svg)](https://www.nuget.org/packages/ScissorHands.Web)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/getscissorhands/ScissorHands.NET/blob/vnext/LICENSE)

`ScissorHands.Web` is the static site generation engine for [ScissorHands.NET](https://getscissorhands.app). It turns Markdown documents with YAML frontmatter into static HTML using Razor themes and optional content plugins.

## Features

- Markdown posts/pages with frontmatter, tags, and hierarchical navigation
- Filename-based page reading order and automatic previous/next links
- Razor themes and optional content-processing plugins
- Configurable routes and static output for subpath hosting
- Local preview and a built-in light/dark theme

ScissorHands.NET currently targets .NET 10.

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

No theme component types need to be registered in `Program.cs`. The built-in theme is used when `Site:Theme` is `default`.

Add these sections to `appsettings.json`:

```json
{
  "Site": {
    "Title": "My site",
    "Theme": "default",
    "SiteUrl": "https://example.com",
    "BaseUrl": "/"
  },
  "Plugins": []
}
```

Create `contents/posts/hello.md`:

```markdown
---
title: Hello, ScissorHands
published: 2026-09-11
tags:
  - static-site
---

# Hello, ScissorHands

Write the post in Markdown.
```

Place pages under `contents/pages/`. Page navigation is opt-in through `show_in_navigation: true`; hiding a navigation link is not access control.

## Page reading order

File-backed pages use filename order, with `index.md` first in each directory. The built-in theme renders automatic previous/next links for eligible pages; navigation grouping still follows slugs.

Renaming sources can change inferred URLs, so use explicit slugs to preserve existing addresses. See the [reading order and navigation guide](https://github.com/getscissorhands/Scissorhands.NET/blob/vnext/docs/website-documentation.md#reading-order-and-previousnext-links) for traversal, eligibility, custom-loader behavior, and plugin snapshots.

## Locale-specific generation

Set `Site.UseLocaleInUrl` to `true` to generate a homepage and eligible tag listings for each published content locale, plus `Site.Locale` even when empty. The root redirects to the default locale. Home/Tags links, page navigation and previous/next stay within the active locale; there is no automatic translation or language fallback.

Locale folders under `contents\posts` and `contents\pages` are organizational only. Frontmatter `locale` overrides `Site.Locale`; explicit locale-free slugs keep URLs independent of source folders. Keep the single root `404.html` in the site-default locale: a conflicting custom-404 locale fails generation.

Unprefixed tag URLs redirect only when the matching default-locale destination exists. Existing locale-enabled sites and custom themes must follow the [locale migration guidance](https://github.com/getscissorhands/Scissorhands.NET/blob/vnext/docs/website-documentation.md#locale-routing-migration). Disabled-mode routes remain unchanged. Locale redirects require a rooted `BaseUrl` ending in `/`, such as `/blog/`; generation does not fix the separately tracked preview-prefix mount in #89.

## Preview and build

Run from the application directory. Start the local preview server:

```bash
dotnet run -- --preview
```

Generate the static site into `dist/`:

```bash
dotnet run -- --build
```

Refresh the browser after preview regeneration. Razor/C# changes require recompilation. Use `BaseUrl` for subpath hosting.

vNext includes breaking plugin identity, theme-component, and page-route changes. Review the migration reference before upgrading.

## Learn more

- [vNext guides and reference (website handoff)](https://github.com/getscissorhands/Scissorhands.NET/blob/vnext/docs/website-documentation.md)
- [vNext migration reference](https://github.com/getscissorhands/Scissorhands.NET/blob/vnext/docs/website-documentation.md#upgrading-to-vnext)
- [Documentation](https://getscissorhands.app/docs/)
- [Source code](https://github.com/getscissorhands/ScissorHands.NET)
- [Theme template](https://github.com/getscissorhands/theme-template)
- [Official plugins](https://github.com/getscissorhands/plugins)
- [Issue tracker](https://github.com/getscissorhands/ScissorHands.NET/issues)

## License

ScissorHands.NET is licensed under the [MIT License](https://github.com/getscissorhands/ScissorHands.NET/blob/vnext/LICENSE).

The built-in theme is adapted from [PlainPage](https://github.com/ChurchTao/PlainPage). See the [third-party notices](https://github.com/getscissorhands/Scissorhands.NET/blob/vnext/src/ScissorHands.Web/THIRD-PARTY-NOTICES.md).
