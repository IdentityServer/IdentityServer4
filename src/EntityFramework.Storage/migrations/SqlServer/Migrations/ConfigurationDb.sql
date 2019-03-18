﻿IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;

GO

CREATE TABLE [ApiResources] (
    [Id] int NOT NULL IDENTITY,
    [Enabled] bit NOT NULL,
    [Name] nvarchar(200) NOT NULL,
    [DisplayName] nvarchar(200) NULL,
    [Description] nvarchar(1000) NULL,
    [Created] datetime2 NOT NULL,
    [Updated] datetime2 NULL,
    [LastAccessed] datetime2 NULL,
    [NonEditable] bit NOT NULL,
    CONSTRAINT [PK_ApiResources] PRIMARY KEY ([Id])
);

GO

CREATE TABLE [Clients] (
    [Id] int NOT NULL IDENTITY,
    [Enabled] bit NOT NULL,
    [ClientId] nvarchar(200) NOT NULL,
    [ProtocolType] nvarchar(200) NOT NULL,
    [RequireClientSecret] bit NOT NULL,
    [ClientName] nvarchar(200) NULL,
    [Description] nvarchar(1000) NULL,
    [ClientUri] nvarchar(2000) NULL,
    [LogoUri] nvarchar(2000) NULL,
    [RequireConsent] bit NOT NULL,
    [AllowRememberConsent] bit NOT NULL,
    [AlwaysIncludeUserClaimsInIdToken] bit NOT NULL,
    [RequirePkce] bit NOT NULL,
    [AllowPlainTextPkce] bit NOT NULL,
    [AllowAccessTokensViaBrowser] bit NOT NULL,
    [FrontChannelLogoutUri] nvarchar(2000) NULL,
    [FrontChannelLogoutSessionRequired] bit NOT NULL,
    [BackChannelLogoutUri] nvarchar(2000) NULL,
    [BackChannelLogoutSessionRequired] bit NOT NULL,
    [AllowOfflineAccess] bit NOT NULL,
    [IdentityTokenLifetime] int NOT NULL,
    [AccessTokenLifetime] int NOT NULL,
    [AuthorizationCodeLifetime] int NOT NULL,
    [ConsentLifetime] int NULL,
    [AbsoluteRefreshTokenLifetime] int NOT NULL,
    [SlidingRefreshTokenLifetime] int NOT NULL,
    [RefreshTokenUsage] int NOT NULL,
    [UpdateAccessTokenClaimsOnRefresh] bit NOT NULL,
    [RefreshTokenExpiration] int NOT NULL,
    [AccessTokenType] int NOT NULL,
    [EnableLocalLogin] bit NOT NULL,
    [IncludeJwtId] bit NOT NULL,
    [AlwaysSendClientClaims] bit NOT NULL,
    [ClientClaimsPrefix] nvarchar(200) NULL,
    [PairWiseSubjectSalt] nvarchar(200) NULL,
    [Created] datetime2 NOT NULL,
    [Updated] datetime2 NULL,
    [LastAccessed] datetime2 NULL,
    [UserSsoLifetime] int NULL,
    [UserCodeType] nvarchar(100) NULL,
    [DeviceCodeLifetime] int NOT NULL,
    [NonEditable] bit NOT NULL,
    CONSTRAINT [PK_Clients] PRIMARY KEY ([Id])
);

GO

CREATE TABLE [IdentityResources] (
    [Id] int NOT NULL IDENTITY,
    [Enabled] bit NOT NULL,
    [Name] nvarchar(200) NOT NULL,
    [DisplayName] nvarchar(200) NULL,
    [Description] nvarchar(1000) NULL,
    [Required] bit NOT NULL,
    [Emphasize] bit NOT NULL,
    [ShowInDiscoveryDocument] bit NOT NULL,
    [Created] datetime2 NOT NULL,
    [Updated] datetime2 NULL,
    [NonEditable] bit NOT NULL,
    CONSTRAINT [PK_IdentityResources] PRIMARY KEY ([Id])
);

GO

CREATE TABLE [ApiClaims] (
    [Id] int NOT NULL IDENTITY,
    [Type] nvarchar(200) NOT NULL,
    [ApiResourceId] int NOT NULL,
    CONSTRAINT [PK_ApiClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ApiClaims_ApiResources_ApiResourceId] FOREIGN KEY ([ApiResourceId]) REFERENCES [ApiResources] ([Id]) ON DELETE CASCADE
);

GO

CREATE TABLE [ApiProperties] (
    [Id] int NOT NULL IDENTITY,
    [Key] nvarchar(250) NOT NULL,
    [Value] nvarchar(2000) NOT NULL,
    [ApiResourceId] int NOT NULL,
    CONSTRAINT [PK_ApiProperties] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ApiProperties_ApiResources_ApiResourceId] FOREIGN KEY ([ApiResourceId]) REFERENCES [ApiResources] ([Id]) ON DELETE CASCADE
);

GO

CREATE TABLE [ApiScopes] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(200) NOT NULL,
    [DisplayName] nvarchar(200) NULL,
    [Description] nvarchar(1000) NULL,
    [Required] bit NOT NULL,
    [Emphasize] bit NOT NULL,
    [ShowInDiscoveryDocument] bit NOT NULL,
    [ApiResourceId] int NOT NULL,
    CONSTRAINT [PK_ApiScopes] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ApiScopes_ApiResources_ApiResourceId] FOREIGN KEY ([ApiResourceId]) REFERENCES [ApiResources] ([Id]) ON DELETE CASCADE
);

GO

CREATE TABLE [ApiSecrets] (
    [Id] int NOT NULL IDENTITY,
    [Description] nvarchar(1000) NULL,
    [Value] nvarchar(4000) NOT NULL,
    [Expiration] datetime2 NULL,
    [Type] nvarchar(250) NOT NULL,
    [Created] datetime2 NOT NULL,
    [ApiResourceId] int NOT NULL,
    CONSTRAINT [PK_ApiSecrets] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ApiSecrets_ApiResources_ApiResourceId] FOREIGN KEY ([ApiResourceId]) REFERENCES [ApiResources] ([Id]) ON DELETE CASCADE
);

GO

CREATE TABLE [ClientClaims] (
    [Id] int NOT NULL IDENTITY,
    [Type] nvarchar(250) NOT NULL,
    [Value] nvarchar(250) NOT NULL,
    [ClientId] int NOT NULL,
    CONSTRAINT [PK_ClientClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ClientClaims_Clients_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [Clients] ([Id]) ON DELETE CASCADE
);

GO

CREATE TABLE [ClientCorsOrigins] (
    [Id] int NOT NULL IDENTITY,
    [Origin] nvarchar(150) NOT NULL,
    [ClientId] int NOT NULL,
    CONSTRAINT [PK_ClientCorsOrigins] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ClientCorsOrigins_Clients_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [Clients] ([Id]) ON DELETE CASCADE
);

GO

CREATE TABLE [ClientGrantTypes] (
    [Id] int NOT NULL IDENTITY,
    [GrantType] nvarchar(250) NOT NULL,
    [ClientId] int NOT NULL,
    CONSTRAINT [PK_ClientGrantTypes] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ClientGrantTypes_Clients_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [Clients] ([Id]) ON DELETE CASCADE
);

GO

CREATE TABLE [ClientIdPRestrictions] (
    [Id] int NOT NULL IDENTITY,
    [Provider] nvarchar(200) NOT NULL,
    [ClientId] int NOT NULL,
    CONSTRAINT [PK_ClientIdPRestrictions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ClientIdPRestrictions_Clients_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [Clients] ([Id]) ON DELETE CASCADE
);

GO

CREATE TABLE [ClientPostLogoutRedirectUris] (
    [Id] int NOT NULL IDENTITY,
    [PostLogoutRedirectUri] nvarchar(2000) NOT NULL,
    [ClientId] int NOT NULL,
    CONSTRAINT [PK_ClientPostLogoutRedirectUris] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ClientPostLogoutRedirectUris_Clients_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [Clients] ([Id]) ON DELETE CASCADE
);

GO

CREATE TABLE [ClientProperties] (
    [Id] int NOT NULL IDENTITY,
    [Key] nvarchar(250) NOT NULL,
    [Value] nvarchar(2000) NOT NULL,
    [ClientId] int NOT NULL,
    CONSTRAINT [PK_ClientProperties] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ClientProperties_Clients_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [Clients] ([Id]) ON DELETE CASCADE
);

GO

CREATE TABLE [ClientRedirectUris] (
    [Id] int NOT NULL IDENTITY,
    [RedirectUri] nvarchar(2000) NOT NULL,
    [ClientId] int NOT NULL,
    CONSTRAINT [PK_ClientRedirectUris] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ClientRedirectUris_Clients_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [Clients] ([Id]) ON DELETE CASCADE
);

GO

CREATE TABLE [ClientScopes] (
    [Id] int NOT NULL IDENTITY,
    [Scope] nvarchar(200) NOT NULL,
    [ClientId] int NOT NULL,
    CONSTRAINT [PK_ClientScopes] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ClientScopes_Clients_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [Clients] ([Id]) ON DELETE CASCADE
);

GO

CREATE TABLE [ClientSecrets] (
    [Id] int NOT NULL IDENTITY,
    [Description] nvarchar(2000) NULL,
    [Value] nvarchar(4000) NOT NULL,
    [Expiration] datetime2 NULL,
    [Type] nvarchar(250) NOT NULL,
    [Created] datetime2 NOT NULL,
    [ClientId] int NOT NULL,
    CONSTRAINT [PK_ClientSecrets] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ClientSecrets_Clients_ClientId] FOREIGN KEY ([ClientId]) REFERENCES [Clients] ([Id]) ON DELETE CASCADE
);

GO

CREATE TABLE [IdentityClaims] (
    [Id] int NOT NULL IDENTITY,
    [Type] nvarchar(200) NOT NULL,
    [IdentityResourceId] int NOT NULL,
    CONSTRAINT [PK_IdentityClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_IdentityClaims_IdentityResources_IdentityResourceId] FOREIGN KEY ([IdentityResourceId]) REFERENCES [IdentityResources] ([Id]) ON DELETE CASCADE
);

GO

CREATE TABLE [IdentityProperties] (
    [Id] int NOT NULL IDENTITY,
    [Key] nvarchar(250) NOT NULL,
    [Value] nvarchar(2000) NOT NULL,
    [IdentityResourceId] int NOT NULL,
    CONSTRAINT [PK_IdentityProperties] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_IdentityProperties_IdentityResources_IdentityResourceId] FOREIGN KEY ([IdentityResourceId]) REFERENCES [IdentityResources] ([Id]) ON DELETE CASCADE
);

GO

CREATE TABLE [ApiScopeClaims] (
    [Id] int NOT NULL IDENTITY,
    [Type] nvarchar(200) NOT NULL,
    [ApiScopeId] int NOT NULL,
    CONSTRAINT [PK_ApiScopeClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ApiScopeClaims_ApiScopes_ApiScopeId] FOREIGN KEY ([ApiScopeId]) REFERENCES [ApiScopes] ([Id]) ON DELETE CASCADE
);

GO

CREATE INDEX [IX_ApiClaims_ApiResourceId] ON [ApiClaims] ([ApiResourceId]);

GO

CREATE INDEX [IX_ApiProperties_ApiResourceId] ON [ApiProperties] ([ApiResourceId]);

GO

CREATE UNIQUE INDEX [IX_ApiResources_Name] ON [ApiResources] ([Name]);

GO

CREATE INDEX [IX_ApiScopeClaims_ApiScopeId] ON [ApiScopeClaims] ([ApiScopeId]);

GO

CREATE INDEX [IX_ApiScopes_ApiResourceId] ON [ApiScopes] ([ApiResourceId]);

GO

CREATE UNIQUE INDEX [IX_ApiScopes_Name] ON [ApiScopes] ([Name]);

GO

CREATE INDEX [IX_ApiSecrets_ApiResourceId] ON [ApiSecrets] ([ApiResourceId]);

GO

CREATE INDEX [IX_ClientClaims_ClientId] ON [ClientClaims] ([ClientId]);

GO

CREATE INDEX [IX_ClientCorsOrigins_ClientId] ON [ClientCorsOrigins] ([ClientId]);

GO

CREATE INDEX [IX_ClientGrantTypes_ClientId] ON [ClientGrantTypes] ([ClientId]);

GO

CREATE INDEX [IX_ClientIdPRestrictions_ClientId] ON [ClientIdPRestrictions] ([ClientId]);

GO

CREATE INDEX [IX_ClientPostLogoutRedirectUris_ClientId] ON [ClientPostLogoutRedirectUris] ([ClientId]);

GO

CREATE INDEX [IX_ClientProperties_ClientId] ON [ClientProperties] ([ClientId]);

GO

CREATE INDEX [IX_ClientRedirectUris_ClientId] ON [ClientRedirectUris] ([ClientId]);

GO

CREATE UNIQUE INDEX [IX_Clients_ClientId] ON [Clients] ([ClientId]);

GO

CREATE INDEX [IX_ClientScopes_ClientId] ON [ClientScopes] ([ClientId]);

GO

CREATE INDEX [IX_ClientSecrets_ClientId] ON [ClientSecrets] ([ClientId]);

GO

CREATE INDEX [IX_IdentityClaims_IdentityResourceId] ON [IdentityClaims] ([IdentityResourceId]);

GO

CREATE INDEX [IX_IdentityProperties_IdentityResourceId] ON [IdentityProperties] ([IdentityResourceId]);

GO

CREATE UNIQUE INDEX [IX_IdentityResources_Name] ON [IdentityResources] ([Name]);

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20181129175524_Config', N'2.1.4-rtm-31024');

GO

