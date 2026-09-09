BEGIN TRANSACTION;
CREATE TABLE [Usuario] (
    [Id] uniqueidentifier NOT NULL,
    [NomeCompleto] nvarchar(200) NOT NULL,
    [Email] nvarchar(254) NOT NULL,
    [EmailNormalizado] nvarchar(254) NOT NULL,
    [SenhaHash] nvarchar(512) NOT NULL,
    [Role] int NOT NULL,
    CONSTRAINT [PK_Usuario] PRIMARY KEY ([Id])
);

CREATE UNIQUE INDEX [IX_Usuario_EmailNormalizado] ON [Usuario] ([EmailNormalizado]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260909132533_CadastroUsuarios', N'10.0.11');

COMMIT;
GO

