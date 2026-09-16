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

## Site base URL

`SiteManifest.BaseUrl` normalizes a missing trailing slash on site path prefixes during initialization. `/docs` and `/docs/` both read back as `/docs/`; `/manual/docs` becomes `/manual/docs/`, and `/` stays `/`. This applies to direct object initialization and configuration binding, so themes, plugins, build output and preview consume the same effective value. The public property remains init-only, and the source configuration is not rewritten.

This is trailing-slash normalization, not URL validation: absolute URLs, network-relative URLs (`//host/path`), relative paths without a leading slash, and values containing backslashes, queries or fragments are not rewritten by this rule. Their existing behavior and the broader supported-URL policy are unchanged; preservation does not establish safety or preview support.

## Generated page navigation

`PageNavigation` and `PageNavigationLink` provide optional, immutable previous/next page data for themes. They are generated models, not frontmatter.

See the [page navigation API reference](https://github.com/getscissorhands/Scissorhands.NET/blob/vnext/docs/website-documentation.md#pagenavigation-and-pagenavigationlink) and [theme integration guide](https://github.com/getscissorhands/Scissorhands.NET/blob/vnext/docs/website-documentation.md#adjacent-page-context) for their contracts and usage.

## Learn more

- [ScissorHands.Web package](https://www.nuget.org/packages/ScissorHands.Web)
- [vNext API reference (website handoff)](https://github.com/getscissorhands/Scissorhands.NET/blob/vnext/docs/website-documentation.md#core-api-reference)
- [vNext migration reference](https://github.com/getscissorhands/Scissorhands.NET/blob/vnext/docs/website-documentation.md#upgrading-to-vnext)
- [Documentation](https://getscissorhands.app/docs/)
- [Source code](https://github.com/getscissorhands/ScissorHands.NET)
- [Issue tracker](https://github.com/getscissorhands/ScissorHands.NET/issues)

## License

ScissorHands.NET is licensed under the [MIT License](https://github.com/getscissorhands/ScissorHands.NET/blob/vnext/LICENSE).
