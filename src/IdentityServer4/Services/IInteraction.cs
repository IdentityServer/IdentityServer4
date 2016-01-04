// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4.Core.Extensions;
using IdentityServer4.Core.Models;
using Microsoft.AspNet.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer4.Core.Services
{
    public interface IInteraction<TRequest>
    {
        Task<TRequest> GetRequestAsync();
        Task ClearRequestAsync();
    }

    public abstract class Interaction<TRequest> : IInteraction<TRequest>
        where TRequest : class
    {
        string _requestId;
        Message<TRequest> _requestMessage;

        private readonly IMessageStore<TRequest> _requestStore;
        protected readonly HttpContext _context;

        protected internal Interaction(IMessageStore<TRequest> requestStore, HttpContext context)
        {
            _requestStore = requestStore;
            _context = context;
        }

        string RequestId
        {
            get
            {
                if (_requestId.IsMissing())
                {
                    if (_context.Request.Query.ContainsKey("id"))
                    {
                        _requestId = _context.Request.Query["id"].First();
                    }

                    if (_requestId.IsMissing())
                    {
                        throw new InvalidOperationException("HTTP request must have an 'id' query parameter.");
                    }
                }

                return _requestId;
            }
        }

        protected internal async Task<Message<TRequest>> GetRequestMessageAsync()
        {
            if (_requestMessage == null)
            {
                _requestMessage = await _requestStore.ReadAsync(RequestId);
            }

            return _requestMessage;
        }

        public async Task<TRequest> GetRequestAsync()
        {
            return (await GetRequestMessageAsync())?.Data;
        }

        public async Task ClearRequestAsync()
        {
            await _requestStore.DeleteAsync(RequestId);
        }
    }

    public interface IInteraction<TRequest, TResponse> : IInteraction<TRequest>
    {
        Task ProcessResponseAsync(TResponse response);
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

        public async Task ProcessResponseAsync(TResponse response)
        {
            var requestMessage = await GetRequestMessageAsync();
            await ClearRequestAsync();

            var message = new Message<TResponse>(response)
            {
                AuthorizeRequestParameters = requestMessage.AuthorizeRequestParameters,
            };
            await _responseStore.WriteAsync(message);

            var url = requestMessage.ReturnUrl.AddQueryString("id=" + message.Id);
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
