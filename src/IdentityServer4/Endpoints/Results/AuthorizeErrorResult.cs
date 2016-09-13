// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Extensions;
using IdentityServer4.Hosting;
using IdentityModel;
using Microsoft.AspNetCore.Http;
using IdentityServer4.Validation;
using IdentityServer4.ResponseHandling;
using Microsoft.Extensions.DependencyInjection;
using System;
using IdentityServer4.Services;
using IdentityServer4.Stores;

namespace IdentityServer4.Endpoints.Results
{
    class AuthorizeErrorResult : IEndpointResult
    {
        private readonly string _error;
        private readonly string _errorDescription;
        private readonly ValidatedAuthorizeRequest _request;

        public AuthorizeErrorResult(ValidatedAuthorizeRequest request, string error, string errorDescription = null)
        {
            _request = request;
            _error = error;
            _errorDescription = errorDescription;
        }

        public async Task ExecuteAsync(HttpContext context)
        {
            AuthorizeResponse response = null;

            // these are the conditions where we can send a response 
            // back directly to the client, otherwise we're only showing the error UI
            var isPromptNoneError = _error == OidcConstants.AuthorizeErrors.AccountSelectionRequired ||
                _error == OidcConstants.AuthorizeErrors.LoginRequired ||
                _error == OidcConstants.AuthorizeErrors.ConsentRequired ||
                _error == OidcConstants.AuthorizeErrors.InteractionRequired;

            if (_error == OidcConstants.AuthorizeErrors.AccessDenied ||
                (_request.PromptMode == OidcConstants.PromptModes.None && isPromptNoneError)
            )
            {
                response = new AuthorizeResponse
                {
                    Request = _request,
                    IsError = true,
                    Error = _error,
                    ErrorDescription = _errorDescription,
                    State = _request.State,
                    RedirectUri = _request.RedirectUri
                };

                await new AuthorizeResult(response).ExecuteAsync(context);
            }
            else
            {
                // we now know we must show error page
                var errorModel = new ErrorMessage
                {
                    RequestId = context.TraceIdentifier,
                    Error = _error,
                };

                await new ErrorPageResult(errorModel).ExecuteAsync(context);
            }
        }
    }
}
