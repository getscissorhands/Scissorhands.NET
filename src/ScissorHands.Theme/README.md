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
- `TagListViewBase`
- `TagViewBase`

Older themes relying on tag-view fallback must add both views; see the [migration guidance](https://github.com/getscissorhands/Scissorhands.NET/blob/vnext/docs/website-documentation.md#required-tag-views).

## Rendering content

Start from the [theme template](https://github.com/getscissorhands/theme-template) for the complete layout and discovery setup. A page view renders the content supplied by the engine:

```razor
@inherits ScissorHands.Theme.PageViewBase

<article>
    @((MarkupString)Document?.Html!)
</article>
```

The engine prepares routes and `NavigationTree`; themes control markup, styling, and interaction. A navigation node with `Url == null` is a non-clickable group. The flat `NavigationPages` parameter remains available for compatibility.

Use the inherited URL helpers instead of duplicating URL rules in theme files. Render metadata through ordinary Razor expressions to retain encoding. Plugin components select manifests by `Id`, not display `Name`.

## Optional previous/next page links

The engine supplies optional `PageNavigation` data for previous/next links. To opt in, forward it through `CascadingMainLayoutBase` and render the available links in your page view. Existing themes can ignore this data; no new view role is required, and `NavigationPages`/`NavigationTree` remain layout-only.

See the [adjacent-page context guide](https://github.com/getscissorhands/Scissorhands.NET/blob/vnext/docs/website-documentation.md#adjacent-page-context) for cascading setup, rendering examples, URL handling, and keyboard guidance.

## Locale-aware layouts

Forward the optional `LocaleContext` parameter through `CascadingMainLayoutBase`. All view bases can consume that typed cascade. With context, the layout's language metadata uses the active locale and existing post/page/tag-list `GetTagUrl` wrappers generate locale-specific links.

Use `GetHomeUrl()` for the site title/Home links and `GetTagIndexUrl()` for Tags; omit Tags when the latter returns null. Without context they retain `.` and `tags`. Full `NavigationPages`/`NavigationTree` remain layout-only and describe the active locale; no eighth theme role is required. Existing themes compile unchanged but must replace hard-coded Home/Tags routes to adopt locale-aware browsing.

This feature does not translate theme labels or site text. See the [locale context and migration guide](https://github.com/getscissorhands/Scissorhands.NET/blob/vnext/docs/website-documentation.md#locale-render-context).

## Learn more

- [vNext theme guide (website handoff)](https://github.com/getscissorhands/Scissorhands.NET/blob/vnext/docs/website-documentation.md#theme-authoring)
- [vNext migration reference](https://github.com/getscissorhands/Scissorhands.NET/blob/vnext/docs/website-documentation.md#upgrading-to-vnext)
- [Theme documentation](https://getscissorhands.app/docs/themes/)
- [Theme template](https://github.com/getscissorhands/theme-template)
- [ScissorHands.Web package](https://www.nuget.org/packages/ScissorHands.Web)
- [Source code](https://github.com/getscissorhands/ScissorHands.NET)

## License

ScissorHands.NET is licensed under the [MIT License](https://github.com/getscissorhands/ScissorHands.NET/blob/vnext/LICENSE).
