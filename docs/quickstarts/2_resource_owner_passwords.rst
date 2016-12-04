.. _refResosurceOwnerQuickstart:
Protecting an API using Passwords
=================================

The OAuth 2.0 resource owner password grant allows a client to send username and password
to the token service and get an access token back that represents that user.

The spec recommends using the resource owner password grant only for "trusted" (or legacy) applications.
Generally speaking you are typically far better off using one of the interactive
OpenID Connect flows when you want to authenticate a user and request access tokens.

Nevertheless, this grant type allows us to introduce the concept of users to our
quickstart IdentityServer, and that's why we show it.

Adding users
^^^^^^^^^^^^
Just like there are in-memory stores for scopes and clients, there is also one for users.

.. note:: Check the ASP.NET Identity based quickstarts for more information on how to properly store and manage user accounts.

The class ``InMemoryUser`` represents a user and its claims. Let's create a couple of users
by adding the following code to our config class::

    public static List<InMemoryUser> GetUsers()
    {
        return new List<InMemoryUser>
        {
            new InMemoryUser
            {
                Subject = "1",
                Username = "alice",
                Password = "password"
            },
            new InMemoryUser
            {
                Subject = "2",
                Username = "bob",
                Password = "password"
            }
        };
    }

Then register the in-memory users with IdentityServer::

    public void ConfigureServices(IServiceCollection services)
    {
        // configure identity server with in-memory stores, keys, clients and scopes
        services.AddDeveloperIdentityServer()
            .AddInMemoryScopes(Config.GetScopes())
            .AddInMemoryClients(Config.GetClients())
            .AddInMemoryUsers(Config.GetUsers());
    }

The ``AddInMemoryUsers`` extension method does a couple of things under the hood

* makes the list of users available via DI
* adds support for the resource owner password grant
* adds support to user related services typically used by a login UI (we'll use that in the next quickstart)

Adding a client for the resource owner password grant
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
You could simply add support for the grant type to our existing client by changing the
``AllowedGrantTypes`` property. If you need your client to be able to use both grant types
that is absolutely supported.

Typically you want to create a separate client for the resource owner use case, 
add the following to your clients configuration::

    public static IEnumerable<Client> GetClients()
    {
        return new List<Client>
        {
            // other clients omitted...

            // resource owner password grant client
            new Client
            {
                ClientId = "ro.client",
                AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,

                ClientSecrets = 
                {
                    new Secret("secret".Sha256())
                },
                AllowedScopes = { "api1" }
            }
        };
    }

Requesting a token using the password grant
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
The client looks very similar to what we did for the client credentials grant.
The main difference is now that the client would collect the user's password somehow, 
and send it to the token service during the token request.

Again IdentityModel's ``TokenClient`` can help out here::

    // request token
    var tokenClient = new TokenClient(disco.TokenEndpoint, "ro.client", "secret");
    var tokenResponse = await tokenClient.RequestResourceOwnerPasswordAsync("alice", "password", "api1");

    if (tokenResponse.IsError)
    {
        Console.WriteLine(tokenResponse.Error);
        return;
    }

    Console.WriteLine(tokenResponse.Json);
    Console.WriteLine("\n\n");

When you send the token to the identity API endpoint, you will notice one small
but important difference compared to the client credentials grant. The access token will
now contain a ``sub`` claim which uniquely identifies the user.

The presence (or absence) of the ``sub`` claim let's the API distinguish between calls on behalf
of clients and calls on behalf of users.
