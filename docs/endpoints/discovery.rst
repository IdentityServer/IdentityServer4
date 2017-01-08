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

For security reasons DiscoveryClient has a configurable validation policy that checks the following rules by default:

* HTTPS must be used for the discovery endpoint and all protocol endpoints
* The issuer name should match the authority specified when downloading the document (that’s actually a MUST in the discovery spec)
* The protocol endpoints should be “beneath” the authority – and not on a different server or URL (this could be especially interesting for multi-tenant OPs)
* A key set must be specified

If for whatever reason (e.g. dev environments) you need to relax a setting, you can use the following code::

    var client = new DiscoveryClient("http://dev.identityserver.internal");
    client.Policy.RequireHttps = false;
 
    var disco = await client.GetAsync();

Btw – you can always connect over HTTP to localhost and 127.0.0.1 (but this is also configurable).
