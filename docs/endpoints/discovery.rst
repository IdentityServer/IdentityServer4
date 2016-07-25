Discovery Endpoint
==================

The discovery endpoint can be used to retrieve metadata about your identityserver - 
it returns information like the issuer name, key material, supported scopes etc.

The discovery endpoint is available via `/.well-known/openid-configuration` relative to the base address, e.g.::

    https://demo.identityserver.io/.well-known/openid-configuration