# This PowerShel script runs all test projects.

$projects = Get-ChildItem .\test | ?{$_.PsIsContainer}

$exitCode = 0

foreach($project in $projects) {
    # Display project name
    $project

    # Move to the project
    cd test/$project

    # Run test
    dnx test

    $LASTEXITCODE

    $exitCode += $LASTEXITCODE

    # Move back to the solution root
    cd ../../
}

if($exitCode -ne 0) {
    $host.SetShouldExit($exitCode)
}