using Microsoft.AspNet.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer4.Core.Endpoints
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
                var path = key;
                if (!path.StartsWith("/"))
                {
                    path = "/" + path;
                };

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
