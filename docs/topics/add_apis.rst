Adding more API Endpoints
=========================
It's a common scenario to add additional API endpoints to the application hosting IdentityServer.
These endpoints are typically protected by IdentityServer itself.

For simple scenarios, we give you some helpers. See the advanced section to understand more of the internal plumbing.

Start by registering your API as an ``ApiResource``, e.g.::

    public static IEnumerable<ApiResource> Apis = new List<ApiResource>
    {
        // local API
        new ApiResource(IdentityServerConstants.LocalApi.ScopeName),
    };

..and give your clients access to this API, e.g.::

    new Client
    {
        // rest omitted
        AllowedScopes = { IdentityServerConstants.LocalApi.ScopeName },   
    }

.. note:: The value of ``IdentityServerConstants.LocalApi.ScopeName`` is ``IdentityServerApi``.

To enable token validation for local APIs, add the following to your IdentityServer startup::

    services.AddLocalApiAuthentication();

To protect an API controller, decorate it with an ``Authorize`` attribute using the ``LocalApi.PolicyName`` policy::

    [Route("localApi")]
    [Authorize(LocalApi.PolicyName)]
    public class LocalApiController : ControllerBase
    {
        public IActionResult Get()
        {
            // omitted
        }
    }

Discovery
^^^^^^^^^
You can also add your endpoints to the discovery document if you want, e.g like this::

    services.AddIdentityServer(options =>
    {
        options.Discovery.CustomEntries.Add("local_api", "~/localapi");
    })