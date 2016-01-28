# Set the list of DNVM objects
$dnvm1 = New-Object -TypeName PSObject -Property (@{ "Version" = "1.0.0-rc1-update1"; "Runtime" = "clr"; "Architecture" = "x86" })
$dnvm2 = New-Object -TypeName PSObject -Property (@{ "Version" = "1.0.0-rc1-update1"; "Runtime" = "clr"; "Architecture" = "x64" })
$dnvm3 = New-Object -TypeName PSObject -Property (@{ "Version" = "1.0.0-rc1-update1"; "Runtime" = "coreclr"; "Architecture" = "x86" })
$dnvm4 = New-Object -TypeName PSObject -Property (@{ "Version" = "1.0.0-rc1-update1"; "Runtime" = "coreclr"; "Architecture" = "x64" })
$dnvms = ($dnvm1, $dnvm2, $dnvm3, $dnvm4)

# This PowerShel script runs all test projects.

$projects = Get-ChildItem .\test | ?{$_.PsIsContainer}

$exitCode = 0

foreach($project in $projects) {
    # Display project name
    Write-Host "`n$project`n" -ForegroundColor Green

    # Move to the project
    cd test/$project

    # Run test
    foreach($dnvm in $dnvms) {
        dnvm run $dnvm.Version -r $dnvm.Runtime -a $dnvm.Architecture test

        Write-Host "`n"

        $exitCode += $LASTEXITCODE
    }

    # Move back to the solution root
    cd ../../
}

if($exitCode -ne 0) {
    $host.SetShouldExit($exitCode)
}