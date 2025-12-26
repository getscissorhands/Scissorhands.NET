<#
.SYNOPSIS
Sets GitHub Packages environment variables for the current shell session.

.DESCRIPTION
This script sets:
- GH_PACKAGE_USERNAME
- GH_PACKAGE_TOKEN

To persist the variables in your current PowerShell session, you must dot-source the script.

.EXAMPLE
. ./setup-gh-auth.ps1 -Username <value> -Token <value>

.NOTES
- If you run this script normally (e.g. `./scripts/setup-gh-auth.ps1`), variables are only set
  for that PowerShell process.
- Dot-sourcing is required to keep them in your current session.
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [string] $Username,

    [Parameter(Mandatory = $false)]
    [string] $Token,

    [switch] $Help
)

function Show-Usage {
@'
Usage:
  . ./setup-gh-auth.ps1 --Username <value> --Token <value>

Options:
  -Username   GitHub Packages username (e.g., your GitHub handle)
  -Token      GitHub Packages token (PAT) with appropriate scopes
  -Help       Show this help

Notes:
  - To persist environment variables in your current shell session, you must dot-source this script.
    If you run it ("./scripts/setup-gh-auth.ps1"), exports only apply to the subprocess.
'@ | Write-Host
}

if ($Help) {
    Show-Usage
    return
}

if ([string]::IsNullOrWhiteSpace($Username)) {
    Write-Error "-Username is required"
    Show-Usage
    exit 1
}

if ([string]::IsNullOrWhiteSpace($Token)) {
    Write-Error "-Token is required"
    Show-Usage
    exit 1
}

$env:GH_PACKAGE_USERNAME = $Username
$env:GH_PACKAGE_TOKEN = $Token

Write-Host "Exported GH_PACKAGE_USERNAME and GH_PACKAGE_TOKEN for this shell session."