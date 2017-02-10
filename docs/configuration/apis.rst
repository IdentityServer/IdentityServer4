Protecting APIs
===============

Protecting APIs with access tokens issued by your identityserver is easy - simply add our token validation middleware
to the ASP.NET Core pipeline and configure the identityserver base address and the scope::

    public class Startup
    {
        public void Configure(IApplicationBuilder app)
        {
            app.UseIdentityServerAuthentication(new IdentityServerAuthenticationOptions
            {
                Authority = "https://demo.identityserver.io",
                AllowedScopes = { "api1" },
            });

            app.UseMvc();
        }
    }

You can get the middleware from `nuget <https://www.nuget.org/packages/IdentityServer4.AccessTokenValidation/>`_ 
or `github <https://github.com/IdentityServer/IdentityServer4.AccessTokenValidation>`_.

**Note on Targeting Earlier .NET Frameworks**

When the middleware calls the configured metadata endpoint during token validation, you may encounter runtime exceptions related to SSL/TLS failures if you are targeting your build to an earlier .NET Framework (for example, NET452) due to the default configuration for HTTPS communication found in earlier versions of the framework.  If this occurs, you can avoid the problem by enabling support for the latest versions of TLS through your security protocol configuration located within ServicePointManager.  The code can go in your Startup.cs for example, and would be as follows::

    #if NET452
        System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
    #endif

The highest level error you will likely see will be:
    
    System.InvalidOperationException: IDX10803: Unable to obtain configuration from: 'https://MYWEBSITE.LOCAL/.well-known/openid-configuration'.

The originating error will reflect something similar to the following:
    
    System.Security.Authentication.AuthenticationException: A call to SSPI failed, see inner exception. ---> System.ComponentModel.Win32Exception: The client and server cannot communicate, because they do not possess a common algorithm

