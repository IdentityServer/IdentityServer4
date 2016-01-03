// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Core.Configuration;

namespace IdentityServer4.Tests.Validation
{
    class TestIdentityServerOptions
    {
        public static IdentityServerOptions Create()
        {
            var options = new IdentityServerOptions
            {
                IssuerUri = "https://idsrv3.com",
                SiteName = "IdentityServer3 - test",
                //todo
                //DataProtector = new NoDataProtector(),
            };

            options.SigningCertificate = TestCert.Load();
            
            return options;
        }
    }
}