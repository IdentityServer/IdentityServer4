using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.Logging;
using IdentityServer4.Core.Models;
using IdentityServer4.Core.Extensions;
using Microsoft.Extensions.WebEncoders;

namespace IdentityServer4.Core.Results
{
    public class AuthorizeFormPostResult : AuthorizeResult
    {
        private readonly AuthorizeResponse _response;
        private readonly IHtmlEncoder _encoder;

        public AuthorizeFormPostResult(AuthorizeResponse response, IHtmlEncoder encoder)
        {
            _response = response;
            _encoder = encoder;
        }

        public static string BuildFormBody(AuthorizeResponse response, IHtmlEncoder encoder)
        {
            return response.ToNameValueCollection().ToFormPost(encoder);
        }

        public override Task ExecuteAsync(HttpContext context, ILogger logger)
        {
            return Task.FromResult(0);
        }
    }
}
