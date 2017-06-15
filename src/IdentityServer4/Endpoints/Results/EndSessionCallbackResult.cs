﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Linq;
using IdentityServer4.Validation;
using System.Threading.Tasks;
using IdentityServer4.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System;
using IdentityServer4.Extensions;
using IdentityServer4.Configuration;
using IdentityServer4.Infrastructure;

namespace IdentityServer4.Endpoints.Results
{
    class EndSessionCallbackResult : IEndpointResult
    {
        private readonly EndSessionCallbackValidationResult _result;

        public EndSessionCallbackResult(EndSessionCallbackValidationResult result)
        {
            _result = result ?? throw new ArgumentNullException(nameof(result));
        }

        internal EndSessionCallbackResult(
            EndSessionCallbackValidationResult result,
            IdentityServerOptions options,
            BackChannelLogoutClient backChannelClient)
            : this(result)
        {
            _options = options;
            _backChannelClient = backChannelClient;
        }

        private IdentityServerOptions _options;
        private BackChannelLogoutClient _backChannelClient;

        void Init(HttpContext context)
        {
            _options = _options ?? context.RequestServices.GetRequiredService<IdentityServerOptions>();
            _backChannelClient = _backChannelClient ?? context.RequestServices.GetRequiredService<BackChannelLogoutClient>();
        }

        public async Task ExecuteAsync(HttpContext context)
        {
            Init(context);

            if (_result.IsError)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }
            else
            {
                context.Response.SetNoCache();
                AddXfoHeaders(context);
                AddCspHeaders(context);

                var html = GetHtml();
                await context.Response.WriteHtmlAsync(html);
                await context.Response.Body.FlushAsync();

                // todo: discuss if we should do this before rendering/flushing
                // or even from a forked task
                await _backChannelClient.SendLogoutsAsync(_result.BackChannelLogouts);
            }
        }

        private void AddCspHeaders(HttpContext context)
        {
            // 'unsafe-inline' for edge
            // the hash matches the embedded style element being used below
            var value = "default-src 'none'; style-src 'unsafe-inline' 'sha256-u+OupXgfekP+x/f6rMdoEAspPCYUtca912isERnoEjY='";

            var origins = _result.FrontChannelLogoutUrls?.Select(x => x.GetOrigin());
            if (origins != null && origins.Any())
            {
                var list = origins.Aggregate((x, y) => $"{x} {y}");
                value += $";frame-src {list}";
            }

            if (!context.Response.Headers.ContainsKey("Content-Security-Policy"))
            {
                context.Response.Headers.Add("Content-Security-Policy", value);
            }

            if (!context.Response.Headers.ContainsKey("X-Content-Security-Policy"))
            {
                context.Response.Headers.Add("X-Content-Security-Policy", value);
            }
        }

        private void AddXfoHeaders(HttpContext context)
        {
            if (!context.Response.Headers.ContainsKey("X-Frame-Options"))
            {
                var logoutPageUrl = _options.UserInteraction.LogoutUrl;
                if (logoutPageUrl.IsLocalUrl())
                {
                    context.Response.Headers.Add("X-Frame-Options", "SAMEORIGIN");
                }
                else
                {
                    context.Response.Headers.Add("X-Frame-Options", $"ALLOW-FROM {logoutPageUrl.GetOrigin()}");
                }
            }
        }

        string GetHtml()
        {
            string framesHtml = null;

            if (_result.FrontChannelLogoutUrls != null && _result.FrontChannelLogoutUrls.Any())
            {
                var frameUrls = _result.FrontChannelLogoutUrls.Select(url => $"<iframe src='{url}'></iframe>");
                framesHtml = frameUrls.Aggregate((x, y) => x + y);
            }

            return $"<!DOCTYPE html><html><style>iframe{{display:none;width:0;height:0;}}</style><body>{framesHtml}</body></html>";
        }
    }
}
