using IdentityServer4.Core.Extensions;
using IdentityServer4.Core.Hosting;
using Microsoft.AspNet.Http;
using System;
using System.Collections.Generic;

namespace IdentityServer4.Core.Hosting
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
                    endpoint = context.ApplicationServices.GetService(_map[key]) as IEndpoint;
                    break;
                }
            }

            return endpoint;
        }
    }
}
