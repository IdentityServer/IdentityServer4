// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace Microsoft.Extensions.PlatformAbstractions
{
    public static class ApplicationEnvironmentExtensions
    {
        public static X509Certificate2 LoadSigningCert(this IApplicationEnvironment appEnv)
        {
            return new X509Certificate2(Path.Combine(appEnv.ApplicationBasePath, "idsrv3test.pfx"), "idsrv3test");
        }
    }
}
