-- sp_SaveRefreshToken
IF EXISTS (SELECT 1 FROM sys.objects WHERE type = 'P' AND name = 'sp_SaveRefreshToken')
    DROP PROCEDURE sp_SaveRefreshToken;
GO
CREATE PROCEDURE sp_SaveRefreshToken
    @UserId     INT,
    @TokenHash  VARCHAR(64),
    @DeviceName VARCHAR(500) = NULL,
    @IpAddress  VARCHAR(45) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO RefreshTokens (UserId, TokenHash, ExpiresAt, DeviceName, IpAddress)
    VALUES (@UserId, @TokenHash, DATEADD(DAY, 7, GETDATE()), @DeviceName, @IpAddress);
END;
GO

-- sp_GetRefreshToken
IF EXISTS (SELECT 1 FROM sys.objects WHERE type = 'P' AND name = 'sp_GetRefreshToken')
    DROP PROCEDURE sp_GetRefreshToken;
GO
CREATE PROCEDURE sp_GetRefreshToken
    @TokenHash VARCHAR(64)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, UserId, TokenHash, ExpiresAt, CreatedAt, RevokedAt, DeviceName, IpAddress
    FROM RefreshTokens
    WHERE TokenHash = @TokenHash
      AND RevokedAt IS NULL
      AND ExpiresAt > GETDATE();
END;
GO

-- sp_RevokeRefreshToken
IF EXISTS (SELECT 1 FROM sys.objects WHERE type = 'P' AND name = 'sp_RevokeRefreshToken')
    DROP PROCEDURE sp_RevokeRefreshToken;
GO
CREATE PROCEDURE sp_RevokeRefreshToken
    @Id INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE RefreshTokens SET RevokedAt = GETDATE() WHERE Id = @Id;
END;
GO

-- sp_RevokeAllUserTokens
IF EXISTS (SELECT 1 FROM sys.objects WHERE type = 'P' AND name = 'sp_RevokeAllUserTokens')
    DROP PROCEDURE sp_RevokeAllUserTokens;
GO
CREATE PROCEDURE sp_RevokeAllUserTokens
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE RefreshTokens SET RevokedAt = GETDATE() WHERE UserId = @UserId AND RevokedAt IS NULL;
END;
GO

-- sp_GetUserById
IF EXISTS (SELECT 1 FROM sys.objects WHERE type = 'P' AND name = 'sp_GetUserById')
    DROP PROCEDURE sp_GetUserById;
GO
CREATE PROCEDURE sp_GetUserById
    @UserId INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT u.UserId, uc.Username, u.RoleId, r.RoleName, u.FirstName, u.LastName
    FROM Users u
    INNER JOIN UserCredentials uc ON uc.UserId = u.UserId
    INNER JOIN Roles r ON r.RoleId = u.RoleId
    WHERE u.UserId = @UserId AND u.IsActive = 1 AND uc.IsActive = 1;
END;
GO
