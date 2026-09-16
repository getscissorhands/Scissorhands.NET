# ScissorHands.NET Sample

A runnable sample using the built-in `default` theme and local project references. Requires the [repository SDK](../../global.json).

Run from this directory and select the mode explicitly:

Bash:

```bash
dotnet run -- --preview
```

PowerShell:

```powershell
dotnet run -- --preview
```

For IDE runs, supply `--preview` or `--build` as application arguments; the launch profile does not select a mode.

Generate static files without starting the preview server:

Bash:

```bash
dotnet run -- --build
```

PowerShell:

```powershell
dotnet run -- --build
```

Generated preview and build outputs are written to `preview/` and `dist/` respectively.

Open the logged preview URL, refresh after edits, and stop with Ctrl+C.

## Explore

Start with the [post](contents/posts/hello-scissorhands.md), [page tree](contents/pages/parent/index.md), and [custom 404](contents/pages/not-found.md).

See the [sample walkthrough](../../docs/website-documentation.md#sample-walkthrough) for navigation experiments and locale/subpath settings, and the [vNext reference](../../docs/website-documentation.md) for the full guides.
