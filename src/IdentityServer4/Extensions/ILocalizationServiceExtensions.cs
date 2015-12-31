// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Core.Services;
using System;

namespace IdentityServer4.Core.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="IdentityServer3.Core.Services.ILocalizationService"/>
    /// </summary>
    public static class ILocalizationServiceExtensions
    {
        /// <summary>
        /// Gets a localized string for the message category.
        /// </summary>
        /// <param name="localization">The localization.</param>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">localization</exception>
        public static string GetMessage(this ILocalizationService localization, string id)
        {
            if (localization == null) throw new ArgumentNullException("localization");

            return localization.GetString(Constants.LocalizationCategories.Messages, id);
        }

        /// <summary>
        /// Gets a localized string for the event category.
        /// </summary>
        /// <param name="localization">The localization.</param>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">localization</exception>
        public static string GetEvent(this ILocalizationService localization, string id)
        {
            if (localization == null) throw new ArgumentNullException("localization");

            return localization.GetString(Constants.LocalizationCategories.Events, id);
        }

        /// <summary>
        /// Gets a localized scope display name.
        /// </summary>
        /// <param name="localization">The localization.</param>
        /// <param name="scope">The scope.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">localization</exception>
        public static string GetScopeDisplayName(this ILocalizationService localization, string scope)
        {
            if (localization == null) throw new ArgumentNullException("localization");
            
            return localization.GetString(Constants.LocalizationCategories.Scopes, scope + Constants.ScopeDisplayNameSuffix);
        }

        /// <summary>
        /// Gets a localized scope description.
        /// </summary>
        /// <param name="localization">The localization.</param>
        /// <param name="scope">The scope.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">localization</exception>
        public static string GetScopeDescription(this ILocalizationService localization, string scope)
        {
            if (localization == null) throw new ArgumentNullException("localization");
            
            return localization.GetString(Constants.LocalizationCategories.Scopes, scope + Constants.ScopeDescriptionSuffix);
        }
    }
}
