﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Collections.Generic;
using IdentityModel;
using IdentityServer4.Extensions;
using IdentityServer4.Validation;

namespace IdentityServer4.Logging.Models
{
    internal class EndSessionRequestValidationLog
    {
        public string ClientId { get; set; }
        public string ClientName { get; set; }
        public string SubjectId { get; set; }

        public string PostLogOutUri { get; set; }
        public string State { get; set; }

        public Dictionary<string, string> Raw { get; set; }

        public EndSessionRequestValidationLog(ValidatedEndSessionRequest request)
        {
            Raw = request.Raw.ToScrubbedDictionary(OidcConstants.EndSessionRequest.IdTokenHint);

            SubjectId = "unknown";
            if (request.Subject != null)
            {
                var subjectClaim = request.Subject.FindFirst(JwtClaimTypes.Subject);
                if (subjectClaim != null)
                {
                    SubjectId = subjectClaim.Value;
                }
            }

            if (request.Client != null)
            {
                ClientId = request.Client.ClientId;
                ClientName = request.Client.ClientName;
            }

            PostLogOutUri = request.PostLogOutUri;
            State = request.State;
        }

        public override string ToString()
        {
            return LogSerializer.Serialize(this);
        }
    }
}