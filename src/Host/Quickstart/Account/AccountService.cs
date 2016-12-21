// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Services;
using IdentityServer4.Stores;
using IdentityServer4.Test;

namespace IdentityServer4.Quickstart.UI
{
    public class AccountService
    {
        private readonly IClientStore _clientStore;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly TestUserStore _users;

        public AccountService(TestUserStore users, IIdentityServerInteractionService interaction, IClientStore clientStore)
        {
            _users = users;
            _interaction = interaction;
            _clientStore = clientStore;
        }
    }
}
