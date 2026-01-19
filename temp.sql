CREATE TABLE [AspNetRoles] (
    [Id] nvarchar(450) NOT NULL,
    [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [BoardGames] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(200) NOT NULL,
    [Description] nvarchar(1000) NOT NULL,
    [Genre] int NOT NULL,
    [Type] int NOT NULL,
    [Is18Plus] bit NOT NULL,
    [PhotoUrl] nvarchar(500) NULL,
    CONSTRAINT [PK_BoardGames] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [Persons] (
    [Id] int NOT NULL IDENTITY,
    [FirstName] nvarchar(100) NOT NULL,
    [LastName] nvarchar(100) NOT NULL,
    [Email] nvarchar(255) NOT NULL,
    [DateOfBirth] datetime2 NOT NULL,
    [Gender] int NOT NULL,
    [Address_Street] nvarchar(200) NOT NULL,
    [Address_HouseNumber] nvarchar(10) NOT NULL,
    [Address_PostalCode] nvarchar(max) NOT NULL,
    [Address_City] nvarchar(100) NOT NULL,
    [DietaryRequirements_IsLactoseFree] bit NOT NULL,
    [DietaryRequirements_IsNutFree] bit NOT NULL,
    [DietaryRequirements_IsVegetarian] bit NOT NULL,
    [DietaryRequirements_IsAlcoholFree] bit NOT NULL,
    CONSTRAINT [PK_Persons] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [AspNetRoleClaims] (
    [Id] int NOT NULL IDENTITY,
    [RoleId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [AspNetUsers] (
    [Id] nvarchar(450) NOT NULL,
    [PersonId] int NULL,
    [UserName] nvarchar(256) NULL,
    [NormalizedUserName] nvarchar(256) NULL,
    [Email] nvarchar(256) NULL,
    [NormalizedEmail] nvarchar(256) NULL,
    [EmailConfirmed] bit NOT NULL,
    [PasswordHash] nvarchar(max) NULL,
    [SecurityStamp] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    [PhoneNumber] nvarchar(max) NULL,
    [PhoneNumberConfirmed] bit NOT NULL,
    [TwoFactorEnabled] bit NOT NULL,
    [LockoutEnd] datetimeoffset NULL,
    [LockoutEnabled] bit NOT NULL,
    [AccessFailedCount] int NOT NULL,
    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUsers_Persons_PersonId] FOREIGN KEY ([PersonId]) REFERENCES [Persons] ([Id]) ON DELETE SET NULL
);
GO


CREATE TABLE [GameNights] (
    [Id] int NOT NULL IDENTITY,
    [Title] nvarchar(max) NOT NULL,
    [Description] nvarchar(max) NULL,
    [DateTime] datetime2 NOT NULL,
    [MaxPlayers] int NOT NULL,
    [Is18Plus] bit NOT NULL,
    [OrganizerId] int NOT NULL,
    [Address_Street] nvarchar(200) NOT NULL,
    [Address_HouseNumber] nvarchar(10) NOT NULL,
    [Address_PostalCode] nvarchar(max) NOT NULL,
    [Address_City] nvarchar(100) NOT NULL,
    [HasLactoseFreeOption] bit NOT NULL,
    [HasNutFreeOption] bit NOT NULL,
    [HasVegetarianOption] bit NOT NULL,
    [HasAlcoholFreeOption] bit NOT NULL,
    CONSTRAINT [PK_GameNights] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_GameNights_Persons_OrganizerId] FOREIGN KEY ([OrganizerId]) REFERENCES [Persons] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [AspNetUserClaims] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [AspNetUserLogins] (
    [LoginProvider] nvarchar(128) NOT NULL,
    [ProviderKey] nvarchar(128) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [AspNetUserRoles] (
    [UserId] nvarchar(450) NOT NULL,
    [RoleId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [AspNetUserTokens] (
    [UserId] nvarchar(450) NOT NULL,
    [LoginProvider] nvarchar(128) NOT NULL,
    [Name] nvarchar(128) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [GameNightBoardGames] (
    [GameNightId] int NOT NULL,
    [BoardGameId] int NOT NULL,
    CONSTRAINT [PK_GameNightBoardGames] PRIMARY KEY ([GameNightId], [BoardGameId]),
    CONSTRAINT [FK_GameNightBoardGames_BoardGames_BoardGameId] FOREIGN KEY ([BoardGameId]) REFERENCES [BoardGames] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_GameNightBoardGames_GameNights_GameNightId] FOREIGN KEY ([GameNightId]) REFERENCES [GameNights] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [GameNightPlayers] (
    [GameNightId] int NOT NULL,
    [PersonId] int NOT NULL,
    [RegisteredAt] datetime2 NOT NULL,
    CONSTRAINT [PK_GameNightPlayers] PRIMARY KEY ([GameNightId], [PersonId]),
    CONSTRAINT [FK_GameNightPlayers_GameNights_GameNightId] FOREIGN KEY ([GameNightId]) REFERENCES [GameNights] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_GameNightPlayers_Persons_PersonId] FOREIGN KEY ([PersonId]) REFERENCES [Persons] ([Id]) ON DELETE CASCADE
);
GO


CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
GO


CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;
GO


CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
GO


CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
GO


CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
GO


CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
GO


CREATE INDEX [IX_AspNetUsers_PersonId] ON [AspNetUsers] ([PersonId]);
GO


CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;
GO


CREATE INDEX [IX_GameNightBoardGames_BoardGameId] ON [GameNightBoardGames] ([BoardGameId]);
GO


CREATE INDEX [IX_GameNightPlayers_PersonId] ON [GameNightPlayers] ([PersonId]);
GO


CREATE INDEX [IX_GameNights_OrganizerId] ON [GameNights] ([OrganizerId]);
GO


CREATE UNIQUE INDEX [IX_Persons_Email] ON [Persons] ([Email]);
GO


