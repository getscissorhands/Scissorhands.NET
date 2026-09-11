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
