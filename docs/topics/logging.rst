
Logging
=======
IdentityServer uses the standard logging facilities provided by ASP.NET Core.
The Microsoft `documentation <https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging>`_ has a good intro and a description of the built-in logging providers.

We are roughly following the Microsoft guidelines for usage of log levels:

* ``Trace`` For information that is valuable only to a developer troubleshooting an issue. These messages may contain sensitive application data like tokens and should not be enabled in a production environment.
* ``Debug`` For following the internal flow and understanding why certain decisions are made. Has short-term usefulness during development and debugging.
* ``Information`` For tracking the general flow of the application. These logs typically have some long-term value.
* ``Warning`` For abnormal or unexpected events in the application flow. These may include errors or other conditions that do not cause the application to stop, but which may need to be investigated.
* ``Error`` For errors and exceptions that cannot be handled. Examples: failed validation of a protocol request.
* ``Critical`` For failures that require immediate attention. Examples: missing store implementation, invalid key material...

Setup
^^^^^
We personally like `Serilog <https://serilog.net/>`_ a lot. Give it a try.

Install Serilog packages: 
From Package Manager Console verify that Default Project drop-down has your project selected and run

```
install-package Serilog.Extensions.Logging
install-package Serilog.Sinks.File
```

You want to setup logging as early as possible in your application host, e.g. in the constructor of your startup class, e.g::

    public class Startup
    {
        public Startup(ILoggerFactory loggerFactory, IHostingEnvironment environment)
        {
            var serilog = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.FromLogContext()
                .WriteTo.File(@"identityserver4_log.txt");
                
            if (environment.IsDevelopment())
            {
                serilog.WriteTo.LiterateConsole(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message}{NewLine}{Exception}{NewLine}");
            }

            loggerFactory
                .WithFilter(new FilterLoggerSettings
                {
                    { "IdentityServer", LogLevel.Debug },
                    { "Microsoft", LogLevel.Information },
                    { "System", LogLevel.Error },
                })
                .AddSerilog(serilog.CreateLogger());
        }
    }

Further reading
^^^^^^^^^^^^^^^
* `ASP.NET Core Logging with Azure App Service and Serilog <https://blogs.msdn.microsoft.com/webdev/2017/04/26/asp-net-core-logging/>`_
