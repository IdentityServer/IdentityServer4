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

**In-Memory configuration stores**

The various "in-memory" configuration APIs allow for configuring IdentityServer from an in-memory list of configuration objects.
These "in-memory" collections can be hard-coded in the hosting application, or could be loaded dynamically from a configuration file or a database.
By design, though, these collections are only created when the hosting application is starting up.

Use of these configuration APIs are designed for use when prototyping, developing, and/or testing where it is not necessary to dynamically consult database at runtime for the configuration data.
This style of configuration might also be appropriate for production scenarios if the configuration rarely changes, or it is not inconvenient to require restarting the application if the value must be changed.

* ``AddInMemoryClients``
    Adds the in-memory collection of ``Client`` configuration objects.
* ``AddInMemoryIdentityResources``
    Adds the in-memory collection of ``IdentityResource`` configuration objects.
* ``AddInMemoryApiResources``
    Adds the in-memory collection of ``ApiResource`` configuration objects.

**Test stores**

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

There is no additional configuration for the middleware.

Be aware that order matters in the pipeline. 
For example, you will want to add IdentitySever before the UI framework that implementes the login screen.
