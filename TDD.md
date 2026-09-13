# ScissorHands.NET - Technical design document

## Document control

| Field | Value |
| --- | --- |
| Document version | 0.4 |
| Status | Review-ready |
| Last updated | 2026-09-13 |
| Baselines consulted | 2026-09-13 |
| Product owner | @justinyoo |
| Design approver | @justinyoo |
| Implementation owners | Not assigned |
| Sign-off | v0.3 approved by @justinyoo on 2026-09-12; v0.4 alignment revision not separately signed off |
| Approval scope | Historical approval covers v0.3 design, choices and gaps; the requested alignment is not new sign-off, technical-gap resolution, passing verification, or implementation/release authorization |
| Decision confirmation | @justinyoo confirmed DEC-001's per-write ownership approach on 2026-09-11 and DEC-002's prefix-aware static serving on 2026-09-12 |
| Intended audience | Engine, theme, and plugin contributors reviewing implementation mechanisms and compatibility |
| Release scope | Current vNext baseline including merged #86/#87; no speculative capabilities |
| PRD baseline | [PRD.md](PRD.md) v0.7, Review-ready alignment; historical v0.6 approval retained |
| TRD baseline | [TRD.md](TRD.md) v0.3, Review-ready alignment; historical v0.2 approval retained and TG-001 unresolved |
| Implementation baseline | `getscissorhands/Scissorhands.NET` at `1f963adfd5a1e807f9adca901043d9c72c17d951`; source inspection, not executed validation |

**Authority:** the PRD owns product scope and acceptance; the TRD owns technical obligations; this TDD explains the corresponding design. v0.4 aligns merged behavior at the user's request without extending historical v0.3 sign-off to new text. Existing DEC-001/002 confirmations stand, but URL-policy and integration gaps remain unresolved. V-001 through V-007 remain open verification areas; no execution results or authority to implement, experiment, install, publish, or deploy are supplied by this update.

## 1. Design context and scope

Retain the four-package, local .NET application: Core defines shared models/contracts; Plugin and Theme extend Core; Web composes loading, transformations, rendering, filesystem output, and preview. Visitors receive static files, not a server-interactive Blazor application. Existing interfaces and the owner-controlled executable-extension boundary are the starting point, not a reason to replace the engine.

The target is the existing architecture with focused corrections where needed to satisfy the governing obligations. This document separates **Confirmed** constraints/current mechanisms, **Proposed** changes, and **Unknown** details or feasibility. Current navigation and URL mechanisms are source-backed, not new design proposals; existing hardening/mount proposals remain distinct. A confirmed mechanism is not a claim that it satisfies every associated requirement.

| Driver / constraint | Source and status | Design consequence |
| --- | --- | --- |
| .NET 10, nullable references, warnings-as-errors, current package direction | Confirmed: [SDK](global.json), [build settings](Directory.Build.props), [repository rules](AGENTS.md), TRD CD-001/002 | Keep the existing solution and public boundaries; no new runtime, database, service, or package dependency |
| Markdig, YamlDotNet, Scrutor, System.Reactive, IO abstractions | Confirmed declarations in [Web project](src\ScissorHands.Web\ScissorHands.Web.csproj) and [central versions](Directory.Packages.props) | Reuse parsing, DI discovery, event serialization, and filesystem seams. Floating version declarations are not a record of resolved package versions |
| Owner-controlled builds with deliberately trusted executable extensions | Confirmed: PRD NFR-001/010, TR-009/015 | Validate data and engine-managed I/O; do not claim to sandbox arbitrary code running with process privileges |
| Fixed stage ordering and existing public compatibility | Confirmed: TR-004/010/011 and package guides | Preserve returned plugin values, ordinal stage order, seven theme roles, collection snapshots, and obsolete adapters |
| Non-atomic generation and next-phase evidence | Confirmed: PRD Sections 4/7, TR-012/016/019/020 | Do not introduce full-site buffering, rollback, a graceful-shutdown deadline, an incremental guarantee, or an SLA as implicit requirements |

**Non-goals:** hosted CMS/accounts, a database, managed publishing, arbitrary Razor-route discovery, new feeds/search/example plugins, draft preview, scheduling, automatic browser reload, an extension sandbox, and offline guarantees remain excluded. The original design assessment found no standalone ADR or preceding TDD; this revision preserves its local decision records. No external design records or owner-blog benchmark dataset were supplied.

**Source limits:** the current PRD/TRD, #86/#87 diffs, Core models/helpers, engine navigation/renderer/loader, package guides, [website handoff](docs\website-documentation.md), built-in presentation/client assets, and selected regression source inform this alignment. Unchanged mechanisms retain their earlier source references. This is not a complete sink inventory, security audit, browser assessment, or passing test record; no external research was performed.

## 2. Design baseline

### Architecture and responsibilities

Design IDs identify mechanisms, not new public APIs. Existing `ContentUrlHelper`, `NavigationNode`, and `NavigationTreeBuilder` declarations are current contracts; proposed containment helpers below describe responsibilities, not finalized signatures.

| ID | Component / mechanism | Responsibility and owned state | Interfaces / dependencies | Requirements and state |
| --- | --- | --- | --- | --- |
| DES-001 | Application composition and mode host | Own application configuration, selected theme component set, mode, and output lifecycle | Builder, application, DI extensions, `IAppPaths` | TR-001/007/015/020; Confirmed existing composition |
| DES-002 | Content loading and route composition | Produce the document list; parse metadata; exclude drafts; derive date/locale routes | `IContentLoader`, IO abstractions, YamlDotNet, Core models | TR-002/003/011; Confirmed current mechanism, proposed composition correction where needed |
| DES-003 | Plugin registry and stage execution | Snapshot installed/configured plugins and stage dependency orders; propagate hook results sequentially | `IPluginRunner`, `IContentPlugin`, dependency resolver, shared ID validator | TR-004/009/010/015/016; Confirmed existing mechanism |
| DES-004 | Generation orchestration and view data | Own generation, collection selection, navigation snapshot/tree, original route preflight, render order and output requests | `IStaticSiteGenerator`, loader, navigation builder, Markdown service, renderer, theme service | TR-003/004/006/020; Current orchestration/navigation confirmed; final-write ownership proposed |
| DES-005 | Static Razor rendering | Scope/renderer per render; separate layout-only navigation from view/cascading context; adapt flat-only callers | `IComponentRenderer`, `HtmlRenderer`, Theme base components | TR-005/006/011/014/016; Current rendering/compatibility confirmed; broader URL policy unresolved |
| DES-006 | Theme resolution and assets | Resolve component roles and manifest; select/copy the supported asset inventory | Theme resolver/service, assembly catalog, IO abstractions | TR-007/008/011/013; Confirmed discovery/copy surfaces, proposed guarded copying |
| DES-007 | Root containment and output ownership | Share path checks; track file ownership for a single build before engine writes/copies | Internal Web helpers using existing IO seams; explicit per-build context | TR-003/008/013/015/020; Per-write approach confirmed in DEC-001; integration/feasibility remain DQ-002 |
| DES-008 | Preview serving and regeneration | Own the static-file mount, watcher subscription, serialized rebuild callback, and shutdown lifetime | ASP.NET Core static-file middleware, watcher factory, Rx | TR-012/016/017/020; Existing watcher mechanism and DEC-002 mount design confirmed; mount implementation remains pending |
| DES-009 | Built-in presentation and URL emitters | Render prepared hierarchy/metadata/tag links; keep raw content separate; own disclosure and theme preference behavior | Core URL helper, Theme wrappers, Razor views, CSS/JS | TR-006/014/017/018; Current formatting/presentation confirmed; broader scheme policy remains TG-001 |
| DES-010 | Compatibility and failure boundaries | Preserve existing public contracts and distinguish failures/cancellation from completion without exposing sensitive payloads | Core interfaces/models, application, runner, renderer, logging | TR-011/015/016/020; Confirmed obligations, proposed focused corrections |
| DES-011 | Resource model and verification instrumentation | Explain cost/latency drivers and collect later evidence without production telemetry | Existing logs, tests/sample, external measurement tools when authorized | TR-019 and V-001 through V-007; Proposed measurement approach, no executed results |

### DES-001 / DES-004: Startup and generation flow

[Builder](src\ScissorHands.Web\ScissorHandsApplicationBuilder.cs) creates a `WebApplication`, binds `Site`/`Plugins`, registers services and Razor components, and retains an optional explicit `ThemeComponentSet`. [DI registration](src\ScissorHands.Web\Extensions\ServiceCollectionExtensions.cs) uses singleton engine services and plugin implementations; the renderer creates child scopes for rendering. Site runtime properties such as `IsPreview`, `SiteUrl`, and `DescriptionInHtml` are mutable, so this is not an isolated multi-tenant or concurrently reconfigurable host.

[Application](src\ScissorHands.Web\ScissorHandsApplication.cs) validates the requested mode before resolving the generator/theme set in `RunAsync`. Application construction and assembly discovery have already occurred; help is not described as an extension-free sandbox. Retain the current mode precedence, explicit help/error behavior, cached reflective invocation of the seven-generic-parameter generator method, and explicit `AddLayouts` overrides.

The [generator](src\ScissorHands.Web\Generators\StaticSiteGenerator.cs) currently runs this sequence:

1. Create the destination, set `IsPreview`, convert site description Markdown, load the theme manifest, and load the document list.
2. Preflight original page routes, index, 404, and planned tag destinations; select visible navigation pages and build one hierarchy before document hooks.
3. Render/write the index, then the source-backed or synthetic 404.
4. Process/render/write ordinary documents sequentially, then render/write tag surfaces.
5. Copy content images and supported theme assets. Return only after the invocation's work completes.

For each source-backed document, run pre-Markdown, convert Markdown into `Html`, run post-Markdown, render the selected view/layout, run post-HTML, and write UTF-8 HTML. Synthetic index/tag/default-404 documents receive post-HTML only. The source-backed 404 follows the Markdown stages, is written to the fixed `404.html` path, and is excluded from ordinary page rendering and displayed tag collections. Ordinary document paths are selected from the post-Markdown document before post-HTML, which returns HTML rather than a replacement route. Keep this ordering under DEC-001; do not pre-run hooks twice for planning or reorder them merely to discover destinations.

**Confirmed direction (DEC-001):** retain original-route preflight and claim each actual output destination through DES-007 before directory creation or file replacement. A later collision can fail after earlier pages were written, but must fail before overwriting the conflicting owner's file. This preserves the documented partial-output model rather than inventing an all-or-nothing build. Platform-safe I/O and compatible custom-service integration still need resolution; confirmation of the direction does not establish feasibility or authorize implementation.

### DES-002 / DES-004: Content, routing, and collection state

[ContentLoader](src\ScissorHands.Web\Loaders\ContentLoader.cs) reads posts then pages recursively, splits optional frontmatter, validates supported fields, parses dates with invariant culture, applies route settings, and removes drafts before returning documents. Missing optional roots warn; malformed content fails. There is no future-date withholding or preview-only draft path.

`ReadBooleanMetadata` parses both `draft` and `show_in_navigation`, defaulting missing fields to false and rejecting invalid values with source/field context. The latter populates `ContentMetadata.ShowInNavigation`, not a generation permission. `InferSlugFromFile` collapses only nested page files named `index` (case-insensitively) to their containing directory when the slug is omitted/blank. Explicit slugs, root page `index`, and post inference retain their behavior; locale/date composition follows inference.

| State / artifact | Ownership and lifecycle | Design boundary |
| --- | --- | --- |
| Markdown, frontmatter, and configuration | Owner-managed files; read for generation, not rewritten | Validate before use; never infer permission to read unrelated paths |
| `ContentDocument` / `ContentMetadata` | Loaded per build; metadata is a record, document `Html` is mutable, hooks can return replacement documents | Assign every returned value. Do not describe all models as deeply immutable |
| `NavigationPages` / `NavigationTree` | Build-local flat read-only list and immutable hierarchy shared across layout renders | Flat entries reference loaded documents; tree records snapshot titles/escaped paths/URLs/children before document hooks. Groups are not documents or routes to generate |
| Theme assets and manifest | Local selected theme root or application-bundled fallback; manifest loaded per build | Preserve selected-root precedence and supported inventory; do not download external references |
| Plugin manifests and dependency orders | Singleton runner snapshots at construction | Configuration/dependency changes require reconstructing the host/runner; no live reconfiguration is introduced |
| Output registry in DES-007 | Proposed ephemeral state for one build | Not a persistent catalog, cleanup manifest, source snapshot, cross-process lock, or retry journal |
| `dist` / `preview` | Generated files remain until overwritten or removed | Initial CLI cleanup remains intentional; rebuilds can retain stale output |
| Browser preference | Existing localStorage `theme` key | No server account, telemetry, or new storage-disabled fallback |

Retain ordinary `<route>\index.html`, root index, and fixed `404.html` destinations. Separate route text using `/` from filesystem paths using platform APIs. Existing `NormalizeRoute`, `ResolveOutputPath`, and route collision checks are starting points to extract/reuse, not proof that links or all path aliases are safe.

**Proposed route composition:** when locale prefixing is enabled, identify an existing matching leading locale in the original slug before composing the post-date prefix; form a single effective locale prefix around the remaining route. Preserve the 404 exception, page-versus-post distinction, and missing-date warning. The current loader adds date first, then checks for a locale prefix, so combined date/locale/already-prefixed inputs need particular attention. Record exact compatibility cases in V-007 before a correction; no runtime result is asserted here.

Index posts remain newest-first, undated last. Tag data groups lowercased names with encoded tag-route text, ordering posts by date and pages by title. Reuse the same eligible-content selection for tag planning and rendering: current preflight considers tags on the custom 404 while actual tag rendering excludes it. That source difference is a focused V-001/V-007 reconciliation case, not justification to change the 404 contract.

**Collection mutation boundary:** current collection views use the loaded list, while an ordinary document's own renderer receives a replacement returned by hooks. The generator does not automatically replace that entry throughout previously rendered collections. Whether route-changing plugins require updated collection links is an unresolved interaction between TR-004 and TR-017 (DQ-006); this TDD does not silently prohibit metadata changes or promise a two-pass renderer.

### DES-004: Navigation selection and hierarchy

The generator records normalized routes of loaded hidden pages, then selects non-draft, non-404 pages with `ShowInNavigation`. It rejects candidates with a hidden ancestor using ordinal comparisons of complete slash-separated route prefixes, not text-prefix matching. Drafts are already absent from the normal loader result; they are not retained as hidden-parent markers. The flat list sorts by ordinal title then slug and is wrapped read-only.

[NavigationTreeBuilder](src\ScissorHands.Web\Navigation\NavigationTreeBuilder.cs) accepts that visibility-filtered list, formats each route through `ContentUrlHelper.GetContentUrl`, and adds missing ancestors as null-URL groups. Labels decode the final path segment, replace hyphens/underscores with spaces, and apply invariant title casing. A real visible parent retains its title/link; a hidden existing parent has already removed the branch. When locale routing is enabled, remove synthesized effective-locale nodes, not actual visible locale landing pages. Parent lookup uses escaped paths and siblings sort by ordinal title then path. With no visible descendants, there is nothing from which to synthesize an empty group.

The builder returns read-only lists of Core [NavigationNode](src\ScissorHands.Core\Models\NavigationNode.cs) records; each node snapshots `Children` and carries only `Title`, `Path`, nullable `Url`, and children, with no HTML/CSS/DOM identifiers. It checks cancellation at entry and during construction; invalid routes fail through the shared helper rather than becoming placeholder links. Groups never participate in output planning or asset ownership.

One `NavigationContext` carries the flat list and tree into `CreateBaseParameters` for index, ordinary documents, custom/synthetic 404, and tag layouts. Themes render it rather than rebuilding hierarchy. Construction precedes pre-/post-Markdown hooks, so later replacement metadata does not refresh navigation titles, URLs, or membership. Preserve that snapshot boundary while recording the wider post-plugin collection-link question in DQ-006; no new pipeline pass is implied.

### DES-003: Extension ordering and execution

[PluginRunner](src\ScissorHands.Web\Runners\PluginRunner.cs) builds an ordinal manifest lookup, validates every installed implementation's ID/display-name contract, rejects duplicate/unmatched configuration, and enables only IDs with manifests. It delegates stage ordering to [PluginDependencyResolver](src\ScissorHands.Web\Runners\PluginDependencyResolver.cs).

The resolver snapshots enabled plugins' declarations, checks identity/stage/self/missing/disabled dependency cases, and performs a separate topological sort for each stage. A sorted ready set provides ordinal-ID tie-breaking; an unresolved remainder reports a cycle. Keep the resulting lists cached in the runner and await hooks sequentially, replacing the current document or HTML after each hook. Declaration order and registration order are not scheduling inputs.

The assembly catalog discovers application assemblies, including installed assemblies in the application output directory. Manifest absence disables hooks, not assembly discovery or every possible constructor effect. This remains deliberately trusted executable code, not a permission broker. Do not load assemblies named by content or introduce automatic installation. Dependencies remain author-declared, stage-local, and explicitly enabled by the owner.

### DES-005 / DES-009: Rendering, encoding, and URLs

[ComponentRenderer](src\ScissorHands.Web\Renderers\ComponentRenderer.cs) creates a DI scope and `HtmlRenderer`, builds a layout `Body` fragment, and filters cascading-only values from ordinary view attributes. It invokes rendering on the renderer dispatcher, awaits the root, and returns `ToHtmlString()`. [CascadingMainLayoutBase](src\ScissorHands.Theme\CascadingMainLayoutBase.razor) supplies documents, tag collections, manifests, and site context. Preserve this separation and disposal, rather than crawling routes or launching a browser to generate pages.

`NavigationPages` and `NavigationTree` are explicit layout parameters excluded from content-view attributes and not added to automatic cascades. Both default to empty lists on `MainLayoutBase`. For a layout derived from that base, `ComponentRenderer` builds a tree only if the parameter dictionary lacks a tree and supplies an `IReadOnlyList<ContentDocument>` flat list, forwarding site/token context. An explicitly supplied tree is retained. Direct component rendering has no such adapter and should supply the prepared tree.

The [layout base](src\ScissorHands.Theme\MainLayoutBase.cs) derives title/description/locale from document values with site fallbacks. [MarkdownService](src\ScissorHands.Web\Services\MarkdownService.cs) retains its existing Markdig pipeline; the optional trim removes an enclosing paragraph only for a single paragraph block.

The following is a **partial source inventory**, not TG-001's completed scheme policy:

| Observed context | Current mechanism | Proposed treatment / unresolved detail |
| --- | --- | --- |
| Page title, description meta attribute, locale, index title/description text | Normal Razor interpolation | Preserve encoding; validate the actual sink rather than pre-encoding text and causing double encoding |
| Layout `<base href>` | `Site.BaseUrl` interpolated into an attribute | URL validation is separate from HTML encoding; supported forms and failure cases belong to TG-001 |
| Theme stylesheet/script/icon references | Protected layout `GetThemeUrl` delegates to Core `ContentUrlHelper.GetThemeUrl` | Preserve theme-local concatenation and exception behavior; not arbitrary external stylesheet/script support or a sanitizer |
| Post hero image | Protected `GetImageUrl` strips leading forward slashes only via Core | Preserve HTTP(S), query/fragment and existing encoding; do not apply content-slug escaping or imply scheme validation |
| Post/index/tag links and prepared navigation | Shared content segment escaping and normalized tag URLs; tree URLs are already formatted | Keep base-relative formatting separate from scheme policy; render group `Title` as text and do not link its `Path` |
| Post/page tag lists | `GetTagUrl` wrappers and normal Razor interpolation | Share trim/lowercase/one-segment escaping with engine tag destinations; invalid empty/dot tags fail |
| Site introduction | `DescriptionInHtml` rendered as `MarkupString` | Intentional Markdown-derived HTML, even though its source is a site setting; not equivalent to the plain description meta attribute |
| Document body and post-HTML plugin output | Intentional raw rendered HTML | Preserve the explicit trust boundary; no blanket sanitizer, arbitrary HTML rewrite, or universal safety guarantee for custom themes |

**Recommendation:** after TG-001 is resolved in the TRD, implement context-specific checks at the responsible built-in URL emitters and relevant engine input boundaries, reusing local helpers instead of creating a general HTML rewriting layer. Cover post-plugin values where that boundary consumes them. Do not invent an allowlist, reinterpret all Markdown as unsafe text, or change external-asset compatibility in this document.

**Current helper integration:** Core owns `ContentUrlHelper`; the loader reuses locale normalization and the generator reuses tag route formatting, wrapping invalid tag-route arguments as contextual `InvalidDataException`. Theme base wrappers expose the applicable content/theme/image/tag operations to derived layouts/views. TR-017 and the [helper reference](docs\website-documentation.md#shared-url-helpers) specify exact input/output rules. Helpers do not prepend `Site.BaseUrl`; the layout's `<base>` resolves relative URLs. This reuse neither supplies the missing full scheme policy nor implements DEC-002's preview mount.

### DES-006 / DES-007: Theme assets, containment, and ownership

[ThemeComponentResolver](src\ScissorHands.Web\Services\ThemeComponentResolver.cs) groups concrete components by namespace, requires exactly one component for each of the seven roles including both tag views, and selects a unique normalized suffix match. Missing or ambiguous roles fail without implicit built-in tag-view substitution. Keep explicit seven-role overrides and the default-theme path; an unmatched custom theme remains an error.

[ThemeService](src\ScissorHands.Web\Services\ThemeService.cs) prefers the configured local theme directory, otherwise an application-output theme directory. It deserializes case-insensitive JSON, validates non-empty name/slug and configured-slug agreement, then copies `assets` recursively plus the currently supported image extensions and `manifest.json` under `themes\<slug>`. The generator separately copies `contents\images`. Preserve the declared supported surfaces rather than exporting arbitrary `wwwroot` or plugin files.

**Proposed containment mechanism:**

1. Anchor trusted roots from application paths and the caller's destination; content cannot redefine those anchors. Keep local-theme and bundled-theme roots distinct instead of broadening to an arbitrary common ancestor.
2. Reuse syntactic route checks, then resolve filesystem paths under the intended root. Validate root boundaries with platform-correct filesystem semantics; contractual case-insensitive output collision checks are a separate rule.
3. Inspect every traversed directory/file link, including output-root and ancestor links before cleanup or directory creation. Detect cycles, broken/unresolvable targets, and escape targets; do not follow an escape and validate after reading/copying.
4. Claim a destination and its file-versus-directory relationships before the engine creates directories, copies, or writes. Include actual post-Markdown paths, fixed generated routes, content images, and theme assets.
5. Bind validation to the I/O operation closely enough to satisfy containment. A `GetFullPath` check followed by an ordinary path-based open is not proof against a link swap. Platform link/handle behavior and IO-abstraction support are design-critical evidence still missing in DQ-002.

The proposed per-build registry records a stable engine-owned source/asset identity and destination, not a mutable plugin-supplied display name. A second distinct owner or a file/parent-directory conflict fails. Repeated enumeration of the **same** theme source/destination is deduplicated: the current theme copier can encounter an image both under `assets` and through its extension scan. Do not mistake that for two independent owners or silently permit two different sources to overwrite each other.

The registry is fresh on each regeneration and permits intentional regeneration within the dedicated output tree. It does not delete stale files, recover the previous build, or provide cross-process coordination. A shared stateless path helper and explicit build-local ownership context are preferable to a singleton mutable dictionary that leaks ownership across rebuilds.

**Compatibility integration remains open:** built-in page/content/theme writers can share internal helpers, but the public `IThemeService.CopyAssetsAsync` contract accepts only slug, destination, and token; it exposes neither an inventory nor an ownership context. Preserve that interface, its obsolete overloads, and existing public constructors. The design needs a compatible integration path for custom services before claiming comprehensive asset ownership protection. An internal built-in-only overload is an option, not complete coverage for arbitrary replacements; a post-copy scan cannot prevent an overwrite that already happened. DQ-002 records this blocker rather than silently bypassing custom implementations.

### DES-008: Preview mount and event lifecycle

Current preview startup clears/creates its destination, attaches `PhysicalFileProvider` to default/static-file middleware, starts the server, sets the effective `SiteUrl` from its listening address, and then performs the first build. It installs the watcher only after that build succeeds and stops the server in `finally`. The server may therefore be listening during initial generation; no atomic visibility or initial-readiness barrier is promised.

[ContentWatcher](src\ScissorHands.Web\Watchers\ContentWatcher.cs) creates missing watched roots and observes recursive create/change/delete/rename events. Rx `Throttle` coalesces bursts; asynchronous callbacks are serialized with `Concat`. A failed callback is logged so future events can still rebuild. Disposal removes subscriptions/watchers; application stopping supplies the preview generation token.

Keep the current 500 ms debounce as an implementation setting, not an SLA. Serialization is not bounded queueing, exactly-once delivery, source snapshots, or cross-process coordination. Continuous changes may delay a throttled notification; queued rebuilds and slow plugins may increase latency. Browser refresh stays manual; changing C#/Razor still requires compilation, and configured component/plugin choices require host reconstruction.

**Confirmed mount design (DEC-002):** keep files directly under `preview`, but map the configured site path prefix to that root before default/static-file processing, using ASP.NET Core path-base handling for supported path-prefix configurations. `/docs/post/` should reach the same artifact-relative `post\index.html` that root hosting would serve. Do not generate a second physical `docs` directory or rewrite unrelated external URLs. Retain root behavior for `/`; URL forms outside an agreed path-prefix shape need TG-001 clarification rather than guessed coercion.

The current middleware has no corresponding configured prefix handling even though rendered output uses `BaseUrl` and the logged preview URL includes it. V-003 must exercise actual requests, not merely inspect the `<base>` element.

### DES-009 / DES-010: Client behavior, diagnostics, and compatibility

The built-in layout recursively renders `NavigationTree`: page nodes are anchors, groups are text, and nodes with children receive a separate labelled native button with `aria-controls`/`aria-expanded`. Child lists are visible and buttons hidden in static markup. [Theme JavaScript](src\ScissorHands.Web\themes\default\assets\theme.js) initializes collapsed lists and reveals controls, preserving a readable no-JavaScript hierarchy without recomputing frontmatter visibility.

Native button activation handles mouse/touch/Enter/Space. Toggle handlers close siblings and reset descendants when collapsing; Escape closes the applicable group and focuses its button. Focus leaving navigation, window blur, and outside clicks close menus. Retain CSS focus styling and responsive behavior. Post/page views render tag links through the shared tag helper. The independent theme-preference handler updates `data-theme`, accessible labels, and localStorage, following system preference when no stored choice exists. No Blazor client runtime is needed; source affordances are not proof of browser or accessibility coverage.

For TG-002, the proposed evidence method is to record rendered foreground/background pairs, affected text/control states, actual browser conditions, and an owner/reviewer assessment for both themes; contrast ratios may inform that assessment. A new numerical pass threshold or conformance level would change acceptance and must go through the TRD, not appear as an automatic TDD decision. Preserve the agreed desktop/mobile matrix and document/site locale behavior.

Retain public interfaces, navigation/helper compatibility and model snapshots. Current vNext migration includes required plugin IDs/`PluginDependency.PluginId`, read-only manifest collections, all seven theme roles, and nested page directory-index routes. Supply missing tag components and rebuild; use explicit old slugs when preserving nested-index URLs. The sample requires an explicit mode. These are implemented changes recorded here, not additional changes proposed by the design. Arbitrary plugin option values are not deeply frozen; legacy theme-service adapters retain their cancellation check.

Production CLI generation currently receives `CancellationToken.None`; preview receives `ApplicationStopping`. The renderer checks cancellation before rendering, and synchronous Markdown conversion/copying and third-party hook internals do not gain automatic interruptibility. Proposed cancellation checks between engine-controlled operations must preserve those limits and avoid a completed-build message after observed cancellation; no bounded graceful Ctrl+C behavior is added.

| Boundary | Retained / proposed failure mechanism | Artifact and recovery meaning |
| --- | --- | --- |
| Invalid metadata, identities/dependencies, theme selection/manifest | Fail with source/field/ID/stage context; preserve documented optional-directory warnings | Correct input/configuration and rerun; do not choose an unrelated custom theme or enable plugins silently |
| Traversal, links, or owner conflict | Proposed DES-007 rejection before affected engine I/O | Earlier output may remain; an unsafe copy/write must not be reported as successful |
| Hook, render, copy, or write failure | Propagate failure; add bounded contextual diagnostics where existing errors lack it | No completed-build/rebuild message; do not retry side-effectful hooks automatically |
| Observed cancellation | Propagate `OperationCanceledException` at supported boundaries and forward the original token | Partial output allowed; no new timeout or arbitrary interruption guarantee |
| Preview callback failure | Existing watcher logs and permits later events | Browser may still see previous/partial/stale files; subsequent successful rebuild or restart is recovery, not rollback |
| Production completion | Log the output location only after the entire generation task completes | Artifact is only a publication candidate; deployment is separately authorized |

**Proposed completion correction:** reject a null task from reflective generator invocation instead of the application's current `Task.CompletedTask` fallback. Preserve unwrapped invocation failures and cancellation; do not change public method signatures. Review renderer metadata-discovery fallback diagnostics without representing missing cascading context as confirmed successful discovery.

Logs should carry bounded identifiers and context, not content bodies, whole configuration objects, option dictionaries, or unnecessary exception payloads containing sensitive data. Verification uses synthetic markers outside intended inputs. This is not a universal secret scanner, a suppression of legitimate authored content, or a claim to constrain arbitrary trusted extension code.

### DES-011: Resource model and measurement

The loader materializes source documents; collection views sort/group that list; rendering creates a scope and renderer per page and writes pages sequentially. Navigation selection and ancestor/tree construction run once per generation, retaining node/path metadata shared by layout renders; only compatibility callers supplying no tree rebuild it in the renderer. Plugin graphs are resolved at runner construction. These mechanisms avoid a new database/cache but still retain source strings/content and incur per-page rendering/extension costs; no measured speedup is claimed.

The current route preflight scans existing route entries for ancestor conflicts, so route count can increase validation cost faster than a simple lookup. Asset traversal and synchronous copying add file-count/byte-volume cost; plugins can dominate runtime or perform external work. Do not claim a particular bottleneck, speed improvement, or supported maximum from inspection alone. A proposed ownership registry adds path/owner metadata, not a full rendered-site buffer.

V-006 should record the actual blog snapshot, extension configuration, machine/OS/SDK and resolved package versions, then distinguish compilation/startup, generation duration, event-to-updated-preview delay, and the selected process-memory metric. State whether debounce, asset copies, and renderer/plugin work are included. Record units and observed runs without inventing a sample count, percentile, or pass threshold. No telemetry backend, benchmark package, cloud service, or cost figure is introduced.

### Coverage disposition

| Design area | Applicability / reason | Design or gap references |
| --- | --- | --- |
| Boundaries and responsibilities | Applicable: local owner, engine, executable extensions, output, browser/host | DES-001 through DES-010 |
| Runtime and concurrency | Applicable: sequential hooks, shared service state, asynchronous rendering, watcher events | DES-001/003/004/005/008; no distributed transaction |
| Data and lifecycle | Applicable: file input, in-memory documents, artifacts and browser preference; no database/accounts | DES-002/004/006/007/009 |
| Interfaces and integrations | Applicable: .NET APIs, manifests, filesystem, themes/plugins, static host | DES-001/003/005/006/008/010; DQ-002 |
| Security, privacy, and abuse | Applicable: untrusted data despite trusted executables; no tenant isolation service | DES-003/005/007/009/010; DQ-001/002 |
| Performance and capacity | Applicable: actual-blog workload and event/resource costs | DES-011; DQ-004 |
| Cost and dependencies | Applicable: existing local runtime/tooling and optional external extension costs | DES-001/011; no new paid dependency or sourced cost estimate |
| Reliability and recovery | Applicable: partial output, cancellation, callback recovery; no availability/atomicity target | DES-004/007/008/010 |
| Deployment and operations | Applicable: local generation/preview and static-artifact handoff; managed deployment excluded | DES-001/008/010; DQ-005 |
| Migration and rollback | Applicable: documented API/configuration migration; no engine-managed artifact rollback | DES-010 and Section 4 |
| Clients, accessibility, localization | Applicable: native navigation, theme preference, locale metadata/URLs | DES-002/009; DQ-001/003 |
| AI and automation | AI not applicable; deterministic plugin/watcher automation applies | DES-003/008; no models, retrieval, or agent runtime |
| Domain-specific mechanisms | Static-file routing/asset ownership apply; payments, hardware safety, regulated workflows, and offline synchronization excluded | DES-004/006/007/008 |
| Verification and feasibility | Applicable; future release checks differ from design-critical platform/API evidence | DES-011, Section 4, DQ-002 |
| Decisions and evolution | Applicable: compatibility-sensitive hardening and mount behavior | DEC-001/002; no accepted ADR found in the inspected Markdown sources |

## 3. Alternatives and decisions

### DEC-001: Incremental destination claims versus whole-site output planning

- **Context:** TR-003/004/008/013/020 require ownership protection without introducing atomic output or changing the fixed transformation stages.
- **State / confirmation:** Confirmed direction by @justinyoo on 2026-09-11, selecting the existing pipeline with per-write ownership checks. DQ-002 remains unresolved; full document approval is recorded separately in Document control.
- **Option A - retain the current pipeline and add per-write claims (chosen):** keep original-route preflight, then validate/claim actual destinations before writes/copies. Share path checks and build-local ownership state. This catches final-route/asset conflicts before their conflicting writes without storing every rendered page.
- **Option B - prepare a complete output plan before writing:** render/collect final page outputs and enumerate assets first, then validate the complete plan. This can find more conflicts before page writes but adds memory/storage and integration work; extensions that inspect intermediate output or custom asset services need compatibility analysis. Planning alone is not an atomic publication or rollback mechanism.
- **Rationale / consequences:** Option A fits the existing partial-output contract and minimizes changes to hook timing and memory use. Failures can still occur after earlier output is written. Both options require link-safe I/O and a compatible theme-service integration; neither settles DQ-002 by itself.
- **ADR relationship:** No applicable ADR found. Candidate for a later ADR if adopted because it establishes a reusable ownership boundary across engine writers; no ADR is created.

### DEC-002: Mount preview at the base path rather than duplicate the artifact

- **Context:** TR-017 requires actual root/subpath requests to resolve, while TR-003/008 define artifact-relative destinations.
- **State / confirmation:** Confirmed by @justinyoo on 2026-09-12, selecting Option A. This settles the mount mechanism, not unspecified URL rules or implementation evidence. Full document approval is recorded separately in Document control.
- **Option A - prefix-aware static serving (chosen):** place path-base handling before default/static-file middleware for supported configured prefixes; keep one artifact layout.
- **Option B - duplicate or nest generated output under the prefix:** changes physical layout and couples generation to deployment URLs, increasing the risk of double-prefixes and divergent root/subpath artifacts.
- **Rationale / consequences:** Option A changes the server boundary rather than every generator/theme destination. Retain root-mode behavior and verify directory-index redirects and asset requests. Unsupported `BaseUrl` forms cannot be silently normalized; TG-001 remains authoritative.
- **ADR relationship:** None; a local middleware mechanism does not need a separate ADR unless its contract later expands.

Keeping .NET/Razor, current package direction, explicit plugin identity, and stage order is required by the approved baseline, not a set of speculative alternatives. URL scheme rules remain an upstream technical decision; this TDD does not fabricate a third accepted decision for them.

## 4. Requirement coverage and verification strategy

All mappings use TRD v0.3 against PRD v0.7. **Pending** means this document claims no completed verification for that area; it does not assert that no checks have ever run elsewhere. Read test source establishes intent, not a passing result or full-area completion.

| TRD requirement | Design references | Mechanism / coverage | Verification and evidence state |
| --- | --- | --- | --- |
| TR-001 | DES-001/010 | Existing mode validation and generator invocation; truthful completion | V-004/V-007: argument/process behavior and sample; Pending |
| TR-002 | DES-002 | Frontmatter/model loading including navigation boolean, publication filtering, explicit errors | V-004/V-007: defaults/invalid booleans, missing inputs and both modes; Pending |
| TR-003 | DES-002/004/007 | Directory-index inference, route composition/preflight, proposed final ownership | V-001/V-007: explicit slugs, inferred collisions, combined prefixes, page/assets, 404/tags; Pending; DQ-002/006 |
| TR-004 | DES-003/004 | Sequential stage propagation, replacement documents, synthetic-page distinction | V-007: observable hook results and ordering; Pending; DQ-006 for cross-document route effects |
| TR-005 | DES-005 | Scoped renderer, layout-only navigation, flat-only adapter, cascades, UTF-8 | V-003/V-007: parameter filtering/explicit tree, rendered output and serving; Pending |
| TR-006 | DES-002/004/005/009 | Index/tag/404 data, visibility-filtered hierarchy, immutable tree, pre-hook snapshot | V-001/V-003/V-007: hidden/missing ancestors, groups/order/locale, all layouts and plugin boundary; Pending; DQ-006 |
| TR-007 | DES-001/006 | Existing discovery/overrides and manifest validation/fallback | V-007: complete/missing/ambiguous theme sets, malformed manifests; Pending |
| TR-008 | DES-006/007 | Supported inventory and structure; proposed guarded copying | V-001/V-003/V-007: inventories, aliases, copies, asset requests; Pending; DQ-002 |
| TR-009 | DES-003 | Validated ordinal IDs and manifest-based enablement | V-007: identities, disabled/unmatched plugins, component selectors; Pending |
| TR-010 | DES-003 | Per-stage topological sorting with ordinal ready set | V-007: permutations, invalid declarations, disabled/missing dependencies, cycles; Pending |
| TR-011 | DES-002/005/006/009/010 | Model snapshots, flat-navigation/helper compatibility, obsolete adapters, route/theme migration | V-007: snapshots, consumers, required roles, explicit slugs, legacy compatibility; Pending; DQ-002 |
| TR-012 | DES-008 | Existing throttled, serialized callbacks and logging/disposal | V-004/V-007: real event bursts, overlap, failure/recovery, shutdown; Pending |
| TR-013 | DES-007 | Proposed root/alias checks and pre-I/O claims; custom-service integration unresolved | V-001/V-004: link/ownership/failure cases; Pending; DQ-002 blocks comprehensive implementation readiness |
| TR-014 | DES-005/009 | Razor encoding/raw boundaries; shared formatting with limited checks, broader scheme policy pending | V-002: navigation labels and sink-specific compatible/unsafe cases; Pending; DQ-001 |
| TR-015 | DES-001/003/007/010 | Installed-code trust boundary, bounded data access, contextual diagnostics | V-001/V-002/V-007: synthetic markers and explicit enablement; Pending |
| TR-016 | DES-003/005/008/010 | Token forwarding, observed cancellation, legacy checks, honest completion | V-004/V-007: controlled cancellation at supported boundaries; Pending |
| TR-017 | DES-004/008/009 | Shared base-relative formatting; confirmed but unimplemented preview mount | V-003/V-007: helper/view parity, encoded links/images, actual root/prefix requests; Pending; DQ-001/006 |
| TR-018 | DES-009 | Native disclosures/no-JavaScript hierarchy, page tags, preference and metadata | V-005/V-007: agreed browsers, keyboard/touch/Escape/focus, labels and contrast; Pending; DQ-003 |
| TR-019 | DES-011 | Resource model and reproducible actual-blog measurement boundaries | V-006: workload/environment/results record; Pending; DQ-004 |
| TR-020 | DES-001/004/008/010 | Propagation/logging and documented partial/stale artifact lifecycle | V-004/V-007: success, invalid input, I/O/hooks/render/cancellation failures; Pending |

The TRD already maps all 20 PRD FR/NFR entries. Its retained product outcomes G-001 through G-003 are supported by local static generation and theme/plugin separation, not new telemetry or numerical claims. PRD release criteria, scope exclusions, optional further outcome evaluation, and separate release authority remain unchanged.

### Evidence and rollout boundaries

Reuse existing xUnit v3/Shouldly/NSubstitute/bUnit and IO test seams, with real temporary-filesystem cases where mocks cannot establish link behavior. Selected consulted starting points are [route tests](test\ScissorHands.Web.Tests\Generators\StaticSiteGeneratorRouteTests.cs), [renderer/context tests](test\ScissorHands.Web.Tests\Renderers\ComponentRendererCascadingParametersTests.cs), [legacy adapters](test\ScissorHands.Core.Tests\Services\ThemeServiceCompatibilityTests.cs), and [watcher tests](test\ScissorHands.Web.Tests\Watchers\ContentWatcherTests.cs). The consulted watcher test only checks missing-directory creation; it is not evidence of concurrency/recovery coverage. The subpath renderer test checks markup strings, not served HTTP requests.

Current regression sources add [directory-index cases](test\ScissorHands.Web.Tests\Loaders\ContentLoaderDirectoryIndexTests.cs), [all-surface navigation](test\ScissorHands.Web.Tests\Generators\StaticSiteGeneratorNavigationTests.cs), [tree construction](test\ScissorHands.Web.Tests\Navigation\NavigationTreeBuilderTests.cs), [Core URL contracts](test\ScissorHands.Core.Tests\Urls\ContentUrlHelperTests.cs), [Theme wrapper parity](test\ScissorHands.Theme.Tests\UrlHelperTests.cs), and [default navigation markup](test\ScissorHands.Web.Tests\Themes\DefaultNavigationTests.cs). The all-surface cases inspect generated files and resolved URLs with mocked filesystem dependencies; they do not serve HTTP or establish JavaScript interaction. Retain V-003/V-005 and DQ-006 rather than inferring closure from these tests.

**Design-critical evidence, not yet authorized:** establish link/alias handling and safe I/O behavior on supported operating systems, and prove a custom-theme-service integration can preserve public compatibility while enforcing ownership before writes. A mock-only success or post-copy inspection cannot settle DQ-002. Do not conduct those experiments as part of writing this document.

**Later implementation/release evidence:** retain V-001 through V-007, the [sample](samples\ScissorHands.Sample\README.md), [repository test guidance](AGENTS.md), and the [CI platform matrix](.github\workflows\main.yaml). Start with affected test projects; shared contracts require broader compatibility coverage. Record code/configuration, platform/tool versions, cases, outcomes, and blocked/skipped coverage. No deadline or execution owner is assigned here.

Migration is coordinated application/theme/plugin recompilation with explicit ID/configuration updates, not mixed-version compatibility or a data backfill. Proposed helpers must not remove public constructors/overloads or add mandatory interface members. If a new contract is genuinely necessary, seek TRD authorization and reconcile product compatibility before implementation.

The implemented baseline adds optional `show_in_navigation` frontmatter and changes inferred nested page-index routes; this alignment proposes no further schema change or automated content rewrite. Opt pages in deliberately, preserve old routes with explicit slugs where required, and resolve collisions. Rollback requires matching prior packages, extension assemblies, configuration and compatible frontmatter; an older loader may reject the new field. The generator does not restore output. Host-specific deployed-artifact retention and rollback remain outside this task.

## 5. Risks, unresolved questions, and readiness

| ID | Issue / state and source | Affected references | Impact and blocking distinction | Owner / next action |
| --- | --- | --- | --- | --- |
| DQ-001 | Shared formatting now explicit; complete scheme rules and sink inventory remain unknown under TG-001 | DES-004/005/008/009; TR-014/017 | Blocks new scheme rules and unsupported base-URL decisions, not existing helper reuse; preview mount remains separately unimplemented | Unassigned; resolve remaining TRD rules before new validation; route product compatibility changes to PRD |
| DQ-002 | Per-write ownership direction confirmed; platform-safe link/I/O details and compatible custom-theme-service integration remain unknown | DEC-001, DES-006/007; TR-011/013 | Blocks comprehensive containment/ownership implementation readiness; neither string checks nor built-in-only integration establish full coverage | Unassigned; resolve detailed mechanism and authorize bounded platform/API analysis separately; retain public contracts meanwhile |
| DQ-003 | Unknown reproducible contrast acceptance method; inherited TG-002 | DES-009; TR-018 | Blocks contrast evidence sign-off, not implementation of existing keyboard/label/focus obligations | Unassigned; agree method before V-005 sign-off; new quality thresholds belong in TRD |
| DQ-004 | Missing actual-blog snapshot/access, extensions, environment and executor; inherited TG-003 | DES-011; TR-019 | Blocks benchmark execution, not baseline engine design; no invented substitute dataset or results | Unassigned; owner selects inputs for next-phase V-006 |
| DQ-005 | Historical v0.3 approval confirmed on 2026-09-12; v0.4 is unsigned, and execution arrangements remain unknown | Entire design; PRD Q-004, TRD TG-005 | Revision review, technical blockers, and implementation/release authorization are separate | Historical approver is @justinyoo; obtain revised-text sign-off and resolve technical gaps/execution roles separately |
| DQ-006 | Collections and immutable navigation built before hooks versus plugin-returned title/route/visibility; combined route/404 planning cases | DES-002/004/005/009; TR-003/004/006/017; TG-004 | Current snapshot timing is established, but post-plugin cross-page consistency is not guaranteed. New collection semantics or pipeline redesign still need clarification and regression evidence | Unassigned; preserve per-document propagation and the documented snapshot while evaluating representative route-changing consumers; changes to obligations belong in TRD |
| DQ-007 | Pending verification and known non-atomic/stale preview behavior | DES-004/008/010/011; V-001 through V-007 | Missing release evidence is not automatic failure or a requirement waiver. Partial/stale output remains an accepted limitation, not a new promise to fix it | Unassigned; record/recheck next-phase results and publish only under separate release authority |

### Readiness assessment

- **Supported status:** Review-ready against PRD v0.7/TRD v0.3. The current-code mechanisms, flows and 20 mappings are coherent for review; v0.4 has no separate sign-off. Historical design approval does not remove unresolved technical details.
- **Material blockers:** TG-001/DQ-001, containment/ownership integration and feasibility in DQ-002, and the metadata/collection interaction in DQ-006 remain explicit. These do not invalidate the local static-generation foundation but prevent an all-scope Implementation-ready claim.
- **Confirmation:** Historical PRD v0.6/TRD v0.2 and TDD v0.3 approvals stand, as do DEC-001/002 confirmations. The 2026-09-13 update request authorizes alignment with merged behavior, not retroactive approval of revised text, gap resolution, verification results, or implementation/release.
- **Reviewer pass:** Directly reconciled the revised PRD/TRD, source-backed navigation/URL/route mechanisms, compatibility, and evidence mappings. Retained IDs, decision history, cancellation limits, and DQ-001/002/006 rather than treating shared helpers or snapshot construction as comprehensive fixes. No independent review is implied.
- **Deferred work:** Contrast method, benchmark inputs, execution arrangements, and V-001 through V-007 remain visible with next actions. No verification was started or declared successful.
- **Review limitations:** No external ADRs, complete rendering-sink inventory, owner-blog dataset, platform I/O feasibility experiment, or independently executed review/test results are available.

## 6. Material changes and references

| Date | Change | Basis / confirmation | Effect |
| --- | --- | --- | --- |
| 2026-09-11 | Create TDD v0.1 against PRD v0.6 and TRD v0.2 | User requested a design based on both documents and current code | Preserve requirements and deferrals; document current mechanisms, proposed focused changes, alternatives, and unresolved integration details |
| 2026-09-11 | Confirm the existing pipeline with per-write ownership checks | Explicit selection by @justinyoo | Accept DEC-001's direction; retain DQ-002's platform/API integration work and the documented partial-output model |
| 2026-09-12 | Confirm DEC-002 Option A; issue TDD v0.2 | Explicit selection by @justinyoo | Adopt prefix-aware preview serving with one artifact layout; preserve root behavior, TG-001's unresolved URL details, and pending V-003 evidence |
| 2026-09-12 | Record full TDD approval; issue signed TDD v0.3 | Explicit approval by @justinyoo | Resolve document sign-off without changing the governing PRD/TRD, resolving unspecified technical details, executing verification, or authorizing implementation/release |
| 2026-09-13 | Align TDD v0.4 with PRD v0.7/TRD v0.3 at code `1f963ad` | User update request and merged #86/#87 source inspection | Record navigation construction/layout delivery, shared URL wrappers, directory indexes, client behavior and migration; retain old approvals/decisions and unimplemented hardening/mount gaps |

### Source map

| Area | Consulted sources |
| --- | --- |
| Governing requirements and repository rules | [PRD](PRD.md), [TRD](TRD.md), [AGENTS](AGENTS.md), [SDK](global.json), [build settings](Directory.Build.props), [dependencies](Directory.Packages.props) |
| Composition, roots, installed-code boundary | [Builder](src\ScissorHands.Web\ScissorHandsApplicationBuilder.cs), [application](src\ScissorHands.Web\ScissorHandsApplication.cs), [DI](src\ScissorHands.Web\Extensions\ServiceCollectionExtensions.cs), [paths](src\ScissorHands.Web\Infrastructure\CurrentDirectoryAppPaths.cs), [assembly catalog](src\ScissorHands.Web\Infrastructure\DefaultAssemblyCatalog.cs) |
| Data and generation | [Loader](src\ScissorHands.Web\Loaders\ContentLoader.cs), [generator](src\ScissorHands.Web\Generators\StaticSiteGenerator.cs), [Markdown service](src\ScissorHands.Web\Services\MarkdownService.cs), [document](src\ScissorHands.Core\Models\ContentDocument.cs), [metadata](src\ScissorHands.Core\Models\ContentMetadata.cs) |
| Navigation and shared URLs | [NavigationNode](src\ScissorHands.Core\Models\NavigationNode.cs), [NavigationTreeBuilder](src\ScissorHands.Web\Navigation\NavigationTreeBuilder.cs), [ContentUrlHelper](src\ScissorHands.Core\Urls\ContentUrlHelper.cs), [website handoff](docs\website-documentation.md) |
| Extension mechanisms | [Runner](src\ScissorHands.Web\Runners\PluginRunner.cs), [dependencies](src\ScissorHands.Web\Runners\PluginDependencyResolver.cs), [theme resolver](src\ScissorHands.Web\Services\ThemeComponentResolver.cs), [theme service](src\ScissorHands.Web\Services\ThemeService.cs), [legacy/current service contract](src\ScissorHands.Core\Services\IThemeService.cs) |
| Manifests and guidance | [Site](src\ScissorHands.Core\Manifests\SiteManifest.cs), [theme](src\ScissorHands.Core\Manifests\ThemeManifest.cs), [plugin](src\ScissorHands.Core\Manifests\PluginManifest.cs), [Core guide](src\ScissorHands.Core\README.md), [Plugin guide](src\ScissorHands.Plugin\README.md), [Theme guide](src\ScissorHands.Theme\README.md), [Web guide](src\ScissorHands.Web\README.md) |
| Rendering and preview | [Renderer](src\ScissorHands.Web\Renderers\ComponentRenderer.cs), [layout base](src\ScissorHands.Theme\MainLayoutBase.cs), [cascades](src\ScissorHands.Theme\CascadingMainLayoutBase.razor), [watcher](src\ScissorHands.Web\Watchers\ContentWatcher.cs) |
| Built-in presentation | [Layout](src\ScissorHands.Web\themes\default\MainLayout.razor), [index](src\ScissorHands.Web\themes\default\IndexView.razor), [post](src\ScissorHands.Web\themes\default\PostView.razor), [theme CSS](src\ScissorHands.Web\themes\default\assets\theme.css), [theme JS](src\ScissorHands.Web\themes\default\assets\theme.js) |

**Terms:** an *artifact-relative path* names a file below generated output; a *deployment prefix* mounts that artifact in URL space; an *output owner* identifies the engine source responsible for a destination in one build. None of these is an account, tenant, or filesystem permission grant.
