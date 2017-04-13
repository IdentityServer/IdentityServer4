// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityModel;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer4.Validation
{
    /// <summary>
    /// Default userinfo request validator
    /// </summary>
    /// <seealso cref="IdentityServer4.Validation.IUserInfoRequestValidator" />
    public class UserInfoRequestValidator : IUserInfoRequestValidator
    {
        private readonly ITokenValidator _tokenValidator;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserInfoRequestValidator"/> class.
        /// </summary>
        /// <param name="tokenValidator">The token validator.</param>
        public UserInfoRequestValidator(ITokenValidator tokenValidator)
        {
            _tokenValidator = tokenValidator;
        }

        /// <summary>
        /// Validates a userinfo request.
        /// </summary>
        /// <param name="accessToken">The access token.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public async Task<UserInfoRequestValidationResult> ValidateRequestAsync(string accessToken)
        {
            // the access token needs to be valid and have at least the openid scope
            var tokenResult = await _tokenValidator.ValidateAccessTokenAsync(
                accessToken,
                IdentityServerConstants.StandardScopes.OpenId);

            if (tokenResult.IsError)
            {
                //_logger.LogError(tokenResult.Error);

                return new UserInfoRequestValidationResult
                {
                    IsError = true,
                    Error = OidcConstants.ProtectedResourceErrors.InsufficientScope
                };
            }

            // the token must have a one sub claim
            var subClaim = tokenResult.Claims.SingleOrDefault(c => c.Type == JwtClaimTypes.Subject);
            if (subClaim == null)
            {
                //var error = "Token contains no sub claim";
                //_logger.LogError(error);

                return new UserInfoRequestValidationResult
                {
                    IsError = true,
                    Error = OidcConstants.ProtectedResourceErrors.InvalidToken
                };
            }

            // create subject from incoming access token
            var claims = tokenResult.Claims.Where(x => !Constants.Filters.ProtocolClaimsFilter.Contains(x.Type));
            var subject = Principal.Create("UserInfo", claims.ToArray());

            return new UserInfoRequestValidationResult
            {
                IsError = false,
                TokenValidationResult = tokenResult,
                Subject = subject
            };
        }
    }
}