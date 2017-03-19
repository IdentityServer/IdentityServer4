Architecture
============
IdentityServer is ASP.NET Core middleware and runs on .NET Core or the classic .NET Framework.

IdentityServer adds OpenID Connect and OAuth 2.0 protocol endpoints to any ASP.NET Core-based application.
In the easiest case, you build an application that contains a login and logout page (and maybe consent - depending on your needs),
and the IdentityServer middleware adds the necessary protocol heads to it, so that client applications can talk to it using those standard protocols.

.. image:: images/architecture1.png

An OpenID Connect or OAuth 2.0 client would then use one of the protocol endpoints (e.g. the authorize endpoint) to request tokens (step 1 on below figure).
Our authorize endpoint implementation will then do all the necessary protocol pre-processing, e.g. checking the syntactical and semantic validity of the protocol parameters.
Then - if necessary - we will redirect to the registered URL of the login page (*/account/login* by default) passing a return URL parameter 
that points back to the authorize endpoint (step 2 on below figure).

On that page, your code will take over and can implement whatever login/register/change password workflow you want. You can also use an 
arbitrary UI framework for that. We will give you APIs that you can use to communicate back to IdentityServer to get more information about the 
current authentication request, e.g. client information, requested display language etc.

When you are done, you sign-in the user using the standard ASP.NET Core authentication manager APIs and redirect back to the return URL.
From there we will take over to do all the protocol post-processing, creating the tokens and redirection back to the client.

.. image:: images/architecture2.png

IdentityServer contains more protocol endpoints. Most of them are customizable by implementing an interface.