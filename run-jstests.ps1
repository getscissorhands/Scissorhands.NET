# This PowerShell script runs all javascript tests.

# Installs npm packages.
cd  ./src/Scissorhands.NET
npm install

# Gets all test harness HTML files.
$htmls = Get-ChildItem -Path ./wwwroot/js/tests/* -Include *.html -Recurse

# Moves to the test runner directory.
cd ./node_modules/mocha-phantomjs/bin

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
