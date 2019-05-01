using Clients;
using IdentityModel;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Linq;
using Microsoft.AspNetCore.Authentication;

namespace MvcImplicit
{
    public class Startup
    {
        public Startup()
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = "oidc";
            })
                .AddCookie(options =>
                {
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
                    options.Cookie.Name = "mvcimplicit";
                    //options.Cookie.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict;
                })
                .AddOpenIdConnect("oidc", options =>
                {
                    options.Authority = Constants.Authority;// "https://demo.identityserver.io";
                    options.RequireHttpsMetadata = false;

                    options.ClientId = "mvc.implicit";
                    //options.ClientSecret = "secret";
                    //options.ResponseType = "code";
                    //options.ResponseMode = "query";

                    options.Scope.Clear();
                    options.Scope.Add("openid");
                    options.Scope.Add("profile");
                    options.Scope.Add("email");

                    options.SaveTokens = true;

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        NameClaimType = JwtClaimTypes.Name,
                        RoleClaimType = JwtClaimTypes.Role,
                    };

                    //options.Events = new Microsoft.AspNetCore.Authentication.OpenIdConnect.OpenIdConnectEvents
                    //{
                    //    OnTokenValidated = async n =>
                    //    {
                    //        //await n.HttpContext.Response.WriteAsync("<script>window.location='/';</script>");
                    //        //return Task.CompletedTask;
                    //    }
                    //};
                });
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseDeveloperExceptionPage();
            app.UseStaticFiles();

            //app.Use(async (ctx, next) =>
            //{
            //    var schemes = ctx.RequestServices.GetRequiredService<IAuthenticationSchemeProvider>();
            //    var handlers = ctx.RequestServices.GetRequiredService<IAuthenticationHandlerProvider>();
            //    foreach (var scheme in await schemes.GetRequestHandlerSchemesAsync())
            //    {
            //        var handler = await handlers.GetHandlerAsync(ctx, scheme.Name) as IAuthenticationRequestHandler;
            //        if (handler != null && await handler.HandleRequestAsync())
            //        {
            //            string location = null;
            //            if (ctx.Response.StatusCode == 302)
            //            {
            //                location = ctx.Response.Headers["location"];
            //            }
            //            else if (ctx.Request.Method == "GET" && !ctx.Request.Query["skip"].Any())
            //            {
            //                location = ctx.Request.Path + ctx.Request.QueryString + "&skip=1";
            //            }

            //            if (location != null)
            //            {
            //                ctx.Response.StatusCode = 200;
            //                var html = $@"
            //            <html><head>
            //                <meta http-equiv='refresh' content='0;url={location}' />
            //            </head></html>";
            //                await ctx.Response.WriteAsync(html);
            //            }

            //            return;
            //        }
            //    }

            //    await next();


            //    //                if (ctx.Request.Path == "/signout-oidc" && !ctx.Request.Query["skip"].Any())
            //    //                {
            //    //                    var location = ctx.Request.Path + ctx.Request.QueryString + "&skip=1";
            //    //                    ctx.Response.StatusCode = 200;
            //    //                    var html = $@"
            //    //<html><head>
            //    //<meta http-equiv='refresh' content='0;url={location}' />
            //    //</head></html>";
            //    //                    await ctx.Response.WriteAsync(html);
            //    //                    return;
            //    //                }

            //    //                await next();

            //    //                if (ctx.Request.Path == "/signin-oidc" && ctx.Response.StatusCode == 302)
            //    //                {
            //    //                    var location = ctx.Response.Headers["location"];
            //    //                    ctx.Response.StatusCode = 200;
            //    //                    var html = $@"
            //    //                        <html><head>
            //    //                            <meta http-equiv='refresh' content='0;url={location}' />
            //    //                        </head></html>";
            //    //                    await ctx.Response.WriteAsync(html);
            //    //                }
            //});

            app.UseAuthentication();
            app.UseMvcWithDefaultRoute();
        }
    }
}