// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Configuration;
using IdentityServer4.Extensions;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IdentityServer4.Hosting
{
    class EndpointRouter : IEndpointRouter
    {
        private readonly Dictionary<string, EndpointName> _pathToNameMap;
        private readonly IdentityServerOptions _options;
        private readonly IEnumerable<EndpointMapping> _mappings;

        public EndpointRouter(Dictionary<string, EndpointName> pathToNameMap, IdentityServerOptions options, IEnumerable<EndpointMapping> mappings)
        {
            _pathToNameMap = pathToNameMap;
            _options = options;
            _mappings = mappings;
        }

        public IEndpoint Find(HttpContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            foreach(var key in _pathToNameMap.Keys)
            {
                var path = key.EnsureLeadingSlash();
                if (context.Request.Path.StartsWithSegments(path))
                {
                    return GetEndpoint(_pathToNameMap[key], context);
                }
            }

            return null;
        }

        private IEndpoint GetEndpoint(EndpointName endpointName, HttpContext context)
        {
            if (_options.Endpoints.IsEndpointEnabled(endpointName))
            {
                var mapping = _mappings.Where(x => x.Endpoint == endpointName).LastOrDefault();
                if (mapping != null)
                {
                    return context.RequestServices.GetService(mapping.Handler) as IEndpoint;
                }
            }
            return null;
        }
    }
}
