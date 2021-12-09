using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace Bornlogic.IdentityServer.Host
{
    public class ApplicationUser : IdentityUser
    {
        public bool IsEnabled { get; set; }
        public string EmployeeId { get; set; }
        public IEnumerable<Claim> Claims { get; set; }
    }
}
