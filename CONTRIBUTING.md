# Contributing to ScissorHands.NET

ScissorHands.NET is a .NET 10 static site generator distributed as Core, Plugin,
Theme, and Web packages. Contributions to the engine, themes, plugins, tests,
and documentation are welcome.

## Code of Conduct

Please follow the [Code of Conduct](CODE_OF_CONDUCT.md) in project spaces.
For help using the project, see [SUPPORT.md](SUPPORT.md). Report suspected
vulnerabilities privately using [SECURITY.md](SECURITY.md), not a public issue.

## Getting Started

Install Git and the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0).
[global.json](global.json) selects SDK 10.0.100 with `latestFeature` roll-forward
and excludes prerelease SDKs.

Fork the repository, clone your fork, and create a branch from the current
`vnext` branch. Use a descriptive name such as `feat/my-feature`,
`fix/route-handling`, or `docs/theme-guide`. Open pull requests against `vnext`.

Run these commands from the repository root:

```powershell
dotnet restore .\ScissorHands.slnx
dotnet build .\ScissorHands.slnx -c Release --no-restore
```

The examples use PowerShell path syntax; use forward-slash paths in other shells.
Node.js is not required for the .NET build. Node.js 24 and npm are needed only
for the browser acceptance suite.

## Testing Changes

Start with the affected test project, replacing `Web` as appropriate:

```powershell
dotnet test --project .\test\ScissorHands.Web.Tests\ScissorHands.Web.Tests.csproj -c Release
```

After a successful Release build, run the full suite for shared-contract or
cross-project changes:

```powershell
dotnet test --solution .\ScissorHands.slnx -c Release --no-build --verbosity normal
```

The repository uses Microsoft.Testing.Platform, not VSTest. Use the explicit
`--project` and `--solution` selectors and keep build and test configurations
aligned when using `--no-build`.

Add regression tests in the matching test project. Follow nearby
`Given_..._When_..._Then_...` tests using xUnit v3, Shouldly, NSubstitute, and
bUnit where appropriate. Reuse existing fixtures and isolate filesystem or
process-wide state.

For generation, theme, or preview changes, also exercise the
[sample](samples/ScissorHands.Sample/README.md). Run it from its own directory
with an explicit `--preview` or `--build` argument; stop preview with Ctrl+C.
For built-in page-navigation markup or style changes, follow the
[browser acceptance guide](test/browser/README.md) for Chromium, Firefox, and
WebKit checks. Documentation-only changes do not require a .NET build.

## Coding and Documentation

Follow [.editorconfig](.editorconfig) and nearby code. Keep nullable reference
types and warnings-as-errors enabled. Manage dependency versions centrally in
[Directory.Packages.props](Directory.Packages.props).

Read [AGENTS.md](AGENTS.md) for the repository map, architecture boundaries,
validation expectations, and change guardrails. Read the affected package's
README before changing its contracts or behavior. Update that README, related
sample guidance, and the [website handoff](docs/website-documentation.md) when
needed. Package READMEs are shipped in NuGet packages.

Do not commit generated `bin`, `obj`, `preview`, `dist`, test results, packages,
credentials, or local secrets. Keep unrelated refactors and dependency upgrades
out of a feature or bug-fix change.

## Issues and Pull Requests

Use the [bug report form](https://github.com/getscissorhands/Scissorhands.NET/issues/new?template=01-BUG-REPORT.yml)
with a minimal reproduction, package and SDK versions, expected and actual
behavior, and relevant environment details. Remove secrets and private content
from logs and examples.

Use the [feature request form](https://github.com/getscissorhands/Scissorhands.NET/issues/new?template=02-FEATURE-REQUEST.yml)
to discuss the problem, proposed behavior, and alternatives before undertaking
large changes.

Keep each pull request focused, complete the
[PR template](.github/PULL_REQUEST_TEMPLATE.md), and reference the related
issue when applicable. Explain compatibility changes and provide migration
guidance for breaking changes. Include the commands used to validate the change
and disclose checks that were blocked or skipped. CI must pass before merge.

Use [Conventional Commits](https://www.conventionalcommits.org/), for example
`fix(web): preserve relative URLs` or `docs: clarify theme setup`. Mark breaking
changes with `!` or a `BREAKING CHANGE:` footer. Each commit should be one
complete logical change, with tightly coupled implementation, tests, and
documentation kept together.
