.. _refOptions:
IdentityServer Options
======================

* ``IssuerUri``
    Set the issuer name that will appear in the discovery document and the issued JWT tokens.
    It is recommended to not set this property, which infers the issuer name from the host name that is used by the clients.
* ``ProtocolLogoutUrls```
    todo

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
* ``RequireAuthenticatedUserForSignOutMessage```
    Indicates if user must be authenticated to accept parameters to end session endpoint. Defaults to false.
* ``FederatedSignOutPaths``
    todo

Events
^^^^^^
todo.

InputLengthRestrictions
^^^^^^^^^^^^^^^^^^^^^^^
Allows setting length restrictions on various protocol parameters like client id, scope, redirect URI etc.

UserInteraction
^^^^^^^^^^^^^^^

* ``LoginUrl``, ``LogoutUrl``, ``ConsentUrl``, ``ErrorUrl``
    Sets the the URLs for the login, logout, consent and error pages.
* ``LoginReturnUrlParameter``
    Sets the name of the login return URL parameter. Default to *returnUrl*
* ``LogoutIdParameter``
* ``ConsentReturnUrlParameter``
* ``ErrorIdParameter``
* ``CustomRedirectReturnUrlParameter``
* ``CookieMessageThreshold``

Caching
^^^^^^^
* ``ClientStoreExpiration```
* ``ResourceStoreExpiration``

CORS
^^^^
* ``CorsPolicyName``
* ``CorsPaths``