// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Threading.Tasks;
using IdentityServer4.Configuration;
using IdentityServer4.Hosting;
using Microsoft.AspNetCore.Http;
using IdentityServer4.Validation;
using IdentityServer4.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityServer4.Endpoints.Results
{
    public class ConsentPageResult : IEndpointResult //RedirectToPageWithReturnUrlResult
    {
        private readonly ValidatedAuthorizeRequest _request;

        //public ConsentPageResult(UserInteractionOptions options, string returnUrl)
        //    : base(options.ConsentUrl, options.ConsentReturnUrlParameter, returnUrl)
        //{
        //}

        public ConsentPageResult(ValidatedAuthorizeRequest request)
        {
            _request = request;
        }

        public Task ExecuteAsync(HttpContext context)
        {
            var returnUrl = context.Request.PathBase.ToString().EnsureTrailingSlash() + Constants.ProtocolRoutePaths.AuthorizeAfterConsent;
            returnUrl = returnUrl.AddQueryString(_request.Raw.ToQueryString());

            var options = context.RequestServices.GetRequiredService<IdentityServerOptions>();
            var consentUrl = options.UserInteractionOptions.ConsentUrl;
            if (!consentUrl.IsLocalUrl())
            {
                // this converts the relative redirect path to an absolute one if we're 
                // redirecting to a different server
                returnUrl = context.GetIdentityServerBaseUrl().EnsureTrailingSlash() + returnUrl.RemoveLeadingSlash();
            }

            var url = consentUrl.AddQueryString(options.UserInteractionOptions.ConsentReturnUrlParameter, returnUrl);
            context.Response.RedirectToAbsoluteUrl(url);

            return Task.FromResult(0);
        }
    }
}
