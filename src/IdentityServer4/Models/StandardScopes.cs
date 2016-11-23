// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityModel;
using System.Collections.Generic;
using System.Linq;

namespace IdentityServer4.Models
{
    /// <summary>
    /// Convenience class that defines standard identity scopes.
    /// </summary>
    public static class StandardScopes
    {
        /// <summary>
        /// All identity scopes.
        /// </summary>
        /// <value>
        /// All.
        /// </value>
        public static IEnumerable<IdentityResource> All => new[]
        {
            OpenId,
            Profile,
            Email,
            Phone,
            Address
        };

        /// <summary>
        /// All identity resources (always include claims in token).
        /// </summary>
        /// <value>
        /// All always include.
        /// </value>
        public static IEnumerable<IdentityResource> AllAlwaysInclude => new[]
        {
            OpenId,
            ProfileAlwaysInclude,
            EmailAlwaysInclude,
            PhoneAlwaysInclude,
            AddressAlwaysInclude
        };


        /// <summary>
        /// Gets the "openid" scope.
        /// </summary>
        /// <value>
        /// The open identifier.
        /// </value>
        public static IdentityResource OpenId => new IdentityResource
        {
            Name = Constants.StandardScopes.OpenId,
            DisplayName = "Your user identifier",
            Required = true,
            UserClaims = new List<ScopeClaim>
            {
                new ScopeClaim(JwtClaimTypes.Subject, alwaysInclude: true)
            }
        };

        /// <summary>
        /// Gets the "profile" scope.
        /// </summary>
        /// <value>
        /// The profile.
        /// </value>
        public static IdentityResource Profile
        {
            get
            {
                return new IdentityResource
                {
                    Name = Constants.StandardScopes.Profile,
                    DisplayName = "User profile",
                    Description = "Your user profile information (first name, last name, etc.)",
                    Emphasize = true,
                    UserClaims = (Constants.ScopeToClaimsMapping[Constants.StandardScopes.Profile].Select(claim => new ScopeClaim(claim)).ToList())
                };
            }
        }

        /// <summary>
        /// Gets the "profile" scope (always include claims in token).
        /// </summary>
        /// <value>
        /// The profile always include.
        /// </value>
        public static IdentityResource ProfileAlwaysInclude
        {
            get
            {
                return new IdentityResource
                {
                    Name = Constants.StandardScopes.Profile,
                    DisplayName = "User profile",
                    Description = "Your user profile information (first name, last name, etc.)",
                    Emphasize = true,
                    UserClaims = (Constants.ScopeToClaimsMapping[Constants.StandardScopes.Profile].Select(claim => new ScopeClaim(claim, alwaysInclude: true)).ToList())
                };
            }
        }

        /// <summary>
        /// Gets the "email" scope.
        /// </summary>
        /// <value>
        /// The email.
        /// </value>
        public static IdentityResource Email
        {
            get
            {
                return new IdentityResource
                {
                    Name = Constants.StandardScopes.Email,
                    DisplayName = "Your email address",
                    Emphasize = true,
                    UserClaims = (Constants.ScopeToClaimsMapping[Constants.StandardScopes.Email].Select(claim => new ScopeClaim(claim)).ToList())
                };
            }
        }

        /// <summary>
        /// Gets the "email" scope (always include claims in token).
        /// </summary>
        /// <value>
        /// The email always include.
        /// </value>
        public static IdentityResource EmailAlwaysInclude
        {
            get
            {
                return new IdentityResource
                {
                    Name = Constants.StandardScopes.Email,
                    DisplayName = "Your email address",
                    Emphasize = true,
                    UserClaims = (Constants.ScopeToClaimsMapping[Constants.StandardScopes.Email].Select(claim => new ScopeClaim(claim, alwaysInclude: true)).ToList())
                };
            }
        }

        /// <summary>
        /// Gets the "phone" scope.
        /// </summary>
        /// <value>
        /// The phone.
        /// </value>
        public static IdentityResource Phone
        {
            get
            {
                return new IdentityResource
                {
                    Name = Constants.StandardScopes.Phone,
                    DisplayName = "Your phone number",
                    Emphasize = true,
                    UserClaims = (Constants.ScopeToClaimsMapping[Constants.StandardScopes.Phone].Select(claim => new ScopeClaim(claim)).ToList())
                };
            }
        }

        /// <summary>
        /// Gets the "phone" scope (always include claims in token).
        /// </summary>
        /// <value>
        /// The phone always include.
        /// </value>
        public static IdentityResource PhoneAlwaysInclude
        {
            get
            {
                return new IdentityResource
                {
                    Name = Constants.StandardScopes.Phone,
                    DisplayName = "Your phone number",
                    Emphasize = true,
                    UserClaims = (Constants.ScopeToClaimsMapping[Constants.StandardScopes.Phone].Select(claim => new ScopeClaim(claim, alwaysInclude: true)).ToList())
                };
            }
        }

        /// <summary>
        /// Gets the "address" scope.
        /// </summary>
        /// <value>
        /// The address.
        /// </value>
        public static IdentityResource Address
        {
            get
            {
                return new IdentityResource
                {
                    Name = Constants.StandardScopes.Address,
                    DisplayName = "Your postal address",
                    Emphasize = true,
                    UserClaims = (Constants.ScopeToClaimsMapping[Constants.StandardScopes.Address].Select(claim => new ScopeClaim(claim)).ToList())
                };
            }
        }

        /// <summary>
        /// Gets the "address" scope (always include claims in token).
        /// </summary>
        /// <value>
        /// The address always include.
        /// </value>
        public static IdentityResource AddressAlwaysInclude
        {
            get
            {
                return new IdentityResource
                {
                    Name = Constants.StandardScopes.Address,
                    DisplayName = "Your postal address",
                    Emphasize = true,
                    UserClaims = (Constants.ScopeToClaimsMapping[Constants.StandardScopes.Address].Select(claim => new ScopeClaim(claim, alwaysInclude: true)).ToList())
                };
            }
        }

        /// <summary>
        /// Gets the "all_claims" scope.
        /// </summary>
        /// <value>
        /// All claims.
        /// </value>
        public static IdentityResource AllClaims => new IdentityResource
        {
            Name = Constants.StandardScopes.AllClaims,
            DisplayName = "All user information",
            Emphasize = true,
            IncludeAllClaimsForUser = true
        };

        /// <summary>
        /// Gets the "roles" scope.
        /// </summary>
        /// <value>
        /// The roles.
        /// </value>
        public static IdentityResource Roles => new IdentityResource
        {
            Name = Constants.StandardScopes.Roles,
            DisplayName = "User roles",
            Emphasize = true,
            UserClaims = new List<ScopeClaim> 
            {
                new ScopeClaim(JwtClaimTypes.Role)
            }
        };

        /// <summary>
        /// Gets the "roles" scope (always include claims in token).
        /// </summary>
        /// <value>
        /// The roles always include.
        /// </value>
        public static IdentityResource RolesAlwaysInclude => new IdentityResource
        {
            Name = Constants.StandardScopes.Roles,
            DisplayName = "User roles",
            Emphasize = true,
            UserClaims = new List<ScopeClaim>
            {
                new ScopeClaim(JwtClaimTypes.Role, alwaysInclude: true)
            }
        };

        // TODO: maybe make this a client flag
        /// <summary>
        /// Gets the "offline_access" scope.
        /// </summary>
        /// <value>
        /// The offline access.
        /// </value>
        //public static Scope OfflineAccess => new Scope
        //{
        //    Name = Constants.StandardScopes.OfflineAccess,
        //    DisplayName = "Offline access",
        //    Emphasize = true
        //};
    }
}