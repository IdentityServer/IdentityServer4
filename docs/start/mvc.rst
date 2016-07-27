Connecting an MVC Application
=============================

You can integrate identityserver into your MVC application to authenticate users and request access token.

An MVC application typically uses the hybrid flow - use :ref:`this <start_clients_mvc>` sample
to register the client.

In your MVC application startup, you can use the standard Microsoft ASP.NET OpenID Connect middleware to connect to identityserver::

    public class Startup
    {
        public void Configure(IApplicationBuilder app)
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            
            app.UseStaticFiles();

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationScheme = "cookies",
                AutomaticAuthenticate = true,
            });

            var oidcOptions = new OpenIdConnectOptions
            {
                AuthenticationScheme = "oidc",
                SignInScheme = "cookies",

                Authority = "https://demo.identityserver.io",
                ClientId = "mvc",
                ClientSecret = "secret",
                ResponseType = "code id_token",
                SaveTokens = true,
                GetClaimsFromUserInfoEndpoint = true,
                
                TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = "name",
                    RoleClaimType = "role"
                }
            };

            oidcOptions.Scope.Clear();
            oidcOptions.Scope.Add("openid");
            oidcOptions.Scope.Add("profile");
            oidcOptions.Scope.Add("api1");

            app.UseOpenIdConnectAuthentication(oidcOptions);

            app.UseMvcWithDefaultRoutes();
        }
    }
