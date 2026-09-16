# Page navigation browser acceptance

This opt-in suite records V-008 evidence for the generated default-theme pager
and V-009 checks for locale generation and navigation.
It does not replace the .NET regression suite, the broader V-005 real-device
assessment, or the preview-prefix work in issue #89.

## Run

Prerequisites: the repository's .NET SDK, restored sample project dependencies,
and Node.js 24. From the repository root:

```powershell
dotnet restore .\samples\ScissorHands.Sample\ScissorHands.Sample.csproj
Set-Location .\test\browser
npm ci
npx playwright install chromium firefox webkit
npm test
```

On Linux, use `npx playwright install --with-deps chromium firefox webkit` to
install the browser system dependencies as well.

`npm test` builds the sample, copies its content/configuration into the ignored
`artifacts/locale-source` directory, and adds English, Japanese and draft-only
locale fixtures. It generates `/docs/` output with default `ko-kr` under
`artifacts/prefix`, then regenerates the default root-site `dist` output.
The normal sample content is unchanged; settings are process-local. Tests serve only these
artifacts through loopback servers on dynamically assigned ports; fixtures
close their servers and isolated browser contexts. The controlled prefix host
tests generated URLs, not the separate application preview-mount defect.

## Coverage and evidence

The six projects combine Chromium, Firefox and WebKit with desktop (1280x800)
and mobile-width (375x812) viewports. They cover the sample reading sequence,
real root and localized subpath requests, endpoint/exclusion behavior, accessible link labels,
keyboard navigation, JavaScript-disabled navigation and horizontal layout.

Locale cases follow generated home/tag/navigation links, check language-specific
collections and adjacency endpoints, exercise root/legacy redirects without
JavaScript, and verify absent tag/draft-only routes and shared asset requests.
These are controlled static-host requests, not evidence that #89 is fixed.

Both themes are measured in normal, hover and keyboard-focus states:

- All pager text must reach **4.5:1** contrast.
- The focus indicator must reach **3:1** against its adjacent background.

Measurements use rendered RGB/alpha values and relative luminance. Transparent
layers are composited; unsupported backgrounds or group opacity fail explicitly.
Transitions are disabled only in measurement tests to inspect settled states.
The math has independent Node tests. No whole-site WCAG or real Safari/iOS/device
conformance is implied by this component suite.

The ignored `test-results` directory contains the JSON report and attached
per-engine/theme/state color measurements. Failures also retain traces and
screenshots. A failed or incomplete project is not passing V-008 evidence.

The pager uses explicit `tabindex="0"` on its native links so WebKit's default
keyboard mode includes them without introducing positive tab ordering.
