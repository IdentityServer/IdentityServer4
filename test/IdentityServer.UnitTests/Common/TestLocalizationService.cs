// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Services;

namespace UnitTests.Common
{
    public class TestLocalizationService : ILocalizationService
    {
        public string Result { get; set; }

        public string GetString(string category, string id)
        {
            return Result;
        }
    }
}
