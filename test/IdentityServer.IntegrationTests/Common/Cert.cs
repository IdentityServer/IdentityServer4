// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Microsoft.Extensions.PlatformAbstractions;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace IdentityServer4.Tests.Common
{
    public static class Cert
    {
        public static X509Certificate2 Load()
        {
            return new X509Certificate2(Path.Combine(PlatformServices.Default.Application.ApplicationBasePath, "idsvrtest.pfx"), "idsrv3test");
        }
    }
}
