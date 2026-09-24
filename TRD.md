# ScissorHands.NET - Technical requirements document

## Current scope amendment: directory locales and fallback

[PRD's #104 amendment](PRD.md#current-scope-amendment-directory-locales-and-fallback) supersedes the locale-specific obligations of TR-023 below, including prefixed primary routes, discovered frontmatter locales, and root/tag redirects. Preserve the existing IDs, approval records, historical baselines, and explicit wider gaps.

Validate configured locale normalization/format independently from ordinary folders. Reject removed frontmatter, duplicate primary-locale source directories, unsafe paths, conflicting pair slugs, and missing/mismatched written dates in authored post pairs. Read drafts for pair validation but publish only eligible primary documents and their ready translations; synthesize primary fallback in each configured additional scope as needed. Primary-gated withdrawal must remove generated files during in-place regeneration, not merely stop rendering them.

Preserve package dependency direction and plugin-stage order. Extend rendering context additively with actual content language, fallback state/message, canonical URL and real-translation alternatives. The Theme package provides abstract localization component bases; each theme owns its markup. Custom banners must render the base's encoded message fragment and required marker/language attributes; inheritance alone is not delivery, and post-HTML hooks must preserve the notice. Maintain stable primary-source navigation ordering for mixed translated/fallback documents. Enforce output ownership and link/path validation for persisted cleanup, and retain root/subpath preview/build parity.

The subsequent switcher/link scope prepares a separate destination inventory including fallback documents; it must not weaken SEO's actual-translation-only alternatives. Theme-owned native labels, region disambiguation and overrides consume that inventory without reconstructing routes. Localize only known page/post links after post-Markdown processing, preserving explicit locale choices, Markdown opt-out, shared resources, external URLs, query/fragment data and site mount boundaries. Real preview and browser tests must verify no-JavaScript switching on documents and generated pages. The [current detailed reference](docs/website-documentation.md#locale-specific-sites) defines compatibility and migration. This amendment does not broaden old sign-offs or mark historical V-009 evidence as validation of the new contract.

## Document control

| Field | Value |
| --- | --- |
| Document version | 0.9 |
| Status | Review-ready |
| Last updated / PRD consulted | 2026-09-16 |
| PRD baseline | [PRD.md](PRD.md) v0.13, Review-ready with locale generation and merged #89 mounting; prior approvals/evidence retained |
| Release scope | Retained baseline/#81, implemented TR-023 and merged TR-017 preview fix; combined V-009 verification remains distinct from historical results |
| Code baseline | `de526bd` merged with `da895fd` (#101/#89); historical execution baselines retained |
| Product owner | @justinyoo, as recorded in the PRD |
| Execution owners | Not assigned |
| Intended audience | Engine, theme, and plugin contributors translating the PRD into implementation and verification obligations |
| Sign-off | @justinyoo approved v0.7 for the locale scope only on 2026-09-16; historical v0.2/v0.4 approvals and v0.6 component criteria retained |
| Approval scope | TR-023, its application of shared contracts and V-009 acceptance/evidence requirements under FR-012. No approval of unrelated revisions, waiver of existing gaps, implementation authorization or release sign-off |
| #81 scoped approval | @justinyoo approved TR-021/TR-022, including the confirmed TG-006 compatibility policy and V-008 evidence requirements, on 2026-09-13; [approval record](https://github.com/getscissorhands/Scissorhands.NET/issues/81#issuecomment-5653569790) |
| Feature decision | @justinyoo confirmed #81's replacement scope and two-tier compatibility policy on 2026-09-13; TR-021/022 elaborate those decisions without claiming delivered behavior |
| Locale scoped approval | Explicit user approval of PRD/TRD/TDD only for this scope on 2026-09-16; reviewed document revision `94fc60b`. This entry records approval without changing requirements or document versions |
| Implementation authorization | @justinyoo separately requested implementation within the agreed scope on 2026-09-16; recorded code/verification does not authorize broader work or release |

**Baseline rule:** the PRD owns product scope and acceptance. This TRD elaborates technical obligations without modifying the PRD. V-001 through V-007 remain open broad programs; V-008 records scoped execution and remaining #81 acceptance. Only the explicitly recorded runs establish evidence, not document generation or approval.

**Readiness:** v0.9 remains Review-ready overall against PRD v0.13. Integrated TR-023/TR-017 behavior passes scoped V-009 acceptance, including actual prefix preview; prior approvals and V-003/V-008 history remain intact. These results do not close wider programs or authorize release.

## 1. Purpose and source relationship

The system converts local Markdown and configuration into static site artifacts using compiled Razor themes and optional plugins. This TRD supports PRD FR-001 through FR-012 and NFR-001 through NFR-010. TR-023 qualifies retained root/tag/navigation contracts only for the new locale-enabled behavior; disabled-mode compatibility and the implemented locale behavior remain distinct from outstanding acceptance evidence.

[Original discussion (archived)](https://github.com/getscissorhands/Scissorhands.NET/blob/464ce0f3454d473d4a39bc6f5c9005e86cd5396a/DISCUSSIONS.md) establishes historical intent. Its assistant-proposed renderer snippets, route crawling, general asset copying, feeds, search, and example plugins are not additional requirements. The current PRD governs those scope distinctions.

Source and guides describe the implementation; [TDD.md](TDD.md) v0.10 records the combined mechanisms. PRD V-003 retains #89's scoped HTTP history, V-008 retains #81 results, and V-009 owns integrated locale acceptance. Earlier failures and wider conformance limits remain explicit.

## 2. Technical scope and boundaries

| Boundary | Responsibility and handoff |
| --- | --- |
| Site owner to generator | Owner runs a local .NET application, supplies site content/configuration, and deliberately installs trusted executable themes/plugins. Content-derived strings remain data requiring validation |
| Configuration/content to engine | Engine reads the configured site and plugin manifests, Markdown/frontmatter, and theme files, then validates and transforms them |
| Engine to extension code | Theme/plugin contracts receive content, manifests, and site context. Extensions execute with the host process's privileges; no isolation, permission broker, automatic download, or sandbox is promised |
| Engine to filesystem | Engine writes generated pages and supported assets into the intended build/preview destination. Containment and collision obligations apply to reads, writes, and copying |
| Preview to browser | Preview serves generated static files and regenerates after filesystem changes; the browser must be refreshed manually |
| Artifact to production host | Owner deploys the static artifact separately. Directory-index behavior, mounting under a prefix, and HTTP not-found routing are host responsibilities; generated URLs must still honor the configured base path |
| Generated HTML to visitor | Metadata encoding and URL handling protect output boundaries. Explicit raw Markdown/plugin HTML and optional JavaScript are not universally sanitized or disabled |

In scope are Core contracts/models, Plugin and Theme extension contracts, the Web engine/default theme, the integration sample, and the existing compatibility/testing obligations. A persistent database, account service, editorial workflow, contact-form backend, distributed build service, or hosted deployment control plane is not introduced.

The PRD's exclusions remain: no new draft-preview mode, scheduler, automatic browser reload, incremental guarantee, atomic output swap, last-good-artifact preservation, bounded CLI shutdown, offline guarantee, or concrete deferred plugins/feeds/search.

FR-011 adds source-filename ordering and automatic adjacent-page links while retaining slug-based grouping, source/URL rules, and navigation eligibility. `section`, `pages.json`, and authored `prev`/`next` objects are superseded requirements, not additional inputs to implement. Post-date and tag-page title ordering are unchanged.

FR-012 adds generated locale collections and the default-locale root redirect under `UseLocaleInUrl`. Q-006 through Q-012 confirm default/content-discovered locales, organizational folders, strict locale browsing, conditional legacy redirects and shared-404 validation. Translation catalogs/text, a switcher and localized 404 variants are excluded.

### Current behavior versus required evidence

| Area | Source-backed current behavior | Remaining obligation |
| --- | --- | --- |
| Route integrity | Generator preflights planned page routes and rejects relative path segments and collisions | Establish containment across filesystem links, asset ownership, and any plugin-transformed output routes in V-001; preflight alone is not complete evidence |
| Navigation and URLs | Engine supplies a per-generation navigation hierarchy; shared helpers format content/theme/image/tag/locale references; nested page indexes infer directory routes | Include hidden/missing ancestors, layout delivery, encoded links, route migration, and client interaction in V-003/V-005/V-007; neither formatting nor markup tests establish scheme safety or preview prefix mounting |
| #81 reading sequence | Implemented two-tier order, retained slug grouping and immutable page context | Scoped V-008 acceptance passed, including ratios and three-engine checks. Preview mounting remains #89/V-003 work |
| Locale collections | TR-023 generation/context/navigation and root/legacy redirects integrate with TR-017 prefix mounting and canonical base paths; disabled-mode generation remains shared | V-009 passes 678 .NET tests, 60 browser cases, slash-variant artifact parity and actual prefixed preview/browser/regeneration checks |
| Output lifecycle | Application clears initial output; generator writes files directly; preview rebuilds reuse the output directory | Verify reported failures and documented partial/stale output behavior, not an unpromised atomicity guarantee |
| Cancellation | Services expose tokens; renderer checks before rendering; production CLI supplies `CancellationToken.None` | Preserve contract-level cancellation without asserting interruptibility or a CLI cancellation deadline |
| Theme/client quality | Default theme has responsive styling, labelled controls, and a persisted light/dark preference | Evaluate the agreed accessibility/browser matrix in V-005 rather than infer conformance from source affordances |
| Performance | Pipeline and watcher behavior can be inspected | Record actual-blog measurements in V-006; no numerical performance gate exists |

## 3. Constraints, interfaces, and coverage

### Existing constraints

| ID | Constraint / source | State and technical impact |
| --- | --- | --- |
| CD-001 | [global.json](global.json), [Directory.Build.props](Directory.Build.props) | Confirmed: .NET 10 target, SDK 10.0.100 with `latestFeature` roll-forward and no prerelease SDKs; repository-selected language version, nullable references, and warnings-as-errors. The historical C# 14 discussion does not override compiler configuration |
| CD-002 | [AGENTS.md](AGENTS.md), package guides | Confirmed: dependencies point inward; Plugin and Theme depend on Core, and Web consumes all three. Existing abstractions/DI and public compatibility must be preserved; no reverse dependency or parallel engine is required |
| CD-003 | [CurrentDirectoryAppPaths](src/ScissorHands.Web/Infrastructure/CurrentDirectoryAppPaths.cs), [IAppPaths](src/ScissorHands.Web/Abstractions/IAppPaths.cs) | Confirmed baseline: default roots resolve from the site working directory. CLI output names are `dist` and `preview`; the generator interface accepts a destination. Application path configuration is not content authorization |
| CD-004 | [Core guide](src/ScissorHands.Core/README.md), [Plugin guide](src/ScissorHands.Plugin/README.md), [Theme guide](src/ScissorHands.Theme/README.md), [migration reference](docs/website-documentation.md#upgrading-to-vnext) | Current contracts: required plugin IDs, read-only collections, cancellation-aware theme services, seven required theme roles, directory-index route changes, and retained navigation/URL compatibility. Old name-only plugins and themes relying on implicit tag views need migration |
| CD-005 | [Markdown service](src/ScissorHands.Web/Services/MarkdownService.cs), [content loader](src/ScissorHands.Web/Loaders/ContentLoader.cs) | Current implementation evidence: Markdig with advanced extensions/SmartyPants and YamlDotNet parsing. No library replacement, version upgrade, or new Markdown dialect is proposed |
| CD-006 | PRD NFR-008/NFR-009 and Section 7 | Confirmed: stated desktop/mobile browsers and actual-blog benchmarking; no formal WCAG claim, performance SLA, numerical speed threshold, or maximum supported site size |
| CD-007 | [AGENTS.md](AGENTS.md), [CI](.github/workflows/main.yaml), [sample guide](sample/README.md) | Confirmed repository verification conventions: Microsoft.Testing.Platform, existing xUnit/Shouldly/NSubstitute/bUnit tooling, aligned build/test configurations, and Windows/macOS/Linux CI. Execution is deferred, not presumed successful |

Execution owners for the constraints and verification activities are unassigned. Existing source versions are the code baseline above; no external standards assessment or package-catalog research was performed.

### Existing interface boundaries

The linked declarations are authoritative for signatures; these summaries describe obligations, not replacements for API reference documentation.

| Surface | Contract and technical meaning |
| --- | --- |
| [Application builder](src/ScissorHands.Web/ScissorHandsApplicationBuilder.cs) / [application](src/ScissorHands.Web/ScissorHandsApplication.cs) | `Build()` produces the runnable application; `RunAsync()` selects the command mode. Generic and `Type`-based `AddLayouts` overrides accept seven theme component roles and validate assignability |
| [IContentLoader](src/ScissorHands.Web/Loaders/IContentLoader.cs) | `LoadAsync(CancellationToken)` returns `Task<IEnumerable<ContentDocument>>`; missing optional directories are distinct from invalid source data |
| [IMarkdownService](src/ScissorHands.Core/Services/IMarkdownService.cs) | `ToHtmlAsync(string, bool?, CancellationToken)` returns HTML. Existing `trim` behavior removes an enclosing paragraph only for a single paragraph, not arbitrary block markup |
| [IThemeService](src/ScissorHands.Core/Services/IThemeService.cs) | Manifest loading and asset copying have cancellation-aware overloads and retained obsolete overloads. Default interface adapters check cancellation before delegating to legacy implementations |
| [Plugin contracts](src/ScissorHands.Plugin/README.md) | Hooks operate before Markdown, after Markdown, and after full HTML rendering. IDs identify configuration/components/dependencies; optional dependencies are stage-scoped |
| [IComponentRenderer](src/ScissorHands.Web/Renderers/IComponentRenderer.cs) | `RenderAsync<TComponent>` takes layout type, parameter dictionary, and optional token, returning `Task<string>` HTML |
| [IStaticSiteGenerator](src/ScissorHands.Web/Generators/IStaticSiteGenerator.cs) | `BuildAsync` accepts seven constrained theme component types, destination, preview flag, and cancellation token; successful completion represents completion of that generation call, not deployment |
| [Theme contracts](src/ScissorHands.Theme/README.md) | All seven layout/view roles are required. `MainLayoutBase.NavigationPages` and `NavigationTree` default to empty read-only lists and are layout-only, not view attributes or automatic cascades. Existing document/collection context is unchanged; raw HTML remains an explicit rendering boundary |
| [NavigationNode](src/ScissorHands.Core/Models/NavigationNode.cs) / [navigation reference](docs/website-documentation.md#prepared-navigation) | Immutable node with text `Title`, escaped base-relative `Path`, nullable `Url`, and snapshotted read-only `Children`; null URL means a non-clickable group. Flat navigation contains only actual eligible pages |
| [ContentUrlHelper](src/ScissorHands.Core/Urls/ContentUrlHelper.cs) / [Theme wrappers](docs/website-documentation.md#url-helpers) | Shared static formatting and protected view/layout helpers retain distinct content, theme, image, tag, and locale contracts; formatting is not general URI sanitization |

### Coverage disposition

Applicability is separate from whether implementation evidence exists.

| Area | Applicability and basis | Technical coverage |
| --- | --- | --- |
| System behavior and boundaries | Applicable: local generator, renderer, extensions, preview, static artifact, planned reading sequence | TR-001 through TR-010, TR-012/021/022 |
| Identity and permissions | Applicable to local filesystem/process access and explicit plugin enablement; no accounts/tenants | TR-009, TR-013, TR-015 |
| Data and integrity | Applicable: frontmatter, content models, source identity, routes, collections, adjacency snapshots, and assets | TR-002/003/006/008/011/013/021/022 |
| Data lifecycle and privacy | Applicable: source files, generated artifacts, logs, browser theme preference | TR-012/015/018/020 |
| Interfaces and integrations | Applicable: .NET APIs, configuration/files, extensions, additive page context, browser and static host | Interface table; TR-004/005/007/009/010/011/017/022; TG-006 |
| Security and abuse | Applicable despite local authoring: untrusted data and executable/raw-content boundaries | TR-013/014/015 |
| Compliance and policy | Repository policy applies; no additional regulatory certification or regulated workflow is specified | CD-002/004/007; TR-013/014/015 |
| Performance and capacity | Applicable: actual-blog generation/preview/resource measurements | TR-019 |
| Reliability and recovery | Applicable: direct writes, rebuilds, failures/cancellation; no hosted availability or atomicity target | TR-012/016/020 |
| Operations and observability | Applicable: mode selection, errors, output location, rebuild status, and evidence records | TR-001/012/019/020 |
| Migration and rollout | API/configuration migration applies; managed deployment and automatic rollback are outside scope | TR-011; PRD release gates |
| Compatibility and clients | Applicable: .NET contracts, CI platforms, generated-site browsers/subpaths | TR-001/011/017/018 |
| Accessibility and localization | Applicable: default-theme accessibility and adjacent links, effective locale, URL prefixes, dates, UTF-8 HTML; translation management excluded | TR-002/003/005/018/022 |
| AI and automation | AI/model decisions not applicable; deterministic watcher/plugin automation is in scope | No AI obligations; TR-004/010/012 |
| Domain-specific constraints | Static-file deployment applies; payment, hardware safety, offline synchronization, and transactional domains are not part of the PRD | TR-005/008/017; no additional domain services |

## 4. Technical requirements

Each `TR-...` is stable; **must** expresses obligation, not complete verification. v0.9 retains all 23 IDs against PRD v0.13, integrating TR-017 preview mounting with TR-023 locale generation without changing TR-022's approved criteria. Historical approvals and evidence ownership remain intact.

### TR-001: Execute the selected application mode

- **State / source:** Confirmed; PRD FR-001, NFR-007; CD-001/003 and application/builder contracts.
- **Rationale / obligation:** To support local generation without a production server, the application must preserve explicit build, preview, and help entry points using the repository's .NET configuration.
- **Acceptance:** Build produces output under `dist` and reports its location without starting the preview listener. Preview uses `preview` and starts serving. Help displays usage. An invocation with no recognized mode reports an error and sets exit code 1; preview wins if both build and preview are supplied.
- **Boundaries:** The sample launch profile does not select a mode; callers supply it explicitly. This does not introduce strict rejection of every unrecognized extra argument or a separate standalone CLI package.
- **Verification:** V-007 application/argument tests and sample process demonstrations; V-004 distinguishes failure from successful completion.

### TR-002: Parse content into the existing logical model

- **State / source:** Confirmed; PRD FR-002; [ContentDocument](src/ScissorHands.Core/Models/ContentDocument.cs), [ContentMetadata](src/ScissorHands.Core/Models/ContentMetadata.cs), loader.
- **Rationale / obligation:** To separate content from presentation, loading must preserve source identity, post/page kind, supported metadata, source Markdown, and generated HTML through the pipeline.
- **Acceptance:** Recursively load `.md` under `contents/posts` and `contents/pages`. Optional frontmatter maps the PRD field set into the model; `published` becomes nullable `DateTimeOffset`, and `draft`/`show_in_navigation` map to booleans defaulting to false (`Draft`/`ShowInNavigation`). Tags accept a YAML list or comma-separated string. Derive missing title/slug from filename/relative path under TR-003; effective locale falls back to site locale.
- **Feature relationship:** TR-021 owns #81's use of source identity; this loader's supported authoring schema is unchanged.
- **Locale extension:** TR-023/confirmed PRD Q-008 retain organizational folders, frontmatter/site fallback and relative-slug inference. Folder-locale inference and folder-prefix stripping are explicitly not introduced.
- **Failure / publication:** Missing content directories warn; malformed YAML, unsupported fields, unclosed frontmatter, and invalid date/boolean/tag values fail with source/field context. Exclude drafts in both modes; do not withhold future-dated content. `ShowInNavigation` changes membership under TR-006, not generation or authorization; posts may parse the flag but never join page navigation.
- **Verification:** V-007 loader cases covering both kinds, missing metadata, explicit/invalid navigation booleans, effective locale, and publication modes; V-004 diagnostics.

### TR-003: Resolve routes without conflicting page ownership

- **State / source:** Confirmed; PRD FR-003/004, NFR-002; loader and [generator](src/ScissorHands.Web/Generators/StaticSiteGenerator.cs).
- **Rationale / obligation:** To preserve predictable addresses, routing must produce the PRD's content/index/tag/404 destinations and reject conflicting page ownership.
- **Acceptance:** Ordinary routes write `<route>/index.html`; the site index owns root `index.html`, and the not-found page owns `404.html`. Date prefixes apply only to dated posts; enabled locale prefixes use the effective normalized locale without duplicating an existing locale prefix, as required by the PRD. Preserve the PRD's `ko-kr/2026/09/11/hello` example and the 404 exception. Missing dates warn rather than schedule or fail a post.
- **Directory-index rules:** Omitted/blank slugs on nested page `index.md` files infer the containing directory, comparing the filename case-insensitively. Explicit non-blank slugs win. Root page `index.md` retains `index`; posts are unchanged. Apply locale rules after inference. `parent.md` and `parent/index.md` cannot both claim `parent`; explicit `slug: parent/index` preserves the older nested-index route.
- **Integrity:** Reject literal `.`/`..` segments, case-insensitive output collisions, and file-versus-parent-directory conflicts. Source route validation occurs before page writes, not necessarily before initial output cleanup. Final route/asset safety is also TR-013; plugin transformations must not bypass it.
- **Verification:** V-007 directory-index/explicit-slug/date/locale/404 cases and V-001 collision/containment cases, including inferred-directory conflicts, generated routes, and transformation boundaries.
- **Locale extension:** TR-023 replaces root homepage ownership with a redirect and adds generated locale-home/tag owners when enabled. Existing source/generated collisions remain errors; this qualification does not change current runtime behavior until implemented.

### TR-004: Propagate transformations through the fixed pipeline

- **State / source:** Confirmed; PRD FR-007/008; generator, Markdown service, and Plugin guide.
- **Rationale / obligation:** To compose extensions predictably, the engine must propagate each returned result through pre-Markdown, Markdown conversion, post-Markdown, Razor rendering, and post-HTML in that order.
- **Acceptance:** Pre-Markdown output supplies conversion input; conversion populates `ContentDocument.Html`; post-Markdown output reaches the view; post-HTML output is written. A plugin returning a replacement document is not ignored. Disabled plugins do not participate.
- **Boundaries:** Source-backed custom 404 content receives Markdown stages; synthetic index/tag/default-404 documents have no source Markdown stages but receive post-HTML. `SiteManifest.IsPreview` is set before hooks. Ordering within a stage follows TR-010.
- **Verification:** V-007 distinguishable transforms across stages, replacement-document cases, disabled plugins, synthetic/source-backed pages, and both modes.

### TR-005: Render complete static documents through Razor contracts

- **State / source:** Confirmed shared requirement; PRD FR-001/007/010; [ComponentRenderer](src/ScissorHands.Web/Renderers/ComponentRenderer.cs) and Theme guide.
- **Rationale / obligation:** To produce ordinary static pages, rendering must compose the selected layout/view with the required parameters and cascading context, then return complete HTML for post-processing and UTF-8 output.
- **Acceptance:** Layout and view can access their applicable document/collection, plugins, theme, and site context without mistaking cascading-only values for normal view parameters. Deliver `NavigationPages`/`NavigationTree` to the layout only; neither is an automatic cascading value. Normal generation supplies both; for a `MainLayoutBase` layout, an older renderer caller supplying only `IReadOnlyList<ContentDocument>` navigation pages gets a prepared tree. Preserve an explicitly supplied tree. Await rendering before post-HTML processing and UTF-8 output; no Blazor circuit is needed to serve it.
- **Feature relationship:** TR-022 owns #81's additive page context and V-008 its acceptance evidence; this shared rendering contract continues to apply.
- **Boundaries:** The current implementation uses `HtmlRenderer`; optional theme/plugin JavaScript is allowed. Do not import lifecycle assertions or route-enumeration suggestions from the historical discussion as new contract guarantees.
- **Verification:** V-007 renderer/context cases including navigation filtering, flat-only compatibility, explicit-tree preservation, and generated-file inspection; V-003 artifact serving without the application runtime.

### TR-006: Construct collection, navigation, and not-found data

- **State / source:** Confirmed retained behavior; PRD FR-004/010; generator, [navigation builder](src/ScissorHands.Web/Navigation/NavigationTreeBuilder.cs), and Theme guide.
- **Rationale / obligation:** The engine must supply index/tag/404 data and consistent page navigation while leaving markup and interaction to themes.
- **Acceptance:** Index posts are ordered by descending publication date with undated posts last. Always generate index/404. A page with `slug: 404.html` uses the not-found view and is excluded from normal page rendering and displayed tag groups. Group eligible tags by lowercased name, then emit `tags` and encoded per-tag routes, with tagged posts newest first and pages ordered by title.
- **Empty / failure:** No eligible tagged documents means no tag pages. An empty content set still renders index/404. Output conflicts remain errors under TR-003; generating a 404 file does not configure a host's HTTP status behavior.
- **Navigation selection:** From loaded pages, select opted-in, non-draft, non-404 documents. A loaded hidden page suppresses descendants using ordinal, whole-route-segment matching. Drafts removed by the loader do not participate in selection; navigation is not an alternative publication filter.
- **Hierarchy:** Derive `NavigationTree` from resolved routes with non-clickable groups for missing ancestors, retaining only groups with visible descendants. Group titles decode the path segment, replace hyphens/underscores with spaces, and apply invariant title casing. Suppress synthesized locale-prefix groups when locale routing is enabled, but retain actual visible locale landing pages. Groups create no documents, files, or placeholder links.
- **Lifecycle / compatibility:** Every generated layout receives the same per-generation flat list and immutable tree. Preserve empty defaults and defensively copied node children. The current snapshot is taken before document hooks; plugin-returned title/slug/visibility changes do not refresh it. Broader post-plugin collection consistency remains TDD DQ-006, not a promise of two-pass generation.
- **Feature relationship:** TR-021 owns #81's reading sequence and replacement of title-first navigation ordering; TR-022 owns adjacency. V-008 records those additions and preservation of these shared collection/grouping rules, not completion of all collection work.
- **Verification:** V-007 empty/mixed/404/tag-order cases, hidden/missing ancestors, whole-segment matching, locale groups, immutability, all-layout delivery and snapshot boundaries; V-001 route conflicts and V-003 real links. Feature ordering evidence is in V-008.
- **Locale extension:** Shared tag destinations and the same navigation on every layout are the current/disabled-mode contract. TR-023/confirmed Q-009 require per-locale data and snapshots when enabled, with conditional default-locale legacy redirects rather than shared tag listings.

### TR-007: Resolve the configured theme and manifest

- **State / source:** Confirmed; PRD FR-005/006; Theme guide, resolver/service evidence, builder overrides.
- **Rationale / obligation:** To make themes replaceable, the engine must resolve a compatible component set and manifest for the configured theme without rewriting content or requiring explicit registration in the conventional case.
- **Acceptance:** Match the normalized namespace suffix to `Site:Theme`; require exactly one concrete component for each layout/index/post/page/not-found/tag-list/tag role, with no implicit built-in tag-view substitution. Explicit seven-role `AddLayouts` overrides remain usable. Load `themes/<slug>/theme.json`; invalid/mismatched custom manifests or unresolved required custom components fail. Preserve built-in theme selection for empty/default theme settings.
- **Boundaries:** Compiled Razor changes require compilation; theme selection changes require restart/rebuild. No install UI, arbitrary untrusted extension loader, or name-based plugin fallback is introduced.
- **Verification:** V-007 conventional/override/custom/default theme cases, missing or ambiguous required tag roles, invalid manifests, and configuration changes.

### TR-008: Copy only the supported asset surfaces

- **State / source:** Confirmed; PRD FR-006; generator and Theme service/guide.
- **Rationale / obligation:** To make generated output usable on a static host, copying must retain relative structure and the baseline's supported content/theme assets.
- **Acceptance:** Copy `contents/images` recursively to output `images`; copy theme `assets` content and the theme service's supported image/manifest files below `themes/<slug>`. Theme stylesheet/script entries remain available for inclusion by the layout. Retain bundled default-theme assets.
- **Boundaries:** Missing optional content images warn; missing custom manifests fail under TR-007, while missing theme asset folders can warn. Arbitrary application `wwwroot`, automatic plugin asset export, and downloading external references are not guaranteed. All copies are subject to TR-013.
- **Verification:** V-007 fixture inventories and relative destination paths; V-001 collision/link containment and V-003 asset requests.

### TR-009: Select enabled plugins by validated stable ID

- **State / source:** Confirmed; PRD FR-008, NFR-001; [PluginIdValidator](src/ScissorHands.Core/Validation/PluginIdValidator.cs), Plugin guide.
- **Rationale / obligation:** To make extension configuration unambiguous, plugin implementation, manifest, dependency, and Razor selector identity must use the existing validated ID contract.
- **Acceptance:** IDs match `\A[a-z0-9]+(?:-[a-z0-9]+)*\z` and are compared ordinally without trimming, normalization, or name fallback. Invalid/duplicate IDs and configured-but-uninstalled plugins fail. Installed plugins without manifests stay disabled; no universal `Options.Enabled` switch replaces manifest presence.
- **Boundaries:** Implementation `Name` remains non-empty display metadata; manifest names are optional and need not be unique. A valid component selector with no manifest resolves `Plugin` to null; the component owns suppression of its disabled output. Configuration must not install/enable dependencies implicitly.
- **Verification:** V-007 ID matrices, duplicate configuration, missing implementation/manifest, display-name independence, and component selection.

### TR-010: Resolve execution dependencies independently per stage

- **State / source:** Confirmed; PRD FR-007/008; Plugin dependency guide and resolver evidence.
- **Rationale / obligation:** To avoid incidental discovery order, enabled plugin hooks must execute in stage-scoped dependency order with ordinal ID tie-breaking.
- **Acceptance:** Every declared prerequisite must be installed and enabled; transitive prerequisites run before dependents in the declared stage. Among ready plugins, select ordinal ID order. Manifest/registration reordering does not change that order.
- **Failure / limits:** Invalid declarations, stages/IDs, self-dependencies, duplicate declarations within a stage, and cycles fail before hooks run. Disabled plugins' declarations are ignored. Cross-stage dependencies are not expressed; no automatic installation, parallel-hook execution, or exactly-once side-effect promise is added.
- **Verification:** V-007 dependency graph permutations, disabled/missing prerequisites, invalid declarations, cycles, and stage separation.

### TR-011: Preserve the documented data and public API compatibility boundary

- **State / source:** Confirmed; PRD NFR-007, FR-005/008; Core/Plugin/Theme guides and public declarations.
- **Rationale / obligation:** To support existing integrations within the stated vNext migration, implementation must preserve current contracts except for already documented breaking changes.
- **Acceptance:** Theme `Stylesheets`/`Scripts` remain non-null defensively copied `IReadOnlyList<string>`; nullable plugin `Options` remains a defensively copied `IReadOnlyDictionary<string, object?>`; `ContentMetadata.Tags` and `NavigationNode.Children` snapshot input. Mutation of a supplied collection after initialization does not change its snapshot. Retain flat navigation, empty defaults, and existing protected URL-helper signatures/exception behavior.
- **Compatibility limits:** Do not promise deep immutability of arbitrary option values or of all `SiteManifest`/document state. Required implementation IDs, manifest IDs, component selectors, and `PluginDependency.PluginId` retain the documented migration. Obsolete theme-service overloads remain callable with their cancellation-aware adapters; consuming assemblies/configuration must be rebuilt/updated as documented.
- **Migration:** Themes previously relying on tag-view fallback must supply both tag roles and rebuild; explicit `AddLayouts` still takes seven roles. Nested page indexes may move from `parent/index` to `parent`; set that explicit slug to retain the old URL and resolve new collisions. These are documented behavior changes, unlike additive navigation models and shared helpers. Sample callers now choose build/preview explicitly.
- **Feature relationship:** TR-021 owns #81's source-less policy and ordering migration; TR-022 owns additive theme-context compatibility. V-008 records their evidence without completing this broader API/package migration requirement.
- **Verification:** V-007 snapshots, flat-navigation and helper compatibility, migrated consumers, required tag roles, directory-index overrides, explicit layout registration, and legacy theme-service adapters.

### TR-012: Serialize preview regeneration within the watcher

- **State / source:** Confirmed; PRD FR-009; application and [ContentWatcher](src/ScissorHands.Web/Watchers/ContentWatcher.cs).
- **Rationale / obligation:** To support the local editing loop, preview must build initial output, observe recursive content/theme changes, and serialize coalesced regeneration callbacks.
- **Acceptance:** Create/change/delete/rename events can trigger rebuilds. Within one watcher, callbacks do not overlap. Successful rebuilds report completion and manual refresh guidance; failed callbacks log failure and do not permanently prevent later events from being processed. Initial generation errors propagate.
- **Lifecycle limits:** Razor/C# changes require recompilation. Output can be partial or stale after failures/deletions; restart/rebuild recovery follows TR-020. The current 500 ms debounce is implementation evidence, not a latency SLA. This contract does not establish cross-process locking or exactly-once processing.
- **Verification:** V-004 sequential event bursts, callback failure/recovery, shutdown/disposal, and initial failure; V-007 sample edit/refresh behavior.

### TR-013: Enforce intended filesystem roots and output ownership

- **State / source:** Confirmed obligation; PRD NFR-002, FR-003/006 and AGENTS.md. Complete implementation evidence is pending.
- **Rationale / obligation:** To prevent content-derived access or overwrites outside their scope, every content read, generated write, and supported asset copy must remain within its intended root and respect output ownership.
- **Acceptance:** Reject traversal and filesystem-link escapes for inputs and destinations; reject page/asset or file/directory ownership conflicts rather than silently replacing unrelated output. Apply the obligation to paths after plugin transformations as well as original metadata. Diagnostics must identify the relevant source/route/asset without dumping secrets.
- **Boundary:** Preserve intentional cleanup of the configured generated-output directory and regeneration of its own files; this is not permission for content to choose other roots. Existing string-based route preflight is partial evidence, not proof of full containment.
- **Verification:** V-001 filesystem fixtures for reads/writes/copies, links and collisions at original/final paths; V-004 explicit failure. Hardening design and reproduction of coverage gaps remain next-phase work.

### TR-014: Preserve encoding and validate URL-bearing output boundaries

- **State / source:** Confirmed obligation; PRD NFR-003, FR-010 and AGENTS.md; Theme/Markdown service evidence.
- **Rationale / obligation:** To prevent metadata from becoming unintended executable output, rendering must preserve metadata encoding and reject unsafe URL schemes at applicable URL-bearing boundaries while retaining supported raw-content semantics.
- **Acceptance:** Metadata intended as text does not become injected elements/attributes. Unsafe executable URL inputs are rejected at the affected boundary; ordinary supported internal/external URLs remain compatible. Explicit Markdown/plugin HTML is not silently subjected to blanket sanitization or treated as guaranteed safe.
- **Current boundary / unresolved detail:** Content/tag helpers now escape route text and reject literal relative segments or invalid tag names; navigation titles retain Razor encoding. Theme/image helpers preserve their narrower formatting semantics and are not sanitizers. The complete context-specific scheme matrix and raw-versus-metadata sink inventory remain TG-001; do not infer universal URL safety or introduce an allowlist from these formatting helpers.
- **Verification:** V-002 synthetic text/attribute/URL payloads and compatible-content cases; correlate each outcome to an identified rendering context. TG-001 limits implementation readiness for new URL validation rules.

### TR-015: Respect the local trust model without exposing secrets

- **State / source:** Confirmed; PRD NFR-001/010 and AGENTS.md.
- **Rationale / obligation:** To preserve author and visitor privacy, the engine must treat metadata/content as data, avoid automatically acquiring executable extensions from content, and avoid accidental disclosure of local secrets in HTML, logs, or fixtures.
- **Acceptance:** Installed/configured extension selection follows TR-007/009; a content value is not an instruction to execute commands or install code. Synthetic secret markers outside intended inputs are not incorporated through file escapes or error dumps. Tests use synthetic values, not real credentials.
- **Boundary:** Trusted extensions run with application privileges, not in a sandbox. Intentional site content remains publishable; the engine is not a universal secret scanner. Remote hero images and optional theme/plugin requests are allowed by the baseline; no analytics integration, consent service, or zero-network claim is added.
- **Verification:** V-001/V-002 input-boundary, diagnostics, and output inspection; V-007 explicit plugin enablement cases.

### TR-016: Preserve cancellation-aware completion semantics

- **State / source:** Confirmed; PRD NFR-006/004; Core service interfaces, renderer/generator contracts, application.
- **Rationale / obligation:** To avoid representing cancelled work as a finished artifact, cancellable operations must preserve supplied cancellation tokens and surface observed cancellation rather than return successful completion.
- **Acceptance:** Pre-cancelled token cases fail at supported boundaries; downstream token-aware calls receive the supplied token. Cancellation during supported asynchronous work propagates instead of producing a completed-build message. Legacy theme-service adapters retain their documented pre-delegation cancellation check.
- **Limits:** Synchronous copying/conversion and third-party rendering/plugin internals do not acquire an invented interruptibility guarantee. Production CLI still supplies `CancellationToken.None`; no bounded graceful Ctrl+C behavior is added. Partial files may remain under TR-020.
- **Verification:** V-004 pre-cancelled/in-progress cases using controlled token-aware dependencies; V-007 legacy interface compatibility.

### TR-017: Resolve generated internal URLs under the configured base path

- **State / source:** Confirmed shared requirement; PRD NFR-005, FR-003/006/010; Site manifest and Theme/Plugin guides.
- **Rationale / obligation:** To support subpath deployment, generated navigation and asset references must resolve under `SiteManifest.BaseUrl` where they address the site's own content.
- **Acceptance:** At root and `/docs/`, requests for generated post/page/tag links, content images, theme CSS/JS, and representative plugin links reach the intended files without escaping to the domain root. Validate rendered requests rather than only the presence of a `<base>` tag.
- **Shared formatting contract:** Reuse [ContentUrlHelper](src/ScissorHands.Core/Urls/ContentUrlHelper.cs) through the [Theme wrappers](docs/website-documentation.md#url-helpers). Content URLs trim outer whitespace, split either slash separator, discard empty segments, escape each segment, and return `.` for an empty route; literal `.`/`..` segments fail. Tag URLs trim/lowercase and escape one segment under `tags/`; empty/dot tags fail. Theme URLs concatenate `themes/<slug>/<path>` with forward-slash trimming only. Image URLs only strip leading forward slashes, preserving HTTP(S), queries, fragments, and existing encoding. Locale segments retain trim/lowercase and underscore/forward-slash-to-hyphen normalization.
- **Errors / base handling:** Content/theme/image/tag null inputs fail; locale null/whitespace maps to empty. The layout theme wrapper checks null path before missing `Theme`, which throws `InvalidOperationException`. Content, tag, and theme URLs remain base-relative without prepending `Site.BaseUrl`; do not escape images as content slugs or treat formatting as scheme validation.
- **Boundary:** Distinguish artifact-relative paths from deployment mount paths. The production host owns mounting, directory indexes, and 404 routing; preview behavior must also be evaluated. Do not rewrite unrelated absolute external URLs into local assets or promise a specific hosting provider.
- **Preview implementation:** [#89](https://github.com/getscissorhands/Scissorhands.NET/issues/89) implements TDD DEC-002 with scoped passing V-003 HTTP evidence. Preview serves the existing artifact beneath `/` or slash-delimited path prefixes such as `/docs/` and `/manual/docs/`, with or without locale routes. Directory redirects retain the prefix and query; missing files remain 404. No additional physical prefix directory or URL-helper prefix is introduced.
- **Preview mount boundary:** Per the user's 2026-09-16 clarifications under PRD FR-009, a non-root prefix must exclusively mount preview output, with a root-navigation exception: GET/HEAD `/` returns a temporary 302 redirect to the mounted homepage, preserving the query string and emitting no homepage body at root. Following the redirect reaches the base URL; a query value must not select another destination. Other outside-prefix requests, including unprefixed pages/assets and POST `/`, return 404 without redirecting. Matching uses complete path segments, so `/docs-other/` does not match `/docs/`. A request to the mount without its final slash (`/docs`) redirects to `/docs/`, preserving the query. Configuring `/` retains root hosting without redirecting. This supersedes the initial alias assumption and the later root 404; update preview links that used content aliases. It is not authentication.
- **Base-path normalization:** Per the user's 2026-09-16 request under PRD FR-009/NFR-005, accept leading-slash path prefixes with or without the final slash. The shared site manifest exposes `/docs/` for both `/docs` and `/docs/`, `/manual/docs/` for either nested form, and `/` unchanged. Normalize at initialization, including configuration binding and direct .NET callers, without changing the public init-only API or rewriting configuration sources. Build/preview `<base>` markup, plugin site context, preview mount, logged URL and redirects use this effective value; repeated initialization with it is idempotent.
- **Normalization / preview limits:** Only a missing terminal slash is supplied for single-leading-slash path values without backslashes, queries or fragments; existing slash-terminated values are preserved. Absolute/network-relative URLs and relative paths without a leading slash are not rewritten. No whitespace trimming, decoding, case folding or general URL validation is introduced. Null/empty values retain their prior manifest behavior rather than becoming a new success-shaped default. Other URL-form support and safety decisions remain TG-001; preserving a value does not certify it for preview or rendering.
- **Feature relationship:** TR-022/V-008 retain correct locale/base-relative neighbor URLs, root preview navigation and requests on correctly mounted static output. The user explicitly removed #89 from #81's completion gate; this broader preview-serving obligation remains in force independently.
- **Verification:** V-003 real root/prefix requests including hierarchy, directory indexes, date/locale routes, and extension output; V-007 helper/view parity, encoded text, null/invalid inputs, and preserved image semantics.

### TR-018: Deliver the agreed built-in-theme client behavior

- **State / source:** Confirmed shared requirement; PRD v0.13 FR-010 and NFR-008, retaining the user's broader quality decision.
- **Rationale / obligation:** To make generated pages usable, the built-in theme must support the agreed browser matrix, keyboard navigation, visible focus, meaningful control labels, and readable light/dark contrast.
- **Acceptance:** Evaluate ordinary reading/navigation and the theme control on current stable desktop Edge/Chrome/Firefox/Safari, Android Chrome, and iOS Safari; record actual versions/platforms/devices. With JavaScript and storage available, the light/dark selection persists. Page title/description/locale follow the document/site fallback contract.
- **Navigation / tags:** Render page nodes as anchors and null-URL groups as text, with separate labelled disclosure buttons and accurate expanded state. JavaScript enables mouse/touch/Enter/Space toggling, closes the applicable group on Escape and restores button focus, and dismisses menus outside navigation. Without JavaScript, eligible child links remain visible. Render normalized tag links on both posts and pages.
- **Feature relationship:** TR-022/V-008 use the user-approved #81-only ratios and three-engine desktop/mobile automation. This is sufficient for the feature, not completed real-browser/device coverage or resolution of the broader TG-002 method.
- **Limits / detail:** Custom-theme authors own their accessibility. The broader theme has no new numerical contrast threshold or certification claim; TR-022's explicit thresholds apply only to the pager. No new storage-disabled preference fallback is promised.
- **Verification:** V-005 browser/keyboard/touch/disclosure/focus/contrast and no-JavaScript demonstrations, plus V-007 metadata, navigation markup, and page/post tag links. Source/component cases do not establish browser interaction results.

### TR-019: Produce a reproducible actual-blog performance baseline

- **State / source:** Confirmed; PRD NFR-009 and V-006.
- **Rationale / obligation:** To characterize the supported personal-blog workflow without inventing an SLA, benchmarking must record build duration, preview-update delay, and memory usage on the owner's actual blog.
- **Acceptance:** Identify the content snapshot, post/page counts, assets, enabled theme/plugins, machine/OS/SDK, commands/configuration, measurement boundaries, units, observation runs, and results. Distinguish compilation/startup from generation time when reporting, and identify whether preview timing includes event coalescing.
- **Limits:** There is no required numerical threshold, statistical percentile, run count, maximum site size, or unlimited-scale guarantee. Record the method actually used; unavailable inputs/platforms or failed runs must be explicit rather than substituted with a successful-looking result.
- **Verification:** V-006 benchmark record; the blog snapshot, access/location, environment, and execution owner are next-phase inputs (TG-003), not assumed files available in this workspace.

### TR-020: Report artifact state and failures without a false success

- **State / source:** Confirmed; PRD FR-001/009, NFR-004 and documented output limitations.
- **Rationale / obligation:** To prevent accidental publication of incomplete output, application diagnostics must distinguish successful generation from input/configuration, rendering/plugin, I/O, watcher, and cancellation failures.
- **Acceptance:** Errors identify actionable source/field/plugin/stage/path context where relevant, subject to TR-015. Optional missing content/assets warn as documented; invalid configuration does not become a success-shaped fallback. Successful CLI build reports the output location only after generation completes; watcher success follows a successful callback.
- **Lifecycle:** Initial build/preview output may be cleared and writes are non-atomic. Failure may leave partial/mixed output; deletions/renames may leave stale preview files until restart. No automatic rollback or last-good guarantee is added. Recovery is correction plus a fresh build/restart, with production deployment separate.
- **Verification:** V-004 diagnostics and artifact inspection under each applicable failure category; V-007 successful mode/sample behavior.

### TR-021: Prepare deterministic source-based navigation order

- **State / source:** Approved #81 scope; PRD FR-011/NFR-007 and the [decision](https://github.com/getscissorhands/Scissorhands.NET/issues/81#issuecomment-5653313907). Implemented in the current workspace by `PageReadingOrder`, the navigation builder and generator; V-008 records scoped evidence.
- **Rationale / obligation:** The engine must prepare one reading sequence with eligible file-backed pages ordered by source identity first, followed by eligible source-less pages using the confirmed compatibility fallback.
- **Ordering acceptance:** Within each source directory under the pages root, prioritize `index.md` case-insensitively, then compare the remaining file and directory names together using ordinal ordering. Fully traverse an eligible directory subtree at its sorted position before the next sibling. Preserve filename text, including numeric prefixes; do not substitute natural-number, culture-sensitive, title, or URL sorting.
- **Eligibility and hierarchy:** Apply TR-006's existing eligibility and hidden-route-ancestor rules. Flat `NavigationPages` must follow the combined sequence, and engine-prepared tree sibling ordering must derive from its ranks while retaining slug-based parent/group relationships. A slug tree can differ from the reading sequence; its flattened display order must not redefine adjacency. Groups with no eligible descendants remain absent.
- **Lifecycle / failure:** Prepare the sequence once per generation before document hooks, alongside navigation. Recompute it on each successful preview regeneration; do not retain stale adjacency across generations. Preserve cancellation and contextual input failures. Source-path processing must respect intended roots and must not introduce additional unvalidated filesystem reads.
- **Confirmed compatibility policy:** Under the [TG-006 / DQ-008 decision](https://github.com/getscissorhands/Scissorhands.NET/issues/81#issuecomment-5653457814), append eligible pages with no recorded source path after all file-backed pages, sorting that tier by ordinal `Metadata.Title` then resolved `Metadata.Slug`, matching existing flat ordering. All-source-less inputs use that order throughout. Preserve public signatures, direct/flat-only compatibility, and explicitly supplied `NavigationTree` values. Do not fabricate filenames or apply the fallback to invalid supplied paths; validation failures remain explicit. The fallback is documented compatibility behavior, never a substitute for file-backed ordering.
- **Authoring and migration:** Do not add `section`, authored `prev`/`next`, ordering fields, or a `pages.json` reader. Document the intentional navigation-order change and source-less fallback when implemented. Preserve URL-generation rules; numeric prefixes in inferred slugs are not stripped, and no automatic source renaming is authorized. Demonstrate explicit-slug preservation when authors rename files and record the remaining inferred-URL consequence under PRD RD-005.
- **Verification:** V-008 owns ordering, two-tier compatibility, invalid-input/cancellation, eligibility/grouping preservation, regeneration, and migration acceptance. Precisely scoped results may support V-001/V-004/V-007; passing this feature does not complete those programs.

### TR-022: Supply and render automatic adjacent-page links

- **State / source:** Approved #81 scope, implemented with passing V-008 evidence. Pager styles satisfy the adopted ratios and explicit zero tab indices preserve WebKit keyboard access. Actual preview mounting remains excluded and tracked in #89.
- **Rationale / obligation:** For each page participating in TR-021's sequence, the engine must supply its immediately preceding/following eligible pages as optional generated navigation data. Authors must not maintain adjacency metadata.
- **Acceptance:** Derive both neighbors from the same per-generation combined sequence across directories and the file-backed/source-less boundary. Eligible source-less pages participate; the last file-backed page links forward to the first source-less page, which links back. No first-to-last wraparound is allowed. The first page has no previous target, the last has no next target, and empty/single-page inputs have no adjacent links. Excluded pages and synthetic groups are not targets; non-participating pages receive no sequence links.
- **Data / rendering:** Carry target title and resolved content URL without exposing filesystem paths in rendered links. Preserve Razor text encoding and immutable snapshots, and render available links in the built-in page view in both build and preview. Generated adjacent-page data is runtime context, not new frontmatter.
- **Theme compatibility:** Expose per-page adjacency through an additive optional theme contract under TR-005/TR-011, preserving public signatures and all seven theme roles. Keep `NavigationPages`/`NavigationTree` layout-only; do not turn those full collections into view attributes or mandatory cascades. Existing custom themes remain valid without rendering the links; document how they can adopt the new context.
- **URL acceptance:** Retain TR-017's content URL, locale and base-relative formatting rules for each neighbor. Verify actual root-preview navigation and requests on correctly mounted static output, including locale-prefixed routes under `/docs/`. Per the [explicit scope update](https://github.com/getscissorhands/Scissorhands.NET/issues/81#issuecomment-5653999108), actual built-in preview-prefix mounting and its 404 regression belong to #89/V-003, not #81's gate. Locale checks remain; wrong target URLs or failures in the remaining feature checks are not excused.
- **Control acceptance:** Retain meaningful direction/target labels, keyboard access, visible focus, endpoint omission and no-JavaScript navigation. Per the [2026-09-14 decision](https://github.com/getscissorhands/Scissorhands.NET/issues/81#issuecomment-5654089522), all pager text must achieve at least 4.5:1 contrast against the rendered background and focus indicators at least 3:1 against the adjacent background. Evaluate light/dark themes and relevant normal/hover/focus states using relative-luminance ratios; do not substitute nominal tokens for actual rendered colors.
- **Browser acceptance:** Chromium, Firefox and WebKit automation at desktop and mobile viewport sizes is sufficient evidence for #81. Record engine versions, platform, viewport, theme and JavaScript mode. This deliberately scopes the component's evidence; the wider real-browser/device matrix stays in V-005, and WebKit is not proof of Safari/iOS coverage. No whole-site WCAG conformance is claimed.
- **Lifecycle boundary:** Match the existing pre-hook navigation snapshot; do not add a second plugin pass or promise refresh after route-changing hooks. TDD DQ-006 retains that wider collection-link limitation. Generated adjacent-page data is not `ContentMetadata` frontmatter and requires no `section`, `pages.json`, `prev`, `next`, or icon field.
- **Verification:** V-008 owns adjacency/boundary/exclusion, rendering/context, target encoding/privacy, new-link serving, controls, and affected legacy-theme acceptance. Precisely scoped results may support V-002/V-003/V-005/V-007 without completing their broader obligations.

### TR-023: Generate and link locale-scoped collection pages

- **State / source:** Locale-only approval followed by separate implementation and #89-integration requests from @justinyoo on 2026-09-16. Implemented with passing scoped V-009 acceptance; TG-007/DQ-009 retain integration boundaries. No whole-document or release approval is inferred.
- **Rationale / enablement:** With `UseLocaleInUrl: true`, the engine must compose home/tag routes and content from one consistently resolved locale scope. False must retain existing root/shared-collection behavior, source rules and 404 behavior. No new enablement flag or supported-locale list is introduced.
- **Locale identity / inventory:** Use `ContentUrlHelper.GetLocaleSegment` consistently to key `Site.Locale` and effective document locales; equivalent normalized spellings share a scope. Inventory is the default plus locales of loaded, published non-404 posts/pages, including navigation-hidden pages; exclude drafts and do not infer membership from slugs/folders. Retain frontmatter-over-site fallback and FR-002 future-date behavior. With routing enabled, an empty/unsafe default or effective locale cannot produce an invalid root target or unsafe path; fail contextually. This is not a new culture-name whitelist or broader URL-scheme policy.
- **Collection acceptance:** Generate one homepage at `<locale>/` per participating locale, even with no posts. Its tag index and individual tag routes are under `<locale>/tags`, generated only when that locale has eligible tagged content. No tags means no built-in Tags link. Include only that locale's loaded eligible documents, retaining custom-404 exclusion and existing post/tagged-page ordering; do not invent another locale's content for empty collections.
- **Root acceptance:** Root `index.html` must lead to the generated default-locale homepage, honoring `Site.BaseUrl` exactly once. For `/blog/` and `en-US`, the target is `/blog/en-us/`. The output must work on an ordinary directory-index static host without a production .NET service or required JavaScript, with an ordinary fallback anchor. Do not promise an HTTP 3xx from an HTML file. A missing/invalid target or redirect loop must not silently produce a successful artifact.
- **Browsing acceptance:** Prepare navigation and automatic adjacency within the active locale using TR-006/TR-021/TR-022 eligibility, ordering, snapshot and source-less compatibility rules. Preserve layout-only full navigation and optional page context. Locale boundaries, not source-directory boundaries, stop cross-language neighbors. Home/site-title/tag links use active-locale routes, with no automatic language fallback. This applies to engine-generated links, not rewriting authored Markdown links.
- **Theme/plugin context:** Generated collections must expose their locale and resolved route consistently during rendering and post-HTML processing, without fabricating source Markdown or post metadata. Preserve index/site metadata defaults except the effective locale; preserve existing tag-title distinctions. Do not mutate shared `Site.Locale` between renders or remove existing public helper signatures. Any new context/helper must be additive, reuse shared URL semantics and retain seven-role theme discovery.
- **Authoring / integrity:** Folders remain organizational; explicit `locale` and locale-free `slug` are recommended, not newly required fields. A folder/frontmatter mismatch does not override metadata or fail solely because the folder differs. Omitted/blank slugs retain path inference. Validate locale-derived paths and include every generated/redirect destination in collision ownership; source pages cannot overwrite locale homes. Preserve explicit-slug precedence, date-only-on-post rules, normalization parity, contextual failures, cancellation and containment. Combined date/already-prefixed locale composition must not duplicate the effective prefix; existing literal source-directory text is not automatically removed.
- **Legacy redirects:** Generate unprefixed `tags` and per-tag redirects only for corresponding generated default-locale destinations. Plan redirects from that locale's actual tag routes, not a global tag union; missing targets emit no redirect or mixed-language listing. Otherwise-unowned missing routes use the host's normal 404 behavior. Validate each emitted redirect's target, base path and ownership like the root redirect. In-place preview can retain stale removed routes under TR-020; acceptance of missing-route behavior uses a clean build/restart and must disclose this limitation.
- **Presentation / shared 404:** Retain shared asset locations, text defaults and seven theme roles; add no translation catalog, translated site settings, switcher or locale-specific 404. The single root 404 uses default-locale context/navigation. For enabled routing, a custom-404 locale that normalizes differently from `Site.Locale` must fail with source/field context; omission/equivalent spellings pass. Exclude the custom 404 from inventory regardless of its tags. Content publication and navigation visibility remain distinct from language filtering and authorization.
- **Snapshot boundary:** Inventory, collection membership, active locale and navigation are based on the loaded documents before hooks, as with existing navigation. Do not add a second plugin pass or promise refreshed collection membership after a plugin changes metadata. Preserve per-document hook propagation and DQ-006's wider post-plugin consistency gap; synthetic routes/locales must still agree between rendering and post-HTML processing.
- **Verification:** V-009 covers English/Korean same-slug content and shared tags, empty/default/draft-only and normalized-equivalent locales, folder/frontmatter mismatch, no duplicate date/locale prefixes, scoped navigation including source-less tiers, absent tags, conditional legacy routes, 404 mismatch, collisions, invalid input/cancellation, and context/helper/theme compatibility. Exercise root and legacy redirects without required JavaScript and follow all targets/assets at `/` and `/blog/` in build and preview. Reuse existing .NET/sample/browser seams and record outcomes separately. #89 owns middleware mounting; a failing preview-prefix request blocks that acceptance rather than proving locale generation fixes it.

## 5. Verification and bidirectional traceability

### Product-to-technical coverage

All active references below use PRD v0.13. V-003 retains #89's execution history, V-008 retains #81 results, and V-009 owns combined locale acceptance. Neither mappings nor imported evidence substitute for integrated verification.

| PRD ID | TRD coverage | Disposition |
| --- | --- | --- |
| FR-001 | TR-001/005/016/020 | Mode, static output, completion/failure |
| FR-002 | TR-002/004/006 | Content model, navigation flag parsing, publication rules, conversion |
| FR-003 | TR-003/013/017 | Routes, final path integrity, deployment URLs |
| FR-004 | TR-003/005/006 | Collection/404 destinations and rendering |
| FR-005 | TR-007/011 | Theme selection, overrides, compatibility |
| FR-006 | TR-007/008/013/017 | Manifests, assets, containment, URLs |
| FR-007 | TR-004/005/010/016 | Stage propagation, rendering, order, cancellation |
| FR-008 | TR-009/010/011 | Identity, enablement, dependencies, compatibility |
| FR-009 | TR-012/016/020 | Watcher serialization, failures, output lifecycle |
| FR-010 | TR-005/006/011/017/018 | Layout-only hierarchy, compatibility, shared URLs, client behavior and tag links; TG-002 retains contrast-method detail |
| FR-011 | TR-021/TR-022 | Feature-owned ordering, compatibility, adjacency and rendering criteria, verified by V-008; inherit TR-005/006/011/013/014/015/016/017/018 as applicable shared constraints, not completion claims; TG-006 policy resolved |
| FR-012 | TR-023; TR-002/003/005/006/011/017/018/021/022 as qualified | Implemented locale behavior and migration with completed scoped V-009 evidence after integrating and exercising #89 |
| NFR-001 | TR-007/009/015; Section 2 | Local/executable trust boundary; no added identity service |
| NFR-002 | TR-003/008/013 | Required root/ownership invariants; implementation evidence pending |
| NFR-003 | TR-014 | Encoding/raw/URL boundary; TG-001 records the unspecified scheme-context policy |
| NFR-004 | TR-016/020 | Failure versus successful completion; no atomicity guarantee |
| NFR-005 | TR-017 | Root/subpath requests; host responsibilities retained |
| NFR-006 | TR-016 | Token-aware contracts and explicit limitations |
| NFR-007 | TR-001/011; CD-001/002/004/007 | Platform, package/API compatibility and migration |
| NFR-008 | TR-018 | Agreed browser/accessibility baseline; no conformance claim |
| NFR-009 | TR-019 | Measured baseline, no performance gate |
| NFR-010 | TR-015 | Privacy and executable/external-request boundaries |

PRD G-001 through G-003 remain product outcomes; no additional telemetry requirement is created. PRD scope exclusions and release decisions remain authoritative. Every TRD requirement above has its PRD/contract source and verification method in its own record.

### Feature ownership and shared obligations

Under the [accepted #81 separation](https://github.com/getscissorhands/Scissorhands.NET/issues/81#issuecomment-5653514346), FR-011 and TR-021/TR-022 own feature acceptance; V-008 owns its evidence. The following are dependencies and shared constraints, not additional records to mark complete with #81.

| Shared requirement | #81-owned extension | Wider obligation retained |
| --- | --- | --- |
| PRD FR-010 | FR-011's reading order and page navigation | Overall presentation, metadata, tags, theme preference, and disclosure behavior |
| TR-005 | TR-022's optional adjacency context | All other renderer/view contracts, post-processing and static output |
| TR-006 | TR-021's reading order and TR-022's neighbors | Collection/tag/404 behavior, grouping/eligibility invariants, and wider snapshot consistency |
| TR-011 | TR-021's ordering/source-less migration and TR-022's additive theme contract | Other public APIs, package/plugin migrations and legacy theme-service adapters |
| TR-017 | TR-022's locale-aware URL, root-preview and correctly mounted static-host evidence | Other link/asset surfaces, broader URL policy and #89's preview-prefix mounting; not a #81 gate |
| TR-018 | TR-022's approved pager ratios and three-engine desktop/mobile checks | Full-theme real-browser/device assessment and broader contrast method |

TR-013/TR-014/TR-015/TR-016 still govern new source handling, metadata, privacy and cancellation. V-008 scopes feature checks without waiving shared obligations. The user explicitly moved preview mounting to #89, outside #81's gate; any blocker of the remaining mandatory #81 checks must still be recorded.

### Technical-to-evidence coverage

The original V-001 through V-007 programs retain their broad scope. V-008 separately records #81 acceptance and may be cited for individual overlapping results without closing a broader program. This is evidence ownership, not an implementation schedule; Section 4 remains authoritative for technical behavior.

| PRD verification reference | TRD requirements | Method / conditions | Evidence state |
| --- | --- | --- | --- |
| V-001 | TR-003/008/013/015 | Filesystem and path/ownership cases, including link escapes, asset interactions, and final transformed routes | Pending; existing [route tests](test/ScissorHands.Web.Tests/Generators/StaticSiteGeneratorRouteTests.cs) are a starting point, not full proof |
| V-002 | TR-014/015 | Contextual rendering/URL, navigation-label encoding, and synthetic-privacy cases; TG-001 precedes new scheme rules | Pending; shared formatting is not a completed sink inventory or safety certification |
| V-003 | TR-005/006/008/017 | Serve artifacts and preview at root/prefix; inspect hierarchy, locale routes, directory indexes and assets. #89 owns the preview-mount fix | Broad program open; normalization follow-up passed ten actual-application HTTP cases and 274 real-sample HTTP checks on Windows, plus canonical HTML and slash-variant build parity. Earlier results and failures retained; see PRD V-003 |
| V-004 | TR-001/002/012/013/016/020 | Controlled failures/cancellation/event bursts; inspect completion signals, recovery, and filesystem state | Pending; no atomic or bounded-shutdown requirement |
| V-005 | TR-018 | Browser/device versions, disclosure/touch/keyboard/focus/no-JavaScript behavior and contrast; TG-002 retains method uncertainty | Pending; component markup cases do not prove browser coverage |
| V-006 | TR-019 | Actual-blog measurements with workload/environment/method; TG-003 supplies execution inputs | Pending; no measurements or numerical performance target |
| V-007 | TR-001 through TR-012, TR-015 through TR-018, TR-020 | Baseline unit/component suites, sample build/preview, navigation/route/helper regressions, migration, and recorded CI results | Open; may cite V-008 for feature-specific regression evidence, not full-area completion |
| V-008 | TR-021/TR-022; scoped shared constraints | Existing feature checks plus approved ratios and three-engine desktop/mobile automation | Passed locally: 42 browser and 4 math cases, 168 measurements, minima 8.58:1 text / 9.51:1 focus; 493 .NET cases pass. Full record in PRD V-008; broader scopes remain open |
| V-009 | TR-023 and affected shared contracts | Mixed-locale generation/context, root/legacy redirects, empty scopes, 404, HTTP, both locale modes, authoring, ownership and invalid inputs | Completed within approved scope: 678 .NET tests, 60 browser cases plus four math cases, 14 real-generation/Kestrel configurations, slash-variant byte parity and live prefixed browser/watcher checks. Earlier failure retained in PRD |

### Technical acceptance and release relationship

A verification record must identify the code/configuration/workload, environment, exercised cases, outcomes, and reproducible failures or blocked/skipped coverage. Recording an attempt is not passing evidence. For TR-019, acceptance is a usable measurement record rather than beating an unagreed time budget.

For #81, completion requires its delivered obligations, V-008 evidence and RD-005 documentation, now recorded as complete. #89 remains independently owned: its later passing preview checks do not rewrite the earlier failed request or change V-008. Shared programs and original IDs remain open where work remains; no product-wide release readiness is inferred.

Technical evidence supports the approved PRD release criteria: functional/contract regression, failure and integrity behavior, root/subpath serving, client accessibility, actual-blog measurement, and consistent migration documentation. Approval of those criteria is not evidence that they have been met or authorization for a release. Failed mandatory behavior needs correction and re-evaluation; scope changes or waivers cannot be invented by this TRD.

Document approval records acceptance of the technical obligations; it does not start checks, code changes, package publication, or deployment. Verification stays in the next phase. A later implementation-ready TRD would still not mean the product is tested or releasable.

## 6. Technical gaps, risks, and readiness

### Explicit gaps and dependencies

| ID | State / source and issue | Affected references | Impact and blocking distinction | Owner / next action |
| --- | --- | --- | --- | --- |
| TG-001 | Shared URL formatting is documented; complete context-specific scheme policy and raw/metadata sink inventory remain unknown under PRD NFR-003 | TR-014/017, V-002/V-003 | Blocks new scheme rules and unsupported base-URL decisions, not reuse of existing helpers. Theme/image formatting does not establish safety; preview mounting is separately pending | Unassigned; inventory remaining contexts, resolve rules, and route product-content compatibility changes to the PRD |
| TG-002 | Broader theme method remains open; #81's component method is resolved and its checks passed | TR-018/V-005 generally; TR-022/V-008 exception | No remaining #81 contrast gap; broader theme acceptance and certification remain separate | Preserve V-008 results and repeatable checks; resolve wider methodology separately |
| TG-003 | Confirmed next-phase dependency: actual blog snapshot, access/location, configured extensions, machine details, and executor are not supplied as benchmark inputs | TR-019, V-006 | Blocks running the benchmark until inputs are available; does not block defining or implementing the existing generator behavior | Unassigned; obtain owner-selected inputs and record the method/environment in the next phase |
| TG-004 | Confirmed limitation: source inspection and selected test references do not establish complete route-composition, containment, final-route, asset, cancellation, or browser coverage | TR-003/008/012 through TR-018/020, V-001 through V-005 and V-007 | Verification/fix work remains pending. Do not reinterpret missing evidence as either a passed guarantee or automatic failure | Unassigned; execute the already deferred PRD verification, include combined date/locale/existing-prefix cases, and track reproducible gaps |
| TG-005 | Historical approvals and the 2026-09-16 locale-only approval retained; no whole-document approval of other revisions | TRD revisions outside the #81 and locale approval scopes; PRD Q-004 | Scoped sign-offs do not supply execution results, resolve shared gaps or settle release arrangements | Approver is @justinyoo; remaining execution/release roles are unassigned |
| TG-006 | Resolved and implemented: file-backed source order first, then source-less ordinal title/route order, with one combined adjacency sequence | PRD FR-011; TR-021/TR-022 under TR-005/011; TDD DQ-008 | No remaining source-less policy/integration gap in this implementation; Windows regressions cover all-file/all-source-less/mixed/direct/explicit-tree cases. Overall V-008 blockers are separate | Retain decision and implementation evidence under V-008; no broad acceptance or release claim |
| TG-007 | Resolved choices/approval and implemented TR-023/TR-017 integration on 2026-09-16 | FR-012/TR-023 and affected shared contracts; V-009 | Scoped integration and actual prefix preview pass after merging #89. Wider TG-001/DQ-006 obligations are not closed | Retain approved DES-013 and the combined V-009 regressions; broader shared obligations remain separately tracked |

Within-process watcher serialization is defined; cross-process coordination, transactional rebuilds, plugin side-effect idempotency, and guaranteed source snapshots are not added. A future requirement for those behaviors must go through the PRD rather than appearing as an implicit locking/retry design here.

### Decisions and material changes

| Date | Decision / interpretation | Basis | Effect |
| --- | --- | --- | --- |
| 2026-09-11 | Use PRD v0.5, Review-ready, for the current vNext baseline | User's requested sources and current PRD | Preserve FR/NFR IDs, confirmed quality choices, and product exclusions |
| 2026-09-11 | Keep V-001 through V-007 in the next phase | Explicit user decision recorded in PRD v0.5 | Define verification methods without executing them or inventing results/owners/dates |
| 2026-09-11 | Separate contract obligations, current implementation, and evidence gaps | Source/PRD synthesis; not an additional product approval | Avoid treating historical suggestions, partial protections, or proposed hardening designs as settled implementation facts |
| 2026-09-11 | Record approval of both documents; issue signed TRD v0.2 against approved PRD v0.6 | Explicit approval by @justinyoo | Preserve all technical obligations and verification deferrals; resolve document sign-off, retain TG-001 through TG-004, and separate remaining release arrangements in TG-005 |
| 2026-09-13 | Align TRD v0.3 with PRD v0.7 and code `1f963ad` | User update request; merged #86/#87 and source/reference inspection | Preserve 20 IDs; expand navigation, directory-index, shared-helper, rendering/client, migration and evidence contracts without claiming new sign-off or resolving scheme/mount gaps |
| 2026-09-13 | Align TRD v0.4 with PRD v0.8 and #81's settled alternative | @justinyoo's decision comment and requested document update at `843272a` | Add TR-021/022, revise affected ordering/context/compatibility obligations, record TG-006, and exclude superseded metadata/JSON inputs; preserve existing IDs, approvals, gaps, and open issue state |
| 2026-09-13 | Include TG-006 / DQ-008 resolution in #81's required delivery | Explicit user scope decision; clarification linked in TR-021 | Require compatible source-less caller behavior and coverage before issue completion; preserve the unresolved policy until selected rather than treating scope inclusion as resolution |
| 2026-09-13 | Resolve TG-006 / DQ-008 with the confirmed two-tier policy | @justinyoo accepted the recommendation; policy comment linked in TR-021 | Specify file-backed-first/source-less-last ordering, combined adjacency, retained public/explicit-tree contracts, and invalid-path rejection; mark decision resolved without claiming implementation |
| 2026-09-13 | Separate feature-owned obligations and V-008 evidence | @justinyoo accepted the separation; issue comment linked in Section 5 | Consolidate #81 detail in TR-021/TR-022; retain shared requirements with cross-references, preserve V-001 through V-007, and add independent feature acceptance without waiving dependencies |
| 2026-09-13 | Approve TRD v0.4 only for #81 | Explicit scoped approval by @justinyoo; approval record linked in Document control | Approve TR-021/TR-022 and feature application of shared constraints, including compatibility/evidence requirements; retain unrelated gaps and whole-document review, with implementation and V-008 results pending |
| 2026-09-13 | Align TRD v0.5 with #81 implementation and PRD v0.9 evidence | User requested implementation after scoped approval | Record implemented TR-021/TR-022 and TG-006 compatibility, preserve all IDs, and distinguish scoped passing checks from the reproduced preview-prefix blocker and incomplete browser/contrast acceptance |
| 2026-09-13 | Track preview-prefix mounting independently in #89 | Explicit user confirmation; scope update linked in TR-022 | Remove the mount from #81/V-008 completion while retaining locale-aware neighbor checks; keep TR-017/V-003/DEC-002 obligations and failed HTTP evidence with #89 |
| 2026-09-14 | Specify approved component acceptance in TRD v0.6 | User selected measurable ratios and three-engine desktop/mobile evidence; decision linked in TR-022 | Resolve the #81-specific method, preserve global TR-018/V-005 scope, and require execution rather than infer success from earlier Chromium observations |
| 2026-09-14 | Record complete component acceptance | Further implementation request; browser suite, math tests and full .NET run | Correct contrast and WebKit keyboard failures; V-008 passes its approved scope, with #89 and broader obligations retained |
| 2026-09-16 | Draft TR-023 against PRD v0.11 | Requested locale-document revision and explicit default-locale root redirect selection | Add V-009/TG-007 traceability, qualify retained routing/navigation contracts and keep implementation/presentation/migration decisions explicit; no runtime or prior-approval changes |
| 2026-09-16 | Reconcile confirmed locale choices | @justinyoo selected PRD Q-007 through Q-012 | Resolve TG-007's product decisions, define inventory/authoring/scoping/redirect/404 acceptance and retain shared gaps and unimplemented status; no whole-document sign-off |
| 2026-09-16 | Approve TRD v0.7 only for the locale scope | Explicit @justinyoo approval of PRD/TRD/TDD only for this scope, reviewing `94fc60b` | Approve TR-023 and feature application of shared/V-009 obligations; retain DQ-009 integration checks, broader gaps, #89 and historical approvals. No implementation, evidence or release claim |
| 2026-09-16 | Record authorized TR-023 implementation in v0.8 | User requested implementation within approved scope; PRD V-009 records execution | Implement context, scoped generation/navigation, redirects, validation and migration without public API removal; preserve #89's failed prefix acceptance and wider gaps |
| 2026-09-16 | Record #89 preview implementation in v0.7 | User requested the fix; PRD v0.11 and actual HTTP evidence | Update TR-017's implementation/compatibility boundary and V-003 evidence; preserve all 22 IDs, TG-001 and broader verification |
| 2026-09-16 | Enforce prefix-only preview under TR-017 | User clarification and fix request, reflected in PRD FR-009 | Replace the unrequested alias assumption with outside-prefix 404; preserve root mode, redirects, public APIs and broader TG-001 scope; record passing follow-up HTTP evidence |
| 2026-09-16 | Add root navigation redirect under TR-017 | User requested `/` redirect to the configured base URL; PRD FR-009 updated | Root GET/HEAD returns 302 with query preserved and no homepage body; all other outside-prefix requests remain 404; root configuration is unchanged |
| 2026-09-16 | Normalize both configured base-path spellings under TR-017 | User requested optional trailing slashes; PRD FR-009 updated | Canonical site context is shared across binding, rendering, plugins and preview; preserve API, configuration sources and broader URL-policy boundaries |
| 2026-09-16 | Integrate TR-017 and TR-023 in v0.9 | User requested merging the completed fix from `vnext` | Preserve both requirement histories, adapt locale acceptance to canonical BaseUrl values, and verify actual mounted locale output under V-009 |

### Readiness assessment

- **Supported status:** Review-ready overall against PRD v0.13 with retained locale-only approval and authorized integration. No whole-document/release sign-off is inferred.
- **Material limitations:** V-009 passes the combined scope, including the former prefix blocker. This is not wider TG-001/DQ-006 or full asset/link safety; no V-008 acceptance is reopened.
- **Approval:** The user separately authorized implementation and this merge. Historical sign-offs remain intact; neither action closes unrelated gaps or authorizes release.
- **Reviewer pass:** Reconciled all 23 IDs with PRD v0.13/TDD v0.10, shared normalization, prefix-only serving and integrated evidence while preserving earlier failures.
- **Review limitations:** No real-device matrix, full audit, benchmark or release is claimed.

## 7. Reference map

| Topic | Sources used |
| --- | --- |
| Product baseline and origin | [PRD.md](PRD.md), [Original discussion (archived)](https://github.com/getscissorhands/Scissorhands.NET/blob/464ce0f3454d473d4a39bc6f5c9005e86cd5396a/DISCUSSIONS.md) |
| Public usage and migration | [Root guide](README.md), [Core](src/ScissorHands.Core/README.md), [Plugin](src/ScissorHands.Plugin/README.md), [Theme](src/ScissorHands.Theme/README.md), [Web](src/ScissorHands.Web/README.md) |
| Runtime and content | [Application](src/ScissorHands.Web/ScissorHandsApplication.cs), [loader](src/ScissorHands.Web/Loaders/ContentLoader.cs), [generator](src/ScissorHands.Web/Generators/StaticSiteGenerator.cs), [renderer](src/ScissorHands.Web/Renderers/ComponentRenderer.cs), [watcher](src/ScissorHands.Web/Watchers/ContentWatcher.cs) |
| Model and configuration contracts | [ContentDocument](src/ScissorHands.Core/Models/ContentDocument.cs), [ContentMetadata](src/ScissorHands.Core/Models/ContentMetadata.cs), [SiteManifest](src/ScissorHands.Core/Manifests/SiteManifest.cs), [ThemeManifest](src/ScissorHands.Core/Manifests/ThemeManifest.cs), [PluginManifest](src/ScissorHands.Core/Manifests/PluginManifest.cs) |
| Navigation, URLs, and design | [NavigationNode](src/ScissorHands.Core/Models/NavigationNode.cs), [builder](src/ScissorHands.Web/Navigation/NavigationTreeBuilder.cs), [ContentUrlHelper](src/ScissorHands.Core/Urls/ContentUrlHelper.cs), [website handoff](docs/website-documentation.md), [TDD v0.10](TDD.md) |
| Implemented reading order and adjacency | [Reading order](src/ScissorHands.Web/Navigation/PageReadingOrder.cs), [PageNavigation](src/ScissorHands.Core/Models/PageNavigation.cs), PRD FR-011/V-008, TR-021/TR-022 |
| Extension implementation evidence | [Plugin runner](src/ScissorHands.Web/Runners/PluginRunner.cs), [dependency resolver](src/ScissorHands.Web/Runners/PluginDependencyResolver.cs), [theme resolver](src/ScissorHands.Web/Services/ThemeComponentResolver.cs), [theme service](src/ScissorHands.Web/Services/ThemeService.cs) |
| Standards and verification conventions | [AGENTS.md](AGENTS.md), [SDK configuration](global.json), [build settings](Directory.Build.props), [CI](.github/workflows/main.yaml), [sample guide](sample/README.md) |
