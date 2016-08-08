// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Threading.Tasks;
using IdentityServer4.Hosting;
using IdentityServer4.Extensions;
using System.Text.Encodings.Web;

namespace IdentityServer4.Endpoints.Results
{
    public class RedirectToPageWithReturnUrlResult : RedirectToPageResult
    {
        public RedirectToPageWithReturnUrlResult(string url, string returnUrlParamName, string returnUrl) : 
            base(url, returnUrlParamName, returnUrl)
        {
        }

        public override Task ExecuteAsync(IdentityServerContext context)
        {
            if (!Url.IsLocalUrl())
            {
                // this converts the relative redirect path to an absolute one if we're 
                // redirecting to a different server
                ParamValue = context.GetIdentityServerBaseUrl().EnsureTrailingSlash() + ParamValue.RemoveLeadingSlash();
            }

            return base.ExecuteAsync(context);
        }
    }
}
