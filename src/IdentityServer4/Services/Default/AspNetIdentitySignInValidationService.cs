// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Features.Authentication;
using System;
using System.Security.Claims;
using IdentityModel;
using System.Linq;

namespace IdentityServer4.Services.Default
{
    public class AspNetIdentitySignInValidationService : ISignInValdationService
    {
        public Task ValidateAsync(SignInContext context)
        {
            var sub = context.Principal.FindFirst(JwtClaimTypes.Subject);
            if (sub == null)
            {
                sub = context.Principal.FindFirst(ClaimTypes.NameIdentifier);
                if (sub == null) sub = context.Principal.FindFirst(ClaimTypes.Name);
                if (sub == null)
                {
                    throw new InvalidOperationException("A sub claim, name identifier claim, or name claim is required");
                }

                var id = context.Principal.Identities.First();
                id.AddClaim(new Claim(JwtClaimTypes.Subject, sub.Value));
            }

            var name = context.Principal.FindFirst(JwtClaimTypes.Name);
            if (name == null)
            {
                name = context.Principal.FindFirst(ClaimTypes.Name);
                if (name == null)
                {
                    throw new InvalidOperationException("A name claim is required");
                }

                var id = context.Principal.Identities.First();
                id.AddClaim(new Claim(JwtClaimTypes.Name, name.Value));
            }

            return Task.FromResult(0);
        }
    }
}
