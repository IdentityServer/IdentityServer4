using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace IdentityServer4.Tests.Common
{
    public class MessageHandlerWrapper : DelegatingHandler
    {
        public HttpResponseMessage Response { get; set; }

        public MessageHandlerWrapper(HttpMessageHandler handler)
            : base(handler)
        {
        }

        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Response = await base.SendAsync(request, cancellationToken);
            return Response;
        }
    }
}
