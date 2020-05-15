// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Models;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading.Tasks;

namespace IdentityServer4.Services
{
    /// <summary>
    /// Default JwtRequest client
    /// </summary>
    public class DefaultJwtRequestUriHttpClient : IJwtRequestUriHttpClient
    {
        private readonly HttpClient _client;
        private readonly ILogger<DefaultJwtRequestUriHttpClient> _logger;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="client">An HTTP client</param>
        /// <param name="loggerFactory">The logger factory</param>
        public DefaultJwtRequestUriHttpClient(HttpClient client, ILoggerFactory loggerFactory)
        {
            _client = client;
            _logger = loggerFactory.CreateLogger<DefaultJwtRequestUriHttpClient>();
        }


        /// <inheritdoc />
        public async Task<string> GetJwtAsync(string url, Client client)
        {
            var req = new HttpRequestMessage(HttpMethod.Get, url);
            req.Properties.Add(IdentityServerConstants.JwtRequestClientKey, client);

            var response = await _client.SendAsync(req);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                _logger.LogDebug("Success http response from jwt url {url}", url);

                // todo: check for content-type of "application/oauth.authz.req+jwt"?
                // this might break OIDC's 
                var json = await response.Content.ReadAsStringAsync();
                return json;
            }
                
            _logger.LogError("Invalid http status code {status} from jwt url {url}", response.StatusCode, url);

            return null;
        }
    }
}
