﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Services;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Cors.Infrastructure;
using IdentityServer4.Configuration;
using IdentityServer4.Extensions;

namespace IdentityServer4.Hosting.Cors
{
    public class PolicyProvider<T> : ICorsPolicyProvider
        where T : ICorsPolicyProvider
    {
        private readonly ICorsPolicyService _corsPolicyService;
        private readonly ILogger<PolicyProvider<T>> _logger;
        private readonly T _inner;
        private readonly IdentityServerOptions _options;

        public PolicyProvider(
            ILogger<PolicyProvider<T>> logger,
            T inner,
            IdentityServerOptions options,
            ICorsPolicyService corsPolicyService)
        {
            _logger = logger;
            _inner = inner;
            _options = options;
            _corsPolicyService = corsPolicyService;
        }

        public Task<CorsPolicy> GetPolicyAsync(HttpContext context, string policyName)
        {
            if (_options.CorsOptions.CorsPolicyName == policyName)
            {
                return ProcessAsync(context);
            }
            else
            {
                return _inner.GetPolicyAsync(context, policyName);
            }
        }

        async Task<CorsPolicy> ProcessAsync(HttpContext context)
        {
            var origin = context.Request.Headers["Origin"].First();
            var thisOrigin = context.Request.Scheme + "://" + context.Request.Host;

            // see if the Origin is different than this server's origin. if so
            // that indicates a proper CORS request. some browsers send Origin
            // on POST requests.
            // todo: do we still need this check?
            if (origin != null && origin != thisOrigin)
            {
                var path = context.Request.Path;
                if (IsPathAllowed(path))
                {
                    _logger.LogInformation("CORS request made for path: {0} from origin: {1}", path, origin);

                    if (await _corsPolicyService.IsOriginAllowedAsync(origin))
                    {
                        _logger.LogInformation("CorsPolicyService allowed origin");
                        return Allow(origin);
                    }
                    else
                    {
                        _logger.LogInformation("CorsPolicyService did not allow origin");
                    }
                }
                else
                {
                    _logger.LogWarning("CORS request made for path: {0} from origin: {1} but rejected because invalid CORS path", path, origin);
                }
            }

            return null;
        }

        private CorsPolicy Allow(string origin)
        {
            var policyBuilder = new CorsPolicyBuilder();

            var policy = policyBuilder
                .WithOrigins(origin)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .Build();

            return policy;
        }

        private bool IsPathAllowed(PathString path)
        {
            return _options.CorsOptions.CorsPaths.Any(x => path == x);
        }
    }
}
