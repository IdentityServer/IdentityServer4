using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Core.Models;
using IdentityServer4.Core.Validation;
using IdentityServer4.Core.Extensions;
using IdentityServer4.Core.Services;

namespace IdentityServer4.Core.ResponseHandling
{
    class EndSessionResponseGenerator : IEndSessionResponseGenerator
    {
        private IMessageStore<SignOutRequest> _messageStore;

        public EndSessionResponseGenerator(IMessageStore<SignOutRequest> messageStore)
        {
            _messageStore = messageStore;
        }

        public async Task<Message<SignOutRequest>> ProcessAsync(EndSessionRequestValidationResult validationResult)
        {
            var signedOutRequest = new SignOutRequest();

            string redirectUri = null;

            if (validationResult.Client != null)
            {
                signedOutRequest.ClientId = validationResult.Client.ClientId;

                if (validationResult.PostLogoutUri != null)
                {
                    redirectUri = validationResult.PostLogoutUri;
                }
                else
                {
                    if (validationResult.Client.PostLogoutRedirectUris.Any())
                    {
                        redirectUri = validationResult.Client.PostLogoutRedirectUris.First();
                    }
                }

                if (validationResult.State.IsPresent())
                {
                    if (redirectUri.IsPresent())
                    {
                        redirectUri = redirectUri.AddQueryString("state=" + validationResult.State);
                    }
                }
            }

            var message = new Message<SignOutRequest>(signedOutRequest);
            message.ResponseUrl = redirectUri;

            await _messageStore.WriteAsync(message);

            return message;
        }
    }
}
