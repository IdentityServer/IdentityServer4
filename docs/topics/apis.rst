.. _refProtectingApis:
Protecting APIs
===============
IdentityServer issues access tokens in the `JWT <https://tools.ietf.org/html/rfc7519>`_ (JSON Web Token) format by default.

Every relevant platform today has support for validating JWT tokens, a good list of JWT libraries can be found `here <https://jwt.io>`_.
Popular libraries are e.g.:

* `JWT bearer authentication middleware <https://www.nuget.org/packages/Microsoft.AspNetCore.Authentication.JwtBearer/>`_ for ASP.NET Core
* `JWT bearer authentication middleware <https://www.nuget.org/packages/Microsoft.Owin.Security.Jwt>`_ for Katana
* `jsonwebtoken <https://www.npmjs.com/package/jsonwebtoken>`_ for nodejs

Protecting an MVC Core-based API is only a matter of adding the nuget package *(Microsoft.AspNetCore.Authentication.JwtBearer)* to your project
and adding the middleware to the ASP.NET Core pipeline::

    public class Startup
    {
        public void Configure(IApplicationBuilder app)
        {
            app.UseJwtBearerAuthentication(new JwtBearerOptions
            {
                // base-address of your identityserver
                Authority = "https://demo.identityserver.io",
                
                // name of the API resource
                Audience = "api1",

                AutomaticAuthenticate = true,
                AutomaticChallenge = true
            });

            app.UseMvc();
        }
    }

The IdentityServer authentication middleware
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
Our authentication middleware serves the same purpose as the above middleware 
(in fact it uses the Microsoft JWT middleware internally), but adds a couple of additional features:

* support for both JWTs and reference tokens
* extensible caching for reference tokens
* unified configuration model
* scope validation

For the simplest case, our middleware looks very similar to the above snippet::

    public class Startup
    {
        public void Configure(IApplicationBuilder app)
        {
            app.UseIdentityServerAuthentication(new IdentityServerAuthenticationOptions
            {
                Authority = "https://demo.identityserver.io",
                ApiName = "api1"

                AutomaticAuthenticate = true,
                AutomaticChallenge = true
            });

            app.UseMvc();
        }
    }

You can get the middleware from `nuget <https://www.nuget.org/packages/IdentityServer4.AccessTokenValidation/>`_ 
or `github <https://github.com/IdentityServer/IdentityServer4.AccessTokenValidation>`_.

Supporting reference tokens
^^^^^^^^^^^^^^^^^^^^^^^^^^^
If the incoming token is not a JWT, our middleware will contact the introspection endpoint found in the discovery document to validate the token.
Since the introspection endpoint requires authentication, you need to supply the configured API secret, e.g.::

    app.UseIdentityServerAuthentication(new IdentityServerAuthenticationOptions
    {
        Authority = "https://demo.identityserver.io",
        ApiName = "api1",
        ApiSecret = "secret",

        AutomaticAuthenticate = true,
        AutomaticChallenge = true
    });

Typically, you don't want to do a roundtrip to the introspection endpoint for each incoming request. The middleware has a built-in cache that you can enable like this::

    app.UseIdentityServerAuthentication(new IdentityServerAuthenticationOptions
    {
        Authority = "https://demo.identityserver.io",
        ApiName = "api1",
        ApiSecret = "secret",

        EnableCaching = true,
        CacheDuration = TimeSpan.FromMinutes(10), // that's the default

        AutomaticAuthenticate = true,
        AutomaticChallenge = true
    });

The middleware will use whatever `IDistributedCache` implementation is registered in the DI container (e.g. the standad `IDistributedInMemoryCache`).

Validating scopes
^^^^^^^^^^^^^^^^^
The `ApiName` property checks if the token has a matching audience (or short ``aud``) claim.

In IdentityServer you can also sub-divide APIs into multiple scopes. If you need that granularity and want to check those scopes at the middleware level, 
you can add the ``AllowedScopes`` property::

    app.UseIdentityServerAuthentication(new IdentityServerAuthenticationOptions
    {
        Authority = "https://demo.identityserver.io",
        ApiName = "api1",
        
        AllowedScopes = { "api1.read", "api1.write" }

        AutomaticAuthenticate = true,
        AutomaticChallenge = true
    });


**Note on Targeting Earlier .NET Frameworks**

When the middleware calls the configured metadata endpoint during token validation, you may encounter runtime exceptions related to SSL/TLS failures if you are targeting your build to an earlier .NET Framework (for example, NET452) due to the default configuration for HTTPS communication found in earlier versions of the framework.  If this occurs, you can avoid the problem by enabling support for the latest versions of TLS through your security protocol configuration located within ServicePointManager.  The code can go in your Startup.cs for example, and would be as follows::

    #if NET452
        System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
    #endif

The highest level error you will likely see will be:
    
    System.InvalidOperationException: IDX10803: Unable to obtain configuration from: 'https://MYWEBSITE.LOCAL/.well-known/openid-configuration'.

The originating error will reflect something similar to the following:
    
    System.Security.Authentication.AuthenticationException: A call to SSPI failed, see inner exception. ---> System.ComponentModel.Win32Exception: The client and server cannot communicate, because they do not possess a common algorithm

