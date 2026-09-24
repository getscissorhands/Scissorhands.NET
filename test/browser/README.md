# Page navigation browser acceptance

Test-only Playwright coverage for the default-theme pager and locale navigation
in Chromium, Firefox and WebKit.

## Run

Prerequisites: the [repository's .NET SDK](../../global.json) and Node.js 24.
From the repository root:

```bash
dotnet restore ./sample/sample.csproj
cd ./test/browser
npm ci
npx playwright install chromium firefox webkit
npm test
```

On Linux, use `npx playwright install --with-deps chromium firefox webkit` to
install the browser system dependencies as well.

`npm test` regenerates sample `dist` and test fixtures without changing the
sample's source content. Reports are written to ignored `test-results`.

Browsers run headlessly using Playwright-managed builds; no separate Firefox
application is needed.

See [browser acceptance details](../../docs/website-documentation.md#browser-acceptance)
for fixture setup, Firefox configuration, coverage, and evidence limits.
