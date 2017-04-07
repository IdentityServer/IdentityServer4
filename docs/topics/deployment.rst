Deployment
==========
Your identity server is `just` a standard ASP.NET Core appplication including the IdentityServer middleware.
Read the official Microsoft `documenatation <https://docs.microsoft.com/en-us/aspnet/core/publishing>`_ on publishing and deployment first.

One common question is how to configure ASP.NET Core correctly behind a load-balancer or a reverse proxy. Check this github `issue <https://github.com/aspnet/Docs/issues/2384>`_ for more info.

Configuration data
^^^^^^^^^^^^^^^^^^
This typically includes:

* resources
* clients
* startup configuration, e.g. key material

All of that configuration data must be shared by all instances running your identity server. For resources and clients you can either implement
``IResourceStore`` and ``IClientStore`` from scratch - or you can use our built-in support for `Entity Framework <https://github.com/IdentityServer/IdentityServer4.EntityFramework>`_ based databases.

Startup configuration is often either hardcoded or loaded from a configuration file or environment variables. You can use the standard
ASP.NET Core configuration system for that (see `documentation <https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration>`_).

One important piece of startup configuration is your key material, see :ref:`here <refCrypto>` for more details on key material and cryptography.

Operational data
^^^^^^^^^^^^^^^^
For certain operations, IdentityServer needs a persistence store to keep state, this includes:

* issuing authorization codes
* issuing reference and refresh tokens
* storing consent

If any of the above features are used, you need an implementation of ``IPersistedGrantStore`` - by default IdentityServer injects an in-memory version.
Again you can use our EF Core based one, build one from scratch, or use a community contribution.