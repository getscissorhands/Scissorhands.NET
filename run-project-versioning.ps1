# This PowerShel script appends AppVeyor build number to the package version.

$projects = Get-ChildItem .\src | ?{$_.PsIsContainer}

foreach($project in $projects) {
    cd src/$project
    npm install
    node project-version.js
    cd ../../
}