# ScissorHands.Web

[![NuGet](https://img.shields.io/nuget/vpre/ScissorHands.Web.svg)](https://www.nuget.org/packages/ScissorHands.Web)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/getscissorhands/ScissorHands.NET/blob/vnext/LICENSE)

`ScissorHands.Web` is the static site generation engine for
[ScissorHands.NET](https://getscissorhands.app). It turns Markdown documents
with YAML frontmatter into static HTML using Razor themes and optional content
plugins.

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

No theme component types need to be registered in `Program.cs`. The built-in
theme is used when `Site:Theme` is `default`.

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

Invalid frontmatter, unsafe routes, and duplicate output paths fail the build
with the source file included in the error.

To provide a custom not-found page, add a page with `slug: 404.html`.

## Preview and build

Start the local preview server:

```bash
dotnet run -- --preview
```

Generate the static site into `dist/`:

```bash
dotnet run -- --build
```

Preview mode regenerates the site after content or theme-file changes. Refresh
the browser to display regenerated HTML. Razor or C# changes still require
recompilation, typically with `dotnet watch`.

## Themes

A custom theme provides concrete Razor components derived from:

- `MainLayoutBase`
- `IndexViewBase`
- `PostViewBase`
- `PageViewBase`
- `NotFoundViewBase`

`TagListViewBase` and `TagViewBase` are optional; the built-in tag views are
used when they are omitted.

Keep the components in one namespace whose normalized suffix matches the
configured theme slug. For example:

```text
Site:Theme = minimal-blog
Namespace  = ScissorHands.Theme.MinimalBlog
```

Theme assets and metadata live under `themes/{slug}/`, including a
`theme.json` manifest.

## Plugins

Install plugin packages and configure each enabled plugin by its unique name:

```json
{
  "Plugins": [
    {
      "Name": "Example Plugin",
      "Options": {
        "Enabled": true
      }
    }
  ]
}
```

Plugins can transform a document before Markdown conversion, after Markdown
conversion, or after the final Razor HTML render.

## Learn more

- [Documentation](https://getscissorhands.app/docs/)
- [Source code](https://github.com/getscissorhands/ScissorHands.NET)
- [Theme template](https://github.com/getscissorhands/theme-template)
- [Official plugins](https://github.com/getscissorhands/plugins)
- [Issue tracker](https://github.com/getscissorhands/ScissorHands.NET/issues)

## License

ScissorHands.NET is licensed under the
[MIT License](https://github.com/getscissorhands/ScissorHands.NET/blob/vnext/LICENSE).

The built-in theme is adapted from
[PlainPage](https://github.com/ChurchTao/PlainPage). Its attribution is
included in `THIRD-PARTY-NOTICES.md`.
