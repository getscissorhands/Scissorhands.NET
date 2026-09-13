# ScissorHands.Core

[![NuGet](https://img.shields.io/nuget/vpre/ScissorHands.Core.svg)](https://www.nuget.org/packages/ScissorHands.Core)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/getscissorhands/ScissorHands.NET/blob/vnext/LICENSE)

`ScissorHands.Core` contains the shared contracts, manifests, models, and command options used by the ScissorHands.NET static site generator.

Most applications should install [`ScissorHands.Web`](https://www.nuget.org/packages/ScissorHands.Web), which references this package automatically. Install `ScissorHands.Core` directly when building a custom integration, theme service, or Markdown service.

## Install

```bash
dotnet add package ScissorHands.Core --prerelease
```

ScissorHands.NET currently targets .NET 10.

## Content models

`ContentDocument` represents a Markdown source document as it moves through the generation pipeline:

```csharp
using ScissorHands.Core.Models;

var document = new ContentDocument
{
    SourcePath = "contents/posts/hello.md",
    Kind = ContentKind.Post,
    Metadata = new ContentMetadata
    {
        Title = "Hello",
        Slug = "hello",
        Tags = ["dotnet", "static-site"],
        Published = DateTimeOffset.UtcNow,
    },
    Markdown = "# Hello",
};
```

The package also provides site/theme/plugin manifests, immutable `NavigationNode` data, shared URL helpers, command options, and cancellation-aware Markdown/theme service contracts. Treat collection inputs as read-only.

vNext changes several collection APIs and requires plugin IDs. Legacy non-cancellable `IThemeService` overloads are obsolete; review the migration reference before upgrading.

## Generated page navigation

`PageNavigation` is an immutable, per-page snapshot with nullable `Previous` and `Next` links. Each `PageNavigationLink` contains a text `Title` and an engine-formatted, base-relative `Url`; both strings default to empty. A new `PageNavigation` has no neighbors.

The engine prepares this data before document hooks. Eligible file-backed pages follow a filename-based depth-first reading order; pages without source paths follow afterward in title/slug order. This reading sequence is separate from the slug-based grouping in `NavigationTree`. These models are generated data, not frontmatter or new `ContentMetadata` fields.

Themes may opt into the per-page links without receiving the full navigation collections. The existing layout-only `NavigationPages` and `NavigationTree` contracts remain available.

## Learn more

- [ScissorHands.Web package](https://www.nuget.org/packages/ScissorHands.Web)
- [vNext API reference (website handoff)](https://github.com/getscissorhands/Scissorhands.NET/blob/vnext/docs/website-documentation.md#core-api-reference)
- [vNext migration reference](https://github.com/getscissorhands/Scissorhands.NET/blob/vnext/docs/website-documentation.md#upgrading-to-vnext)
- [Documentation](https://getscissorhands.app/docs/)
- [Source code](https://github.com/getscissorhands/ScissorHands.NET)
- [Issue tracker](https://github.com/getscissorhands/ScissorHands.NET/issues)

## License

ScissorHands.NET is licensed under the [MIT License](https://github.com/getscissorhands/ScissorHands.NET/blob/vnext/LICENSE).
