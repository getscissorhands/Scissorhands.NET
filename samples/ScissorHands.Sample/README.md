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

To preview below a subpath, set `Site:BaseUrl` in `appsettings.json` to `/docs` or `/docs/` and open the logged URL ending in `/docs/`. Both settings normalize to `/docs/` in code without rewriting the configuration file. With `Site:UseLocaleInUrl` enabled and `Site:Locale` set to `ko-KR`, the Parent page is at `/docs/ko-kr/parent/`; otherwise it is at `/docs/parent/`. Keep the leading slash; the trailing slash is optional in settings and is supplied in generated `<base>` markup. The prefix mounts the existing `preview/` output, not an extra physical `docs/` directory. Browsing `/` redirects to `/docs/` with HTTP 302, preserving the query string; `/docs` also redirects to `/docs/`. Other requests outside the configured prefix return 404. Set `BaseUrl` back to `/` for normal root hosting; a production host must configure its own mount for `dist/`.

## What to explore

- [Hello, ScissorHands](contents/posts/hello-scissorhands.md): rich Markdown formatting for desktop/mobile and light/dark comparisons.
- [Parent](contents/pages/parent/index.md): an index-first landing page whose numbered files and directories control reading order.
- [Child](contents/pages/parent/01-child.md), [Visible Grandchild](contents/pages/parent/02-group/visible-grandchild.md), and [Child 2](contents/pages/parent/03-child-2.md): cross-directory previous/next links with explicit slugs preserving existing routes despite filename-ordering prefixes.
- [Hidden Grandchild](contents/pages/parent/02-group/hidden-grandchild.md): a generated, tagged page omitted from navigation and the reading sequence.
- [Not found](contents/pages/not-found.md): custom content for `404.html`.

Disable the visible grandchild to remove the empty Group, or disable Parent to hide the entire branch. Plugins are disabled by default through the empty `Plugins` array.

The expected sequence is **About, Parent, Child, Visible Grandchild, Child 2**. The endpoints omit unavailable links. The numbered sample sources and explicit slugs demonstrate ordering while retaining their public URLs.

## Try locale routing

The sample leaves locale routing disabled by default. Set `"UseLocaleInUrl": true` in its `Site` settings and run `--build` to generate `dist\en-us\index.html`, locale-specific tags/navigation, and a root redirect to `en-us/`. Preview also supports `/docs` or `/docs/`: browsing `/` first follows the HTTP mount redirect to `/docs/`, then the generated HTML redirect to `/docs/en-us/`. Locale and legacy tag links remain beneath the same mount; unprefixed page/asset aliases are not served.

To author another language, add Markdown under `contents\posts\ko-kr` or `contents\pages\ko-kr` with explicit `locale: ko-KR` and a locale-free `slug`. Folders do not infer locale. Do not create source pages at generated locale-home routes; `pages\ko-kr\index.md` needs a distinct explicit slug to avoid claiming `/ko-kr/`.

The existing custom 404 omits `locale` and therefore follows `Site.Locale`. With locale routing enabled, an explicit conflicting locale is rejected. Shared images/theme assets and authored Markdown links are not rewritten or translated. The [browser suite](../../test/browser/README.md) creates an isolated multi-locale copy of this sample without changing its normal contents.

See the [navigation guide](../../docs/website-documentation.md#reading-order-and-previousnext-links) for the general rules, the [vNext website handoff](../../docs/website-documentation.md) for other reference material, and the [documentation website](https://getscissorhands.app/docs/) for published guides.
