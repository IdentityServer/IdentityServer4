// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
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
using System.Collections.Generic;

namespace IdentityServer4.Endpoints.Results
{
    internal class EndSessionCallbackResult : IEndpointResult
    {
        private readonly EndSessionCallbackValidationResult _result;
        private readonly IEnumerable<string> _urls;

        public EndSessionCallbackResult(EndSessionCallbackValidationResult result, IEnumerable<string> urls = null)
        {
            _result = result ?? throw new ArgumentNullException(nameof(result));
            _urls = urls;
        }

        internal EndSessionCallbackResult(
            EndSessionCallbackValidationResult result,
            IEnumerable<string> urls,
            IdentityServerOptions options)
            : this(result, urls)
        {
            _options = options;
        }

        private IdentityServerOptions _options;

        private void Init(HttpContext context)
        {
            _options = _options ?? context.RequestServices.GetRequiredService<IdentityServerOptions>();
        }

        public async Task ExecuteAsync(HttpContext context)
        {
            Init(context);

            if (_result?.IsError == true)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }
            else
            {
                context.Response.SetNoCache();
                AddCspHeaders(context);

                var html = GetHtml();
                await context.Response.WriteHtmlAsync(html);
            }
        }

        private void AddCspHeaders(HttpContext context)
        {
            if (_options.Authentication.RequireCspFrameSrcForSignout)
            {
                string frameSources = null;
                var origins = _urls?.Select(x => x.GetOrigin());
                if (origins != null && origins.Any())
                {
                    frameSources = origins.Distinct().Aggregate((x, y) => $"{x} {y}");
                }

                // the hash matches the embedded style element being used below
                context.Response.AddStyleCspHeaders(_options.Csp, "sha256-u+OupXgfekP+x/f6rMdoEAspPCYUtca912isERnoEjY=", frameSources);
            }
        }

        private string GetHtml()
        {
            string framesHtml = null;

            if (_urls?.Any() == true)
            {
                var frameUrls = _urls.Select(url => $"<iframe src='{url}'></iframe>");
                framesHtml = frameUrls.Aggregate((x, y) => x + y);
            }

            return $"<!DOCTYPE html><html><style>iframe{{display:none;width:0;height:0;}}</style><body>{framesHtml}</body></html>";
        }
    }
}
