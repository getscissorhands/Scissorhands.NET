# This PowerShel script appends AppVeyor build number to the package version.

$projects = Get-ChildItem .\src | ?{$_.PsIsContainer}

foreach($project in $projects) {
    # Display project name
    $project

    # Move to the project
    cd src/$project

    # NPM install
    npm install

    # Run node.js script
    node project-version.js

    # Move back to the solution root
    cd ../../
}