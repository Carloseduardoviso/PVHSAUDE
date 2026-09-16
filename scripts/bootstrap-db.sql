USE [master];
GO

IF DB_ID(N'PVHSAUDE') IS NULL
BEGIN
    CREATE DATABASE [PVHSAUDE];
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.sql_logins WHERE name = N'pvhsaude_app')
BEGIN
    EXEC(N'CREATE LOGIN [pvhsaude_app] WITH PASSWORD = N''$(AppPassword)'', CHECK_POLICY = ON, CHECK_EXPIRATION = OFF;');
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.sql_logins WHERE name = N'pvhsaude_migrator')
BEGIN
    EXEC(N'CREATE LOGIN [pvhsaude_migrator] WITH PASSWORD = N''$(MigratorPassword)'', CHECK_POLICY = ON, CHECK_EXPIRATION = OFF;');
END;
GO

USE [PVHSAUDE];
GO

IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'pvhsaude_app')
BEGIN
    CREATE USER [pvhsaude_app] FOR LOGIN [pvhsaude_app];
END;

IF IS_ROLEMEMBER(N'db_datareader', N'pvhsaude_app') <> 1
BEGIN
    ALTER ROLE [db_datareader] ADD MEMBER [pvhsaude_app];
END;

IF IS_ROLEMEMBER(N'db_datawriter', N'pvhsaude_app') <> 1
BEGIN
    ALTER ROLE [db_datawriter] ADD MEMBER [pvhsaude_app];
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'pvhsaude_migrator')
BEGIN
    CREATE USER [pvhsaude_migrator] FOR LOGIN [pvhsaude_migrator];
END;

IF IS_ROLEMEMBER(N'db_owner', N'pvhsaude_migrator') <> 1
BEGIN
    ALTER ROLE [db_owner] ADD MEMBER [pvhsaude_migrator];
END;
GO
