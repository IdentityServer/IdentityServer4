New-Item -ItemType Directory -Force -Path ./nuget
dotnet tool install --tool-path tools SignClient

$cd = Get-Location

# storage
""
"###########################################"
"######### IdentityServer4.Storage #########"
"###########################################"
""
set-location ./src/Storage
& ./build.ps1 $args
Set-Location $cd

if ($LASTEXITCODE -ne 0)
{
    exit $LASTEXITCODE
}

Copy-Item -path .\src\Storage\artifacts\*.nupkg -Destination .\nuget



# core
""
"###########################################"
"######### IdentityServer4 #################"
"###########################################"
""
set-location ./src/IdentityServer4
& ./build.ps1 $args
Set-Location $cd

if ($LASTEXITCODE -ne 0)
{
    exit $LASTEXITCODE
}

Copy-Item -path .\src\IdentityServer4\artifacts\*.nupkg -Destination .\nuget



# EF storage
""
"############################################################"
"######### IdentityServer4.EntityFramework.Storage ##########"
"############################################################"
""
set-location ./src/EntityFramework.Storage
& ./build.ps1 $args
Set-Location $cd

if ($LASTEXITCODE -ne 0)
{
    exit $LASTEXITCODE
}

Copy-Item -path .\src\EntityFramework.Storage\artifacts\*.nupkg -Destination .\nuget

# EF
""
"###################################################"
"######### IdentityServer4.EntityFramework #########"
"###################################################"
""
set-location ./src/EntityFramework
& ./build.ps1 $args
Set-Location $cd

if ($LASTEXITCODE -ne 0)
{
    exit $LASTEXITCODE
}

Copy-Item -path .\src\EntityFramework\artifacts\*.nupkg -Destination .\nuget

# aspid
""
"###################################################"
"######### IdentityServer4.AspNetIdentity ##########"
"###################################################"
""
set-location ./src/AspNetIdentity
& ./build.ps1 $args
Set-Location $cd

if ($LASTEXITCODE -ne 0)
{
    exit $LASTEXITCODE
}

Copy-Item -path .\src\AspNetIdentity\artifacts\*.nupkg -Destination .\nuget