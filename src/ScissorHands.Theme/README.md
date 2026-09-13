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

The engine supplies navigation data on every generated surface: home, posts, pages, tag lists, individual tags, and 404:

- `MainLayoutBase.NavigationTree`: the complete, ordered hierarchy of immutable `NavigationNode` objects, including non-clickable groups for missing ancestors.
- `MainLayoutBase.NavigationPages`: the existing flat, read-only collection of actual visible pages, retained for compatibility.

The engine handles opt-in flags, hidden-parent suppression, path matching, locale prefixes, missing ancestors, default group labels, and sibling ordering. It builds the tree once per generation and shares it with every layout. Theme packages do not need a reference to the Web package or their own hierarchy builder.

Each node exposes `Title`, `Path`, `Url`, and read-only `Children`. A null `Url` means the node is a group, not a page link. Themes own the HTML, CSS, disclosure controls, DOM IDs, and accessibility markup. For example, a layout can render the prepared tree as a plain nested list:

```razor
@using ScissorHands.Core.Models

<nav aria-label="Primary navigation">
    <ul>
        <li><a href=".">Home</a></li>
        @RenderNodes(NavigationTree)
        <li><a href="tags">Tags</a></li>
    </ul>
</nav>

@code {
    private RenderFragment RenderNodes(IReadOnlyList<NavigationNode> nodes) => @<text>
        @foreach (var node in nodes)
        {
            <li>
                @if (node.Url is not null)
                {
                    <a href="@node.Url">@node.Title</a>
                }
                else
                {
                    <span>@node.Title</span>
                }
                @if (node.Children.Count > 0)
                {
                    <ul>@RenderNodes(node.Children)</ul>
                }
            </li>
        }
    </text>;
}
```

Both collections default to empty lists and are layout-only parameters, not content-view attributes or automatic cascading values. The existing `Documents` parameter remains the ordered post collection for the home view. Existing themes can continue to use `NavigationPages`; synthesized groups are not added to that collection.

Normal generation supplies both parameters automatically. For compatibility, the engine's `ComponentRenderer` also prepares a tree when an older caller supplies only `NavigationPages` to a layout derived from `MainLayoutBase`. An explicitly supplied tree is used unchanged. Direct component rendering should supply `NavigationTree` when displaying hierarchical navigation.

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

For content links, use the protected `GetContentUrl(string slug)` helper on `MainLayoutBase`, `IndexViewBase`, or `TagViewBase`. It escapes each path segment, preserves nested and locale-prefixed routes, and returns a base-relative URL (or `.` for an empty slug). For example, `guides/about & team` becomes `guides/about%20%26%20team`. Leading/trailing slashes are removed and backslashes are treated as path separators. A null slug throws `ArgumentNullException`; `.` or `..` path segments throw `ArgumentException`.

`PostViewBase.GetImageUrl(string path)` preserves the existing image convention: remove leading slashes, but retain absolute HTTP(S) URLs, query strings, fragments, and percent encoding. Do not use content-slug escaping for image URLs.

`PostViewBase`, `PageViewBase`, and `TagListViewBase` expose `GetTagUrl(string tag)`. It returns a base-relative link below `tags/` using the trimmed, invariant-lowercase, escaped tag name. Empty tags and `.` or `..` are rejected rather than producing invalid links.

These helpers delegate to `ScissorHands.Core.Urls.ContentUrlHelper`; URL rules are shared with the engine rather than implemented by individual themes. Existing layout helper signatures and exception behavior remain supported.

## Start from the template

The official [theme template](https://github.com/getscissorhands/theme-template) provides a complete starting structure for a new theme.

## Learn more

- [Theme documentation](https://getscissorhands.app/docs/themes/)
- [Theme template](https://github.com/getscissorhands/theme-template)
- [ScissorHands.Web package](https://www.nuget.org/packages/ScissorHands.Web)
- [Source code](https://github.com/getscissorhands/ScissorHands.NET)

## License

ScissorHands.NET is licensed under the [MIT License](https://github.com/getscissorhands/ScissorHands.NET/blob/vnext/LICENSE).
