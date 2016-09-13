Discovery Endpoint
==================

The discovery endpoint can be used to retrieve metadata about your IdentityServer - 
it returns information like the issuer name, key material, supported scopes etc.

The discovery endpoint is available via `/.well-known/openid-configuration` relative to the base address, e.g.::

    https://demo.identityserver.io/.well-known/openid-configuration

IdentityModel
^^^^^^^^^^^^^
You can programmatically access the discovery endpoint using the `IdentityModel <https://github.com/IdentityModel/IdentityModel2>`_ library::

    var discoveryClient = new DiscoveryClient("https://demo.identityserver.io");
    var doc = await discoveryClient.GetAsync();

    var tokenEndpoint = doc.TokenEndpoint;
    var keys = doc.KeySet.Keys;