Sign-in
=======

In order for IdentityServer to issue tokens on behalf of a user, that user must sign-in to IdentityServer.

Cookie authentication
^^^^^^^^^^^^^^^^^^^^^

Authentication is tracked with a cookie managed by the cookie authentication middleware from ASP.NET Core.
You can register the cookie middleware yourself, or IdentityServer can automatically register it.

If you wish to use your own cookie authentication middleware (typically to change the default settings), then you must tell IdentityServer by setting the ``AuthenticationScheme`` configuration property (:doc:`../reference/options`).
If you do not configure this, then IdentityServer will register the middleware using the constant ``IdentityServerConstants.DefaultCookieAuthenticationScheme`` as the authentication scheme.

Login User Interface and Identity Management System
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

IdentityServer does not provide any user-interface or user database for authentication.
These are things you are expected to provide or develop yourself.
We have samples that use ASP.NET Identity (:doc:`../quickstarts/6_aspnet_identity`).

Login Workflow
^^^^^^^^^^^^^^

When IdentityServer recieves a request at the authorization endpoint and the user is not authenticated, the user will be redirected to the configured login page.
You must inform IdentityServer of the path to your login page via the ``UserInteraction`` settings (:doc:`../reference/options`).
A ``returnUrl`` parameter will be passed informing your login page where the user should be redirected once login is complete.

.. Note:: Beware `open-redirect attacks <https://en.wikipedia.org/wiki/URL_redirection#Security_issues>`_ via the ``returnUrl`` parameter. You should validate that the ``returnUrl`` refers to well-known location. See the :doc:`../reference/interactionservice` for APIs to validate the ``returnUrl`` parameter.

Login Context
^^^^^^^^^^^^^

On your login page you might require information about the context of the request such as client, prompt parameter, IdP hint, or something else.
This is made available via the ``GetAuthorizationContextAsync`` API on the ``IIdentityServerInteractionService`` (:doc:`../reference/interactionservice`).
This API accepts the ``returnUrl`` as a parameter and returns a ``AuthorizationRequest`` object with the contextual values.
