﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using Microsoft.AspNetCore.Http;
using System.Linq;
using Microsoft.Net.Http.Headers;

#pragma warning disable 1591

namespace IdentityServer4.Extensions
{
    public static class HttpRequestExtensions
    {
        public static string GetCorsOrigin(this HttpRequest request)
        {
            var origin = request.Headers["Origin"].FirstOrDefault();
            var thisOrigin = request.Scheme + "://" + request.Host;

            // see if the Origin is different than this server's origin. if so
            // that indicates a proper CORS request. some browsers send Origin
            // on POST requests.
            if (origin != null && origin != thisOrigin)
            {
                return origin;
            }

            return null;
        }
        
        internal static bool HasApplicationFormContentType(this HttpRequest request)
        {
            if (request.ContentType is null) return false;
            
            if (MediaTypeHeaderValue.TryParse(request.ContentType, out var header))
            {
                // Content-Type: application/x-www-form-urlencoded; charset=utf-8
                return header.MediaType.Equals("application/x-www-form-urlencoded", StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }
    }
}