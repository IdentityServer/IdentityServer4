// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Microsoft.AspNet.Cors.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using IdentityServer4.Core.Services;
using Microsoft.Extensions.Logging;

namespace IdentityServer4.Core.Hosting.Cors
{
    public class PolicyProvider : ICorsPolicyProvider
    {
        private readonly ICorsPolicyService _corsPolicyService;
        private readonly string[] _allowedPaths;
        private readonly ILogger<PolicyProvider> _logger;

        public PolicyProvider(
            ILogger<PolicyProvider> logger,
            IEnumerable<string> allowedPaths, 
            ICorsPolicyService corsPolicyService)
        {
            if (allowedPaths == null) throw new ArgumentNullException("allowedPaths");

            _logger = logger;
            _allowedPaths = allowedPaths.Select(Normalize).ToArray();
            _corsPolicyService = corsPolicyService;
        }

        public async Task<CorsPolicy> GetPolicyAsync(HttpContext context, string policyName)
        {
            var path = context.Request.Path.ToString();
            var origin = context.Request.Headers["Origin"].First();
            var thisOrigin = context.Request.Scheme + "://" + context.Request.Host;

            // see if the Origin is different than this server's origin. if so
            // that indicates a proper CORS request. some browsers send Origin
            // on POST requests.
            // todo: do we still need this check?
            if (origin != null && origin != thisOrigin)
            {
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

        private bool IsPathAllowed(string pathToCheck)
        {
            var requestPath = Normalize(pathToCheck);
            return _allowedPaths.Any(path => requestPath.Equals(path, StringComparison.OrdinalIgnoreCase));
        }

        private string Normalize(string path)
        {
            if (String.IsNullOrWhiteSpace(path) || path == "/")
            {
                path = "/";
            }
            else
            {
                if (!path.StartsWith("/"))
                {
                    path = "/" + path;
                }
                if (path.EndsWith("/"))
                {
                    path = path.Substring(0, path.Length - 1);
                }
            }

            return path;
        }
    }
}
