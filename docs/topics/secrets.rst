Secrets
=======

In certain situations, clients need to authenticate with identityserver, e.g.

* confidential applications (aka clients) requesting tokens at the token endpoint
* APIs (aka resource scopes) validating reference tokens at the introspection endpoint

For that purpose you can assign a list of secrets to a ``Client`` or a ``Scope``.

Secret parsing and validation is an extensibility point in identityserver, out of the box it supports shared secrets
(stored hashed or plaintext - but defaults to hashed) as well as transmitting the shared secret via a basic authentication header or the POST body.

Creating a shared secret
^^^^^^^^^^^^^^^^^^^^^^^^
The following code sets up a hashed shared secret::

    var secret = new Secret("secret".Sha256());

This secret can now be assigned to either a ``Client`` or a ``Scope``. 
Notice that both do not only support a single secret, but multiple. This is useful for secret rollover and rotation::

    var client = new Client
    {
        ClientId = "client",
        ClientSecrets = new List<Secret> { secret },

        AllowedGrantTypes = GrantTypes.ClientCredentials,
        AllowedScopes = new List<string>
        {
            "api1", "api2"
        }
    };

In fact you can also assign a description and an expiration date to a secret. The description will be used for logging, and 
the expiration date for enforcing a secret lifetime::

    var secret = new Secret(
        "secret".Sha256(), 
        "2016 secret", 
        new DateTime(2016, 12, 31));  

Authentication using a shared secret
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
You can either send the client id/secret combination as part of the POST body::

    POST /connect/token
    
    client_id=client1&
    client_secret=secret&
    ...

..or as a basic authentication header::

    POST /connect/token
    
    Authorization: Basic xxxxx

    ...

You can manually create a basic authentication header using the following C# code::

    var credentials = string.Format("{0}:{1}", clientId, clientSecret);
    var headerValue = Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials));

    var client = new HttpClient();
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", headerValue);

The `IdentityModel <https://github.com/IdentityModel/IdentityModel2>`_ library has helper classes called ``TokenClient`` and ``IntrospectionClient`` that encapsulate
both authentication and protocol messages.
