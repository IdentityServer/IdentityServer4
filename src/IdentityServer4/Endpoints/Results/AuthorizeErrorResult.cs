// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Extensions;
using IdentityServer4.Hosting;
using IdentityModel;
using Microsoft.AspNetCore.Http;
using IdentityServer4.Validation;
using Microsoft.Extensions.DependencyInjection;
using IdentityServer4.Stores;
using IdentityServer4.Configuration;

namespace IdentityServer4.Endpoints.Results
{
    class AuthorizeErrorResult : AuthorizeResult, IEndpointResult
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

        public new async Task ExecuteAsync(HttpContext context)
        {
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
                var response = new AuthorizeResponse
                {
                    Request = _request,
                    IsError = true,
                    Error = _error,
                    ErrorDescription = _errorDescription,
                    State = _request.State,
                    RedirectUri = _request.RedirectUri
                };

                await RenderAuthorizeResponseAsync(context, response);
            }
            else
            {
                // we now know we must show error page
                var errorModel = new ErrorMessage
                {
                    RequestId = context.TraceIdentifier,
                    Error = _error,
                };

                await RedirectToErrorPage(context, errorModel);
            }
        }

        async Task RedirectToErrorPage(HttpContext context, ErrorMessage error)
        {
            var errorMessageStore = context.RequestServices.GetRequiredService<IMessageStore<ErrorMessage>>();
            var message = new MessageWithId<ErrorMessage>(error);
            await errorMessageStore.WriteAsync(message.Id, message);

            var options = context.RequestServices.GetRequiredService<IdentityServerOptions>();
            var errorUrl = options.UserInteractionOptions.ErrorUrl;

            var url = errorUrl.AddQueryString(options.UserInteractionOptions.ErrorIdParameter, message.Id);
            context.Response.RedirectToAbsoluteUrl(url);
        }
    }
}
