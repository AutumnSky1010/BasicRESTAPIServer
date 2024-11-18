CREATE TABLE [app].[users]
(
    [id] uniqueidentifier PRIMARY KEY NOT NULL,
    [name] nvarchar(50) NOT NULL,
    [registered_at] datetime NOT NULL DEFAULT CURRENT_TIMESTAMP
);