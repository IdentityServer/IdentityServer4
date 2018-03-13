.. _refInteractionService:
IdentityServer Interaction Service
==================================

The ``IIdentityServerInteractionService`` interface is intended to provide services to be used by the user interface to communicate with IdentityServer, mainly pertaining to user interaction.
It is available from the dependency injection system and would normally be injected as a constructor parameter into your MVC controllers for the user interface of IdentityServer.

IIdentityServerInteractionService APIs
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

``GetAuthorizationContextAsync``
    Returns the ``AuthorizationRequest`` based on the ``returnUrl`` passed to the login or consent pages.

``IsValidReturnUrl``
    Indicates if the ``returnUrl`` is a valid URL for redirect after login or consent.

``GetErrorContextAsync``
    Returns the ``ErrorMessage`` based on the ``errorId`` passed to the error page.

``GetLogoutContextAsync``
    Returns the ``LogoutRequest`` based on the ``logoutId`` passed to the logout page.

``CreateLogoutContextAsync``
    Used to create a ``logoutId`` if there is not one presently.
    This creates a cookie capturing all the current state needed for signout and the ``logoutId`` identifies that cookie.
    This is typically used when there is no current ``logoutId`` and the logout page must capture the current user's state needed for sign-out prior to redirecting to an external identity provider for signout.
    The newly created ``logoutId`` would need to be round-tripped to the external identity provider at signout time, and then used on the signout callback page in the same way it would be on the normal logout page.

``GrantConsentAsync``
    Accepts a ``ConsentResponse`` to inform IdentityServer of the user's consent to a particular ``AuthorizationRequest``.

``GetAllUserConsentsAsync``
    Returns a collection of ``Consent`` for the user.

``RevokeUserConsentAsync``
    Revokes all of a user's consents and grants for a client.

``RevokeTokensForCurrentSessionAsync``
    Revokes all of a user's consents and grants for clients the user has signed into during their current session.

AuthorizationRequest
^^^^^^^^^^^^^^^^^^^^
``ClientId``
    The client identifier that initiated the request.
``RedirectUri``
    The URI to redirect the user to after successful authorization.
``DisplayMode``
    The display mode passed from the authorization request.
``UiLocales``
    The UI locales passed from the authorization request.
``IdP``
    The external identity provider requested. 
    This is used to bypass home realm discovery (HRD). 
    This is provided via the "idp:" prefix to the ``acr_values`` parameter on the authorize request.
``Tenant``
    The tenant requested.
    This is provided via the "tenant:" prefix to the ``acr_values`` parameter on the authorize request.
``LoginHint``
    The expected username the user will use to login. 
    This is requested from the client via the ``login_hint`` parameter on the authorize request.
``PromptMode``
    The prompt mode requested from the authorization request.
``AcrValues``
    The acr values passed from the authorization request.
``ScopesRequested``
    The scopes requested from the authorization request.
``Parameters``
    The entire parameter collection passed to the authorization request.

ErrorMessage
^^^^^^^^^^^^
``DisplayMode``
    The display mode passed from the authorization request.
``UiLocales``
    The UI locales passed from the authorization request.
``Error``
    The error code.
``RequestId``
    The per-request identifier. This can be used to display to the end user and can be used in diagnostics.

LogoutRequest
^^^^^^^^^^^^^
``ClientId``
    The client identifier that initiated the request.
``PostLogoutRedirectUri``
    The URL to redirect the user to after they have logged out.
``SessionId``
    The user's current session id.
``SignOutIFrameUrl``
    The URL to render in an ``<iframe>`` on the logged out page to enable single sign-out.
``Parameters``
    The entire parameter collection passed to the end session endpoint.
``ShowSignoutPrompt``
    Indicates if the user should be prompted for signout based upon the parameters passed to the end session endpoint.

ConsentResponse
^^^^^^^^^^^^^^^
``ScopesConsented``
    The collection of scopes the user consented to.
``RememberConsent``
    Flag indicating if the user's consent is to be persisted.

Consent
^^^^^^^
``SubjectId``
    The subject id that granted the consent.
``ClientId``
    The client identifier for the consent.
``Scopes``
    The collection of scopes consented to.
``CreationTime``
    The date and time when the consent was granted.
``Expiration``
    The date and time when the consent will expire.
