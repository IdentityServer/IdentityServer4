Introspection Endpoint
======================

The introspection endpoint is an implementation of `RFC 7662 <https://tools.ietf.org/html/rfc7662>`_.

It can be used to validate reference tokens (or JWTs if the consumer does not have support for appropriate JWT or cryptographic libraries).
The introspection endpoint requires authentication using a scope name and scope secret.

Example
^^^^^^^

::


    POST /connect/introspect
    Authorization: Basic xxxyyy

    token=<token>


A successful response will return a status code of 200 and either an active or inactive token::


    {
        "active": true,
        "sub": "123"
    }


Unknown or expired tokens will be marked as inactive::


    {
        "active": false,
    }


An invalid request will return a 400, an unauthorized request 401.

IdentityModel
^^^^^^^^^^^^^
You can programmatically access the introspection endpoint using the `IdentityModel <https://github.com/IdentityModel/IdentityModel2>`_ library::

    var introspectionClient = new IntrospectionClient(
        doc.IntrospectionEndpoint,
        "api_name",
        "api_secret");

    var response = await introspectionClient.SendAsync(
        new IntrospectionRequest { Token = token });

    var isActive = response.IsActive;
    var claims = response.Claims;
