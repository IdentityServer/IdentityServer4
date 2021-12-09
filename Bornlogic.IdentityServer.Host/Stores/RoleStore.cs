using Microsoft.AspNetCore.Identity;

namespace Bornlogic.IdentityServer.Host.Stores
{
    public class RoleStore<T> : IRoleStore<T> where T : class
    {
        public void Dispose()
        {
        }

        public Task<IdentityResult> CreateAsync(T role, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityResult> DeleteAsync(T role, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<T> FindByIdAsync(string roleId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<T> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetNormalizedRoleNameAsync(T role, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetRoleIdAsync(T role, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetRoleNameAsync(T role, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetNormalizedRoleNameAsync(T role, string normalizedName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task SetRoleNameAsync(T role, string roleName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IdentityResult> UpdateAsync(T role, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
