Defining Clients
================

Clients represent applications that can request tokens from your identityserver.

The details vary, but you typically define the following common settings for a client:

* a unique client ID
* a secret if needed
* the allowed interactions with the token service (called a grant type)
* a network location where identity and/or access token gets sent to (called a redirect URI)
* a list of scopes (aka resources) the client is allowed to access

.. Note:: At runtime, clients are retrieved via an implementation of the ``IClientStore``. This allows loading them from arbitrary data sources like config files or databases. For this document we will use the in-memory version of the client store. You can wire up the in-memory store in ``ConfigureServices`` via the ``AddInMemoryClients`` extensions method.

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
                    ClientSecrets = { new Secret("secret".Sha256()) },

                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    AllowedScopes = { "api1", "api2.read_only" }
                }
            };
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

        RedirectUris =           { "http://localhost:7017/index.html" },
        PostLogoutRedirectUris = { "http://localhost:7017/index.html" },
        AllowedCorsOrigins =     { "http://localhost:7017" },

        AllowedScopes = 
        {
            IdentityServerConstants.StandardScopes.OpenId,
            IdentityServerConstants.StandardScopes.Profile,
            IdentityServerConstants.StandardScopes.Email,
            
            "api1", "api2.read_only"
        }
    };

.. _startClientsMVC:

Defining a server-side web application (e.g. MVC) for use authentication and delegated API access
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
Interactive server side (or native desktop/mobile) applications use the hybrid flow.
This flow gives you the best security because the access tokens are transmitted via back-channel calls only (and gives you access to refresh tokens)::

    var mvcClient = new Client
    {
        ClientId = "mvc",
        ClientName = "MVC Client",
        ClientUri = "http://identityserver.io",

        AllowedGrantTypes = GrantTypes.Hybrid,
        AllowOfflineAccess = true,
        ClientSecrets = { new Secret("secret".Sha256()) },
        
        RedirectUris =           { "http://localhost:21402/signin-oidc" },
        PostLogoutRedirectUris = { "http://localhost:21402/" },
        FrontChannelLogoutUri =  "http://localhost:21402/signout-oidc",

        AllowedScopes = 
        {
            IdentityServerConstants.StandardScopes.OpenId,
            IdentityServerConstants.StandardScopes.Profile,
            IdentityServerConstants.StandardScopes.Email,

            "api1", "api2.read_only"
        },
    };

Defining clients in appsettings.json
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

The ``AddInMemoryClients`` extensions method also supports adding clients from the ASP.NET Core configuration file. This allows you to define static clients directly from the appsettings.json file::

    "IdentityServer": {
      "IssuerUri": "urn:sso.company.com",
      "Clients": [
        {
          "Enabled": true,
          "ClientId": "local-dev",
          "ClientName": "Local Development",
          "ClientSecrets": [ { "Value": "<Insert Sha256 hash of the secret encoded as Base64 string>" } ],
          "AllowedGrantTypes": [ "implicit" ],
          "AllowedScopes": [ "openid", "profile" ],
          "RedirectUris": [ "https://localhost:5001/signin-oidc" ],
          "RequireConsent": false
        }
      ]
    }
    
Then pass the configuration section to the ``AddInMemoryClients`` method::

    AddInMemoryClients(configuration.GetSection("IdentityServer:Clients"))
