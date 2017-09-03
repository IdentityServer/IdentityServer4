// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityModel;
using IdentityServer4.Validation;
using System.Threading.Tasks;
using IdentityServer4.Configuration;
using System;

namespace IdentityServer4.Test
{
    /// <summary>
    /// Resource owner password validator for test users
    /// </summary>
    /// <seealso cref="IdentityServer4.Validation.IResourceOwnerPasswordValidator" />
    public class TestUserResourceOwnerPasswordValidator : IResourceOwnerPasswordValidator
    {
        private readonly IdentityServerOptions _options;
        private readonly TestUserStore _users;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestUserResourceOwnerPasswordValidator"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <param name="users">The users.</param>
        public TestUserResourceOwnerPasswordValidator(IdentityServerOptions options, TestUserStore users)
        {
            _options = options;
            _users = users;
        }

        /// <summary>
        /// Validates the resource owner password credential
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            if (_users.ValidateCredentials(context.UserName, context.Password))
            {
                var user = _users.FindByUsername(context.UserName);
                context.Result = new GrantValidationResult(
                    user.SubjectId ?? throw new ArgumentException("Subject ID not set", nameof(user.SubjectId)), 
                    OidcConstants.AuthenticationMethods.Password, _options.UtcNow, 
                    user.Claims);
            }

            return Task.CompletedTask;
        }
    }
}