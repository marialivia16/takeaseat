--=============================================================================
IF NOT EXISTS
		(
			SELECT	Schema_Name
			FROM	Information_Schema.Schemata
			WHERE	Schema_Name = 'App'
		) 
	BEGIN
		EXEC SP_ExecuteSQL N'CREATE SCHEMA [App] AUTHORIZATION [dbo]'
	END
go

--=============================================================================
IF NOT EXISTS (SELECT * FROM Sys.Objects WHERE Type = 'U' AND SCHEMA_NAME(SCHEMA_ID) = 'App' AND Name = N'Settings')
CREATE --       DROP
TABLE App.Settings
	(
	Setting_Key VarChar(256) NOT NULL PRIMARY KEY
	,Setting_Value NVarChar(1024) NOT NULL
	)
go