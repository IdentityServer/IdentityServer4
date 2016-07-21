using Host.Configuration;
using Host.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using Serilog.Events;
using IdentityServer4.Configuration;
using System.Collections.Generic;
using IdentityServer4.Services.InMemory;
using IdentityServer4.Validation;

namespace Host
{
    public class Startup
    {
        private readonly IHostingEnvironment _environment;

        public Startup(IHostingEnvironment env)
        {
            _environment = env;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var cert = new X509Certificate2(Path.Combine(_environment.ContentRootPath, "idsrv3test.pfx"), "idsrv3test");

            var builder = services.AddIdentityServer(options =>
            {
                //options.EventsOptions = new EventsOptions
                //{
                //    RaiseErrorEvents = true,
                //    RaiseFailureEvents = true,
                //    RaiseInformationEvents = true,
                //    RaiseSuccessEvents = true
                //};

                options.UserInteractionOptions.LoginUrl = "/ui/login";
                options.UserInteractionOptions.LogoutUrl = "/ui/logout";
                options.UserInteractionOptions.ConsentUrl = "/ui/consent";
                options.UserInteractionOptions.ErrorUrl = "/ui/error";
            })
                .AddInMemoryClients(Clients.Get())
                .AddInMemoryScopes(Scopes.Get())
                //.AddInMemoryUsers(Users.Get())
                //.SetTemporarySigningCredential();
                .SetSigningCredential(cert);

            services.AddSingleton<List<InMemoryUser>>(Users.Get());
            services.AddTransient<UI.Login.LoginService>();
            services.AddTransient<IResourceOwnerPasswordValidator, InMemoryResourceOwnerPasswordValidator>();

            builder.AddExtensionGrantValidator<Host.Extensions.ExtensionGrantValidator>();

            // for the UI
            services
                .AddMvc()
                .AddRazorOptions(razor =>
                {
                    razor.ViewLocationExpanders.Add(new UI.CustomViewLocationExpander());
                });
        }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            // serilog filter
            Func<LogEvent, bool> serilogFilter = (e) =>
            {
                var context = e.Properties["SourceContext"].ToString();

                return (context.StartsWith("\"IdentityServer") ||
                        context.StartsWith("\"IdentityModel") ||
                        e.Level == LogEventLevel.Error ||
                        e.Level == LogEventLevel.Fatal);
            };
        
            // built-in logging filter
            Func<string, LogLevel, bool> filter = (scope, level) =>
                scope.StartsWith("IdentityServer") ||
                scope.StartsWith("IdentityModel") ||
                level == LogLevel.Error ||
                level == LogLevel.Critical;

            loggerFactory.AddConsole(filter);
            loggerFactory.AddDebug(filter);

            //var serilog = new LoggerConfiguration()
            //    .MinimumLevel.Verbose()
            //    .Enrich.FromLogContext()
            //    .Filter.ByIncludingOnly(serilogFilter)
            //    .WriteTo.LiterateConsole()
            //    .CreateLogger();

            //loggerFactory.AddSerilog(serilog);

            app.UseDeveloperExceptionPage();

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationScheme = "Temp",
                AutomaticAuthenticate = false,
                AutomaticChallenge = false
            });

            app.UseGoogleAuthentication(new GoogleOptions
            {
                AuthenticationScheme = "Google",
                SignInScheme = "Temp",
                ClientId = "998042782978-s07498t8i8jas7npj4crve1skpromf37.apps.googleusercontent.com",
                ClientSecret = "HsnwJri_53zn7VcO1Fm7THBb"
            });

            app.UseIdentityServer();

            app.UseStaticFiles();
            app.UseMvcWithDefaultRoute();
        }
    }
}
