#tool "nuget:https://api.nuget.org/v3/index.json?package=GitVersion.CommandLine"

var target          = Argument("target", "Default");
var configuration   = Argument<string>("configuration", "Release");

// call e.g. .\build.ps1 --release=1.0.0 --pre=beta1
// call e.g. .\build.ps1 --release=1.0.0
 var versionOverride = Argument<string>("release", "");
 var suffixOverride = Argument<string>("pre", "");

///////////////////////////////////////////////////////////////////////////////
// GLOBAL VARIABLES
///////////////////////////////////////////////////////////////////////////////
var packPath            = Directory("./src");
var buildArtifacts      = Directory("./artifacts");

var isAppVeyor          = AppVeyor.IsRunningOnAppVeyor;
var isVsts              = TFBuild.IsRunningOnVSTS;
var isWindows           = IsRunningOnWindows();

DotNetCoreMSBuildSettings msBuildSettings;
VersionInfo versions;

///////////////////////////////////////////////////////////////////////////////
// Setup
///////////////////////////////////////////////////////////////////////////////
class VersionInfo
{
    public string AssemblyVersion { get; set; }
    public string VersionSuffix { get; set; }
    public string FileVersion { get; set; }
    public string InformationalVersion { get; set; }
    public string BranchName { get; set; }
    public string PreReleaseLabel { get; set; }
}

Setup(context =>
{
    var gitVersions = Context.GitVersion();
    
    versions = new VersionInfo
    {
        InformationalVersion = gitVersions.InformationalVersion,
        BranchName = gitVersions.BranchName,
        PreReleaseLabel = gitVersions.PreReleaseLabel
    };

    // explicit version has been passed in as argument
    if (!string.IsNullOrEmpty(versionOverride))
    {
        versions.AssemblyVersion = versionOverride;
        versions.FileVersion = versionOverride;

        if (!string.IsNullOrEmpty(suffixOverride))
        {
            versions.VersionSuffix = suffixOverride;
        }
    }
    else
    {
        versions.AssemblyVersion = gitVersions.AssemblySemVer;
        versions.FileVersion = gitVersions.AssemblySemVer;

        if (!string.IsNullOrEmpty(versions.PreReleaseLabel))
        {
            versions.VersionSuffix = gitVersions.PreReleaseLabel + gitVersions.CommitsSinceVersionSourcePadded;      
        }
        
    }

    Information("branch            : " + versions.BranchName);
    Information("pre-release label : " + versions.PreReleaseLabel);
    Information("version           : " + versions.AssemblyVersion);
    Information("version suffix    : " + versions.VersionSuffix);
    Information("informational     : " + versions.InformationalVersion);
    
    msBuildSettings = GetMSBuildSettings();
});

///////////////////////////////////////////////////////////////////////////////
// Clean
///////////////////////////////////////////////////////////////////////////////
Task("Clean")
    .Does(() =>
{
    CleanDirectories(new DirectoryPath[] { buildArtifacts });
});

///////////////////////////////////////////////////////////////////////////////
// Build
///////////////////////////////////////////////////////////////////////////////
Task("Build")
    .IsDependentOn("Clean")
    .Does(() =>
{
    var settings = new DotNetCoreBuildSettings 
    {
        Configuration = configuration,
        MSBuildSettings = msBuildSettings
    };

    
    var projects = GetFiles("./src/**/*.csproj");
    foreach(var project in projects)
	{
	    DotNetCoreBuild(project.GetDirectory().FullPath, settings);
    }

    var tests = GetFiles("./test/**/*.csproj");
    foreach(var test in tests)
	{
	    DotNetCoreBuild(test.GetDirectory().FullPath, settings);
    }
});

///////////////////////////////////////////////////////////////////////////////
// Test
///////////////////////////////////////////////////////////////////////////////
Task("Test")
    .IsDependentOn("Clean")
    .IsDependentOn("Build")
    .Does(() =>
{
    var settings = new DotNetCoreTestSettings
    {
        Configuration = configuration,
        NoBuild = true
    };

    if (!isWindows)
    {
        Information("Not running on Windows - skipping tests for .NET Framework");
        settings.Framework = "netcoreapp2.0";
    }

    var projects = GetFiles("./test/**/*.csproj");
    foreach(var project in projects)
    {
        DotNetCoreTest(project.FullPath, settings);
    }
});

///////////////////////////////////////////////////////////////////////////////
// Pack
///////////////////////////////////////////////////////////////////////////////
Task("Pack")
    .IsDependentOn("Clean")
    .IsDependentOn("Build")
    .Does(() =>
{
    if (SkipPack()) return;

    var settings = new DotNetCorePackSettings
    {
        Configuration = configuration,
        MSBuildSettings = msBuildSettings,
        OutputDirectory = buildArtifacts + Directory("packages"),
        NoBuild = true
    };

    DotNetCorePack(packPath, settings);
});

private bool SkipPack()
{
    if (String.IsNullOrEmpty(versions.PreReleaseLabel) && versions.BranchName != "master") 
    {
        Information("Skipping pack of release version, because not on master.");
        return true;
    }

    if (versions.PreReleaseLabel == "PullRequest")
    {
        Information("Skipping pack for pull requests.");
        return true;
    }

    return false;
}

private DotNetCoreMSBuildSettings GetMSBuildSettings()
{
    var settings = new DotNetCoreMSBuildSettings();

    settings.WithProperty("AssemblyVersion", versions.AssemblyVersion);
    settings.WithProperty("VersionPrefix", versions.AssemblyVersion);
    settings.WithProperty("FileVersion", versions.FileVersion);
    settings.WithProperty("InformationalVersion", versions.InformationalVersion);
    
    if (!String.IsNullOrEmpty(versions.VersionSuffix))
    {
        settings.WithProperty("VersionSuffix", versions.VersionSuffix);
    }

    return settings;
}

Task("Default")
  .IsDependentOn("Build")
  .IsDependentOn("Test")
  .IsDependentOn("Pack");

RunTarget(target);