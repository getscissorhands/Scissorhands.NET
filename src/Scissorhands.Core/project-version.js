"use strict";

var jsonfile = require("jsonfile");
var semver = require("semver");

var file = "/project.json";

var updateVersion = function (err, project) {
    if (err) {
        console.error(err);
        return;
    }

    //var buildNumber = process.env.APPVEYOR_BUILD_NUMBER;
    var buildNumber = 123;
    var version = semver.valid(project.version + "-" + buildNumber);
    project.version = version;

    jsonfile.writeFile(file, project, { spaces: 2 }, onVersionUpdated);
};

var onVersionUpdated = function (err) {
    if (err) {
        console.error(err);
        return;
    }

    console.log("version in project.json has been updated");
};

jsonfile.readFile(file, updateVersion);
