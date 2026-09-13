# Website documentation handoff

This document preserves the detailed material extracted from the repository READMEs. It is organized into guides that can be published on [getscissorhands.app](https://getscissorhands.app).

**Version scope:** the current vNext implementation, targeting .NET 10. These instructions must not be presented as applying to older packages without checking their compatibility. The shorter READMEs retain installation, minimal usage, essential warnings, and links.

## Publishing notes

Each guide below names its intended website destination. Existing destinations were present when this handoff was prepared; proposed destinations need to be created and added to the documentation navigation.

| Guide in this document | Website destination | Publication action |
| --- | --- | --- |
| [Quickstart](#quickstart) | `/docs/quickstart/` | Update the application bootstrap and first-content workflow |
| [Site configuration](#site-configuration) | `/docs/configuration/` | Update site settings and plugin configuration |
| [Content and frontmatter](#content-and-frontmatter) | `/docs/front-matter/`, `/docs/posts/`, `/docs/pages/` | Update supported fields and content guidance |
| [Page routes and navigation](#page-routes-and-navigation) | `/docs/pages/`; proposed `/docs/navigation/` | Document directory indexes and navigation visibility |
| [Preview and build](#preview-and-build) | `/docs/build/` | Document modes, regeneration, and output |
| [Theme authoring](#theme-authoring) | `/docs/themes/` | Update discovery, prepared data, rendering, and URL helpers |
| [Plugin authoring](#plugin-authoring) | `/docs/plugins/` | Replace name-based identity and add dependency rules |
| [Core API reference](#core-api-reference) | Proposed `/docs/api/` | Publish shared models, manifests, services, and URL conventions |
| [Upgrading to vNext](#upgrading-to-vnext) | Proposed `/docs/migration/` | Publish source, binary, configuration, and route migration guidance |

The existing website needs several corrections before it can replace this reference:

- Plugin examples must use stable `Id` values, not name-based configuration or component selection.
- Remove the legacy prerequisite to run `scripts/setup-gh-auth.*` or set `GH_PACKAGE_USERNAME` and `GH_PACKAGE_TOKEN`. Those helpers have been removed; the current repository build does not consume those variables. GitHub Packages publishing remains supported and authenticates directly in the release workflow.
- The page guide must no longer claim that directories never affect URLs. Paths determine inferred routes when an explicit slug is absent.
- The frontmatter reference must include `draft` and `show_in_navigation`.
- The theme guide must explain engine-prepared `NavigationTree` data and shared URL helpers.
- The quickstart should use automatic theme discovery by default. Explicit `AddLayouts(...)` registration remains an override, not a required setup step.
- Examples of content headings must distinguish theme presentation from engine behavior. The built-in page and post views render the Markdown body; they do not automatically add the frontmatter title as an H1.

Publish these guides as vNext/preview documentation, or retain separate documentation for older package versions. Replace the README handoff links with the corresponding website links after publication. Package READMEs use absolute links because they are also displayed on NuGet.

The package README handoff links target the repository's `vnext` branch. Make this document available there before publishing packages that include those links; during review it is available in the worktree and pull request.

## Quickstart

Website destination: `/docs/quickstart/`.

ScissorHands.NET is a .NET 10 static site generator that combines Markdown, YAML frontmatter, Razor themes, and optional plugins.

### Create an application

Create an empty ASP.NET Core application and install the engine:

```bash
dotnet new web -n MyScissorHandsApp
cd MyScissorHandsApp
dotnet add package ScissorHands.Web --prerelease
```

Replace `Program.cs` with:

```csharp
using ScissorHands.Web;

var app = new ScissorHandsApplicationBuilder(args).Build();
await app.RunAsync();
```

Add the following sections to `appsettings.json`:

```json
{
  "Site": {
    "Title": "My site",
    "Description": "Notes about .NET, software, and the web.",
    "Locale": "en-US",
    "Author": "Your name",
    "Theme": "default",
    "SiteUrl": "https://example.com",
    "BaseUrl": "/",
    "UseLocaleInUrl": false,
    "UseDateInPostUrl": true,
    "Debug": false
  },
  "Plugins": []
}
```

No theme component types need to be registered in `Program.cs`. Setting `Site:Theme` to `default` selects the built-in theme.

### Add the first post

Create `contents/posts/hello.md`:

```markdown
---
title: Hello, ScissorHands
description: My first generated post.
slug: hello-scissorhands
published: 2026-09-11
author: Your name
locale: en-US
tags:
  - dotnet
  - static-site
draft: false
---

# Hello, ScissorHands

Write the post in Markdown.
```

Run from the application directory so configuration and content resolve there:

```bash
dotnet run -- --preview
```

Generate deployable static files without starting a preview server:

```bash
dotnet run -- --build
```

The repository's `samples/ScissorHands.Sample` application demonstrates these features using local project references. Its README retains the sample-specific run instructions.

## Site configuration

Website destination: `/docs/configuration/`.

The `Site` section in `appsettings.json` provides site-wide settings. Individual document frontmatter provides document metadata.

| Setting | Purpose |
| --- | --- |
| `Title` | Site title used by the theme |
| `Description` | Site description; the engine also makes its rendered HTML available |
| `Locale` | Default locale used when resolving content locale |
| `Author` | Site-level author information available to themes |
| `HeroImage` | Site-level hero image reference available to themes and plugins |
| `Theme` | Theme slug; use `default` for the built-in theme |
| `SiteUrl` | Public site URL |
| `BaseUrl` | Site base path, such as `/` or `/project/` |
| `UseLocaleInUrl` | Include a normalized locale prefix in content routes |
| `UseDateInPostUrl` | Include the publication date in post routes |
| `Debug` | Site debug setting |

Use `BaseUrl` when publishing below a subpath. Generated navigation and shared URL helpers produce base-relative links rather than hardcoding the domain root.

During generation, the engine sets `SiteManifest.IsPreview` to indicate preview or production output. It also populates `DescriptionInHtml` from the site description.

The `Plugins` array selects installed plugins by ID:

```json
{
  "Plugins": [
    {
      "Id": "example-plugin",
      "Name": "Example Plugin",
      "Options": {
        "Option1": "value1"
      }
    }
  ]
}
```

`Name` is optional display metadata. Options are plugin-specific; validate and interpret them in the plugin rather than assuming every plugin supports the same keys. Installed plugins without a matching manifest remain disabled.

## Content and frontmatter

Website destinations: `/docs/front-matter/`, `/docs/posts/`, and `/docs/pages/`.

### Content directories

The engine reads Markdown from the post and page directories:

```text
contents/
  images/
  pages/
  posts/
```

Posts represent articles and can use publication dates in their URLs. Pages provide stable content sections and can participate in navigation. Content images are copied into the generated site.

### Supported frontmatter fields

Frontmatter is YAML between opening and closing `---` delimiters.

| Field | Purpose |
| --- | --- |
| `title` | Document title |
| `slug` | Explicit route override; omitted or blank values use file-based inference |
| `description` | Document description |
| `locale` | Document locale override |
| `author` | Document author metadata |
| `twitter_handle` | Author's Twitter handle metadata |
| `hero_image` | Hero image path or URL |
| `published` | Publication date/time |
| `tags` | Optional tags associated with the document |
| `draft` | Whether the document should be excluded from generation |
| `show_in_navigation` | Whether a page opts into navigation; defaults to `false` |

Drafts are skipped in both preview and build modes. `show_in_navigation` controls navigation membership, not whether a page is generated.

Invalid frontmatter, unsupported fields, unsafe routes, and duplicate output paths fail generation with actionable context. Frontmatter parsing errors include the source file.

### Rendered content and tags

The engine converts Markdown into `ContentDocument.Html`. Page and post views can render that HTML with `MarkupString`.

The built-in theme displays tags as links below page and post content. Each link opens the corresponding tag page. Untagged content has no generated tag list.

Tags on content remain optional. A page or post without tags is generated normally, is omitted from tag listings, and retains its independently configured navigation visibility. Requiring tag-view components in a theme does not require authors to assign tags.

If there is no eligible tagged content anywhere in the site, the engine skips generating the tag index and individual tag pages. The theme must still provide both tag-view components as part of its rendering contract, even though those views are not invoked for that build.

**Current limitation:** the built-in layout always includes a Tags navigation link. On an entirely untagged site, that link can point to a missing tag index. This is existing behavior, not a requirement that pages or posts have tags.

Use one H1 in the body when using the built-in views, then H2/H3 for sections. Other themes can choose a different heading layout. Do not assume the engine itself inserts an H1 from frontmatter.

Raw HTML is supported by the current Markdown pipeline and is rendered as HTML by the built-in views. This is an explicit trust boundary, not an HTML-sanitization feature. Metadata rendered through ordinary Razor expressions retains Razor's encoding.

## Page routes and navigation

Website destinations: `/docs/pages/` and proposed `/docs/navigation/`.

### Slug inference and directory indexes

When `slug` is omitted or blank, the engine infers it from the Markdown file's path relative to its content directory. Nested page files named `index.md` use their containing directory:

| File under `contents/pages/` | Inferred slug |
| --- | --- |
| `parent.md` | `parent` |
| `parent/index.md` | `parent` |
| `parent/child.md` | `parent/child` |
| `parent/group/index.md` | `parent/group` |
| `index.md` | `index` |

For example, `parent/index.md` can contain:

```markdown
---
title: Parent
show_in_navigation: true
---

# Parent
```

It generates `parent/index.html` and links to `parent`, relative to `Site.BaseUrl`.

- A non-blank explicit slug takes precedence over file-based inference.
- The `index` filename comparison is case-insensitive.
- Only nested pages use the directory-index convention. Post routes are unchanged.
- Root-level `contents/pages/index.md` retains the `index` route; it does not replace the generated homepage.
- Locale prefixes are applied after inference as usual.
- Do not keep both `parent.md` and `parent/index.md` with their inferred slugs: the output collision fails the build.

To preserve an older `parent/index` URL, explicitly set `slug: parent/index`.

### Opting into navigation

Set `show_in_navigation: true` on a page to include it:

```markdown
---
title: About
slug: about
show_in_navigation: true
---

# About
```

The field accepts `true` or `false`, and defaults to `false`. Posts, drafts, and the custom 404 page never enter page navigation.

The engine derives the hierarchy from resolved slugs. No separate parent or section field is needed. Reading order comes from source filenames, independently of display titles and URLs.

```text
Parent
  Child
  Group
    Visible Grandchild
```

### Reading order and previous/next links

Within each source directory under `contents\pages`, `index.md` comes first (case-insensitive filename recognition). Other files and directories share ordinal filename ordering, and a directory's eligible descendants are visited before its next sibling. Numeric prefixes are sorted as text, not as numbers: use consistent padding such as `01-`, `02-`, and `03-`.

For example, with these eligible sources:

| Source | Title | Previous | Next |
| --- | --- | --- | --- |
| `parent\index.md` | Parent | None | Child |
| `parent\01-child.md` | Child | Parent | Visible Grandchild |
| `parent\02-group\visible-grandchild.md` | Visible Grandchild | Child | Child 2 |
| `parent\03-child-2.md` | Child 2 | Visible Grandchild | None |

The sequence continues across source directories and outside this example subtree when other eligible pages exist. The built-in page view displays the previous/next anchors automatically. The first page has no previous link, the last has no next link, and a zero- or one-page sequence has neither. Posts, drafts, hidden/suppressed pages, 404 content, and non-clickable groups are not targets. Group descendants can still participate.

A file named `01-abc.md` with `slug: zulu` precedes `02-pqr.md` with `slug: alpha`, regardless of their titles. Explicit slugs preserve public URLs when source names change; prefixes are not automatically stripped from inferred URLs. Root-level `index.md` still generates its ordinary `index` route, not the site homepage.

The navigation tree continues to group by resolved slugs. Each sibling subtree is ordered by the earliest reading position of a real page in that subtree. When explicit slugs differ from source hierarchy, flattening the displayed tree can differ from reading order; previous/next always uses the independent flat reading sequence.

Custom loaders can supply pages without a recorded `ContentDocument.SourcePath`. Eligible source-less pages follow all file-backed pages, sorted by ordinal title then resolved slug. They join the same previous/next sequence, including the boundary between tiers. With only source-less pages, title/slug ordering applies throughout. The standard Markdown loader records every page's source path.

Supplied source paths must identify files without literal `.`/`..` segments or invalid filenames; engine generation also requires them to remain within the configured pages root. Relative custom-loader paths are interpreted below that root. Invalid supplied paths fail with context instead of becoming source-less fallback entries. Ordering uses source identities without opening additional files; this is not a claim of comprehensive filesystem-link protection in the content loader.

The engine rebuilds the sequence and immutable target title/URL snapshots on every generation, before document plugin hooks. Later plugin replacement metadata does not refresh already prepared navigation or previous/next links. No `section`, `pages.json`, ordering field, or authored `prev`/`next` frontmatter is supported or required.

### Existing hidden parents

An existing page with navigation disabled hides its entire descendant branch, regardless of descendant opt-in flags.

For example, hiding `parent` also hides `parent/child` and `parent/group/visible-grandchild`. Hiding an existing `parent/group` page hides the grandchild but not `parent/child`.

Matching uses complete path segments: `parent` does not govern `parent-other`.

Hidden pages remain generated and can still be reached by URL or through tag listings. Navigation visibility is not access control.

### Missing parents

A missing page is different from an existing page that is hidden. The engine creates a non-clickable group for each missing ancestor level, but only while that level contains a visible descendant.

For example:

- `parent` exists and is visible.
- There is no `parent/group` page.
- `parent/group/visible-grandchild` is visible.
- `parent/group/hidden-grandchild` is hidden.

The navigation contains Parent, a non-clickable Group label, and Visible Grandchild. Hiding both grandchildren removes Group. A real but hidden Group page is never replaced by an implicit group.

Group labels are derived from missing path segments. Hyphens and underscores become spaces, followed by invariant title casing: `deployment-tools` becomes `Deployment Tools`.

Groups do not create pages, output files, or placeholder links. Locale routing prefixes are not synthesized as groups when `UseLocaleInUrl` is enabled, unless there is an actual visible page at that prefix.

### Rendering and interaction

The engine builds an immutable `NavigationTree` once per generation and supplies it to every layout: home, posts, pages, tag lists, individual tags, and 404.

The built-in theme renders page nodes as links and missing-parent nodes as plain text. Adjacent buttons expand and collapse child lists. Mouse, touch, Enter, Space, and Escape are supported. Escape closes the current group and restores focus to its button; moving focus or clicking outside navigation closes the menus.

Without JavaScript, the included hierarchy remains visible and ordinary links still work. JavaScript controls interaction, not frontmatter visibility or hierarchy construction.

### Custom 404 pages

Add a page with `slug: 404.html` to provide custom not-found content. The Markdown filename is not special; `not-found.md` is a useful convention.

```markdown
---
title: Page not found
description: The requested page does not exist.
slug: 404.html
---

# Page not found

Return to the [home page](.).
```

The engine renders it through the theme's not-found view and writes `404.html` at the output root, without a locale prefix. It is not written as `404.html/index.html` and is excluded from navigation and tag listings.

If no custom document exists, the engine still generates the not-found page; the built-in theme supplies a default message. Hosting configuration determines when missing requests use the generated file. The current preview server serves `/404.html` directly but does not automatically rewrite unknown URLs to its contents.

## Preview and build

Website destination: `/docs/build/`.

Run the application from its own directory so configuration and content resolve correctly.

```bash
dotnet run -- --preview
dotnet run -- --build
```

| Mode | Output | Behavior |
| --- | --- | --- |
| Preview | `preview/` | Starts a local server and regenerates after content or theme-file changes |
| Build | `dist/` | Generates the static site without starting the preview server |

Refresh the browser after preview regeneration. Razor and C# changes require recompilation, typically with `dotnet watch`.

The repository sample's launch profile does not select an application mode. Supply `--preview` or `--build` explicitly, including in IDE run arguments. `--no-launch-profile` is optional when you also want to bypass profile environment settings. Stop the preview server with Ctrl+C.

The engine sets `SiteManifest.IsPreview` before plugin hooks and rendering. Plugins can use it to suppress production-only side effects.

## Theme authoring

Website destination: `/docs/themes/`.

Install `ScissorHands.Theme` when authoring a theme. Applications consuming a theme normally install `ScissorHands.Web`, which references the shared packages.

```bash
dotnet add package ScissorHands.Theme --prerelease
```

The official [theme template](https://github.com/getscissorhands/theme-template) provides a starting structure.

### Responsibility boundary

The engine owns content loading, route resolution, visibility filtering, navigation hierarchy, and shared URL conventions. Themes own HTML, CSS, presentation formatting, disclosure controls, DOM identifiers, and accessibility markup.

Theme packages consume shared Core models and Theme base classes. They do not need a dependency on the Web engine implementation or their own navigation builder.

### Required components

A theme supplies a concrete component derived from each required base type:

| Base type | Content |
| --- | --- |
| `MainLayoutBase` | Overall document layout |
| `IndexViewBase` | Ordered post collection |
| `PostViewBase` | Current post |
| `PageViewBase` | Current page |
| `NotFoundViewBase` | Custom or generated 404 document |
| `TagListViewBase` | Grouped tagged documents |
| `TagViewBase` | Current tag and its matching posts/pages |

All seven roles are mandatory, even if the current site has no tagged content. Automatic discovery requires exactly one concrete component for each role in the theme's namespace. Missing or ambiguous tag views fail theme resolution; the engine does not silently substitute built-in tag markup.

The engine owns tag data generation; each selected theme owns its complete rendering contract. Requiring all roles avoids coupling an incomplete custom theme to another theme's presentation. It does not mean that every view generates a page in every build; see [rendered content and tags](#rendered-content-and-tags).

### Automatic discovery

Keep a theme's concrete components in one namespace whose normalized suffix matches `Site:Theme`.

```text
Theme slug: minimal-blog
Namespace:  ScissorHands.Theme.MinimalBlog
```

Normalization ignores punctuation and letter casing. For example, `theme-template` matches a namespace suffix of `Theme.Template`.

The consuming application does not need to register component types:

```csharp
using ScissorHands.Web;

var app = new ScissorHandsApplicationBuilder(args).Build();
await app.RunAsync();
```

`AddLayouts(...)` remains available as an explicit override. Its generic and `Type`-based forms require all seven component types, including the tag-list and tag views. Themes may explicitly reuse existing components, but must select them rather than rely on implicit fallback.

### Theme manifest and assets

Metadata and assets live below `themes/{slug}/`. The manifest slug must match the configured theme.

Example `theme.json`:

```json
{
  "name": "Minimal Blog",
  "version": "1.0.0",
  "description": "A minimal theme for ScissorHands.NET.",
  "slug": "minimal-blog",
  "stylesheets": [
    "/assets/theme.css"
  ],
  "scripts": [
    "/assets/theme.js"
  ]
}
```

Stylesheet and script collections are non-null, read-only, and defensively copied during initialization. Treat manifest collections as immutable input.

### Main layout and cascading data

Inherit from `MainLayoutBase` and pass content data through `CascadingMainLayoutBase`:

```razor
@inherits MainLayoutBase

<CascadingMainLayoutBase
    Documents="@Documents"
    TaggedDocuments="@TaggedDocuments"
    Tag="@Tag"
    TaggedPosts="@TaggedPosts"
    TaggedPages="@TaggedPages"
    Document="@Document"
    PageNavigation="@PageNavigation"
    Plugins="@Plugins"
    Theme="@Theme"
    Site="@Site">
    <!DOCTYPE html>
    <html lang="@PageLocale">
    <head>
        <base href="@Site!.BaseUrl" />
        <title>@PageTitle</title>
    </head>
    <body>
        @Body
    </body>
    </html>
</CascadingMainLayoutBase>
```

`MainLayoutBase` calculates page title, description, and locale from the site and current document. Override `CalculatePageTitle()`, `CalculatePageDescription()`, or `CalculatePageLocale()` to customize those values.

Rendered Markdown is available as `ContentDocument.Html`. Rendering it with `MarkupString` is an explicit raw-HTML trust boundary; render metadata through ordinary Razor expressions so it remains encoded.

### Prepared navigation

The engine supplies two layout parameters:

- `NavigationTree`: the complete, ordered hierarchy of immutable `NavigationNode` objects, including missing-parent groups.
- `NavigationPages`: the flat, read-only reading sequence of actual visible pages, retained for compatibility.

Each node exposes `Title`, `Path`, `Url`, and read-only `Children`. A null `Url` denotes a non-clickable group. Do not turn its `Path` into a link.

The following rendering fragment can be used inside a layout derived from `MainLayoutBase`:

```razor
@using ScissorHands.Core.Models

<nav aria-label="Primary navigation">
    <ul>
        <li><a href=".">Home</a></li>
        @RenderNodes(NavigationTree)
        <li><a href="tags">Tags</a></li>
    </ul>
</nav>

@code {
    private RenderFragment RenderNodes(IReadOnlyList<NavigationNode> nodes) => @<text>
        @foreach (var node in nodes)
        {
            <li>
                @if (node.Url is not null)
                {
                    <a href="@node.Url">@node.Title</a>
                }
                else
                {
                    <span>@node.Title</span>
                }
                @if (node.Children.Count > 0)
                {
                    <ul>@RenderNodes(node.Children)</ul>
                }
            </li>
        }
    </text>;
}
```

Both collections default to empty lists. They are layout-only parameters, not content-view attributes or automatic cascading values. `Documents` remains the ordered post collection for the home view. Implicit groups are not added to `NavigationPages`.

Normal generation supplies both navigation parameters automatically. For compatibility, `ComponentRenderer` prepares a tree when an older caller supplies only `NavigationPages` to a layout derived from `MainLayoutBase`. An explicitly supplied tree is used unchanged. Direct component rendering should supply `NavigationTree`.

### Adjacent-page context

`MainLayoutBase.PageNavigation` is optional generated data with nullable `Previous` and `Next` links. Each immutable `PageNavigationLink` contains the target's text `Title` and already formatted, base-relative `Url`. Forward `PageNavigation` through `CascadingMainLayoutBase` as in the example above; `PageViewBase.PageNavigation` receives it as a cascading value.

A custom page view can opt into the links without rebuilding navigation:

```razor
@inherits ScissorHands.Theme.PageViewBase

<article>@((MarkupString)Document?.Html!)</article>
@if (PageNavigation?.Previous is not null || PageNavigation?.Next is not null)
{
    <nav aria-label="Page navigation">
        @if (PageNavigation?.Previous is { } previous)
        {
            <a href="@previous.Url" rel="prev">Previous: @previous.Title</a>
        }
        @if (PageNavigation?.Next is { } next)
        {
            <a href="@next.Url" rel="next">Next: @next.Title</a>
        }
    </nav>
}
```

Use ordinary Razor text rendering for titles. Do not escape an already formatted target URL again or prepend `Site.BaseUrl`; the layout's base element resolves it. Existing themes that ignore this additive context continue to work, but must forward/render it to show adjacent links. The two full navigation collections remain layout-only and are not added to the cascade. Non-participating pages and collection/404/post views have no sequence links.

### URL helpers

Internal URLs should be base-relative so a site under `/project/` continues to work:

```razor
<link rel="stylesheet" href="@GetThemeUrl("/assets/theme.css")" />
<a href="tags">Tags</a>
```

The Theme base classes expose wrappers over `ScissorHands.Core.Urls.ContentUrlHelper`:

| Base class | Protected helper |
| --- | --- |
| `MainLayoutBase` | `GetThemeUrl(path)`, `GetContentUrl(slug)` |
| `IndexViewBase`, `TagViewBase` | `GetContentUrl(slug)` |
| `PostViewBase` | `GetImageUrl(path)`, `GetTagUrl(tag)` |
| `PageViewBase`, `TagListViewBase` | `GetTagUrl(tag)` |

`GetThemeUrl` resolves assets below the current theme. For slug `minimal-blog`, `/assets/theme.css` becomes `themes/minimal-blog/assets/theme.css`. Supply `Theme` before calling the layout helper. A missing theme throws `InvalidOperationException`; a null path throws `ArgumentNullException` before the theme check.

Do not apply content-slug escaping to image URLs: images may use absolute HTTP(S) URLs, queries, fragments, or existing percent encoding. The detailed shared helper contracts are in the Core API reference.

### Plugin components in themes

Components derived from `PluginComponentBase` select their manifest through the required `Id` parameter:

```razor
<MyPluginComponent Id="reading-time" />
```

`Name` is display-only and cannot be used as a selector. Existing name-based selectors must be migrated and rebuilt.

## Plugin authoring

Website destination: `/docs/plugins/`.

Install `ScissorHands.Plugin` when authoring plugins. Applications consuming plugins normally install `ScissorHands.Web`.

```bash
dotnet add package ScissorHands.Plugin --prerelease
```

### Create a content plugin

Derive from `ContentPlugin`, provide a stable ID and display name, and override only the hooks needed:

```csharp
using ScissorHands.Core.Manifests;
using ScissorHands.Core.Models;
using ScissorHands.Plugin;

public sealed class ReadingTimePlugin : ContentPlugin
{
    public override string Id => "reading-time";
    public override string Name => "Reading Time";

    public override Task<ContentDocument> PostMarkdownAsync(
        ContentDocument document,
        PluginManifest plugin,
        SiteManifest site,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var words = document.Markdown
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Length;

        document.Html = $"<p>{Math.Max(1, words / 200)} min read</p>{document.Html}";
        return Task.FromResult(document);
    }
}
```

This example uses a fixed reading rate. Plugins that expose configurable options should read and validate those options explicitly.

For complete control, implement `IContentPlugin` directly.

### Identity

`Id` is the stable identifier used by configuration, dependencies, and Razor component selection. It must be unique among installed plugins and use lowercase ASCII kebab-case: letters or digits in segments separated by single hyphens.

Examples: `reading-time`, `heading-ids`, and `syntax-highlighting-v2`.

The engine rejects empty IDs, uppercase letters, whitespace, underscores, non-ASCII characters, and leading, trailing, or repeated hyphens. Matching is ordinal. IDs are not trimmed, lowercased, or derived from names.

`IContentPlugin.Name` remains a non-empty display name. `PluginManifest.Name` is optional display metadata. Display names need not be unique and do not determine matching or execution order. Keep IDs stable when renaming display labels.

### Pipeline stages

| Hook | Purpose |
| --- | --- |
| `PreMarkdownAsync` | Transform a document before Markdown conversion |
| `PostMarkdownAsync` | Transform a document after its `Html` is populated |
| `PostHtmlAsync` | Transform final HTML after Razor rendering |

Each plugin's output feeds the next enabled plugin. Preserve the overall sequence: pre-Markdown hooks, Markdown conversion, post-Markdown hooks, Razor rendering, post-HTML hooks.

### Stage-scoped dependencies

Declare a dependency when a hook consumes another plugin's output:

```csharp
public override IReadOnlyList<PluginDependency> DependsOn =>
[
    new("heading-ids", PluginStage.PostMarkdown),
];
```

`PluginDependency.PluginId` identifies the target by ID, not display name. Supported stages are `PreMarkdown`, `PostMarkdown`, and `PostHtml`.

- The target must be installed and enabled.
- Its hook runs before the dependent hook in the declared stage.
- Declare the target separately for each stage that requires it.
- Dependencies do not change the Markdown/Razor stage boundaries; cross-stage declarations are not supported.
- Declarations belong to plugin code, not `PluginManifest.Options` or an additional JSON setting.

`ContentPlugin.DependsOn` defaults to an empty list. Direct `IContentPlugin` implementations can opt in by also implementing `IContentPluginDependencies`.

The engine resolves transitive dependencies separately per stage. Among ready plugins, it chooses by ordinal ID ordering. Manifest position, assembly discovery, and dependency-injection registration order do not control execution.

The runner snapshots declarations when constructed. Before hooks run, it rejects missing or disabled dependencies, invalid IDs, unknown stages, self-dependencies, duplicate declarations within a stage, and cycles. Diagnostics identify affected IDs and stages where applicable.

A disabled plugin's declarations are ignored. Dependencies are never downloaded, installed, or enabled automatically. Plugins without dependencies must not rely on the tie-breaking order as an implicit dependency.

### Enable a plugin

Install the plugin assembly in the application and configure its ID:

```json
{
  "Plugins": [
    {
      "Id": "reading-time",
      "Name": "Reading Time"
    }
  ]
}
```

`Id` must match an installed plugin exactly. Duplicate manifest IDs and manifests without an installed match fail startup. Installed plugins without a manifest remain disabled.

`PluginManifest.Options` is nullable and exposed as `IReadOnlyDictionary<string, object?>`. Treat it as immutable input and validate every option's type and value.

### Razor plugin components

Derive from `PluginComponentBase` when a theme renders plugin-specific output:

```razor
@inherits PluginComponentBase

@if (Plugin is not null)
{
    <meta name="example-plugin" content="@Name" />
}
```

Supply the component's required `Id`, for example `Id="reading-time"`. Its optional `Name` parameter is display text only.

The base type provides the document, document collection, plugin manifests, theme, and site through cascading parameters. Invalid component IDs, invalid manifest IDs, and duplicate manifest IDs fail rendering.

A valid component ID without a configured manifest leaves `Plugin` null, allowing disabled output to be omitted.

### Preview and generated URLs

`SiteManifest.IsPreview` is set before hooks and rendering. Use it when production-only side effects should be suppressed during preview.

Internal URLs emitted by plugins must respect `SiteManifest.BaseUrl`. Do not assume the deployment always occupies the domain root.

## Core API reference

Proposed website destination: `/docs/api/`.

`ScissorHands.Core` contains shared models, manifests, service contracts, and command options. Plugin and Theme depend on Core; Web contains engine implementations and depends on those packages. Avoid reverse references or dependency cycles.

### ContentDocument and ContentMetadata

`ContentDocument` represents a Markdown source document as it passes through generation:

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

`ShowInNavigation` defaults to `false`. The engine applies page eligibility and ancestor visibility before preparing navigation; consumers should not interpret that property alone as final membership.

### NavigationNode

`NavigationNode` is immutable, theme-independent render data:

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

`Path` identifies the node with an escaped, base-relative route. `Url` is null for groups without a page. `Children` is an ordered, read-only snapshot of the supplied collection. Nodes contain no HTML, CSS classes, or DOM identifiers.

The Web engine's `NavigationTreeBuilder` builds the hierarchy from visibility-filtered pages. The generator performs that work once per generation and supplies it to all layouts.

### PageNavigation and PageNavigationLink

`PageNavigation` contains optional immutable `Previous` and `Next` values of type `PageNavigationLink`. A link contains the target's `Title` and formatted `Url`; empty navigation is the default. These are generated render models, not fields in `ContentMetadata` and not a new frontmatter schema.

The generator identifies pages by their original document instance when associating adjacency, not by source path alone: several source-less pages can legitimately have an empty path. Links snapshot the target metadata before plugins and are not references to mutable document HTML.

### Manifests

`SiteManifest` holds site-wide generation settings:

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

`ThemeManifest` describes the selected theme and its assets:

```csharp
var theme = new ThemeManifest
{
    Name = "My Theme",
    Slug = "my-theme",
    Stylesheets = ["/assets/theme.css"],
    Scripts = ["/assets/theme.js"],
};
```

`Stylesheets` and `Scripts` are non-null `IReadOnlyList<string>` collections and are defensively copied during initialization.

`PluginManifest` describes configured plugin options:

```csharp
var plugin = new PluginManifest
{
    Id = "example-plugin",
    Name = "Example Plugin",
    Options = new Dictionary<string, object?>
    {
        ["Option1"] = "value1",
    },
};
```

`Id` is required when the manifest is used. `Name` is optional display metadata. `Options` is nullable, read-only configuration input; plugins must validate the options they consume.

### Shared URL helpers

`ScissorHands.Core.Urls.ContentUrlHelper` is shared by engine and view-base implementations.

| Method | Contract |
| --- | --- |
| `GetContentUrl(slug)` | Trim outer whitespace; split forward/backward slash separators; remove empty segments; escape each segment; join with `/`. An empty slug becomes `.`. Literal `.` and `..` segments are rejected. |
| `GetThemeUrl(themeSlug, path)` | Return `themes/{themeSlug.Trim('/')}/{path.TrimStart('/')}` without additional escaping. |
| `GetImageUrl(path)` | Remove leading forward slashes only; preserve the rest of the reference, including absolute HTTP(S) URLs, query strings, fragments, and percent encoding. |
| `GetTagUrl(tag)` | Trim, lowercase invariantly, escape as one segment, and prefix with `tags/`. Empty tags and `.` or `..` are rejected. |
| `GetLocaleSegment(locale)` | Return an empty string for null/whitespace; otherwise trim, lowercase invariantly, and replace underscores and forward slashes with hyphens. |

Content, tag, and theme URLs are base-relative: they do not prepend `Site.BaseUrl`. For example, `guides/about & team` becomes `guides/about%20%26%20team`, resolved by the layout's `<base>` element.

Null arguments to content, image, and tag helpers throw `ArgumentNullException`. The static theme helper rejects null theme slugs and paths. Invalid content path segments and invalid tag names throw `ArgumentException`.

Image handling retains the existing convention; it is not a general URI sanitizer. Do not reuse content-slug escaping for image references.

The existing protected layout helpers remain supported, including their exception behavior. New view-base wrappers share these rules instead of duplicating them in theme files.

### Service contracts

- `IMarkdownService` converts Markdown to HTML.
- `IThemeService` loads theme manifests and copies theme assets.

Both support cancellation. The legacy non-cancellable `IThemeService` overloads remain temporarily supported but are obsolete and scheduled for removal in the next major version. The generator uses cancellation-aware overloads.

## Upgrading to vNext

Proposed website destination: `/docs/migration/`.

Keep these warnings visible in release notes and link them from package READMEs. Do not silently apply the new guidance to older package versions.

### Collection and public API changes

The following changes affect source and binary compatibility:

- `ThemeManifest.Stylesheets` is now `IReadOnlyList<string>`.
- `ThemeManifest.Scripts` is now `IReadOnlyList<string>`.
- `PluginManifest.Options` is now `IReadOnlyDictionary<string, object?>`.
- Plugins must supply `IContentPlugin.Id` or override `ContentPlugin.Id`.
- `PluginDependency` identifies its target through `PluginId`, not `Name`.

Theme asset collections are defensively copied during initialization. Existing object initializers still work, but callers must treat manifest collections as immutable inputs.

The existing one-argument `IThemeService` methods are still present but obsolete. Move integrations to the cancellation-aware overloads.

### Name-based plugin identity

This is a source, binary, and configuration migration. There is no name-based compatibility fallback or automatic ID generation.

1. Add `Id` to every `IContentPlugin` implementation or override it in every `ContentPlugin` subclass. Choose a permanent lowercase ASCII kebab-case value.
2. Add that ID to every corresponding manifest in the `Plugins` array. Existing `Name` values can remain as display labels.
3. Replace dependency display names with target IDs. Update references to `PluginDependency.Name` to use `PluginDependency.PluginId`, including named constructor arguments.
4. Replace Razor selectors such as `Name="Reading Time"` with `Id="reading-time"`. A display name alone does not select a plugin.
5. Rebuild plugin, theme, and consuming application assemblies against the updated packages, then deploy them with the updated configuration.

Example identity and dependency:

```csharp
public override string Id => "heading-ids";
public override string Name => "Heading IDs";
```

```csharp
new PluginDependency(PluginId: "heading-ids", Stage: PluginStage.PostMarkdown)
```

The matching configuration is `{ "Id": "heading-ids", "Name": "Heading IDs" }`. Renaming the display label later does not require changing dependencies or selectors.

### Plugin ordering

Plugins that relied on assembly discovery, registration order, or manifest position must declare their actual stage-scoped dependencies through `DependsOn`.

Enable every required dependency explicitly. The engine never automatically installs or enables one. Ready plugins are selected by ordinal ID order, which is not a substitute for declaring a dependency.

### Required tag views

Custom themes must now provide concrete components derived from both `TagListViewBase` and `TagViewBase`, alongside the five other required roles. Themes that previously relied on automatic built-in tag-view fallback are incomplete and fail resolution.

Add exactly one implementation of each missing role in the theme's component namespace, then rebuild the theme and consuming application. Explicit `AddLayouts(...)` registrations must supply all seven component types. If deliberately reusing an existing tag view, select it explicitly rather than relying on the engine to choose its markup.

The built-in default theme already provides all seven roles. This change does not disable tag generation or change tag data; it makes each selected theme responsible for its complete rendering contract.

### Directory-index routes

Nested page `index.md` files with omitted or blank slugs now use their containing directory's route. For example, `parent/index.md` changes from `parent/index` to `parent`.

Set `slug: parent/index` to retain the old URL. Post routes and root-level page `index.md` behavior are unchanged. Resolve any collisions between `parent.md` and `parent/index.md` before publishing.

### Navigation compatibility

`NavigationPages` remains a supported flat collection of actual visible pages. New themes can use the prepared `NavigationTree` to render hierarchy without rebuilding it.

Both parameters are layout-only. Normal generation supplies both. `ComponentRenderer` supports older callers that provide only `NavigationPages`, while direct rendering of a hierarchical layout should supply `NavigationTree`.

Navigation now uses filename-based order for file-backed pages, with index-first depth-first traversal and a trailing title/slug-ordered source-less tier. This intentionally replaces title-first engine navigation ordering. Public builder/renderer signatures and explicitly supplied trees remain supported. Use numeric source prefixes to control order and explicit slugs to retain URLs; no automatic source rename or URL-prefix stripping is performed.

`PageNavigation` is additive, optional layout/cascading context. Existing custom themes need no new required view role; they opt in by forwarding it through their cascading layout and rendering links in the page view. The built-in theme does this automatically. Post ordering and title ordering in tag collections are unchanged.

Existing public URL-helper signatures remain supported. Shared helpers also make post/tag-list whitespace normalization and content-link escaping consistent with engine route conventions.

## Source coverage

The following removed or shortened README material is preserved above:

| Original README | Material retained in this document |
| --- | --- |
| Repository root | Detailed page-navigation behavior; source/binary/configuration compatibility; plugin ordering and directory-index migration |
| `ScissorHands.Core` | Content and navigation models; manifest examples and collection contracts; URL-helper contracts; cancellation and obsolete service overloads |
| `ScissorHands.Plugin` | Complete plugin example; ID rules; stages; dependencies and failures; configuration; Razor components; preview behavior; migration steps |
| `ScissorHands.Theme` | Required view roles and tag-view migration; automatic discovery; manifest and assets; layout/cascading data; recursive navigation rendering; URL helpers; plugin selection |
| `ScissorHands.Web` | Full application/configuration/content examples; frontmatter reference; routes; navigation; preview/build; theme and plugin integration; migration notes |
| Sample | General navigation and plugin explanations; sample-specific execution instructions and fixture descriptions remain in the sample README |

Keep repository-specific commands and sample file pointers in the repository. Keep package installation, supported framework, essential compatibility warnings, license links, and required third-party attribution in the corresponding READMEs.
