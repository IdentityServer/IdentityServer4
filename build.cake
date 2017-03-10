var target          = Argument("target", "Default");
var configuration   = Argument<string>("configuration", "Release");

///////////////////////////////////////////////////////////////////////////////
// GLOBAL VARIABLES
///////////////////////////////////////////////////////////////////////////////
var isLocalBuild        = !AppVeyor.IsRunningOnAppVeyor;
var packPath            = Directory("./src/IdentityServer4");
var buildArtifacts      = Directory("./artifacts/packages");

Task("Build")
    .IsDependentOn("Clean")
    .IsDependentOn("Restore")
    .Does(() =>
{
    var settings = new DotNetCoreBuildSettings 
        {
            Configuration = configuration
        };

    if (!IsRunningOnWindows())
        {
            settings.Framework = "netstandard1.4";
        }

    DotNetCoreBuild(Directory("./src/IdentityServer4"), settings);
    
    if (!IsRunningOnWindows())
        {
            settings.Framework = "netcoreapp1.1";
        }

    DotNetCoreBuild(Directory("./src/Host"), settings);     
    DotNetCoreBuild(Directory("./test/IdentityServer.IntegrationTests"), settings);
    DotNetCoreBuild(Directory("./test/IdentityServer.UnitTests"), settings);
});

Task("RunTests")
    .IsDependentOn("Restore")
    .IsDependentOn("Clean")
    .Does(() =>
{
    var projects = GetFiles("./test/**/*.csproj");

    foreach(var project in projects)
    {
        var settings = new DotNetCoreTestSettings
        {
            Configuration = configuration
        };

        if (!IsRunningOnWindows())
        {
            Information("Not running on Windows - skipping tests for full .NET Framework");
            settings.Framework = "netcoreapp1.1";
        }

        DotNetCoreTest(project.FullPath, settings);
    }
});

Task("Pack")
    .IsDependentOn("Restore")
    .IsDependentOn("Clean")
    .Does(() =>
{
    if (!IsRunningOnWindows())
        {
            Information("Not running on Windows - skipping pack");
            return;
        }

    var settings = new DotNetCorePackSettings
    {
        Configuration = configuration,
        OutputDirectory = buildArtifacts,
    };

    // add build suffix for CI builds
    if(!isLocalBuild)
    {
        settings.VersionSuffix = "build" + AppVeyor.Environment.Build.Number.ToString().PadLeft(5,'0');
    }

    DotNetCorePack(packPath, settings);
});

Task("Clean")
    .Does(() =>
{
    CleanDirectories(new DirectoryPath[] { buildArtifacts });
});

Task("Restore")
    .Does(() =>
{
    var settings = new DotNetCoreRestoreSettings
    {
        Sources = new [] { "https://api.nuget.org/v3/index.json" }
    };

	var projects = GetFiles("./**/*.csproj");

	foreach(var project in projects)
	{
	    DotNetCoreRestore(project.GetDirectory().FullPath, settings);
    }
});

Task("Default")
  .IsDependentOn("Build")
  .IsDependentOn("RunTests")
  .IsDependentOn("Pack");

RunTarget(target);