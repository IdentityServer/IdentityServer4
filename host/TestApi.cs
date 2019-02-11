using IdentityServer4;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Host
{
    [Route("test")]
    [Authorize(IdentityServerConstants.LocalApiPolicy)]
    public class TestController
    {
        public string Get()
        {
            return "ok";
        }
    }
}
