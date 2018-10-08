// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using IdentityModel;
using IdentityServer.UnitTests.Validation.Setup;
using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using IdentityServer4.UnitTests.Validation;
using IdentityServer4.Validation;
using Xunit;

namespace IdentityServer.UnitTests.Validation
{
    public class DeviceCodeValidation
    {
        private const string Category = "Device code validation";

        private readonly IClientStore _clients = Factory.CreateClientStore();

        private readonly DeviceCode deviceCode = new DeviceCode
        {
            ClientId = "device_flow",
            IsAuthorized = true,
            Subject = new IdentityServerUser("bob").CreatePrincipal(),
            IsOpenId = true,
            Lifetime = 300,
            CreationTime = DateTime.UtcNow,
            AuthorizedScopes = new[] { "openid", "profile", "resource" }
        };

        [Fact]
        [Trait("Category", Category)]
        public async Task DeviceCode_Missing()
        {
            var client = await _clients.FindClientByIdAsync("device_flow");
            var service = Factory.CreateDeviceCodeService();

            var validator = Factory.CreateDeviceCodeValidator(service);

            var request = new ValidatedTokenRequest();
            request.SetClient(client);

            var context = new DeviceCodeValidationContext { DeviceCode = null, Request = request };

            await validator.ValidateAsync(context);

            context.Result.IsError.Should().BeTrue();
            context.Result.Error.Should().Be(OidcConstants.TokenErrors.InvalidGrant);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task DeviceCode_From_Different_Client()
        {
            var badActor = await _clients.FindClientByIdAsync("codeclient");
            var service = Factory.CreateDeviceCodeService();

            var handle = await service.StoreDeviceAuthorizationAsync(Guid.NewGuid().ToString(), deviceCode);

            var validator = Factory.CreateDeviceCodeValidator(service);

            var request = new ValidatedTokenRequest();
            request.SetClient(badActor);

            var context = new DeviceCodeValidationContext { DeviceCode = handle, Request = request };

            await validator.ValidateAsync(context);

            context.Result.IsError.Should().BeTrue();
            context.Result.Error.Should().Be(OidcConstants.TokenErrors.InvalidGrant);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Expired_DeviceCode()
        {
            deviceCode.CreationTime = DateTime.UtcNow.AddDays(-10);
            deviceCode.Lifetime = 300;

            var client = await _clients.FindClientByIdAsync("device_flow");
            var service = Factory.CreateDeviceCodeService();

            var handle = await service.StoreDeviceAuthorizationAsync(Guid.NewGuid().ToString(), deviceCode);

            var validator = Factory.CreateDeviceCodeValidator(service);

            var request = new ValidatedTokenRequest();
            request.SetClient(client);

            var context = new DeviceCodeValidationContext { DeviceCode = handle, Request = request };

            await validator.ValidateAsync(context);

            context.Result.IsError.Should().BeTrue();
            context.Result.Error.Should().Be(OidcConstants.TokenErrors.ExpiredToken);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Access_Denied()
        {
            deviceCode.AuthorizedScopes = new List<string>();

            var client = await _clients.FindClientByIdAsync("device_flow");
            var service = Factory.CreateDeviceCodeService();

            var handle = await service.StoreDeviceAuthorizationAsync(Guid.NewGuid().ToString(), deviceCode);

            var validator = Factory.CreateDeviceCodeValidator(service);

            var request = new ValidatedTokenRequest();
            request.SetClient(client);

            var context = new DeviceCodeValidationContext { DeviceCode = handle, Request = request };

            await validator.ValidateAsync(context);

            context.Result.IsError.Should().BeTrue();
            context.Result.Error.Should().Be(OidcConstants.TokenErrors.AccessDenied);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task DeviceCode_Not_Yet_Authorized()
        {
            deviceCode.IsAuthorized = false;

            var client = await _clients.FindClientByIdAsync("device_flow");
            var service = Factory.CreateDeviceCodeService();

            var handle = await service.StoreDeviceAuthorizationAsync(Guid.NewGuid().ToString(), deviceCode);

            var validator = Factory.CreateDeviceCodeValidator(service);

            var request = new ValidatedTokenRequest();
            request.SetClient(client);

            var context = new DeviceCodeValidationContext { DeviceCode = handle, Request = request };

            await validator.ValidateAsync(context);

            context.Result.IsError.Should().BeTrue();
            context.Result.Error.Should().Be(OidcConstants.TokenErrors.AuthorizationPending);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task DeviceCode_Missing_Subject()
        {
            deviceCode.Subject = null;

            var client = await _clients.FindClientByIdAsync("device_flow");
            var service = Factory.CreateDeviceCodeService();

            var handle = await service.StoreDeviceAuthorizationAsync(Guid.NewGuid().ToString(), deviceCode);

            var validator = Factory.CreateDeviceCodeValidator(service);

            var request = new ValidatedTokenRequest();
            request.SetClient(client);

            var context = new DeviceCodeValidationContext { DeviceCode = handle, Request = request };

            await validator.ValidateAsync(context);

            context.Result.IsError.Should().BeTrue();
            context.Result.Error.Should().Be(OidcConstants.TokenErrors.AuthorizationPending);
        }


        [Fact]
        [Trait("Category", Category)]
        public async Task User_Disabled()
        {
            var client = await _clients.FindClientByIdAsync("device_flow");
            var service = Factory.CreateDeviceCodeService();

            var handle = await service.StoreDeviceAuthorizationAsync(Guid.NewGuid().ToString(), deviceCode);

            var validator = Factory.CreateDeviceCodeValidator(service, new TestProfileService(false));

            var request = new ValidatedTokenRequest();
            request.SetClient(client);

            var context = new DeviceCodeValidationContext { DeviceCode = handle, Request = request };

            await validator.ValidateAsync(context);

            context.Result.IsError.Should().BeTrue();
            context.Result.Error.Should().Be(OidcConstants.TokenErrors.InvalidGrant);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task DeviceCode_Polling_Too_Fast()
        {
            var client = await _clients.FindClientByIdAsync("device_flow");
            var service = Factory.CreateDeviceCodeService();

            var handle = await service.StoreDeviceAuthorizationAsync(Guid.NewGuid().ToString(), deviceCode);

            var validator = Factory.CreateDeviceCodeValidator(service, throttlingService: new TestDeviceFlowThrottlingService(true));

            var request = new ValidatedTokenRequest();
            request.SetClient(client);

            var context = new DeviceCodeValidationContext { DeviceCode = handle, Request = request };

            await validator.ValidateAsync(context);

            context.Result.IsError.Should().BeTrue();
            context.Result.Error.Should().Be(OidcConstants.TokenErrors.SlowDown);
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_DeviceCode()
        {
            var client = await _clients.FindClientByIdAsync("device_flow");
            var service = Factory.CreateDeviceCodeService();

            var handle = await service.StoreDeviceAuthorizationAsync(Guid.NewGuid().ToString(), deviceCode);

            var validator = Factory.CreateDeviceCodeValidator(service);

            var request = new ValidatedTokenRequest();
            request.SetClient(client);

            var context = new DeviceCodeValidationContext {DeviceCode = handle, Request = request};

            await validator.ValidateAsync(context);
            
            context.Result.IsError.Should().BeFalse();
        }
    }
}