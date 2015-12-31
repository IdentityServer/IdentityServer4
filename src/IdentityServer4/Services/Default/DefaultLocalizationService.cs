// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityServer4.Core.Services.Default
{
    /// <summary>
    /// Default localization services. Uses embedded resource files for strings.
    /// The category is used to identify which resource file from which to read.
    /// </summary>
    public class DefaultLocalizationService : ILocalizationService
    {
        /// <summary>
        /// Gets a localized string based upon the string's category and identifier.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public virtual string GetString(string category, string id)
        {
            switch(category)
            {
                case Constants.LocalizationCategories.Messages:
                    return IdentityServer4.Core.Resources.Messages.GetString(id);
                case Constants.LocalizationCategories.Events:
                    return IdentityServer4.Core.Resources.Events.GetString(id);
                case Constants.LocalizationCategories.Scopes:
                    return IdentityServer4.Core.Resources.Scopes.GetString(id);
            }
            
            return null;
        }
    }
}
