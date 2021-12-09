using Bornlogic.IdentityServer.Host.Repositories;
using Bornlogic.IdentityServer.Storage.Models;
using Bornlogic.IdentityServer.Storage.Stores;

namespace Bornlogic.IdentityServer.Host.Stores
{
    public class ResourcesStore : IResourceStore
    {
        private readonly IApiResourceRepository _apiResourceRepository;
        private readonly IApiScopeRepository _apiScopeRepository;
        private readonly IIdentityResourceRepository _identityResourceRepository;
        
        public ResourcesStore(IApiResourceRepository apiResourceRepository, IApiScopeRepository apiScopeRepository, IIdentityResourceRepository identityResourceRepository)
        {
            _apiResourceRepository = apiResourceRepository;
            _apiScopeRepository = apiScopeRepository;
            _identityResourceRepository = identityResourceRepository;
        }
        
        public async Task<Resources> GetAllResourcesAsync()
        {
            var identityResourcesTask = _identityResourceRepository.GetAll();
            var apiResourcesTask = _apiResourceRepository.GetAll();
            var apiScopesTask = _apiScopeRepository.GetAll();

            var identityResources = await identityResourcesTask;
            var apiResources = await apiResourcesTask;
            var apiScopes = await apiScopesTask;

            return new Resources(identityResources, apiResources, apiScopes);
        }
        
        public async Task<IEnumerable<ApiResource>> FindApiResourcesByNameAsync(IEnumerable<string> apiResourceNames)
        {
            if (apiResourceNames == null) throw new ArgumentNullException(nameof(apiResourceNames));

            var all = (await _apiResourceRepository.GetAll()).ToList();

            return all.Where(a => apiResourceNames.Contains(a.Name)).ToList();
        }
        
        public Task<IEnumerable<IdentityResource>> FindIdentityResourcesByScopeNameAsync(IEnumerable<string> scopeNames)
        {
            if (scopeNames == null) throw new ArgumentNullException(nameof(scopeNames));

            return _identityResourceRepository.GetByScopeNames(scopeNames);
        }
        
        public async Task<IEnumerable<ApiResource>> FindApiResourcesByScopeNameAsync(IEnumerable<string> scopeNames)
        {
            if (scopeNames == null) throw new ArgumentNullException(nameof(scopeNames));

            var all = (await _apiResourceRepository.GetAll()).ToList();

            return all.Where(a => a.Scopes != null && scopeNames.Any(s => a.Scopes.Contains(s))).ToList();
        }
        
        public async Task<IEnumerable<ApiScope>> FindApiScopesByNameAsync(IEnumerable<string> scopeNames)
        {
            if (scopeNames == null) throw new ArgumentNullException(nameof(scopeNames));

            var all = (await _apiScopeRepository.GetAll()).ToList();

            return all.Where(a => scopeNames.Contains(a.Name));
        }
    }
}
