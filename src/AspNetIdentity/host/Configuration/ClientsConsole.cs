// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Collections.Generic;
using IdentityServer4;
using IdentityServer4.Models;

namespace Host.Configuration
{
    public static class ClientsConsole
    {
        public static IEnumerable<Client> Get()
        {
            return new List<Client>
            {
                ///////////////////////////////////////////////////////////////
                // Console-based Client
                ///////////////////////////////////////////////////////////////


                ///////////////////////////////////////////
                // Console Client Credentials Flow Sample
                //////////////////////////////////////////
                new Client
                {
                    ClientId = "client",
                    ClientSecrets = {new Secret("secret".Sha256())},
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    AllowedScopes = { "scope1", "scope2", IdentityServerConstants.LocalApi.ScopeName}
                },
                
                ///////////////////////////////////////////
                // Console Structured Scope Sample
                //////////////////////////////////////////
                new Client
                {
                    ClientId = "parameterized.client",
                    ClientSecrets = {new Secret("secret".Sha256())},
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    AllowedScopes = { "transaction" }
                },

                ///////////////////////////////////////////
                // X509 mTLS Client
                //////////////////////////////////////////
                new Client
                {
                    ClientId = "mtls",
                    ClientSecrets =
                    {
                        // new Secret(@"CN=mtls.test, OU=ROO\ballen@roo, O=mkcert development certificate", "mtls.test")
                        // {
                        //     Type = SecretTypes.X509CertificateName
                        // },
                        new Secret("5D9E9B6B333CD42C99D1DE6175CC0F3EF99DDF68", "mtls.test")
                        {
                            Type = IdentityServerConstants.SecretTypes.X509CertificateThumbprint
                        },
                    },
                    
                    AccessTokenType = AccessTokenType.Jwt,
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    AllowedScopes = { "scope1", "scope2" }
                },

                ///////////////////////////////////////////
                // Console Client Credentials Flow with client JWT assertion
                //////////////////////////////////////////
                new Client
                {
                    ClientId = "client.jwt",
                    ClientSecrets =
                    {
                        new Secret
                        {
                            Type = IdentityServerConstants.SecretTypes.X509CertificateBase64,
                            Value =
                                "MIIEgTCCAumgAwIBAgIQDMMu7l/umJhfEbzJMpcttzANBgkqhkiG9w0BAQsFADCBkzEeMBwGA1UEChMVbWtjZXJ0IGRldmVsb3BtZW50IENBMTQwMgYDVQQLDCtkb21pbmlja0Bkb21icDE2LmZyaXR6LmJveCAoRG9taW5pY2sgQmFpZXIpMTswOQYDVQQDDDJta2NlcnQgZG9taW5pY2tAZG9tYnAxNi5mcml0ei5ib3ggKERvbWluaWNrIEJhaWVyKTAeFw0xOTA2MDEwMDAwMDBaFw0zMDAxMDMxMjM0MDdaMHAxJzAlBgNVBAoTHm1rY2VydCBkZXZlbG9wbWVudCBjZXJ0aWZpY2F0ZTE0MDIGA1UECwwrZG9taW5pY2tAZG9tYnAxNi5mcml0ei5ib3ggKERvbWluaWNrIEJhaWVyKTEPMA0GA1UEAxMGY2xpZW50MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAvNtpipaS8k1zA6w0Aoy8U4l+8zM4jHhhblExf3PULrMR6RauxniTki8p+P8CsZT4V8A4qo+JwsgpLIHrVQrbt9DEhHfBKzxwHqt+GoHt7byTfTtp8A/5nLhYc/5CW4HiR194gVx5+HAlvt+BriMTb1czvTf+H20dj41yUPsN7nMdyRLF+uXapQYMLYnq2BJIDq83mqGwojHk7d+N6GwoO95jlyas7KSoj8/FvfbaqkRNx0446hqPOzFHKc8er8K5VrLp6tVjh8ZJyY0F0dKgx6yWITsL54ctbj/cCyfuGjWEMbS2XXgc+x/xQMnmpfhK1qQAUn9jg5EzF9n6mQomOwIDAQABo3MwcTAOBgNVHQ8BAf8EBAMCBaAwHQYDVR0lBBYwFAYIKwYBBQUHAwIGCCsGAQUFBwMBMAwGA1UdEwEB/wQCMAAwHwYDVR0jBBgwFoAUEMUlw41YsKZQVls3pEG6CrJk4O8wEQYDVR0RBAowCIIGY2xpZW50MA0GCSqGSIb3DQEBCwUAA4IBgQC0TjNY4Q3Wmw7ggamDImV6HUng3WbYGLYbbL2e3myBrjIxGd1Bi8ZyOu8qeUMIRAbZt2YsSX5S8kx0biaVg2zC+aO5eHhEWMwKB66huInXFjI4wtxZ22r+33fg1R0cLuEUePhftOWrbL0MS4YXVyn9HUMWO4WptG9PJdxNw1UbEB8nw3FkVOdAC9RGqiqalSK+E2UT/kUbTIQ1gPSdQ3nh52mre0H/T9+IRqiozJtNK/CQg4NuEV7rUXHnp7Fmigp6RIJ4TCozglspL341y0rV8M7npU1FYZC2UKNr4ed+GOO1n/sF3LbXDlPXwne99CVVn85wjDaevoR7Md0y2KwE9EggLYcViXNehx4YVv/BjfgqxW8NxiKAxP6kPOZE0XdBrZj2rmcDcGOXCzzYpcduKhFyTOpA0K5RNGC3j1KOUjPVlOtLvjASP7udBEYNfH3mgqXAgqNDOEKi2jG9LITv2IyGUsXhTAsKNJ6A6qiDBzDrvPAYDvsfabPq6tRTwjA="
                        },
                        new Secret
                        {
                            Type = IdentityServerConstants.SecretTypes.JsonWebKey,
                            Value =
                                "{'e':'AQAB','kid':'ZzAjSnraU3bkWGnnAqLapYGpTyNfLbjbzgAPbbW2GEA','kty':'RSA','n':'wWwQFtSzeRjjerpEM5Rmqz_DsNaZ9S1Bw6UbZkDLowuuTCjBWUax0vBMMxdy6XjEEK4Oq9lKMvx9JzjmeJf1knoqSNrox3Ka0rnxXpNAz6sATvme8p9mTXyp0cX4lF4U2J54xa2_S9NF5QWvpXvBeC4GAJx7QaSw4zrUkrc6XyaAiFnLhQEwKJCwUw4NOqIuYvYp_IXhw-5Ti_icDlZS-282PcccnBeOcX7vc21pozibIdmZJKqXNsL1Ibx5Nkx1F1jLnekJAmdaACDjYRLL_6n3W4wUp19UvzB1lGtXcJKLLkqB6YDiZNu16OSiSprfmrRXvYmvD8m6Fnl5aetgKw'}"
                        }
                    },
                    
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    AllowedScopes = { "scope1", "scope2" }
                },

                ///////////////////////////////////////////
                // Custom Grant Sample
                //////////////////////////////////////////
                new Client
                {
                    ClientId = "client.custom",
                    ClientSecrets = {new Secret("secret".Sha256())},
                    AllowedGrantTypes = {"custom", "custom.nosubject"},
                    AllowedScopes = { "scope1", "scope2" }
                },

                ///////////////////////////////////////////
                // Console Resource Owner Flow Sample
                //////////////////////////////////////////
                new Client
                {
                    ClientId = "roclient",
                    ClientSecrets = {new Secret("secret".Sha256())},
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                    AllowOfflineAccess = true,
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        "custom.profile",
                        "scope1", "scope2"
                    }
                },

                ///////////////////////////////////////////
                // Console Public Resource Owner Flow Sample
                //////////////////////////////////////////
                new Client
                {
                    ClientId = "roclient.public",
                    RequireClientSecret = false,
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                    AllowOfflineAccess = true,
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Email,
                        "scope1", "scope2"
                    }
                },

                ///////////////////////////////////////////
                // Console with PKCE Sample
                //////////////////////////////////////////
                new Client
                {
                    ClientId = "console.pkce",
                    ClientName = "Console with PKCE Sample",
                    RequireClientSecret = false,
                    AllowedGrantTypes = GrantTypes.Code,
                    RequirePkce = true,
                    RedirectUris = {"http://127.0.0.1"},
                    AllowOfflineAccess = true,
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        "scope1", "scope2"
                    }
                },
                ///////////////////////////////////////////
                // WinConsole with PKCE Sample
                //////////////////////////////////////////
                new Client
                {
                    ClientId = "winconsole",
                    ClientName = "Windows Console with PKCE Sample",
                    RequireClientSecret = false,
                    AllowedGrantTypes = GrantTypes.Code,
                    RequirePkce = true,
                    RedirectUris = {"sample-windows-client://callback"},
                    RequireConsent = false,
                    AllowOfflineAccess = true,
                    AllowedIdentityTokenSigningAlgorithms = {"ES256"},
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        "scope1", "scope2"
                    }
                },


                ///////////////////////////////////////////
                // Introspection Client Sample
                //////////////////////////////////////////
                new Client
                {
                    ClientId = "roclient.reference",
                    ClientSecrets = {new Secret("secret".Sha256())},
                    AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
                    AllowedScopes = {"scope1", "scope2"},
                    AccessTokenType = AccessTokenType.Reference
                },
                
                ///////////////////////////////////////////
                // Device Flow Sample
                //////////////////////////////////////////
                new Client
                {
                    ClientId = "device",
                    ClientName = "Device Flow Client",

                    AllowedGrantTypes = GrantTypes.DeviceFlow,
                    RequireClientSecret = false,

                    AllowOfflineAccess = true,

                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        "scope1", "scope2"
                    }
                }
            };
        }
    }
}