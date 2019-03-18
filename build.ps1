New-Item -ItemType Directory -Force -Path ./nuget
Remove-Item -Path ./nuget/*.*

$cd = Get-Location

# storage
set-location ./src/Storage
& ./build.ps1
Set-Location $cd

if ($LASTEXITCODE -ne 0)
{
    exit $LASTEXITCODE
}

Copy-Item -path .\src\Storage\artifacts\packages\*.nupkg -Destination .\nuget

# EF storage
set-location ./src/EntityFramework.Storage
& ./build.ps1
Set-Location $cd

if ($LASTEXITCODE -ne 0)
{
    exit $LASTEXITCODE
}

Copy-Item -path .\src\EntityFramework.Storage\artifacts\packages\*.nupkg -Destination .\nuget

# EF
set-location ./src/EntityFramework
& ./build.ps1
Set-Location $cd

if ($LASTEXITCODE -ne 0)
{
    exit $LASTEXITCODE
}

Copy-Item -path .\src\EntityFramework\artifacts\packages\*.nupkg -Destination .\nuget

# core
set-location ./src/IdentityServer4
& ./build.ps1
Set-Location $cd

if ($LASTEXITCODE -ne 0)
{
    exit $LASTEXITCODE
}

Copy-Item -path .\src\IdentityServer4\artifacts\packages\*.nupkg -Destination .\nuget

# aspid
set-location ./src/AspNetIdentity
& ./build.ps1
Set-Location $cd

if ($LASTEXITCODE -ne 0)
{
    exit $LASTEXITCODE
}

Copy-Item -path .\src\AspNetIdentity\artifacts\packages\*.nupkg -Destination .\nuget

