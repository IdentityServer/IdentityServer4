// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.IO;
using Microsoft.AspNetCore.Hosting;
using System;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Host
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.Title = "IdentityServer4";

            // todo
            //var serilog = new LoggerConfiguration()
            //    .MinimumLevel.Verbose()
            //    .Enrich.FromLogContext()
            //    .WriteTo.File(@"identityserver4_log.txt")
            //    .WriteTo.LiterateConsole(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message}{NewLine}{Exception}{NewLine}");

            // todo
            //loggerFactory
            //    //.WithFilter(new FilterLoggerSettings
            //    //{
            //    //    { "IdentityServer4", LogLevel.Debug },
            //    //    { "Microsoft", LogLevel.Information },
            //    //    { "System", LogLevel.Error }
            //    //})
            //    //.AddSerilog(serilog.CreateLogger());


            var host = new WebHostBuilder()
                //.UseWebListener(options =>
                //{
                //    options.ListenerSettings.Authentication.Schemes = AuthenticationSchemes.Negotiate | AuthenticationSchemes.NTLM;
                //    options.ListenerSettings.Authentication.AllowAnonymous = true;
                //})
                .ConfigureLogging(builder => builder.AddConsole())
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    }
}