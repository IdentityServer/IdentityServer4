﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Services;
using System.Threading.Tasks;
using IdentityServer4.Models;
using System.Security.Claims;
using System.Collections.Generic;
using System.Linq;

namespace IdentityServer4.UnitTests.Common
{
    public class MockProfileService : IProfileService
    {
        public ICollection<Claim> ProfileClaims { get; set; } = new HashSet<Claim>();
        public bool IsActive { get; set; } = true;

        public bool GetProfileWasCalled => ProfileContext != null;
        public ProfileDataRequestContext ProfileContext { get; set; }

        public bool IsActiveWasCalled => ActiveContext != null;
        public IsActiveContext ActiveContext { get; set; }

        public Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            ProfileContext = context;
            context.IssuedClaims = ProfileClaims.ToList();
            return Task.CompletedTask;
        }

        public Task IsActiveAsync(IsActiveContext context)
        {
            ActiveContext = context;
            context.IsActive = IsActive;
            return Task.CompletedTask;
        }
    }
}