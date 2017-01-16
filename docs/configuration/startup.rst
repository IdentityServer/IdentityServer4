Startup
=======

IdentityServer is a combination of middleware and services.
All configuration is done in your startup class.

Configuring services
^^^^^^^^^^^^^^^^^^^^
You add the IdentityServer services to the DI system by calling::

    public void ConfigureServices(IServiceCollection services)
    {
        var builder = services.AddIdentityServer();
    }

Optionally you can pass in options into this call. See :ref:`here <refOptions>` for details on options.

This will return you a builder object that in turn has a number of convenience methods to wire up additional services.

**Key material**

* ``AddSigningCredential``
    Adds a signing key service that provides the specified key material to the various token creation/validation services.
    You can pass in either an ``X509Certificate2``, a ``SigningCredential`` or a reference to a certificate from the certificate store.
* ``AddTemporarySigningCredential``
    Creates temporary key material at startup time. This is for dev only scenarios when you don't have a certificate to use.
* ``AddValidationKeys``
    Adds keys for validating tokens. They will be used by the internal token validator and will show up in the discovery document.
    This is useful for key roll-over scenarios.

**In-Memory/Test stores**

* ``AddInMemoryClients``
* ``AddInMemoryIdentityResources``
* ``AddInMemoryApiResources``
* ``AddTestUsers``

**Additional services**

* ``AddExtensionGrantValidator``
* ``AddSecretParser``
* ``AddSecretValidator``
* ``AddResourceOwnerValidator``
* ``AddProfileService``
* ``AddAuthorizeInteractionResponseGenerator``
* ``AddCustomAuthorizeRequestValidator``
* ``AddCustomTokenRequestValidator``

**Caching**

* ``AddClientStoreCache``
* ``AddResourceStoreCache``


Configuring the pipeline
^^^^^^^^^^^^^^^^^^^^^^^^
You need to add IdentityServer to the pipeline by calling::

    public void Configure(IApplicationBuilder app)
    {
        app.UseIdentityServer();
    }

Be aware that order matters in the pipeline. You want to add IdentitySever e.g. before the UI framework that implementes the login screen etc.
