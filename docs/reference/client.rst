Client
======

The ``Client`` class models an OpenID Connect or OAuth 2.0 client - 
e.g. a native application, a web application or a JS-based application.


Basics
^^^^^^

``Enabled``
    Specifies if client is enabled. Defaults to `true`.
``ClientId``
    Unique ID of the client
``ClientSecrets``
    List of client secrets - credentials to access the token endpoint.
``PublicClient``
    Specifies whether this client needs a secret to request tokens from the token endpoint.
``AllowedGrantTypes``
    Specifies the grant types the client is allowed to use. Use the ``GrantTypes`` class for common combinations.
``RequirePkce``
    Specifies whether clients using an authorization code based grant type must send a proof key
``RedirectUris``
    Specifies the allowed URIs to return tokens or authorization codes to
``AllowedScopes``
    By default a client has no access to any scopes - either specify the scopes explicitly here (recommended) - 
    or set ``AllowAccessToAllScopes`` to true.
``AllowAccessTokenViaBrowser``
    Specifies whether this client is allowed to receive access tokens via the browser. 
    This is useful to harden flows that allow multiple response types 
    (e.g. by disallowing a hybrid flow client that is supposed to use `code id_token` to add the `token` response type 
    and thus leaking the token to the browser.

Authentication/Logout
^^^^^^^^^^^^^^^^^^^^^

``PostLogoutRedirectUris``
    Specifies allowed URIs to redirect to after logout
``LogoutUri``
    Specifies logout URI at client for HTTP based logout
``LogoutSessionRequired``
    Specifies if the user's session id should be sent to the LogoutUri. Defaults to true.
``RequireSignOutPrompt``
    Specifies if the client will always show a confirmation page for sign-out. Defaults to false.
``EnableLocalLogin``
    Specifies if this client can use local accounts, or external IdPs only. Defaults to `true`.
``IdentityProviderRestrictions``
    Specifies which external IdPs can be used with this client (if list is empty all IdPs are allowed). Defaults to empty.

Token
^^^^^

``IdentityTokenLifetime``
    Lifetime to identity token in seconds (defaults to 300 seconds / 5 minutes)
``AccessTokenLifetime``
    Lifetime of access token in seconds (defaults to 3600 seconds / 1 hour)
``AuthorizationCodeLifetime``
    Lifetime of authorization code in seconds (defaults to 300 seconds / 5 minutes)
``AbsoluteRefreshTokenLifetime``
    Maximum lifetime of a refresh token in seconds. Defaults to 2592000 seconds / 30 days
``SlidingRefreshTokenLifetime``
    Sliding lifetime of a refresh token in seconds. Defaults to 1296000 seconds / 15 days
``RefreshTokenUsage``
    ``ReUse`` the refresh token handle will stay the same when refreshing tokens
    
    ``OneTime`` the refresh token handle will be updated when refreshing tokens
``RefreshTokenExpiration``
    ``Absolute`` the refresh token will expire on a fixed point in time (specified by the AbsoluteRefreshTokenLifetime)
    
    ``Sliding`` when refreshing the token, the lifetime of the refresh token will be renewed (by the amount specified in SlidingRefreshTokenLifetime). The lifetime will not exceed `AbsoluteRefreshTokenLifetime`.
``UpdateAccessTokenClaimsOnRefresh``
    Gets or sets a value indicating whether the access token (and its claims) should be updated on a refresh token request.
``AccessTokenType``
    Specifies whether the access token is a reference token or a self contained JWT token (defaults to `Jwt`).

    **note** production usage of reference tokens requires and implementation of ``ITokenHandleStore``.
``IncludeJwtId``
    Specifies whether JWT access tokens should have an embedded unique ID (via the `jti` claim).
``AllowedCorsOrigins``
    If specified, will be used by the default CORS policy service implementations (In-Memory and EF) to build a CORS policy for JavaScript clients.
``Claims``
    Allows settings claims for the client (will be included in the access token).
``AlwaysSendClientClaims``
    If set, the client claims will be sent for every flow. If not, only for client credentials flow (default is `false`)
``PrefixClientClaims``
    If set, all client claims will be prefixed with `client_` to make sure they don't accidentally collide with user claims. Default is `true`.

Consent Screen
^^^^^^^^^^^^^^

``RequireConsent``
    Specifies whether a consent screen is required. Defaults to `true`.
``AllowRememberConsent``
    Specifies whether user can choose to store consent decisions. Defaults to `true`.
    
    **note** production usage of that features requires an implementation of ``IConsentStore``.
``ClientName``
    Client display name (used for logging and consent screen)
``ClientUri``
    URI to further information about client (used on consent screen)
``LogoUri``
    URI to client logo (used on consent screen)