--=============================================================================
IF NOT EXISTS (SELECT * FROM Sys.Objects WHERE Type = 'U' AND SCHEMA_NAME(SCHEMA_ID) = 'dbo' AND Name = N'Restaurants')
CREATE TABLE [dbo].[Restaurants] (
    [Id]			NVarChar(64)		NOT NULL CONSTRAINT [PK_Restaurants] PRIMARY KEY,
    [Name]			NVARCHAR (MAX)		NOT NULL,
    [Description]	NVARCHAR (MAX)		NULL,
    [Longitude]		DECIMAL (11, 8)		NOT NULL,
    [Latitude]		DECIMAL (10, 8)		NOT NULL,
	[PhoneNumber]	VarChar(64)			NULL,
	[WebAddress]	NVARCHAR (MAX)		NULL,
	[Verified]		NVarChar(8)			NOT NULL DEFAULT N'No'
)
go

--=============================================================================
IF NOT EXISTS (SELECT * FROM Sys.Objects WHERE Type = 'U' AND SCHEMA_NAME(SCHEMA_ID) = 'dbo' AND Name = N'Reservations')
CREATE TABLE [dbo].[Reservations] (
    [Id]				INT				IDENTITY (1, 1) NOT NULL CONSTRAINT [PK_Reservations] PRIMARY KEY,
    [UserId]			NVarChar(64)	NOT NULL CONSTRAINT [FK_Reservations_AspNetUsers] FOREIGN KEY REFERENCES [dbo].[AspNetUsers]([Id]),
	[RestaurantId]		NVarChar(64)	NOT NULL CONSTRAINT [FK_Reservations_Restaurants] FOREIGN KEY REFERENCES [dbo].[Restaurants]([Id]),
    [Duration]			INT				NOT NULL,
	[NumberOfGuests]	INT				NOT NULL,
	[DateAndTime]		DateTime		NOT NULL,
	[SentOn]			DateTime		NOT NULL,
	[Status]			NVarChar(32)	NOT NULL DEFAULT N'Pending' ,
	[UserId]			NVarChar(64)	NULL
)
go

--=============================================================================
IF NOT EXISTS (SELECT * FROM Sys.Objects WHERE Type = 'U' AND SCHEMA_NAME(SCHEMA_ID) = 'dbo' AND Name = N'Managers')
CREATE TABLE [dbo].[Managers] (
    [UserId]		NVarChar(64)		NOT NULL CONSTRAINT [PK_Managers] PRIMARY KEY,
	[RestaurantId]	NVarChar(64)		NOT NULL CONSTRAINT [FK_Managers_Restaurants] FOREIGN KEY REFERENCES [dbo].[Restaurants]([Id])
)
go

--=============================================================================
IF NOT EXISTS (SELECT * FROM Sys.Objects WHERE Type = 'U' AND SCHEMA_NAME(SCHEMA_ID) = 'dbo' AND Name = N'Tags')
CREATE TABLE [dbo].[Tags] (
    [Id]		INT					IDENTITY (1, 1) NOT NULL CONSTRAINT [PK_Tags] PRIMARY KEY,
    [Name]      NVARCHAR (MAX)		NOT NULL,
	[Count]		INT					NOT NULL DEFAULT 0
)
go

--=============================================================================
IF NOT EXISTS (SELECT * FROM Sys.Objects WHERE Type = 'U' AND SCHEMA_NAME(SCHEMA_ID) = 'dbo' AND Name = N'TagsForRestaurant')
CREATE TABLE [dbo].[TagsForRestaurant] (
    [Id]			INT				IDENTITY (1, 1) NOT NULL CONSTRAINT [PK_TagsForRestaurant] PRIMARY KEY,
    [TagId]			INT				NOT NULL CONSTRAINT [FK_TagsForRestaurant_Tags] FOREIGN KEY REFERENCES [dbo].Tags([Id]),
	[RestaurantId]	NVarChar(64)	NOT NULL CONSTRAINT [FK_TagsForRestaurant_Restaurants] FOREIGN KEY REFERENCES [dbo].[Restaurants]([Id]),
)
go

--=============================================================================
IF NOT EXISTS (SELECT * FROM Sys.Objects WHERE Type = 'U' AND SCHEMA_NAME(SCHEMA_ID) = 'dbo' AND Name = N'TagsForUser')
CREATE TABLE [dbo].[TagsForUser] (
    [Id]			INT				IDENTITY (1, 1) NOT NULL CONSTRAINT [PK_TagsForUser] PRIMARY KEY,
    [TagId]			INT				NOT NULL CONSTRAINT [FK_TagsForUser_Tags] FOREIGN KEY REFERENCES [dbo].Tags([Id]),
	[UserId]		NVarChar(64)	NOT NULL CONSTRAINT [FK_TagsForUser_AspNetUsers] FOREIGN KEY REFERENCES [dbo].[AspNetUsers]([Id]),
	[Count]			INT				NOT NULL DEFAULT 0
)
go

--=============================================================================
IF ((SELECT COUNT(*) FROM dbo.Tags) = 0)
BEGIN
INSERT INTO dbo.Tags (Name) VALUES('coffee')
INSERT INTO dbo.Tags (Name) VALUES('tea')
INSERT INTO dbo.Tags (Name) VALUES('pizza')
INSERT INTO dbo.Tags (Name) VALUES('romanian')
INSERT INTO dbo.Tags (Name) VALUES('international')
INSERT INTO dbo.Tags (Name) VALUES('italian')
INSERT INTO dbo.Tags (Name) VALUES('fast food')
INSERT INTO dbo.Tags (Name) VALUES('club')
INSERT INTO dbo.Tags (Name) VALUES('restaurant')
INSERT INTO dbo.Tags (Name) VALUES('bar')
INSERT INTO dbo.Tags (Name) VALUES('romantic')
INSERT INTO dbo.Tags (Name) VALUES('family')
INSERT INTO dbo.Tags (Name) VALUES('friends')
INSERT INTO dbo.Tags (Name) VALUES('chinese')
INSERT INTO dbo.Tags (Name) VALUES('jazz')
INSERT INTO dbo.Tags (Name) VALUES('creative')
INSERT INTO dbo.Tags (Name) VALUES('dance')
INSERT INTO dbo.Tags (Name) VALUES('garden')
END
go