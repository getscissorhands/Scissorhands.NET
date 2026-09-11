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

The sample starts with an empty `Plugins` array in `appsettings.json`. To enable an installed plugin, add an entry such as `{ "Id": "reading-time" }` using its exact lowercase kebab-case ID; `Name` is an optional display label. Array position does not control execution order. When enabling a plugin with `DependsOn` declarations, also enable its required plugin IDs; the engine resolves their order per stage in both preview and build modes. See the [plugin guide](../../src/ScissorHands.Plugin/README.md#plugin-dependencies) and [migration steps](../../src/ScissorHands.Plugin/README.md#migrating-from-name-based-identity).
