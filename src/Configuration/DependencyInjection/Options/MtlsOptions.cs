// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.



namespace IdentityServer4.Configuration
{
    public class MtlsOptions
    {
        public bool Enabled { get; set; }

        public string ClientCertificateAuthenticationScheme { get; set; } = "Certificate";
    }
}