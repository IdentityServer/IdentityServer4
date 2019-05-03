using Clients;
using IdentityModel;
using IdentityModel.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ConsoleMTLSClient
{
    public class Program
    {
        public static async Task Main()
        {
            Console.Title = "Console mTLS Client";

            var response = await RequestTokenAsync();
            response.Show();

            Console.ReadLine();
            await CallServiceAsync(response.AccessToken);
        }

        static async Task<TokenResponse> RequestTokenAsync()
        {
            var handler = new HttpClientHandler();
            var cert = X509.CurrentUser.My.Thumbprint.Find("bf6e2ca4f07994430b86bf9d48833a33f27a5c24").Single();
            handler.ClientCertificates.Add(cert);

            var client = new HttpClient(handler);

            var disco = await client.GetDiscoveryDocumentAsync(Constants.Authority);
            if (disco.IsError) throw new Exception(disco.Error);

            var response = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = disco
                            .TryGetValue(OidcConstants.Discovery.MtlsEndpointAliases)
                            .Value<string>(OidcConstants.Discovery.TokenEndpoint)
                            .ToString(),

                ClientId = "mtls",
                Scope = "api1"
            });

            if (response.IsError) throw new Exception(response.Error);
            return response;
        }

        static async Task CallServiceAsync(string token)
        {
            var baseAddress = Constants.SampleApi;

            var handler = new HttpClientHandler();
            var cert = X509.CurrentUser.My.Thumbprint.Find("bf6e2ca4f07994430b86bf9d48833a33f27a5c24").Single();
            handler.ClientCertificates.Add(cert);

            var client = new HttpClient(handler)
            {
                BaseAddress = new Uri(baseAddress)
            };

            client.SetBearerToken(token);
            var response = await client.GetStringAsync("identity");

            "\n\nService claims:".ConsoleGreen();
            Console.WriteLine(JArray.Parse(response));
        }
    }
}
