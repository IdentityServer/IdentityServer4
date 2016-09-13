// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Threading.Tasks;
using IdentityServer4.Validation;
using IdentityServer4.Hosting;
using Microsoft.AspNetCore.Http;
using IdentityServer4.Configuration;
using Microsoft.Extensions.DependencyInjection;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using IdentityServer4.Extensions;
using System;

namespace IdentityServer4.Endpoints.Results
{
    public class EndSessionResult : IEndpointResult
    {
        private readonly EndSessionValidationResult _result;

        public EndSessionResult(EndSessionValidationResult result)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));

            _result = result;
        }

        public async Task ExecuteAsync(HttpContext context)
        {
            var options = context.RequestServices.GetRequiredService<IdentityServerOptions>();
            var validatedRequest = _result.IsError ? null : _result.ValidatedRequest;

            string id = null;

            if (validatedRequest != null &&
                (validatedRequest.Client != null || validatedRequest.PostLogOutUri != null))
            {
                var msg = new MessageWithId<LogoutMessage>(new LogoutMessage(validatedRequest));
                id = msg.Id;

                var logoutMessageStore = context.RequestServices.GetRequiredService<IMessageStore<LogoutMessage>>();
                await logoutMessageStore.WriteAsync(id, msg);
            }

            var redirect = options.UserInteractionOptions.LogoutUrl;

            if (redirect.IsLocalUrl())
            {
                if (redirect.StartsWith("~/")) redirect = redirect.Substring(1);
                redirect = context.GetIdentityServerBaseUrl().EnsureTrailingSlash() + redirect.RemoveLeadingSlash();
            }

            if (id != null)
            {
                redirect = redirect.AddQueryString(options.UserInteractionOptions.LogoutIdParameter, id);
            }

            context.Response.Redirect(redirect);
        }
    }
}
