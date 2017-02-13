.. _refInteractionService:
IdentityServer Interaction Service
==================================

This interface is intended to provide services be used by the user interface to communicate with IdentityServer, mainly pertaining to user interaction.

``GetAuthorizationContextAsync``
    Returns the ``AuthorizationRequest`` for the login and consent pages.

``GetErrorContextAsync``
    Returns the ``ErrorMessage`` for the error page.

``GetLogoutContextAsync``
    Returns the ``LogoutRequest`` for the logout page.

``IsValidReturnUrl``
    Indicates if the passed URL is a valid URL to redirect the user to after login or consent.

``GrantConsentAsync``
    Informs IdentityServer of the user's consent to a particular authorization request.

``GetAllUserConsentsAsync``
``RevokeUserConsentAsync``
``RevokeTokensForCurrentSessionAsync``

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
^^^^^^^^^^^^^^^^^^^^
``DisplayMode``
    The display mode passed from the authorization request.
``UiLocales``
    The UI locales passed from the authorization request.
``Error``
    The error code.
``RequestId``
    The per-request identifier. This can be used to display to the end user and can be used in diagnostics.

LogoutRequest
^^^^^^^^^^^^^^^^^^^^
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
