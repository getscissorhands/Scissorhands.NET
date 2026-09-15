# Changelog

This file summarizes the repository's tagged releases, newest first, from the
initial commit onward. Each release covers changes since the preceding tag in
the same Git history; the first tag in each history includes its initial
development. Dates are the tagged commits' calendar dates, which can differ from
the publication dates shown in
[GitHub Releases](https://github.com/getscissorhands/Scissorhands.NET/releases).

The original engine began on
[2015-08-08](https://github.com/getscissorhands/Scissorhands.NET/commit/f35f751effe868d336a800229dae5ae5cd57616d).
The modern Blazor line has a separate root commit on
[2022-01-01](https://github.com/getscissorhands/Scissorhands.NET/commit/53b1ca3491e41b6e83dbebb1393ad573a75ab1fb).
These are unrelated Git histories, so the first modern release is not presented
as a tag-to-tag comparison with the legacy alpha.

Preview releases belong to the `vnext` development line. Their inclusion here
does not imply that the same implementation is present on `main`. Use the tag
matching your installed package version.

## v1.0.0-preview.20260915.1 - 2026-09-15

[Release](https://github.com/getscissorhands/Scissorhands.NET/releases/tag/v1.0.0-preview.20260915.1) | [Changes](https://github.com/getscissorhands/Scissorhands.NET/compare/v1.0.0-preview.20260914.1...v1.0.0-preview.20260915.1)

- Fixed generated tag pages to expose their resolved routes to layouts and Razor plugin components before rendering, while preserving site-level metadata, tag collections, and post-HTML hook context (#96).
- Improved repository readiness, contribution guidance, issue templates, and release/migration documentation (#91).
- Updated GitHub Actions: `actions/download-artifact` v6 to v8, `softprops/action-gh-release` v2 to v3, and `actions/upload-artifact` v5 to v7 (#92, #93, #94). The new download action fails on artifact digest mismatches by default.

## v1.0.0-preview.20260914.1 - 2026-09-14

[Release](https://github.com/getscissorhands/Scissorhands.NET/releases/tag/v1.0.0-preview.20260914.1) | [Changes](https://github.com/getscissorhands/Scissorhands.NET/compare/v1.0.0-preview.20260913.1...v1.0.0-preview.20260914.1)

- Added opt-in page navigation through frontmatter, hierarchical navigation, hidden-parent suppression, and non-link groups for missing parents (#87).
- Prepared navigation once per generation and retained the flat `NavigationPages` compatibility path alongside `NavigationTree`.
- Added previous/next page links and accessible built-in navigation, with Chromium, Firefox, and WebKit acceptance coverage at desktop and mobile sizes (#90).
- **Breaking:** File-backed navigation now follows source filenames in index-first, depth-first order instead of title order. Use numeric filename prefixes for ordering and explicit slugs to preserve URLs.
- **Breaking:** Nested page `index.md` files without explicit slugs now use their containing directory's route. Set an explicit slug to retain an existing `/index` URL.
- **Breaking:** Custom themes must supply all seven view roles, including `TagListViewBase` and `TagViewBase`; implicit built-in tag-view fallback was removed.
- Expanded page/tag and sample coverage, aligned requirements and design documents, and consolidated detailed documentation in the website handoff (#87, #88, #90).

## v1.0.0-preview.20260913.1 - 2026-09-13

[Release](https://github.com/getscissorhands/Scissorhands.NET/releases/tag/v1.0.0-preview.20260913.1) | [Changes](https://github.com/getscissorhands/Scissorhands.NET/compare/v1.0.0-preview.20260912.1...v1.0.0-preview.20260913.1)

- Added the shared `GetThemeUrl` helper for derived layouts and reused it in the default theme, preserving base-relative theme asset URLs (#86).
- Clarified explicit preview startup, simplified getting-started instructions, and added package-version badges.

## v1.0.0-preview.20260912.1 - 2026-09-12

[Release](https://github.com/getscissorhands/Scissorhands.NET/releases/tag/v1.0.0-preview.20260912.1) | [Changes](https://github.com/getscissorhands/Scissorhands.NET/compare/v1.0.0-preview.20260102.2...v1.0.0-preview.20260912.1)

- Added tag-list and individual tag pages with corresponding theme views (#79).
- Improved automatic theme discovery, generator reliability, cancellation support, and plugin manifest validation; retained obsolete theme-service overloads for compatibility (#83).
- Redesigned the built-in theme, added a runnable integration sample and package-specific READMEs, centralized build/dependency configuration, migrated to SLNX, and adopted xUnit v3.
- Updated release automation to publish to NuGet.org using OIDC.
- **Breaking:** Plugins now require lowercase ASCII kebab-case IDs in implementations, manifests, dependencies, and Razor selectors. `Name` is display-only; rebuild extensions and migrate configuration (#84).
- **Breaking:** Plugin execution now honors stage-scoped `DependsOn` declarations, with ordinal ID ordering among ready plugins rather than incidental discovery or registration order.
- **Breaking:** Theme asset collections and plugin options moved to read-only collection contracts; consumers must treat manifests as immutable inputs.
- Added approved PRD, TRD, and TDD documents and expanded repository/agent guidance (#85).

## v1.0.0-preview.20260102.2 - 2026-01-02

[Release](https://github.com/getscissorhands/Scissorhands.NET/releases/tag/v1.0.0-preview.20260102.2) | [Changes](https://github.com/getscissorhands/Scissorhands.NET/compare/v1.0.0-preview.20251226.1...v1.0.0-preview.20260102.2)

- Refactored application composition, content watching, and theme/plugin rendering behind testable abstractions; expanded tests for all four packages (#71).
- Added custom/generated 404 pages and `NotFoundViewBase` (#72).
- Added document locale handling and locale-aware layout metadata and content routes (#73).
- Added cascading parameters for theme views and the application builder API (#74).

## v1.0.0-preview.20251226.1 - 2025-12-26

[Release](https://github.com/getscissorhands/Scissorhands.NET/releases/tag/v1.0.0-preview.20251226.1) | [Changes](https://github.com/getscissorhands/Scissorhands.NET/compare/v1.0.0-preview.20251221.1...v1.0.0-preview.20251226.1)

- Added a default hero image and its site configuration URL.
- Added calculated page descriptions with document-to-site fallback.
- **Breaking:** Renamed the layout's `SiteTitle`/`CalculateSiteTitle` members to `PageTitle`/`CalculatePageTitle`.
- Added PowerShell and shell helpers for GitHub Packages authentication setup.

## v1.0.0-preview.20251221.1 - 2025-12-21

[Release](https://github.com/getscissorhands/Scissorhands.NET/releases/tag/v1.0.0-preview.20251221.1) | [Changes](https://github.com/getscissorhands/Scissorhands.NET/compare/v1.0.0-preview.20251220.1...v1.0.0-preview.20251221.1)

- Made the layout, index, page, and post base classes abstract.
- Added calculated layout titles combining the current document title with the site title.
- Cleaned up plugin project metadata/comments.

## v1.0.0-preview.20251220.1 - 2025-12-20

[Release](https://github.com/getscissorhands/Scissorhands.NET/releases/tag/v1.0.0-preview.20251220.1) | [Changes](https://github.com/getscissorhands/Scissorhands.NET/compare/v1.0.0-preview.20251219.1...v1.0.0-preview.20251220.1)

- Expanded plugin components and layouts to receive both document collections and the current document.
- Changed component rendering to render the selected layout directly and forward document, site, theme, and plugin parameters.
- Added the project logo asset.

## v1.0.0-preview.20251219.1 - 2025-12-19

[Release](https://github.com/getscissorhands/Scissorhands.NET/releases/tag/v1.0.0-preview.20251219.1) | [Changes](https://github.com/getscissorhands/Scissorhands.NET/compare/v1.0.0-preview.20251218.1...v1.0.0-preview.20251219.1)

- **Breaking:** Added a `SiteManifest` argument to content-plugin hooks.
- Added site locale, site URL, and hero-image settings, plus author Twitter-handle metadata.
- **Breaking:** Changed hero-image frontmatter from `hero` to `hero_image`; added `twitter_handle`.
- **Breaking:** Replaced configurable content/output directory properties with the standard `contents`, `dist`, and `preview` directory constants.
- Updated preview mode to expose its listening address through the site manifest.

## v1.0.0-preview.20251218.1 - 2025-12-18

[Release](https://github.com/getscissorhands/Scissorhands.NET/releases/tag/v1.0.0-preview.20251218.1) | [Changes](https://github.com/getscissorhands/Scissorhands.NET/compare/v1.0.0-preview.20251217.2...v1.0.0-preview.20251218.1)

- Replaced the default theme's placeholder home page with post links, optional descriptions, and an empty-site welcome message.

## v1.0.0-preview.20251217.2 - 2025-12-17

[Release](https://github.com/getscissorhands/Scissorhands.NET/releases/tag/v1.0.0-preview.20251217.2) | [Changes](https://github.com/getscissorhands/Scissorhands.NET/compare/v1.0.0-preview.20251217.1...v1.0.0-preview.20251217.2)

- Added the built-in default Razor theme, favicon, and project icons.
- Added default site configuration when the `Site` section is absent and a default theme manifest fallback.

## v1.0.0-preview.20251217.1 - 2025-12-17

[Release](https://github.com/getscissorhands/Scissorhands.NET/releases/tag/v1.0.0-preview.20251217.1) | [Changes](https://github.com/getscissorhands/Scissorhands.NET/compare/v1.0.0-preview.20251216.4...v1.0.0-preview.20251217.1)

- Added `SiteManifest.DescriptionInHtml` to keep the rendered Markdown description separate from its source text.
- Added optional outer-paragraph trimming to Markdown conversion and updated related service contracts and call sites.

## v1.0.0-preview.20251216.4 - 2025-12-16

[Release](https://github.com/getscissorhands/Scissorhands.NET/releases/tag/v1.0.0-preview.20251216.4) | [Changes](https://github.com/getscissorhands/Scissorhands.NET/compare/v1.0.0-preview.20251216.3...v1.0.0-preview.20251216.4)

- Added `Site.Debug` to print loaded assemblies while diagnosing plugin discovery.
- **Breaking:** Renamed application initialization from `InitializeAsync` to `BuildAsync` and made service registration accept configuration.

## v1.0.0-preview.20251216.3 - 2025-12-16

[Release](https://github.com/getscissorhands/Scissorhands.NET/releases/tag/v1.0.0-preview.20251216.3) | [Changes](https://github.com/getscissorhands/Scissorhands.NET/compare/v1.0.0-preview.20251216.2...v1.0.0-preview.20251216.3)

- Expanded plugin discovery to load assemblies from the application directory as well as assemblies already loaded into the process.

## v1.0.0-preview.20251216.2 - 2025-12-16

[Release](https://github.com/getscissorhands/Scissorhands.NET/releases/tag/v1.0.0-preview.20251216.2) | [Changes](https://github.com/getscissorhands/Scissorhands.NET/compare/v1.0.0-preview.20251216.1...v1.0.0-preview.20251216.2)

- Allowed null values in `PluginManifest.Options` by changing its value type to `object?`.

## v1.0.0-preview.20251216.1 - 2025-12-16

[Release](https://github.com/getscissorhands/Scissorhands.NET/releases/tag/v1.0.0-preview.20251216.1) | [Changes](https://github.com/getscissorhands/Scissorhands.NET/compare/v1.0.0-preview.20251215...v1.0.0-preview.20251216.1)

- Included the package version in generator metadata.
- Expanded plugin/component context, exposed plugin-runner contracts, skipped plugins without matching manifests, and added cancellation checks.
- Documented manifests, loaders, rendering, generation, and theme/service contracts.
- Expanded GitHub Actions build and package automation.

## v1.0.0-preview.20251215 - 2025-12-15

[Release](https://github.com/getscissorhands/Scissorhands.NET/releases/tag/v1.0.0-preview.20251215) | [Changes](https://github.com/getscissorhands/Scissorhands.NET/compare/v1.0.0-preview.20251212...v1.0.0-preview.20251215)

- Added `PluginComponentBase` for Razor plugin output and converted site descriptions from Markdown to HTML during generation.
- Included theme `manifest.json` files in copied assets and removed the bundled sample site/MinimalBlog copy from the solution.

## v1.0.0-preview.20251212 - 2025-12-12

[Release](https://github.com/getscissorhands/Scissorhands.NET/releases/tag/v1.0.0-preview.20251212) | [History from the modern root](https://github.com/getscissorhands/Scissorhands.NET/commits/v1.0.0-preview.20251212)

- The modern history began on 2022-01-01 with repository setup, followed by a Blazor WebAssembly skeleton, Core library/tests, a route generator, local-settings scripts, and .NET 6 CI.
- Reset and rebuilt the implementation in December 2025 as a .NET 10, Blazor/Razor-based static site generator.
- Introduced the modern Core, Plugin, Theme, and Web packages, Markdown/YAML content loading, and Razor layouts for index, post, and page output.
- Added pre-Markdown, post-Markdown, and post-HTML plugin processing; explicit preview/build modes; content watching; and content/theme asset copying.
- Added package metadata, getting-started instructions, and GitHub Packages/release automation.

## Legacy untagged changes - 2016-01-15

[Changes after the alpha on legacy master](https://github.com/getscissorhands/Scissorhands.NET/compare/Scissorhands.NET-v1.0.0-alpha-76...30c1abc8164916d67eab469c36e9b061db522b10)

These commits followed the legacy alpha on `master`; no subsequent release tag
exists in that history.

- Added the `Scissorhands.Themes.Tests` project and `ThemeLoader` tests.
- Made web application settings properties virtual for testing/mocking.
- Merged release and write-screen development work.

## Scissorhands.NET-v1.0.0-alpha-76 - 2016-01-14

[Release](https://github.com/getscissorhands/Scissorhands.NET/releases/tag/Scissorhands.NET-v1.0.0-alpha-76) | [History from the original root](https://github.com/getscissorhands/Scissorhands.NET/commits/Scissorhands.NET-v1.0.0-alpha-76)

- Began with the repository's first commit on 2015-08-08, adding the README, MIT license, and Git ignore rules.
- Developed Markdown/YAML processing, file and HTTP helpers, Razor rendering, post editing, themed HTML generation, and publishing services.
- Added the Polar Bear theme, theme configuration/loading, Autofac composition, and tests for helpers, services, controllers, and themes.
- Packaged `Scissorhands.Core`, `Scissorhands.Helpers`, `Scissorhands.Themes`, `Scissorhands.Services`, and the web application, renamed to `Scissorhands.NET`.
- Added AppVeyor CI, versioning/release scripts, and development/setup guidance; removed the early console application before this tag.

## Development and migration

Consult the
[vNext documentation handoff](https://github.com/getscissorhands/Scissorhands.NET/blob/vnext/docs/website-documentation.md)
and [migration guide](https://github.com/getscissorhands/Scissorhands.NET/blob/vnext/docs/website-documentation.md#upgrading-to-vnext)
for current behavior and upgrade instructions. Branch documentation can describe
changes not yet in a published package.

Only actual Git tags are release headings above. Draft release names without
corresponding tags and version strings mentioned only in commit messages are not
treated as additional releases.
