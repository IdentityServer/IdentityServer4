using System.Security.Claims;

namespace Bornlogic.IdentityServer.Host.Stores.Contracts
{
    public interface IApplicationUserStore
    {
        Task<ApplicationUser> FindBySubjectId(string subjectId);
        Task<bool> ValidateCredentials(string username, string password);
        Task<ApplicationUser> FindByUsername(string username);
        Task<IList<Claim>> GetClaims(ApplicationUser user);
    }
}
