# This PowerShell script runs all javascript tests.

# Gets all test harness HTML files.
$htmls = Get-ChildItem -Path ./src/Scissorhands.NET/wwwroot/js/* -Include *.html -Recurse

# Moves to the test runner directory.
cd ./src/Scissorhands.NET/node_modules/mocha-phantomjs/bin

# Runs the test harness files.
$exitCode = 0

foreach($html in $htmls) {
    $filename = "../../../wwwroot/js/tests/" + $html.Name

    node mocha-phantomjs $filename

    $exitCode += $LASTEXITCODE
}

cd ../../../../../

if($exitCode -ne 0) {
    $host.SetShouldExit($exitCode)
}
