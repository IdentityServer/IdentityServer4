using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using WasmAppAuth;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, errors) =>
{
    return errors == SslPolicyErrors.None;
};

builder.Services.AddHttpClient("APIs")
                .AddHttpMessageHandler(sp =>
                {
                    var handler = sp.GetService<AuthorizationMessageHandler>()
                        .ConfigureHandler(
                            authorizedUrls: new[] { "https://localhost:6001" },
                            scopes: new[] { "openid", "profile", "api" });

                    return handler;
                });

builder.Services.AddScoped(sp => sp.GetService<IHttpClientFactory>().CreateClient("APIs"));

builder.Services.AddOidcAuthentication(options =>
{
    // Configure your authentication provider options here.
    // For more information, see https://aka.ms/blazor-standalone-auth
    options.ProviderOptions.Authority = "https://localhost:5001";
    options.ProviderOptions.ClientId = "WasmClient-client";
    options.ProviderOptions.ResponseType = "code";
});

await builder.Build().RunAsync();