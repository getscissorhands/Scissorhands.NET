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

`PageNavigation` and `PageNavigationLink` provide optional, immutable previous/next page data for themes. They are generated models, not frontmatter.

See the [page navigation API reference](https://github.com/getscissorhands/Scissorhands.NET/blob/vnext/docs/website-documentation.md#pagenavigation-and-pagenavigationlink) and [theme integration guide](https://github.com/getscissorhands/Scissorhands.NET/blob/vnext/docs/website-documentation.md#adjacent-page-context) for their contracts and usage.

## Locale render context

`LocaleContext` is an optional immutable render snapshot with normalized `Locale`, resolved `Route`, base-relative `HomeUrl` and nullable `TagIndexUrl`. A null tag-index target means that locale has no tagged content. `GetTagUrl(tag)` composes a raw tag with the prepared home URL, reusing shared tag escaping without escaping the route twice.

The engine supplies this context only when `Site.UseLocaleInUrl` is enabled. Existing static `ContentUrlHelper` methods retain their context-free behavior; neither the context nor those formatters is a general URL sanitizer. See the [locale context guide](https://github.com/getscissorhands/Scissorhands.NET/blob/vnext/docs/website-documentation.md#locale-render-context).

## Learn more

- [ScissorHands.Web package](https://www.nuget.org/packages/ScissorHands.Web)
- [vNext API reference (website handoff)](https://github.com/getscissorhands/Scissorhands.NET/blob/vnext/docs/website-documentation.md#core-api-reference)
- [vNext migration reference](https://github.com/getscissorhands/Scissorhands.NET/blob/vnext/docs/website-documentation.md#upgrading-to-vnext)
- [Documentation](https://getscissorhands.app/docs/)
- [Source code](https://github.com/getscissorhands/ScissorHands.NET)
- [Issue tracker](https://github.com/getscissorhands/ScissorHands.NET/issues)

## License

ScissorHands.NET is licensed under the [MIT License](https://github.com/getscissorhands/ScissorHands.NET/blob/vnext/LICENSE).
