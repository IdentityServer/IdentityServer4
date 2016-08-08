﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityModel;
using IdentityServer4.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace IdentityServer4.Models
{
    /// <summary>
    /// Models the parameters to identify a request for consent.
    /// </summary>
    public class ConsentRequest
    {
        public ConsentRequest(AuthorizationRequest request, string subject)
        {
            ClientId = request.ClientId;
            Nonce = request.Nonce;
            ScopesRequested = request.ScopesRequested;
            Subject = subject;
        }

        public ConsentRequest(NameValueCollection parameters, string subject)
        {
            ClientId = parameters[IdentityModel.OidcConstants.AuthorizeRequest.ClientId];
            Nonce = parameters[IdentityModel.OidcConstants.AuthorizeRequest.Nonce];
            ScopesRequested = parameters[IdentityModel.OidcConstants.AuthorizeRequest.Scope].ParseScopesString();
            Subject = subject;
        }

        public string ClientId { get; set; }
        public string Nonce { get; set; }
        public IEnumerable<string> ScopesRequested { get; set; }
        public string Subject { get; set; }

        public string Id
        {
            get
            {
                var normalizedScopes = ScopesRequested?.OrderBy(x => x).Distinct().Aggregate((x, y) => x + "," + y);
                var value = String.Format("{0}:{1}:{2}:{3}",
                    ClientId,
                    Subject,
                    Nonce,
                    normalizedScopes);

                using (var sha = SHA256.Create())
                {
                    var bytes = Encoding.UTF8.GetBytes(value);
                    var hash = sha.ComputeHash(bytes);

                    return Base64Url.Encode(hash);
                }
            }
        }
    }
}
