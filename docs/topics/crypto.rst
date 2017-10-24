.. _refCrypto:
Cryptography, Keys and HTTPS
============================

IdentityServer relies on a couple of crypto mechanisms to do its job.

Token signing and validation
^^^^^^^^^^^^^^^^^^^^^^^^^^^^
IdentityServer needs an asymmetric key pair to sign and validate JWTs. 
This keypair can be a certificate/private key combination or raw RSA keys.
In any case it must support RSA with SHA256.

Loading of signing key and the corresponding validation part is done by implementations of ``ISigningCredentialStore`` and ``IValidationKeysStore``.
If you want to customize the loading of the keys, you can implement those interfaces and register them with DI.

The DI builder extensions has a couple of convenience methods to set signing and validation keys - see :ref:`here <refStartupKeyMaterial>`.

Signing key rollover
^^^^^^^^^^^^^^^^^^^^
While you can only use one signing key at a time, you can publish more than one validation key to the discovery document.
This is useful for key rollover.

A rollover typically works like this:

1. you request/create new key material
2. you publish the new validation key in addition to the current one. You can use the ``AddValidationKeys`` builder extension method for that.
3. all clients and APIs now have a chance to learn about the new key the next time they update their local copy of the discovery document
4. after a certain amount of time (e.g. 24h) all clients and APIs should now accept both the old and the new key material
5. keep the old key material around for as long as you like, maybe you have long-lived tokens that need validation
6. retire the old key material when it is not used anymore
7. all clients and APIs will "forget" the old key next time they update their local copy of the discovery document

This requires that clients and APIs use the discovery document, and also have a feature to periodically refresh their configuration.

Data protection
^^^^^^^^^^^^^^^
Cookie authentication in ASP.NET Core (or anti-forgery in MVC) use the ASP.NET Core data protection feature.
Depending on your deployment scenario, this might require additional configuration. See the Microsoft `docs <https://docs.microsoft.com/en-us/aspnet/core/security/data-protection/configuration/overview>`_ for more information.

HTTPS
^^^^^
We don't enforce the use of HTTPS, but for production it is mandatory for every interaction with IdentityServer.
