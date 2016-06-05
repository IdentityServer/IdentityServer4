// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityServer4.Models
{
    static class GrantType
    {
        public const string Implicit = "implicit";

        public const string Hybrid = "hybrid";
        public const string HybridWithProofKey = "hybrid_with_proof_key";

        public const string Code = "code";
        public const string CodeWithProofKey = "code_with_proof_key";

        public const string ClientCredentials = "client_credentials";
        public const string ResourceOwnerPassword = "password";
    }
}