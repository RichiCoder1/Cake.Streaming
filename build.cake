#l "build\Utilities.cake"
using System.Text.RegularExpressions;

const string version = "0.1.0";

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", EnvironmentVariable("Configuration") ?? "Debug");
var tag = Argument("tag", "local");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

string packageVersion = version + (string.IsNullOrWhiteSpace(tag) ? "" : "-" + tag);

if (EnvironmentVariable("BuildRunner") == "MyGet")
{
    var packageVersionFormat = EnvironmentVariable("VersionFormat");
    packageVersion = string.Format(packageVersionFormat, EnvironmentVariable("BuildCounter"), version);
    packageVersion = new Regex("[0-9].[0-9].[0-9]").Replace(packageVersion, version);
}

// Define directories.
var projectName = "Cake.Streaming";
var repoDir = "./";
var srcDir = JoinPath(repoDir, "src");
var buildDir = JoinPath(repoDir, "build");
var toolsDir = JoinPath(repoDir, "tools");
var nugetSpecDir = JoinPath(buildDir, "nuget");
var buildResultsDir = JoinPath(buildDir, "artifacts");
var testResultsDir = JoinPath(buildDir, "testResults");
var nugetResultDir = JoinPath(buildResultsDir, "packages");

// MSBuild Results Dirs
var binDir = JoinPath(srcDir, "FPS.Streaming/bin", configuration);

// Project Specific Result Dirs
var pkgeDir = JoinPath(nugetSpecDir, "FPS.Streaming");
var pkgeLibDir = JoinPath(pkgeDir, "bin/lib/net45");

var projectSln = JoinPath(srcDir, projectName + ".sln");

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(() =>
{
    Information("Building version {0} of Cake.Streaming.", packageVersion);
});

Teardown(() =>
{
    Information("Finished building version {0} of Cake.Streaming.", packageVersion);
});

///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////

Task("Clean")
    .Description("Cleans the build and output directories.")
    .Does(() =>
{
    ResilientCleanDirs(new DirectoryPath[] { buildResultsDir, testResultsDir, nugetResultDir });
});

Task("Restore-NuGet-Packages")
    .Description("Restores all NuGet packages in solution.")
    .IsDependentOn("Clean")
    .Does(() =>
{
    NuGetRestore(projectSln);
});

Task("Patch-Assembly-Info")
    .Description("Patches the AssemblyInfo files.")
    .IsDependentOn("Restore-NuGet-Packages")
    .Does(() =>
{
    var file = JoinPath(srcDir, "SolutionInfo.cs");
    CreateAssemblyInfo(file, new AssemblyInfoSettings {
        Product = projectName,
        Version = version,
        FileVersion = version,
        InformationalVersion = packageVersion,
        Copyright = "Copyright (c) Richard Simpson " + DateTime.Now.Year
    });
});

Task("Build")
    .Description("Builds the Cake.Streaming source code.")
    .IsDependentOn("Patch-Assembly-Info")
    .Does(() =>
{
    MSBuild(projectSln, settings =>
        settings.SetConfiguration(configuration)
            .WithProperty("TreatWarningsAsErrors", "true")
            .UseToolVersion(MSBuildToolVersion.NET45));
});

Task("Run-Unit-Tests")
    .Description("Runs unit tests.")
    .IsDependentOn("Build")
    .Does(() =>
{
    XUnit("/src/**/bin/" + configuration + "/*.*Tests.dll", new XUnitSettings {
        OutputDirectory = testResultsDir,
        XmlReport = true,
        HtmlReport = true
    });
});


Task("Copy-Files")
    .Description("Copy files to the output directory.")
    .IsDependentOn("Run-Unit-Tests")
    .Does(() =>
{
    // Cake.Streaming	
    CreateDirectory(pkgeLibDir);    
    CopyFileToDirectory(JoinPath(binDir, "/Cake.Streaming.dll"), pkgeLibDir);
    CopyFileToDirectory(JoinPath(binDir, "/Cake.Streaming.pdb"), pkgeLibDir);
    CopyFileToDirectory(JoinPath(binDir, "/Cake.Core.dll"), pkgeLibDir);
    CopyFileToDirectory(JoinPath(binDir, "/Cake.Core.xml"), pkgeLibDir);
});

Task("Create-NuGet-Package")
    .Description("Creates the NuGet package.")
    .IsDependentOn("Copy-Files")
    .Does(() =>
{
    // Cake.Streaming
    NuGetPack(JoinPath(pkgeDir, "Cake.Streaming.nuspec"), new NuGetPackSettings {
        Version = packageVersion,
        BasePath = JoinPath(pkgeDir, "bin"),
        OutputDirectory = nugetResultDir,        
        Symbols = true
    });
});

Task("Package")
    .Description("Creates a package.")
    .IsDependentOn("Create-NuGet-Package");

Task("Default")
    .Description("Default target")
    .IsDependentOn("Run-Unit-Tests");
    
//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);