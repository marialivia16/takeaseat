--=============================================================================
IF NOT EXISTS (SELECT * FROM Sys.Objects WHERE Type = 'U' AND SCHEMA_NAME(SCHEMA_ID) = 'dbo' AND Name = N'AspNetRoles')
CREATE TABLE [dbo].[AspNetRoles]
	(
	[Id] NVarChar(64) NOT NULL CONSTRAINT [PK_AspNetRoles] PRIMARY KEY,
	[Name] NVarChar(128) NOT NULL
	)
go

--=============================================================================
IF NOT EXISTS (SELECT * FROM Sys.Objects WHERE Type = 'U' AND SCHEMA_NAME(SCHEMA_ID) = 'dbo' AND Name = N'AspNetUsers')
CREATE TABLE [dbo].[AspNetUsers]
(
	[Id] NVarChar(64) NOT NULL CONSTRAINT [PK_AspNetUsers] PRIMARY KEY,
	[Email] VarChar(128) NULL,
	[EmailConfirmed] Bit NOT NULL,
	[PasswordHash] VarChar(128) NULL,
	[SecurityStamp] VarChar(64) NULL,
	[PhoneNumber] VarChar(64) NULL,
	[PhoneNumberConfirmed] Bit NOT NULL,
	[TwoFactorEnabled] Bit NOT NULL,
	[LockoutEndDateUtc] SmallDateTime NULL,
	[LockoutEnabled] Bit NOT NULL,
	[AccessFailedCount] Int NOT NULL,
	[UserName] NVarChar(128) NULL,
	[IsBlocked] Bit NOT NULL DEFAULT 0,

	[FirstName] NVarChar(64) NULL,
	[LastName] NVarChar(64) NULL,
	[NotificationsCount] Int NOT NULL DEFAULT 0
)
go

--=============================================================================
IF NOT EXISTS (SELECT * FROM Sys.Objects WHERE Type = 'U' AND SCHEMA_NAME(SCHEMA_ID) = 'dbo' AND Name = N'AspNetUserLogins')
CREATE TABLE [dbo].[AspNetUserLogins]
(
	[LoginProvider] NVarChar(128) NOT NULL,
	[ProviderKey] NVarChar(128) NOT NULL,
	[UserId] NVarChar(64) NOT NULL CONSTRAINT [FK_AspNetUserLogins_AspNetUsers] FOREIGN KEY REFERENCES [dbo].[AspNetUsers]([Id])
	
	CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY CLUSTERED ([LoginProvider] ASC, [ProviderKey] ASC, [UserId] ASC)
		WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
)
go

--=============================================================================
IF NOT EXISTS (SELECT * FROM Sys.Objects WHERE Type = 'U' AND SCHEMA_NAME(SCHEMA_ID) = 'dbo' AND Name = N'AspNetUserClaims')
CREATE TABLE [dbo].[AspNetUserClaims]
	(
	[Id] Int IDENTITY(-2147483648,1) NOT NULL CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY,
	[UserId] NVarChar(64) NULL CONSTRAINT [FK_AspNetUserClaims_AspNetUsers] FOREIGN KEY REFERENCES [dbo].[AspNetUsers]([Id]),
	[ClaimType] NVarChar(256) NULL,
	[ClaimValue] NVarChar(1024) NULL
	)
go

--=============================================================================
IF NOT EXISTS (SELECT * FROM Sys.Objects WHERE Type = 'U' AND SCHEMA_NAME(SCHEMA_ID) = 'dbo' AND Name = N'AspNetUserRoles')
CREATE TABLE [dbo].[AspNetUserRoles]
	(
	[UserId] NVarChar(64) NOT NULL,
	[RoleId] NVarChar(64) NOT NULL CONSTRAINT [FK_AspNetUserRoles_AspNetRoles] FOREIGN KEY REFERENCES [dbo].[AspNetRoles]([Id]) ON DELETE CASCADE,
	[IdentityUserId] NVarChar(64) NULL CONSTRAINT [FK_AspNetUserRoles_AspNetUsers] FOREIGN KEY REFERENCES [dbo].[AspNetUsers]([Id]),

	CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY CLUSTERED ([UserId] ASC, [RoleId] ASC)
		WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON)
	)
go