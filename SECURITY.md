# Security Policy

## Reporting a Vulnerability

Report suspected vulnerabilities privately to **ask (at) getscissorhands (dot) app**. Replace `(at)` with `@` and `(dot)` with `.` when sending email.

Do not disclose vulnerabilities in public GitHub issues or pull requests. Use the email contact above rather than relying on GitHub's private reporting feature being enabled.

Include the following in your report:

- The affected ScissorHands.NET package versions or `vnext` commit.
- A description of the issue, potential impact, and required conditions.
- Minimal reproduction steps or a proof of concept using synthetic data.
- Relevant SDK, operating system, configuration, and preview/build mode.
- Any suggested fix and your preferred contact and attribution details.

Do not send credentials, production secrets, or other people's private data. Test only systems you own or have permission to assess.

Maintainers will assess the report and coordinate investigation, any fix, and disclosure with the reporter. This policy does not promise a fixed response or remediation deadline. Reporter credit will be given only with permission.

## Supported Versions

| Version or branch | Security report handling |
| --- | --- |
| Current `vnext` branch | Reports accepted and assessed |
| Latest published preview packages | Reports accepted and assessed |
| Older published versions | Assessed case by case; fixes or backports are not guaranteed |

See [GitHub Releases](https://github.com/getscissorhands/Scissorhands.NET/releases) for published versions. Preview versions can include breaking changes; see the [vNext migration guide](docs/website-documentation.md#upgrading-to-vnext) before upgrading.

## Scope

Reports may concern the Core, Plugin, Theme, and Web packages, including content path handling, metadata rendering, preview behavior, or unintended exposure of local data.

Theme and plugin assemblies execute application code and must be trusted by the site operator. Raw HTML content is also an explicit trust boundary. Navigation visibility is not publication control or authorization. Include how an issue crosses the relevant boundary when reporting it.

For third-party dependencies, identify the affected dependency and how the issue affects ScissorHands.NET so maintainers can coordinate with its upstream project.
