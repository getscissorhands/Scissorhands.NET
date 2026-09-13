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

The eligible sequence is **About, Parent, Child, Visible Grandchild, Child 2**. The first page has no previous link and the last has no next link. Files and directories share ordinal filename ordering, with `index.md` first in each directory. Renaming a source changes its reading position; an explicit slug preserves its URL. The navigation hierarchy still follows slugs rather than source directories.

Custom loaders may supply pages without a source path. Those pages follow file-backed pages in ordinal title/slug order and participate in the same previous/next sequence. The standard Markdown loader always supplies source paths.

See the [vNext website documentation handoff](../../docs/website-documentation.md) for route, navigation, plugin, and migration details, and the [documentation website](https://getscissorhands.app/docs/) for published guides.
