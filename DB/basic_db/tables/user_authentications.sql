CREATE TABLE [app].[user_authentications]
(
    [id] int PRIMARY KEY IDENTITY(1,1) NOT NULL,
    [user_id] uniqueidentifier NOT NULL,
    [sign_in_id] nvarchar(100) UNIQUE NOT NULL,
    [password] nvarchar(128) NOT NULL,
    [refresh_token] nvarchar(128) DEFAULT NULL,
    [refresh_token_expiration] datetime2 DEFAULT NULL,
    CONSTRAINT [fk_user_authentications_users] FOREIGN KEY([user_id]) 
        REFERENCES [app].[users]([id]) ON DELETE CASCADE
);