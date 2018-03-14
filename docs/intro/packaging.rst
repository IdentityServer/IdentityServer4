Packaging and Builds
====================

IdentityServer consists of a number of nuget packages.

IdentityServer4
^^^^^^^^^^^^^^^
`nuget <https://www.nuget.org/packages/IdentityServer4/>`_ | `github <https://github.com/identityserver/IdentityServer4>`_

Contains the core IdentityServer object model, services and middleware. 
Only contains support for in-memory configuration and user stores - but you can plug-in support for other stores via the configuration. This is what the other repos and packages are about.

Quickstart UI
^^^^^^^^^^^^^
`github <https://github.com/IdentityServer/IdentityServer4.Quickstart.UI>`_

Contains a simple starter UI including login, logout and consent pages.

Access token validation handler
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
`nuget <https://www.nuget.org/packages/IdentityServer4.AccessTokenValidation>`_ | `github <https://github.com/IdentityServer/IdentityServer4.AccessTokenValidation>`_

ASP.NET Core authentication handler for validating tokens in APIs. The handler allows supporting both JWT and reference tokens in the same API.

ASP.NET Core Identity
^^^^^^^^^^^^^^^^^^^^^
`nuget <https://www.nuget.org/packages/IdentityServer4.AspNetIdentity>`_ | `github <https://github.com/IdentityServer/IdentityServer4.AspNetIdentity>`_

ASP.NET Core Identity integration package for IdentityServer. 
This package provides a simple configuration API to use the ASP.NET Identity management library for your IdentityServer users.

EntityFramework Core
^^^^^^^^^^^^^^^^^^^^
`nuget <https://www.nuget.org/packages/IdentityServer4.EntityFramework>`_ | `github <https://github.com/IdentityServer/IdentityServer4.EntityFramework>`_

EntityFramework Core storage implementation for IdentityServer. 
This package provides an EntityFramework implementation for the configuration and operational stores in IdentityServer.

Dev builds
^^^^^^^^^^
In addition we publish dev/interim builds to MyGet.
Add the following feed to your Visual Studio if you want to give them a try:

https://www.myget.org/F/identity/
