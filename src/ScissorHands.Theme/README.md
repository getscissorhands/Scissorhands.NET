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

`MainLayoutBase.PageNavigation` is an optional engine-supplied `PageNavigation` parameter, defaulting to an empty instance. It contains nullable `Previous` and `Next` links with immutable text `Title` and preformatted, base-relative `Url` values. The engine snapshots these values before document hooks: eligible file-backed pages use filename-based depth-first reading order, followed by source-less pages in title/slug order. This is independent of the slug grouping used by `NavigationTree`, and is generated data rather than frontmatter.

To opt in, add `PageNavigation="@PageNavigation"` to your layout's existing `<CascadingMainLayoutBase>` element, retaining its other parameters and child content. It provides a typed cascade consumed by the nullable `PageViewBase.PageNavigation` property. Render available links below your page content, for example:

```razor
@if (PageNavigation?.Previous is not null || PageNavigation?.Next is not null)
{
    <nav class="page-navigation" aria-label="Page navigation">
        @if (PageNavigation?.Previous is { } previous)
        {
            <a href="@previous.Url" rel="prev">Previous: @previous.Title</a>
        }
        @if (PageNavigation?.Next is { } next)
        {
            <a href="@next.Url" rel="next">Next: @next.Title</a>
        }
    </nav>
}
```

Use the supplied URLs directly, without prefixing the site's base URL or escaping them again, and render titles through ordinary Razor expressions, never `MarkupString`. Omit missing endpoints and the entire region when both are absent. The built-in theme includes these links; custom themes that ignore the optional parameter keep their existing behavior. No new theme role is required, and `NavigationPages`/`NavigationTree` remain layout-only, not newly cascaded.

## Learn more

- [vNext theme guide (website handoff)](https://github.com/getscissorhands/Scissorhands.NET/blob/vnext/docs/website-documentation.md#theme-authoring)
- [vNext migration reference](https://github.com/getscissorhands/Scissorhands.NET/blob/vnext/docs/website-documentation.md#upgrading-to-vnext)
- [Theme documentation](https://getscissorhands.app/docs/themes/)
- [Theme template](https://github.com/getscissorhands/theme-template)
- [ScissorHands.Web package](https://www.nuget.org/packages/ScissorHands.Web)
- [Source code](https://github.com/getscissorhands/ScissorHands.NET)

## License

ScissorHands.NET is licensed under the [MIT License](https://github.com/getscissorhands/ScissorHands.NET/blob/vnext/LICENSE).
