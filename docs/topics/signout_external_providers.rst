.. _refSignOutExternal:
Sign-out of External Identity Providers
=======================================

When a user is :ref:`signing-out <refSignOut>` of IdentityServer, and they have used an :ref:`external identity provider <refExternalIdentityProviders>` to sign-in then it is likely that they should be redirected to also sign-out of the external provider.
Not all external providers support sign-out, as it depends on the protocol and features they support.

To detect that a user must be redirected to an external identity provider for sign-out is typically done by using a ``idp`` claim issued into the cookie at IdentityServer.
The value set into this claim is the ``AuthenticationScheme`` of the corresponding authentication middleware.
At sign-out time this claim is consulted to know if an external sign-out is required.

Redirecting the user to an external identity provider is problematic due to the cleanup and state management already required by the normal sign-out workflow.
The only way to then complete the normal sign-out and cleanup process at IdentityServer is to then request from the external identity provider that after its logout that the user be redirected back to IdentityServer.
Not all external providers support post-logout redirects, as it depends on the protocol and features they support.

The workflow at sign-out is then to revoke IdentityServer's authentication cookie, and then redirect to the external provider requesting a post-logout redirect.
The post-logout redirect shoud maintain the necessary sign-out state described :ref:`here <refSignOut>` (i.e. the ``logoutId`` parameter value).
To redirect back to IdentityServer after the external provider sign-out, the ``RedirectUri`` should be used on the ``AuthenticationProperties`` when using ASP.NET Core's ``SignOutAsync`` API, for example::

    // delete local authentication cookie
    await HttpContext.Authentication.SignOutAsync();

    string url = Url.Action("Logout", new { logoutId = logoutId });
    try
    {
        // hack: try/catch to handle social providers that throw
        await HttpContext.Authentication.SignOutAsync(vm.ExternalAuthenticationScheme, 
            new AuthenticationProperties { RedirectUri = url });
    }
    catch(NotSupportedException) // this is for the external providers that don't have signout
    {
    }
    catch(InvalidOperationException) // this is for Windows/Negotiate
    {
    }


.. Note:: It is necessary to wrap the call to ``SignOutAsync`` in a ``try/catch`` because not all external providers support sign-out, and they express it by throwing.

Once the user is signed-out of the external provider and then redirected back, the normal sign-out processing at IdentityServer should execute which involves processing the ``logoutId`` and doing all necessary cleanup.
