Defining Scopes
===============

The first thing you typically define in your system are the resources that you want to protect.
That could be identity information of your users like profile data or email addresses or access to APIs.

.. Note:: At runtime, scopes are retrieved via an implementation of the ``IScopeStore``. This allows loading them from arbitrary data sources like config files or databases. For this document we gonna use the in-memory version of the scope store. You can wire up the in-memory store in ``ConfigureServices`` via the ``AddInMemoryScopes`` extensions method.



Defining the minimal scope for OpenID Connect
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
OpenID Connect requires a scope with a name of `openid`. Since this scope is defined in the OIDC specification, 
we have built-in support for it via the `StandardScopes` class.

Alls our samples define a class called `Scopes` with a method called `Get`. In this method you simply return
a list of scopes you want to support in your identityserver. This list will be later used to configure the 
identityserver service::

    public class Scopes
    {
        public static IEnumerable<Scope> Get()
        {
            return new List<Scope>
            {
                StandardScopes.OpenId
            };
        }
    }


The `StandardScopes` class supports all scopes defined in the specification (openid, email, profile, address and offline_access).
If you want to support them all, you can add them to your list of supported scopes::


    public class Scopes
    {
        public static IEnumerable<Scope> Get()
        {
            return new List<Scope>
            {
                StandardScopes.OpenId,
                StandardScopes.Profile,
                StandardScopes.Email,
                StandardScopes.Address,
                StandardScopes.OfflineAccess
            };
        }
    }
 

Defining custom identity scopes
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
You can also define custom identity scopes. Create a new `Scope` class, give it a name and a display name and define
which user claims should be included in the identity token when this scope gets requested::


    new Scope
    {
        Name = "tenant.info",
        DisplayName = "Tenant Information",
        Type = ScopeType.Identity,

        Claims = new List<ScopeClaim>
        {
            new ScopeClaim("tenantid"),
            new ScopeClaim("subscriptionid")
        }
    }

Add that scope to your list of supported scopes.

Defining scopes for APIs
^^^^^^^^^^^^^^^^^^^^^^^^
To get access tokens for APIs, you also need to register them as a scope. This time the scope type is of type `Resource`::


    new Scope
    {
        Name = "api1",
        DisplayName = "API #1",
        Description = "API 1",
        Type = ScopeType.Resource
    }

If you don't define any scope claims, the access token will contain the subject ID of the user (if present), 
the client ID and the scope name.

You can also add additional user claims to the token by defining scope claims as shown above.