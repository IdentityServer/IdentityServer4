﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityServer4.Services
{
    /// <summary>
    /// Models loading localizable strings.
    /// </summary>
    public interface ILocalizationService
    {
        /// <summary>
        /// Gets a localized string based upon the string's category and identifier.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        string GetString(string category, string id);
    }
}
