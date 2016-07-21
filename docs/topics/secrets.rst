Secrets
=======

In certain situations, clients need to authenticate with identityserver, e.g.

* confidential clients requesting tokens at the token endpoint
* APIs validating reference tokens at the introspection endpoint

For that purpose you can assign a list of secrets to a ``Client`` or a ``Scope``.