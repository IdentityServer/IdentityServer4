Adding more API Endpoints
=========================
You can add more API endpoints to the application hosting IdentityServer4.

You typically want to protect those APIs by the very instance of IdentityServer they are hosted in. 
That's not a problem. Simply add the token validation handler to the host (see :ref:`here <refProtectingApis>`)::

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddMvc();

        // details omitted
        services.AddIdentityServer();

        services.AddAuthentication()
            .AddIdentityServerAuthentication("token", isAuth =>
            {
                isAuth.Authority = "base_address_of_identityserver";
                isAuth.ApiName = "name_of_api";
            });
    }

On your API, you need to add the ``[Authorize]`` attribute and explicitly reference the authentication scheme you want to use
(this is ``token`` in this example, but you can choose whatever name you like)::

    public class TestController : ControllerBase
    {
        [Route("test")]
        [Authorize(AuthenticationSchemes = "token")]
        public IActionResult Get()
        {
            var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToArray();
            return Ok(new { message = "Hello API", claims });
        }
    }

If you want to call that API from browsers, you additionally need to configure CORS (see :ref:`here <refCors>`).

Discovery
^^^^^^^^^
You can also add your endpoints to the discovery document if you want, e.g like this::

    services.AddIdentityServer(options =>
    {
        options.Discovery.CustomEntries.Add("custom_endpoint", "~/api/custom");
    })