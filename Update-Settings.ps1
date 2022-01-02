Param(
    [string]
    [Parameter(Mandatory=$false, Position=0)]
    $ProjectPath = "./test/Scissorhands.Core.Tests",

    [string]
    [Parameter(Mandatory=$false, Position=0)]
    $Filename = "testsettings.json",

    [switch]
    [Parameter(Mandatory=$false)]
    $Help
)

function Show-Usage {
    Write-Output "
    Usage: ./$(Split-Path $MyInvocation.ScriptName -Leaf) [-ProjectPath ProjectPath] [-Filename Filename] [-Help]

    Options:
        -ProjectPath:  Project path.
        -Filename:     Project path.
        -Help:         Show this message.

"

    Exit 0
}

# Show usage
if ($Help -eq $true) {
    Show-Usage
    Exit 0
}

# Update (app|test)settings.json
Write-Output "[$(Get-Date -Format "yyyy-MM-dd HH:mm:ss")] Updating $($ProjectPath)/$(Filename) ..."

$json = Get-Content -Path "$($ProjectPath)/$(Filename)" | ConvertFrom-Json
$json.projectDirectory = $json.projectDirectory -replace "~", "$($pwd.Path)"
$json.projectDirectory = $json.projectDirectory -replace "/", "$([System.IO.Path]::DirectorySeparatorChar)"
$json | ConvertTo-Json | Set-Content -Path "$($ProjectPath)/$(Filename)" -Force

Get-Content -Path "$($ProjectPath)/$(Filename)"

Write-Output "[$(Get-Date -Format "yyyy-MM-dd HH:mm:ss")] Updated $($ProjectPath)/$(Filename)"
