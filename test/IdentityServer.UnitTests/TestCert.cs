﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.IdentityModel.Tokens;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace IdentityServer4.Tests
{
    static class TestCert
    {
        public static X509Certificate2 Load()
        {
            var cert = Path.Combine(PlatformServices.Default.Application.ApplicationBasePath, "idsvrtest.pfx");
            return new X509Certificate2(cert, "idsrv3test");
        }

        public static SigningCredentials LoadSigningCredentials()
        {
            var cert = Load();
            return new SigningCredentials(new X509SecurityKey(cert), "RS256");
        }
    }
}