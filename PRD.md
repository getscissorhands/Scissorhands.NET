# ScissorHands.NET - Product requirements document

## Current scope amendment: directory locales and fallback

The consolidated [#104 contract](https://github.com/getscissorhands/Scissorhands.NET/issues/104) supersedes the locale portions of FR-002/003/004/012 and Q-006 through Q-012 below. The existing document-control, approval, and execution records remain historical baselines, not approval or verification of this amendment.

Primary content and URLs stay unprefixed. Explicit `Site:Locale` enables localization; `LocalizationFallbackMessages` keys declare additional locales and their values supply required notices. Undeclared folders remain ordinary content; declared locale formats must match the primary format. Same-kind, same-relative-path/filename pairs require matching slugs and required matching written calendar dates for paired posts. The primary publication state gates every translation/fallback. Missing/draft translations use labelled primary content, while withdrawn primary content removes all active variants and collection entries.

Collections select translation or fallback once per primary identity; generated navigation stays in the requested locale. The [scope expansion](https://github.com/getscissorhands/Scissorhands.NET/issues/104#issuecomment-5810010309) adds automatic authored internal document-link localization with Markdown `{data-localize="false"}` opt-out, preserving explicit locale targets, shared resources, external URLs, queries and fragments. The shared language switcher uses engine-prepared destinations and theme-owned native labels; generated tag pages switch to the corresponding collection or locale homepage, and the shared 404 switches to locale homepages. Individual documents retain canonical/real-translation alternate metadata and required banners with distinct requested/content languages. Generated home/tag collections and the one shared 404 do not acquire document fallback/SEO semantics. See the [current behavior and migration reference](docs/website-documentation.md#locale-specific-sites) for the complete contract and exceptions.

Verification for this amendment belongs to the updated locale loader/generator tests, actual preview integration, and browser fixtures. Earlier V-009 counts below must not be cited as evidence for #104; release authorization and broader gaps are unchanged.

The default and custom themes own all localization markup. `ScissorHands.Theme.Components` provides abstract switcher, metadata, and fallback-banner base classes with prepared data and reusable behavior, not fixed Razor templates. The required notice uses the base's encoded text fragment and marker/language attributes so validation remains independent of the theme's wrapper and formatting.

Initial directory-locale implementation evidence for #104: the Release solution build and all 714 .NET tests passed, including real root/subpath preview and withdrawal. Sample root/subpath generation, slash-variant byte equivalence, four contrast-math cases, and 48 Chromium/WebKit desktop/mobile cases passed. The 24 Firefox cases were blocked before test execution by the installed browser reporting `Could not find profile folder`, including a retry with a session-local temporary profile directory.

Expanded switcher/link-localization evidence: the Release build and all 766 .NET tests pass, including native labels/overrides, generated-page switch targets, rewritten/opted-out links, and actual preview requests. Sample generation and four contrast-math checks pass; all 72 Chromium/WebKit desktop/mobile cases pass, including no-JavaScript, keyboard, and switcher contrast checks. Firefox still fails at browser startup with the same profile-folder error, so its 36 cases remain unverified. This is not a three-engine acceptance pass or release authorization.

Theme-owned markup follow-up: the Release build and all 790 .NET tests pass, including custom derived components, encoded nested notice markup, and missing/hidden notice rejection. Regenerated sample artifacts, four contrast-math checks, and all 72 Chromium/WebKit cases pass. Firefox was not rerun for this extraction; the earlier startup blocker remains an outstanding verification limitation.

Firefox startup follow-up: the existing Playwright Firefox 155.0 build launches with explicit `MOZ_APP_DATA` and `MOZ_LOCAL_APP_DATA` directories. The test configuration now supplies isolated roots under ignored browser artifacts without changing browser security settings or personal profiles. The normal `npm test` command passes all 108 Chromium/Firefox/WebKit desktop/mobile cases and four contrast-math checks, clearing the browser-startup blocker. This does not resolve the separate regeneration and hidden-message defects recorded in the [acceptance audit](https://github.com/getscissorhands/Scissorhands.NET/issues/104#issuecomment-5812385148), or imply complete issue acceptance.

Acceptance-gap closure: `db222cb` repairs output ownership and pre-write reconciliation; `3ebf901` validates exposed fallback-message text. The original audit reproductions now retain renamed output ownership, remove both variants after withdrawal, complete real-file directory transitions, and reject hidden message children. Fifteen regeneration cases and eleven additional notice cases cover these failures, unmanaged blockers, retry recovery, and permitted custom formatting. The Release solution build and all 816 .NET tests pass; sample root/subpath generation, four contrast-math checks, and all 108 three-engine desktop/mobile browser cases pass without skips or retries. This closes the reproduced AC 9/18 gaps and their coverage follow-up under the agreed contract; it does not claim transactional output rollback, arbitrary CSS visibility analysis, or broader security/release certification.

## Document control

| Field | Value |
| --- | --- |
| Document version | 0.13 |
| Status | Review-ready |
| Last updated | 2026-09-16 |
| Scope | Retained vNext/#81 baseline, FR-012 locale generation and merged #89 preview mounting; combined V-009 verification recorded separately from the historical runs |
| Code baseline | Locale implementation `de526bd` merged with `da895fd` (#101/#89); historical V-003/V-008/V-009 baselines retained below |
| Intended audience | Site owner and engine/theme/plugin contributors making baseline and compatibility decisions |
| Product owner / reviewers | @justinyoo |
| Target release / date | Not specified; this document does not schedule a release |
| Sign-off | @justinyoo approved v0.11 for the locale scope only on 2026-09-16; historical v0.6/v0.8 approvals and v0.10 component criteria retained |
| Approval scope | FR-012, Q-006 through Q-012, V-009 acceptance and RD-006 migration/risk obligations, including their application of shared requirements. No approval of unrelated revisions, waiver of existing gaps, implementation authorization or release sign-off |
| #81 scoped approval | @justinyoo approved FR-011, V-008's acceptance structure and RD-005's feature mitigation/residual-risk requirements on 2026-09-13; [approval record](https://github.com/getscissorhands/Scissorhands.NET/issues/81#issuecomment-5653569790) |
| Feature decision | @justinyoo confirmed #81's replacement scope and two-tier source-less compatibility policy on 2026-09-13; this is behavior confirmation, not whole-document sign-off or delivered code |
| Component acceptance | @justinyoo confirmed #81-only contrast thresholds and three-engine desktop/mobile automation on 2026-09-14; [decision record](https://github.com/getscissorhands/Scissorhands.NET/issues/81#issuecomment-5654089522) |
| Locale revision | @justinyoo requested this revision and confirmed Q-006 through Q-012 on 2026-09-16; individual feature decisions are not whole-document sign-off or implementation authorization |
| Locale scoped approval | Explicit user approval of PRD/TRD/TDD only for this scope on 2026-09-16; reviewed document revision `94fc60b`. This entry records approval without changing requirements or document versions |
| Implementation authorization | @justinyoo separately requested implementation within the agreed scope on 2026-09-16; this authorizes the recorded code and verification, not broader scope or release |

**Readiness:** v0.13 remains Review-ready overall, retaining locale-only and historical approvals. V-009's scoped acceptance is complete after integrating #89 and verifying the combined implementation: 678 .NET tests, 60 browser cases, four contrast-math cases and actual prefixed preview/browser/regeneration checks pass. Earlier failures remain historical; broader programs and release authorization are not implied.

**Historical behavior before #104:** FR-001 through FR-011 retained their baseline except for the explicit FR-012 locale-enabled qualifications. The amendment above supersedes that locale contract; historical acceptance gaps and approvals below are retained.

## 1. Overview and evidence

ScissorHands.NET enables a developer to maintain a personal blog as local Markdown files, generate posts and independent pages through reusable Razor themes, and publish static HTML and assets without operating a Blazor application server for readers. Optional plugins transform content or the final document. A separate local preview mode supports the authoring loop.

The product direction comes from the user's requests in [Original discussion (archived)](https://github.com/getscissorhands/Scissorhands.NET/blob/464ce0f3454d473d4a39bc6f5c9005e86cd5396a/DISCUSSIONS.md): a Blazor-based static site generator, personal blog posts with frontmatter, independent pages such as About and Contact, interchangeable themes, and pre/post-conversion extensions. The current codebase implements this foundation and adds tags, a 404 page, opt-in hierarchical page navigation, directory-index page routes, shared URL handling, and preview regeneration.

The product addresses the owner's need for a reusable .NET-based publishing workflow. In feedback on 2026-09-11, the owner confirmed that the workflow already saves time, makes customization substantially easier, and meets their expectations. These are user-reported outcomes supported by direct experience, not merely hypothetical benefits.

This is qualitative evidence: the amount of time saved and degree of improvement have not been measured numerically. That limits claims about the size of the improvement or benefits for other users; it does not invalidate the owner's experience. Quantitative benchmarks or market research are not prerequisites for recognizing that this personal blogging platform meets the owner's needs.

### Evidence and interpretation

| Source | What it establishes | Limits |
| --- | --- | --- |
| User statements in [Original discussion (archived)](https://github.com/getscissorhands/Scissorhands.NET/blob/464ce0f3454d473d4a39bc6f5c9005e86cd5396a/DISCUSSIONS.md) | Original product intent and personal-blog use case | Illustrative Mermaid and analytics plugins were examples, not completed implementations |
| Assistant responses in that discussion | Historical solution suggestions | Feed, sitemap, search, renderer snippets, and lifecycle explanations are not approved requirements or verified implementation evidence |
| [Root guide](README.md), [Web guide](src/ScissorHands.Web/README.md), package guides, and source links below | Current configuration, contracts, and observable code behavior | Existing code does not itself prove release readiness or stakeholder approval |
| [Repository guardrails](AGENTS.md) | Compatibility, validation, data handling, and testing expectations | Guardrails explicitly do not certify existing security coverage |
| User clarifications on 2026-09-11 | Current vNext baseline is the scope; unimplemented ideas are deferred; authoring is owner-controlled with trusted executable extensions | Does not approve proposed quality targets or a release |
| Owner's PRD review feedback on 2026-09-11 | The workflow saves the owner time, makes customization substantially easier, and meets their expectations | Direct qualitative evidence for this owner; no numerical improvement or broader-user result is claimed |
| User's acceptance of quality recommendations on 2026-09-11 | Basic built-in-theme accessibility, desktop/mobile browser coverage, and benchmarking the actual blog without a formal SLA or maximum supported site size | Confirms expectations, not successful implementation checks, WCAG conformance, or release approval |
| User's next-phase decision on 2026-09-11 | The seven implementation-verification areas are deferred to the next phase | Confirms the verification work's placement, not its execution, results, deadline, or release approval |
| User's approval of both documents on 2026-09-11 | Approval of the PRD and TRD requirements and their documented limitations/deferrals | Does not resolve unspecified technical details, supply verification results, assign execution owners, or authorize publication/deployment |
| Merged [theme URL helper change](https://github.com/getscissorhands/Scissorhands.NET/pull/86), [navigation change](https://github.com/getscissorhands/Scissorhands.NET/pull/87), and user update request on 2026-09-13 | Current implementation baseline and authorization to align these documents; detailed behavior is in the [website documentation handoff](docs/website-documentation.md) | Source-backed behavior and existing regression cases are not new document sign-off, completed release verification, or evidence that the website handoff has been published |
| User-confirmed [#81 decision comment](https://github.com/getscissorhands/Scissorhands.NET/issues/81#issuecomment-5653313907) and document update request on 2026-09-13 | Filename-based, index-first, depth-first reading order and automatic previous/next links; retain slug-based grouping and replace the original metadata/JSON proposal | Confirmed implementation scope, not current behavior or issue completion; the issue remains open until delivery |
| User's implementation request and scoped execution on 2026-09-13 | #81 code, sample and guides implemented; Release solution build and 493 tests passed on Windows; root HTTP, controlled prefix/locale serving and Chromium interactions observed under V-008 | Preview-prefix serving returned 404 at the configured prefix; other browsers/devices, formal contrast acceptance and wider verification remain outstanding. No release or issue-closure claim |
| Locale discussion and focused decisions on 2026-09-16 | Owner selected default-locale root redirect, discovered locales, organizational folders, strict locale browsing, conditional legacy redirects, routing-only presentation scope and rejection of conflicting shared-404 locale metadata | Q-006 through Q-012 record the selections. This is a requirements revision, not implementation authorization, passing verification or approval of unrelated document scope |

**Terminology:** *Confirmed* means supported by a user decision or identified source, with the basis stated. *Proposed* means not yet agreed. *Unknown* means unresolved. *Not applicable* means deliberately outside this scope with a reason. Code-backed baseline behavior is distinguished from new proposals throughout.

## 2. Users and essential journeys

The primary user is a personal-blog owner comfortable creating and running a .NET application. Secondary actors are theme/plugin authors and visitors reading the generated site. The user confirmed owner-controlled local authoring with deliberately trusted executable themes/plugins, not a service accepting untrusted uploads or sandboxing extensions. There is no editorial account or role system. This boundary does not remove input-validation, path-containment, encoding, or secret-handling obligations.

| Journey | Trigger and successful outcome | Important alternate or failure path |
| --- | --- | --- |
| J-001: Publish content | Owner edits a post or page, opts selected pages into navigation, runs build, and obtains HTML plus supported assets for a static host | Invalid metadata or route collisions fail generation; hiding a navigation item does not withhold its page |
| J-002: Preview an edit | Owner runs preview, changes content or theme files, then refreshes the browser after regeneration | Drafts are omitted; recompilation is required for Razor/C# changes; regeneration errors are logged |
| J-003: Change appearance | Owner installs/provides a compatible theme, changes `Site:Theme`, then restarts/rebuilds without rewriting Markdown | Missing or unmatched custom theme configuration fails; all seven view roles, including both tag views, are required |
| J-004: Extend generation | Extension author supplies a plugin; owner explicitly configures its ID and dependencies | Invalid identity/dependency configuration fails before hooks run; dependencies are not installed or enabled automatically |
| J-005: Read the site | Visitor browses the post index, hierarchical page navigation, post/page URLs, and available tag pages using ordinary HTTP navigation; FR-011 adds automatic previous/next links in an author-controlled reading order | Missing navigation ancestors are non-clickable groups, not pages; sequence boundaries omit unavailable links; `404.html` is generated, but the host configures not-found behavior |

## 3. Goals and outcome measurement

The owner has confirmed the workflow's usefulness through experienced time savings and easier customization. Optional measurement proposals below could quantify or track those benefits; they are not required to accept the reported outcomes and do not substitute for feature acceptance. No product telemetry collection is authorized by this PRD.

| Goal | Desired outcome | Current qualitative evidence | Optional measurement method | Quantitative baseline / target / window |
| --- | --- | --- | --- | --- |
| G-001 | Owner can publish a personal blog without operating a .NET server in production | Static-only hosting is the original intent; current build behavior is documented in FR-001 | Owner records successful publishes and any runtime-server dependencies at the chosen host | Not specified |
| G-002 | Owner saves time maintaining posts and independent pages without changing engine code | Owner reports that the workflow saves time and meets expectations | Observe the edit-preview-publish journey and record interventions and elapsed time | Not measured / not set / not set |
| G-003 | Owner finds customization easier while keeping presentation and transformations independent of content authoring | Owner reports substantially easier customization and expectations met | Exercise a theme replacement and plugin enable/disable workflow; record required content or engine rewrites | Not measured / not set / not set |

Guardrail observations should include broken internal links, accidental draft publication, output overwritten by conflicting inputs, and failed builds mistaken for deployable artifacts. Responsibilities and cadence for any additional measurement remain undecided (Q-004); the owner's qualitative feedback is already recorded.

## 4. Scope and priorities

FR-001 through FR-011 retain their applicable baseline and completed #81 evidence. FR-012 is **implemented with completed scoped V-009 acceptance**, including actual prefixed preview after #89 integration. This does not complete broader programs or authorize a release.

### In scope

Local .NET application setup; explicit build/preview/help modes; Markdown and supported YAML frontmatter; posts and independent pages; nested page directory indexes; date/locale URL options; index, tags, and 404 generation; opt-in hierarchical page navigation; Razor rendering; configured themes and supported assets; opt-in plugin stages and dependencies; local regeneration; current compatibility obligations.

The #81 addition is source-filename navigation ordering and automatic previous/next links across source directories, retaining existing slug-based grouping and eligibility. Eligible source-less pages follow file-backed pages using the confirmed title/route fallback. It does not change post-date ordering or title ordering on tag collection pages.

The locale addition generates language-specific collections when `UseLocaleInUrl` is enabled and redirects the generated root to the default locale. FR-012 specifies discovered locales, organizational source folders, isolated browsing and conditional legacy-tag redirects. Translation management, translated theme/site text and a language switcher remain outside this revision.

### Non-goals

This baseline does not aim to operate a hosted CMS, require a Blazor runtime on the public host, or turn reader navigation into a server-interactive Blazor application. Static generation still executes the .NET application and compiled Razor/theme/plugin code on the build machine; it is not generation without executing code.

### Outside this scope

A browser-based editor, accounts/editorial approvals, built-in contact-form processing, comments, commerce, managed hosting/deployment, and arbitrary Razor-route crawling are not part of this baseline. An independent Contact page means generated content, not a message-submission backend.

No new automatic browser reload, draft-preview mode, publication scheduler, atomic output swap, last-good-artifact preservation, bounded graceful CLI cancellation, fully offline output, or incremental-build guarantee is introduced. These exclusions do not claim such behavior would be undesirable; changing it requires a separate scope decision.

For #81, `section` frontmatter, `pages.json`, and manually authored `prev`/`next` objects (including per-link icon metadata) are intentionally superseded, not deferred completion requirements. No new ordering field, URL-prefix stripping, or automatic source-file renaming is required.

### Deferred possibilities, not roadmap commitments

| Idea | Origin | Current disposition |
| --- | --- | --- |
| Mermaid blocks rendered as images; Google Analytics injection | User examples in the original discussion | Extension points exist; concrete bundled implementations are not established in the inspected code |
| RSS/Atom feeds, sitemap, JSON search index | Historical assistant suggestions | Not produced by the inspected generator; deferred |
| Table of contents and syntax-highlighting plugins | Historical assistant suggestions | No dedicated bundled implementation is established here; Markdown code blocks alone do not establish a highlighting product |

## 5. Functional requirements

### FR-001: Build a site without starting its preview server

- **Basis / scope:** Confirmed original intent and current application/renderer behavior; Core. Sources: [application](src/ScissorHands.Web/ScissorHandsApplication.cs), [renderer](src/ScissorHands.Web/Renderers/ComponentRenderer.cs), [sample guide](sample/README.md).
- **Actor / rationale:** Owner runs the application from the site directory to obtain a deployable static artifact (J-001, G-001).
- **Behavior:** `--build` generates into `dist`; `--preview` generates into `preview` and starts a local server; `--help` displays usage. Recognized modes are explicit, not inferred from content.
- **Acceptance:** A successful build writes the generated HTML and supported assets and reports the output location without starting the preview listener. HTML rendering uses compiled Razor components through `HtmlRenderer`; serving the resulting site does not require the generation application.
- **Boundaries:** An invocation with no recognized mode reports an error and exit code 1. Preview takes precedence if both build and preview are supplied. The sample's launch profile does not select a mode; callers supply it explicitly. Build replaces existing `dist`; it does not preserve a last-good artifact (FR-009).

### FR-002: Load posts and independent pages with explicit publication rules

- **Basis / scope:** Confirmed discussion intent and [content loader](src/ScissorHands.Web/Loaders/ContentLoader.cs); Core.
- **Actor / rationale:** Owner stores `.md` files recursively under `contents/posts` and `contents/pages` to keep content independent of presentation (J-001, G-002).
- **Behavior:** Parse optional YAML frontmatter and convert Markdown. Supported fields are `title`, `slug`, `description`, `locale`, `author`, `twitter_handle`, `hero_image`, `published`, `tags`, `draft`, and `show_in_navigation`. Tags accept a YAML list or comma-separated text. Both boolean fields default to `false`; navigation opt-in applies to pages under FR-010.
- **Acceptance:** Without frontmatter, the title defaults to the filename and the slug derives from the relative path under FR-003. Malformed YAML, an unclosed frontmatter block, unsupported fields, or invalid date/boolean/tag values produce an error identifying the source and, where applicable, field. `show_in_navigation` must parse as true or false; a value such as `sometimes` fails rather than silently hiding the page. Missing content directories log a warning and supply an empty collection.
- **Publication boundary:** `draft: true` is excluded from both build and preview, including collection pages. `published` supplies ordering/date metadata; a future date does not schedule or withhold publication. This preserves the current baseline rather than introducing draft preview or scheduling.

### FR-003: Generate predictable content URLs and reject conflicting routes

- **Basis / scope:** Confirmed [loader](src/ScissorHands.Web/Loaders/ContentLoader.cs), [generator](src/ScissorHands.Web/Generators/StaticSiteGenerator.cs), and [route regression cases](test/ScissorHands.Web.Tests/Generators/StaticSiteGeneratorRouteTests.cs); Core.
- **Actor / rationale:** Owner supplies slugs and URL settings so visitors can address content predictably (J-001, J-005).
- **Behavior:** Ordinary content routes write `<route>/index.html`. `UseDateInPostUrl` prefixes dated posts, not pages, with `yyyy/MM/dd`; a missing date logs a warning and leaves the route undated. `UseLocaleInUrl` adds the normalized effective document/site locale without duplicating an existing locale prefix. `404.html` is not locale-prefixed.
- **Directory indexes:** With an omitted or blank slug, a nested page `parent/index.md` infers `parent`; the filename comparison is case-insensitive. A non-blank explicit slug wins. Root-level page `index.md` still infers `index`, not the generated homepage, and post inference is unchanged. Locale handling follows inference. Keeping both `parent.md` and `parent/index.md` with inferred slugs fails as a collision; use explicit `slug: parent/index` to retain that older nested-page URL.
- **Acceptance:** A `hello` post dated 2026-09-11 with effective locale `ko-KR` becomes URL path `ko-kr/2026/09/11/hello` when both options are enabled. A page does not acquire the post-date prefix. Literal `.`/`..` route segments and case-insensitive output collisions, including a file used as a parent directory, fail validation.
- **Boundary:** Planned content and generated routes are checked before page writes, but after the application may have removed the previous output. This is not proof of complete asset-collision or symlink protection (NFR-002).
- **Locale extension:** FR-012 adds generated locale roots and tag/redirect routes to output ownership, preserving explicit slug precedence and source inference. Source-page/generated-home collisions remain errors. Q-012 additionally rejects conflicting locale metadata on the shared 404 when enabled.

### FR-004: Provide index, tag, and not-found pages

- **Basis / scope:** Confirmed [generator](src/ScissorHands.Web/Generators/StaticSiteGenerator.cs) and [generation regression cases](test/ScissorHands.Web.Tests/Generators/StaticSiteGeneratorTests.cs); Supporting.
- **Actor / rationale:** Visitors need entry points and discovery beyond individual post URLs (J-005).
- **Behavior:** Always generate the site index and `404.html`; list posts newest first, with undated posts last. A page whose slug is `404.html` supplies the custom not-found document rather than a normal content page.
- **Acceptance:** With ordinary tagged content, produce a tag index at `tags` and per-tag routes under `tags`; group names case-insensitively via lowercase normalization and encode tag route text. Pass posts ordered by publication date descending and pages by title ascending. Exclude the custom 404 document from displayed tag collections.
- **Empty / hosting boundary:** With no eligible tagged content, emit no tag pages; an empty post collection still renders the index and 404. Generated `404.html` does not itself configure HTTP status codes or fallback routing on a static host.
- **Locale extension:** These unprefixed collection destinations describe current behavior and remain the disabled-mode contract. FR-012 replaces them with locale-scoped collections and a root redirect when enabled, with per-locale empty-tag rules and conditional legacy-tag redirects.

### FR-005: Swap compatible Razor themes without rewriting content

- **Basis / scope:** Confirmed original intent, [Theme guide](src/ScissorHands.Theme/README.md), and [theme resolver](src/ScissorHands.Web/Services/ThemeComponentResolver.cs); Core.
- **Actor / rationale:** Owner selects a theme; theme author provides compatible components and metadata (J-003, G-003).
- **Behavior:** Resolve the configured `Site:Theme` against the normalized component namespace suffix. A custom theme provides all seven roles: layout, index, post, page, not-found, tag-list, and tag views. Missing or ambiguous required roles fail resolution without implicit built-in tag-view substitution. Explicit seven-role `AddLayouts` registration remains available.
- **Acceptance:** An installed `minimal-blog` theme with a compatible namespace can be selected through configuration without listing its component types in application setup. Missing required components or an unmatched custom theme fail instead of silently selecting another custom theme.
- **Boundary:** Theme component changes need compilation; changing configured theme requires restarting/rebuilding. This is author-side theme replacement, distinct from the default theme's visitor-facing light/dark toggle. No theme marketplace, sandbox, or install UI is promised.

### FR-006: Carry supported content and theme assets into the artifact

- **Basis / scope:** Confirmed [generator](src/ScissorHands.Web/Generators/StaticSiteGenerator.cs), [theme service](src/ScissorHands.Web/Services/ThemeService.cs), and [Theme guide](src/ScissorHands.Theme/README.md); Core.
- **Actor / rationale:** Owner and theme author need images, styles, and scripts to remain available after static deployment (J-001, J-003).
- **Behavior:** Copy `contents/images` recursively into output `images`. Load theme metadata from `themes/<slug>/theme.json`; copy supported theme assets below output `themes/<slug>`.
- **Acceptance:** Files under a theme's `assets` directory are copied with their relative structure; the theme service also includes its supported image extensions and `manifest.json`. Manifest CSS/script entries are available to the theme for page inclusion. The built-in default theme remains usable through its bundled assets/default fallback.
- **Failure / limits:** Missing content images log a warning; missing/invalid custom theme manifests, including a slug that does not match configuration, fail, while missing theme asset folders can warn. Arbitrary application `wwwroot` and plugin-asset copying are not established guarantees. This requirement does not guarantee every externally referenced asset is downloaded or works offline.

### FR-007: Preserve ordered content transformation stages

- **Basis / scope:** Confirmed discussion intent, [Plugin guide](src/ScissorHands.Plugin/README.md), [generator](src/ScissorHands.Web/Generators/StaticSiteGenerator.cs), and [runner](src/ScissorHands.Web/Runners/PluginRunner.cs); Core.
- **Actor / rationale:** Extension author transforms source content or complete documents without changing the engine (J-004, G-003).
- **Behavior:** For a source post/page, run pre-Markdown plugins, Markdown conversion, post-Markdown plugins, Razor rendering, then post-HTML plugins. Each enabled plugin's returned output feeds the next.
- **Acceptance:** A pre-Markdown change affects the Markdown conversion; a post-Markdown change reaches the Razor view; a post-HTML change reaches the written file. Source-backed custom 404 content receives Markdown stages. All emitted pages, including index/tag/generated 404 pages, receive post-HTML processing.
- **Boundary:** Synthetic collection pages do not have their own source Markdown stages. `SiteManifest.IsPreview` is set before hooks and rendering so plugins can distinguish modes; the engine does not automatically suppress analytics or other plugin side effects in preview.

### FR-008: Enable plugins explicitly and resolve dependencies predictably

- **Basis / scope:** Confirmed [Plugin guide](src/ScissorHands.Plugin/README.md), [runner](src/ScissorHands.Web/Runners/PluginRunner.cs), and [dependency resolver](src/ScissorHands.Web/Runners/PluginDependencyResolver.cs); Core.
- **Actor / rationale:** Owner chooses installed plugins; extension author declares prerequisites (J-004).
- **Behavior:** Match manifests, dependencies, and Razor plugin selectors by required unique lowercase ASCII kebab-case `Id`. `Name` is display-only. Installed plugins without manifests remain disabled; configuration presence enables a plugin rather than a universal `Options.Enabled` switch.
- **Acceptance:** Invalid/missing/duplicate IDs or configured-but-uninstalled plugins fail. Declared dependencies must be installed and enabled in the declared stage. Missing/disabled prerequisites, invalid stages, self/duplicate dependencies, and cycles fail before hooks execute.
- **Ordering / limits:** Within each stage, prerequisites precede dependents; ties among ready plugins use ordinal ID order, not configuration or registration order. Dependencies are neither installed nor enabled automatically. Extension-specific options remain the extension's responsibility to validate. A valid Razor selector with no configured manifest resolves `Plugin` to null; its component author is responsible for omitting disabled output.

### FR-009: Regenerate local preview and expose failures honestly

- **Basis / scope:** Confirmed [application](src/ScissorHands.Web/ScissorHandsApplication.cs), [watcher](src/ScissorHands.Web/Watchers/ContentWatcher.cs), and [Web guide](src/ScissorHands.Web/README.md); Supporting.
- **Actor / rationale:** Owner iterates on content and sees generated results before publishing (J-002, G-002).
- **Behavior:** Perform initial generation and serve `preview`; watch content/theme file create, change, rename, and delete events, coalesce bursts, and serialize regeneration. Report successful rebuilds and instruct the owner to refresh the browser.
- **Base path:** Preview serves pages and assets only at the configured path prefix under NFR-005, including locale-enabled routes, without changing the physical output layout. With `/docs/`, browsing `/` redirects to `/docs/` without serving a duplicate homepage at root; `/docs` also redirects to `/docs/`. Other outside-prefix requests return 404. Configuring `/` retains root hosting without a redirect loop. The user confirmed prefix-only serving and then requested the root-redirect exception on 2026-09-16, superseding the initial alias-preservation assumption and the later root 404; see V-003 and TR-017.
- **Base-path configuration:** Per the user's subsequent 2026-09-16 request, the trailing slash is optional in `Site:BaseUrl`. `/docs` and `/docs/` have the same effective `/docs/` value in build and preview; nested prefixes behave likewise and `/` is unchanged. Generated links, `<base>` markup, preview mount and redirects must agree on that value without rewriting user configuration. Broader URL forms and scheme policy remain outside this change.
- **Acceptance:** After a successful content edit and rebuild, browser refresh displays the regenerated file. A watcher callback failure is logged; subsequent changes can trigger another rebuild. Initial generation errors propagate rather than being reported as completed builds.
- **Recovery boundary:** Preview regeneration writes into the existing output, so deleted/renamed sources can leave stale generated files until preview is restarted. Writes are not atomic: a failed build/rebuild may leave partial or mixed output, and initial build/preview startup clears its output folder. Use only a successful production build as a publication candidate; automated cleanup/rollback is not promised.

### FR-010: Provide a readable static default presentation

- **Basis / scope:** Source-backed current behavior in the [Web guide](src/ScissorHands.Web/README.md), [navigation reference](docs/website-documentation.md#page-routes-and-navigation), [generator](src/ScissorHands.Web/Generators/StaticSiteGenerator.cs), and built-in [layout](src/ScissorHands.Web/themes/default/MainLayout.razor); Supporting.
- **Actor / rationale:** Visitors need ordinary page navigation and owners need a usable starting theme (J-005).
- **Behavior:** Render page/site title, description, and locale through theme metadata; include theme styles/scripts and base-relative internal navigation. The default theme supplies responsive styling, a labelled light/dark control, hierarchical page navigation alongside Home/Tags, and tag links on posts and pages.
- **Navigation membership:** Only non-draft pages with `show_in_navigation: true` participate; posts and custom 404 content do not. Hierarchy follows resolved slugs, not a separate parent field. An existing non-draft page with navigation disabled suppresses its descendant branch, matching whole route segments (`parent` does not hide `parent-other`). Hidden non-draft pages remain generated and eligible for tag listings; this is not access control.
- **Hierarchy acceptance:** Missing ancestors become non-clickable groups only while they contain visible descendants; they create no output files. A hidden existing parent is not replaced by a group. Group labels derive from path segments; the #81 ordering extension is owned by FR-011. Locale prefixes alone do not create groups when locale routing is enabled, but a visible page at that prefix remains a real navigation item. Every generated layout, including index, posts, pages, tags, and 404, receives the same generation's navigation.
- **Client acceptance:** With JavaScript enabled, adjacent buttons expand/collapse child lists using mouse, touch, Enter, or Space; Escape closes the applicable group and returns focus to its button. Leaving navigation or clicking outside closes menus. Without JavaScript, the eligible hierarchy remains visible with working links. Metadata retains document/site fallbacks; the light/dark preference persists when JavaScript and browser storage are available. Reading requires no Blazor circuit or client runtime.
- **Boundary:** Optional theme/plugin JavaScript can still run in the browser. Browser coverage and basic accessibility expectations follow NFR-008; their implementation is not established by the presence of a toggle or responsive CSS. Formal WCAG conformance is not claimed. Storage-disabled preference persistence remains unestablished; no new storage fallback is promised.
- **Feature relationship:** FR-011 owns #81's new ordering and previous/next behavior, with acceptance evidence in V-008. Delivering that extension does not establish completion of this broader presentation requirement or its assessment.
- **Locale extension:** The shared-navigation rule above is the current/disabled-mode contract. FR-012/Q-009 require locale-specific navigation and Home/Tags targets when enabled; a locale without tagged content has no generated Tags link.

### FR-011: Follow an author-controlled page reading sequence

- **Basis / scope:** Confirmed by @justinyoo on 2026-09-13 in the [#81 replacement decision](https://github.com/getscissorhands/Scissorhands.NET/issues/81#issuecomment-5653313907); Supporting. The current workspace implements the two-tier reading sequence and automatic previous/next links; scoped evidence and outstanding acceptance are in V-008.
- **Actor / rationale:** The owner arranges source files without coupling reading order to display titles or URLs; visitors follow adjacent pages without the owner maintaining link metadata (J-001/J-002/J-005, G-002).
- **Ordering:** Within each directory under `contents/pages`, visit `index.md` first when present, recognizing that filename case-insensitively. Sort the remaining files and directories together by ordinal filename order. At a directory's position, visit its eligible descendants before continuing to the next sibling. Numeric prefixes are an authoring convention, not numeric-value sorting.
- **Independence:** Source paths determine the file-backed tier's reading sequence; titles provide labels and resolved slugs provide URLs and navigation grouping. Explicit slugs must not reorder otherwise eligible file-backed pages. Source and slug hierarchies can differ: previous/next follows the combined reading sequence, not a newly flattened slug tree. Existing slug inference remains unchanged; prefixes in inferred URLs are not automatically stripped.
- **Eligibility:** Reuse FR-010's navigation membership and hidden-route-ancestor rules. Posts, drafts, hidden/suppressed pages, and custom 404 content do not participate. Non-clickable groups are not targets, but their eligible descendants participate. Navigation visibility remains distinct from publication.
- **Adjacent links:** In the built-in page view, render ordinary previous/next anchors to the immediately preceding/following eligible pages across directory boundaries. Use target titles as encoded text and resolved content URLs with existing locale/base-path semantics. Do not wrap at sequence boundaries; an empty or one-page sequence has no adjacent links.
- **Pager acceptance:** Per the [2026-09-14 decision](https://github.com/getscissorhands/Scissorhands.NET/issues/81#issuecomment-5654089522), all pager text must have at least 4.5:1 contrast against its rendered background, and keyboard-focus indicators at least 3:1 against the adjacent background, in light/dark themes and relevant normal/hover/focus states. Chromium, Firefox and WebKit automation at desktop and mobile viewport sizes is sufficient browser evidence for #81. These component checks do not claim whole-site WCAG conformance or completed real Safari/iOS/device coverage.
- **Acceptance example:** With only `parent/index.md`, `parent/01-child.md`, `parent/02-group/visible-grandchild.md`, and `parent/03-child-2.md` eligible, the sequence is Parent, Child, Visible Grandchild, Child 2. Visible Grandchild links back to Child and forward to Child 2. Eligible pages outside this subtree continue the sequence. With `01-abc.md` mapped to `zulu` and `02-pqr.md` mapped to `alpha`, the former still precedes the latter regardless of their titles.
- **Modes and compatibility:** Build and preview use the same rules. Successful regeneration recomputes the sequence after source renames, additions, removals, or visibility changes; existing stale-output limitations remain. Retain custom-theme compatibility and existing grouping; custom themes may adopt the additive adjacent-page data to render their own links. Post and tag-list ordering remain FR-004.
- **Confirmed compatibility policy:** @justinyoo resolved TRD TG-006 / TDD DQ-008 on 2026-09-13 in the [two-tier policy decision](https://github.com/getscissorhands/Scissorhands.NET/issues/81#issuecomment-5653457814). File-backed pages come first in source order; eligible pages without a recorded `SourcePath` follow in ordinal title-then-resolved-route order. All eligible pages participate in one previous/next sequence, including the link between tiers. All-source-less input uses title/route ordering throughout. Titles never control file-backed reading order. Preserve public signatures and explicitly supplied navigation trees; an invalid supplied source path must not silently trigger this missing-path fallback.
- **Acceptance ownership:** FR-011 owns the feature's product criteria; TRD TR-021/TR-022 own its technical obligations, with TDD DES-012/DEC-003 providing design and decision history. The [accepted separation](https://github.com/getscissorhands/Scissorhands.NET/issues/81#issuecomment-5653514346) assigns V-008 as the feature acceptance record. Shared rendering, navigation, compatibility, URL, and accessibility requirements still apply without making all their broader work part of #81.
- **Preview-mount separation:** The user explicitly moved the preview base-path defect to [#89](https://github.com/getscissorhands/Scissorhands.NET/issues/89), removing it from #81's completion gate while retaining locale-aware previous/next checks; see the [scope update](https://github.com/getscissorhands/Scissorhands.NET/issues/81#issuecomment-5653999108). #81 owns correct target URLs, root preview navigation, and requests against correctly mounted static output, including locale/subpath combinations. Actual preview-prefix mounting remains required separately under NFR-005/V-003 and DEC-002.
- **Completion boundary:** The approved feature obligations, compatibility policy, V-008 evidence and RD-005 mitigation are complete. The implementing PR can close #81 on merge. #89 and broader programs remain independent; this document does not merge or directly close an issue.
- **Locale extension:** FR-012 applies the same ordering and adjacency rules separately within each locale when enabled. This is new scope under V-009, not a retroactive change to #81's acceptance or historical evidence.

### FR-012: Browse generated collections within a locale

- **State / source:** Approved on 2026-09-16, then separately authorized for implementation and integration with the completed #89 fix. Implemented with passing scoped V-009 evidence, including actual preview-prefix requests. No release authorization is implied.
- **Actor / rationale:** Visitors reading one language need matching entry and discovery pages instead of returning from localized content to mixed-language listings (J-005). Owners continue authoring Markdown and using Razor themes rather than maintaining collection-page Markdown.
- **Enablement / compatibility:** Apply the new behavior under `Site.UseLocaleInUrl: true`; preserve current unprefixed generation when false. Existing sites already using true need a documented route migration, not a claim of transparent compatibility.
- **Locale inventory (Q-007):** Always include `Site.Locale`, plus effective locales of published, non-404 posts/pages. Group normalized equivalents such as `en-US` and `en_US` as one locale. No supported-locale list or inference from directory names is added. Draft-only locales do not participate; publication retains FR-002's rules, including future-dated content. A navigation-hidden but published page still contributes its locale.
- **Generated collections:** For each participating locale, generate `<locale>/index.html`, listing only its posts, even if empty. Generate `<locale>/tags/index.html` and per-tag pages only when that locale has eligible tagged content. Retain newest-first posts, title-ordered tagged pages, draft exclusion and the custom-404 exclusion. A locale with no tags has no tag pages or built-in Tags navigation link. No Markdown homepage or tag index is required.
- **Confirmed root entry point (Q-006):** Generate root `index.html` that redirects to the locale homepage selected by `Site.Locale`. With base `/blog/` and default `en-US`, `/blog/` leads to `/blog/en-us/`, not `/en-us/` or a mixed-language root homepage. This is static output, not a production .NET endpoint; TRD/TDD specify the portable mechanism and failure safeguards.
- **Browsing scope (Q-009):** Engine-generated Home/site-title links, tag links, page navigation and previous/next remain in the active locale. Reuse FR-010 eligibility and FR-011 ordering within that locale; do not cross languages at source-directory or source-less-tier boundaries. Do not substitute another language's content when a translation is absent. This controls generated discovery/navigation, not author-written links, publication permissions or a new translation-pair model.
- **Authoring convention (Q-008):** Retain `contents/posts` and `contents/pages`, with optional locale subdirectories. Document locale comes from frontmatter, otherwise `Site.Locale`; folders do not infer, validate or override it. Recommend explicit `locale` plus locale-free `slug` for locale-folder content. Example: `contents/pages/ko-kr/about.md` with `locale: ko-KR` and `slug: about` produces `/ko-kr/about`. Missing/blank slugs still use existing path inference, including source folders; nothing strips folder/numeric prefixes or mandates new frontmatter fields. No `contents/<locale>/posts` discovery or automatic source rewriting is added.
- **Legacy tag routes (Q-010):** Generate redirects from unprefixed `tags` and `tags/<tag>` only to corresponding generated default-locale destinations. A tag used only in another locale has no legacy redirect; neither does a tag index when the default locale has no tagged content. An otherwise unowned missing route follows the host's normal not-found behavior, not a different-language listing or a dangling redirect. Do not retain shared mixed-language tag listings when enabled.
- **Presentation boundary (Q-011):** Generated locale pages expose matching locale metadata/context to themes and plugins. Retain seven theme roles, shared assets and existing site/theme text defaults. No translated UI catalog, translated site settings, language switcher, translation pairing or per-locale 404 is added. An English theme label on a Korean route is not automatically translated by this feature.
- **Shared 404 (Q-012):** Keep one root `404.html` with default-locale context, including default-locale navigation. When enabled, an explicit custom-404 locale that normalizes differently from `Site.Locale` fails generation with source/field context; omission uses the site default. Equivalent spellings are accepted. Its content remains owner-authored, does not create a locale and is not automatically translated. Disabled-mode behavior remains unchanged.
- **Acceptance example:** Given published English and Korean posts sharing tag `dotnet`, each locale homepage and tag page lists only its own content and links to the correct locale-prefixed documents. `/blog/` reaches the default locale homepage. With routing disabled, the original root homepage and shared tag listings remain. Route collisions and unsafe destinations fail explicitly rather than overwriting generated collections.
- **Dependencies / evidence:** Q-006 through Q-012, TR-023 and DES-013/DEC-004 retain locale-only approval. DQ-009's scoped context/helper/snapshot integration is covered, not broader gaps. V-009 owns locale evidence; NFR-002/003/005/006/007/008/010 still apply. The independently implemented #89 middleware is now merged, and actual combined prefix requests pass; generation alone was not treated as proof of mounting.

**Authoring examples:** for `BaseUrl: "/blog/"`, locale routing enabled and dated post URLs enabled, organizational folders plus explicit slugs keep routes independent of source placement:

| Source under `contents` | Frontmatter | Resulting URL |
| --- | --- | --- |
| `pages/en-us/about.md` | `locale: en-US`, `slug: about` | `/blog/en-us/about` |
| `pages/ko-kr/about.md` | `locale: ko-KR`, `slug: about` | `/blog/ko-kr/about` |
| `pages/ko-kr/docs/index.md` | `locale: ko-KR`, `slug: docs` | `/blog/ko-kr/docs` |
| `posts/ko-kr/hello.md` | `locale: ko-KR`, `slug: hello`, `published: 2026-09-16` | `/blog/ko-kr/2026/09/16/hello` |

The generated root and locale homepages are output only; do not add a source `index.md` to claim those routes. Existing sources that collide must use a distinct explicit slug.

## 6. Quality requirements and constraints

The user confirmed the following quality baseline on 2026-09-11. Agreed requirements and documented limitations are distinct from evidence that the implementation satisfies them. Verification is deferred to the next phase in Section 7; it is engineering and release work, not an unanswered decision about whether a requirement applies.

| ID | Area and requirement | Observable acceptance or gap | State / basis |
| --- | --- | --- | --- |
| NFR-001 | Local authoring and extension trust | The owner controls builds and deliberately installs trusted executable themes/plugins; no untrusted multi-user upload or extension sandbox promise. Content metadata still requires validation, and trusted authoring does not exempt generated HTML from visitor-safety obligations | Confirmed by the user on 2026-09-11; Q-001 resolved |
| NFR-002 | Filesystem containment and unintended overwrites | Reject content-derived traversal/symlink escapes and conflicting output owners, including assets. Existing route cases demonstrate only part of this obligation; full read/copy/write coverage remains unverified | Confirmed policy in AGENTS.md; implementation completeness unknown, Q-002 |
| NFR-003 | Encoding and raw HTML | Preserve Razor's metadata encoding and reject unsafe URL schemes at relevant boundaries. Raw Markdown/plugin HTML is an explicit trust boundary, not universally sanitized content. Do not claim all theme overrides are safe | Confirmed policy; [Markdown service](src/ScissorHands.Web/Services/MarkdownService.cs) and Theme guide establish raw-content rendering; coverage unknown, Q-002 |
| NFR-004 | Diagnostic and artifact integrity | Invalid input and generation failures must not be presented as successful builds. Missing optional content/assets can warn as specified above. No atomic publication or last-good-output guarantee | Confirmed baseline and policy; FR-001, FR-002, FR-006, FR-009 |
| NFR-005 | Subpath hosting | Generated internal links and theme/plugin asset URLs must respect `SiteManifest.BaseUrl`, including `/docs/`. Assess by serving an artifact at its intended prefix; a `<base>` tag alone does not prove deployment/preview correctness | Confirmed documented contract; host/path behavior must be evaluated, Q-002 |
| NFR-006 | Cancellation | Preserve cancellation-aware service contracts and propagation where tokens are supplied. Cancellation must surface instead of yielding a completed artifact | Confirmed policy and service contracts. Production CLI currently calls generation with `CancellationToken.None`; no graceful CLI cancellation-time bound is claimed |
| NFR-007 | Compatibility and distribution | Keep .NET 10 package boundaries and documented migration behavior. Legacy theme-service overloads remain obsolete but available. Read-only manifest collections, required plugin IDs/`PluginDependency.PluginId`, all seven required theme roles, and nested page directory-index inference are intentional vNext compatibility changes. Retain flat navigation and existing protected URL helpers | Source-backed [migration reference](docs/website-documentation.md#upgrading-to-vnext); update/rebuild consuming extensions, supply missing tag views, and set explicit slugs where older URLs must remain |
| NFR-008 | Accessibility and browser coverage | The built-in theme must support keyboard navigation, visible focus, meaningful labels, and readable contrast in light/dark modes across the browser coverage below. Custom-theme authors own their themes' accessibility. No formal WCAG conformance claim or comprehensive assistive-technology certification is made for this baseline | Confirmed by the user on 2026-09-11; Q-003 resolved. Implementation coverage remains to be evaluated |
| NFR-009 | Performance and scale | Use the owner's actual blog, including its assets and enabled themes/plugins, as the representative workload on a documented development environment. Record build duration, preview-update delay, and memory usage. This baseline sets no formal performance SLA, numerical pass/fail thresholds, or maximum supported site size; measurements inform any later targets | Confirmed by the user on 2026-09-11; Q-003 resolved. Benchmark execution and results remain pending; absence of a declared size limit is not an unlimited-scale guarantee |
| NFR-010 | Privacy and external requests | Do not expose local secrets in generated HTML, logs, or fixtures. No built-in analytics implementation is established; optional plugins/themes may introduce external requests and obligations | Confirmed policy; the [site manifest](src/ScissorHands.Core/Manifests/SiteManifest.cs) includes a remote default hero-image URL, so zero-network/offline behavior is not promised |

**FR-011 compatibility:** filename-based ordering intentionally replaces title-first engine navigation ordering. Preserve public signatures, layout-only navigation, and the flat-list compatibility path; document the changed ordering and additive theme data when implemented. Renaming source files can change inferred URLs, so use explicit slugs where existing addresses must be retained.

**Browser coverage (NFR-008):** target the current stable versions of Edge, Chrome, Firefox, and Safari on desktop, plus Chrome on Android and Safari on iOS. Record the actual browser, OS, and device versions used when evaluating a release. This is agreed coverage for generated-site reading/navigation and the built-in theme, not evidence that all combinations have passed.

**#81 evidence boundary:** FR-011/V-008 use the explicitly approved three-engine desktop/mobile automation and component-only contrast ratios. The wider real-browser/device matrix and broader theme assessment remain in V-005; they are not prerequisites for closing #81. The global quality requirement is retained, not declared satisfied by engine emulation.

**Benchmark record (NFR-009):** identify the blog snapshot, post/page counts, asset volume, enabled theme/plugins, machine specifications, OS, and .NET SDK used, together with the measurement method and results. The particular snapshot and machine details are execution inputs to record when benchmarking, not new product-scope decisions. Do not infer an SLA from a single measurement or require arbitrary numerical targets to accept this baseline.

**Additional coverage:** FR-012 implements locale-scoped collections and navigation alongside existing locale metadata/URL prefixes. Translation management and a localized UI catalog remain outside this revision under confirmed Q-011. Accounts, account-data retention/deletion, payments, entitlements, and AI behavior are not applicable because this scope contains no such services. Local content/output lifecycle is covered by FR-002 and FR-009; generated artifacts remain on disk until removed/replaced. No special compliance certification or regulated workflow was supplied. Third-party analytics/consent obligations must be addressed if an analytics integration is separately scoped.

## 7. Next-phase verification, release, and evaluation

### Next phase: implementation verification

The user agreed on 2026-09-11 to keep V-001 through V-007 as broad next-phase verification areas. They remain open. On 2026-09-13, the user accepted V-008 as a separate #81 record and subsequently requested implementation. Feature-specific cases are consolidated there, not removed from scope; recorded execution below is distinct from source inspection or broader-program completion.

| ID | Area / requirements | Verification scope and expected evidence |
| --- | --- | --- |
| V-001 | Filesystem safety / NFR-002 | Exercise content reads, generated-page writes, and asset copying against traversal, symbolic-link escapes, and output collisions. Record containment and overwrite-protection results, including gaps beyond existing route tests |
| V-002 | Rendering and privacy / NFR-003/010 | Check metadata encoding, unsafe URL schemes at relevant boundaries, and accidental secret exposure in generated output/logs using synthetic fixtures, not real credentials. Preserve supported raw Markdown/plugin HTML as an explicit trust boundary; record results and any unsafe paths |
| V-003 | Subpath hosting / NFR-005 | Exercise preview and production output at `/` and prefixes such as `/docs/`, including pages, locale routes and assets. #89's preview-mount checks pass in the execution record below; the broader program remains open. Record serving configuration and request results; helper assertions alone do not establish mounting. V-008's evidence remains separately scoped |
| V-004 | Failures and cancellation / NFR-004/006 | Exercise invalid input, generation failures, and cancellation-aware APIs. Confirm actionable errors and no misleading success; record partial-output behavior against documented limitations, without introducing atomic-output or bounded CLI-shutdown guarantees |
| V-005 | Accessibility and browsers / NFR-008, FR-010 | Evaluate reading/navigation, nested disclosure buttons, keyboard/touch use, Escape/focus return, outside dismissal, no-JavaScript links, labels, and light/dark contrast across the agreed desktop/mobile browsers. Record actual browser/OS/device coverage and findings; no formal WCAG certification is required. V-008 separately records the #81 controls |
| V-006 | Performance baseline / NFR-009 | Benchmark the owner's actual blog with its assets and enabled extensions. Record the workload/environment details, build duration, preview-update delay, memory usage, and measurement method. Completion establishes measurements, not compliance with an unagreed numerical target |
| V-007 | Functional regression and compatibility / FR-001 through FR-010, NFR-007 | Run existing suites and sample build/preview; cover metadata defaults/errors, directory-index overrides/collisions, navigation visibility/groups/layout delivery, shared URL semantics, tags/404, seven-role theme discovery, plugin ordering, and migration compatibility. Reuse V-008 for FR-011-specific evidence without treating it as completion of this system-wide program. Record results and review Windows/macOS/Linux CI evidence without assuming untested platforms passed |
| V-008 | #81 feature acceptance / FR-011, TR-021/TR-022 | Complete under the approved #81 scope: prior engine/root-preview evidence plus 42 passing three-engine browser checks, 4 contrast-math tests and passing rendered ratios. #89 and broader V-005 remain outside this gate |
| V-009 | Locale-generation acceptance / FR-012, TR-023 | Complete within the approved scope: merged implementation passes 678 .NET tests, 60 browser cases and four math cases; actual prefix-only Kestrel generation, redirect chains, slash variants and watcher regeneration pass. Historical failures and independent #89 evidence remain below |

For each item, record what was exercised, the environment, results, and reproducible gaps. Failed, blocked, or skipped checks remain explicit; an attempted check is not a passing result. Track necessary corrections against the existing requirements, then recheck affected behavior. The benchmark needs reproducible measurements, not an invented speed threshold. These records are next-phase deliverables, not prerequisites for finishing the requirements document.

### V-009: Locale implementation evidence, 2026-09-16

**State at the initial locale revision (`de526bd`):** implementation delivered; acceptance was incomplete because #89 blocked real preview-prefix requests. These results are retained as historical evidence. The integration follow-up records the combined behavior after merging the fix; prior locale-only approvals are unchanged.

| Area | Evidence | Outcome / boundary |
| --- | --- | --- |
| Build and .NET regressions | Windows, SDK 10.0.401, Release build with zero warnings/errors; full suite 620/620, including Web 344/344 | Passed, no skipped tests. Covers optional context/cascades/helpers, normalization/folder/date rules, scoped collections/adjacency, empty/default/draft-only scopes, shared 404, redirects, collisions and snapshot behavior |
| Browser matrix | Node 24.18.0, Playwright 1.63.0; Chromium 153.0.8010.12, Firefox 155.0 and WebKit 26.6 at 1280x800 and 375x812; final 60/60 plus 4 contrast-math cases | Passed with no configured retries. Existing pager checks plus 18 new locale cases use real root/multi-locale subpath artifacts on controlled loopback static hosts; minimum text/focus ratios remain 8.58/9.51 light and 10.22/12.36 dark |
| Real root preview | Isolated multi-locale sample: root, locale homes, page/tag targets, root 404 and shared assets returned 200; tagless/draft-only/missing-default legacy routes returned 404 | Passed. Adding then removing a temporary English page regenerated English adjacency without changing Korean navigation; source restored and preview stopped |
| Actual prefix preview | With `BaseUrl: "/docs/"`, `/docs/`, `/docs/ko-kr/` and `/docs/en-us/tags/dotnet/` returned 404, while equivalent unprefixed files returned 200. Root redirect correctly targets `/docs/ko-kr/` | Failed under existing #89. No middleware workaround, nested deployment-prefix output or acceptance waiver was introduced |
| Output integrity and migration | Preflight includes every generated/redirect destination; per-write ownership rejects plugin-transformed page collisions and locale-enabled content-image overwrites. Locale-output reparse-point regression rejects linked destinations. Guides document source/theme/plugin migration | Scoped cases passed, not a complete filesystem/symlink/asset audit. Theme-service copy ownership and the wider DQ-002/DQ-006 programs remain separate |

**Earlier attempts:** Web's first run exposed five over-broad new test selectors that included the existing All tags backlink; correcting those assertions accompanied discovery/fixing of the actual hard-coded built-in back links. The first full browser run passed 59/60, with one Firefox-mobile same-URL navigation interruption in an existing test. That isolated test passed three repetitions, then the complete unchanged-criteria matrix passed 60/60. Missing sample restore assets and browser test dependencies were restored/installed after their commands failed; no dependency versions changed. These earlier failures are not relabeled as passes.

Root and legacy redirects are generated HTML, not claimed HTTP 3xx responses. That execution did not establish real-device coverage, broad security/compliance conformance, a performance SLA, package publication or release readiness. V-009 was blocked pending #89's actual prefix behavior; broader V-001 through V-007 are not closed by integration.

#### Integration follow-up after merging #89: 2026-09-16

The user requested merging `vnext` after #89 was fixed. The combined tree integrates locale implementation `de526bd` with `da895fd` from #101, preserving both branches' approvals, requirements and execution history. **The former V-009 preview-prefix blocker is resolved by actual integrated execution**, not by waiving the check or substituting a controlled static host.

| Scope | Integrated evidence | Outcome |
| --- | --- | --- |
| Release/.NET | Windows, SDK 10.0.401; solution build has zero warnings/errors; full suite **678/678**, including Web **385/385**, with no skips | Passed |
| Real generation and preview middleware | New `ScissorHandsApplicationLocaleTests` has **14** Kestrel cases using the real loader/generator/renderer, `/`, `/docs`, `/docs/`, `/blog`, `/blog/`, `/manual/docs`, `/manual/docs/`, and both locale modes | Passed: canonical `<base>`, HTTP mount and HTML locale/legacy redirects, page/tag/post/asset requests, outside-prefix 404, unchanged build/preview file inventories and regeneration callbacks |
| Browser/artifact regression | **60/60** Chromium/Firefox/WebKit desktop/mobile cases and **4** math cases; the fixture builder now compares every generated byte for `/docs` and `/docs/` | Passed with no skips or retries; source configuration and normal sample content unchanged |
| Live sample preview | Isolated real multi-locale sample configured with `/docs` and default `ko-KR`: **17** direct HTTP checks, plus no-JavaScript flows in Chromium 153.0.8010.12, Firefox 155.0 and WebKit 26.6 at 375x812 | Passed: `/` HTTP 302 to `/docs/`, generated HTML redirect to `/docs/ko-kr/`, locale/legacy tag navigation, English adjacency, prefixed assets and outside-prefix rejection |
| Real filesystem watcher | Added then removed a temporary English page while the prefixed sample preview was running | English adjacency regenerated and the new target returned 200 beneath `/docs/`; Korean navigation stayed isolated. Source restored and temporary server stopped |

The locale regression that formerly rejected `/blog` was updated to honor shared `SiteManifest.BaseUrl` normalization; unsupported/unsafe forms still fail. The locale HTML redirect and preview HTTP mount redirect remain separate stages. Scoped V-009 acceptance is complete on the recorded environment; wider V-003/V-005/V-007, DQ-002/DQ-006, real-device/third-party-host assessment and release authority remain separate.

### V-003: #89 preview-mount execution, 2026-09-16

The user requested the fix after issue analysis. On Windows with .NET SDK **10.0.401**, Release configuration, and code `3e50dab` plus the initial fix committed as `4b32a4c`, the original middleware reproduced the reported prefix 404. The new [preview HTTP regression](test/ScissorHands.Web.Tests/ScissorHandsApplicationTests.cs) first passed both root cases and failed both `/docs/` cases; after prefix handling was added, all six expanded cases passed. The following table records that initial run; its root-alias behavior is superseded by the prefix-only follow-up below.

| Scope | Observed evidence | Outcome |
| --- | --- | --- |
| Actual application middleware | Ephemeral Kestrel servers at `/`, `/docs/`, and `/manual/docs/`, each with locale routing on/off; fixture generator supplies known files through the real preview startup path | Six cases passed |
| Request behavior | Home, nested/encoded pages, dated/locale posts, tags, explicit 404 document, CSS/JS/images, CSS HEAD, directory and mount-root redirects with retained queries, missing routes/assets, nonmatching/doubled prefixes and retained unprefixed aliases | Passed; no generated prefix directory or file-inventory changes |
| Real sample build and preview | Four configurations: `/` and `/docs/`, each with `UseLocaleInUrl` false/true and `Locale=ko-KR`; isolated sample processes using `--preview` and `--build`, with preview listening on `http://127.0.0.1:0` | 56 HTTP checks passed; each build/preview pair had the same 21 artifact-relative files |
| Reported failure and generated links | `/docs/ko-kr/parent/group/visible-grandchild/`, its generated Child/Child 2 neighbor targets, and prefixed theme/image requests | Returned 200; directory redirects returned 301 retaining prefix/query, and missing routes returned 404 |
| Regression gate | Release solution build, affected Web suite, then `dotnet test --solution ./ScissorHands.slnx -c Release --no-build --verbosity normal` | Build succeeded; Web 290/290 and full suite 503/503 passed, no skips |

The automated HTTP fixture isolates serving from generation; the separate sample run exercises real generation and rendered neighbor URLs. Temporary servers were stopped and sample configuration overrides were child-process-local. The initial fix did not change URL helpers, locale generation, production-host responsibilities, or watcher behavior. Root and slash-delimited path prefixes are the documented usage; other base-URL forms and broader scheme policy remain TG-001. Retaining unprefixed aliases was an implementation assumption, not a user-confirmed compatibility requirement.

The historical September 13/14 failures below remain valid for those revisions. This run did not rerun browser/device tests, test representative third-party plugin output, or certify other operating systems or a production host. It does not close the broader V-003 program, TG-001, V-005, or authorize publication or issue closure. #81/V-008 acceptance remains unchanged.

#### Prefix-only follow-up: 2026-09-16

The user clarified that preview must serve only beneath `Site.BaseUrl` and requested the correction, committed as `589d45f`. With a non-root prefix, this revision returned 404 outside the mount instead of serving duplicate root aliases; `/docs` still redirected to `/docs/`, and root configuration `/` was unchanged. No new authentication or general URL-normalization policy was introduced. Its root 404 is superseded by the root-redirect follow-up below; these results remain historical evidence for that revision.

On Windows with .NET SDK **10.0.401**, Release configuration, and `4b32a4c` plus this follow-up, the revised tests first failed all four non-root cases because `/` returned 200; both root cases still passed. After isolating static-file middleware inside the prefix branch, all six cases passed, including outside-prefix page/asset GET and CSS HEAD rejection, no outside redirects, locale/date/tag routes, mount/directory redirects with queries, and unchanged file inventories. The affected Web suite passed **290/290** and the full solution passed **503/503**, with no skips, after a successful Release solution build.

Real sample preview passed **74 HTTP checks**: `/` with locale off/on had 12 checks each; `/docs/` with locale off/on had 25 each, using `Locale=ko-KR` and ephemeral loopback ports. Valid page/neighbor/theme/image URLs returned 200, outside-prefix and nonmatching/doubled-prefix requests returned 404, and redirects retained the prefix/query. Each preview contained 21 files with no physical `docs` directory. Configuration overrides were child-process-local, user sample settings were preserved, and all temporary servers were stopped. The earlier 56-check build/preview comparison remains historical evidence, not a rerun claim. Browser/device, third-party plugin and production-host verification were not rerun; broader evidence boundaries remain unchanged.

#### Root-redirect follow-up: 2026-09-16

The user requested that `/` redirect to `Site.BaseUrl`. With a non-root mount, GET/HEAD `/` now returns **302**, an empty body, and a same-origin path `Location` pointing at the mounted homepage with the query preserved. Other outside-prefix requests remain 404. Root configuration remains 200 without a redirect. The six actual-application HTTP cases first failed all four prefixed configurations on the existing root 404, then passed after the redirect was added; checks include queries, HEAD, POST rejection, empty redirect bodies and successful one-hop target requests.

On Windows with .NET SDK **10.0.401**, Release, and `589d45f` plus this follow-up, the solution built successfully and **503/503** tests passed with no skips. A temporary copy of real sample content/configuration passed **94 HTTP checks**: 17 each for `/` with locale off/on and 30 each for `/docs/` with locale off/on, using `Locale=ko-KR` and ephemeral ports. Raw root responses were 200 for root hosting or 302 for a prefix; automatically followed requests ended at the base URL with their query intact. In-prefix content/assets and existing directory redirects passed, other outside-prefix requests remained 404, and each preview retained 21 files. Temporary processes and files were cleaned up; the user's running server, output and sample settings were left untouched. The existing port-5000 process was separately observed returning root 404 and `/docs/` 200 before restart, not duplicate homepage bodies. Browser-cache behavior was not diagnosed. Broader verification limitations and earlier results remain unchanged.

#### BaseUrl normalization follow-up: 2026-09-16

The user requested accepting both slash-terminated and non-terminated path prefixes in settings. `SiteManifest.BaseUrl` now supplies a missing trailing slash during initialization, covering direct .NET callers and configuration binding. `/docs` and `/docs/` both yield `/docs/`, nested prefixes follow the same rule, and `/` stays unchanged. Root redirects and prefix-only serving consume the canonical value, as do build/preview rendering and plugin site context. Source settings are not rewritten. This is not general URL validation: absolute/network-relative URLs, relative paths without a leading slash, and backslash/query/fragment-bearing values are not rewritten by the rule and gain no new safety or serving guarantee.

On Windows, .NET SDK **10.0.401**, Release, and `b3069df` plus this follow-up, the new manifest test first failed the two missing-slash cases. After normalization, **19** manifest cases and **36** focused Web binding/rendering/HTTP cases passed. The solution built successfully and **538/538** .NET tests passed with no skips. The existing HTTP matrix now covers **10** cases: `/`, `/docs`, `/docs/`, `/manual/docs`, and `/manual/docs/`, each with locale routing off/on.

A temporary real-sample copy exercised all ten configurations in both `--build` and `--preview`, with `Locale=ko-KR` and ephemeral loopback ports. **274 HTTP checks** passed; generated HTML in both modes and the logged preview URL used the canonical base. Corresponding trailing/non-trailing variants produced byte-identical build artifacts at each locale setting, and every build/preview pair retained the same 21-file layout. Root 302/query behavior, mount/directory redirects, locale-aware neighbor links, assets and outside-prefix 404s remained correct. Temporary files/processes were cleaned up; user settings and any existing preview process/output were untouched. Earlier runs remain historical evidence. No browser/device, third-party plugin, production-host or broader scheme-policy completion is claimed.
### V-008: #81 feature acceptance

**Scope and state:** complete under the explicitly approved #81 criteria. FR-011/TR-021/TR-022 remain authoritative, and the execution records below retain both the original failures and final passing evidence. Broader programs are not declared complete.

| Feature area | Required evidence for #81 | Broader evidence reuse |
| --- | --- | --- |
| Ordering and compatibility | Index-first mixed file/directory traversal; file-backed title/slug independence; all-file, all-source-less and mixed two-tier inputs; legacy/direct/flat-only callers and explicit trees | Relevant cases may support V-007; they do not cover all package/extension migrations |
| Adjacency and rendering | The agreed Parent/Child/Visible Grandchild/Child 2 sequence, cross-tier links, exclusions, empty/single/endpoint cases, additive theme context, build/preview regeneration, and retained grouping/post/tag behavior | Relevant cases may support V-007; other rendering/collection obligations remain |
| Input, encoding, privacy and failure | New source-path handling, invalid-path rejection rather than fallback, encoded target labels, no source-path exposure in new markup, cancellation, and actionable failures in sequence preparation | Scoped evidence for V-001/V-002/V-004, not a comprehensive filesystem/sink/privacy audit |
| Locale-aware links and scoped serving | Correct base-relative/locale-prefixed neighbor URLs, actual root-preview navigation, and neighbor requests against correctly mounted static output at root/subpaths | Scoped evidence for V-003; actual built-in preview-prefix mounting belongs to #89, not #81's acceptance gate |
| New controls | Chromium/Firefox/WebKit at desktop/mobile viewports: labels, keyboard and no-JavaScript navigation, endpoints, focus/layout, and the approved 4.5:1 text / 3:1 focus contrast checks in light/dark states | Sufficient component evidence for #81; broader real-browser/device and theme contrast assessment stays in V-005/TG-002 |
| Migration and residual risk | Document the ordering change, additive theme adoption and source-less fallback; demonstrate explicit-slug preservation when filenames change; record RD-005 mitigation and its residual inferred-URL consequence | Scoped compatibility evidence for V-007; no automatic source rewrite, prefix stripping or deployed rollback |

V-008 can complete independently of the broader programs and #89. The explicit scope update removes actual preview-prefix mounting from #81's required checks; it does not remove locale-aware URL correctness or root-preview/correctly mounted static-host checks. The observed preview 404 is retained under #89/V-003, not relabeled as a pass. Other failures or blockers within the remaining #81 criteria must still be reported honestly.

Reuse precisely identified V-008 results in V-001 through V-005 and V-007 without closing the whole broader item. V-006 actual-blog benchmarking is separate; #81 earns no automatic performance evidence. The seven original programs and all historical IDs remain intact. No product-wide sign-off or release authorization follows from V-008 completion.

#### Execution record: 2026-09-13

Environment: Windows, .NET SDK **10.0.401**, Release configuration, current #81 workspace based on `843272a`. A normal solution build completed with **zero warnings/errors**, and `dotnet test --solution ./ScissorHands.slnx -c Release --no-build --verbosity minimal` passed **493/493**, with no skipped tests or file exclusions. The Web project passed **280/280** in a separate normal build/run.

| Area | Observed evidence | Outcome |
| --- | --- | --- |
| Ordering and compatibility | [Reading-order cases](test/ScissorHands.Web.Tests/Navigation/PageReadingOrderTests.cs), [generation cases](test/ScissorHands.Web.Tests/Generators/StaticSiteGeneratorReadingOrderTests.cs), and [legacy renderer cases](test/ScissorHands.Web.Tests/Renderers/ComponentRendererCascadingParametersTests.cs) cover both tiers, explicit trees, invalid paths and document identity | Passed on Windows |
| Adjacency and regeneration | Default sample served About, Parent, Child, Visible Grandchild and Child 2 in the expected sequence; all adjacent targets returned 200. Hidden page, tag index and custom 404 had no pager. Temporarily hiding/restoring Visible Grandchild caused the running root preview to remove/restore its group and Child's next link; source restored afterward | Passed for the observed sample |
| Data and rendering boundaries | Core/Theme/default-page tests cover defaults, immutable values, optional cascades, encoded labels and missing endpoints; generator cases cover cancellation and snapshot behavior. New sample links exposed no absolute workspace path | Scoped checks passed; not a full filesystem/sink audit |
| Locale-aware links and scoped hosting | Root build/preview requests returned 200. A controlled static host mapping `/docs/` to `dist` served `ko-kr` neighbor URLs with 200 responses | Passed for the observed #81 cases; locale-aware checks remain in scope |
| Separately tracked preview mounting | Prefix-configured preview returned **404** for `/docs/ko-kr/parent/group/visible-grandchild/` but 200 without `/docs/` | Still failing; moved to #89 under V-003/DEC-002, explicitly not a #81 blocker |
| New controls (initial evidence) | Chromium **153.0.8010.37** on Windows at 375x812: no overflow, visible focus, repeated Tab/Enter and a JavaScript-disabled traversal | Initial observation only; superseded for component acceptance by the complete 2026-09-14 execution below |
| Migration and RD-005 | Sample source filenames/directories now demonstrate numeric ordering while explicit slugs retain the prior Child/Grandchild URLs, confirmed by successful requests. Package/theme/website/sample guidance documents ordering, fallback and custom-theme adoption | Mitigation recorded; inferred URLs still change on renaming without an explicit slug |

The controlled static host was a local Python `SimpleHTTPRequestHandler` with prefix-to-directory translation, not the application's preview middleware. Temporary servers were stopped and environment overrides were process-local. An initial combined browser probe timed out; isolated checks and three repeated Tab/Enter traversals subsequently passed. Final direction-label colors use the existing primary text palette: `rgb(68,68,68)` on `rgb(245,240,230)` in light mode and `rgb(232,232,232)` on `rgb(44,53,58)` in dark mode; the regenerated sample retained focus visibility and no horizontal overflow. These observations do not establish the unexecuted browser matrix or contrast sign-off. No package publication, deployment, broad-audit closure, or #81 closure occurred.

**Subsequent acceptance decision, 2026-09-14:** the user selected 4.5:1 text, 3:1 focus and three-engine desktop/mobile automation for #81. The initial observations alone were insufficient; the execution below applies those criteria. Broader TG-002/DQ-003 and actual device assessment remain separate.

**CI update:** [build/test jobs](https://github.com/getscissorhands/Scissorhands.NET/actions/runs/34764181111) for commit `935f537` reported success on Windows, macOS and Ubuntu. These are .NET build/test results, not execution of the three-engine browser/contrast checks.

#### Component acceptance completed: 2026-09-14

The committed [browser acceptance suite](test/browser/README.md) generates real root and `/docs/` plus `ko-kr` sample artifacts, serves them on isolated loopback ports, and runs the same controls in six engine/viewport projects. Local execution used Windows, Node **24.18.0**, Playwright **1.63.0**, desktop **1280x800**, and mobile-width **375x812**.

| Engine | Version | Browser cases | Outcome |
| --- | --- | --- | --- |
| Chromium | 153.0.8010.12 | 14 (both viewport sizes) | Passed |
| Firefox | 155.0 | 14 (both viewport sizes) | Passed |
| WebKit | 26.6 | 14 (both viewport sizes) | Passed |

All **42 browser cases**, **4 independent contrast-math cases**, and **493 .NET tests** passed, with no skipped or retried browser cases. The normal Release solution build had zero warnings/errors. Browser coverage includes endpoints, exclusions, labels, actual root/localized-subpath requests, keyboard navigation, JavaScript-disabled navigation, layout and both themes' normal/hover/focus states.

Across **168 rendered color-pair measurements**, minima were:

| Theme | Minimum text ratio | Minimum focus ratio |
| --- | --- | --- |
| Light | 8.58:1 | 9.51:1 |
| Dark | 10.22:1 | 12.36:1 |

Ratios are rounded to two decimals; assertions use unrounded values against 4.5:1 and 3:1. The baseline exposed light text at 4.41:1 and focus at 1.79:1. Pager-only foreground/outline rules now use the existing `--text` palette. WebKit also skipped implicitly tabbable anchors in its default keyboard mode; explicit `tabindex="0"` restored their native sequential keyboard behavior without positive tab ordering.

The suite records JSON results and per-state color measurements under ignored `test/browser/test-results`; PR CI runs it in a dedicated job and uploads those results. This repeatable component evidence completes the remaining V-008 gap. It does not fix #89, certify real Safari/iOS devices, complete V-005, or authorize publication/deployment. #81 can close when the implementing PR merges.

### Approved release criteria

This PRD describes a public-preview baseline, not a new release authorization. The following criteria are approved as requirements that a release must satisfy; their approval is not evidence that they have been satisfied:

1. Use the approved baseline, resolve any remaining technical behavior/safety details required by the chosen release, assign a release decision-maker, and document accepted limitations.
2. Demonstrate the essential journeys and acceptance cases, including malformed metadata, collisions, missing dependencies, empty content, draft exclusion, and theme fallback.
3. Pass the relevant repository checks and exercise sample build/preview; evaluate generated links/assets under both root and subpath deployment. The existing CI configuration targets Windows, macOS, and Linux; it is not evidence of a currently passing run.
4. Reconcile NFR-002/NFR-003 containment and rendering coverage, evaluate the agreed NFR-008 accessibility/browser expectations, and record NFR-009 benchmark results. Do not introduce an unagreed numerical performance gate or claim formal WCAG conformance.
5. Ship consistent package/sample guidance and migration notes. ID/immutable-collection, mandatory-tag-view, and directory-index route changes must not be presented as transparent upgrades.

**Rollout and recovery:** package publication, host selection, release timing, and deployment automation are not authorized here. The documented operational practice is to publish only a successful artifact and retain a previous deployed artifact through the chosen host; the engine does not implement deployment rollback. Recovery of local generated output is correction plus a fresh build or preview restart.

**Evaluation:** the owner has already confirmed time savings, easier customization, and expectations met. Any further measurement of G-001 through G-003 is optional; its timing, responsibilities, and numerical outcome targets remain undecided (Q-004) and are not prerequisites for accepting the reported benefits. Separately, V-005 and V-006 carry the agreed accessibility/browser evaluation and actual-blog benchmarking into the next phase. These quality decisions are settled; their checks and measurements have not been performed by this document update.

## 8. Risks and unresolved decisions

### Risks and dependencies

| ID | Risk / dependency | Impact and next action | Owner |
| --- | --- | --- | --- |
| RD-001 | Theme/plugin assemblies execute code; generated raw HTML and scripts cross trust boundaries | Building untrusted extensions or publishing unsafe markup can affect author/visitor environments. Retain the owner-controlled boundary; assess guardrail coverage in V-001/V-002 without a sandbox assurance | Unassigned |
| RD-002 | Non-atomic output and stale preview files | Failed builds can lose previous output; renamed/deleted content may remain in preview. Retain the limitation and evaluate failure/output behavior in V-004 | Unassigned |
| RD-003 | Static-host behavior varies | Directory indexes, subpaths, 404 handling, and external assets can differ from local preview. Record and evaluate serving assumptions in V-003; no particular host is required | Unassigned |
| RD-004 | Third-party extensions and existing content may lag vNext contracts | Name-based configuration, mutable-collection consumers, and themes relying on tag-view fallback can break; inferred nested-index URLs can move or collide. Verify the documented migration, explicit slug overrides, and compatibility in V-007 | Unassigned |
| RD-005 | #81-specific source-order migration can diverge from slug hierarchy or change inferred URLs after renaming | Mitigated in the implementation/sample/guides and scoped V-008 evidence: grouping retained and explicit-slug URLs preserved. Not eliminated: renaming still changes inferred URLs without an explicit slug | Residual behavior documented; further release ownership unassigned |
| RD-006 | Locale generation changes public collection routes, navigation membership and generated-route ownership | Conditional redirects preserve only bookmarks with default-locale targets; a source such as `pages/ko-kr/index.md` can collide with a generated locale home. Confirmed decisions retain explicit failures; implementation must document source/theme/plugin adoption and unsupported legacy URLs. In-place preview can retain stale redirects under FR-009 | Product decisions: @justinyoo; implementation/release owners unassigned |

### Decision and evidence gaps

| ID / state | Decision or evidence needed | Affected areas | Blocking effect / next action |
| --- | --- | --- | --- |
| Q-001 / Confirmed | Owner-controlled local authoring with deliberately trusted executable themes/plugins; no untrusted upload service or extension sandbox | Users, FR-005, FR-008, NFR-001/003/010 | Resolved by the user on 2026-09-11; retain validation and visitor-safety obligations |
| Q-002 / Confirmed (next-phase scope) | Defer containment, rendering/privacy, and subpath verification to V-001 through V-003; results remain unknown | FR-003/006/009, NFR-002/003/005/010 | Placement resolved by the user on 2026-09-11. Evidence and necessary corrections remain next-phase work before claiming compliance or release readiness, not PRD-definition blockers |
| Q-003 / Confirmed | Built-in-theme accessibility and the NFR-008 desktop/mobile browser coverage; custom-theme authors own accessibility. Benchmark the actual blog on a documented environment, with no formal SLA, numerical performance gate, maximum supported site size, or WCAG conformance claim | FR-010, NFR-008/009 | Quality choices resolved by the user on 2026-09-11; verification is assigned to next-phase V-005/V-006, with execution owners and dates still unassigned |
| Q-004 / Unknown (release arrangements) | Who owns operational release acceptance and any further outcome evaluation, and when will they occur? | Goals and release criteria | Historical v0.6 and scoped #81 approvals are recorded; remaining whole-document review is separate. Execution ownership, timing and actual release authorization remain to be confirmed; these are not new feature behavior decisions |
| Q-005 / Confirmed | Approved ordering/adjacency, two-tier fallback and component criteria; #89 and broad V-005 tracked separately | FR-010/011, NFR-007, RD-005 | Implemented with passing V-008 evidence; #81 is ready to close on merge, without completing unrelated work |
| Q-006 / Confirmed | Root entry point redirects to the generated homepage for `Site.Locale` when locale routing is enabled | FR-012, TR-023, DEC-004 | Selected by @justinyoo on 2026-09-16; preserve `BaseUrl` and guarantee a valid generated default-locale target |
| Q-007 / Confirmed | Generate `Site.Locale` plus effective locales of published non-404 content; always generate the default homepage, even empty | FR-012 locale inventory | Selected by @justinyoo on 2026-09-16; no configured list, draft-only locale or folder-based discovery |
| Q-008 / Confirmed | Locale folders are organizational only; retain frontmatter/site fallback and recommend explicit locale-free slugs | FR-002/003/012, source migration | Selected by @justinyoo on 2026-09-16; do not infer locale or strip folders from inferred slugs |
| Q-009 / Confirmed | Strict locale-scoped generated browsing, with no automatic language fallback | FR-010/011/012 | Selected by @justinyoo on 2026-09-16; apply existing ordering/eligibility separately within each locale |
| Q-010 / Confirmed | Redirect old unprefixed tag routes only when their corresponding default-locale page exists; otherwise normal not-found behavior | FR-004/012, NFR-007, RD-006 | Selected by @justinyoo on 2026-09-16; no mixed-language legacy listings or nonexistent/cross-language redirect targets |
| Q-011 / Confirmed | Routing/filtering and locale context only; unchanged text defaults, shared assets and one default-locale root 404 | FR-012, Theme scope | Selected by @justinyoo on 2026-09-16; translated text/catalogs, language-switching UI and localized 404 variants are outside scope |
| Q-012 / Confirmed | Reject a custom shared-404 locale that normalizes differently from `Site.Locale` when locale routing is enabled | FR-003/004/012, metadata compatibility | Selected by @justinyoo on 2026-09-16; omission uses site default, equivalent spellings pass, mismatch fails with actionable source/field context |

Historical approvals and evidence from both branches remain intact. The user authorized locale implementation after approving PRD v0.11/TRD v0.7/TDD v0.8, and separately requested merging the completed #89 fix. Q-006 through Q-012 are unchanged. V-003 retains the mount work's history; V-009 owns combined locale acceptance. Neither reopens #81 or closes broader verification or release arrangements.

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
| 2026-09-13 | Align v0.7 with merged #86/#87 at `1f963ad` | User requested updates to all four documents; source and website handoff inspected | Retain IDs; add navigation, directory-index, shared-URL and migration coverage; preserve seven-role/explicit-mode corrections, historical approval, and open verification |
| 2026-09-13 | Add confirmed #81 replacement scope in v0.8 at `843272a` | @justinyoo's settled decision and request to update PRD/TRD/TDD; decision comment linked in FR-011 | Add FR-011 and Q-005, supersede title-first navigation ordering and the original metadata/JSON proposal, retain grouping/publication rules and existing gaps; no implementation or issue closure |
| 2026-09-13 | Require TG-006 / DQ-008 resolution within #81 | Explicit user scope decision; clarification linked in FR-011 | Make source-less caller compatibility part of feature completion, not a deferred follow-up; exact policy remains to be selected and implemented |
| 2026-09-13 | Confirm the two-tier source-less compatibility policy | @justinyoo accepted the recommendation; policy comment linked in FR-011 | Resolve TG-006 / DQ-008 as decisions: file-backed source order first, then source-less title/route order, with combined adjacency and preserved API/explicit-tree behavior; implementation still pending |
| 2026-09-13 | Separate #81 acceptance from broader requirements and verification | @justinyoo accepted the recommendation; separation comment linked in FR-011 | Keep FR-011/TR-021/TR-022 authoritative, add V-008, and retain V-001 through V-007 with scoped evidence reuse; preserve RD-005 residual risk and genuine feature dependencies |
| 2026-09-13 | Approve PRD v0.8 only for #81 | Explicit scoped approval by @justinyoo; approval record linked in Document control | Approve FR-011/V-008/RD-005 feature requirements without whole-document sign-off, resolution of shared gaps, implementation, evidence claims, or issue closure |
| 2026-09-13 | Record #81 implementation and scoped V-008 execution in v0.9 | User requested implementation after scoped approval | Preserve requirements/sign-off history; record code, sample migration, 493 passing Windows tests and limited HTTP/Chromium evidence; keep preview-prefix/browser/contrast blockers and #81 open |
| 2026-09-13 | Move preview base-path mounting out of #81's acceptance gate | Explicit user confirmation; #89 and scope update linked in FR-011 | Supersede the earlier mount dependency gate, retain the 404 evidence under V-003/DEC-002, keep locale-aware new-link checks and other V-008 criteria unchanged; neither issue closed |
| 2026-09-14 | Set component acceptance standards for #81 in v0.10 | Explicit user selections; decision linked in FR-011 | Require 4.5:1 pager text / 3:1 focus contrast and accept three-engine desktop/mobile automation; retain broad V-005 obligations separately. Method settled, execution pending |
| 2026-09-14 | Complete the approved V-008 component checks | User requested further implementation; reproducible browser suite and recorded measurements | Fix pager-only contrast and WebKit tabbing; 42 browser, 4 math and 493 .NET cases pass. Mark V-008 complete for #81 while retaining broader gaps |
| 2026-09-16 | Draft locale-specific generated collections and confirm the root entry point in v0.11 | @justinyoo requested PRD/TRD/TDD updates and selected redirect to `Site.Locale` | Add FR-012/V-009/RD-006/Q-006 through Q-011; preserve historical IDs/approvals, mark unresolved choices explicitly, and retain #89 as independent mount work. No application implementation |
| 2026-09-16 | Resolve locale feature choices and issue a Review-ready revision | Explicit selections by @justinyoo for Q-007 through Q-012 | Require discovered locales, organizational folders, isolated browsing, conditional legacy redirects, routing-only presentation and shared-404 mismatch errors; reconcile TRD/TDD without implementation or whole-document sign-off |
| 2026-09-16 | Approve PRD v0.11 only for the locale scope | Explicit @justinyoo approval of PRD/TRD/TDD only for this scope, reviewing `94fc60b` | Approve FR-012/Q-006 through Q-012/V-009/RD-006 and applicable shared obligations; preserve existing gaps, historical approvals, #89 separation and unimplemented status. No implementation or release authorization |
| 2026-09-16 | Implement FR-012 and record V-009 in v0.12 | Explicit request to start implementation within the agreed scope | Deliver locale generation/context/redirects, regressions and migration guides; record scoped passing evidence and retain #89's reproduced preview-prefix blocker. No broader approval or release |
| 2026-09-16 | Record #89 preview-mount implementation in v0.11 | User requested the fix; actual application HTTP regressions, sample build/preview matrix and full .NET suite | Record scoped V-003 passes and unchanged artifact layout; preserve historical failures, all IDs, approvals and broader gaps |
| 2026-09-16 | Require prefix-only preview serving | User clarified the expected meaning of `BaseUrl` and requested the fix | Supersede the unrequested root-alias assumption; FR-009/TR-017 require outside-prefix 404, with retained mount redirects and root configuration; record follow-up V-003 evidence without broader sign-off |
| 2026-09-16 | Redirect the preview root to a non-root base URL | User requested a root redirect instead of a duplicate homepage | Add a root GET/HEAD redirect exception to FR-009/TR-017; preserve other outside-prefix 404s, root-mode serving, historical evidence and broader gaps |
| 2026-09-16 | Accept both trailing-slash forms of site base paths | User requested code-side normalization of `Site:BaseUrl` | Canonicalize supported path prefixes for build/preview without rewriting settings; retain root/redirect behavior and broader URL-policy gaps; record 538 tests and 274 sample HTTP checks |
| 2026-09-16 | Integrate the completed #89 fix with FR-012 in v0.13 | User requested merging `vnext`; incoming `da895fd` from #101 | Preserve both implementation and decision histories, adopt shared BaseUrl normalization and prefix-only serving, and re-evaluate V-009 using the combined code rather than waiving its former blocker |

### Readiness assessment

- **Supported status:** Review-ready overall with prior approvals retained and completed scoped V-009 acceptance based on combined execution, not imported evidence alone.
- **Remaining decisions versus execution:** #81/V-008 is unchanged. #89 is merged and the former V-009 blocker passes actual integrated preview/browser checks; wider programs and release work remain separate.
- **Approval:** The user approved and authorized the locale implementation, then requested this merge. No unrelated scope, broader gap closure, publication or deployment is authorized.
- **Reviewer pass:** Reconciled PRD v0.13 with TRD v0.9/TDD v0.10, shared normalization and integrated evidence, preserving both branches' requirements, failures and approval records.
- **Review limitations:** No comprehensive accessibility/security audit, physical-device matrix or blog benchmark is claimed.

## 9. Reference map

Use linked source files and the [website documentation handoff](docs/website-documentation.md) for detailed behavior and migration; package guides provide entry points. This document does not replace [technical requirements](TRD.md) or [design](TDD.md), and the handoff is not evidence of published website updates.

| Topic | References |
| --- | --- |
| Original discussion and product setup | [Original discussion (archived)](https://github.com/getscissorhands/Scissorhands.NET/blob/464ce0f3454d473d4a39bc6f5c9005e86cd5396a/DISCUSSIONS.md), [README.md](README.md), [sample](sample/README.md) |
| Shared contracts and extension guidance | [Core](src/ScissorHands.Core/README.md), [Plugin](src/ScissorHands.Plugin/README.md), [Theme](src/ScissorHands.Theme/README.md), [Web](src/ScissorHands.Web/README.md) |
| Content/route regression evidence | [Loader tests](test/ScissorHands.Web.Tests/Loaders/ContentLoaderTests.cs), [generator tests](test/ScissorHands.Web.Tests/Generators/StaticSiteGeneratorTests.cs), [route tests](test/ScissorHands.Web.Tests/Generators/StaticSiteGeneratorRouteTests.cs) |
| Navigation and current route behavior | [Navigation reference](docs/website-documentation.md#page-routes-and-navigation), [navigation generation cases](test/ScissorHands.Web.Tests/Generators/StaticSiteGeneratorNavigationTests.cs), [directory-index cases](test/ScissorHands.Web.Tests/Loaders/ContentLoaderDirectoryIndexTests.cs), [shared URL cases](test/ScissorHands.Core.Tests/Urls/ContentUrlHelperTests.cs) |
| Implemented navigation change and remaining acceptance | [#81 replacement decision](https://github.com/getscissorhands/Scissorhands.NET/issues/81#issuecomment-5653313907), FR-011/V-008, [technical requirements](TRD.md), [design](TDD.md) |
| Extension regression evidence | [Plugin runner tests](test/ScissorHands.Web.Tests/Runners/PluginRunnerTests.cs), [dependency tests](test/ScissorHands.Web.Tests/Runners/PluginDependencyTests.cs), [theme resolver tests](test/ScissorHands.Web.Tests/Services/ThemeComponentResolverTests.cs), [theme service tests](test/ScissorHands.Web.Tests/Services/ThemeServiceTests.cs) |
| Constraints and release practices | [AGENTS.md](AGENTS.md), [SDK selection](global.json), [build configuration](Directory.Build.props), [CI](.github/workflows/main.yaml) |
