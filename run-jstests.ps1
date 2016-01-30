# This PowerShell script runs all javascript tests.

# Installs npm packages.
cd  ./src/Scissorhands.NET
npm install mocha-phantomjs

# Gets all test harness HTML files.
$htmls = Get-ChildItem -Path ./wwwroot/js/tests/* -Include *.html -Recurse

# Moves to the test runner directory.
cd ./node_modules/mocha-phantomjs/bin

# Runs the test harness files.
$exitCode = 0

foreach($html in $htmls) {
    # Display test harness name.
    Write-Host "`n$($html.Name)`n" -ForegroundColor Green

    $filename = "../../../wwwroot/js/tests/$($html.Name)"

    # Run test and export report as an xUnit format.
    node mocha-phantomjs -R xunit $filename > ../../../report.xml

    # Read report.xml
    $xml = [xml](Get-Content ../../../report.xml)
    $testsuite = $xml.testsuite

    del ../../../report.xml

    # Display test summary.
    Write-Host "=== TEST EXECUTION SUMMARY ==="
    Write-Host "   $($html.Name)  Total: $($testsuite.tests), Errors: $($testsuite.errors), Failed: $($testsuite.failures), Skipped: $($testsuite.skipped), Time: $($testsuite.time)s`n"

    # Uploads test results to AppVeyor.
    foreach ($testcase in $testsuite.testcase) {
        [decimal]$time = [Convert]::ToDecimal($testcase.time) * 1000
        $testname = $testcase.classname + " " + $testcase.name

        $failed = $testcase.failure
        if ($failed) {
            $message = $($failed.InnerText -split "`n")[0]
            $trace = $failed.InnerText

            Add-AppveyorTest $testname -Outcome Failed -FileName $html.Name -Duration $time -ErrorMessage $message -ErrorStackTrace $trace
        }
        else {
            Add-AppveyorTest $testname -Outcome Passed -FileName $html.Name -Duration $time
        }
    }

    $exitCode += $LASTEXITCODE
}

cd ../../../../../

if($exitCode -ne 0) {
    $host.SetShouldExit($exitCode)
}
