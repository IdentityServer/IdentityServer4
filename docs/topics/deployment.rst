Deployment
==========
Your identity server is `just` a standard ASP.NET Core appplication including the IdentityServer middleware.
Read the official Microsoft `documentation <https://docs.microsoft.com/en-us/aspnet/core/publishing>`_ on publishing and deployment first.

The two most common task for deploying to load-balanced environment is configuration of `data protection <https://docs.microsoft.com/en-us/aspnet/core/security/data-protection/configuration/default-settings>`_, 
and setting the right `protocol scheme/host name behind load-balancers <https://docs.microsoft.com/en-us/aspnet/core/publishing/linuxproduction?tabs=aspnetcore2x>`_.

.. note:: If setting the public origin behind a reverse-proxy or load balancer does not work for you, you can hard-code the host name using the ``PublicOrigin`` property on the ``IdentityServerOptions``.

IdentityServer configuration data
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
This typically includes:

* resources
* clients
* startup configuration, e.g. key material

All of that configuration data must be shared by all instances running your identity server. For resources and clients you can either implement
``IResourceStore`` and ``IClientStore`` from scratch - or you can use our built-in support for `Entity Framework <https://github.com/IdentityServer/IdentityServer4.EntityFramework>`_ based databases.

Startup configuration is often either hardcoded or loaded from a configuration file or environment variables. You can use the standard
ASP.NET Core configuration system for that (see `documentation <https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration>`_).

One important piece of startup configuration is your key material, see :ref:`here <refCrypto>` for more details on key material and cryptography.

IdentityServer operational data
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
For certain operations, IdentityServer needs a persistence store to keep state, this includes:

* issuing authorization codes
* issuing reference and refresh tokens
* storing consent

If any of the above features are used, you need an implementation of ``IPersistedGrantStore`` - by default IdentityServer injects an in-memory version.
Again you can use our EF Core based one, build one from scratch, or use a community contribution.
