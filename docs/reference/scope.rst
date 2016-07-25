Scope
=====

The `Scope` class models a resource in your system.

Basics
^^^^^^

``Enabled``
    Indicates if scope is enabled and can be requested. Defaults to `true`.
``Name``
    The unique name of the scope. This is the value a client will use to request the scope.
``Type``
    Either ``Identity`` (for user identity data like name or email) or ``Resource`` (for APIs). Defaults to ``Resource``.
``Claims``
    List of user claims that should be included in the identity (identity scope) or access token (resource scope). 
``IncludeAllClaimsForUser``
    If enabled, all claims for the user will be included in the token. Defaults to `false`.
``ClaimsRule``
    Rule for determining which claims should be included in the token (this is implementation specific)
``ShowInDiscoveryDocument``
    Specifies whether this scope is shown in the discovery document. Defaults to `true`.


Scope Claims
^^^^^^^^^^^^

Scope can also specify the claims that go into the corresponding token 
(identity token for identity scopes, access token for resource scopes).

The ``ScopeClaim`` class has the following properties:

``Name``
    Name of the claim
``Description``
    Description of the claim
``AlwaysIncludeInIdToken``
    If a client requests and identity token only, all claims will be included in the token.
    If a client requests both an identity and access token, by default the identity claims will be available
    via the userinfo endpoint only. This reduces the size of the identity token.

    If identity token size is not a concern, this setting can be used to override the default behavior. Defaults to `false`.

Introspection
^^^^^^^^^^^^^

The introspection endpoint requires authentication. 
Use the below settings to configure the credential as well as introspection behavior.

``ScopeSecrets``
    Adds a list of secrets to the scope for accessing the the introspection endpoint (only applicable to resource scopes).
``AllowUnrestrictedIntrospection``
    Allows this scope to see all other scopes in the access token when using the introspection endpoint.

Consent Screen
^^^^^^^^^^^^^^

``DisplayName``
    Display name for consent screen.
``Description``
    Description for the consent screen.
``Required``
    Specifies whether the user can de-select the scope on the consent screen. Defaults to `false`.
``Emphasize``
    Specifies whether the consent screen will emphasize this scope. Use this setting for sensitive or important scopes. Defaults to `false`.