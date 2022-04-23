// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using Microsoft.AspNetCore.Builder;
using IdentityServer;
using IdentityServerHost.Quickstart.UI;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins",
        builder => builder.WithOrigins(
            "https://localhost:5015", 
            "http://localhost:5016", 
            "https://localhost:44360",
            "https://localhost:44350",
            "https://localhost:5003",
            "https://localhost:44300",
            "https://localhost:6001"
            )
        .AllowAnyMethod()
        .AllowAnyHeader());
});

#region idp-db
// uncomment, if you want to add an MVC-based UI

builder.Services.AddIdentityServer(options =>
{
    // see https://identityserver4.readthedocs.io/en/latest/topics/resources.html
    options.EmitStaticAudienceClaim = true;
})
    .AddInMemoryIdentityResources(Config.IdentityResources)
    .AddInMemoryApiScopes(Config.ApiScopes)
    .AddInMemoryApiResources(Config.ApiResources)
    .AddInMemoryClients(Config.Clients)
    .AddTestUsers(TestUsers.Users)
    // not recommended for production - you need to store your key material somewhere secure
    .AddDeveloperSigningCredential();
#endregion

builder.Services.AddControllersWithViews();

#region Log
Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Information)
                .Enrich.FromLogContext()
                // uncomment to write to Azure diagnostics stream
                //.WriteTo.File(
                //    @"D:\home\LogFiles\Application\identityserver.txt",
                //    fileSizeLimitBytes: 1_000_000,
                //    rollOnFileSizeLimit: true,
                //    shared: true,
                //    flushToDiskInterval: TimeSpan.FromSeconds(1))
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}", theme: AnsiConsoleTheme.Code)
                .CreateLogger();
Log.Information("Starting host...");
builder.Logging.AddSerilog(Log.Logger);
#endregion

var app = builder.Build();
app.UseCors("AllowSpecificOrigins");

#region app
//if (Environment.IsDevelopment())
//{
app.UseDeveloperExceptionPage();
//}

// uncomment if you want to add MVC
app.UseStaticFiles();
app.UseRouting();

app.UseIdentityServer();

//app.UseSerilogRequestLogging();

// uncomment, if you want to add MVC
app.UseAuthorization();
app.UseEndpoints(endpoints =>
{
    endpoints.MapDefaultControllerRoute();
});

app.Run();
#endregion