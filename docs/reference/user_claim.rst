.. _refUserClaim:
User Claim
==========
A user claim models a claim that should be included either in an identity or access token.

``Type``
    Specifies the claim type. This type will be passed to the profile services as part of the ``RequestedClaimTypes`` parameter.
``AlwaysIncludeInIdToken``
    As an optimization mechanism, the OpenID Connect spec suggests, that if an access token is requested, the identity claims are not included inside the identity token. The client can use the userinfo endpoint instead to get to the claim values. This flag allows to change the default behavior.