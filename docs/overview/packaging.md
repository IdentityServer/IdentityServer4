# Packaging and Builds

IdentityServer consists of a number of nuget packages.

### IdentityServer4

[nuget](https://www.nuget.org/packages/IdentityServer4/) | [github](https://github.com/identityserver/IdentityServer4)

Contains the core IdentityServer object model, services and middleware. 
Only contains support for in-memory configuration and user stores - but you can plug-in support for other stores via the configuration. This is what the other repos and packages are about.

### Access token validation middleware
ASP.NET Core middleware for validating tokens in APIs. Provides an easy way to validate access tokens (both JWT and reference) and enforce scope requirements.

[nuget](https://www.nuget.org/packages/IdentityServer4.AccessTokenValidation/) | [github](https://github.com/IdentityServer/IdentityServer4.AccessTokenValidation)

## Dev builds

In addition we publish dev/interim builds to MyGet.
Add the following feed to your Visual Studio if you want to give them a try:

[https://www.myget.org/F/identity/](https://www.myget.org/F/identity/)