// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Extensions;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace IdentityServer4.Hosting
{
    class EndpointRouter : IEndpointRouter
    {
        IDictionary<string, Type> _map;

        public EndpointRouter(IDictionary<string, Type> map)
        {
            if (map == null) throw new ArgumentNullException(nameof(map));

            _map = map;
        }

        public IEndpoint Find(HttpContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            IEndpoint endpoint = null;
            foreach(var key in _map.Keys)
            {
                var path = key.EnsureLeadingSlash();
                if (context.Request.Path.StartsWithSegments(path))
                {
                    endpoint = context.RequestServices.GetService(_map[key]) as IEndpoint;
                    break;
                }
            }

            return endpoint;
        }
    }
}
