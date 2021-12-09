using Microsoft.AspNetCore.Identity;

namespace Bornlogic.IdentityServer.Host.Repositories
{
    public interface IUserLoginsRepository
    {
        Task<IEnumerable<UserLoginInfo>> GetByID(string userID);
        Task<IdentityUserLogin<string>> GetByFilters(string userID, string provider, string providerKey);
        Task<IdentityUserLogin<string>> GetByFilters(string provider, string providerKey);
        Task Insert(string userID, UserLoginInfo login);
        Task DeleteByFilters(string userID, string provider, string providerKey);

    }
}
