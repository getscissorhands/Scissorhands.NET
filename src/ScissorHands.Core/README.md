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

`ContentMetadata.Tags` snapshots the supplied collection during initialization.

`ContentMetadata.ShowInNavigation` defaults to `false`. Set it to `true` to opt a page into site navigation. An existing page ancestor with navigation disabled also hides its descendants, regardless of their own opt-in values. The engine excludes posts, drafts, and the custom 404 page from navigation.

### Navigation data

`NavigationNode` is the shared, immutable navigation model consumed by themes. The engine prepares the complete tree, including non-clickable groups for missing parent pages:

```csharp
var group = new NavigationNode
{
    Title = "Group",
    Path = "parent/group",
    Url = null,
    Children =
    [
        new NavigationNode
        {
            Title = "Child",
            Path = "parent/group/child",
            Url = "parent/group/child",
        },
    ],
};
```

`Path` identifies the node using its escaped, base-relative route. `Url` is null for a group without a page; themes must not turn such a node's `Path` into a link. `Children` is an ordered, read-only snapshot of the supplied collection. Nodes contain no HTML, CSS classes, or DOM identifiers.

### Shared URL helpers

`ScissorHands.Core.Urls.ContentUrlHelper` centralizes URL conventions for the engine and themes:

| Method | Purpose |
| --- | --- |
| `GetContentUrl(slug)` | Escape individual content-route segments, preserving nested paths. Empty slugs become `.`; relative `.` and `..` segments are rejected. |
| `GetThemeUrl(themeSlug, path)` | Resolve an asset below `themes/{themeSlug}/`, preserving the existing asset-path convention. |
| `GetImageUrl(path)` | Remove leading slashes without slug-escaping image references. Absolute HTTP(S) URLs, queries, fragments, and existing percent encoding remain intact. |
| `GetTagUrl(tag)` | Trim and lowercase a tag, then escape it as one segment below `tags/`. Empty tags and `.` or `..` are rejected. |
| `GetLocaleSegment(locale)` | Normalize locale prefixes using the same rules as content loading; absent locales produce an empty segment. |

Content, tag, and theme URLs are relative to the site's HTML base URL rather than prepended with `Site.BaseUrl`. Image handling preserves the existing behavior and is not a general URI sanitizer. See the [theme guide](../ScissorHands.Theme/README.md#assets-and-urls) for the protected helpers exposed by view base classes.

## Site manifest

`SiteManifest` contains site-wide generation settings:

```csharp
using ScissorHands.Core.Manifests;

var site = new SiteManifest
{
    Title = "My site",
    Description = "A statically generated site.",
    Locale = "en-US",
    Theme = "default",
    SiteUrl = "https://example.com",
    BaseUrl = "/",
    UseDateInPostUrl = true,
};
```

During generation, `IsPreview` indicates whether the engine is creating the preview site or the production output.

## Theme manifest

`ThemeManifest` describes a Razor theme and its static assets:

```csharp
var theme = new ThemeManifest
{
    Name = "My Theme",
    Slug = "my-theme",
    Stylesheets = ["/assets/theme.css"],
    Scripts = ["/assets/theme.js"],
};
```

`Stylesheets` and `Scripts` are non-null read-only collections and are defensively copied during initialization.

## Plugin manifest

`PluginManifest` represents configured plugin options:

```csharp
var plugin = new PluginManifest
{
    Id = "example-plugin",
    Name = "Example Plugin",
    Options = new Dictionary<string, object?>
    {
        ["Enabled"] = true,
    },
};
```

`Id` is required when a manifest is used and must be lowercase ASCII kebab-case, such as `example-plugin`. IDs are matched ordinally, must be unique, and are never inferred from `Name`. The optional `Name` is display metadata and can change or be shared by multiple plugins without changing identity. The engine and Razor plugin components validate IDs before using manifests.

`Options` is exposed as a nullable `IReadOnlyDictionary<string, object?>` and should be treated as immutable configuration.

Name-only manifests are no longer supported. Add explicit IDs to existing configuration and update plugin implementations, dependencies, and component selectors together. See the [plugin migration guide](../ScissorHands.Plugin/README.md#migrating-from-name-based-identity).

## Service contracts

The package exposes:

- `IMarkdownService` for Markdown-to-HTML conversion
- `IThemeService` for loading manifests and copying theme assets

Both contracts support cancellation. The legacy non-cancellable `IThemeService` overloads are obsolete and scheduled for removal in the next major version.

## Learn more

- [ScissorHands.Web package](https://www.nuget.org/packages/ScissorHands.Web)
- [Documentation](https://getscissorhands.app/docs/)
- [Source code](https://github.com/getscissorhands/ScissorHands.NET)
- [Issue tracker](https://github.com/getscissorhands/ScissorHands.NET/issues)

## License

ScissorHands.NET is licensed under the [MIT License](https://github.com/getscissorhands/ScissorHands.NET/blob/vnext/LICENSE).
