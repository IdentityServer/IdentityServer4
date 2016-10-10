Grant Types
^^^^^^^^^^^

Grant types are a way to specify how a client wants to interact with IdentityServer.
The OpenID Connect and OAuth 2 specs define the following grant types:

* Implicit (OAuth 2 & extended via OpenID Connect)
* Authorization code (OAuth 2 & extended via OpenID Connect)
* Hybrid (OpenID Connect only)
* Client credentials (OAuth 2 only)
* Resource owner password (OAuth 2 only)
* Refresh tokens (OAuth 2 only)
* Extension grants (OAuth 2 only)

You can specify which grant type a client can use via the ``AllowedGrantTypes`` property on the ``Client`` configuration.

A client can be configured to use more than a single grant type (e.g. Hybrid for user centric operations and client credentials for server to server communication).
The ``GrantTypes`` class can be used to pick from typical grant type combinations, or you can specify the list grant types manually.

.. Note:: For security reasons, not all grant type combinations are allowed. See below for more details.

For the remainder, the grant types are briefly described, and when you would used them.
It is also recommended, that in addition you read the corresponding specs to get a better understanding of the differences.

Client credentials
==================
This is the simplest grant type and is used for server to server communication - tokens are always requested on behalf of a client, not a user.

With this grant type you send a token request to the token endpoint, and get an access token back that represents the client.
The client typically has to authenticate with the token endpoint using its client ID and secret.

See the :ref:`Client Credentials Quick Start <refClientCredentialsQuickstart>` for a sample how to use it. 


Resource owner password
=======================
The resource owner password grant type allows to request tokens on behalf of a user by sending the user's name and password to the token endpoint.
This is so called "non-interactive" authentication and is generally not recommended.

There might be reasons for certain legacy or first-party integration scenarios, where this grant type is useful, but the general recommendation
is to use an interactive flow like implicit or hybrid for user authentication instead.

See the :ref:`Client Credentials Quick Start <_refResosurceOwnerQuickstart>` for a sample how to use it.
You also need to provide code for the username/password validation which can be supplied by implementing the ``IResourceOwnerPasswordValidator`` interface.
You can find more information about this interface :ref:`here <refResourceOwnerPasswordValidator>`. 

Implicit
========
The implicit grant type is optimized for browser-based applications. Either for user authentication-only (both server-side and JavaScript applications),
or authentication and access token requests (JavaScript applications).

In the implicit flow, all tokens are transmitted via the browser, and advanced features like refresh tokens are thus not allowed.

:ref:`This <refImplicitQuickstart>` quickstart shows authentication for service-side web apps, and 
:ref:`this <refJavaScriptQuickstart>` shows JavaScript.

Authorization code
==================

Hybrid
======

Refresh tokens
==============

Extension grants
================