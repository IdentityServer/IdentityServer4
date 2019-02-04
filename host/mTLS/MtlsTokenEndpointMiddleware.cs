using System.Threading.Tasks;
using idunno.Authentication.Certificate;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace Host.Mtls
{
    public class MtlsTokenEndpointMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _x509SchemeName;

        public MtlsTokenEndpointMiddleware(RequestDelegate next, string x509SchemeName = CertificateAuthenticationDefaults.AuthenticationScheme)
        {
            _next = next;
            _x509SchemeName = x509SchemeName;
        }

        public async Task Invoke(HttpContext context, IAuthenticationSchemeProvider schemes)
        {
            if (context.Request.Path == "/connect/token/mtls")
            {
                var x509AuthResult = await context.AuthenticateAsync(_x509SchemeName);
                if (!x509AuthResult.Succeeded)
                {
                    await context.ForbidAsync(_x509SchemeName);
                    return;
                }
                else
                {
                    // todo: update from auth result above
                    context.Items[MtlsConstants.X509CertificateItemKey] = context.Connection.ClientCertificate;
                    context.Request.Path = "/connect/token";
                }
            }

            await _next(context);
        }
    }
}
