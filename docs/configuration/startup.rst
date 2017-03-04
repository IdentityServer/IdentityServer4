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
    Registers ``IClientStore`` and ``ICorsPolicyService`` implementations based on the in-memory collection of ``Client`` configuration objects.
* ``AddInMemoryIdentityResources``
    Registers ``IResourceStore`` implementation based on the in-memory collection of ``IdentityResource`` configuration objects.
* ``AddInMemoryApiResources``
    Registers ``IResourceStore`` implementation based on the in-memory collection of ``ApiResource`` configuration objects.

**Test stores**

The ``TestUser`` class models a user, their credentials, and claims in IdentityServer. 
Use of ``TestUser`` is simiar to the use of the "in-memory" stores in that it is intended for when prototyping, developing, and/or testing.
The use of ``TestUser`` is not recommended in production.

* ``AddTestUsers``
    Registers ``TestUserStore`` based on a collection of ``TestUser`` objects.
    ``TestUserStore`` is used by the default quickstart UI.
    Also registers implementations of ``IProfileService`` and ``IResourceOwnerPasswordValidator``.

**Additional services**

* ``AddExtensionGrantValidator``
    Adds ``IExtensionGrantValidator`` implementation for use with extension grants.

* ``AddSecretParser``
    Adds ``ISecretParser`` implementation for parsing client or API resource credentials.

* ``AddSecretValidator``
    Adds ``ISecretValidator`` implementation for validating client or API resource credentials against a credential store.

* ``AddResourceOwnerValidator``
    Adds ``IResourceOwnerPasswordValidator`` implementation for validating user credentials for the resource owner password credentials grant type.

* ``AddProfileService``
    Adds ``IProfileService`` implementation for connecting to your custom user profile store.
    The ``DefaultProfileService`` class provides the default implementation which relies upon the authentication cookie as the only source of claims for issuing in tokens.

* ``AddAuthorizeInteractionResponseGenerator``
    Adds ``IAuthorizeInteractionResponseGenerator`` implementation to customize logic at authorization endpoint for when a user must be shown a UI for error, login, consent, or any other custom page.
    The ``AuthorizeInteractionResponseGenerator`` class provides a default implementation, so consider deriving from this existing class if you need to augment the existing behavior.

* ``AddCustomAuthorizeRequestValidator``
    Adds ``ICustomAuthorizeRequestValidator`` implementation to customize request parameter validation at the authorization endpoint.

* ``AddCustomTokenRequestValidator``
    Adds ``ICustomTokenRequestValidator`` implementation to customize request parameter validation at the token endpoint.

**Caching**

Client and resource configuration data is used frequently by IdentityServer.
If this data is being loaded from a database or other external store, then it might be expensive to frequently re-load the same data.

* ``AddClientStoreCache``
    Registers a ``IClientStore`` decorator implementation which will maintain an in-memory cache of ``Client`` configuration objects.
    The cache duration is configurable on the ``Caching`` configuration options on the ``IdentityServerOptions``.

* ``AddResourceStoreCache``
    Registers a ``IResourceStore`` decorator implementation which will maintain an in-memory cache of ``IdentityResource`` and ``ApiResource`` configuration objects.
    The cache duration is configurable on the ``Caching`` configuration options on the ``IdentityServerOptions``.

Further customization of the cache is possible:

The default caching relies upon the ``ICache<T>`` implementation.
If you wish to customize the caching behavior for the specific configuration objects, you can replace this implementation in the dependency injection system.

The default implementation of the ``ICache<T>`` itself relies upon the ``IMemoryCache`` interface (and ``MemoryCache`` implementation) provided by .NET.
If you wish to customize the in-memory caching behavior, you can replace the ``IMemoryCache`` implementation in the dependency injection system.

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
