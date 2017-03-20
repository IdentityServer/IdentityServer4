// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityModel;
using IdentityServer4.Events;
using IdentityServer4.Services;
using IdentityServer4.Validation;
using System.Threading.Tasks;

namespace IdentityServer4.Test
{
    public class TestUserResourceOwnerPasswordValidator : IResourceOwnerPasswordValidator
    {
        private readonly TestUserStore _users;
        private readonly IEventService _events;

        public TestUserResourceOwnerPasswordValidator(TestUserStore users, IEventService events)
        {
            _users = users;
            _events = events;
        }

        public async Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            if (_users.ValidateCredentials(context.UserName, context.Password))
            {
                var user = _users.FindByUsername(context.UserName);

                await _events.RaiseAsync(new UserLoginSuccessEvent(context.UserName, user.SubjectId));
                context.Result = new GrantValidationResult(user.SubjectId, OidcConstants.AuthenticationMethods.Password, user.Claims);
            }
            else
            {
                await _events.RaiseAsync(new UserLoginFailureEvent(context.UserName, "invalid credentials"));
            }
        }
    }
}