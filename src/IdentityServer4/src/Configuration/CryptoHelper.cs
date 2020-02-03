﻿using IdentityModel;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace IdentityServer4.Configuration
{
    /// <summary>
    /// Crypto helper
    /// </summary>
    public static class CryptoHelper
    {
        /// <summary>
        /// Creates a new RSA security key.
        /// </summary>
        /// <returns></returns>
        public static RsaSecurityKey CreateRsaSecurityKey(int keySize = 2048)
        {
            return new RsaSecurityKey(RSA.Create(keySize))
            {
                KeyId = CryptoRandom.CreateUniqueId(16)
            };
        }

        /// <summary>
        /// Creates a new ECDSA security key.
        /// </summary>
        /// <param name="curve">The name of the curve as defined in
        /// https://tools.ietf.org/html/rfc7518#section-6.2.1.1.</param>
        /// <returns></returns>
        public static ECDsaSecurityKey CreateECDsaSecurityKey(string curve = JsonWebKeyECTypes.P256)
        {
            return new ECDsaSecurityKey(ECDsa.Create(GetCurveFromCrvValue(curve)))
            {
                KeyId = CryptoRandom.CreateUniqueId(16)
            };
        }

        /// <summary>
        /// Creates an RSA security key.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public static RsaSecurityKey CreateRsaSecurityKey(RSAParameters parameters, string id)
        {
            var key = new RsaSecurityKey(parameters)
            {
                KeyId = id
            };

            return key;
        }

        /// <summary>
        /// Creates the hash for the various hash claims (e.g. c_hash, at_hash or s_hash).
        /// </summary>
        /// <param name="value">The value to hash.</param>
        /// <param name="tokenSigningAlgorithm">The token signing algorithm</param>
        /// <returns></returns>
        public static string CreateHashClaimValue(string value, string tokenSigningAlgorithm)
        {
            using (var sha = GetHashAlgorithmForSigningAlgorithm(tokenSigningAlgorithm))
            {
                var hash = sha.ComputeHash(Encoding.ASCII.GetBytes(value));
                var size = (sha.HashSize / 8) / 2;

                var leftPart = new byte[size];
                Array.Copy(hash, leftPart, size);

                return Base64Url.Encode(leftPart);
            }
        }

        /// <summary>
        /// Returns the matching hashing algorithm for a token signing algorithm
        /// </summary>
        /// <param name="signingAlgorithm">The signing algorithm</param>
        /// <returns></returns>
        public static HashAlgorithm GetHashAlgorithmForSigningAlgorithm(string signingAlgorithm)
        {
            var signingAlgorithmBits = int.Parse(signingAlgorithm.Substring(signingAlgorithm.Length - 3));

            return signingAlgorithmBits switch
            {
                256 => SHA256.Create(),
                384 => SHA384.Create(),
                512 => SHA512.Create(),
                _ => throw new InvalidOperationException($"Invalid signing algorithm: {signingAlgorithm}"),
            };
        }

        /// <summary>
        /// Returns the matching named curve for RFC 7518 crv value
        /// </summary>
        internal static ECCurve GetCurveFromCrvValue(string crv)
        {
            return crv switch
            {
                JsonWebKeyECTypes.P256 => ECCurve.NamedCurves.nistP256,
                JsonWebKeyECTypes.P384 => ECCurve.NamedCurves.nistP384,
                JsonWebKeyECTypes.P521 => ECCurve.NamedCurves.nistP521,
                _ => throw new InvalidOperationException($"Unsupported curve type of {crv}"),
            };
        }

        /// <summary>
        /// Return the matching RFC 7518 crv value for curve
        /// </summary>
        internal static string GetCrvValueFromCurve(ECCurve curve)
        {
            return curve.Oid.Value switch
            {
                Constants.CurveOids.P256 => JsonWebKeyECTypes.P256,
                Constants.CurveOids.P384 => JsonWebKeyECTypes.P384,
                Constants.CurveOids.P521 => JsonWebKeyECTypes.P521,
                _ => throw new InvalidOperationException($"Unsupported curve type of {curve.Oid.Value} - {curve.Oid.FriendlyName}"),
            };
        }

        internal static bool IsValidCurveForAlgorithm(ECDsaSecurityKey key, string algorithm)
        {
            var parameters = key.ECDsa.ExportParameters(false);

            if (algorithm == SecurityAlgorithms.EcdsaSha256 && parameters.Curve.Oid.Value != Constants.CurveOids.P256
                || algorithm == SecurityAlgorithms.EcdsaSha384 && parameters.Curve.Oid.Value != Constants.CurveOids.P384
                || algorithm == SecurityAlgorithms.EcdsaSha512 && parameters.Curve.Oid.Value != Constants.CurveOids.P521)
            {
                return false;
            }

            return true;
        }
        internal static bool IsValidCrvValueForAlgorithm(string crv)
        {
            return crv == JsonWebKeyECTypes.P256 ||
                   crv == JsonWebKeyECTypes.P384 ||
                   crv == JsonWebKeyECTypes.P521;
        }

        internal static string GetRsaSigningAlgorithmValue(IdentityServerConstants.RsaSigningAlgorithm value)
        {
            return value switch
            {
                IdentityServerConstants.RsaSigningAlgorithm.RS256 => SecurityAlgorithms.RsaSha256,
                IdentityServerConstants.RsaSigningAlgorithm.RS384 => SecurityAlgorithms.RsaSha384,
                IdentityServerConstants.RsaSigningAlgorithm.RS512 => SecurityAlgorithms.RsaSha512,

                IdentityServerConstants.RsaSigningAlgorithm.PS256 => SecurityAlgorithms.RsaSsaPssSha256,
                IdentityServerConstants.RsaSigningAlgorithm.PS384 => SecurityAlgorithms.RsaSsaPssSha384,
                IdentityServerConstants.RsaSigningAlgorithm.PS512 => SecurityAlgorithms.RsaSsaPssSha512,
                _ => throw new ArgumentException("Invalid RSA signing algorithm value", nameof(value)),
            };
        }

        internal static string GetECDsaSigningAlgorithmValue(IdentityServerConstants.ECDsaSigningAlgorithm value)
        {
            return value switch
            {
                IdentityServerConstants.ECDsaSigningAlgorithm.ES256 => SecurityAlgorithms.EcdsaSha256,
                IdentityServerConstants.ECDsaSigningAlgorithm.ES384 => SecurityAlgorithms.EcdsaSha384,
                IdentityServerConstants.ECDsaSigningAlgorithm.ES512 => SecurityAlgorithms.EcdsaSha512,
                _ => throw new ArgumentException("Invalid ECDsa signing algorithm value", nameof(value)),
            };
        }

        internal static X509Certificate2 FindCertificate(string name, StoreLocation location, NameType nameType)
        {
            X509Certificate2 certificate = null;

            if (location == StoreLocation.LocalMachine)
            {
                if (nameType == NameType.SubjectDistinguishedName)
                {
                    certificate = X509.LocalMachine.My.SubjectDistinguishedName.Find(name, validOnly: false).FirstOrDefault();
                }
                else if (nameType == NameType.Thumbprint)
                {
                    certificate = X509.LocalMachine.My.Thumbprint.Find(name, validOnly: false).FirstOrDefault();
                }
            }
            else
            {
                if (nameType == NameType.SubjectDistinguishedName)
                {
                    certificate = X509.CurrentUser.My.SubjectDistinguishedName.Find(name, validOnly: false).FirstOrDefault();
                }
                else if (nameType == NameType.Thumbprint)
                {
                    certificate = X509.CurrentUser.My.Thumbprint.Find(name, validOnly: false).FirstOrDefault();
                }
            }

            return certificate;
        }

        // used for serialization to temporary RSA key
        internal class TemporaryRsaKey
        {
            public string KeyId { get; set; }
            public RSAParameters Parameters { get; set; }
        }

        internal class RsaKeyContractResolver : DefaultContractResolver
        {
            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                var property = base.CreateProperty(member, memberSerialization);

                property.Ignored = false;

                return property;
            }
        }
    }

    /// <summary>
    /// Describes the string so we know what to search for in certificate store
    /// </summary>
    public enum NameType
    {
        /// <summary>
        /// subject distinguished name
        /// </summary>
        SubjectDistinguishedName,

        /// <summary>
        /// thumbprint
        /// </summary>
        Thumbprint
    }
}