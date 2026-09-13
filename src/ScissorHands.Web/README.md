# ScissorHands.Web

[![NuGet](https://img.shields.io/nuget/vpre/ScissorHands.Web.svg)](https://www.nuget.org/packages/ScissorHands.Web)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/getscissorhands/ScissorHands.NET/blob/vnext/LICENSE)

`ScissorHands.Web` is the static site generation engine for [ScissorHands.NET](https://getscissorhands.app). It turns Markdown documents with YAML frontmatter into static HTML using Razor themes and optional content plugins.

## Features

- Markdown and YAML frontmatter processing
- Razor-based layouts and page views
- Automatic theme discovery from configuration
- Pre-Markdown, post-Markdown, and post-HTML plugin stages
- Posts, pages, tags, custom 404 pages, and content assets
- Locale-aware and date-based URL options
- Local preview server with automatic content regeneration
- Responsive built-in theme with light and dark modes

## Install

```bash
dotnet add package ScissorHands.Web --prerelease
```

ScissorHands.NET currently targets .NET 10.

## Create an application

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

## Configure the site

Add the `Site` and `Plugins` sections to `appsettings.json`:

```json
{
  "Site": {
    "Title": "My site",
    "Description": "Notes about .NET, software, and the web.",
    "Locale": "en-US",
    "Author": "Your name",
    "Theme": "default",
    "SiteUrl": "https://example.com",
    "BaseUrl": "/",
    "UseLocaleInUrl": false,
    "UseDateInPostUrl": true,
    "Debug": false
  },
  "Plugins": []
}
```

Use `BaseUrl` when publishing below a subpath, such as `/docs/`.

## Add content

ScissorHands reads Markdown from:

```text
contents/
├── images/
├── pages/
└── posts/
```

Example post:

```markdown
---
title: Hello, ScissorHands
description: My first generated post.
slug: hello-scissorhands
published: 2026-09-11
author: Your name
locale: en-US
tags:
  - dotnet
  - static-site
draft: false
---

# Hello, ScissorHands

Write the post in Markdown.
```

Supported frontmatter fields are:

- `title`
- `slug`
- `description`
- `locale`
- `author`
- `twitter_handle`
- `hero_image`
- `published`
- `tags`
- `draft`
- `show_in_navigation`

Invalid frontmatter, unsafe routes, and duplicate output paths fail the build with the source file included in the error.

To provide a custom not-found page, add a page with `slug: 404.html`.

### Page navigation

Opt a page under `contents/pages/` into the built-in navigation:

```markdown
---
title: About
slug: about
show_in_navigation: true
---

# About
```

`show_in_navigation` accepts `true` or `false` and defaults to `false` when omitted. Hidden pages are still generated and can be reached by their URL; this setting only controls navigation links, not access to content.

The built-in theme keeps Home and Tags, then adds opted-in pages using their titles and generated slugs. Page slugs determine the navigation hierarchy; no separate parent field is needed. Siblings are ordered by title, with slug as the tie-breaker, using ordinal comparisons. Posts, drafts, and the custom 404 page are never included. The same navigation appears on every generated surface in both preview and build modes, including sites hosted under a `BaseUrl` subpath.

For example, with all four pages opted in:

```text
Docs                       (slug: docs)
  Deployment               (slug: docs/deployment)
    GitHub Pages           (slug: docs/deployment/github-pages)
  Quickstart               (slug: docs/quickstart)
```

**A hidden parent hides its entire descendant branch.** If `docs/deployment` sets `show_in_navigation: false` or omits the field, GitHub Pages is also hidden, even when it sets the field to `true`. Hiding `docs` hides all four navigation links. These pages are still generated and accessible by URL.

Ancestors are matched on complete slug path segments, so `docs` does not govern `docs-other`. A directory segment with no corresponding page is not a hidden parent: the built-in theme creates a non-clickable group for that missing level, but only when it contains at least one visible descendant. Empty groups disappear automatically.

For example, when `docs` is visible and `docs/deployment` has no page, a visible `docs/deployment/github-pages` creates a **Deployment** group under Docs. An invisible `docs/deployment/netlify` is omitted. If both children are invisible, Deployment is omitted too. A real Deployment page with navigation disabled still hides the branch; it is never replaced with a group.

Group labels come from the missing path segment, replacing hyphens and underscores with spaces and applying invariant title casing (`deployment-tools` becomes `Deployment Tools`). Groups do not create pages, output files, or placeholder links. Locale routing prefixes are not synthesized as groups when `UseLocaleInUrl` is enabled, and locale-prefixed routes remain distinct.

Existing parent titles remain ordinary links to their pages; missing-parent labels are plain text. Adjacent buttons expand and collapse child lists with mouse, touch, Enter, or Space; Escape closes the current expanded group and returns focus to its button. Moving focus or clicking outside navigation closes the menus. Without JavaScript, the full nested list remains accessible.

Custom themes can render the layout's `NavigationPages` collection; see the [theme guide](../ScissorHands.Theme/README.md#page-navigation).

## Preview and build

Start the local preview server:

```bash
dotnet run -- --preview
```

Generate the static site into `dist/`:

```bash
dotnet run -- --build
```

Preview mode regenerates the site after content or theme-file changes. Refresh the browser to display regenerated HTML. Razor or C# changes still require recompilation, typically with `dotnet watch`.

## Themes

A custom theme provides concrete Razor components derived from:

- `MainLayoutBase`
- `IndexViewBase`
- `PostViewBase`
- `PageViewBase`
- `NotFoundViewBase`

`TagListViewBase` and `TagViewBase` are optional; the built-in tag views are used when they are omitted.

Keep the components in one namespace whose normalized suffix matches the configured theme slug. For example:

```text
Site:Theme = minimal-blog
Namespace  = ScissorHands.Theme.MinimalBlog
```

Theme assets and metadata live under `themes/{slug}/`, including a `theme.json` manifest.

## Plugins

Install plugin packages and configure each enabled plugin by its stable, unique kebab-case ID:

```json
{
  "Plugins": [
    {
      "Id": "example-plugin",
      "Name": "Example Plugin",
      "Options": {
        "Enabled": true
      }
    }
  ]
}
```

Plugins can transform a document before Markdown conversion, after Markdown conversion, or after the final Razor HTML render.

At every stage, each plugin's output feeds the next. IDs must be lowercase ASCII kebab-case and are matched ordinally; installed plugins without a manifest remain disabled. Manifest `Name` values are optional display labels, not identifiers. Missing or invalid IDs, duplicate IDs, and unmatched manifests fail without name fallback or automatic normalization.

Execution order is resolved from optional, stage-scoped `DependsOn` declarations provided by plugins, not the `Plugins` array position or registration order. Declared dependencies must be installed and enabled. Missing or disabled dependencies, invalid declarations, and cycles fail when the runner is constructed, before any plugin hooks execute. Dependencies are never automatically installed or enabled.

Within each stage, the engine chooses among ready plugins by ordinal ID ordering for deterministic output. A plugin without dependency declarations must not rely on another plugin's execution order.

**Migration:** plugin implementations must now expose `Id`, configured manifests must include it, dependency references must use IDs, and Razor plugin components must select with `Id` instead of `Name`. Rebuild existing plugin and consuming assemblies; name-only configuration is no longer accepted. Follow the [plugin ID migration guide](../ScissorHands.Plugin/README.md#migrating-from-name-based-identity).

## Learn more

- [Documentation](https://getscissorhands.app/docs/)
- [Source code](https://github.com/getscissorhands/ScissorHands.NET)
- [Theme template](https://github.com/getscissorhands/theme-template)
- [Official plugins](https://github.com/getscissorhands/plugins)
- [Issue tracker](https://github.com/getscissorhands/ScissorHands.NET/issues)

## License

ScissorHands.NET is licensed under the [MIT License](https://github.com/getscissorhands/ScissorHands.NET/blob/vnext/LICENSE).

The built-in theme is adapted from [PlainPage](https://github.com/ChurchTao/PlainPage). Its attribution is included in [`THIRD-PARTY-NOTICES.md`](THIRD-PARTY-NOTICES.md).
