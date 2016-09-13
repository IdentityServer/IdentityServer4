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

namespace IdentityServer4.Endpoints.Results
{
    class AuthorizeResult : IEndpointResult
    {
        protected AuthorizeResponse _response;

        protected AuthorizeResult()
        {
        }

        public AuthorizeResult(AuthorizeResponse response)
        {
            _response = response;
        }

        public async Task ExecuteAsync(HttpContext context)
        {
            await RenderAuthorizeResponseAsync(context, _response);
        }

        protected async Task RenderAuthorizeResponseAsync(HttpContext context, AuthorizeResponse response)
        {
            if (response == null)
            {
                throw new InvalidOperationException("No response");
            }

            if (response.IsError == false)
            {
                var clientSession = context.RequestServices.GetRequiredService<IClientSessionService>();
                //_logger.LogDebug("Adding client {0} to client list cookie for subject {1}", request.ClientId, request.Subject.GetSubjectId());
                await clientSession.AddClientIdAsync(response.Request.ClientId);
            }

            await WriteResponseAsync(context, response);
        }

        private async Task WriteResponseAsync(HttpContext context, AuthorizeResponse response)
        {
            var request = response.Request;

            if (request.ResponseMode == OidcConstants.ResponseModes.Query ||
                request.ResponseMode == OidcConstants.ResponseModes.Fragment)
            {
                context.Response.SetNoCache();
                context.Response.Redirect(BuildRedirectUri(response));
            }
            else if (request.ResponseMode == OidcConstants.ResponseModes.FormPost)
            {
                await context.Response.WriteHtmlAsync(GetFormPostHtml(response));
            }
            else
            {
                //_logger.LogError("Unsupported response mode.");
                throw new InvalidOperationException("Unsupported response mode");
            }
        }

        internal static string BuildRedirectUri(AuthorizeResponse response)
        {
            var uri = response.RedirectUri;
            var query = response.ToNameValueCollection().ToQueryString();

            if (response.Request.ResponseMode == OidcConstants.ResponseModes.Query)
            {
                uri = uri.AddQueryString(query);
            }
            else
            {
                uri = uri.AddHashFragment(query);
            }

            if (response.IsError && !uri.Contains("#"))
            {
                // https://tools.ietf.org/html/draft-bradley-oauth-open-redirector-00
                uri += "#_=_";
            }

            return uri;
        }

        const string _formPostHtml = "<!DOCTYPE html><html><head><title>Submit this form</title><meta name='viewport' content='width=device-width, initial-scale=1.0' /></head><body><form method='post' action='{uri}'>{body}</form><script>(function(){document.forms[0].submit();})();</script></body></html>";

        string GetFormPostHtml(AuthorizeResponse response)
        {
            var html = _formPostHtml;

            html = html.Replace("{uri}", response.RedirectUri);
            html = html.Replace("{body}", BuildFormPostBody(response));

            return html;
        }

        string BuildFormPostBody(AuthorizeResponse response)
        {
            return response.ToNameValueCollection().ToFormPost();
        }
    }
}
