# This is the test runner for all test projects.
# It gets all test projects and run each project.
# This PowerShel script runs all test projects.

$projects = Get-ChildItem .\test | ?{$_.PsIsContainer}

foreach($project in $projects) {
    cd test/$project
    dnx test
    cd ../../
}