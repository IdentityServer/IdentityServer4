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

        internal EndSessionResult(
            EndSessionValidationResult result,
            IdentityServerOptions options,
            IMessageStore<LogoutMessage> logoutMessageStore)
            : this(result)
        {
            _options = options;
            _logoutMessageStore = logoutMessageStore;
        }

        private IdentityServerOptions _options;
        private IMessageStore<LogoutMessage> _logoutMessageStore;

        void Init(HttpContext context)
        {
            _options = _options ?? context.RequestServices.GetRequiredService<IdentityServerOptions>();
            _logoutMessageStore = _logoutMessageStore ?? context.RequestServices.GetRequiredService<IMessageStore<LogoutMessage>>();
        }

        public async Task ExecuteAsync(HttpContext context)
        {
            Init(context);

            var validatedRequest = _result.IsError ? null : _result.ValidatedRequest;

            string id = null;

            if (validatedRequest != null &&
                (validatedRequest.Client != null || validatedRequest.PostLogOutUri != null))
            {
                var msg = new MessageWithId<LogoutMessage>(new LogoutMessage(validatedRequest));
                id = msg.Id;

                await _logoutMessageStore.WriteAsync(id, msg);
            }

            var redirect = _options.UserInteractionOptions.LogoutUrl;

            if (redirect.IsLocalUrl())
            {
                // TODO: look at GetIdentityServerRelativeUrl instead and logic if the above if check; compare to login result
                if (redirect.StartsWith("~/")) redirect = redirect.Substring(1);
                redirect = context.GetIdentityServerBaseUrl().EnsureTrailingSlash() + redirect.RemoveLeadingSlash();
            }

            if (id != null)
            {
                redirect = redirect.AddQueryString(_options.UserInteractionOptions.LogoutIdParameter, id);
            }

            context.Response.Redirect(redirect);
        }
    }
}
