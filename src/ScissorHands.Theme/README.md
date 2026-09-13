# ScissorHands.Theme

[![NuGet](https://img.shields.io/nuget/vpre/ScissorHands.Theme.svg)](https://www.nuget.org/packages/ScissorHands.Theme)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/getscissorhands/ScissorHands.NET/blob/vnext/LICENSE)

`ScissorHands.Theme` provides the Razor component base types used to build themes for the ScissorHands.NET static site generator.

Install this package when authoring a theme. Applications that only consume a theme should install [`ScissorHands.Web`](https://www.nuget.org/packages/ScissorHands.Web).

## Install

```bash
dotnet add package ScissorHands.Theme --prerelease
```

ScissorHands.NET currently targets .NET 10.

## Required components

A theme provides one concrete Razor component derived from each required base type:

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

Keep all theme components in one namespace whose normalized suffix matches the configured `Site:Theme` slug:

```text
Theme slug: minimal-blog
Namespace:  ScissorHands.Theme.MinimalBlog
```

Normalization ignores punctuation and letter casing. For example, `theme-template` matches `ScissorHands.Theme.Template`.

The consuming application does not register component types:

```csharp
using ScissorHands.Web;

var app = new ScissorHandsApplicationBuilder(args).Build();
await app.RunAsync();
```

`AddLayouts(...)` remains available as an explicit override for themes that cannot follow the discovery convention.

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

The manifest slug must match `Site:Theme`. Stylesheet and script collections are non-null, read-only, and defensively copied by the engine.

## Main layout

Inherit from `MainLayoutBase` and render the body through `CascadingMainLayoutBase`:

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

`MainLayoutBase` calculates page title, description, and locale from the site and current document. Override the calculation methods to customize those values.

### Page navigation

The engine supplies `MainLayoutBase.NavigationPages` on every generated surface: home, posts, pages, tag lists, individual tags, and 404. It is a read-only list of pages that opt in with `show_in_navigation: true` frontmatter. The default is off; posts, drafts, and the custom 404 page are excluded. Pages are ordered by title and then slug using ordinal comparisons.

An existing page ancestor with navigation disabled removes its entire descendant branch from this collection, regardless of descendant opt-in flags. Ancestors are matched on full slug path segments. Missing intermediate pages do not themselves hide a branch.

The collection remains flat for custom-theme compatibility and contains only actual pages. The built-in theme preserves every ancestor level in nested disclosure menus. Existing parents retain their page links; missing parents become non-clickable labels only when they contain a visible descendant. These synthesized groups are not added to `NavigationPages` and do not create output pages. Group labels are derived from their slug segments; see the [navigation guide](../ScissorHands.Web/README.md#page-navigation).

A custom layout can choose the same hierarchy or render a flat list alongside its existing navigation:

```razor
<nav aria-label="Primary navigation">
    <a href=".">Home</a>
    <a href="tags">Tags</a>
    @foreach (var pageDocument in NavigationPages)
    {
        <a href="@GetContentUrl(pageDocument.Metadata.Slug)">@pageDocument.Metadata.Title</a>
    }
</nav>
```

`NavigationPages` defaults to an empty list when not supplied. It is a layout parameter, not a content-view parameter or cascading value. The existing `Documents` parameter remains the ordered post collection for the home view. Existing custom themes need to add navigation markup to display opted-in pages; the built-in theme already does so.

## Page views

Page view base types expose the data needed for each generated surface:

- `IndexViewBase`: ordered post collection
- `PostViewBase`: current post document
- `PageViewBase`: current page document
- `NotFoundViewBase`: custom or generated 404 document
- `TagListViewBase`: grouped tagged documents
- `TagViewBase`: current tag and matching posts/pages

Rendered Markdown is available through `ContentDocument.Html` and can be written with `MarkupString`.

## Plugin components

When a theme renders a component derived from `PluginComponentBase`, supply its required kebab-case `Id` parameter to select the plugin manifest. `Name` is display-only and cannot be used as a selector. Existing themes using name-based selectors must update and rebuild; see the [plugin migration guide](../ScissorHands.Plugin/README.md#migrating-from-name-based-identity).

## Assets and URLs

Store theme assets below `themes/{slug}/assets/` and list them in `theme.json`.

Internal links and asset URLs should be base-relative, without a leading `/`, so sites published below a path such as `/docs/` continue to work:

```razor
<link rel="stylesheet" href="@GetThemeUrl("/assets/theme.css")" />
<a href="tags">Tags</a>
```

Layouts derived from `MainLayoutBase` can use the protected `GetThemeUrl(string path)` helper for theme assets. With a theme slug of `minimal-blog`, the example returns `themes/minimal-blog/assets/theme.css`. The helper trims leading and trailing `/` characters from the slug and leading `/` characters from the path.

The returned URL is relative to the `<base href="@Site!.BaseUrl" />` in the layout; it does not prepend `Site.BaseUrl`. Supply the `Theme` parameter before calling the helper. A missing theme throws `InvalidOperationException`, and a null path throws `ArgumentNullException`.

For content links, use the protected `GetContentUrl(string slug)` helper. It escapes each path segment, preserves nested and locale-prefixed routes, and returns a base-relative URL (or `.` for an empty slug). For example, `guides/about & team` becomes `guides/about%20%26%20team`. Leading/trailing slashes are removed and backslashes are treated as path separators. A null slug throws `ArgumentNullException`; `.` or `..` path segments throw `ArgumentException`.

## Start from the template

The official [theme template](https://github.com/getscissorhands/theme-template) provides a complete starting structure for a new theme.

## Learn more

- [Theme documentation](https://getscissorhands.app/docs/themes/)
- [Theme template](https://github.com/getscissorhands/theme-template)
- [ScissorHands.Web package](https://www.nuget.org/packages/ScissorHands.Web)
- [Source code](https://github.com/getscissorhands/ScissorHands.NET)

## License

ScissorHands.NET is licensed under the [MIT License](https://github.com/getscissorhands/ScissorHands.NET/blob/vnext/LICENSE).
