Introspection Endpoint
======================

The introspection endpoint is an implementation of `RFC 7662 <https://tools.ietf.org/html/rfc7662>`_.

It can be used to validate reference tokens (or JWTs if the consumer does not have support for appropriate JWT or cryptographic libraries).
The introspection endpoint requires authentication using a scope secret.

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
Our `identitymodel <https://github.com/IdentityModel/IdentityModel>`_ library 
has a helper class called ``IntrospecionClient`` that encapsulates client authentication and introspection requests.