IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE type = 'U' AND name = 'RefreshTokens')
BEGIN
    CREATE TABLE RefreshTokens (
        Id          INT IDENTITY(1,1) PRIMARY KEY,
        UserId      INT NOT NULL,
        TokenHash   VARCHAR(64) NOT NULL,
        ExpiresAt   DATETIME NOT NULL,
        CreatedAt   DATETIME NOT NULL DEFAULT GETDATE(),
        RevokedAt   DATETIME NULL,
        DeviceName  VARCHAR(500) NULL,
        IpAddress   VARCHAR(45) NULL,
        FOREIGN KEY (UserId) REFERENCES Users(UserId)
    );
END
GO
