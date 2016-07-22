var target          = Argument("target", "Default");
var configuration   = Argument<string>("configuration", "Release");

///////////////////////////////////////////////////////////////////////////////
// GLOBAL VARIABLES
///////////////////////////////////////////////////////////////////////////////
var isLocalBuild        = !AppVeyor.IsRunningOnAppVeyor;
var solutionPath        = Directory("./src/IdentityServer4");
var sourcePath          = Directory("./src");
var testsPath           = Directory("test");
var buildArtifacts      = Directory("./cakeartifacts/packages");

Task("Build")
    .IsDependentOn("Clean")
    .IsDependentOn("Restore")
    .Does(() =>
{
	var projects = GetFiles("./**/project.json");

	foreach(var project in projects)
	{
	    DotNetCoreBuild(project.GetDirectory().FullPath, new DotNetCoreBuildSettings {
	        Configuration = configuration
	        // Runtime = IsRunningOnWindows() ? null : "unix-x64"
	    });
    }
});

Task("RunTests")
    .IsDependentOn("Restore")
    .IsDependentOn("Clean")
    .Does(() =>
{
    var projects = GetFiles("./test/**/project.json");

    foreach(var project in projects)
	{
        DotNetCoreTest(project.GetDirectory().FullPath, new DotNetCoreTestSettings {
            Configuration = configuration
      });
    }
});

Task("Pack")
    .IsDependentOn("Restore")
    .IsDependentOn("Clean")
    .Does(() =>
{
    var settings = new DotNetCorePackSettings
    {
        Configuration = configuration,
        OutputDirectory = buildArtifacts,
    };

    if(!isLocalBuild)
    {
        settings.VersionSuffix = AppVeyor.Environment.Build.Number.ToString().PadLeft(5,'0');
    }

    DotNetCorePack(solutionPath, settings);
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
    Sources = new [] {
        "https://api.nuget.org/v3/index.json",
    },
  };

  //Restore at root until preview1-002702 bug fixed
  DotNetCoreRestore(sourcePath, settings);
  DotNetCoreRestore(testsPath, settings);
});

Task("Default")
  .IsDependentOn("Build")
  .IsDependentOn("RunTests")
  .IsDependentOn("Pack");

RunTarget(target);