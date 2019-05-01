﻿using Clients;
using IdentityModel.Client;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ConsoleIntrospectionClient
{
    public class Program
    {
        static IDiscoveryCache _cache = new DiscoveryCache(Constants.Authority);

        static async Task Main()
        {
            Console.Title = "Console Introspection Client";

            var response = await RequestTokenAsync();
            await IntrospectAsync(response.AccessToken);
        }

        static async Task<TokenResponse> RequestTokenAsync()
        {
            var disco = await _cache.GetAsync();
            if (disco.IsError) throw new Exception(disco.Error);

            var client = new HttpClient();
            var response = await client.RequestPasswordTokenAsync(new PasswordTokenRequest
            {
                Address = disco.TokenEndpoint,

                ClientId = "roclient.reference",
                ClientSecret = "secret",

                UserName = "bob",
                Password = "bob",
                Scope = "api1 api2.read_only"
            });

            if (response.IsError) throw new Exception(response.Error);
            return response;
        }

        private static async Task IntrospectAsync(string accessToken)
        {
            var disco = await _cache.GetAsync();
            if (disco.IsError) throw new Exception(disco.Error);

            var client = new HttpClient();
            var result = await client.IntrospectTokenAsync(new TokenIntrospectionRequest
            {
                Address = disco.IntrospectionEndpoint,

                ClientId = "api1",
                ClientSecret = "secret",
                Token = accessToken
            });

            if (result.IsError)
            {
                Console.WriteLine(result.Error);
            }
            else
            {
                if (result.IsActive)
                {
                    result.Claims.ToList().ForEach(c => Console.WriteLine("{0}: {1}",
                        c.Type, c.Value));
                }
                else
                {
                    Console.WriteLine("token is not active");
                }
            }
        }
    }
}