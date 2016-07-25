Token Endpoint
==============

The token endpoint can be used to programmatically request tokens.
It supports the ``password``, ``authorization_code``, ``client_credentials`` and ``refresh_token`` grant types).
Furthermore the token endpoint can be extended to support extension grant types.

.. Note:: IdentityServer supports a subset of the OpenID Connect and OAuth 2.0 token request parameters. For a full list, see `here <http://openid.net/specs/openid-connect-core-1_0.html#TokenRequest>`_.

``client_id``
    client identifier (required)
``client_secret``
    client secret either in the post body, or as a basic authentication header. Required for confidential clients.
``grant_type``
    ``authorization_code``, ``client_credentials``, ``password``, ``refresh_token`` or custom
``scope``
    one or more registered scopes (required for all grant types besides ``refresh_token`` and ``authorization_code``)
``redirect_uri`` 
    required for the ``authorization_code`` grant type
``code``
    the authorization code (required for ``authorization_code`` grant type)
``code_verifier``
    PKCE proof key
``username`` 
    resource owner username (required for ``password`` grant type)
``password``
    resource owner password (required for ``password`` grant type)
``acr_values``
   allows passing in additional authentication related information for the ``password`` grant type - identityserver special cases the following proprietary acr_values:
        
        ``idp:name_of_idp`` bypasses the login/home realm screen and forwards the user directly to the selected identity provider (if allowed per client configuration)
        
        ``tenant:name_of_tenant`` can be used to pass a tenant name to the token endpoint
``refresh_token``
    the refresh token (required for ``refresh_token`` grant type)

Example
^^^^^^^

::

    POST /connect/token

        client_id=client1&
        client_secret=secret&
        grant_type=authorization_code&
        code=hdh922&
        redirect_uri=https://myapp.com/callback

(Form-encoding removed and line breaks added for readability)

IdentityModel
^^^^^^^^^^^^^
Our `identitymodel <https://github.com/IdentityModel/IdentityModel>`_ library 
has a helper class called ``TokenClient`` that encapsulates client authentication and token requests.