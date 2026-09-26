# Page navigation browser acceptance

Playwright acceptance tests for the default-theme navigation and preview badges in Chromium, Firefox, and WebKit.

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

`npm test` regenerates ignored sample/test output without changing sample source.

See [browser acceptance](../../docs/website-documentation.md#browser-acceptance) for Linux setup, fixtures, reports, coverage, and evidence limits.
