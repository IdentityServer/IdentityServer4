// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Extensions;
using IdentityServer4.Hosting;
using IdentityModel;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using IdentityServer4.Services;
using IdentityServer4.Configuration;
using IdentityServer4.Stores;

namespace IdentityServer4.Endpoints.Results
{
    class AuthorizeResult : IEndpointResult
    {
        public AuthorizeResponse Response { get; private set; }

        public AuthorizeResult(AuthorizeResponse response)
        {
            Response = response;
        }

        public async Task ExecuteAsync(HttpContext context)
        {
            if (Response.IsError)
            {
                await ProcessErrorAsync(context);
            }
            else
            {
                await ProcessResponseAsync(context);
            }
        }

        async Task ProcessErrorAsync(HttpContext context)
        {
            // these are the conditions where we can send a response 
            // back directly to the client, otherwise we're only showing the error UI
            var isPromptNoneError = Response.Error == OidcConstants.AuthorizeErrors.AccountSelectionRequired ||
                Response.Error == OidcConstants.AuthorizeErrors.LoginRequired ||
                Response.Error == OidcConstants.AuthorizeErrors.ConsentRequired ||
                Response.Error == OidcConstants.AuthorizeErrors.InteractionRequired;

            if (Response.Error == OidcConstants.AuthorizeErrors.AccessDenied ||
                (isPromptNoneError && Response.Request?.PromptMode == OidcConstants.PromptModes.None)
            )
            {
                // this scenario we can return back to the client
                await ProcessResponseAsync(context);
            }
            else
            {
                // we now know we must show error page
                await RedirectToErrorPageAsync(context);
            }
        }

        protected async Task ProcessResponseAsync(HttpContext context)
        {
            if (!Response.IsError)
            {
                // success response -- track client authorization for sign-out
                var clientSession = context.RequestServices.GetRequiredService<IClientSessionService>();
                //_logger.LogDebug("Adding client {0} to client list cookie for subject {1}", request.ClientId, request.Subject.GetSubjectId());
                await clientSession.AddClientIdAsync(Response.Request.ClientId);
            }

            await RenderAuthorizeResponseAsync(context);
        }

        private async Task RenderAuthorizeResponseAsync(HttpContext context)
        {
            if (Response.Request.ResponseMode == OidcConstants.ResponseModes.Query ||
                Response.Request.ResponseMode == OidcConstants.ResponseModes.Fragment)
            {
                context.Response.SetNoCache();
                context.Response.Redirect(BuildRedirectUri());
            }
            else if (Response.Request.ResponseMode == OidcConstants.ResponseModes.FormPost)
            {
                context.Response.SetNoCache();
                await context.Response.WriteHtmlAsync(GetFormPostHtml());
            }
            else
            {
                //_logger.LogError("Unsupported response mode.");
                throw new InvalidOperationException("Unsupported response mode");
            }
        }

        string BuildRedirectUri()
        {
            var uri = Response.RedirectUri;
            var query = Response.ToNameValueCollection().ToQueryString();

            if (Response.Request.ResponseMode == OidcConstants.ResponseModes.Query)
            {
                uri = uri.AddQueryString(query);
            }
            else
            {
                uri = uri.AddHashFragment(query);
            }

            if (Response.IsError && !uri.Contains("#"))
            {
                // https://tools.ietf.org/html/draft-bradley-oauth-open-redirector-00
                uri += "#_=_";
            }

            return uri;
        }

        const string _formPostHtml = "<!DOCTYPE html><html><head><title>Submit this form</title><meta name='viewport' content='width=device-width, initial-scale=1.0' /></head><body><form method='post' action='{uri}'>{body}</form><script>(function(){document.forms[0].submit();})();</script></body></html>";

        string GetFormPostHtml()
        {
            var html = _formPostHtml;

            html = html.Replace("{uri}", Response.Request.RedirectUri);
            html = html.Replace("{body}", Response.ToNameValueCollection().ToFormPost());

            return html;
        }

        async Task RedirectToErrorPageAsync(HttpContext context)
        {
            var errorModel = new ErrorMessage
            {
                RequestId = context.TraceIdentifier,
                Error = Response.Error
            };

            var errorMessageStore = context.RequestServices.GetRequiredService<IMessageStore<ErrorMessage>>();
            var message = new MessageWithId<ErrorMessage>(errorModel);
            await errorMessageStore.WriteAsync(message.Id, message);

            var options = context.RequestServices.GetRequiredService<IdentityServerOptions>();
            var errorUrl = options.UserInteractionOptions.ErrorUrl;

            var url = errorUrl.AddQueryString(options.UserInteractionOptions.ErrorIdParameter, message.Id);
            context.Response.RedirectToAbsoluteUrl(url);
        }
    }
}
