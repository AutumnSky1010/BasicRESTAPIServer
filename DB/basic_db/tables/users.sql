CREATE TABLE [app].[users]
(
    [id] int PRIMARY KEY IDENTITY(1,1) NOT NULL,
    [name] nvarchar(50) NOT NULL,
    [registered_at] datetime NOT NULL DEFAULT CURRENT_TIMESTAMP
);