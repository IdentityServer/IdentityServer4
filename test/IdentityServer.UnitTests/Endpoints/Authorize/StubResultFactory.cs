// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Threading.Tasks;
using IdentityServer4.Validation;
using IdentityServer4.Hosting;
using IdentityServer4.Endpoints.Results;
using IdentityServer4.Configuration;

namespace UnitTests.Endpoints.Authorize
{
    class StubResultFactory : IAuthorizeEndpointResultFactory
    {
        public IEndpointResult AuthorizeResult { get; set; } = new AuthorizeRedirectResult(null);
        public IEndpointResult ConsentResult { get; set; } = new ConsentPageResult(new UserInteractionOptions(), "consent_id");
        public IEndpointResult LoginResult { get; set; } = new LoginPageResult(new UserInteractionOptions(), "login_id");
        public IEndpointResult ErrorResult { get; set; } = new ErrorPageResult(new UserInteractionOptions(), "error_id");

        public Task<IEndpointResult> CreateAuthorizeResultAsync(ValidatedAuthorizeRequest request)
        {
            return Task.FromResult(AuthorizeResult);
        }

        public Task<IEndpointResult> CreateConsentResultAsync(ValidatedAuthorizeRequest request)
        {
            return Task.FromResult(ConsentResult);
        }

        public Task<IEndpointResult> CreateErrorResultAsync(ErrorTypes errorType, string error, ValidatedAuthorizeRequest request)
        {
            return Task.FromResult(ErrorResult);
        }

        public Task<IEndpointResult> CreateLoginResultAsync(ValidatedAuthorizeRequest request)
        {
            return Task.FromResult(LoginResult);
        }
    }
}