﻿IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;

GO

CREATE TABLE [DeviceCodes] (
    [UserCode] nvarchar(200) NOT NULL,
    [DeviceCode] nvarchar(200) NOT NULL,
    [SubjectId] nvarchar(200) NULL,
    [ClientId] nvarchar(200) NOT NULL,
    [CreationTime] datetime2 NOT NULL,
    [Expiration] datetime2 NOT NULL,
    [Data] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_DeviceCodes] PRIMARY KEY ([UserCode])
);

GO

CREATE TABLE [PersistedGrants] (
    [Key] nvarchar(200) NOT NULL,
    [Type] nvarchar(50) NOT NULL,
    [SubjectId] nvarchar(200) NULL,
    [ClientId] nvarchar(200) NOT NULL,
    [CreationTime] datetime2 NOT NULL,
    [Expiration] datetime2 NULL,
    [Data] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_PersistedGrants] PRIMARY KEY ([Key])
);

GO

CREATE UNIQUE INDEX [IX_DeviceCodes_DeviceCode] ON [DeviceCodes] ([DeviceCode]);

GO

CREATE INDEX [IX_DeviceCodes_Expiration] ON [DeviceCodes] ([Expiration]);

GO

CREATE INDEX [IX_PersistedGrants_Expiration] ON [PersistedGrants] ([Expiration]);

GO

CREATE INDEX [IX_PersistedGrants_SubjectId_ClientId_Type] ON [PersistedGrants] ([SubjectId], [ClientId], [Type]);

GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20190906144224_Grants', N'3.0.0-preview9.19423.6');

GO

