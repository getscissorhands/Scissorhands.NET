# ScissorHands.NET - Technical requirements document

## Document control

| Field | Value |
| --- | --- |
| Document version | 0.3 |
| Status | Review-ready |
| Last updated / PRD consulted | 2026-09-13 |
| PRD baseline | [PRD.md](PRD.md), version 0.7, Review-ready current-code alignment; historical v0.6 approval retained |
| Release scope | Current vNext baseline including merged #86/#87; no speculative capabilities |
| Code baseline | `1f963adfd5a1e807f9adca901043d9c72c17d951` |
| Product owner | @justinyoo, as recorded in the PRD |
| Execution owners | Not assigned |
| Intended audience | Engine, theme, and plugin contributors translating the PRD into implementation and verification obligations |
| Sign-off | v0.2 approved by @justinyoo on 2026-09-11; v0.3 alignment revision not separately signed off |
| Approval scope | Historical approval covers v0.2 obligations and gaps/deferrals; the requested alignment is not new sign-off, resolution of unspecified rules, or release authorization |

**Baseline rule:** the PRD owns product scope and acceptance. This TRD elaborates technical obligations without modifying the PRD. PRD verification items V-001 through V-007 remain pending next-phase work; creating this document does not start those checks or establish passing results.

**Readiness:** v0.3 remains Review-ready, aligned with PRD v0.7 and the merged URL/navigation changes at the user's request. Historical v0.2 approval is preserved, not extended to revised text. Shared URL formatting is now explicit, but context-specific scheme policy still needs elaboration before new validation behavior is introduced (TG-001). Contrast assessment and benchmark inputs remain TG-002/TG-003; unexecuted verification alone is not the reason for this classification.

## 1. Purpose and source relationship

The system converts local Markdown and configuration into static site artifacts using compiled Razor themes and optional plugins. This TRD specifies the technical behavior, data boundaries, integration contracts, compatibility, and verification conditions supporting PRD FR-001 through FR-010 and NFR-001 through NFR-010.

[Original discussion (archived)](https://github.com/getscissorhands/Scissorhands.NET/blob/464ce0f3454d473d4a39bc6f5c9005e86cd5396a/DISCUSSIONS.md) establishes historical intent. Its assistant-proposed renderer snippets, route crawling, general asset copying, feeds, search, and example plugins are not additional requirements. The current PRD governs those scope distinctions.

Existing source code identifies contracts and current behavior, not proof that all required protections are implemented. Package guides, [AGENTS.md](AGENTS.md), and the [website documentation handoff](docs\website-documentation.md) supply usage and migration context. [TDD.md](TDD.md) v0.4 explains mechanisms and unresolved design details against this revision; detailed test procedures, execution results, and release runbooks are not supplied here.

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

### Current behavior versus required evidence

| Area | Source-backed current behavior | Remaining obligation |
| --- | --- | --- |
| Route integrity | Generator preflights planned page routes and rejects relative path segments and collisions | Establish containment across filesystem links, asset ownership, and any plugin-transformed output routes in V-001; preflight alone is not complete evidence |
| Navigation and URLs | Engine supplies a per-generation navigation hierarchy; shared helpers format content/theme/image/tag/locale references; nested page indexes infer directory routes | Include hidden/missing ancestors, layout delivery, encoded links, route migration, and client interaction in V-003/V-005/V-007; neither formatting nor markup tests establish scheme safety or preview prefix mounting |
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
| CD-003 | [CurrentDirectoryAppPaths](src\ScissorHands.Web\Infrastructure\CurrentDirectoryAppPaths.cs), [IAppPaths](src\ScissorHands.Web\Abstractions\IAppPaths.cs) | Confirmed baseline: default roots resolve from the site working directory. CLI output names are `dist` and `preview`; the generator interface accepts a destination. Application path configuration is not content authorization |
| CD-004 | [Core guide](src\ScissorHands.Core\README.md), [Plugin guide](src\ScissorHands.Plugin\README.md), [Theme guide](src\ScissorHands.Theme\README.md), [migration reference](docs\website-documentation.md#upgrading-to-vnext) | Current contracts: required plugin IDs, read-only collections, cancellation-aware theme services, seven required theme roles, directory-index route changes, and retained navigation/URL compatibility. Old name-only plugins and themes relying on implicit tag views need migration |
| CD-005 | [Markdown service](src\ScissorHands.Web\Services\MarkdownService.cs), [content loader](src\ScissorHands.Web\Loaders\ContentLoader.cs) | Current implementation evidence: Markdig with advanced extensions/SmartyPants and YamlDotNet parsing. No library replacement, version upgrade, or new Markdown dialect is proposed |
| CD-006 | PRD NFR-008/NFR-009 and Section 7 | Confirmed: stated desktop/mobile browsers and actual-blog benchmarking; no formal WCAG claim, performance SLA, numerical speed threshold, or maximum supported site size |
| CD-007 | [AGENTS.md](AGENTS.md), [CI](.github\workflows\main.yaml), [sample guide](samples\ScissorHands.Sample\README.md) | Confirmed repository verification conventions: Microsoft.Testing.Platform, existing xUnit/Shouldly/NSubstitute/bUnit tooling, aligned build/test configurations, and Windows/macOS/Linux CI. Execution is deferred, not presumed successful |

Execution owners for the constraints and verification activities are unassigned. Existing source versions are the code baseline above; no external standards assessment or package-catalog research was performed.

### Existing interface boundaries

The linked declarations are authoritative for signatures; these summaries describe obligations, not replacements for API reference documentation.

| Surface | Contract and technical meaning |
| --- | --- |
| [Application builder](src\ScissorHands.Web\ScissorHandsApplicationBuilder.cs) / [application](src\ScissorHands.Web\ScissorHandsApplication.cs) | `Build()` produces the runnable application; `RunAsync()` selects the command mode. Generic and `Type`-based `AddLayouts` overrides accept seven theme component roles and validate assignability |
| [IContentLoader](src\ScissorHands.Web\Loaders\IContentLoader.cs) | `LoadAsync(CancellationToken)` returns `Task<IEnumerable<ContentDocument>>`; missing optional directories are distinct from invalid source data |
| [IMarkdownService](src\ScissorHands.Core\Services\IMarkdownService.cs) | `ToHtmlAsync(string, bool?, CancellationToken)` returns HTML. Existing `trim` behavior removes an enclosing paragraph only for a single paragraph, not arbitrary block markup |
| [IThemeService](src\ScissorHands.Core\Services\IThemeService.cs) | Manifest loading and asset copying have cancellation-aware overloads and retained obsolete overloads. Default interface adapters check cancellation before delegating to legacy implementations |
| [Plugin contracts](src\ScissorHands.Plugin\README.md) | Hooks operate before Markdown, after Markdown, and after full HTML rendering. IDs identify configuration/components/dependencies; optional dependencies are stage-scoped |
| [IComponentRenderer](src\ScissorHands.Web\Renderers\IComponentRenderer.cs) | `RenderAsync<TComponent>` takes layout type, parameter dictionary, and optional token, returning `Task<string>` HTML |
| [IStaticSiteGenerator](src\ScissorHands.Web\Generators\IStaticSiteGenerator.cs) | `BuildAsync` accepts seven constrained theme component types, destination, preview flag, and cancellation token; successful completion represents completion of that generation call, not deployment |
| [Theme contracts](src\ScissorHands.Theme\README.md) | All seven layout/view roles are required. `MainLayoutBase.NavigationPages` and `NavigationTree` default to empty read-only lists and are layout-only, not view attributes or automatic cascades. Existing document/collection context is unchanged; raw HTML remains an explicit rendering boundary |
| [NavigationNode](src\ScissorHands.Core\Models\NavigationNode.cs) / [navigation reference](docs\website-documentation.md#prepared-navigation) | Immutable node with text `Title`, escaped base-relative `Path`, nullable `Url`, and snapshotted read-only `Children`; null URL means a non-clickable group. Flat navigation contains only actual eligible pages |
| [ContentUrlHelper](src\ScissorHands.Core\Urls\ContentUrlHelper.cs) / [Theme wrappers](docs\website-documentation.md#url-helpers) | Shared static formatting and protected view/layout helpers retain distinct content, theme, image, tag, and locale contracts; formatting is not general URI sanitization |

### Coverage disposition

Applicability is separate from whether implementation evidence exists.

| Area | Applicability and basis | Technical coverage |
| --- | --- | --- |
| System behavior and boundaries | Applicable: local generator, renderer, extensions, preview, static artifact | TR-001 through TR-010, TR-012 |
| Identity and permissions | Applicable to local filesystem/process access and explicit plugin enablement; no accounts/tenants | TR-009, TR-013, TR-015 |
| Data and integrity | Applicable: frontmatter, content models, routes, collections, and assets | TR-002/003/006/008/011/013 |
| Data lifecycle and privacy | Applicable: source files, generated artifacts, logs, browser theme preference | TR-012/015/018/020 |
| Interfaces and integrations | Applicable: .NET APIs, configuration/files, extensions, browser and static host | Interface table; TR-004/005/007/009/010/011/017 |
| Security and abuse | Applicable despite local authoring: untrusted data and executable/raw-content boundaries | TR-013/014/015 |
| Compliance and policy | Repository policy applies; no additional regulatory certification or regulated workflow is specified | CD-002/004/007; TR-013/014/015 |
| Performance and capacity | Applicable: actual-blog generation/preview/resource measurements | TR-019 |
| Reliability and recovery | Applicable: direct writes, rebuilds, failures/cancellation; no hosted availability or atomicity target | TR-012/016/020 |
| Operations and observability | Applicable: mode selection, errors, output location, rebuild status, and evidence records | TR-001/012/019/020 |
| Migration and rollout | API/configuration migration applies; managed deployment and automatic rollback are outside scope | TR-011; PRD release gates |
| Compatibility and clients | Applicable: .NET contracts, CI platforms, generated-site browsers/subpaths | TR-001/011/017/018 |
| Accessibility and localization | Applicable: default-theme accessibility, effective locale, URL prefixes, dates, UTF-8 HTML; translation management excluded | TR-002/003/005/018 |
| AI and automation | AI/model decisions not applicable; deterministic watcher/plugin automation is in scope | No AI obligations; TR-004/010/012 |
| Domain-specific constraints | Static-file deployment applies; payment, hardware safety, offline synchronization, and transactional domains are not part of the PRD | TR-005/008/017; no additional domain services |

## 4. Technical requirements

Each `TR-...` is a stable technical requirement within the retained vNext scope; **must** expresses obligation, not implementation completeness. **Confirmed** identifies a PRD, existing contract, or policy source, not fresh document approval. v0.3 aligns those obligations with PRD v0.7 and source-backed #86/#87 behavior; historical approval applies to v0.2. Verification references remain V-001 through V-007, and execution owners remain unassigned.

### TR-001: Execute the selected application mode

- **State / source:** Confirmed; PRD FR-001, NFR-007; CD-001/003 and application/builder contracts.
- **Rationale / obligation:** To support local generation without a production server, the application must preserve explicit build, preview, and help entry points using the repository's .NET configuration.
- **Acceptance:** Build produces output under `dist` and reports its location without starting the preview listener. Preview uses `preview` and starts serving. Help displays usage. An invocation with no recognized mode reports an error and sets exit code 1; preview wins if both build and preview are supplied.
- **Boundaries:** The sample launch profile does not select a mode; callers supply it explicitly. This does not introduce strict rejection of every unrecognized extra argument or a separate standalone CLI package.
- **Verification:** V-007 application/argument tests and sample process demonstrations; V-004 distinguishes failure from successful completion.

### TR-002: Parse content into the existing logical model

- **State / source:** Confirmed; PRD FR-002; [ContentDocument](src\ScissorHands.Core\Models\ContentDocument.cs), [ContentMetadata](src\ScissorHands.Core\Models\ContentMetadata.cs), loader.
- **Rationale / obligation:** To separate content from presentation, loading must preserve source identity, post/page kind, supported metadata, source Markdown, and generated HTML through the pipeline.
- **Acceptance:** Recursively load `.md` under `contents\posts` and `contents\pages`. Optional frontmatter maps the PRD field set into the model; `published` becomes nullable `DateTimeOffset`, and `draft`/`show_in_navigation` map to booleans defaulting to false (`Draft`/`ShowInNavigation`). Tags accept a YAML list or comma-separated string. Derive missing title/slug from filename/relative path under TR-003; effective locale falls back to site locale.
- **Failure / publication:** Missing content directories warn; malformed YAML, unsupported fields, unclosed frontmatter, and invalid date/boolean/tag values fail with source/field context. Exclude drafts in both modes; do not withhold future-dated content. `ShowInNavigation` changes membership under TR-006, not generation or authorization; posts may parse the flag but never join page navigation.
- **Verification:** V-007 loader cases covering both kinds, missing metadata, explicit/invalid navigation booleans, effective locale, and publication modes; V-004 diagnostics.

### TR-003: Resolve routes without conflicting page ownership

- **State / source:** Confirmed; PRD FR-003/004, NFR-002; loader and [generator](src\ScissorHands.Web\Generators\StaticSiteGenerator.cs).
- **Rationale / obligation:** To preserve predictable addresses, routing must produce the PRD's content/index/tag/404 destinations and reject conflicting page ownership.
- **Acceptance:** Ordinary routes write `<route>\index.html`; the site index owns root `index.html`, and the not-found page owns `404.html`. Date prefixes apply only to dated posts; enabled locale prefixes use the effective normalized locale without duplicating an existing locale prefix, as required by the PRD. Preserve the PRD's `ko-kr/2026/09/11/hello` example and the 404 exception. Missing dates warn rather than schedule or fail a post.
- **Directory-index rules:** Omitted/blank slugs on nested page `index.md` files infer the containing directory, comparing the filename case-insensitively. Explicit non-blank slugs win. Root page `index.md` retains `index`; posts are unchanged. Apply locale rules after inference. `parent.md` and `parent\index.md` cannot both claim `parent`; explicit `slug: parent/index` preserves the older nested-index route.
- **Integrity:** Reject literal `.`/`..` segments, case-insensitive output collisions, and file-versus-parent-directory conflicts. Source route validation occurs before page writes, not necessarily before initial output cleanup. Final route/asset safety is also TR-013; plugin transformations must not bypass it.
- **Verification:** V-007 directory-index/explicit-slug/date/locale/404 cases and V-001 collision/containment cases, including inferred-directory conflicts, generated routes, and transformation boundaries.

### TR-004: Propagate transformations through the fixed pipeline

- **State / source:** Confirmed; PRD FR-007/008; generator, Markdown service, and Plugin guide.
- **Rationale / obligation:** To compose extensions predictably, the engine must propagate each returned result through pre-Markdown, Markdown conversion, post-Markdown, Razor rendering, and post-HTML in that order.
- **Acceptance:** Pre-Markdown output supplies conversion input; conversion populates `ContentDocument.Html`; post-Markdown output reaches the view; post-HTML output is written. A plugin returning a replacement document is not ignored. Disabled plugins do not participate.
- **Boundaries:** Source-backed custom 404 content receives Markdown stages; synthetic index/tag/default-404 documents have no source Markdown stages but receive post-HTML. `SiteManifest.IsPreview` is set before hooks. Ordering within a stage follows TR-010.
- **Verification:** V-007 distinguishable transforms across stages, replacement-document cases, disabled plugins, synthetic/source-backed pages, and both modes.

### TR-005: Render complete static documents through Razor contracts

- **State / source:** Confirmed; PRD FR-001/007/010; [ComponentRenderer](src\ScissorHands.Web\Renderers\ComponentRenderer.cs), Theme guide.
- **Rationale / obligation:** To produce ordinary static pages, rendering must compose the selected layout/view with the required parameters and cascading context, then return complete HTML for post-processing and UTF-8 output.
- **Acceptance:** Layout and view can access their applicable document/collection, plugins, theme, and site context without mistaking cascading-only values for normal view parameters. Deliver `NavigationPages`/`NavigationTree` to the layout only; neither is an automatic cascading value. Normal generation supplies both; for a `MainLayoutBase` layout, an older renderer caller supplying only `IReadOnlyList<ContentDocument>` navigation pages gets a prepared tree. Preserve an explicitly supplied tree. Await rendering before post-HTML processing and UTF-8 output; no Blazor circuit is needed to serve it.
- **Boundaries:** The current implementation uses `HtmlRenderer`; optional theme/plugin JavaScript is allowed. Do not import lifecycle assertions or route-enumeration suggestions from the historical discussion as new contract guarantees.
- **Verification:** V-007 renderer/context cases including navigation filtering, flat-only compatibility, explicit-tree preservation, and generated-file inspection; V-003 artifact serving without the application runtime.

### TR-006: Construct collection, navigation, and not-found data

- **State / source:** Confirmed source-backed behavior; PRD FR-004/010; generator, [navigation builder](src\ScissorHands.Web\Navigation\NavigationTreeBuilder.cs), and Theme guide.
- **Rationale / obligation:** The engine must supply index/tag/404 data and consistent page navigation while leaving markup and interaction to themes.
- **Acceptance:** Index posts are ordered by descending publication date with undated posts last. Always generate index/404. A page with `slug: 404.html` uses the not-found view and is excluded from normal page rendering and displayed tag groups. Group eligible tags by lowercased name, then emit `tags` and encoded per-tag routes, with tagged posts newest first and pages ordered by title.
- **Empty / failure:** No eligible tagged documents means no tag pages. An empty content set still renders index/404. Output conflicts remain errors under TR-003; generating a 404 file does not configure a host's HTTP status behavior.
- **Navigation selection:** From loaded pages, select opted-in, non-draft, non-404 documents. A loaded hidden page suppresses descendants using ordinal, whole-route-segment matching. Flat `NavigationPages` sorts by ordinal title then resolved slug. Drafts removed by the loader do not participate in this selection; navigation is not an alternative publication filter.
- **Hierarchy:** Derive `NavigationTree` from resolved routes with non-clickable groups for missing ancestors, retaining only groups with visible descendants. Group titles decode the path segment, replace hyphens/underscores with spaces, and apply invariant title casing. Sort siblings by ordinal title then escaped path. Suppress synthesized locale-prefix groups when locale routing is enabled, but retain actual visible locale landing pages. Groups create no documents, files, or placeholder links.
- **Lifecycle / compatibility:** Every generated layout receives the same per-generation flat list and immutable tree. Preserve empty defaults and defensively copied node children. The current snapshot is taken before document hooks; plugin-returned title/slug/visibility changes do not refresh it. Broader post-plugin collection consistency remains TDD DQ-006, not a promise of two-pass generation.
- **Verification:** V-007 empty/mixed/404/tag-order cases, hidden/missing ancestors, whole-segment matching, locale groups, ordinal ordering, immutability, all-layout delivery and snapshot boundaries; V-001 route conflicts and V-003 real links.

### TR-007: Resolve the configured theme and manifest

- **State / source:** Confirmed; PRD FR-005/006; Theme guide, resolver/service evidence, builder overrides.
- **Rationale / obligation:** To make themes replaceable, the engine must resolve a compatible component set and manifest for the configured theme without rewriting content or requiring explicit registration in the conventional case.
- **Acceptance:** Match the normalized namespace suffix to `Site:Theme`; require exactly one concrete component for each layout/index/post/page/not-found/tag-list/tag role, with no implicit built-in tag-view substitution. Explicit seven-role `AddLayouts` overrides remain usable. Load `themes\<slug>\theme.json`; invalid/mismatched custom manifests or unresolved required custom components fail. Preserve built-in theme selection for empty/default theme settings.
- **Boundaries:** Compiled Razor changes require compilation; theme selection changes require restart/rebuild. No install UI, arbitrary untrusted extension loader, or name-based plugin fallback is introduced.
- **Verification:** V-007 conventional/override/custom/default theme cases, missing or ambiguous required tag roles, invalid manifests, and configuration changes.

### TR-008: Copy only the supported asset surfaces

- **State / source:** Confirmed; PRD FR-006; generator and Theme service/guide.
- **Rationale / obligation:** To make generated output usable on a static host, copying must retain relative structure and the baseline's supported content/theme assets.
- **Acceptance:** Copy `contents\images` recursively to output `images`; copy theme `assets` content and the theme service's supported image/manifest files below `themes\<slug>`. Theme stylesheet/script entries remain available for inclusion by the layout. Retain bundled default-theme assets.
- **Boundaries:** Missing optional content images warn; missing custom manifests fail under TR-007, while missing theme asset folders can warn. Arbitrary application `wwwroot`, automatic plugin asset export, and downloading external references are not guaranteed. All copies are subject to TR-013.
- **Verification:** V-007 fixture inventories and relative destination paths; V-001 collision/link containment and V-003 asset requests.

### TR-009: Select enabled plugins by validated stable ID

- **State / source:** Confirmed; PRD FR-008, NFR-001; [PluginIdValidator](src\ScissorHands.Core\Validation\PluginIdValidator.cs), Plugin guide.
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
- **Verification:** V-007 snapshots, flat-navigation and helper compatibility, migrated consumers, required tag roles, directory-index overrides, explicit layout registration, and legacy theme-service adapters.

### TR-012: Serialize preview regeneration within the watcher

- **State / source:** Confirmed; PRD FR-009; application and [ContentWatcher](src\ScissorHands.Web\Watchers\ContentWatcher.cs).
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

- **State / source:** Confirmed; PRD NFR-005, FR-003/006/010; Site manifest and Theme/Plugin guides.
- **Rationale / obligation:** To support subpath deployment, generated navigation and asset references must resolve under `SiteManifest.BaseUrl` where they address the site's own content.
- **Acceptance:** At root and `/docs/`, requests for generated post/page/tag links, content images, theme CSS/JS, and representative plugin links reach the intended files without escaping to the domain root. Validate rendered requests rather than only the presence of a `<base>` tag.
- **Shared formatting contract:** Reuse [ContentUrlHelper](src\ScissorHands.Core\Urls\ContentUrlHelper.cs) through the [Theme wrappers](docs\website-documentation.md#url-helpers). Content URLs trim outer whitespace, split either slash separator, discard empty segments, escape each segment, and return `.` for an empty route; literal `.`/`..` segments fail. Tag URLs trim/lowercase and escape one segment under `tags/`; empty/dot tags fail. Theme URLs concatenate `themes/<slug>/<path>` with forward-slash trimming only. Image URLs only strip leading forward slashes, preserving HTTP(S), queries, fragments, and existing encoding. Locale segments retain trim/lowercase and underscore/forward-slash-to-hyphen normalization.
- **Errors / base handling:** Content/theme/image/tag null inputs fail; locale null/whitespace maps to empty. The layout theme wrapper checks null path before missing `Theme`, which throws `InvalidOperationException`. Content, tag, and theme URLs remain base-relative without prepending `Site.BaseUrl`; do not escape images as content slugs or treat formatting as scheme validation.
- **Boundary:** Distinguish artifact-relative paths from deployment mount paths. The production host owns mounting, directory indexes, and 404 routing; preview behavior must also be evaluated. Do not rewrite unrelated absolute external URLs into local assets or promise a specific hosting provider.
- **Implementation gap:** Preview still lacks configured prefix mounting; TDD DEC-002 remains an unimplemented design. Shared helpers and `<base>` markup do not close that gap.
- **Verification:** V-003 real root/prefix requests including hierarchy, directory indexes, date/locale routes, and extension output; V-007 helper/view parity, encoded text, null/invalid inputs, and preserved image semantics.

### TR-018: Deliver the agreed built-in-theme client behavior

- **State / source:** Confirmed; PRD v0.7 FR-010/NFR-008, retaining the user's original quality decision and documenting current navigation behavior.
- **Rationale / obligation:** To make generated pages usable, the built-in theme must support the agreed browser matrix, keyboard navigation, visible focus, meaningful control labels, and readable light/dark contrast.
- **Acceptance:** Evaluate ordinary reading/navigation and the theme control on current stable desktop Edge/Chrome/Firefox/Safari, Android Chrome, and iOS Safari; record actual versions/platforms/devices. With JavaScript and storage available, the light/dark selection persists. Page title/description/locale follow the document/site fallback contract.
- **Navigation / tags:** Render page nodes as anchors and null-URL groups as text, with separate labelled disclosure buttons and accurate expanded state. JavaScript enables mouse/touch/Enter/Space toggling, closes the applicable group on Escape and restores button focus, and dismisses menus outside navigation. Without JavaScript, eligible child links remain visible. Render normalized tag links on both posts and pages.
- **Limits / detail:** Custom-theme authors own their accessibility. No formal WCAG or comprehensive assistive-technology certification is claimed. PRD v0.7 retains no numerical contrast threshold; a reproducible method remains TG-002. No new storage-disabled preference fallback is promised.
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

## 5. Verification and bidirectional traceability

### Product-to-technical coverage

All active references below use PRD v0.7. Coverage means a technical obligation exists, not new document approval or a passing test.

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

### Technical-to-evidence coverage

The following references the PRD's existing next-phase work rather than creating a separate implementation schedule. Acceptance criteria reside in Section 4 and are not replaced by this grouping.

| PRD verification reference | TRD requirements | Method / conditions | Evidence state |
| --- | --- | --- | --- |
| V-001 | TR-003/008/013/015 | Filesystem and path/ownership cases, including link escapes, asset interactions, and final transformed routes | Pending; existing [route tests](test\ScissorHands.Web.Tests\Generators\StaticSiteGeneratorRouteTests.cs) are a starting point, not full proof |
| V-002 | TR-014/015 | Contextual rendering/URL, navigation-label encoding, and synthetic-privacy cases; TG-001 precedes new scheme rules | Pending; shared formatting is not a completed sink inventory or safety certification |
| V-003 | TR-005/006/008/017 | Serve artifacts and preview at root/prefix; inspect hierarchy, directory-index and asset requests | Pending; prefix mount remains unimplemented, and no production host is selected |
| V-004 | TR-001/002/012/013/016/020 | Controlled failures/cancellation/event bursts; inspect completion signals, recovery, and filesystem state | Pending; no atomic or bounded-shutdown requirement |
| V-005 | TR-018 | Browser/device versions, disclosure/touch/keyboard/focus/no-JavaScript behavior and contrast; TG-002 retains method uncertainty | Pending; component markup cases do not prove browser coverage |
| V-006 | TR-019 | Actual-blog measurements with workload/environment/method; TG-003 supplies execution inputs | Pending; no measurements or numerical performance target |
| V-007 | TR-001 through TR-012, TR-015 through TR-018, TR-020 | Unit/component suites, sample build/preview, navigation/route/helper regressions, migration, and recorded CI results | Open; existing test source is available, but this update establishes no executed results or full-area completion |

### Technical acceptance and release relationship

A verification record must identify the code/configuration/workload, environment, exercised cases, outcomes, and reproducible failures or blocked/skipped coverage. Recording an attempt is not passing evidence. For TR-019, acceptance is a usable measurement record rather than beating an unagreed time budget.

Technical evidence supports the approved PRD release criteria: functional/contract regression, failure and integrity behavior, root/subpath serving, client accessibility, actual-blog measurement, and consistent migration documentation. Approval of those criteria is not evidence that they have been met or authorization for a release. Failed mandatory behavior needs correction and re-evaluation; scope changes or waivers cannot be invented by this TRD.

Document approval records acceptance of the technical obligations; it does not start checks, code changes, package publication, or deployment. Verification stays in the next phase. A later implementation-ready TRD would still not mean the product is tested or releasable.

## 6. Technical gaps, risks, and readiness

### Explicit gaps and dependencies

| ID | State / source and issue | Affected references | Impact and blocking distinction | Owner / next action |
| --- | --- | --- | --- | --- |
| TG-001 | Shared URL formatting is documented; complete context-specific scheme policy and raw/metadata sink inventory remain unknown under PRD NFR-003 | TR-014/017, V-002/V-003 | Blocks new scheme rules and unsupported base-URL decisions, not reuse of existing helpers. Theme/image formatting does not establish safety; preview mounting is separately pending | Unassigned; inventory remaining contexts, resolve rules, and route product-content compatibility changes to the PRD |
| TG-002 | Unknown assessment detail: PRD NFR-008 confirms readable contrast but deliberately makes no WCAG claim and specifies no numerical threshold | TR-018, V-005 | Limits reproducible contrast acceptance and associated sign-off, not implementation of the agreed keyboard/label/focus behavior. Do not equate this with a missing decision to pursue formal certification | Unassigned; agree and record a proportionate contrast-assessment method before V-005 sign-off; no conformance level is assumed |
| TG-003 | Confirmed next-phase dependency: actual blog snapshot, access/location, configured extensions, machine details, and executor are not supplied as benchmark inputs | TR-019, V-006 | Blocks running the benchmark until inputs are available; does not block defining or implementing the existing generator behavior | Unassigned; obtain owner-selected inputs and record the method/environment in the next phase |
| TG-004 | Confirmed limitation: source inspection and selected test references do not establish complete route-composition, containment, final-route, asset, cancellation, or browser coverage | TR-003/008/012 through TR-018/020, V-001 through V-005 and V-007 | Verification/fix work remains pending. Do not reinterpret missing evidence as either a passed guarantee or automatic failure | Unassigned; execute the already deferred PRD verification, include combined date/locale/existing-prefix cases, and track reproducible gaps |
| TG-005 | Historical v0.2 approval confirmed on 2026-09-11; v0.3 is an unsigned alignment revision; execution/release arrangements remain unknown | Entire TRD; PRD Q-004 | Review of updated text is separate from unchanged technical blockers and release arrangements | Historical approver is @justinyoo; execution/release roles remain unassigned. Obtain revision sign-off separately from scheduling verification or release |

Within-process watcher serialization is defined; cross-process coordination, transactional rebuilds, plugin side-effect idempotency, and guaranteed source snapshots are not added. A future requirement for those behaviors must go through the PRD rather than appearing as an implicit locking/retry design here.

### Decisions and material changes

| Date | Decision / interpretation | Basis | Effect |
| --- | --- | --- | --- |
| 2026-09-11 | Use PRD v0.5, Review-ready, for the current vNext baseline | User's requested sources and current PRD | Preserve FR/NFR IDs, confirmed quality choices, and product exclusions |
| 2026-09-11 | Keep V-001 through V-007 in the next phase | Explicit user decision recorded in PRD v0.5 | Define verification methods without executing them or inventing results/owners/dates |
| 2026-09-11 | Separate contract obligations, current implementation, and evidence gaps | Source/PRD synthesis; not an additional product approval | Avoid treating historical suggestions, partial protections, or proposed hardening designs as settled implementation facts |
| 2026-09-11 | Record approval of both documents; issue signed TRD v0.2 against approved PRD v0.6 | Explicit approval by @justinyoo | Preserve all technical obligations and verification deferrals; resolve document sign-off, retain TG-001 through TG-004, and separate remaining release arrangements in TG-005 |
| 2026-09-13 | Align TRD v0.3 with PRD v0.7 and code `1f963ad` | User update request; merged #86/#87 and source/reference inspection | Preserve 20 IDs; expand navigation, directory-index, shared-helper, rendering/client, migration and evidence contracts without claiming new sign-off or resolving scheme/mount gaps |

### Readiness assessment

- **Supported status:** Review-ready against PRD v0.7. Historical v0.2 approval remains recorded, while v0.3 is not separately signed off. TG-001 still limits new URL-validation implementation; status is not held back merely because verification has not run.
- **Material limitations:** TG-001 limits new URL-rule implementation, TG-002 limits reproducible contrast acceptance, and TG-003 blocks benchmark execution until inputs exist. TG-004 is deferred evidence/fix work, not an unresolved product decision.
- **Approval:** Historical v0.2 sign-off accepts its obligations and deferrals. The user authorized this alignment on 2026-09-13, not new sign-off, TG-001/TG-002 resolution, passing verification, or release.
- **Reviewer pass:** Reconciled PRD v0.7, TDD v0.4, current code and the website handoff; retained all 20 technical IDs and updated product/evidence mappings. Distinguished shared formatting and component regression intent from scheme safety, browser behavior, and actual prefix serving.
- **Review limitations:** This revision uses repository source, current guides, selected regression source, and recorded historical decisions. No external research, independent audit, runtime/browser tests, or benchmark was performed. TDD v0.4 is the companion design, not a verification-results artifact.

## 7. Reference map

| Topic | Sources used |
| --- | --- |
| Product baseline and origin | [PRD.md](PRD.md), [Original discussion (archived)](https://github.com/getscissorhands/Scissorhands.NET/blob/464ce0f3454d473d4a39bc6f5c9005e86cd5396a/DISCUSSIONS.md) |
| Public usage and migration | [Root guide](README.md), [Core](src\ScissorHands.Core\README.md), [Plugin](src\ScissorHands.Plugin\README.md), [Theme](src\ScissorHands.Theme\README.md), [Web](src\ScissorHands.Web\README.md) |
| Runtime and content | [Application](src\ScissorHands.Web\ScissorHandsApplication.cs), [loader](src\ScissorHands.Web\Loaders\ContentLoader.cs), [generator](src\ScissorHands.Web\Generators\StaticSiteGenerator.cs), [renderer](src\ScissorHands.Web\Renderers\ComponentRenderer.cs), [watcher](src\ScissorHands.Web\Watchers\ContentWatcher.cs) |
| Model and configuration contracts | [ContentDocument](src\ScissorHands.Core\Models\ContentDocument.cs), [ContentMetadata](src\ScissorHands.Core\Models\ContentMetadata.cs), [SiteManifest](src\ScissorHands.Core\Manifests\SiteManifest.cs), [ThemeManifest](src\ScissorHands.Core\Manifests\ThemeManifest.cs), [PluginManifest](src\ScissorHands.Core\Manifests\PluginManifest.cs) |
| Navigation, URLs, and design | [NavigationNode](src\ScissorHands.Core\Models\NavigationNode.cs), [builder](src\ScissorHands.Web\Navigation\NavigationTreeBuilder.cs), [ContentUrlHelper](src\ScissorHands.Core\Urls\ContentUrlHelper.cs), [website handoff](docs\website-documentation.md), [TDD v0.4](TDD.md) |
| Extension implementation evidence | [Plugin runner](src\ScissorHands.Web\Runners\PluginRunner.cs), [dependency resolver](src\ScissorHands.Web\Runners\PluginDependencyResolver.cs), [theme resolver](src\ScissorHands.Web\Services\ThemeComponentResolver.cs), [theme service](src\ScissorHands.Web\Services\ThemeService.cs) |
| Standards and verification conventions | [AGENTS.md](AGENTS.md), [SDK configuration](global.json), [build settings](Directory.Build.props), [CI](.github\workflows\main.yaml), [sample guide](samples\ScissorHands.Sample\README.md) |
