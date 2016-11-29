// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityModel;
using System.Collections.Generic;
using System.Linq;

namespace IdentityServer4.Models
{
    /// <summary>
    /// Convenience class that defines standard identity resources.
    /// </summary>
    public static class IdentityResources
    {
        public class OpenId : IdentityResource
        {
            public OpenId()
            {
                Name = IdentityServerConstants.StandardScopes.OpenId;
                DisplayName = "Your user identifier";
                Required = true;
                UserClaims.Add(new UserClaim(JwtClaimTypes.Subject, alwaysInclude: true));
            }
        }

        public class Profile : IdentityResource
        {
            public Profile()
            {
                Name = IdentityServerConstants.StandardScopes.Profile;
                DisplayName = "User profile";
                Description = "Your user profile information (first name, last name, etc.)";
                Emphasize = true;
                UserClaims = Constants.ScopeToClaimsMapping[IdentityServerConstants.StandardScopes.Profile].Select(claim => new UserClaim(claim)).ToList();
            }
        }

        public class Email : IdentityResource
        {
            public Email()
            {
                Name = IdentityServerConstants.StandardScopes.Email;
                DisplayName = "Your email address";
                Emphasize = true;
                UserClaims = (Constants.ScopeToClaimsMapping[IdentityServerConstants.StandardScopes.Email].Select(claim => new UserClaim(claim)).ToList());
            }
        }

        public class Phone : IdentityResource
        {
            public Phone()
            {
                Name = IdentityServerConstants.StandardScopes.Phone;
                DisplayName = "Your phone number";
                Emphasize = true;
                UserClaims = (Constants.ScopeToClaimsMapping[IdentityServerConstants.StandardScopes.Phone].Select(claim => new UserClaim(claim)).ToList());
            }
        }

        public class Address : IdentityResource
        {
            public Address()
            {
                Name = IdentityServerConstants.StandardScopes.Address;
                DisplayName = "Your postal address";
                Emphasize = true;
                UserClaims = (Constants.ScopeToClaimsMapping[IdentityServerConstants.StandardScopes.Address].Select(claim => new UserClaim(claim)).ToList());
            }
        }
    }
}