Defining Clients
================

Clients represent applications that can request tokens from your identityserver.

The details vary, but you typically define the following common settings for a client:

* a unique client ID
* a secret if needed
* the allowed interactions with the token service (called a grant type)
* a network location where identity and/or access token gets sent to (called a redirect URI)
* a list of scopes (aka resources) the client is allowed to access

.. Note:: At runtime, clients are retrieved via an implementation of the ``IClientStore``. This allows loading them from arbitrary data sources like config files or databases. For this document we gonna use the in-memory version of the client store. You can wire up the in-memory store in ``ConfigureServices`` via the ``AddInMemoryClients`` extensions method.

Defining a client for server to server communication
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
In this scenario no interactive user is present - a service (aka client) wants to communicate with an API (aka scope)::

    public class Clients
    {
        public static IEnumerable<Client> Get()
        {
            return new List<Client>
            {
                new Client
                {
                    ClientId = "service.client",
                    ClientSecrets = new List<Secret>
                    {
                        new Secret("secret".Sha256())
                    },

                    AllowedGrantTypes = GrantTypes.ClientCredentials,

                    AllowedScopes = new List<string>
                    {
                        "api1", "api2"
                    }
                };
            }
        }
    }

Defining browser-based JavaScript client (e.g. SPA) for user authentication and delegated access and API
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
This client uses the so called implicit flow to request an identity and access token from JavaScript::

    var jsClient = new Client
    {
        ClientId = "js",
        ClientName = "JavaScript Client",
        ClientUri = "http://identityserver.io",

        AllowedGrantTypes = GrantTypes.Implicit,
        AllowAccessTokensViaBrowser = true,

        RedirectUris = new List<string>
        {
            "http://localhost:7017/index.html",
        },
        PostLogoutRedirectUris = new List<string>
        {
            "http://localhost:7017/index.html",
        },
        AllowedCorsOrigins = new List<string>
        {
            "http://localhost:7017"
        },

        AllowedScopes = new List<string>
        {
            StandardScopes.OpenId.Name,
            StandardScopes.Profile.Name,
            StandardScopes.Email.Name,
            "api1", "api2"
        },
    };

.. _start_clients_mvc:
Defining a server-side web application (e.g. MVC) for use authentication and delegated API access
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
Interactive server side (or native desktop/mobile) applications use the hybrid flow.
This flow gives you the best security because the access tokens are transmitted via back-channel calls only (and gives you access to refresh tokens)::

    var mvcClient = new Client
    {
        ClientId = "mvc",
        ClientName = "MVC Client",
        ClientSecrets = new List<Secret>
        {
            new Secret("secret".Sha256())
        },
        ClientUri = "http://identityserver.io",

        AllowedGrantTypes = GrantTypes.Hybrid,
        
        RedirectUris = new List<string>
        {
            "http://localhost:21402/signin-oidc"
        },
        PostLogoutRedirectUris = new List<string>
        {
            "http://localhost:21402/"
        },
        LogoutUri = "http://localhost:21402/signout-oidc",

        AllowedScopes = new List<string>
        {
            StandardScopes.OpenId.Name,
            StandardScopes.Profile.Name,
            StandardScopes.OfflineAccess.Name,

            "api1", "api2",
        },
    };