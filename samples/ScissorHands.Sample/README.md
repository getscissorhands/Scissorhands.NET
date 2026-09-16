# ScissorHands.NET Sample

This project provides an end-to-end preview of the engine using the built-in `default` theme and local project references.

Run from this directory and select the mode explicitly:

```bash
dotnet run -- --preview
```

For IDE runs, supply `--preview` or `--build` as application arguments; the launch profile does not select a mode.

Generate static files without starting the preview server:

```bash
dotnet run -- --build
```

Generated preview and build outputs are written to `preview/` and `dist/` respectively.

## What to explore

- [Hello, ScissorHands](contents/posts/hello-scissorhands.md): rich Markdown formatting for desktop/mobile and light/dark comparisons.
- [Parent](contents/pages/parent/index.md): an index-first landing page whose numbered files and directories control reading order.
- [Child](contents/pages/parent/01-child.md), [Visible Grandchild](contents/pages/parent/02-group/visible-grandchild.md), and [Child 2](contents/pages/parent/03-child-2.md): cross-directory previous/next links with explicit slugs preserving unprefixed URLs.
- [Hidden Grandchild](contents/pages/parent/02-group/hidden-grandchild.md): a generated, tagged page omitted from navigation and the reading sequence.
- [Not found](contents/pages/not-found.md): custom content for `404.html`.

Disable the visible grandchild to remove the empty Group, or disable Parent to hide the entire branch. Plugins are disabled by default through the empty `Plugins` array.

The expected sequence is **About, Parent, Child, Visible Grandchild, Child 2**. The endpoints omit unavailable links. The numbered sample sources and explicit slugs demonstrate ordering while retaining their public URLs.

## Try locale routing

The sample leaves locale routing disabled by default. Set `"UseLocaleInUrl": true` in its `Site` settings and run `--build` to generate `dist\en-us\index.html`, locale-specific tags/navigation, and a root redirect to `en-us/`. Keep `BaseUrl: "/"` when exercising the built-in preview; prefix mounting is separately tracked in #89.

To author another language, add Markdown under `contents\posts\ko-kr` or `contents\pages\ko-kr` with explicit `locale: ko-KR` and a locale-free `slug`. Folders do not infer locale. Do not create source pages at generated locale-home routes; `pages\ko-kr\index.md` needs a distinct explicit slug to avoid claiming `/ko-kr/`.

The existing custom 404 omits `locale` and therefore follows `Site.Locale`. With locale routing enabled, an explicit conflicting locale is rejected. Shared images/theme assets and authored Markdown links are not rewritten or translated. The [browser suite](../../test/browser/README.md) creates an isolated multi-locale copy of this sample without changing its normal contents.

See the [navigation guide](../../docs/website-documentation.md#reading-order-and-previousnext-links) for the general rules, the [vNext website handoff](../../docs/website-documentation.md) for other reference material, and the [documentation website](https://getscissorhands.app/docs/) for published guides.
