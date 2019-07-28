New-Item -ItemType Directory -Force -Path ./nuget
set-location ./src/Storage
dotnet --info
dotnet restore
dotnet "C:\Users\VssAdministrator\.nuget\packages\minver\1.1.0\build\../minver/MinVer.dll"
