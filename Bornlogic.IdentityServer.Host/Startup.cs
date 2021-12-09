// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Bornlogic.IdentityServer.AspNetIdentity;
using Bornlogic.IdentityServer.Configuration;
using Bornlogic.IdentityServer.Configuration.DependencyInjection;
using Bornlogic.IdentityServer.Configuration.DependencyInjection.BuilderExtensions;
using Bornlogic.IdentityServer.Host.Repositories.Extensions;
using Bornlogic.IdentityServer.Host.Services;
using Bornlogic.IdentityServer.Host.Stores;
using Bornlogic.IdentityServer.Host.Stores.Contracts;
using Bornlogic.IdentityServer.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace Bornlogic.IdentityServer.Host
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddRazorPages()
                .AddRazorPagesOptions(options =>
                    {
                        options.Conventions.AuthorizeAreaFolder("Identity", "/Account/Manage");
                    });

            services.AddIdentity<ApplicationUser, IdentityRole>().AddDefaultTokenProviders();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
                options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
                options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            }).AddCookie(options =>
            {
                options.CookieManager = new ChunkingCookieManager();

                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.None;
                options.Cookie.SecurePolicy = CookieSecurePolicy.None;
            });

            services.AddIdentityServer(options =>
                {
                    options.UserInteraction.LoginUrl = "/Identity/Account/Login";
                    options.UserInteraction.LogoutUrl = "/Identity/Account/Logout";
                })
                .AddResourceStore<ResourcesStore>()
                .AddClientStore<ClientStore>()
                .AddPersistedGrantStore<PersistentGrantStore>()
                .AddAspNetIdentity<ApplicationUser>()
                .AddProfileService<ProfileService>()
                .AddResourceOwnerValidator<MyResourceOwnerPasswordValidator>().AddDeveloperSigningCredential();

            services.AddTransient(typeof(IRoleStore<>), typeof(RoleStore<>));
            services.AddTransient<IApplicationUserStore, UserStore<ApplicationUser>>();
            services.AddTransient(typeof(IUserStore<>), typeof(UserStore<>));

            services.AddTransient<IProfileService, ProfileService>();
            services.AddTransient<IEmailSender, EmailSender>();

            services
                //.AddMongoRepositoryOptions
                //(
                //    _environmentConfiguration.AuthServerMongo.ConnectionString,
                //    _environmentConfiguration.AuthServerMongo.DatabaseName,
                //    null,
                //    TimeSpan.FromSeconds(60)
                //)
                .RegisterMongoStoreRepositories();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseIdentityServer();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }
    }
}