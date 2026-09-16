# Website documentation handoff

This document preserves the detailed material extracted from the repository READMEs. It is organized into guides that can be published on [getscissorhands.app](https://getscissorhands.app).

**Version scope:** the current vNext implementation, targeting .NET 10. These instructions must not be presented as applying to older packages without checking their compatibility. The shorter READMEs retain installation, minimal usage, essential warnings, and links.

## Publishing notes

The table distinguishes website destinations from repository-specific reference material. Existing destinations were present when this handoff was prepared; proposed destinations need to be created and added to the documentation navigation.

| Guide in this document | Website destination | Publication action |
| --- | --- | --- |
| [Quickstart](#quickstart) | `/docs/quickstart/` | Update the application bootstrap and first-content workflow |
| [Site configuration](#site-configuration) | `/docs/configuration/` | Update site settings and plugin configuration |
| [Content and frontmatter](#content-and-frontmatter) | `/docs/front-matter/`, `/docs/posts/`, `/docs/pages/` | Update supported fields and content guidance |
| [Page routes and navigation](#page-routes-and-navigation) | `/docs/pages/`; proposed `/docs/navigation/` | Document directory indexes and navigation visibility |
| [Preview and build](#preview-and-build) | `/docs/build/` | Document modes, regeneration, and output |
| [Sample walkthrough](#sample-walkthrough) | Repository sample reference | Keep fixture details here and run commands in the sample README |
| [Theme authoring](#theme-authoring) | `/docs/themes/` | Update discovery, prepared data, rendering, and URL helpers |
| [Plugin authoring](#plugin-authoring) | `/docs/plugins/` | Replace name-based identity and add dependency rules |
| [Core API reference](#core-api-reference) | Proposed `/docs/api/` | Publish shared models, manifests, services, and URL conventions |
| [Upgrading to vNext](#upgrading-to-vnext) | Proposed `/docs/migration/` | Publish source, binary, configuration, and route migration guidance |
| [Browser acceptance](#browser-acceptance) | Repository contributor reference | Keep coverage and evidence methodology here and run commands with the test suite |

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

Bash:

```bash
dotnet new web -n MyScissorHandsApp
cd MyScissorHandsApp
dotnet add package ScissorHands.Web --prerelease
```

PowerShell:

```powershell
dotnet new web -n MyScissorHandsApp
Set-Location MyScissorHandsApp
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

Bash:

```bash
dotnet run -- --preview
```

PowerShell:

```powershell
dotnet run -- --preview
```

Generate deployable static files without starting a preview server:

Bash:

```bash
dotnet run -- --build
```

PowerShell:

```powershell
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
| `BaseUrl` | Site base path, such as `/`, `/project` or `/project/`; path prefixes normalize to a trailing slash |
| `UseLocaleInUrl` | Generate locale-prefixed content, home/tag collections and locale-scoped navigation |
| `UseDateInPostUrl` | Include the publication date in post routes |
| `Debug` | Site debug setting |

Use `BaseUrl` when publishing below a subpath. Generated navigation and shared URL helpers produce base-relative links rather than hardcoding the domain root.

`SiteManifest.BaseUrl` supplies a trailing slash for a configured path prefix: `/docs` and `/docs/` both become `/docs/`, `/manual/docs` becomes `/manual/docs/`, and `/` remains `/`. This happens during manifest initialization, including configuration binding and direct .NET initialization. Build, preview, themes and plugins use the same effective value, while source settings remain unchanged. Generated HTML therefore uses `<base href="/docs/">` for either spelling. This does not trim whitespace, change case, decode URLs, or validate them. Absolute/network-relative URLs, relative paths without a leading slash, and values containing backslashes, query strings or fragments are not rewritten by this rule and gain no new support guarantee.

Preview serves generated files only at the effective path prefix. With either `/docs` or `/docs/` configured, a page route `parent/child` is served at `/docs/parent/child/`, and `ko-kr/parent/child` at `/docs/ko-kr/parent/child/`; theme assets and images use the same mount. GET/HEAD `/` returns HTTP 302 to `/docs/`, preserving the query and serving no homepage body at root. Browsers follow to the generated entry point; locale-enabled entry points then use the HTML locale redirect below. Other outside-prefix requests, including `/parent/child/`, `/docs-other/`, unprefixed assets and POST `/`, return 404. `/docs` and directory redirects retain the prefix and query. Files remain directly under `preview/`; update old preview bookmarks rather than relying on root aliases. Configuring `/` adds no HTTP mount redirect but retains any generated locale redirect. This is a routing boundary, not authentication. Other base-URL forms remain outside this preview contract; normalization is not general URL validation.

For production, configure the static host to mount `dist/` at the intended prefix and serve directory indexes. Do not copy output into an additional prefix directory or prepend `BaseUrl` to already base-relative links.

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

### Locale-specific sites

With `Site.UseLocaleInUrl: true`, the engine generates `Site.Locale` plus the effective locales of published non-404 posts and pages. Frontmatter `locale` takes precedence over the site default. Locale keys are trimmed, lowercased, and normalize underscores/forward slashes to hyphens; equivalent spellings such as `ko-KR` and `ko_KR` share one scope. Draft-only locales are excluded; future dates and hidden-navigation pages retain their normal publication rules.

For `Site.Locale: "en-US"` and `BaseUrl: "/blog/"`:

| Surface | URL / behavior |
| --- | --- |
| Root | `/blog/` redirects to `/blog/en-us/` |
| Locale homepages | `/blog/en-us/`, `/blog/ko-kr/`, each listing only its own posts |
| Locale tag index | `/blog/ko-kr/tags`, only if Korean content has tags |
| Locale tag view | `/blog/ko-kr/tags/dotnet`, containing only Korean tagged content |
| Legacy tag URL | `/blog/tags/dotnet` redirects only if `/blog/en-us/tags/dotnet` exists |
| Shared not-found page | `/blog/404.html`, using the site-default locale |

Every discovered locale has a generated homepage, including page-only locales. The default homepage is always generated, even when empty. A locale without eligible tags has no tag pages or built-in Tags link. Home/site-title links, tag links, navigation and previous/next stay within the active locale; there is no automatic cross-language fallback or language switcher.

The root and supported legacy tag URLs are portable HTML redirects with an immediate meta refresh and an ordinary fallback anchor, not HTTP 301/302 guarantees. They use the canonical rooted `BaseUrl`: configured `/blog` and `/blog/` both become `/blog/`. External URLs, traversal, encoded separators and malformed percent escapes fail locale-redirect validation. A missing legacy tag target produces no redirect; an otherwise unowned missing route follows the host's normal 404 behavior. Initial builds are clean, but in-place preview can retain old locale/redirect files until restarted.

Keep `contents/posts` and `contents/pages` as the discovery roots. Optional locale folders organize files but do not infer or override metadata:

```text
contents/
  posts/
    en-us/hello.md
    ko-kr/hello.md
  pages/
    en-us/about.md
    ko-kr/about.md
    not-found.md
```

Use `locale: ko-KR` with `slug: about` in the Korean page to obtain `/blog/ko-kr/about`. Explicit locale-free slugs avoid coupling URLs to source folders; when omitted, existing directory-based inference still applies. A matching leading locale is recognized before post-date composition to avoid duplicating it. Folders with a different name are not stripped, and `contents/<locale>/posts` is not a discovery root.

Home/tag pages are generated, not additional Markdown sources. A page at a generated locale-home or tag/redirect destination fails as an output collision; for example, a Korean `pages/ko-kr/index.md` needs a distinct explicit slug rather than claiming `/ko-kr/`. Empty/unsafe locale segments are rejected; locale-enabled generated-page and content-image destinations reject linked ancestors. This is not a comprehensive audit of input links or theme-service copying.

Shared images/theme assets, site text and authored Markdown links are not translated or rewritten. `Site.Locale` is never mutated between renders. With locale routing disabled, the existing root homepage, shared tag pages and unprefixed navigation behavior remain unchanged.

**Serving boundary:** preview now mounts the artifact at canonical `Site.BaseUrl` through the merged [#89 fix](https://github.com/getscissorhands/Scissorhands.NET/pull/101). With `/docs` and default `en-US`, `/` returns HTTP 302 to `/docs/`, whose HTML entry redirects to `/docs/en-us/`. Legacy tag redirects use `/docs/tags/...`; unrelated domain-root page/asset URLs remain 404. Production hosts still configure their own mount. No physical deployment-prefix directory is added.

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

If there is no eligible tagged content, the engine skips the tag index and individual tag pages. With locale routing enabled this rule applies independently to each locale. The theme must still provide both tag-view components even when those views are not invoked.

**Disabled-mode limitation:** without locale context, the built-in layout retains its existing Tags link even on an untagged site. Locale-enabled rendering omits that link when the active locale has no tags. Neither mode requires every post or page to have tags.

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

Within each source directory under `contents/pages`, `index.md` comes first (case-insensitive filename recognition). Other files and directories share ordinal filename ordering, and a directory's eligible descendants are visited before its next sibling. Numeric prefixes are sorted as text, not as numbers: use consistent padding such as `01-`, `02-`, and `03-`.

For example, with these eligible sources:

| Source | Title | Previous | Next |
| --- | --- | --- | --- |
| `parent/index.md` | Parent | None | Child |
| `parent/01-child.md` | Child | Parent | Visible Grandchild |
| `parent/02-group/visible-grandchild.md` | Visible Grandchild | Child | Child 2 |
| `parent/03-child-2.md` | Child 2 | Visible Grandchild | None |

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

The engine builds immutable navigation once per generation, or once per active locale when locale routing is enabled. Home, posts, pages and tag views receive their locale's tree and reading sequence; the shared 404 receives the default locale's navigation. Previous/next never cross locale boundaries, including the file-backed/source-less boundary.

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

With locale routing enabled, the shared 404 must use `Site.Locale`. Omit its locale or supply a normalized-equivalent spelling; an explicit mismatch fails generation with source/field context. It does not create a locale or get translated automatically. Disabled-mode metadata behavior remains unchanged.

If no custom document exists, the engine still generates the not-found page; the built-in theme supplies a default message. Hosting configuration determines when missing requests use the generated file. The current preview server serves `/404.html` directly but does not automatically rewrite unknown URLs to its contents.

## Preview and build

Website destination: `/docs/build/`.

Run the application from its own directory so configuration and content resolve correctly.

Bash:

```bash
dotnet run -- --preview
dotnet run -- --build
```

PowerShell:

```powershell
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

### Sample walkthrough

The [repository sample](../samples/ScissorHands.Sample/README.md) uses local project references and the built-in `default` theme. Its README contains the run commands; the following sources demonstrate the engine's behavior:

| Source | What to explore |
| --- | --- |
| [Hello, ScissorHands](../samples/ScissorHands.Sample/contents/posts/hello-scissorhands.md) | Rich Markdown for desktop/mobile and light/dark comparisons |
| [About](../samples/ScissorHands.Sample/contents/pages/about.md) | The first page in the sample reading sequence |
| [Parent](../samples/ScissorHands.Sample/contents/pages/parent/index.md) | An index-first landing page with numbered child files and directories |
| [Child](../samples/ScissorHands.Sample/contents/pages/parent/01-child.md), [Visible Grandchild](../samples/ScissorHands.Sample/contents/pages/parent/02-group/visible-grandchild.md), [Child 2](../samples/ScissorHands.Sample/contents/pages/parent/03-child-2.md) | Cross-directory previous/next links; explicit slugs preserve routes despite filename-ordering prefixes |
| [Hidden Grandchild](../samples/ScissorHands.Sample/contents/pages/parent/02-group/hidden-grandchild.md) | Published, tagged content omitted from navigation and the reading sequence |
| [Not found](../samples/ScissorHands.Sample/contents/pages/not-found.md) | Custom root `404.html` content with locale inherited from the site |

The default sequence is **About, Parent, Child, Visible Grandchild, Child 2**. Set Visible Grandchild's `show_in_navigation` to false to remove the empty Group, or disable Parent to hide that entire branch. The endpoint links and visibility rules are described in [reading order](#reading-order-and-previousnext-links).

The sample starts with an empty `Plugins` array and locale routing disabled. In its `appsettings.json`, enable `Site.UseLocaleInUrl` to generate `dist/en-us/index.html` and a default-locale root redirect. To explore Korean-prefixed preview, also set `Site.Locale` to `ko-KR` and `Site.BaseUrl` to `/docs` or `/docs/`; Parent then lives at `/docs/ko-kr/parent/`. See [site configuration](#site-configuration) for slash normalization and the HTTP/HTML redirect stages, and [locale-specific sites](#locale-specific-sites) for additional-language authoring and generated-route collisions.

The custom 404 omits `locale`, so it follows the site default. Shared assets and authored Markdown links are not translated or rewritten. The [browser acceptance fixtures](#browser-acceptance) exercise additional locales in an isolated copy rather than altering the normal sample's sources.

## Theme authoring

Website destination: `/docs/themes/`.

Install `ScissorHands.Theme` when authoring a theme. Applications consuming a theme normally install `ScissorHands.Web`, which references the shared packages.

Bash:

```bash
dotnet add package ScissorHands.Theme --prerelease
```

PowerShell:

```powershell
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
    LocaleContext="@LocaleContext"
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

`MainLayoutBase` calculates page title and description from the site/document. Locale uses the active `LocaleContext` when supplied, otherwise the existing document/site fallback. Override `CalculatePageTitle()`, `CalculatePageDescription()`, or `CalculatePageLocale()` to customize those values.

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
        <li><a href="@GetHomeUrl()">Home</a></li>
        @RenderNodes(NavigationTree)
        @if (GetTagIndexUrl() is { } tagIndexUrl)
        {
            <li><a href="@tagIndexUrl">Tags</a></li>
        }
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

### Locale render context

`ScissorHands.Core.Models.LocaleContext` is an optional immutable engine snapshot:

| Member | Meaning |
| --- | --- |
| `Locale` | Normalized active locale, independent of the unchanged `Site.Locale` default |
| `Route` | Resolved current route; source documents retain the loaded pre-hook route snapshot |
| `HomeUrl` | Already escaped, base-relative home URL, such as `ko-kr/` |
| `TagIndexUrl` | Already escaped, base-relative tag-index URL, or null if the locale has no tags |
| `GetTagUrl(tag)` | Compose a raw tag with the prepared home URL using shared tag escaping |

Forward `LocaleContext` through `CascadingMainLayoutBase` for view and plugin components. The renderer keeps it out of ordinary view attributes. Full navigation remains layout-only and the seven required theme roles are unchanged.

Use layout `GetHomeUrl()` and `GetTagIndexUrl()` instead of hard-coded `.`/`tags`; without context they retain those original values. Existing post/page/tag-list `GetTagUrl` wrappers become locale-aware when context exists. Core static helpers keep their context-free contracts. Context values are prepared rendering data, not a general URL sanitizer.

Locale inventory, collections and navigation are prepared from loaded documents before hooks. They are not recomputed when plugins return replacement locale/slug/title metadata; per-document hook propagation is retained without promising cross-collection refresh or a second plugin pass.

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
            <a href="@previous.Url" rel="prev" tabindex="0">Previous: @previous.Title</a>
        }
        @if (PageNavigation?.Next is { } next)
        {
            <a href="@next.Url" rel="next" tabindex="0">Next: @next.Title</a>
        }
    </nav>
}
```

Use ordinary Razor text rendering for titles. Do not escape an already formatted target URL again or prepend `Site.BaseUrl`; the layout's base element resolves it. Existing themes that ignore this additive context continue to work, but must forward/render it to show adjacent links. The two full navigation collections remain layout-only and are not added to the cascade. Non-participating pages and collection/404/post views have no sequence links.

The explicit zero tab index preserves native sequential link access in WebKit keyboard modes. The default pager reuses the theme's text palette for labels and focus outlines; its component-only 4.5:1 text and 3:1 focus checks run through the [browser acceptance suite](../test/browser/README.md). Custom-theme authors remain responsible for their own complete accessibility.

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

Bash:

```bash
dotnet add package ScissorHands.Plugin --prerelease
```

PowerShell:

```powershell
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

### Generated tag route context

The engine resolves tag routes before Razor rendering and supplies a synthetic page `Document` with the final slug (`tags` or `tags/{tag}` when disabled; `<locale>/tags` or `<locale>/tags/{tag}` when enabled). Locale-enabled homepages also receive a route-only rendering document. Layouts must forward `Document` and `LocaleContext` through `CascadingMainLayoutBase` for plugin components to receive them.

The rendering document's title, Markdown, HTML and source path are empty, with no document-specific description, author, Twitter handle, image or publication date. `Metadata.Locale` is the active normalized locale when enabled and remains null on disabled-mode tag documents. Site title/description defaults and tag-view headings remain unchanged. A non-null `Document` does not imply a post. Collections/navigation are locale-scoped when enabled; tag-page adjacency remains empty.

Post-HTML hooks retain synthetic tag titles (`Tags` or `Tag: {tag}`) and rendered `Html`, with the same resolved slug/locale supplied during rendering. Synthetic pages, including root/legacy redirects, bypass Markdown hooks and receive post-HTML processing. Compose publication URLs using `Site.SiteUrl`, `Site.BaseUrl` and the supplied slug, without rebuilding or escaping it again. For tag `C#` in `ko-KR` with locale routing enabled, the slug is `ko-kr/tags/c%23`; with site URL `https://example.com` and base `/blog/`, the publication URL is `https://example.com/blog/ko-kr/tags/c%23`.

### Preview and generated URLs

`SiteManifest.IsPreview` is set before hooks and rendering. Use it when production-only side effects should be suppressed during preview.

Internal URLs emitted by plugins must respect `SiteManifest.BaseUrl`. Do not assume the deployment always occupies the domain root.

## Core API reference

Proposed website destination: `/docs/api/`.

`ScissorHands.Core` contains shared models, manifests, service contracts, and command options. Plugin and Theme depend on Core; Web contains engine implementations and depends on those packages. Avoid reverse references or dependency cycles.

See [site configuration](#site-configuration) for `SiteManifest.BaseUrl` normalization and [locale render context](#locale-render-context) for the `LocaleContext` contract and helper behavior.

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

`PageNavigation` contains optional immutable `Previous` and `Next` values of type `PageNavigationLink`. A link contains the target's `Title` and formatted `Url`, both defaulting to empty strings; a new `PageNavigation` has no neighbors. These are generated render models, not fields in `ContentMetadata` and not a new frontmatter schema.

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

### Locale routing migration

Enabling `UseLocaleInUrl` now changes generated collection URLs and navigation scope, not just source-document prefixes. The root becomes a default-locale HTML redirect; home/tag collections and navigation move inside each locale. Unprefixed tag redirects are generated only for existing default-locale destinations. A tag present only in another locale has no legacy redirect, and an empty default locale still has a homepage but no tag index.

Retain frontmatter/site locale fallback and optional organizational folders. Use explicit locale-free slugs to preserve source-independent URLs and resolve any collision with a generated locale homepage/tag/redirect. Existing already-prefixed dated posts now get the effective locale once before the date, rather than duplicating it around the date.

Custom themes should forward `LocaleContext` and adopt the Home/Tags helpers above. Existing public signatures and direct rendering without context remain supported, but hard-coded links are not automatically rewritten. Plugins must consume synthetic route/locale metadata rather than prepend another locale. A custom root 404 with a different explicit locale must be corrected or omit its locale.

Use a rooted `BaseUrl`; the manifest supplies a missing trailing slash before locale redirects and preview mounting. No deployment-prefix directories are added to the artifact. The #89 middleware fix is integrated, so update preview bookmarks to use the effective mount rather than former domain-root aliases. Rebuild cleanly or restart preview after removing locales/tags to avoid existing stale-output behavior. Site/theme text is not translated and no language switcher is added.

## Browser acceptance

This is repository contributor reference material for the [browser suite](../test/browser/README.md), whose README contains prerequisites and run commands. The suite provides V-008 pager and V-009 locale evidence, not a replacement for the .NET suite, actual preview-server coverage, or the broader V-005 real-device assessment.

### Fixtures and coverage

The [fixture builder](../test/browser/build-sample.mjs) copies sample content/configuration into ignored `test/browser/artifacts/locale-source`, then adds English, Japanese and draft-only locale fixtures. It generates `/docs/` output with default `ko-kr` under `artifacts/prefix`, checks byte-identical artifacts for configured `/docs` and `/docs/`, and regenerates the normal root-site sample `dist`. Settings are process-local; normal sample source content is unchanged.

Six projects combine Chromium, Firefox and WebKit with desktop (1280x800) and mobile-width (375x812) viewports. The [browser cases](../test/browser/page-navigation.spec.mjs) cover:

- Reading sequence, labelled previous/next targets, endpoint/exclusion behavior, keyboard access and horizontal layout.
- Locale-specific home/tag collections, navigation boundaries, root/legacy redirects without JavaScript, tagless/draft-only routes and shared assets.
- Light/dark pager appearance in normal, hover and keyboard-focus states.

Fixtures serve only generated artifacts through loopback servers on dynamic ports, and close their servers and browser contexts afterward. These are controlled static-host requests. [Real preview integration tests](../test/ScissorHands.Web.Tests/ScissorHandsApplicationLocaleTests.cs) separately cover the actual generator/middleware, canonical base-path variants, mount/locale redirects, outside-prefix rejection and regeneration callbacks.

### Contrast and evidence

All pager text must reach **4.5:1** contrast, and focus indicators **3:1** against the adjacent background. Measurements use rendered RGB/alpha values and relative luminance, compositing transparent layers. Unsupported backgrounds or group opacity fail explicitly; transitions are disabled only while measuring settled states. The [contrast math](../test/browser/contrast.mjs) has [independent Node tests](../test/browser/contrast.test.mjs).

The pager uses native links with `tabindex="0"` so WebKit's keyboard mode includes them without positive tab ordering. These component checks do not claim whole-site WCAG or real Safari/iOS/device conformance.

Ignored `test/browser/test-results` contains the JSON report and attached per-engine/theme/state color measurements; failures retain traces and screenshots. Record the actual browser, platform and viewport alongside results. Failed or incomplete runs are not passing acceptance. Execution history belongs in the existing [PRD verification records](../PRD.md#7-next-phase-verification-release-and-evaluation), not in the suite README.

## Source coverage

The following removed or shortened README material is preserved above:

| Original README | Material retained in this document |
| --- | --- |
| Repository root | Detailed page-navigation behavior; source/binary/configuration compatibility; plugin ordering and directory-index migration |
| `ScissorHands.Core` | Content/navigation models, locale context, BaseUrl normalization, manifest/collection contracts, URL helpers, cancellation and obsolete service overloads |
| `ScissorHands.Plugin` | Complete plugin example; ID rules; stages; dependencies and failures; configuration; Razor components; preview behavior; migration steps |
| `ScissorHands.Theme` | Required view roles and tag-view migration; automatic discovery; manifest and assets; layout/cascading data; recursive navigation rendering; URL helpers; plugin selection |
| `ScissorHands.Web` | Full application/configuration/content examples; frontmatter reference; routes; navigation; preview/build; theme and plugin integration; migration notes |
| Sample | Fixture descriptions, navigation experiments and locale/subpath walkthrough; essential execution instructions and entry-point source links remain in the sample README |
| Browser tests | Fixture lifecycle, viewport/engine coverage, contrast thresholds and measurement, reports and evidence limits; prerequisites and run commands remain in the suite README |

Keep repository-specific commands and sample file pointers in the repository. Keep package installation, supported framework, essential compatibility warnings, license links, and required third-party attribution in the corresponding READMEs.
