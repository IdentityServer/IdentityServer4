Windows Authentication
======================

On supported platforms, you can use IdentityServer to authenticate users using Windows authentication (e.g. against Active Directory).
Currently Windows authentication is available when you host IdentityServer using:

* Kestrel on Windows using IIS and the IIS integration package

* WebListener on Windows

In both cases, Windows authentication is treated as external authentication that has to be invoked using an ASP.NET authentication manager challenge command.
The account controller in our `quickstart UI <https://github.com/IdentityServer/IdentityServer4.Quickstart.UI>`_ implements the necessary logic.

Using WebListener
^^^^^^^^^^^^^^^^^
When using WebListener you need to enable Windows authentication when setting up the host, e.g.::

    var host = new WebHostBuilder()
        .UseWebListener(options =>
        {
            options.ListenerSettings.Authentication.Schemes = AuthenticationSchemes.Negotiate | AuthenticationSchemes.NTLM;
            options.ListenerSettings.Authentication.AllowAnonymous = true;
        })
        .UseUrls("https://myserver:443")
        .UseContentRoot(Directory.GetCurrentDirectory())
        .UseStartup<Startup>()
        .Build();

The WebListener plumbing will insert Windows authentication middleware for each authentication scheme you selected.
You can enumerate the schemes by using the ASP.NET Core authentication manager ``GetAvailableSchemes`` method, and invoke it using the ``ChallengeAsync`` method.

Using Kestrel
^^^^^^^^^^^^^
When using Kestrel, you must run "behind" IIS and use the IIS integration::

    var host = new WebHostBuilder()
        .UseKestrel()
        .UseUrls("http://localhost:5000")
        .UseContentRoot(Directory.GetCurrentDirectory())
        .UseIISIntegration()
        .UseStartup<Startup>()
        .Build();

Also the virtual directory in IIS (or IIS Express) must have Windows and anonymous authentication enabled.

Just as WebListener, the IIS integration will insert a Windows authentication middleware into the HTTP pipeline that can be invoked via the authentication manager.
