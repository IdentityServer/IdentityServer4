Cryptography, Keys and HTTPS
============================

IdentityServer relies on a couple of crypto mechanisms to do its job.

Token signing and validation
^^^^^^^^^^^^^^^^^^^^^^^^^^^^
IdentityServer needs an asymmetric key pair to sign and validate JWTs. 
This keypair can can be a certificate/private key combination or raw RSA keys.
In any case it must support RSA with SHA256.

Loading of signing key and the corresponding validation part is done by implementations of ``ISigningCredentialStore`` and ``IValidationKeysStore``.
If you want to customize the loading of the keys, you can implement those interfaces and register them with DI.

The DI builder extensions has a couple of convenience methods to set signing and validation keys.  

``SetSigningCredential`` allows setting either an RSA key or a certificate from the store or an instance of ``X509Certificate2``.

``SetTemporarySigningCredential`` creates a fresh RSA key pair on every startup. This is useful for development situation where
you don't have access to key material but want to get started.

Signing key rollover
^^^^^^^^^^^^^^^^^^^^

Data protection
^^^^^^^^^^^^^^^
We use the ASP.NET Core data protection API. For the most parts this requires no manual configuration - some adjustments might be needed
depending on your deployment scenario (e.g. self-hosted web farms). 
See `here <https://docs.asp.net/en/latest/security/data-protection/index.html>`_ for more information.

HTTPS
^^^^^
We don't enforce the use of HTTPS, but for production it is mandatory for every interaction with IdentityServer.

HTTPS is typically provided by the reverse proxy that sits in front of ASP.NET Core's built-in webser,
`here <https://docs.asp.net/en/latest/publishing/iis.html>`_ are some instructions for using IIS.