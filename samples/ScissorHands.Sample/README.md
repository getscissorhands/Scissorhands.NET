# ScissorHands.NET Sample

This project provides an end-to-end preview of the engine using the built-in `default` theme and local project references.

Run from this directory. The launch profile enables preview mode:

```bash
dotnet run -- --preview
```

The launch profile also starts preview mode automatically when run from an IDE.

Generate static files without starting the preview server:

```bash
dotnet run --no-launch-profile -- --build
```

Generated preview and build outputs are written to `preview/` and `dist/` respectively.

The [Hello, ScissorHands post](contents/posts/hello-scissorhands.md) is a Markdown rendering showcase with heading hierarchy, paragraphs, emphasis, links, nested lists, a blockquote, C# and JSON code blocks, a table, a horizontal rule, and a bundled image. Use it to compare typography and spacing across light/dark themes and desktop/mobile layouts.

The About page opts into the built-in navigation with `show_in_navigation: true` in `contents/pages/about.md`. Pages are hidden from navigation by default; set the field to `false` or remove it to hide the link while keeping the page accessible at its URL. Preview regeneration updates the navigation across the site.

The hierarchy sample uses generic parent, child, and grandchild pages. Their slugs are inferred from their paths; `contents/pages/parent/index.md` is the landing page at `parent`, not `parent/index`:

```text
Parent                  (parent)
  Child                 (parent/child)
  Group                 (parent/group; no page, non-clickable)
    Visible Grandchild  (parent/group/visible-grandchild)
```

`parent/group/hidden-grandchild` is generated but omitted from navigation. There is no `parent/group` page, so the built-in menu creates Group as a non-clickable label. Disable Visible Grandchild as well and the empty Group disappears. Both grandchild pages are still generated.

Use the buttons beside Parent and Group to expand their children; Parent itself remains a page link. Disable navigation in `contents/pages/parent/index.md` to hide the entire branch. Child opt-in flags cannot override a hidden existing parent.

To give Group a clickable landing page, add `contents/pages/parent/group/index.md` with a title and `show_in_navigation: true`; no slug is required. Explicit slugs remain available to override inferred routes.

The sample starts with an empty `Plugins` array in `appsettings.json`. To enable an installed plugin, add an entry such as `{ "Id": "reading-time" }` using its exact lowercase kebab-case ID; `Name` is an optional display label. Array position does not control execution order. When enabling a plugin with `DependsOn` declarations, also enable its required plugin IDs; the engine resolves their order per stage in both preview and build modes. See the [plugin guide](../../src/ScissorHands.Plugin/README.md#plugin-dependencies) and [migration steps](../../src/ScissorHands.Plugin/README.md#migrating-from-name-based-identity).
