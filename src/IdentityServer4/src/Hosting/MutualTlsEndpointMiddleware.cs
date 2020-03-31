using System;
using System.Threading.Tasks;
using IdentityServer4.Configuration;
using IdentityServer4.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace IdentityServer4.Hosting
{
    /// <summary>
    ///     Middleware for re-writing the MTLS enabled endpoints to the standard protocol endpoints
    /// </summary>
    public class MutualTlsEndpointMiddleware
    {
        private readonly ILogger<MutualTlsEndpointMiddleware> _logger;
        private readonly RequestDelegate _next;
        private readonly IdentityServerOptions _options;

        /// <summary>
        ///     ctor
        /// </summary>
        /// <param name="next"></param>
        /// <param name="options"></param>
        /// <param name="logger"></param>
        public MutualTlsEndpointMiddleware(RequestDelegate next, IdentityServerOptions options,
            ILogger<MutualTlsEndpointMiddleware> logger)
        {
            _next = next;
            _options = options;
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task Invoke(HttpContext context, IAuthenticationSchemeProvider schemes)
        {
            if (_options.MutualTls.Enabled)
            {
                // domain-based MTLS
                if (_options.MutualTls.DomainName.IsPresent())
                {
                    // separate domain
                    if (_options.MutualTls.DomainName.Contains("."))
                    {
                        if (context.Request.Host.Host.Equals(_options.MutualTls.DomainName,
                            StringComparison.OrdinalIgnoreCase))
                        {
                            var result = await TriggerCertificateAuthentication(context);
                            if (!result.Succeeded)
                            {
                                return;
                            }
                        }
                    }
                    // sub-domain
                    else
                    {
                        if (context.Request.Host.Host.StartsWith(_options.MutualTls.DomainName + ".", StringComparison.OrdinalIgnoreCase))
                        {
                            var result = await TriggerCertificateAuthentication(context);
                            if (!result.Succeeded)
                            {
                                return;
                            }
                        }
                    }
                }
                // path based MTLS
                else if (context.Request.Path.StartsWithSegments(Constants.ProtocolRoutePaths.MtlsPathPrefix.EnsureLeadingSlash(), out var subPath))
                {
                    var result = await TriggerCertificateAuthentication(context);

                    if (result.Succeeded)
                    {
                        var path = Constants.ProtocolRoutePaths.ConnectPathPrefix +
                                   subPath.ToString().EnsureLeadingSlash();
                        path = path.EnsureLeadingSlash();

                        _logger.LogDebug("Rewriting MTLS request from: {oldPath} to: {newPath}",
                            context.Request.Path.ToString(), path);
                        context.Request.Path = path;
                    }
                    else
                    {
                        return;
                    }
                }
            }
            
            await _next(context);
        }

        private async Task<AuthenticateResult> TriggerCertificateAuthentication(HttpContext context)
        {
            var x509AuthResult =
                await context.AuthenticateAsync(_options.MutualTls.ClientCertificateAuthenticationScheme);

            if (!x509AuthResult.Succeeded)
            {
                _logger.LogDebug("MTLS authentication failed, error: {error}.",
                    x509AuthResult.Failure?.Message);
                await context.ForbidAsync(_options.MutualTls.ClientCertificateAuthenticationScheme);
            }

            return x509AuthResult;
        }
    }
}