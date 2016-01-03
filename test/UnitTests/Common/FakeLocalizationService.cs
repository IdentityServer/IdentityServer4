// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Core.Services;

namespace UnitTests.Common
{
    public class FakeLocalizationService : ILocalizationService
    {
        public string GetString(string category, string id)
        {
            return null;
        }
    }
}
