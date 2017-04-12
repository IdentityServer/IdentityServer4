.. _refExternalIdentityProviders:
Sign-in with External Identity Providers
========================================

ASP.NET Core has a flexible way to deal with external authentication. This involves a couple of steps.

.. Note:: If you are using ASP.NET Identity, many of the underlying technical details are hidden from you. It is recommended that you also read the Microsoft `docs <https://docs.microsoft.com/en-us/aspnet/core/security/authentication/social/>`_ and do the ASP.NET Identity :ref:`quickstart <refAspNetIdentityQuickstart>`.

Adding authentication middleware
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
The protocol implementation that is needed to talk to an external provider is encapsulated in an so-called *authentication middleware*.
Some providers use proprietary protocols (e.g. social providers like Facebook) and some use standard protocols, e.g. OpenID Connect, WS-Federation or SAML2p.

See this :ref:`quickstart <refExternalAuthenticationQuickstart>` for step-by-step instructions for adding middleware and configuring it.

The role of cookies
^^^^^^^^^^^^^^^^^^^
One parameter on the authentication middleware options is called the ``SignInScheme``, e.g.::

   app.UseGoogleAuthentication(new GoogleOptions
    {
        AuthenticationScheme = "unique name of middleware",
        SignInScheme = "name of cookie middleware to use",

        ClientId = "..."",
        ClientSecret = "..."
    });

The signin scheme specifies the name of the cookie middleware that will temporarily store the outcome of the external authentication, 
e.g. the claims that got sent by the external provider. This is necessary, since there are typically a couple of redirects involved until you are done with the 
external authentication process.

If you don't take over control of your cookie configuration by setting your own authentication scheme on the IdentityServer options (see :ref:`here <refSignIn>`),
we automatically register a cookie middleware called ``idsrv.external``.

You can also register your own like this::

    app.UseCookieAuthentication(new CookieAuthenticationOptions
    {
        AuthenticationScheme = "my.custom.scheme",
        AutomaticAuthenticate = false,
        AutomaticChallenge = false
    });


Triggering the authentication middleware
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
You invoke an external authentication middleware via the ``ChallengeAsync`` method on the ASP.NET Core authentication manager (or using the MVC ``ChallengeResult``).

You typically want to pass in some options to the challenge operation, e.g. the path to your callback page and the name of the provider for bookkeeping, e.g.::

    var callbackUrl = Url.Action("ExternalLoginCallback", new { returnUrl = returnUrl });
    
    var props = new AuthenticationProperties
    {
        RedirectUri = callbackUrl,
        Items = { { "scheme", provider } }
    };
    
    return new ChallengeResult(provider, props);

Handling the callback and signing in the user
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
On the callback page your typical tasks are:

* inspect the identity returned by the external provider.
* make a decision how you want to deal with that user. This might be different based on the fact if this is a new user or a returning user.
* new users might need additional steps and UI before they are allowed in.
* probably create a new internal user account that is linked to the external provider.
* store the external claims that you want to keep.
* delete the temporary cookie
* sign-in the user

**Inspecting the external identity**::

    // read external identity from the temporary cookie
    var info = await HttpContext.Authentication.GetAuthenticateInfoAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme);
    var tempUser = info?.Principal;
    if (tempUser == null)
    {
        throw new Exception("External authentication error");
    }

    // retrieve claims of the external user
    var claims = tempUser.Claims.ToList();

    // try to determine the unique id of the external user - the most common claim type for that are the sub claim and the NameIdentifier
    // depending on the external provider, some other claim type might be used
    var userIdClaim = claims.FirstOrDefault(x => x.Type == JwtClaimTypes.Subject);
    if (userIdClaim == null)
    {
        userIdClaim = claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
    }
    if (userIdClaim == null)
    {
        throw new Exception("Unknown userid");
    }

**Clean-up and sign-in**::

    // issue authentication cookie for user
    await HttpContext.Authentication.SignInAsync(user.SubjectId, user.Username, provider, props, additionalClaims.ToArray());

    // delete temporary cookie used during external authentication
    await HttpContext.Authentication.SignOutAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme);

    // validate return URL and redirect back to authorization endpoint or a local page
    if (_interaction.IsValidReturnUrl(returnUrl) || Url.IsLocalUrl(returnUrl))
    {
        return Redirect(returnUrl);
    }

    return Redirect("~/");