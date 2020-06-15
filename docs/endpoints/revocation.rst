Revocation Endpoint
===================

This endpoint allows revoking access tokens (reference tokens only) and refresh token. 
It implements the token revocation specification `(RFC 7009) <https://tools.ietf.org/html/rfc7009>`_.

``token``
    the token to revoke (required)
``token_type_hint``
    either ``access_token`` or ``refresh_token`` (optional)
    
.. Note:: IdentityServer supports `client authentication <https://openid.net/specs/openid-connect-core-1_0.html#ClientAuthentication>`_ for token request parameters. If using client authentication for token request you must use it here as well to revoke the access token.

Example
^^^^^^^

::

    POST /connect/revocation HTTP/1.1
    Host: server.example.com
    Content-Type: application/x-www-form-urlencoded
    Authorization: Basic czZCaGRSa3F0MzpnWDFmQmF0M2JW

    token=45ghiukldjahdnhzdauz&token_type_hint=refresh_token

.. Note:: You can use the `IdentityModel <https://github.com/IdentityModel/IdentityModel2>`_ client library to programmatically access the revocation endpoint from .NET code. For more information check the IdentityModel `docs <https://identitymodel.readthedocs.io/en/latest/client/revocation.html>`_.
