# ScissorHands.NET - Product requirements document

## Document control

| Field | Value |
| --- | --- |
| Document version | 0.6 |
| Status | Implementation-ready |
| Last updated | 2026-09-11 |
| Scope | Current vNext product baseline; unimplemented discussion ideas are deferred |
| Code baseline | `43a4c3bd7bfa025e625fc7084be5b7251e450578` |
| Intended audience | Site owner and engine/theme/plugin contributors making baseline and compatibility decisions |
| Product owner / reviewers | @justinyoo |
| Target release / date | Not specified; this document does not schedule a release |
| Sign-off | Approved by @justinyoo on 2026-09-11 |
| Approval scope | Product requirements, release criteria, documented limitations, and next-phase verification placement; not passing verification or authorization to publish/deploy |

**Readiness:** the product scope, essential requirements, quality baseline, and release criteria are approved. The PRD is implementation-ready as a product baseline; this does not establish that every technical detail in the companion TRD is resolved. V-001 through V-007 remain pending next-phase work, and operational release arrangements remain Q-004. Approval does not waive requirements, establish passing results, or authorize a release.

## 1. Overview and evidence

ScissorHands.NET enables a developer to maintain a personal blog as local Markdown files, generate posts and independent pages through reusable Razor themes, and publish static HTML and assets without operating a Blazor application server for readers. Optional plugins transform content or the final document. A separate local preview mode supports the authoring loop.

The product direction comes from the user's requests in [Original discussion (archived)](https://github.com/getscissorhands/Scissorhands.NET/blob/464ce0f3454d473d4a39bc6f5c9005e86cd5396a/DISCUSSIONS.md): a Blazor-based static site generator, personal blog posts with frontmatter, independent pages such as About and Contact, interchangeable themes, and pre/post-conversion extensions. The current codebase implements this foundation and adds tags, a 404 page, URL options, and preview regeneration.

The product addresses the owner's need for a reusable .NET-based publishing workflow. In feedback on 2026-09-11, the owner confirmed that the workflow already saves time, makes customization substantially easier, and meets their expectations. These are user-reported outcomes supported by direct experience, not merely hypothetical benefits.

This is qualitative evidence: the amount of time saved and degree of improvement have not been measured numerically. That limits claims about the size of the improvement or benefits for other users; it does not invalidate the owner's experience. Quantitative benchmarks or market research are not prerequisites for recognizing that this personal blogging platform meets the owner's needs.

### Evidence and interpretation

| Source | What it establishes | Limits |
| --- | --- | --- |
| User statements in [Original discussion (archived)](https://github.com/getscissorhands/Scissorhands.NET/blob/464ce0f3454d473d4a39bc6f5c9005e86cd5396a/DISCUSSIONS.md) | Original product intent and personal-blog use case | Illustrative Mermaid and analytics plugins were examples, not completed implementations |
| Assistant responses in that discussion | Historical solution suggestions | Feed, sitemap, search, renderer snippets, and lifecycle explanations are not approved requirements or verified implementation evidence |
| [Root guide](README.md), [Web guide](src\ScissorHands.Web\README.md), package guides, and source links below | Current configuration, contracts, and observable code behavior | Existing code does not itself prove release readiness or stakeholder approval |
| [Repository guardrails](AGENTS.md) | Compatibility, validation, data handling, and testing expectations | Guardrails explicitly do not certify existing security coverage |
| User clarifications on 2026-09-11 | Current vNext baseline is the scope; unimplemented ideas are deferred; authoring is owner-controlled with trusted executable extensions | Does not approve proposed quality targets or a release |
| Owner's PRD review feedback on 2026-09-11 | The workflow saves the owner time, makes customization substantially easier, and meets their expectations | Direct qualitative evidence for this owner; no numerical improvement or broader-user result is claimed |
| User's acceptance of quality recommendations on 2026-09-11 | Basic built-in-theme accessibility, desktop/mobile browser coverage, and benchmarking the actual blog without a formal SLA or maximum supported site size | Confirms expectations, not successful implementation checks, WCAG conformance, or release approval |
| User's next-phase decision on 2026-09-11 | The seven implementation-verification areas are deferred to the next phase | Confirms the verification work's placement, not its execution, results, deadline, or release approval |
| User's approval of both documents on 2026-09-11 | Approval of the PRD and TRD requirements and their documented limitations/deferrals | Does not resolve unspecified technical details, supply verification results, assign execution owners, or authorize publication/deployment |

**Terminology:** *Confirmed* means supported by a user decision or identified source, with the basis stated. *Proposed* means not yet agreed. *Unknown* means unresolved. *Not applicable* means deliberately outside this scope with a reason. Code-backed baseline behavior is distinguished from new proposals throughout.

## 2. Users and essential journeys

The primary user is a personal-blog owner comfortable creating and running a .NET application. Secondary actors are theme/plugin authors and visitors reading the generated site. The user confirmed owner-controlled local authoring with deliberately trusted executable themes/plugins, not a service accepting untrusted uploads or sandboxing extensions. There is no editorial account or role system. This boundary does not remove input-validation, path-containment, encoding, or secret-handling obligations.

| Journey | Trigger and successful outcome | Important alternate or failure path |
| --- | --- | --- |
| J-001: Publish content | Owner edits a post or page, runs build, and obtains HTML plus supported assets for a static host | Invalid metadata or route collisions fail generation; the owner corrects the reported input and rebuilds |
| J-002: Preview an edit | Owner runs preview, changes content or theme files, then refreshes the browser after regeneration | Drafts are omitted; recompilation is required for Razor/C# changes; regeneration errors are logged |
| J-003: Change appearance | Owner installs/provides a compatible theme, changes `Site:Theme`, then restarts/rebuilds without rewriting Markdown | Missing or unmatched custom theme configuration fails; optional tag views can use built-in fallbacks |
| J-004: Extend generation | Extension author supplies a plugin; owner explicitly configures its ID and dependencies | Invalid identity/dependency configuration fails before hooks run; dependencies are not installed or enabled automatically |
| J-005: Read the site | Visitor browses the post index, post/page URLs, and available tag pages using ordinary HTTP navigation | `404.html` is generated; the deployment host must configure its own not-found behavior |

## 3. Goals and outcome measurement

The owner has confirmed the workflow's usefulness through experienced time savings and easier customization. Optional measurement proposals below could quantify or track those benefits; they are not required to accept the reported outcomes and do not substitute for feature acceptance. No product telemetry collection is authorized by this PRD.

| Goal | Desired outcome | Current qualitative evidence | Optional measurement method | Quantitative baseline / target / window |
| --- | --- | --- | --- | --- |
| G-001 | Owner can publish a personal blog without operating a .NET server in production | Static-only hosting is the original intent; current build behavior is documented in FR-001 | Owner records successful publishes and any runtime-server dependencies at the chosen host | Not specified |
| G-002 | Owner saves time maintaining posts and independent pages without changing engine code | Owner reports that the workflow saves time and meets expectations | Observe the edit-preview-publish journey and record interventions and elapsed time | Not measured / not set / not set |
| G-003 | Owner finds customization easier while keeping presentation and transformations independent of content authoring | Owner reports substantially easier customization and expectations met | Exercise a theme replacement and plugin enable/disable workflow; record required content or engine rewrites | Not measured / not set / not set |

Guardrail observations should include broken internal links, accidental draft publication, output overwritten by conflicting inputs, and failed builds mistaken for deployable artifacts. Responsibilities and cadence for any additional measurement remain undecided (Q-004); the owner's qualitative feedback is already recorded.

## 4. Scope and priorities

All functional requirements below belong to the **retained baseline**, not an implementation backlog. `Core` identifies original publishing/theme/extension needs; `Supporting` identifies existing authoring and navigation capabilities that remain in scope. These labels describe contribution to the baseline, not promised delivery order.

### In scope

Local .NET application setup; explicit build/preview/help modes; Markdown and supported YAML frontmatter; posts and independent pages; date/locale URL options; index, tags, and 404 generation; Razor rendering; configured themes and supported assets; opt-in plugin stages and dependencies; local regeneration; current compatibility obligations.

### Non-goals

This baseline does not aim to operate a hosted CMS, require a Blazor runtime on the public host, or turn reader navigation into a server-interactive Blazor application. Static generation still executes the .NET application and compiled Razor/theme/plugin code on the build machine; it is not generation without executing code.

### Outside this scope

A browser-based editor, accounts/editorial approvals, built-in contact-form processing, comments, commerce, managed hosting/deployment, and arbitrary Razor-route crawling are not part of this baseline. An independent Contact page means generated content, not a message-submission backend.

No new automatic browser reload, draft-preview mode, publication scheduler, atomic output swap, last-good-artifact preservation, bounded graceful CLI cancellation, fully offline output, or incremental-build guarantee is introduced. These exclusions do not claim such behavior would be undesirable; changing it requires a separate scope decision.

### Deferred possibilities, not roadmap commitments

| Idea | Origin | Current disposition |
| --- | --- | --- |
| Mermaid blocks rendered as images; Google Analytics injection | User examples in the original discussion | Extension points exist; concrete bundled implementations are not established in the inspected code |
| RSS/Atom feeds, sitemap, JSON search index | Historical assistant suggestions | Not produced by the inspected generator; deferred |
| Table of contents and syntax-highlighting plugins | Historical assistant suggestions | No dedicated bundled implementation is established here; Markdown code blocks alone do not establish a highlighting product |

## 5. Functional requirements

### FR-001: Build a site without starting its preview server

- **Basis / scope:** Confirmed original intent and current application/renderer behavior; Core. Sources: [application](src\ScissorHands.Web\ScissorHandsApplication.cs), [renderer](src\ScissorHands.Web\Renderers\ComponentRenderer.cs), [sample guide](samples\ScissorHands.Sample\README.md).
- **Actor / rationale:** Owner runs the application from the site directory to obtain a deployable static artifact (J-001, G-001).
- **Behavior:** `--build` generates into `dist`; `--preview` generates into `preview` and starts a local server; `--help` displays usage. Recognized modes are explicit, not inferred from content.
- **Acceptance:** A successful build writes the generated HTML and supported assets and reports the output location without starting the preview listener. HTML rendering uses compiled Razor components through `HtmlRenderer`; serving the resulting site does not require the generation application.
- **Boundaries:** An invocation with no recognized mode reports an error and exit code 1. Preview takes precedence if both build and preview are supplied. The sample's launch profile injects preview, so explicit build usage requires `--no-launch-profile`. Build replaces existing `dist`; it does not preserve a last-good artifact (FR-009).

### FR-002: Load posts and independent pages with explicit publication rules

- **Basis / scope:** Confirmed discussion intent and [content loader](src\ScissorHands.Web\Loaders\ContentLoader.cs); Core.
- **Actor / rationale:** Owner stores `.md` files recursively under `contents\posts` and `contents\pages` to keep content independent of presentation (J-001, G-002).
- **Behavior:** Parse optional YAML frontmatter and convert Markdown. Supported fields are `title`, `slug`, `description`, `locale`, `author`, `twitter_handle`, `hero_image`, `published`, `tags`, and `draft`. Tags accept a YAML list or comma-separated text.
- **Acceptance:** Without frontmatter, the title defaults to the filename and the slug derives from the relative path. Malformed YAML, an unclosed frontmatter block, unsupported fields, or invalid date/draft/tag values produce an error identifying the source and, where applicable, field. Missing content directories log a warning and supply an empty collection.
- **Publication boundary:** `draft: true` is excluded from both build and preview, including collection pages. `published` supplies ordering/date metadata; a future date does not schedule or withhold publication. This preserves the current baseline rather than introducing draft preview or scheduling.

### FR-003: Generate predictable content URLs and reject conflicting routes

- **Basis / scope:** Confirmed [loader](src\ScissorHands.Web\Loaders\ContentLoader.cs), [generator](src\ScissorHands.Web\Generators\StaticSiteGenerator.cs), and [route regression cases](test\ScissorHands.Web.Tests\Generators\StaticSiteGeneratorRouteTests.cs); Core.
- **Actor / rationale:** Owner supplies slugs and URL settings so visitors can address content predictably (J-001, J-005).
- **Behavior:** Ordinary content routes write `<route>\index.html`. `UseDateInPostUrl` prefixes dated posts, not pages, with `yyyy/MM/dd`; a missing date logs a warning and leaves the route undated. `UseLocaleInUrl` adds the normalized effective document/site locale without duplicating an existing locale prefix. `404.html` is not locale-prefixed.
- **Acceptance:** A `hello` post dated 2026-09-11 with effective locale `ko-KR` becomes URL path `ko-kr/2026/09/11/hello` when both options are enabled. A page does not acquire the post-date prefix. Literal `.`/`..` route segments and case-insensitive output collisions, including a file used as a parent directory, fail validation.
- **Boundary:** Planned content and generated routes are checked before page writes, but after the application may have removed the previous output. This is not proof of complete asset-collision or symlink protection (NFR-002).

### FR-004: Provide index, tag, and not-found pages

- **Basis / scope:** Confirmed [generator](src\ScissorHands.Web\Generators\StaticSiteGenerator.cs) and [generation regression cases](test\ScissorHands.Web.Tests\Generators\StaticSiteGeneratorTests.cs); Supporting.
- **Actor / rationale:** Visitors need entry points and discovery beyond individual post URLs (J-005).
- **Behavior:** Always generate the site index and `404.html`; list posts newest first, with undated posts last. A page whose slug is `404.html` supplies the custom not-found document rather than a normal content page.
- **Acceptance:** With ordinary tagged content, produce a tag index at `tags` and per-tag routes under `tags`; group names case-insensitively via lowercase normalization and encode tag route text. Pass posts ordered by publication date descending and pages by title ascending. Exclude the custom 404 document from displayed tag collections.
- **Empty / hosting boundary:** With no eligible tagged content, emit no tag pages; an empty post collection still renders the index and 404. Generated `404.html` does not itself configure HTTP status codes or fallback routing on a static host.

### FR-005: Swap compatible Razor themes without rewriting content

- **Basis / scope:** Confirmed original intent, [Theme guide](src\ScissorHands.Theme\README.md), and [theme resolver](src\ScissorHands.Web\Services\ThemeComponentResolver.cs); Core.
- **Actor / rationale:** Owner selects a theme; theme author provides compatible components and metadata (J-003, G-003).
- **Behavior:** Resolve the configured `Site:Theme` against the normalized component namespace suffix. A custom theme provides layout, index, post, page, and not-found views; tag views are optional and fall back to built-ins. Explicit `AddLayouts` registration remains available.
- **Acceptance:** An installed `minimal-blog` theme with a compatible namespace can be selected through configuration without listing its component types in application setup. Missing required components or an unmatched custom theme fail instead of silently selecting another custom theme.
- **Boundary:** Theme component changes need compilation; changing configured theme requires restarting/rebuilding. This is author-side theme replacement, distinct from the default theme's visitor-facing light/dark toggle. No theme marketplace, sandbox, or install UI is promised.

### FR-006: Carry supported content and theme assets into the artifact

- **Basis / scope:** Confirmed [generator](src\ScissorHands.Web\Generators\StaticSiteGenerator.cs), [theme service](src\ScissorHands.Web\Services\ThemeService.cs), and [Theme guide](src\ScissorHands.Theme\README.md); Core.
- **Actor / rationale:** Owner and theme author need images, styles, and scripts to remain available after static deployment (J-001, J-003).
- **Behavior:** Copy `contents\images` recursively into output `images`. Load theme metadata from `themes\<slug>\theme.json`; copy supported theme assets below output `themes\<slug>`.
- **Acceptance:** Files under a theme's `assets` directory are copied with their relative structure; the theme service also includes its supported image extensions and `manifest.json`. Manifest CSS/script entries are available to the theme for page inclusion. The built-in default theme remains usable through its bundled assets/default fallback.
- **Failure / limits:** Missing content images log a warning; missing/invalid custom theme manifests, including a slug that does not match configuration, fail, while missing theme asset folders can warn. Arbitrary application `wwwroot` and plugin-asset copying are not established guarantees. This requirement does not guarantee every externally referenced asset is downloaded or works offline.

### FR-007: Preserve ordered content transformation stages

- **Basis / scope:** Confirmed discussion intent, [Plugin guide](src\ScissorHands.Plugin\README.md), [generator](src\ScissorHands.Web\Generators\StaticSiteGenerator.cs), and [runner](src\ScissorHands.Web\Runners\PluginRunner.cs); Core.
- **Actor / rationale:** Extension author transforms source content or complete documents without changing the engine (J-004, G-003).
- **Behavior:** For a source post/page, run pre-Markdown plugins, Markdown conversion, post-Markdown plugins, Razor rendering, then post-HTML plugins. Each enabled plugin's returned output feeds the next.
- **Acceptance:** A pre-Markdown change affects the Markdown conversion; a post-Markdown change reaches the Razor view; a post-HTML change reaches the written file. Source-backed custom 404 content receives Markdown stages. All emitted pages, including index/tag/generated 404 pages, receive post-HTML processing.
- **Boundary:** Synthetic collection pages do not have their own source Markdown stages. `SiteManifest.IsPreview` is set before hooks and rendering so plugins can distinguish modes; the engine does not automatically suppress analytics or other plugin side effects in preview.

### FR-008: Enable plugins explicitly and resolve dependencies predictably

- **Basis / scope:** Confirmed [Plugin guide](src\ScissorHands.Plugin\README.md), [runner](src\ScissorHands.Web\Runners\PluginRunner.cs), and [dependency resolver](src\ScissorHands.Web\Runners\PluginDependencyResolver.cs); Core.
- **Actor / rationale:** Owner chooses installed plugins; extension author declares prerequisites (J-004).
- **Behavior:** Match manifests, dependencies, and Razor plugin selectors by required unique lowercase ASCII kebab-case `Id`. `Name` is display-only. Installed plugins without manifests remain disabled; configuration presence enables a plugin rather than a universal `Options.Enabled` switch.
- **Acceptance:** Invalid/missing/duplicate IDs or configured-but-uninstalled plugins fail. Declared dependencies must be installed and enabled in the declared stage. Missing/disabled prerequisites, invalid stages, self/duplicate dependencies, and cycles fail before hooks execute.
- **Ordering / limits:** Within each stage, prerequisites precede dependents; ties among ready plugins use ordinal ID order, not configuration or registration order. Dependencies are neither installed nor enabled automatically. Extension-specific options remain the extension's responsibility to validate. A valid Razor selector with no configured manifest resolves `Plugin` to null; its component author is responsible for omitting disabled output.

### FR-009: Regenerate local preview and expose failures honestly

- **Basis / scope:** Confirmed [application](src\ScissorHands.Web\ScissorHandsApplication.cs), [watcher](src\ScissorHands.Web\Watchers\ContentWatcher.cs), and [Web guide](src\ScissorHands.Web\README.md); Supporting.
- **Actor / rationale:** Owner iterates on content and sees generated results before publishing (J-002, G-002).
- **Behavior:** Perform initial generation and serve `preview`; watch content/theme file create, change, rename, and delete events, coalesce bursts, and serialize regeneration. Report successful rebuilds and instruct the owner to refresh the browser.
- **Acceptance:** After a successful content edit and rebuild, browser refresh displays the regenerated file. A watcher callback failure is logged; subsequent changes can trigger another rebuild. Initial generation errors propagate rather than being reported as completed builds.
- **Recovery boundary:** Preview regeneration writes into the existing output, so deleted/renamed sources can leave stale generated files until preview is restarted. Writes are not atomic: a failed build/rebuild may leave partial or mixed output, and initial build/preview startup clears its output folder. Use only a successful production build as a publication candidate; automated cleanup/rollback is not promised.

### FR-010: Provide a readable static default presentation

- **Basis / scope:** Confirmed [Web guide](src\ScissorHands.Web\README.md), [Theme guide](src\ScissorHands.Theme\README.md), and built-in [layout](src\ScissorHands.Web\themes\default\MainLayout.razor); Supporting.
- **Actor / rationale:** Visitors need ordinary page navigation and owners need a usable starting theme (J-005).
- **Behavior:** Render page/site title, description, and locale through theme metadata; include theme styles/scripts and base-relative internal navigation. The default theme supplies responsive styling and a labelled light/dark control.
- **Acceptance:** The default layout derives page title, description, and locale from the current document with site-level fallbacks. With JavaScript enabled and browser storage available, the default theme toggle persists a light/dark preference. Reading generated content does not require a Blazor circuit or client runtime.
- **Boundary:** Optional theme/plugin JavaScript can still run in the browser. Browser coverage and basic accessibility expectations follow NFR-008; their implementation is not established by the presence of a toggle or responsive CSS. Formal WCAG conformance is not claimed. Storage-disabled preference persistence remains unestablished; no new storage fallback is promised.

## 6. Quality requirements and constraints

The user confirmed the following quality baseline on 2026-09-11. Agreed requirements and documented limitations are distinct from evidence that the implementation satisfies them. Verification is deferred to the next phase in Section 7; it is engineering and release work, not an unanswered decision about whether a requirement applies.

| ID | Area and requirement | Observable acceptance or gap | State / basis |
| --- | --- | --- | --- |
| NFR-001 | Local authoring and extension trust | The owner controls builds and deliberately installs trusted executable themes/plugins; no untrusted multi-user upload or extension sandbox promise. Content metadata still requires validation, and trusted authoring does not exempt generated HTML from visitor-safety obligations | Confirmed by the user on 2026-09-11; Q-001 resolved |
| NFR-002 | Filesystem containment and unintended overwrites | Reject content-derived traversal/symlink escapes and conflicting output owners, including assets. Existing route cases demonstrate only part of this obligation; full read/copy/write coverage remains unverified | Confirmed policy in AGENTS.md; implementation completeness unknown, Q-002 |
| NFR-003 | Encoding and raw HTML | Preserve Razor's metadata encoding and reject unsafe URL schemes at relevant boundaries. Raw Markdown/plugin HTML is an explicit trust boundary, not universally sanitized content. Do not claim all theme overrides are safe | Confirmed policy; [Markdown service](src\ScissorHands.Web\Services\MarkdownService.cs) and Theme guide establish raw-content rendering; coverage unknown, Q-002 |
| NFR-004 | Diagnostic and artifact integrity | Invalid input and generation failures must not be presented as successful builds. Missing optional content/assets can warn as specified above. No atomic publication or last-good-output guarantee | Confirmed baseline and policy; FR-001, FR-002, FR-006, FR-009 |
| NFR-005 | Subpath hosting | Generated internal links and theme/plugin asset URLs must respect `SiteManifest.BaseUrl`, including `/docs/`. Assess by serving an artifact at its intended prefix; a `<base>` tag alone does not prove deployment/preview correctness | Confirmed documented contract; host/path behavior must be evaluated, Q-002 |
| NFR-006 | Cancellation | Preserve cancellation-aware service contracts and propagation where tokens are supplied. Cancellation must surface instead of yielding a completed artifact | Confirmed policy and service contracts. Production CLI currently calls generation with `CancellationToken.None`; no graceful CLI cancellation-time bound is claimed |
| NFR-007 | Compatibility and distribution | Keep .NET 10 package boundaries and documented migration behavior. Existing legacy theme-service overloads remain obsolete but available; read-only `ThemeManifest.Stylesheets`/`Scripts` and `PluginManifest.Options`, required plugin IDs, and `PluginDependency.PluginId` are intentional vNext breaking changes | Confirmed [global.json](global.json), [build settings](Directory.Build.props), root/package guides; existing consumers must update configuration and rebuild |
| NFR-008 | Accessibility and browser coverage | The built-in theme must support keyboard navigation, visible focus, meaningful labels, and readable contrast in light/dark modes across the browser coverage below. Custom-theme authors own their themes' accessibility. No formal WCAG conformance claim or comprehensive assistive-technology certification is made for this baseline | Confirmed by the user on 2026-09-11; Q-003 resolved. Implementation coverage remains to be evaluated |
| NFR-009 | Performance and scale | Use the owner's actual blog, including its assets and enabled themes/plugins, as the representative workload on a documented development environment. Record build duration, preview-update delay, and memory usage. This baseline sets no formal performance SLA, numerical pass/fail thresholds, or maximum supported site size; measurements inform any later targets | Confirmed by the user on 2026-09-11; Q-003 resolved. Benchmark execution and results remain pending; absence of a declared size limit is not an unlimited-scale guarantee |
| NFR-010 | Privacy and external requests | Do not expose local secrets in generated HTML, logs, or fixtures. No built-in analytics implementation is established; optional plugins/themes may introduce external requests and obligations | Confirmed policy; the [site manifest](src\ScissorHands.Core\Manifests\SiteManifest.cs) includes a remote default hero-image URL, so zero-network/offline behavior is not promised |

**Browser coverage (NFR-008):** target the current stable versions of Edge, Chrome, Firefox, and Safari on desktop, plus Chrome on Android and Safari on iOS. Record the actual browser, OS, and device versions used when evaluating a release. This is agreed coverage for generated-site reading/navigation and the built-in theme, not evidence that all combinations have passed.

**Benchmark record (NFR-009):** identify the blog snapshot, post/page counts, asset volume, enabled theme/plugins, machine specifications, OS, and .NET SDK used, together with the measurement method and results. The particular snapshot and machine details are execution inputs to record when benchmarking, not new product-scope decisions. Do not infer an SLA from a single measurement or require arbitrary numerical targets to accept this baseline.

**Additional coverage:** locale metadata and optional URL prefixes are supported, but translation management and a localized UI catalog are outside this baseline. Accounts, account-data retention/deletion, payments, entitlements, and AI behavior are not applicable because this scope contains no such services. Local content/output lifecycle is covered by FR-002 and FR-009; generated artifacts remain on disk until removed/replaced. No special compliance certification or regulated workflow was supplied. Third-party analytics/consent obligations must be addressed if an analytics integration is separately scoped.

## 7. Next-phase verification, release, and evaluation

### Next phase: implementation verification

The user agreed on 2026-09-11 to treat the following as next-phase items. All are pending; execution owners and dates are unassigned. This PRD update records the work without starting it. Existing requirements and exclusions remain unchanged.

| ID | Area / requirements | Verification scope and expected evidence |
| --- | --- | --- |
| V-001 | Filesystem safety / NFR-002 | Exercise content reads, generated-page writes, and asset copying against traversal, symbolic-link escapes, and output collisions. Record containment and overwrite-protection results, including gaps beyond existing route tests |
| V-002 | Rendering and privacy / NFR-003/010 | Check metadata encoding, unsafe URL schemes at relevant boundaries, and accidental secret exposure in generated output/logs using synthetic fixtures, not real credentials. Preserve supported raw Markdown/plugin HTML as an explicit trust boundary; record results and any unsafe paths |
| V-003 | Subpath hosting / NFR-005 | Exercise preview and production output at `/` and a prefix such as `/docs/`, including navigation, posts/pages/tags, images, styles, scripts, and representative theme/plugin URLs. Record serving configuration and link/asset results |
| V-004 | Failures and cancellation / NFR-004/006 | Exercise invalid input, generation failures, and cancellation-aware APIs. Confirm actionable errors and no misleading success; record partial-output behavior against documented limitations, without introducing atomic-output or bounded CLI-shutdown guarantees |
| V-005 | Accessibility and browsers / NFR-008, FR-010 | Evaluate generated-site reading/navigation and built-in-theme controls, keyboard access, focus, labels, and light/dark contrast across the agreed desktop/mobile browsers. Record actual browser/OS/device coverage and findings; no formal WCAG certification is required |
| V-006 | Performance baseline / NFR-009 | Benchmark the owner's actual blog with its assets and enabled extensions. Record the workload/environment details, build duration, preview-update delay, memory usage, and measurement method. Completion establishes measurements, not compliance with an unagreed numerical target |
| V-007 | Functional regression and compatibility / FR-001 through FR-010, NFR-007 | Run the existing suite and sample build/preview; cover posts/pages, draft exclusion, tags/404, theme discovery/fallbacks, plugin ordering/errors, and documented API/configuration compatibility. Record results and review Windows/macOS/Linux CI evidence without assuming untested platforms passed |

For each item, record what was exercised, the environment, results, and reproducible gaps. Failed, blocked, or skipped checks remain explicit; an attempted check is not a passing result. Track necessary corrections against the existing requirements, then recheck affected behavior. The benchmark needs reproducible measurements, not an invented speed threshold. These records are next-phase deliverables, not prerequisites for finishing the requirements document.

### Approved release criteria

This PRD describes a public-preview baseline, not a new release authorization. The following criteria are approved as requirements that a release must satisfy; their approval is not evidence that they have been satisfied:

1. Use the approved baseline, resolve any remaining technical behavior/safety details required by the chosen release, assign a release decision-maker, and document accepted limitations.
2. Demonstrate the essential journeys and acceptance cases, including malformed metadata, collisions, missing dependencies, empty content, draft exclusion, and theme fallback.
3. Pass the relevant repository checks and exercise sample build/preview; evaluate generated links/assets under both root and subpath deployment. The existing CI configuration targets Windows, macOS, and Linux; it is not evidence of a currently passing run.
4. Reconcile NFR-002/NFR-003 containment and rendering coverage, evaluate the agreed NFR-008 accessibility/browser expectations, and record NFR-009 benchmark results. Do not introduce an unagreed numerical performance gate or claim formal WCAG conformance.
5. Ship consistent package/sample guidance and migration notes. The existing ID/immutable-collection breaking changes must not be presented as transparent upgrades.

**Rollout and recovery:** package publication, host selection, release timing, and deployment automation are not authorized here. The documented operational practice is to publish only a successful artifact and retain a previous deployed artifact through the chosen host; the engine does not implement deployment rollback. Recovery of local generated output is correction plus a fresh build or preview restart.

**Evaluation:** the owner has already confirmed time savings, easier customization, and expectations met. Any further measurement of G-001 through G-003 is optional; its timing, responsibilities, and numerical outcome targets remain undecided (Q-004) and are not prerequisites for accepting the reported benefits. Separately, V-005 and V-006 carry the agreed accessibility/browser evaluation and actual-blog benchmarking into the next phase. These quality decisions are settled; their checks and measurements have not been performed by this document update.

## 8. Risks and unresolved decisions

### Risks and dependencies

| ID | Risk / dependency | Impact and next action | Owner |
| --- | --- | --- | --- |
| RD-001 | Theme/plugin assemblies execute code; generated raw HTML and scripts cross trust boundaries | Building untrusted extensions or publishing unsafe markup can affect author/visitor environments. Retain the owner-controlled boundary; assess guardrail coverage in V-001/V-002 without a sandbox assurance | Unassigned |
| RD-002 | Non-atomic output and stale preview files | Failed builds can lose previous output; renamed/deleted content may remain in preview. Retain the limitation and evaluate failure/output behavior in V-004 | Unassigned |
| RD-003 | Static-host behavior varies | Directory indexes, subpaths, 404 handling, and external assets can differ from local preview. Record and evaluate serving assumptions in V-003; no particular host is required | Unassigned |
| RD-004 | Third-party extensions and existing consumers may lag vNext contracts | Old name-based configuration and mutable-collection consumers can break. Verify the documented migration and compatibility in V-007 | Unassigned |

### Decision and evidence gaps

| ID / state | Decision or evidence needed | Affected areas | Blocking effect / next action |
| --- | --- | --- | --- |
| Q-001 / Confirmed | Owner-controlled local authoring with deliberately trusted executable themes/plugins; no untrusted upload service or extension sandbox | Users, FR-005, FR-008, NFR-001/003/010 | Resolved by the user on 2026-09-11; retain validation and visitor-safety obligations |
| Q-002 / Confirmed (next-phase scope) | Defer containment, rendering/privacy, and subpath verification to V-001 through V-003; results remain unknown | FR-003/006/009, NFR-002/003/005/010 | Placement resolved by the user on 2026-09-11. Evidence and necessary corrections remain next-phase work before claiming compliance or release readiness, not PRD-definition blockers |
| Q-003 / Confirmed | Built-in-theme accessibility and the NFR-008 desktop/mobile browser coverage; custom-theme authors own accessibility. Benchmark the actual blog on a documented environment, with no formal SLA, numerical performance gate, maximum supported site size, or WCAG conformance claim | FR-010, NFR-008/009 | Quality choices resolved by the user on 2026-09-11; verification is assigned to next-phase V-005/V-006, with execution owners and dates still unassigned |
| Q-004 / Unknown (release arrangements) | Who owns operational release acceptance and any further outcome evaluation, and when will they occur? | Goals and release criteria | Document approval is complete. Execution ownership, timing, and authorization for an actual release remain to be confirmed; these do not block implementation of the approved product requirements |

The product requirements and documented release criteria are approved. Optional outcome measurement and unassigned release/rollout arrangements remain separate from document approval. No authorization for an actual release is inferred.

### Decisions and material changes

| Date | Decision | Basis | Effect |
| --- | --- | --- | --- |
| 2026-09-11 | Document the current vNext baseline; defer unimplemented discussion ideas | Explicit user clarification | Preserve current capabilities and limits; do not add feeds, search, or concrete example plugins to release scope |
| 2026-09-11 | Use owner-controlled local authoring and deliberately trusted executable themes/plugins as the trust boundary | Explicit user clarification | Resolve Q-001; exclude an untrusted upload service and sandbox guarantees without weakening validation or visitor protections |
| 2026-09-11 | Record experienced time savings, substantially easier customization, and expectations met as user-reported outcomes | Explicit owner feedback during PRD review | Replace the benefit hypotheses with qualitative evidence; leave numerical improvement unquantified and broader-user claims unestablished |
| 2026-09-11 | Adopt basic built-in-theme accessibility, the specified desktop/mobile browser coverage, and actual-blog benchmarking without formal SLA/size limits or a WCAG conformance claim | Explicit user acceptance of recommendations | Resolve Q-003 and confirm NFR-008/NFR-009; retain existing safety/compatibility obligations and out-of-scope reliability features; keep implementation verification separate from product decisions |
| 2026-09-11 | Defer the seven implementation-verification areas to the next phase | Explicit user decision | Record V-001 through V-007 with expected evidence; resolve Q-002's placement while preserving unknown results, all requirements, and release safeguards |
| 2026-09-11 | Separate original intent, current code behavior, and proposed quality/release criteria | Document synthesis; not a new product approval | Prevent historical suggestions and incomplete guardrails from being represented as delivered requirements |
| 2026-09-11 | Record approval of both documents; issue PRD v0.6 as the signed product baseline | Explicit approval by @justinyoo | Approve product requirements and release criteria without changing requirement IDs, resolving unspecified TRD details, executing verification, or authorizing release |

### Readiness assessment

- **Supported status:** Implementation-ready as a product requirements baseline. @justinyoo explicitly approved the essential requirements, quality choices, documented limitations, and release criteria. Product-scope questions are resolved or explicitly deferred; this status does not override technical blockers recorded in the TRD.
- **Remaining decisions versus execution:** Q-002's next-phase placement and Q-003's quality choices are settled. V-001 through V-007 remain pending. Q-004 concerns operational release arrangements and optional further evaluation, not outstanding document approval.
- **Approval:** Approved by @justinyoo on 2026-09-11. Approval includes the recorded next-phase deferrals but does not imply passing verification, resolution of unspecified technical details, or authorization to publish/deploy.
- **Reviewer pass:** Reconciled the saved document with explicit approval and the companion TRD. Preserved all FR/NFR/V IDs, requirements, exclusions, and pending evidence; separated approved release criteria from authorization for an actual release.
- **Review limitations:** Evidence includes repository documentation, source inspection, selected regression cases, and the owner's qualitative experience. No agent-executed product validation or quantitative benefit measurement was performed. No external research, external theme/plugin inventory, accessibility audit, or security certification was performed. No .NET build is required for this documentation-only change.

## 9. Reference map

Use the linked source files in each requirement for behavior. The package guides remain authoritative for detailed API/configuration usage; this document does not replace a technical design.

| Topic | References |
| --- | --- |
| Original discussion and product setup | [Original discussion (archived)](https://github.com/getscissorhands/Scissorhands.NET/blob/464ce0f3454d473d4a39bc6f5c9005e86cd5396a/DISCUSSIONS.md), [README.md](README.md), [sample](samples\ScissorHands.Sample\README.md) |
| Shared contracts and extension guidance | [Core](src\ScissorHands.Core\README.md), [Plugin](src\ScissorHands.Plugin\README.md), [Theme](src\ScissorHands.Theme\README.md), [Web](src\ScissorHands.Web\README.md) |
| Content/route regression evidence | [Loader tests](test\ScissorHands.Web.Tests\Loaders\ContentLoaderTests.cs), [generator tests](test\ScissorHands.Web.Tests\Generators\StaticSiteGeneratorTests.cs), [route tests](test\ScissorHands.Web.Tests\Generators\StaticSiteGeneratorRouteTests.cs) |
| Extension regression evidence | [Plugin runner tests](test\ScissorHands.Web.Tests\Runners\PluginRunnerTests.cs), [dependency tests](test\ScissorHands.Web.Tests\Runners\PluginDependencyTests.cs), [theme resolver tests](test\ScissorHands.Web.Tests\Services\ThemeComponentResolverTests.cs), [theme service tests](test\ScissorHands.Web.Tests\Services\ThemeServiceTests.cs) |
| Constraints and release practices | [AGENTS.md](AGENTS.md), [SDK selection](global.json), [build configuration](Directory.Build.props), [CI](.github\workflows\main.yaml) |
