using System.Security.Claims;

namespace Bornlogic.IdentityServer.Host.Repositories
{
    public interface IUserRepository
    {
        Task Insert(ApplicationUser user);
        Task Update(ApplicationUser user);
        Task UpdateClaims(string userID, IEnumerable<Claim> claims);
        Task DeleteByID(string id);
        Task<ApplicationUser> GetByID(string id);
        Task<IEnumerable<ApplicationUser>> GetByClaim(Claim claim);
        Task<ApplicationUser> GetByUserName(string userName);
        Task<ApplicationUser> GetByUserEmail(string email);
        Task<ApplicationUser> GetByNormalizedUserName(string normalizedUserName);
        Task<ApplicationUser> GetByNormalizedUserEmail(string normalizedUserEmail);
    }
}
