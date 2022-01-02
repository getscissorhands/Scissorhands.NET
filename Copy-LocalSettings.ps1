# Copy appsettings.json
$ProjectPath = "./src/Scissorhands.RouteGenerator"

Write-Output "[$(Get-Date -Format "yyyy-MM-dd HH:mm:ss")] Copying $($ProjectPath)/appsettings.json ..."

$json = Get-Content -Path "$($ProjectPath)/appsettings.json" | ConvertFrom-Json
$json.projectDirectory = $json.projectDirectory -replace "~", "$($pwd.Path)"
$json.projectDirectory = $json.projectDirectory -replace "/", "$([System.IO.Path]::DirectorySeparatorChar)"
$json | ConvertTo-Json | Set-Content -Path "$($ProjectPath)/appsettings.local.json" -Force

Get-Content -Path "$($ProjectPath)/appsettings.local.json"

Write-Output "[$(Get-Date -Format "yyyy-MM-dd HH:mm:ss")] Copied $($ProjectPath)/appsettings.json"

# Copy testsettings.json
$ProjectPath = "./test/Scissorhands.Core.Tests"

Write-Output "[$(Get-Date -Format "yyyy-MM-dd HH:mm:ss")] Copying $($ProjectPath)/testsettings.json ..."

$json = Get-Content -Path "$($ProjectPath)/testsettings.json" | ConvertFrom-Json
$json.projectDirectory = $json.projectDirectory -replace "~", "$($pwd.Path)"
$json.projectDirectory = $json.projectDirectory -replace "/", "$([System.IO.Path]::DirectorySeparatorChar)"
$json | ConvertTo-Json | Set-Content -Path "$($ProjectPath)/testsettings.local.json" -Force

Get-Content -Path "$($ProjectPath)/testsettings.local.json"

Write-Output "[$(Get-Date -Format "yyyy-MM-dd HH:mm:ss")] Copied $($ProjectPath)/testsettings.json"
