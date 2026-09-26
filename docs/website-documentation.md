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

Install the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) before running the examples. Repository contributors should use the SDK selection in [global.json](../global.json).

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
    "Author": "Your name",
    "Theme": "default",
    "SiteUrl": "https://example.com",
    "BaseUrl": "/",
    "UseDateInPostUrl": true,
    "Debug": false
  },
  "Plugins": []
}
```

No theme component types need to be registered in `Program.cs`. Setting `Site:Theme` to `default` selects the built-in theme.

This minimal configuration declares no locales and uses English message defaults. To enable localized routes and messages, configure `Site.Locales` and the complete [theme localization catalog](#locale-specific-sites).

### Add the first post

Create `contents/posts/hello.md`:

```markdown
---
title: Hello, ScissorHands
description: My first generated post.
slug: hello-scissorhands
published: 2026-09-11
author: Your name
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

The repository's `sample` application demonstrates these features using local project references. Its README retains the sample-specific run instructions.

## Site configuration

Website destination: `/docs/configuration/`.

The `Site` section in `appsettings.json` provides site-wide settings. Individual document frontmatter provides document metadata.

| Setting | Purpose |
| --- | --- |
| `Title` | Site title used by the theme |
| `Description` | Site description; the engine also makes its rendered HTML available |
| `Locales` | Ordered locale array; the first is primary, and omitted/null/empty disables localization without declaring an implicit locale |
| `Author` | Site-level author information available to themes |
| `HeroImage` | Site-level hero image reference available to themes and plugins |
| `Theme` | Theme slug; use `default` for the built-in theme |
| `SiteUrl` | Public site URL |
| `BaseUrl` | Site base path, such as `/`, `/project` or `/project/`; path prefixes normalize to a trailing slash |
| `UseDateInPostUrl` | Include the publication date in post routes |
| `TimeZone` | Publication timezone for offset-free post dates/times; defaults to `UTC` |
| `Debug` | Site debug setting |

Use `BaseUrl` when publishing below a subpath. Generated navigation and shared URL helpers produce base-relative links rather than hardcoding the domain root.

`SiteManifest.BaseUrl` supplies a trailing slash for a configured path prefix: `/docs` and `/docs/` both become `/docs/`, `/manual/docs` becomes `/manual/docs/`, and `/` remains `/`. This happens during manifest initialization, including configuration binding and direct .NET initialization. Build, preview, themes and plugins use the same effective value, while source settings remain unchanged. Generated HTML therefore uses `<base href="/docs/">` for either spelling. This does not trim whitespace, change case, decode URLs, or validate them. Absolute/network-relative URLs, relative paths without a leading slash, and values containing backslashes, query strings or fragments are not rewritten by this rule and gain no new support guarantee.

Preview serves generated files only at the effective path prefix. With either `/docs` or `/docs/` configured, a page route `parent/child` is served at `/docs/parent/child/`, and `ko-kr/parent/child` at `/docs/ko-kr/parent/child/`; theme assets and images use the same mount. GET/HEAD `/` returns HTTP 302 to `/docs/`, preserving the query and serving no homepage body at root. The entry point is the primary homepage, not a locale redirect. Other outside-prefix requests, including `/parent/child/`, `/docs-other/`, unprefixed assets and POST `/`, return 404. `/docs` and directory redirects retain the prefix and query. Files remain directly under `preview/`; update old preview bookmarks rather than relying on root aliases. Configuring `/` adds no HTTP mount redirect. This is a routing boundary, not authentication. Other base-URL forms remain outside this preview contract; normalization is not general URL validation.

For production, configure the static host to mount `dist/` at the intended prefix and serve directory indexes. Do not copy output into an additional prefix directory or prepend `BaseUrl` to already base-relative links.

During generation, the engine sets `SiteManifest.IsPreview` to indicate preview or production output. It also populates `DescriptionInHtml` from the site description.

`Site.TimeZone` accepts system timezone identifiers such as `Asia/Seoul` and `America/New_York`. Omission selects UTC, never the machine's local timezone; an invalid or blank configured identifier fails generation. For post `published` values, date-only input means midnight at the start of that date in the site timezone, and an offset-free datetime uses the same timezone. Supply a complete calendar date with a four-digit year; partial dates, time-only values, and two-digit years are rejected rather than filled from the machine's date/calendar settings. Explicit `Z` or numeric offsets identify the instant directly and are not reinterpreted. Ambiguous or nonexistent offset-free daylight-saving times fail with source/field context; supply an explicit offset to disambiguate.

For example, `published: 2026-10-01` with `Site.TimeZone: Asia/Seoul` becomes eligible at `2026-09-30T15:00:00Z`. The authored October 1 date remains unchanged in dated URLs, translation pairing, and preview badges. Ordinary-page dates retain their existing parsing behavior and never activate scheduling.

Timezone-rule lookups use millisecond transition precision to keep DST boundary classification consistent across platforms. The returned publication timestamp retains all authored fractional-second ticks; publication eligibility is not rounded.

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

Primary content stays in its existing `contents/pages/` and `contents/posts/` structure, with unprefixed URLs. `Site.Locales` is the only locale inventory: its first item is primary and subsequent items enable additional locales. Top-level `Theme.Localization` supplies application-owned messages without enabling routes. `Site.Theme` remains the selected theme's slug string.

```json
{
  "Site": {
    "Locales": ["en-us", "ko-kr", "ja-jp"],
    "Theme": "default",
    "BaseUrl": "/blog/"
  },
  "Theme": {
    "Localization": {
      "en-us": {
        "TranslationUnavailable": "This page is not currently available in English. Showing the original content.",
        "Draft": "Draft",
        "ScheduledOn": "Scheduled on {0}"
      },
      "ko-kr": {
        "TranslationUnavailable": "이 페이지는 현재 한국어 번역을 제공하지 않습니다",
        "Draft": "초안",
        "ScheduledOn": "{0} 공개 예정"
      },
      "ja-jp": {
        "TranslationUnavailable": "このページは現在日本語翻訳を提供していません",
        "Draft": "下書き",
        "ScheduledOn": "{0}に公開予定"
      }
    }
  }
}
```

For that configuration:

| Surface | URL / behavior |
| --- | --- |
| Primary homepage and About | `/blog/`, `/blog/about/`; no locale redirects |
| Additional locale homepages | `/blog/ko-kr/`, `/blog/ja-jp/`, including primary-content fallbacks |
| Locale tag index | `/blog/ko-kr/tags`, only if its selected documents have tags |
| Locale tag view | `/blog/ko-kr/tags/dotnet`, using translated-or-fallback documents and their selected metadata |
| Primary tag URL | `/blog/tags/dotnet` remains the primary tag collection |
| Shared not-found page | `/blog/404.html`, using the site-default locale |

Every configured additional locale has a generated homepage, even before any translation files exist. The primary homepage is always generated. A locale without eligible tags has no tag pages or built-in Tags link. Generated Home/site-title links, tag links, navigation and previous/next remain in the requested locale, including links to fallback documents.

```text
contents/
  posts/
    hello.md
    ko-kr/hello.md
  pages/
    about.md
    ko-kr/about.md
    not-found.md
```

**Recognition and validation:** only a matching additional `Site.Locales` item makes the directory immediately below `pages/` or `posts/` a translation directory. An undeclared `pages/it/` is ordinary primary content, not automatically Italian. When `Locales` is omitted, null, or empty, localization is disabled, the catalog is not needed, and locale-looking folders remain ordinary content. English message defaults do not create an implicit language or fallback banners. Removing a locale declaration therefore does not exclude files left in its folder: remove, relocate, or mark them draft if they should not publish. Catalog entries alone never enable locales.

Locale identifiers normalize case and underscores/forward slashes to hyphens. Reject invalid/blank array items and duplicate normalized declarations; never skip an invalid first element or sort before selecting primary. Identifiers must be safe language tags; the implementation accepts two/three-letter languages, optional four-letter scripts, and optional two-letter or three-digit regions. Additional locales must have the same script/region structure as the primary: `en-us` with `ko-kr` is valid, `en-us` with `ko` is not. This validates declarations, not ordinary folder names. Content under an explicitly declared primary-locale directory, such as `pages/en-us/`, fails with migration guidance to move it to the primary root. Frontmatter `locale` is removed and always produces a migration error.

**Message completeness:** every declared locale, including a sole primary locale, requires nonblank `TranslationUnavailable`, `Draft`, and `ScheduledOn` strings under `Theme.Localization`. Validate these before rendering in both modes, even on empty sites or when no notice/badge is currently needed. `ScheduledOn` must be a valid complete template with a real date argument `{0}`; escaped `{{0}}` alone is not a date argument, and unsupported argument indexes fail. Missing/blank messages or invalid templates fail with the locale and configuration path. Declared locales never silently borrow missing values from English, parent cultures, or package messages.

Use actual arrays/objects in JSON. Some configuration providers flatten empty arrays/objects to a childless empty-string marker; that marker is accepted as empty because `IConfiguration` cannot distinguish it from an authored `""`. Nonempty scalars, invalid array elements, and mixed scalar/child shapes are rejected. This is a provider representation limitation, not a recommendation to use strings for collections.

**Manifest composition:** the selected theme's `theme.json` remains authoritative for identity, version, slug, stylesheets, and scripts. Application `Theme.Localization` is composed into a fresh effective `ThemeManifest.Localization` read-only snapshot; it does not replace package metadata/assets or mutate input collections. Localization in a package manifest cannot mask missing required application configuration. With no locales declared, effective messages use the English defaults under the `en` catalog key without enabling an `en` route.

**Pairing:** match the content kind and complete locale-relative filename/path: `pages/guides/start.md` pairs with `pages/ko-kr/guides/start.md`, not a differently named file with the same slug. Both resolved slugs must match without the additional prefix. Explicit slugs remain supported, including a matching additional prefix that is stripped before date composition. Nested page `index.md` inference is relative to the locale root; root-level `index.md` remains route `index`. Documents cannot claim generated destinations or primary routes under an active additional-locale prefix.

Both authored files in a post pair must declare valid `published` values. Compare their written year/month/day without timezone conversion; times and offsets may differ. `2026-09-01T09:00:00+09:00` matches `2026-09-01T12:00:00Z`, but a September 1 value does not match an August 31 value representing the same instant. Missing values on either/both sides or different dates fail build and preview with source context. Dated URLs and scheduled badges use that same date. Dates remain optional for unpaired posts.

**Production publication:** the primary document must exist and not be draft; posts must also have reached their publication instant. An eligible translation then replaces primary content at its localized URL; a missing/draft/future-scheduled translation uses eligible primary content with the configured destination-language banner. An ineligible primary suppresses all its variants, even ready translations. Additional-language-only files never publish independently while they are classified as translations. Pair validation does not silently override conflicting metadata.

**Preview selection:** authored translations are shown even when draft or future-scheduled, without substituting primary fallback or showing a fallback notice for the authored translation. A translation receives a draft-status badge when either member of its pair is draft, and posts receive a scheduled-status badge when either publication instant is future. The default theme selects configured `Draft`/`ScheduledOn` messages for the requested render locale, not the article's content language, and formats the authored date as ISO. With no locales it uses `Draft` and `Scheduled on yyyy-mm-dd`. Custom themes own their display formatting. Both badges appear when applicable, even if the statuses originate from different members of the pair. Authored draft flags are not modified. A missing translation still uses primary-content fallback with the normal notice and applicable status badges. A missing primary still suppresses translations in preview.

For missing `pages/ko-kr/about.md`, `/blog/ko-kr/about/` stays at that URL and displays the English About document with its Korean `TranslationUnavailable` notice; it does not redirect. That key describes unavailable content translations, not dictionary-key fallback. Requested locale and actual content language remain distinct in rendering context. Translated-or-fallback collections have one entry per primary identity and use the selected document's title/tags. Navigation preserves visibility and hierarchy, ordering all variants by the primary source path.

**Regeneration and output ownership:** `.scissorhands-output.json` records generated HTML paths inside the output root, preserving their written spelling independently of case-insensitive route-collision checks. After validating the route plan and prior ledger, regeneration removes only stale owned files that obstruct new writes, including case-only replacements and file/directory shape changes. Empty output directories may be pruned; unrelated files are not deleted or silently adopted. An unmanaged blocking file or nonempty directory fails with an actionable error. Other obsolete owned files are removed after successful generation. Planned and newly claimed destinations are recorded before writing so a later run can clean incomplete output after failure. This works across generator instances and retains root/link checks. Keep the ledger for in-place regeneration; initial application preview/build still starts with a clean output directory. Deploy with deletion of withdrawn files rather than merely copying new files. Content source links and generated output links are rejected; this does not certify arbitrary executable theme/plugin behavior.

**SEO and rendering:** primary documents and real translations are self-canonical; a fallback's canonical points to its primary document. Reciprocal `hreflang` alternatives list actual published versions, never fallback copies. Absolute URLs use `SiteUrl` and `BaseUrl`. These document-specific rules and the banner do not apply to home/tag collections or the shared 404. Themes must implement the [locale render context](#locale-render-context) contract.

The shared layout includes a [language switcher](#language-switcher) with engine-prepared document, home, tag, and shared-404 destinations. It can offer primary-content fallbacks, unlike SEO alternatives. `Site.Locales` and global culture are not mutated between renders. Shared resources and other site/theme UI text are not automatically translated; the configured catalog supplies the notice and publication labels.

**Authored content links:** on additional-locale documents, including fallbacks, links to a known primary page/post route are rewritten to that locale's generated equivalent. This runs on the converted document HTML after post-Markdown hooks and before Razor rendering; it does not rewrite the surrounding theme or run another plugin pass. Primary/disabled generation leaves authored URLs unchanged.

Relative links resolve against the site's HTML `<base>` (not the Markdown source directory). Root-relative URLs must include `BaseUrl` when the site is mounted below a subpath: for `/docs/`, `about/` and `/docs/about/` target site content, but `/about/` is outside the mount and stays unchanged. Same-origin absolute and scheme-relative links are recognized; preview also recognizes the originally configured publication origin. Internal links retain their root-relative, base-relative, or absolute form, trailing slash/index-file form, query string, and fragment. Existing explicit configured-locale links are not reprefixed.

Only page/post destinations generated in the current mode participate. Missing or build-withheld targets, generated collection links, fragment-only/query-only links, external URLs and resources are not guessed or rewritten; preview includes its generated draft/scheduled targets. Images, downloads, stylesheets and scripts stay shared; anchors with `download` are left alone. Mark an intentional primary-language link with the per-link opt-out:

```markdown
[Localized About](about/?source=docs#team)
[Primary About](about/?source=docs#team){data-localize="false"}
[Japanese About](ja-jp/about/)
[Shared image](images/sample.svg)
```

On Korean output, the first becomes `ko-kr/about/?source=docs#team` when that document exists; the others remain unchanged. Markdig's generic-attributes syntax supports this without raw HTML. Rewriting changes only anchor destinations, not link text, and is not URL sanitization. Custom post-HTML plugins remain responsible for links they subsequently alter.

**Serving boundary:** preview mounts the artifact at canonical `Site.BaseUrl`. With `/docs`, domain-root GET/HEAD `/` redirects to the primary `/docs/` homepage, not `/docs/en-us/`. Other outside-prefix requests remain 404. Production hosts configure their own mount; no physical deployment-prefix directory is added.

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
| `author` | Document author metadata |
| `twitter_handle` | Author's Twitter handle metadata |
| `hero_image` | Hero image path or URL |
| `published` | Publication date/time; future posts are withheld from builds; required in both authored posts of a primary/translation pair |
| `tags` | Optional tags associated with the document |
| `draft` | Exclude the post/page from production builds; show it with a Draft badge in preview |
| `show_in_navigation` | Whether a page opts into navigation; defaults to `false` |

Draft posts and ordinary pages are included in preview and excluded from builds. A publication timestamp never clears a draft flag. Future-dated posts are included in preview but excluded from builds until eligible; ordinary pages are not scheduled by date. The shared custom 404 retains its existing draft exclusion and receives no publication badges. `show_in_navigation` controls navigation membership, not whether a page is generated.

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
- Configured additional-locale prefixes are applied after locale-relative inference; primary routes stay unprefixed.
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

The field accepts `true` or `false`, and defaults to `false`. Posts and the custom 404 page never enter page navigation. Draft pages participate in preview only, using the same opt-in, hierarchy, hidden-parent suppression, and reading-order rules as other pages.

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

The sequence continues across source directories and outside this example subtree when other eligible pages exist. The built-in page view displays the previous/next anchors automatically. The first page has no previous link, the last has no next link, and a zero- or one-page sequence has neither. Posts, build-excluded drafts, hidden/suppressed pages, 404 content, and non-clickable groups are not targets. Opted-in draft pages and eligible group descendants participate in preview.

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

Groups do not create pages, output files, or placeholder links. Configured additional-locale prefixes are not synthesized as groups when localization is enabled, unless a direct navigation caller supplies an actual visible page at that prefix. Normal generation reserves locale homepages.

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

The shared 404 uses the primary language when localization is enabled. It is excluded from ordinary translation pairing, fallback replication, document SEO and fallback banners. Do not add frontmatter `locale`; that field is no longer supported.

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
| Preview | `preview/` | Starts a local server and regenerates after content or theme-file changes; includes draft posts/pages and scheduled posts with status badges |
| Build | `dist/` | Generates the static site without starting the preview server; excludes drafts and future-scheduled posts |

Generation captures one reference instant before content loading. A post is future-scheduled only when its resolved publication instant is strictly later than that reference. Equality is eligible; a future date alone skips a post rather than failing generation. The same snapshot controls all preview badges during that generation, even if time advances while rendering.

Preview badges appear at the beginning of affected post/page main content and beside their entries in home/tag listings. With no locales declared, the default theme uses `Draft`, `Scheduled on yyyy-mm-dd`, or both; declared locales use their configured `Theme.Localization` labels/templates. Themes own display date formatting with an explicit culture, without changing the authored date or publication eligibility. Pages can have draft but not scheduled badges. Unaffected content and production output have no status badges. These statuses are required for custom themes too; see the [publication status theme contract](#publication-status-theme-contract).

Never deploy `preview/`: it intentionally contains unpublished content. Build and deploy at or after publication time; there is no automatic scheduler or timer-driven preview regeneration. Refresh or trigger regeneration to obtain a new status snapshot. For in-place output, retain the ownership ledger and deploy with removals so rescheduling, draft changes, or switching preview output to build mode withdraw stale pages and generated references.

Refresh the browser after preview regeneration. Razor and C# changes require recompilation, typically with `dotnet watch`.

The repository sample's launch profile does not select an application mode. Supply `--preview` or `--build` explicitly, including in IDE run arguments. `--no-launch-profile` is optional when you also want to bypass profile environment settings. Stop the preview server with Ctrl+C.

The engine sets `SiteManifest.IsPreview` before plugin hooks and rendering. Plugins can use it to suppress production-only side effects.

### Sample walkthrough

The [repository sample](../sample/README.md) uses local project references and the built-in `default` theme. Run it from the repository-root `sample/` directory, which contains `sample.csproj`. The project inherits the root build settings, is not packable, and retains `ScissorHands.Sample` as its assembly name and root namespace. Its README contains the run commands; the following sources demonstrate the engine's behavior:

| Source | What to explore |
| --- | --- |
| [Hello, ScissorHands](../sample/contents/posts/hello-scissorhands.md) | Rich Markdown for desktop/mobile and light/dark comparisons |
| [About](../sample/contents/pages/about.md) | The first page in the sample reading sequence |
| [Korean About](../sample/contents/pages/ko-kr/about.md) | A paired translation with localized links, a primary-language opt-out, and a shared resource link |
| [Parent](../sample/contents/pages/parent/index.md) | An index-first landing page with numbered child files and directories |
| [Child](../sample/contents/pages/parent/01-child.md), [Visible Grandchild](../sample/contents/pages/parent/02-group/visible-grandchild.md), [Child 2](../sample/contents/pages/parent/03-child-2.md) | Cross-directory previous/next links; explicit slugs preserve routes despite filename-ordering prefixes |
| [Hidden Grandchild](../sample/contents/pages/parent/02-group/hidden-grandchild.md) | Published, tagged content omitted from navigation and the reading sequence |
| [Not found](../sample/contents/pages/not-found.md) | Custom root `404.html` content with locale inherited from the site |

The default sequence is **About, Parent, Child, Visible Grandchild, Child 2**. Set Visible Grandchild's `show_in_navigation` to false to remove the empty Group, or disable Parent to hide that entire branch. The endpoint links and visibility rules are described in [reading order](#reading-order-and-previousnext-links).

The sample starts with an empty `Plugins` array, `Site.Locales: ["en-US", "ko-KR"]`, and complete English/Korean `Theme.Localization` entries. Its primary sequence stays unprefixed. `/ko-kr/about/` uses the [Korean translation](../sample/contents/pages/ko-kr/about.md); `/ko-kr/parent/` and other untranslated documents demonstrate the notice and English fallback. Set `Site.BaseUrl` to `/docs` or `/docs/` to explore subpath hosting. See [site configuration](#site-configuration) and [locale-specific sites](#locale-specific-sites). Emptying `Site.Locales` disables localization, but leaves locale-looking directories as ordinary content rather than excluding them.

The custom 404 follows the primary language and offers switcher links to locale homepages without a fallback banner. The About pages demonstrate a localized Parent link with a query/fragment, a primary-language opt-out, and an unchanged shared image. The [browser acceptance fixtures](#browser-acceptance) exercise additional locales in an isolated copy rather than altering the normal sample's sources.

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
    LocaleContext="@LocaleContext"
    Plugins="@Plugins"
    Theme="@Theme"
    Site="@Site">
    <!DOCTYPE html>
    <html lang="@(string.IsNullOrWhiteSpace(PageLocale) ? null : PageLocale)">
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

`MainLayoutBase` calculates page title and description from the site/document. When `Site.Locales` is empty, `CalculatePageLocale()` returns an empty string without assuming a language from English message defaults, document metadata, or render context; the built-in layout omits the HTML `lang` attribute. When locales are declared, the active `LocaleContext` takes precedence, otherwise explicit document metadata falls back to the first site locale. Override `CalculatePageTitle()`, `CalculatePageDescription()`, or `CalculatePageLocale()` to customize those values.

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
| `Locale` | Normalized active locale, independent of the unchanged `Site.Locales` inventory |
| `ContentLocale` | Actual document language, primary for a fallback and requested locale for a translation |
| `IsFallback`, `FallbackMessage` | Whether this document needs the requested locale's configured `TranslationUnavailable` notice; render-context member names remain supported |
| `CanonicalUrl` | Absolute document canonical URL; null for generated home/tag/404 pages |
| `AlternateLanguageUrls` | Read-only engine snapshot of absolute primary/real-translation URLs, keyed by locale |
| `SwitchLanguageUrls` | Read-only engine snapshot of base-relative switch destinations, including generated fallback documents; separate from SEO alternatives |
| `Route` | Resolved current route; source documents retain the loaded pre-hook route snapshot |
| `HomeUrl` | Already escaped, base-relative home URL: `.` for primary or, for example, `ko-kr/` |
| `TagIndexUrl` | Already escaped, base-relative tag-index URL, or null if the locale has no tags |
| `GetTagUrl(tag)` | Compose a raw tag with the prepared home URL using shared tag escaping |

Forward `LocaleContext` through `CascadingMainLayoutBase` for view and plugin components. The renderer keeps it out of ordinary view attributes. Full navigation remains layout-only and the seven required theme roles are unchanged.

The Theme package supplies **abstract base classes, not fixed localization markup**, in `ScissorHands.Theme.Components`. Each theme defines its own Razor components inheriting these classes. The built-in examples live in `src/ScissorHands.Web/themes/default/`; they are not a mandatory template for custom themes.

| Base class | Prepared data/behavior |
| --- | --- |
| `LanguageSwitcherBase` | Read-only `Links` containing `Locale`, `Url`, `Label`, and `IsCurrent`; label/order parameters, validation, and overridable `GetNativeLabel` |
| `LocalizationMetadataBase` | `CanonicalUrl` and read-only `AlternateLanguageUrls` for theme-owned head markup |
| `LocalizationFallbackBannerBase` | `IsFallback`, `FallbackMessage`, required `BannerAttributes`, and encoded `FallbackMessageContent` |

Import `ScissorHands.Theme.Components` in the theme's `_Imports.razor`, alongside `ScissorHands.Theme`. Place the theme's metadata component in `<head>`, its switcher in the shared layout, and its banner above page/post content. Mark the article container with `lang="@LocaleContext?.ContentLocale"`. Metadata markup should be omitted when `CanonicalUrl` is null; the shared 404 and generated collections do not acquire paired-document SEO.

For example, a theme can choose its own banner element, formatting, classes, and accessible role:

```razor
@inherits LocalizationFallbackBannerBase

@if (IsFallback)
{
    <section class="my-notice" role="note" @attributes="BannerAttributes"><strong>@FallbackMessageContent</strong></section>
}
```

Render `FallbackMessageContent` rather than raw HTML or only the message string. This fragment encodes the configured text and records actual message rendering; it emits no surrounding HTML. Apply `BannerAttributes` to the message-bearing element to provide the required marker and requested-language annotation. Merely inheriting the base, declaring support, or embedding a lookalike in Markdown does not satisfy the rendering receipt.

After post-HTML hooks, the generator requires exactly one marked notice whose exposed text matches the configured message (ignoring surrounding formatting whitespace). Text under HTML `hidden` or `aria-hidden="true"` does not count, whether the hiding element is an ancestor, the marked element, or a nested child. A child's `aria-hidden="false"` cannot undo an ancestor's hiding. Theme-owned formatting and hidden decorative content are allowed when the complete required message remains exposed. Script/style/template message content is rejected. Missing, altered, partially hidden, or removed notices fail generation. Themes remain responsible for placement and CSS visibility/accessibility; this is structural HTML validation, not an arbitrary stylesheet audit.

Use layout `GetHomeUrl()` and `GetTagIndexUrl()` instead of hard-coded `.`/`tags`; without context they retain those original values. Existing post/page/tag-list `GetTagUrl` wrappers become locale-aware when context exists. Core static helpers keep their context-free contracts. Context values are prepared rendering data, not a general URL sanitizer.

Locale inventory, collections and navigation are prepared from loaded documents before hooks. They are not recomputed when plugins return replacement locale/slug/title metadata; per-document hook propagation is retained without promising cross-collection refresh or a second plugin pass.

Normal generation supplies both navigation parameters automatically. For compatibility, `ComponentRenderer` prepares a tree when an older caller supplies only `NavigationPages` to a layout derived from `MainLayoutBase`. An explicitly supplied tree is used unchanged. Direct component rendering should supply `NavigationTree`.

### Publication status theme contract

All themes must render prepared preview status for affected posts/pages and their homepage/tag-list entries. `ContentDocument.PublicationStatus` is an immutable pre-hook snapshot: `Route` identifies the generated document, `IsDraft` includes inherited primary draft status, and nullable `ScheduledDate` is the authored date when this post or its primary is future-scheduled. `IsScheduled` and `HasBadges` are derived flags. Production, custom 404, and unaffected content have no required badges. Do not infer status from wall-clock time, `Metadata.Draft` alone, or converted dates in theme code.

Define a theme-owned component derived from `ScissorHands.Theme.Components.PublicationBadgeBase`. It receives the cascading `Document`/`Site` by default; set `Content` and `Placement="PublicationBadgePlacement.Listing"` for an individual collection entry. Each prepared `Badges` item exposes `Kind` (`draft` or `scheduled`), nullable `ScheduledDate`, required machine-readable `Attributes`, and `RenderContent(themeLabel)` for encoded delivery. The base does not supply human-readable wording or a display date format. For example, a theme can choose:

```razor
@using System.Globalization
@inherits PublicationBadgeBase

@foreach (var badge in Badges)
{
    var label = badge.ScheduledDate is { } date
        ? $"Planned for {date.ToString("MMM dd, yyyy", CultureInfo.GetCultureInfo("en-US"))}"
        : "Draft";
    <span class="my-status" @attributes="badge.Attributes">@badge.RenderContent(label)</span>
}
```

The default theme keeps its [PublicationBadges component](../src/ScissorHands.Web/themes/default/Components/PublicationBadges.razor) in its `Components/` directory.

The effective `ThemeManifest.Localization` catalog is already available through the existing theme cascade. Select messages using `LocaleContext.Locale` for requested-language UI, falling back to the first declared site locale when no render context exists; never select the fallback document's content language. The default component uses the configured `Draft` and complete `ScheduledOn` template and keeps its explicit invariant ISO display date. If `Site.Locales` is empty, it uses `ThemeLocalization.English` rather than inferring a locale from catalog keys. No global culture mutation, second `.resx` catalog, or mandatory `DateFormat` configuration key is involved.

Render `RenderContent(label)`, not raw HTML or only a plain label expression: it encodes the theme's nonempty text and records delivery of that text. Receipts prevent a theme from satisfying the contract by inheritance alone or lookalike authored Markdown. The default Razor component formats the date as invariant `yyyy-MM-dd` and selects the requested locale's configured wording, using English defaults only with no locales declared. Another theme can use `MMM dd, yyyy`, `dd/MM/yyyy`, or another explicit cultural format. Do not implicitly depend on the build machine's culture or convert the authored date to another timezone. Both statuses remain required when both flags apply.

In post/page views, apply `PublicationBadgeBase.GetRegionAttributes(Document, PublicationBadgePlacement.Detail)` to the content `article` inside `<main>` (or a `role="main"` container), and place the badge component first, before headings, hero images, publication dates, and authored content. In home/tag views, apply `GetRegionAttributes(entry, PublicationBadgePlacement.Listing)` to each corresponding listing-entry wrapper inside main content, and render the badge component beside that entry's link. For example:

```razor
<li @attributes="PublicationBadgeBase.GetRegionAttributes(post, PublicationBadgePlacement.Listing)">
    <a href="@GetContentUrl(post.Metadata.Slug)">@post.Metadata.Title</a>
    <MyPublicationBadges Content="@post" Placement="PublicationBadgePlacement.Listing" />
</li>
```

Regions use `data-publication-content` or `data-publication-entry` with the stable route. Badges use `data-publication-badge`, `data-publication-route`, and `data-publication-placement`. Scheduled badges additionally carry `data-publication-date` as invariant ISO `yyyy-MM-dd`, independent of the visible date format; draft badges have no date attribute. Preserve these attributes through post-HTML hooks. Each affected route must have its own unique region; badges in navigation, unrelated entries, or outside main content do not satisfy the contract. No additional discovered theme role or cascading layout parameter is required.

Renderer receipts and final-HTML validation enforce the required statuses, correct machine-readable dates, and complete visible theme-provided labels, not a fixed English sentence or display format. They reject missing, duplicate, wrong-route/date, altered, hidden/inert, or incorrectly placed badges, including omission of one of two required labels. Post-HTML hooks must preserve the labels actually rendered by the theme; hidden decorative children are permitted only when the complete label remains exposed. Detail badges must precede actual article content. Production output and unaffected renders must not contain publication badge markers. Themes own the correctness of translations/formatting, CSS, and accessible styling; structural validation cannot interpret arbitrary human languages or audit stylesheets.

Generation prepares collections, effective statuses, and locale contexts before hooks. A replacement document from a Markdown hook retains its original status snapshot rather than resetting eligibility or recomputing collections. Custom loaders supplying typed `Published` values must supply the intended `DateTimeOffset` instants; `Site.TimeZone` interprets raw frontmatter in the built-in loader, not already typed offsets.

### Language switcher

Define a theme-owned Razor component with `@inherits LanguageSwitcherBase` and place it in the shared cascading layout, outside the main content article. The base provides prepared `Links` without emitting HTML; themes choose their own structure. The built-in `LanguageSwitcher.razor` places ordinary accessible links between the site header and `<main>`, needs no JavaScript, and renders nothing when localization is disabled or only one destination language exists.

The engine supplies supported locale identifiers, current requested locale, and valid targets through `LocaleContext.SwitchLanguageUrls`; themes must not reconstruct URLs or infer translation availability.

| Current page | Switch destination |
| --- | --- |
| Individual page/post | Equivalent translation or generated fallback in the selected locale |
| Homepage | Selected locale homepage |
| Tag index | Selected locale tag index if generated, otherwise its homepage |
| Tag page | Same tag in that locale if generated, otherwise its homepage |
| Shared `404.html` | Selected locale homepage; there are no localized 404 copies |

Each prepared link's `IsCurrent` reflects the requested locale, not a fallback article's language. The built-in markup maps it to `aria-current="true"` and includes `lang`, `hreflang`, and `tabindex="0"`; these links are not SEO `rel="alternate"` declarations. Custom themes should preserve accessible, no-JavaScript switching while choosing their own markup. Generated home/tag pages and the shared 404 still do not receive fallback banners or paired-document SEO.

`LanguageSwitcherBase` provides native-language label defaults using .NET culture names, for example English, 한국어 and 日本語. Multiple variants of the same language use full native culture names to distinguish their region/script. Themes may override `GetNativeLabel` or use these inherited parameters without changing routing:

| Parameter | Type / behavior |
| --- | --- |
| `Labels` | Optional `IReadOnlyDictionary<string, string>` of normalized locale to plain-text label; blank labels fail; render labels using normal encoded Razor expressions |
| `LocaleOrder` | Optional `IReadOnlyList<string>` of locales to display first, followed by remaining destinations in engine order |
| `AriaLabel` | Accessible navigation name; defaults to `Language` and can be localized by the theme |
| `Class` | Optional theme CSS classes; the built-in markup adds them alongside `language-switcher` |

These display settings neither enable locales nor change their identifiers, destinations, or publication eligibility. The default engine order is primary language followed by additional locales in ordinal order.

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

```bash
dotnet add package ScissorHands.Plugin --prerelease
```

Browse the [official plugins repository](https://github.com/getscissorhands/plugins) for available extensions.

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

The engine resolves tag routes before Razor rendering and supplies a synthetic page `Document` with the final slug (`tags` or `tags/{tag}` for primary/disabled generation; `<locale>/tags` or `<locale>/tags/{tag}` for configured additional locales). Locale-enabled homepages also receive a route-only rendering document. Layouts must forward `Document` and `LocaleContext` through `CascadingMainLayoutBase` for plugin components to receive them.

The rendering document's title, Markdown, HTML and source path are empty, with no document-specific description, author, Twitter handle, image or publication date. `Metadata.Locale` is the active normalized locale when enabled and remains null on disabled-mode tag documents. Site title/description defaults and tag-view headings remain unchanged. A non-null `Document` does not imply a post. Collections/navigation are locale-scoped when enabled; tag-page adjacency remains empty.

Post-HTML hooks retain synthetic tag titles (`Tags` or `Tag: {tag}`) and rendered `Html`, with the same resolved slug/locale supplied during rendering. Generated collections bypass Markdown hooks and receive post-HTML processing. Each individual translation/fallback document follows the existing pre-Markdown, conversion, post-Markdown, Razor, post-HTML sequence. Compose publication URLs using `Site.SiteUrl`, `Site.BaseUrl` and the supplied slug, without rebuilding or escaping it again. For tag `C#` in the configured additional locale `ko-KR`, the slug is `ko-kr/tags/c%23`; with site URL `https://example.com` and base `/blog/`, the publication URL is `https://example.com/blog/ko-kr/tags/c%23`.

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

`ContentDocument.PublicationStatus` carries the engine's immutable preview-only status snapshot, separate from authored metadata. See the [publication status theme contract](#publication-status-theme-contract) for its fields, inheritance, and required rendering behavior.

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
    Locales = ["en-US"],
    Theme = "default",
    SiteUrl = "https://example.com",
    BaseUrl = "/",
    UseDateInPostUrl = true,
};
```

`SiteManifest.Locales` snapshots its input, treats null as empty, and enables localization only when nonempty. Validation rejects invalid members; the model does not silently skip them.

`ThemeManifest` describes the selected theme, its assets, and the effective application localization snapshot:

```csharp
var theme = new ThemeManifest
{
    Name = "My Theme",
    Slug = "my-theme",
    Stylesheets = ["/assets/theme.css"],
    Scripts = ["/assets/theme.js"],
    Localization = new Dictionary<string, ThemeLocalization?>
    {
        ["en-us"] = ThemeLocalization.English,
    },
};
```

`Stylesheets` and `Scripts` are non-null `IReadOnlyList<string>` collections and are defensively copied during initialization. `Localization` is a defensive read-only dictionary snapshot of immutable `ThemeLocalization` records. Configured records have nullable `TranslationUnavailable`, `Draft`, and `ScheduledOn` fields so omitted values can be diagnosed rather than silently defaulted. `ThemeLocalization.English` supplies the explicit English default record. Application configuration binds only the catalog, then `ThemeService` composes it with the package's metadata; custom `IThemeService` implementations must return complete effective messages for declared locales.

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

`UseLocaleInUrl` was removed previously. `Site.Locale` and `Site.LocalizationFallbackMessages` are now replaced by ordered `Site.Locales` and top-level `Theme.Localization`. Legacy configuration keys fail with migration guidance rather than silently disabling localization. Declare the former primary first, followed by the desired additional locales; changing the first item changes which language has unprefixed routes. Omitted/null/empty `Locales` disables localization and leaves the HTML language unspecified, even though presentation messages default to English. `Site.Theme` remains a slug string, not an object.

Remove every frontmatter `locale` field, including on the shared 404. Keep primary files at their original unprefixed locations, moving old primary-locale-directory files back there and preserving intended slugs. Put translations in configured additional-locale directories with matching filenames/relative paths, matching slugs, and required matching calendar dates for paired posts. Primary home/tag URLs no longer redirect. Hosts migrating from the former prefixed-primary contract must supply any desired redirects from old published primary URLs; this feature does not infer historical URLs.

Move old fallback-message values into `Theme.Localization[locale].TranslationUnavailable`, then supply `Draft` and a valid `ScheduledOn` template for every declared locale, including primary. Blank/missing messages fail before rendering even when fallback or badges are not currently needed; package/English/parent-culture defaults do not fill gaps for declared locales. Additional locale routes exist even before translations do, but only `Site.Locales` enables them. Removing a declaration leaves files in that folder as ordinary nested content, so remove or mark them draft if they should not publish. In builds, draft/scheduled primaries suppress active translations and fallback never uses ineligible primary content. Preview includes drafts and scheduled posts with inherited status badges; a missing primary still suppresses translations.

In C#, replace `SiteManifest.Locale` initializers with `Locales = [...]` and use its first element for primary-language selection. Remove `LocalizationFallbackMessages` initializers and supply complete `ThemeManifest.Localization` data. The effective manifest composes application messages with package identity/assets without changing theme discovery or explicit `AddLayouts` overrides. The existing `LocaleContext.FallbackMessage` and fallback-banner fragment remain rendering APIs; their value now comes from `TranslationUnavailable`.

Custom themes must forward `LocaleContext`, import `ScissorHands.Theme.Components`, and provide their own components derived from `LanguageSwitcherBase`, `LocalizationMetadataBase`, and `LocalizationFallbackBannerBase`. The former concrete localization components in the Theme package are replaced by these bases; Razor implementations now belong to the default theme or the consuming theme. No new required view-discovery role is added. Render the banner's encoded `FallbackMessageContent` with `BannerAttributes`, annotate actual content language, and retain Home/Tags helpers. Missing fallback notices fail rendering. Existing seven view roles and public URL helpers remain. Authored internal document links now follow the active additional locale; add `{data-localize="false"}` to intentional primary-language links. Resources, external links and explicit configured-locale targets remain unchanged. Plugins receive requested and actual language in context and should not prepend another locale. Prepared collections/context remain pre-hook snapshots, not automatically recomputed after plugin metadata changes.

Use rooted `BaseUrl` and an absolute HTTP(S) `SiteUrl` without credentials/query/fragment for document SEO. In-place generation retains its owned-output ledger and deletes withdrawn pages; clean rebuild once when upgrading from versions without that ledger, and ensure deployment removes withdrawn files. The switcher supplies native language labels, but other site/theme UI text is not automatically translated.

### Scheduled publication and draft preview

Future-dated posts previously appeared immediately; production builds now withhold them until their publication instant. Set `Site.TimeZone` explicitly when date-only or offset-free post values should use a timezone other than the UTC default. Explicit offsets remain authoritative, and authored dates in URLs and translation pairs do not move.

Draft posts and ordinary pages are now visible in preview, including applicable lists and opted-in navigation. Never deploy preview output. Custom themes must implement the [publication status contract](#publication-status-theme-contract), including both badges for combined/inherited states and per-entry listing indicators; a missing required badge fails preview generation. Custom 404 behavior is unchanged. Existing generator constructor calls still use the system clock; dependency-injected hosts may supply a `TimeProvider` for deterministic generation.

Themes using the initial fixed-label badge API must replace `badge.Text`/`badge.Content` with theme-owned text generated from `badge.Kind` and `badge.ScheduledDate`, rendered through `badge.RenderContent(label)`. Keep `badge.Attributes` on the badge element, including the new machine-readable scheduled date. The default component retains its ISO display date and no-locales English defaults; declared locales now select their configured wording. No publication or timezone rules change.

## Browser acceptance

This is repository contributor reference material for the [browser suite](../test/browser/README.md), whose README contains prerequisites and run commands. The suite provides V-008 pager and V-009 locale evidence, not a replacement for the .NET suite, actual preview-server coverage, or the broader V-005 real-device assessment.

### Platform setup

The suite requires the repository's .NET SDK, Node.js 24, and the browser builds installed by Playwright. Run the shared install/test commands in the suite README. On Linux, replace the browser-install command with:

```bash
npx playwright install --with-deps chromium firefox webkit
```

This also installs the browser system dependencies. Browsers run headlessly with Playwright-managed builds; no separately installed Firefox application is needed. `npm test` regenerates ignored sample/test output without modifying sample source. Reports, traces, and screenshots remain under ignored `test/browser/test-results`; see [contrast and evidence](#contrast-and-evidence) for interpretation.

### Fixtures and coverage

The [fixture builder](../test/browser/build-sample.mjs) copies sample content/configuration into ignored `test/browser/artifacts/locale-source`, then adds paired English/Korean post fixtures, a configured Japanese fallback-only locale, and unpublished content. It generates `/docs/` output with primary `en-us` under `artifacts/prefix`, checks byte-identical artifacts for configured `/docs` and `/docs/`, and regenerates the normal root-site sample `dist`. It also starts a short-lived preview process, checks its HTTP readiness and scheduled output, copies the draft/scheduled artifact to `artifacts/preview`, and stops that process. Settings are process-local; normal sample source content is unchanged.

Six projects combine Playwright-managed Chromium, Firefox and WebKit with desktop (1280x800) and mobile-width (375x812) viewports. Browsers run headlessly by default. The [browser cases](../test/browser/page-navigation.spec.mjs) cover:

- Reading sequence, labelled previous/next targets, endpoint/exclusion behavior, keyboard access and horizontal layout.
- Primary URL stability, translated/fallback collections and navigation, encoded language-annotated notices without JavaScript, document SEO, unpublished routes and shared assets.
- Preview Draft/Scheduled badges on primary and translated documents, homepage/tag entries, combined/inherited status, draft-page navigation, and light/dark visible placement at desktop/mobile widths.
- No-JavaScript and keyboard language switching on documents and generated pages, missing-tag homepage targets, authored-link localization/opt-out with query/fragment preservation, and switcher label/text/focus contrast.
- Light/dark pager appearance in normal, hover and keyboard-focus states.

Fixtures serve only generated artifacts through loopback servers on dynamic ports, and close their servers and browser contexts afterward. These are controlled static-host requests. [Real preview integration tests](../test/ScissorHands.Web.Tests/ScissorHandsApplicationLocaleTests.cs) separately cover the actual generator/middleware, base-path variants, mount redirects, outside-prefix rejection, fallback and regeneration/withdrawal callbacks.

Firefox uses Playwright's managed headless browser, not a separately installed desktop Firefox. The test configuration sets `MOZ_APP_DATA` and `MOZ_LOCAL_APP_DATA` to writable, ignored `artifacts/firefox-runtime/data` and `cache` directories. These are Firefox's application-data roots, distinct from the temporary per-launch profiles managed by Playwright. This avoids startup failures in default application-data initialization that can report `Could not find profile folder` even when the specified temporary profile exists. No browser security settings or personal Firefox profiles are changed.

After the normal prerequisites/install steps, run only Firefox from `test/browser` with:

```bash
npm test -- --project=firefox-desktop --project=firefox-mobile
```

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
| `ScissorHands.Plugin` | Complete plugin example; ID rules; stages; dependencies and failures; configuration; Razor components; preview behavior; migration steps; official plugin catalog link |
| `ScissorHands.Theme` | Required view roles and tag-view migration; automatic discovery; manifest and assets; layout/cascading data; recursive navigation rendering; URL helpers; plugin selection |
| `ScissorHands.Web` | Full application/configuration/content examples; frontmatter reference; routes; navigation; preview/build; theme and plugin integration; migration notes |
| Sample | Fixture descriptions, IDE mode arguments, publication timing, navigation experiments and locale/subpath walkthrough; essential run commands and the deployment warning remain in the sample README |
| Browser tests | Linux dependency setup, managed-browser behavior, fixture lifecycle, viewport/engine coverage, contrast thresholds and measurement, reports and evidence limits; prerequisites and shared run commands remain in the suite README |

Keep repository-specific commands and sample file pointers in the repository. Keep package installation, supported framework, essential compatibility warnings, license links, and required third-party attribution in the corresponding READMEs.
