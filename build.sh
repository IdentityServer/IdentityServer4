mkdir nuget
cd ./src/Storage
dotnet --info
dotnet restore
dotnet "/home/vsts/.nuget/packages/minver/1.1.0/build/../minver/MinVer.dll"
