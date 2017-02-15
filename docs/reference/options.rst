.. _refOptions:
IdentityServer Options
======================

* ``IssuerUri``
    Set the issuer name that will appear in the discovery document and the issued JWT tokens.
    It is recommended to not set this property, which infers the issuer name from the host name that is used by the clients.

Endpoints
^^^^^^^^^
Allows enabling/disabling individual endpoints, e.g. token, authorize, userinfo etc.

By default all endpoints are enabled, but you can lock down your server by disbling endpoint that you don't need.

Discovery
^^^^^^^^^
Allows enabling/disabling various sections of the discovery document, e.g. endpoints, scopes, claims, grant types etc.

The ``CustomEntries`` dictionary allows adding custom elements to the discovery document.

Authentication
^^^^^^^^^^^^^^
* ``AuthenticationScheme``
    If set, specifies the cookie middleware you want to use. If not set, IdentityServer will use a built-in cookie middleware with default values.

* ``RequireAuthenticatedUserForSignOutMessage``
    Indicates if user must be authenticated to accept parameters to end session endpoint. Defaults to ``false``.

* ``FederatedSignOutPaths``
    Collection of paths that match ``SignedOutCallbackPath`` on any middleware being used to support external identity providers (such as AzureAD, or ADFS).
    ``SignedOutCallbackPath`` is used as the "signout cleanup" endpoint called from upstream identity providers when the user signs out of that upstream provider.
    This ``SignedOutCallbackPath`` is typically invoked in an ``<iframe>`` from the upstream identity provider, and is intended to sign the user out of the application. 
    Given that IdentityServer should notify all of its client applications when a user signs out, IdentityServer must extend the behavior at these ``SignedOutCallbackPath`` endpoints to sign the user our of any client applictions of IdentityServer.


Events
^^^^^^
Not yet implemented in IdentityServer.

InputLengthRestrictions
^^^^^^^^^^^^^^^^^^^^^^^
Allows setting length restrictions on various protocol parameters like client id, scope, redirect URI etc.

UserInteraction
^^^^^^^^^^^^^^^

* ``LoginUrl``, ``LogoutUrl``, ``ConsentUrl``, ``ErrorUrl``
    Sets the the URLs for the login, logout, consent and error pages.
* ``LoginReturnUrlParameter``
    Sets the name of the return URL parameter passed to the login page. Defaults to *returnUrl*.
* ``LogoutIdParameter``
    Sets the name of the logout message id parameter passed to the logout page. Defaults to *logoutId*.
* ``ConsentReturnUrlParameter``
    Sets the name of the return URL parameter passed to the consent page. Defaults to *returnUrl*.
* ``ErrorIdParameter``
    Sets the name of the error message id parameter passed to the error page. Defaults to *errorId*.
* ``CustomRedirectReturnUrlParameter``
    Sets the name of the return URL parameter passed to a custom redirect from the authorization endpoint. Defaults to *returnUrl*.
* ``CookieMessageThreshold``
    Certain interactions between IdentityServer and some UI pages require a cookie to pass state and context (any of the pages above that have a configurable "message id" parameter).
    Since browsers have limits on the number of cookies and their size, this setting is used to prevent too many cookies being created. 
    The value sets the maximum number of message cookies of any type that will be created.
    The oldest message cookies will be purged once the limit has been reached.
    This effectively indicates how many tabs can be opened by a user when using IdentityServer.

Caching
^^^^^^^
These setting only apply if the respective caching has been enabled in the services configuration in startup.

* ``ClientStoreExpiration``
    Cache duration of client configuration loaded from the client store.

* ``ResourceStoreExpiration``
    Cache duration of identity and API resource configuration loaded from the resource store.

CORS
^^^^
IdentityServer supports CORS for some of its endpoints.
The underlying CORS implementation is provided from ASP.NET Core, and as such it is automatically registered in the dependency injection system.

* ``CorsPolicyName``
    Name of the CORS policy that will be evaluated for CORS requests into IdentityServer (defaults to ``"IdentityServer4"``).
    The policy provider that handles this is implemented in terms of the ``ICorsPolicyService`` registered in the dependency injection system.
    If you wish to customize the set of CORS origins allowed to connect, then it is recommended that you provide a custom implementation of ``ICorsPolicyService``.

* ``CorsPaths``
    The endpoints within IdentityServer where CORS is supported. 
    Defaults to the discovery, user info, token, and revocation endpoints.
