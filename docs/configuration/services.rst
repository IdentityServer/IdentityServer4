Registering Services
====================

IdentityServer4 uses the ASP.NET Core DI system, and you need to register our services before you can use IdentityServer.

We provide a couple of extension methods on the `IServiceCollection` to automate adding our services. Typically you need to specify the following details explicitly:

* how to get to your user details (the profile service)
* how to load your scope definitions (the scope store)
* how to load your client definitions (the client store)
* how to get to your key material (key stores)
* how to persist state information (the grant store)

We provide in-memory and EntityFramework based services for all of the above.

AddIdentityServer
^^^^^^^^^^^^^^^^^
This method adds all standard services to the DI container. It does not specify the stores for key material and state persistence.

AddDeveloperIdentityServer
^^^^^^^^^^^^^^^^^^^^^^^^^^
This convenience method adds all default IdentityServer services and additionally configures in-memory keys and state persistence. 
As the name implies, this is useful for testing and development - not production.

