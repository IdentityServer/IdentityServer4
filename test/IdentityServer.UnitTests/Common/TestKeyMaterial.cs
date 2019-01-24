// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Microsoft.IdentityModel.Tokens;

namespace IdentityServer4.UnitTests.Common
{

    internal static class TestKeyMaterial
    {
        private static string Es256Json =
            @"{
                ""kty"":""EC"",
                ""alg"":""ES256"",
                ""crv"":""P-256"",
                ""x"":""MKBCTNIcKUSDii11ySs3526iDZ8AiTo7Tu6KPAqv7D4"",
                ""y"":""4Etl6SRW2YiLUrN5vfvVHuhp7x8PxltmWWlbbM4IFyM"",
                ""d"":""870MB6gfuTJ4HtUnUvYMyJpr5eUZNP4Bk43bVdj3eAE"",
                ""use"":""enc"",
                ""kid"":""1""
            }";

        public static SigningCredentials Es256SigningCredentials =>
            new SigningCredentials(new JsonWebKey(Es256Json), "ES256");
    }
}