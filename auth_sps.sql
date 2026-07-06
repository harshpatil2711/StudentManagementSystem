-- Seed Roles
IF NOT EXISTS (SELECT 1 FROM Roles WHERE RoleName = 'Admin')
    INSERT INTO Roles (RoleName, CreatedBy) VALUES ('Admin', 'system');
IF NOT EXISTS (SELECT 1 FROM Roles WHERE RoleName = 'Admission Officer')
    INSERT INTO Roles (RoleName, CreatedBy) VALUES ('Admission Officer', 'system');
IF NOT EXISTS (SELECT 1 FROM Roles WHERE RoleName = 'Clerk')
    INSERT INTO Roles (RoleName, CreatedBy) VALUES ('Clerk', 'system');
GO

-- sp_RegisterUser
IF EXISTS (SELECT 1 FROM sys.objects WHERE type = 'P' AND name = 'sp_RegisterUser')
    DROP PROCEDURE sp_RegisterUser;
GO
CREATE PROCEDURE sp_RegisterUser
    @FirstName      VARCHAR(50),
    @LastName       VARCHAR(50),
    @Email          VARCHAR(100),
    @PhoneNumber    VARCHAR(15) = NULL,
    @Username       VARCHAR(50),
    @PasswordHash   VARCHAR(255),
    @RoleId         INT,
    @CreatedBy      VARCHAR(100),
    @Message        VARCHAR(200) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF EXISTS (SELECT 1 FROM Users WHERE Email = @Email AND IsActive = 1)
        BEGIN
            SET @Message = 'Error: Email already registered';
            RETURN;
        END

        IF EXISTS (SELECT 1 FROM UserCredentials WHERE Username = @Username AND IsActive = 1)
        BEGIN
            SET @Message = 'Error: Username already taken';
            RETURN;
        END

        BEGIN TRANSACTION;

        DECLARE @UserId INT;

        INSERT INTO Users (RoleId, FirstName, LastName, Email, PhoneNumber, CreatedBy)
        VALUES (@RoleId, @FirstName, @LastName, @Email, @PhoneNumber, @CreatedBy);

        SET @UserId = SCOPE_IDENTITY();

        INSERT INTO UserCredentials (UserId, Username, PasswordHash, CreatedBy)
        VALUES (@UserId, @Username, @PasswordHash, @CreatedBy);

        COMMIT TRANSACTION;

        SET @Message = 'Success: User registered successfully';
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        SET @Message = 'Error: ' + ERROR_MESSAGE();
    END CATCH
END;
GO

-- sp_AuthenticateUser
IF EXISTS (SELECT 1 FROM sys.objects WHERE type = 'P' AND name = 'sp_AuthenticateUser')
    DROP PROCEDURE sp_AuthenticateUser;
GO
CREATE PROCEDURE sp_AuthenticateUser
    @Username VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        uc.PasswordHash,
        u.UserId,
        uc.Username,
        u.RoleId,
        r.RoleName,
        u.FirstName,
        u.LastName,
        u.IsActive
    FROM UserCredentials uc
    INNER JOIN Users u ON u.UserId = uc.UserId
    INNER JOIN Roles r ON r.RoleId = u.RoleId
    WHERE uc.Username = @Username
      AND uc.IsActive = 1
      AND u.IsActive = 1;
END;
GO

-- sp_UpdateLastLogin
IF EXISTS (SELECT 1 FROM sys.objects WHERE type = 'P' AND name = 'sp_UpdateLastLogin')
    DROP PROCEDURE sp_UpdateLastLogin;
GO
CREATE PROCEDURE sp_UpdateLastLogin
    @UserId  INT,
    @Message VARCHAR(100) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        UPDATE UserCredentials
        SET LastLogin = GETDATE(),
            ModifiedOn = GETDATE(),
            ModifiedBy = (SELECT Username FROM UserCredentials WHERE UserId = @UserId)
        WHERE UserId = @UserId;

        SET @Message = 'Success';
    END TRY
    BEGIN CATCH
        SET @Message = 'Error: ' + ERROR_MESSAGE();
    END CATCH
END;
GO

-- sp_GetRoles
IF EXISTS (SELECT 1 FROM sys.objects WHERE type = 'P' AND name = 'sp_GetRoles')
    DROP PROCEDURE sp_GetRoles;
GO
CREATE PROCEDURE sp_GetRoles
AS
BEGIN
    SET NOCOUNT ON;
    SELECT RoleId, RoleName
    FROM Roles
    WHERE IsActive = 1
    ORDER BY RoleName;
END;
GO
