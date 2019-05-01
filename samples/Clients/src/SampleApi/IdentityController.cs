using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace SampleApi.Controllers
{
    [Route("identity")]
    [Authorize]
    public class IdentityController : ControllerBase
    {
        [HttpGet]
        public ActionResult Get()
        {
            return new JsonResult(User.Claims.Select(
                c => new { c.Type, c.Value }));
        }
    }
}