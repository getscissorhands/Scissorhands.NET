# This PowerShel script runs all test projects.

$projects = Get-ChildItem .\test | ?{$_.PsIsContainer}

foreach($project in $projects) {
    # Display project name
    $project

    # Move to the project
    cd test/$project

    # Run test
    dnx test

    # Move back to the solution root
    cd ../../
}