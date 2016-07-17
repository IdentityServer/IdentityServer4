// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Models;
using IdentityServer4.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer4.Services
{
    public interface IUserInteractionService
    {
        Task<AuthorizationRequest> GetLoginContextAsync(string returnUrl = null);
        Task<LogoutRequest> GetLogoutContextAsync(string logoutId = null);
        Task<AuthorizationRequest> GetConsentContextAsync(string returnUrl = null);
        Task<ErrorMessage> GetErrorContextAsync(string errorId = null);

        bool IsValidReturnUrl(string returnUrl);
        Task GrantConsentAsync(AuthorizationRequest request, ConsentResponse consent, string subject = null);
    }
}
