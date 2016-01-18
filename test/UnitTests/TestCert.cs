// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Microsoft.Extensions.PlatformAbstractions;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace IdentityServer4.Tests
{
    static class TestCert
    {
        public static X509Certificate2 Load()
        {
            var cert = Path.Combine(PlatformServices.Default.Application.ApplicationBasePath, "idsrv3test.pfx");

            // todo
            return new X509Certificate2(cert, "idsrv3test");

            //var assembly = typeof(TestCert).Assembly;
            //using (var stream = assembly.GetManifestResourceStream("IdentityServer4.Tests.idsrv3test.pfx"))
            //{
            //    return new X509Certificate2(ReadStream(stream), "idsrv3test");
            //}
        }

        private static byte[] ReadStream(Stream input)
        {
            var buffer = new byte[16 * 1024];
            using (var ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
    }
}