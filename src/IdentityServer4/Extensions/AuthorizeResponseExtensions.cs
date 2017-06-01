﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Extensions;
using IdentityServer4.ResponseHandling;
using System.Collections.Specialized;

namespace IdentityServer4.Models
{
    internal static class AuthorizeResponseExtensions
    {
        public static NameValueCollection ToNameValueCollection(this AuthorizeResponse response)
        {
            var collection = new NameValueCollection();

            if (response.IsError)
            {
                if (response.Error.IsPresent())
                {
                    collection.Add("error", response.Error);
                }
                if (response.ErrorDescription.IsPresent())
                {
                    collection.Add("error_description", response.ErrorDescription);
                }
            }
            else
            {
                if (response.Code.IsPresent())
                {
                    collection.Add("code", response.Code);
                }

                if (response.IdentityToken.IsPresent())
                {
                    collection.Add("id_token", response.IdentityToken);
                }

                if (response.AccessToken.IsPresent())
                {
                    collection.Add("access_token", response.AccessToken);
                    collection.Add("token_type", "Bearer");
                    collection.Add("expires_in", response.AccessTokenLifetime.ToString());
                }

                if (response.Scope.IsPresent())
                {
                    collection.Add("scope", response.Scope);
                }
            }

            if (response.State.IsPresent())
            {
                collection.Add("state", response.State);
            }
            
            if (response.SessionState.IsPresent())
            {
                collection.Add("session_state", response.SessionState);
            }

            return collection;
        }
    }
}