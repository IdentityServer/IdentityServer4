using System.Threading.Tasks;
using IdentityServer4.Configuration;
using IdentityServer4.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace IdentityServer4.Hosting
{
    /// <summary>
    /// Middleware for re-writing the MTLS enabled endpoints to the standard protocol endpoints
    /// </summary>
    public class MutualTlsTokenEndpointMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IdentityServerOptions _options;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="next"></param>
        /// <param name="options"></param>
        public MutualTlsTokenEndpointMiddleware(RequestDelegate next, IdentityServerOptions options)
        {
            _next = next;
            _options = options;
        }

        /// <inheritdoc/>
        public async Task Invoke(HttpContext context, IAuthenticationSchemeProvider schemes)
        {
            if (_options.MutualTls.Enabled &&
                context.Request.Path == Constants.ProtocolRoutePaths.MtlsToken.EnsureLeadingSlash())
            {
                var x509AuthResult = await context.AuthenticateAsync(_options.MutualTls.ClientCertificateAuthenticationScheme);
                if (!x509AuthResult.Succeeded)
                {
                    await context.ForbidAsync(_options.MutualTls.ClientCertificateAuthenticationScheme);
                    return;
                }
                else
                {
                    // todo: decide how to get cert from auth response above. for now, just get from the connection
                    context.Items[IdentityServerConstants.MutualTls.X509CertificateItemKey] = context.Connection.ClientCertificate;
                    context.Request.Path = Constants.ProtocolRoutePaths.Token.EnsureLeadingSlash();
                }
            }

            await _next(context);
        }
    }
}