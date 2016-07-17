// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Threading.Tasks;
using IdentityServer4.Hosting;
using IdentityServer4.Extensions;
using System.Text.Encodings.Web;

namespace IdentityServer4.Endpoints.Results
{
    public class RedirectToPageResult : IEndpointResult
    {
        public string Url { get; set; }
        public string ParamName { get; set; }
        public string ParamValue { get; set; }

        public RedirectToPageResult(string url)
            : this(url, null, null)
        {
        }

        public RedirectToPageResult(string url, string paramName, string paramValue)
        {
            Url = url;
            if (paramValue.IsPresent())
            {
                ParamName = paramName;
                ParamValue = paramValue;
            }
        }

        public virtual Task ExecuteAsync(IdentityServerContext context)
        {
            var redirect = Url;
            if (redirect.IsLocalUrl())
            {
                if (redirect.StartsWith("~/")) redirect = redirect.Substring(1);
                redirect = context.GetIdentityServerBaseUrl().EnsureTrailingSlash() + redirect.RemoveLeadingSlash();
            }

            if (ParamValue.IsPresent())
            {
                redirect = redirect.AddQueryString(ParamName + "=" + UrlEncoder.Default.Encode(ParamValue));
            }

            context.HttpContext.Response.Redirect(redirect);

            return Task.FromResult(0);
        }
    }
}
