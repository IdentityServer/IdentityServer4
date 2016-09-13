// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Threading.Tasks;
using IdentityServer4.Configuration;
using IdentityServer4.Hosting;
using Microsoft.AspNetCore.Http;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using Microsoft.Extensions.DependencyInjection;
using IdentityServer4.Extensions;

namespace IdentityServer4.Endpoints.Results
{
    public class ErrorPageResult : IEndpointResult
    {
        private readonly ErrorMessage _error;

        public ErrorPageResult(ErrorMessage error)
        {
            _error = error;
        }

        public async Task ExecuteAsync(HttpContext context)
        {
            var errorMessageStore = context.RequestServices.GetRequiredService<IMessageStore<ErrorMessage>>();
            var message = new MessageWithId<ErrorMessage>(_error);
            await errorMessageStore.WriteAsync(message.Id, message);

            var options = context.RequestServices.GetRequiredService<IdentityServerOptions>();
            var errorUrl = options.UserInteractionOptions.ErrorUrl;

            var url = errorUrl.AddQueryString(options.UserInteractionOptions.ErrorIdParameter, message.Id);
            context.Response.RedirectToAbsoluteUrl(url);
        }
    }
}
