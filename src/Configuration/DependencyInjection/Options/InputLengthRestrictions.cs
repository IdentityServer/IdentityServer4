// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


namespace IdentityServer4.Configuration
{
    /// <summary>
    /// 
    /// </summary>
    public class InputLengthRestrictions
    {
        private const int Default = 100;

        /// <summary>
        /// Max length for client_id
        /// </summary>
        public int ClientId { get; set; } = Default;

        /// <summary>
        /// Max length for external client secrets
        /// </summary>
        public int ClientSecret { get; set; } = Default;

        /// <summary>
        /// Max length for scope
        /// </summary>
        public int Scope { get; set; } = 300;

        /// <summary>
        /// Max length for redirect_uri
        /// </summary>
        public int RedirectUri { get; set; } = 400;

        /// <summary>
        /// Max length for nonce
        /// </summary>
        public int Nonce { get; set; } = 300;

        /// <summary>
        /// Max length for ui_locale
        /// </summary>
        public int UiLocale { get; set; } = Default;

        /// <summary>
        /// Max length for login_hint
        /// </summary>
        public int LoginHint { get; set; } = Default;

        /// <summary>
        /// Max length for acr_values
        /// </summary>
        public int AcrValues { get; set; } = 300;

        /// <summary>
        /// Max length for grant_type
        /// </summary>
        public int GrantType { get; set; } = Default;

        /// <summary>
        /// Max length for username
        /// </summary>
        public int UserName { get; set; } = Default;

        /// <summary>
        /// Max length for password
        /// </summary>
        public int Password { get; set; } = Default;

        /// <summary>
        /// Max length for CSP reports
        /// </summary>
        public int CspReport { get; set; } = 2000;

        /// <summary>
        /// Max length for external identity provider name
        /// </summary>
        public int IdentityProvider { get; set; } = Default;

        /// <summary>
        /// Max length for external identity provider errors
        /// </summary>
        public int ExternalError { get; set; } = Default;

        /// <summary>
        /// Max length for authorization codes
        /// </summary>
        public int AuthorizationCode { get; set; } = Default;

        /// <summary>
        /// Max length for device codes
        /// </summary>
        public int DeviceCode { get; set; } = Default;

        /// <summary>
        /// Max length for refresh tokens
        /// </summary>
        public int RefreshToken { get; set; } = Default;

        /// <summary>
        /// Max length for token handles
        /// </summary>
        public int TokenHandle { get; set; } = Default;

        /// <summary>
        /// Max length for JWTs
        /// </summary>
        public int Jwt { get; set; } = 51200;

        /// <summary>
        /// Min length for the code challenge
        /// </summary>
        public int CodeChallengeMinLength { get; } = 43;

        /// <summary>
        /// Max length for the code challenge
        /// </summary>
        public int CodeChallengeMaxLength { get; } = 128;

        /// <summary>
        /// Min length for the code verifier
        /// </summary>
        public int CodeVerifierMinLength { get; } = 43;

        /// <summary>
        /// Max length for the code verifier
        /// </summary>
        public int CodeVerifierMaxLength { get; } = 128;
    }
}