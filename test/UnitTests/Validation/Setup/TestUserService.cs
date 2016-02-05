//// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
//// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

//using IdentityServer4.Core;
//using IdentityServer4.Core.Models;
//using IdentityServer4.Core.Services;
//using System.Threading.Tasks;

//namespace IdentityServer4.Tests.Validation
//{
//    class TestUserService : IUserService
//    {
//        public Task AuthenticateLocalAsync(LocalAuthenticationContext context)
//        {
//            if (context.UserName == context.Password)
//            {
//                var p = IdentityServerPrincipal.Create(context.UserName, context.UserName, "password", "idsvr");
//                context.AuthenticateResult = new AuthenticateResult(p);
//            }
//            else
//            {
//                context.AuthenticateResult = new AuthenticateResult("Username and/or password incorrect");
//            }

//            return Task.FromResult(0);
//        }

//        public Task GetProfileDataAsync(ProfileDataRequestContext context)
//        {
//            return Task.FromResult(0);
//        }

//        public Task AuthenticateExternalAsync(ExternalAuthenticationContext context)
//        {
//            return Task.FromResult(0);
//        }

//        public Task PostAuthenticateAsync(PostAuthenticationContext context)
//        {
//            return Task.FromResult(0);
//        }

//        public Task IsActiveAsync(IsActiveContext context)
//        {
//            context.IsActive = true;
//            return Task.FromResult(0);
//        }

//        public Task PreAuthenticateAsync(PreAuthenticationContext context)
//        {
//            return Task.FromResult(0);
//        }
        
//        public Task SignOutAsync(SignOutContext context)
//        {
//            return Task.FromResult(0);
//        }
//    }
//}