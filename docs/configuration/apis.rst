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