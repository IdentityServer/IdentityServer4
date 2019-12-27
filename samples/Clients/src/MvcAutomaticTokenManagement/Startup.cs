using Clients;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;

namespace MvcCode
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

            // add MVC
            services.AddControllersWithViews();

            // add cookie-based session management with OpenID Connect authentication
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = "cookie";
                options.DefaultChallengeScheme = "oidc";
            })
                .AddCookie("cookie", options =>
                {
                    options.Cookie.Name = "mvcclient";

                    options.ExpireTimeSpan = TimeSpan.FromHours(8);
                    options.SlidingExpiration = false;

                    // could be used to automatically trigger re-authentication (if you want to do that at the pipeline level)
                    //options.Events.OnValidatePrincipal = async e =>
                    //{
                    //    var currentToken = await e.HttpContext.GetAccessTokenAsync();

                    //    if (string.IsNullOrWhiteSpace(currentToken))
                    //    {
                    //        e.RejectPrincipal();
                    //    }
                    //};

                    options.Events.OnSigningOut = async e =>
                    {
                        // automatically revoke refresh token at signout time
                        await e.HttpContext.RevokeUserRefreshTokenAsync();
                    };
                })
                .AddOpenIdConnect("oidc", options =>
                {
                    options.Authority = Constants.Authority;
                    options.RequireHttpsMetadata = false;

                    options.ClientId = "mvc.tokenmanagement";
                    options.ClientSecret = "secret";

                    // code flow + PKCE (PKCE is turned on by default)
                    options.ResponseType = "code";
                    options.UsePkce = true;

                    options.Scope.Clear();
                    options.Scope.Add("openid");
                    options.Scope.Add("profile");
                    options.Scope.Add("api1");
                    options.Scope.Add("offline_access");

                    // keeps id_token smaller
                    options.GetClaimsFromUserInfoEndpoint = true;
                    options.SaveTokens = true;

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        NameClaimType = "name",
                        RoleClaimType = "role"
                    };
                });

            // add automatic token management
            services.AddAccessTokenManagement();

            // add HTTP client to call protected API
            services.AddUserAccessTokenClient("client", client =>
            {
                client.BaseAddress = new Uri(Constants.SampleApi);
            });
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseDeveloperExceptionPage();
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute()
                    .RequireAuthorization();
            });
        }
    }
}