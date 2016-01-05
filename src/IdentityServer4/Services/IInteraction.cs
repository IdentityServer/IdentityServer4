// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Core.Extensions;
using IdentityServer4.Core.Models;
using Microsoft.AspNet.Http;
using System.Threading.Tasks;

namespace IdentityServer4.Core.Services
{
    public interface IInteraction<TRequest>
    {
        Task<TRequest> GetRequestAsync(string requestId);
        Task ClearRequestAsync(string requestId);
    }

    public abstract class Interaction<TRequest> : IInteraction<TRequest>
        where TRequest : class
    {
        private readonly IMessageStore<TRequest> _requestStore;
        protected readonly HttpContext _context;

        protected internal Interaction(IMessageStore<TRequest> requestStore, HttpContext context)
        {
            _requestStore = requestStore;
            _context = context;
        }

        protected internal async Task<Message<TRequest>> GetRequestMessageAsync(string requestId)
        {
            return await _requestStore.ReadAsync(requestId);
        }

        public async Task<TRequest> GetRequestAsync(string requestId)
        {
            return (await GetRequestMessageAsync(requestId))?.Data;
        }

        public async Task ClearRequestAsync(string requestId)
        {
            await _requestStore.DeleteAsync(requestId);
        }
    }

    public interface IInteraction<TRequest, TResponse> : IInteraction<TRequest>
    {
        Task ProcessResponseAsync(string requestId, TResponse response);
    }

    public abstract class Interaction<TRequest, TResponse> : Interaction<TRequest>, IInteraction<TRequest, TResponse>
        where TRequest : class
    {
        private readonly IMessageStore<TResponse> _responseStore;

        protected internal Interaction(
            IMessageStore<TRequest> requestStore,
            IMessageStore<TResponse> responseStore,
            HttpContext context
        )
            : base(requestStore, context)
        {
            _responseStore = responseStore;
        }

        public async Task ProcessResponseAsync(string requestId, TResponse response)
        {
            var requestMessage = await GetRequestMessageAsync(requestId);
            await ClearRequestAsync(requestId);

            var message = new Message<TResponse>(response)
            {
                AuthorizeRequestParameters = requestMessage.AuthorizeRequestParameters,
            };
            await _responseStore.WriteAsync(message);

            var url = requestMessage.ResponseUrl.AddQueryString("id=" + message.Id);
            _context.Response.Redirect(url);
        }
    }

    public class SignInInteraction : Interaction<SignInRequest, SignInResponse>
    {
        public SignInInteraction(IMessageStore<SignInRequest> requestStore, IMessageStore<SignInResponse> responseStore, IHttpContextAccessor contextAccessor)
            : base(requestStore, responseStore, contextAccessor.HttpContext)
        {
        }
    }

    public class SignOutInteraction : Interaction<SignOutRequest, SignOutResponse>
    {
        public SignOutInteraction(IMessageStore<SignOutRequest> requestStore, IMessageStore<SignOutResponse> responseStore, IHttpContextAccessor contextAccessor)
            : base(requestStore, responseStore, contextAccessor.HttpContext)
        {
        }
    }

    public class ConsentInteraction : Interaction<ConsentRequest, ConsentResponse>
    {
        public ConsentInteraction(IMessageStore<ConsentRequest> requestStore, IMessageStore<ConsentResponse> responseStore, IHttpContextAccessor contextAccessor)
            : base(requestStore, responseStore, contextAccessor.HttpContext)
        {
        }
    }

    public class ErrorInteraction : Interaction<ErrorMessage>
    {
        public ErrorInteraction(IMessageStore<ErrorMessage> requestStore, IHttpContextAccessor contextAccessor)
            : base(requestStore, contextAccessor.HttpContext)
        {
        }
    }
}
