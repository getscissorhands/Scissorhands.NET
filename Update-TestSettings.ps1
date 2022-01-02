Param(
    [string]
    [Parameter(Mandatory=$false, Position=0)]
    $ProjectPath = "./test/Scissorhands.Core.Tests",

    [switch]
    [Parameter(Mandatory=$false)]
    $Help
)

function Show-Usage {
    Write-Output "
    Usage: ./$(Split-Path $MyInvocation.ScriptName -Leaf) [-ProjectPath ProjectPath] [-Help]

    Options:
        -ProjectPath:  Project path.
        -Help:         Show this message.

"

    Exit 0
}

# Show usage
if ($Help -eq $true) {
    Show-Usage
    Exit 0
}

# Update testsettings.json
Write-Output "[$(Get-Date -Format "yyyy-MM-dd HH:mm:ss")] Updating $($ProjectPath)/testsettings.json ..."

$json = Get-Content -Path "$($ProjectPath)/testsettings.json" | ConvertFrom-Json
$json.projectDirectory = $json.projectDirectory -replace "~", "$($pwd.Path)"
$json.projectDirectory = $json.projectDirectory -replace "/", "$([System.IO.Path]::DirectorySeparatorChar)"
$json | ConvertTo-Json | Set-Content -Path "$($ProjectPath)/testsettings.json" -Force

Get-Content -Path "$($ProjectPath)/testsettings.json"

Write-Output "[$(Get-Date -Format "yyyy-MM-dd HH:mm:ss")] Updated $($ProjectPath)/testsettings.json"
