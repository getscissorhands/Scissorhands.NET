# ScissorHands.Theme

[![NuGet](https://img.shields.io/nuget/vpre/ScissorHands.Theme.svg)](https://www.nuget.org/packages/ScissorHands.Theme)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/getscissorhands/ScissorHands.NET/blob/vnext/LICENSE)

`ScissorHands.Theme` provides the Razor component base types used to build
themes for the ScissorHands.NET static site generator.

Install this package when authoring a theme. Applications that only consume a
theme should install
[`ScissorHands.Web`](https://www.nuget.org/packages/ScissorHands.Web).

## Install

```bash
dotnet add package ScissorHands.Theme --prerelease
```

ScissorHands.NET currently targets .NET 10.

## Required components

A theme provides one concrete Razor component derived from each required base
type:

- `MainLayoutBase`
- `IndexViewBase`
- `PostViewBase`
- `PageViewBase`
- `NotFoundViewBase`

Tag views are optional:

- `TagListViewBase`
- `TagViewBase`

When tag views are omitted, the engine uses its built-in implementations.

## Automatic discovery

Keep all theme components in one namespace whose normalized suffix matches the
configured `Site:Theme` slug:

```text
Theme slug: minimal-blog
Namespace:  ScissorHands.Theme.MinimalBlog
```

Normalization ignores punctuation and letter casing. For example,
`theme-template` matches `ScissorHands.Theme.Template`.

The consuming application does not register component types:

```csharp
using ScissorHands.Web;

var app = new ScissorHandsApplicationBuilder(args).Build();
await app.RunAsync();
```

`AddLayouts(...)` remains available as an explicit override for themes that
cannot follow the discovery convention.

## Theme manifest

Place `theme.json` under `themes/{slug}/`:

```json
{
  "name": "Minimal Blog",
  "version": "1.0.0",
  "description": "A minimal theme for ScissorHands.NET.",
  "slug": "minimal-blog",
  "stylesheets": [
    "/assets/theme.css"
  ],
  "scripts": [
    "/assets/theme.js"
  ]
}
```

The manifest slug must match `Site:Theme`. Stylesheet and script collections
are non-null, read-only, and defensively copied by the engine.

## Main layout

Inherit from `MainLayoutBase` and render the body through
`CascadingMainLayoutBase`:

```razor
@inherits MainLayoutBase

<CascadingMainLayoutBase
    Documents="@Documents"
    Document="@Document"
    Plugins="@Plugins"
    Theme="@Theme"
    Site="@Site">
    <!DOCTYPE html>
    <html lang="@PageLocale">
    <head>
        <base href="@Site!.BaseUrl" />
        <title>@PageTitle</title>
    </head>
    <body>
        @Body
    </body>
    </html>
</CascadingMainLayoutBase>
```

`MainLayoutBase` calculates page title, description, and locale from the site
and current document. Override the calculation methods to customize those
values.

## Page views

Page view base types expose the data needed for each generated surface:

- `IndexViewBase`: ordered post collection
- `PostViewBase`: current post document
- `PageViewBase`: current page document
- `NotFoundViewBase`: custom or generated 404 document
- `TagListViewBase`: grouped tagged documents
- `TagViewBase`: current tag and matching posts/pages

Rendered Markdown is available through `ContentDocument.Html` and can be
written with `MarkupString`.

## Assets and URLs

Store theme assets below `themes/{slug}/assets/` and list them in
`theme.json`.

Internal links and asset URLs should be base-relative, without a leading `/`,
so sites published below a path such as `/docs/` continue to work:

```razor
<link rel="stylesheet" href="themes/minimal-blog/assets/theme.css" />
<a href="tags">Tags</a>
```

## Start from the template

The official
[theme template](https://github.com/getscissorhands/theme-template) provides a
complete starting structure for a new theme.

## Learn more

- [Theme documentation](https://getscissorhands.app/docs/themes/)
- [Theme template](https://github.com/getscissorhands/theme-template)
- [ScissorHands.Web package](https://www.nuget.org/packages/ScissorHands.Web)
- [Source code](https://github.com/getscissorhands/ScissorHands.NET)

## License

ScissorHands.NET is licensed under the
[MIT License](https://github.com/getscissorhands/ScissorHands.NET/blob/vnext/LICENSE).
