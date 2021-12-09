using System.Security.Claims;
using Bornlogic.IdentityServer.Host.Repositories;
using Bornlogic.IdentityServer.Host.Stores.Contracts;
using Microsoft.AspNetCore.Identity;

namespace Bornlogic.IdentityServer.Host.Stores
{
    public class UserStore<T> : UserStoreBase<T, string, IdentityUserClaim<string>, IdentityUserLogin<string>, IdentityUserToken<string>>, IApplicationUserStore where T : IdentityUser<string>
    {
        private static IdentityResult USER_IS_NOT_APPLICATION_USER_IDENTITY_RESULT_ERROR = IdentityResult.Failed(new IdentityError { Description = "Specified user it not an Application User" });
        private static IdentityResult MISSING_USER_ID_IDENTITY_RESULT_ERROR = IdentityResult.Failed(new IdentityError { Description = "Missing User ID" });

        private readonly IServiceProvider _serviceProvider;
        private readonly IUserRepository _userStoreRepository;
        private readonly IUserTokensRepository _userTokensRepository;
        private readonly IUserLoginsRepository _userLoginsRepository;

        public UserStore
            (
                IServiceProvider serviceProvider, 
                IdentityErrorDescriber describer,
                IUserRepository userStoreRepository,
                IUserTokensRepository userTokensRepository,
                IUserLoginsRepository userLoginsRepository
            ) : base(describer)
        {
            _serviceProvider = serviceProvider;
            _userStoreRepository = userStoreRepository;
            _userTokensRepository = userTokensRepository;
            _userLoginsRepository = userLoginsRepository;
        }

        public override async Task<IdentityResult> CreateAsync(T user, CancellationToken cancellationToken = new CancellationToken())
        {
            if (user is not ApplicationUser applicationUser)
                return USER_IS_NOT_APPLICATION_USER_IDENTITY_RESULT_ERROR;

            try
            {
                await _userStoreRepository.Insert(applicationUser);
            
                return IdentityResult.Success;
            }
            catch (Exception e)
            {
                return IdentityResult.Failed(new IdentityError { Description = e.Message });
            }
        }

        public override async Task<IdentityResult> UpdateAsync(T user, CancellationToken cancellationToken = new CancellationToken())
        {
            if (user is not ApplicationUser applicationUser)
                return USER_IS_NOT_APPLICATION_USER_IDENTITY_RESULT_ERROR;

            try
            {
                await _userStoreRepository.Update(applicationUser);

                return IdentityResult.Success;
            }
            catch (Exception e)
            {
                return IdentityResult.Failed(new IdentityError { Description = e.Message });
            }
        }

        public override async Task<IdentityResult> DeleteAsync(T user, CancellationToken cancellationToken = new CancellationToken())
        {
            if (user is not ApplicationUser applicationUser)
                return USER_IS_NOT_APPLICATION_USER_IDENTITY_RESULT_ERROR;

            if (string.IsNullOrEmpty(applicationUser.Id))
                return MISSING_USER_ID_IDENTITY_RESULT_ERROR;

            try
            {
                await _userStoreRepository.DeleteByID(applicationUser.Id);

                return IdentityResult.Success;
            }
            catch (Exception e)
            {
                return IdentityResult.Failed(new IdentityError { Description = e.Message });
            }
        }

        public override async Task<T> FindByIdAsync(string userId, CancellationToken cancellationToken = new CancellationToken())
        {
            var applicationUser = await _userStoreRepository.GetByID(userId);
            
            return applicationUser is not T user ? null : user;
        }

        public override async Task<T> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken = new CancellationToken())
        {
            var applicationUser = await _userStoreRepository.GetByNormalizedUserName(normalizedUserName);

            return applicationUser is not T user ? null : user;
        }

        protected override async Task<T> FindUserAsync(string userId, CancellationToken cancellationToken)
        {
            var applicationUser = await _userStoreRepository.GetByID(userId);

            return applicationUser is not T user ? null : user;
        }

        protected override Task<IdentityUserLogin<string>> FindUserLoginAsync(string userId, string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            return _userLoginsRepository.GetByFilters(userId, loginProvider, providerKey);
        }

        protected override Task<IdentityUserLogin<string>> FindUserLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            return _userLoginsRepository.GetByFilters(loginProvider, providerKey);
        }

        public override Task AddLoginAsync(T user, UserLoginInfo login, CancellationToken cancellationToken = new CancellationToken())
        {
            if (user is not ApplicationUser applicationUser)
                return Task.CompletedTask;

            return _userLoginsRepository.Insert(applicationUser.Id, login);
        }

        public override Task RemoveLoginAsync(T user, string loginProvider, string providerKey, CancellationToken cancellationToken = new CancellationToken())
        {
            if (user is not ApplicationUser applicationUser)
                return Task.CompletedTask;

            return _userLoginsRepository.DeleteByFilters(applicationUser.Id, loginProvider, providerKey);
        }

        public override async Task<IList<UserLoginInfo>> GetLoginsAsync(T user, CancellationToken cancellationToken = new CancellationToken())
        {
            if (user is not ApplicationUser applicationUser)
                return new List<UserLoginInfo>();

            var logins = await _userLoginsRepository.GetByID(applicationUser.Id);

            return logins?.ToList() ?? new List<UserLoginInfo>();
        }

        public override async Task<IList<Claim>> GetClaimsAsync(T user, CancellationToken cancellationToken = new CancellationToken())
        {
            if (user is not ApplicationUser applicationUser)
                return new List<Claim>();

            return applicationUser.Claims?.ToList() ?? new List<Claim>();
        }

        public override Task AddClaimsAsync(T user, IEnumerable<Claim> claims, CancellationToken cancellationToken = new CancellationToken())
        {
            if (user is not ApplicationUser applicationUser)
                return Task.CompletedTask;

            return _userStoreRepository.UpdateClaims(applicationUser.Id, claims);
        }

        public override Task ReplaceClaimAsync(T user, Claim claim, Claim newClaim, CancellationToken cancellationToken = new CancellationToken())
        {
            if (user is not ApplicationUser applicationUser)
                return Task.CompletedTask;

            var updatedClaims = applicationUser.Claims.Where(c => c.Type != claim.Type || c.Value != claim.Value).ToList();

            updatedClaims.Add(newClaim);

            return _userStoreRepository.UpdateClaims(applicationUser.Id, updatedClaims);
        }

        public override Task RemoveClaimsAsync(T user, IEnumerable<Claim> claims, CancellationToken cancellationToken = new CancellationToken())
        {
            if (user is not ApplicationUser applicationUser)
                return Task.CompletedTask;

            var updatedClaims = new List<Claim>();

            foreach (var applicationUserClaim in applicationUser.Claims)
            {
                if(claims.All(c => c.Type != applicationUserClaim.Type || c.Value != applicationUserClaim.Value))
                    updatedClaims.Add(applicationUserClaim);
            }

            return _userStoreRepository.UpdateClaims(applicationUser.Id, updatedClaims);
        }

        public override async Task<IList<T>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken = new CancellationToken())
        {
            var users = await _userStoreRepository.GetByClaim(claim);

            return users.Select(a => a as T).ToList();
        }

        protected override async Task<IdentityUserToken<string>> FindTokenAsync(T user, string loginProvider, string name, CancellationToken cancellationToken)
        {
            if (user is not ApplicationUser applicationUser)
                return default;

            if (string.IsNullOrEmpty(applicationUser.Id))
                return default;

            var token = await _userTokensRepository.GetByFilters(applicationUser.Id, name, loginProvider);

            return token;
        }

        protected override Task AddUserTokenAsync(IdentityUserToken<string> token)
        {
            return _userTokensRepository.Insert(token);
        }

        protected override Task RemoveUserTokenAsync(IdentityUserToken<string> token)
        {
            return _userTokensRepository.DeleteByFilters(token.UserId, token.Name, token.LoginProvider);
        }

        public override IQueryable<T> Users { get; }

        public override async Task<T> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = new CancellationToken())
        {
            var applicationUser = await _userStoreRepository.GetByNormalizedUserEmail(normalizedEmail);

            return applicationUser is not T user ? null : user;
        }

        public async Task<ApplicationUser> FindBySubjectId(string subjectId)
        {
            var user = FindByIdAsync(subjectId);

            return user as ApplicationUser;
        }

        public async Task<bool> ValidateCredentials(string username, string password)
        {
            var user = await FindByUsername(username);

            if (user != null)
            {
                var passwordHasher = _serviceProvider.GetService<IPasswordHasher<ApplicationUser>>();

                var hashedPassword = passwordHasher.HashPassword(user, password);

                if (string.IsNullOrWhiteSpace(user.PasswordHash) && string.IsNullOrWhiteSpace(hashedPassword))
                {
                    return true;
                }

                return user.PasswordHash.Equals(hashedPassword);
            }

            return false;
        }

        public async Task<ApplicationUser> FindByUsername(string username)
        {
            var applicationUser = await _userStoreRepository.GetByUserName(username);

            return applicationUser;
        }

        public Task<IList<Claim>> GetClaims(ApplicationUser user)
        {
            return GetClaimsAsync(user as T);
        }
    }
}
