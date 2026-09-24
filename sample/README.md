# ScissorHands.NET Sample

A runnable sample using the built-in `default` theme and local project references. Requires the [repository SDK](../global.json).

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

Open the logged preview URL, refresh after edits, and stop with Ctrl+C.

## Documentation

- [Sample walkthrough](../docs/website-documentation.md#sample-walkthrough): content examples, navigation, localization, and subpath settings
- [vNext reference](../docs/website-documentation.md): full configuration and authoring guides
