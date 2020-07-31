.. _refAspNetId:
ASP.NET Identity Support
========================

An ASP.NET Identity-based implementation is provided for managing the identity database for users of IdentityServer.
This implementation implements the extensibility points in IdentityServer needed to load identity data for your users to emit claims into tokens.

The repo for this support is located `here <https://github.com/IdentityServer/IdentityServer4.AspNetIdentity/>`_ and the NuGet package is `here <https://www.nuget.org/packages/IdentityServer4.AspNetIdentity>`_.

To use this library, configure ASP.NET Identity normally. 
Then use the ``AddAspNetIdentity`` extension method after the call to ``AddIdentityServer``::

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services.AddIdentityServer()
            .AddAspNetIdentity<ApplicationUser>();
    }

``AddAspNetIdentity`` requires as a generic parameter the class that models your user for ASP.NET Identity (and the same one passed to ``AddIdentity`` to configure ASP.NET Identity).
This configures IdentityServer to use the ASP.NET Identity implementations of ``IUserClaimsPrincipalFactory``, ``IResourceOwnerPasswordValidator``, and ``IProfileService``.
It also configures some of ASP.NET Identity's options for use with IdentityServer (such as claim types to use and authentication cookie settings).

When using your own implementation of ``IUserClaimsPrincipalFactory``, make sure that you register it before calling the IdentityServer ``AddAspNetIdentity`` extension method.


Including Roles (or other custom claims) in access_token
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

A common request is to include ASP.Net Core Identity 'roles' in the access token issued by IdentityServer.

In principal this is fine but it is worth mentioning that there could be security problems if you aren't careful.

If you have a user in an admin role but you wish to urgently remove them from that role, how would you force the user to 
abandon their current access_token and receive a new one with fewer role claims?

It is OK to include roles in an access_token but it is not OK to treat the presence of that claim being in the access_token at the API level 
as proof that a user has authorization to permit an action.

For further discussion on the topic you could watch `this talk <https://www.youtube.com/watch?v=EJeZ3YNnqz8>`_ by Dominick Baier & Brock Allen.

With that out of the way, let's discuss extending the ProfileService provided by IdentityServer4.AspNetIdentity to include additional claims.

IdentityServer4.AspNetIdentity provides an implementation of IProfileService which handles a lot of the basics, e.g. loading strongly typed IdentityUser entity and adding subject claims to token.

Following on from the AspNetIdentity Quickstart we can add the following class::

    // FooProfileService.cs    
    namespace IdentityServerAspNetIdentity
    {
        public class FooProfileService : IdentityServer4.AspNetIdentity.ProfileService<ApplicationUser>
        {
            public FooProfileService(
                UserManager<ApplicationUser> userManager,
                IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory)
                : base(userManager, claimsFactory)
            { }

            public FooProfileService(
                UserManager<ApplicationUser> userManager,
                IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory,
                ILogger<ProfileService<ApplicationUser>> logger)
                : base(userManager, claimsFactory, logger)
            { }
            
            protected override async Task GetProfileDataAsync(
                ProfileDataRequestContext context, 
                ApplicationUser user)
            {
                await base.GetProfileDataAsync(context, user);

                if (context.RequestedClaimTypes.Any(rct => rct.Equals("foo:roles")))
                {
                    var roles = await UserManager.GetRolesAsync(user);
                    var claims = roles.Select(r => new Claims.Claim("foo:roles", r));

                    context.IssuedClaims.AddRange(claims);
                }
            }
        }
    }

We need to request the claims from the ProfileService for a given api resource::

    // Config.cs
    public static IEnumerable<ApiScope> ApiScopes =>
        new List<ApiScope>
        {
            new ApiScope("foo", "Foo API")
            {
                UserClaims = { "foo:roles" }
            }
        };

And we need to ensure our custom profile service is used::

    // Startup.cs
    services.AddIdentityServer()
        ...
        .AddProfileService<FooProfileService>()

If you wish these claims to show in the userinfo output the UserClaims should be added to an IdentityResource e.g. profile::

    // Config.cs
    public static IEnumerable<IdentityResource> IdentityResources =>
        new List<IdentityResource>
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile
            {
                UserClaims = { "foo:roles" }
            }
        };