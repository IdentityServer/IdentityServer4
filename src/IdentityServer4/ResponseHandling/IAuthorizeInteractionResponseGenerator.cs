/*
 * Copyright 2014, 2015 Dominick Baier, Brock Allen
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using IdentityServer4.Core.Configuration;
using IdentityServer4.Core.Extensions;
using IdentityServer4.Core.Logging;
using IdentityServer4.Core.Models;
using IdentityServer4.Core.Resources;
using IdentityServer4.Core.Services;
using IdentityServer4.Core.Validation;
using IdentityServer4.Core.ViewModels;
using System;
using System.ComponentModel;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityServer4.Core.ResponseHandling
{
    interface IAuthorizeInteractionResponseGenerator
    {
        Task<LoginInteractionResponse> ProcessLoginAsync(ValidatedAuthorizeRequest request, ClaimsPrincipal user);
        Task<LoginInteractionResponse> ProcessClientLoginAsync(ValidatedAuthorizeRequest request);
        Task<ConsentInteractionResponse> ProcessConsentAsync(ValidatedAuthorizeRequest request, UserConsentResponseMessage consent = null);
    }
}