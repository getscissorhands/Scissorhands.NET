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
- [Parent](contents/pages/parent/index.md): a directory landing page with a child and a non-clickable Group containing a visible grandchild.
- [Hidden Grandchild](contents/pages/parent/group/hidden-grandchild.md): a generated, tagged page omitted from navigation.
- [Not found](contents/pages/not-found.md): custom content for `404.html`.

Disable the visible grandchild to remove the empty Group, or disable Parent to hide the entire branch. Plugins are disabled by default through the empty `Plugins` array.

See the [vNext website documentation handoff](../../docs/website-documentation.md) for route, navigation, plugin, and migration details, and the [documentation website](https://getscissorhands.app/docs/) for published guides.
