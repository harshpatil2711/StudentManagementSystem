-- ============================================================
-- Student Photo Upload: Database Changes
-- ============================================================

-- 1. Add PhotoPath column to Student table
IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID('Student') AND name = 'PhotoPath'
)
BEGIN
    ALTER TABLE Student ADD PhotoPath NVARCHAR(255) NULL;
END
GO

-- ============================================================
-- 2. Update sp_InsertStudent to accept @PhotoPath
-- ============================================================
-- NOTE: This drops and recreates the procedure.
-- Adjust parameter list if your existing sp_InsertStudent has additional parameters.

IF EXISTS (SELECT 1 FROM sys.procedures WHERE name = 'sp_InsertStudent')
    DROP PROCEDURE sp_InsertStudent;
GO

CREATE PROCEDURE sp_InsertStudent
    @StudentName    NVARCHAR(100),
    @DateOfBirth    DATE,
    @Email          NVARCHAR(150),
    @Phone          NVARCHAR(20),
    @Gender         NVARCHAR(10),
    @AdmissionYear  INT,
    @PhotoPath      NVARCHAR(255) = NULL,
    @CreatedBy      NVARCHAR(100),
    @LastModifiedBy NVARCHAR(100),
    @Message        NVARCHAR(200) OUTPUT,
    @NewStudentID   INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        INSERT INTO Student
            (StudentName, DateOfBirth, Email, Phone, Gender,
             AdmissionYear, IsActive, DateCreated, CreatedBy,
             DateLastModified, LastModifiedBy, PhotoPath)
        VALUES
            (@StudentName, @DateOfBirth, @Email, @Phone, @Gender,
             @AdmissionYear, 1, GETDATE(), @CreatedBy,
             GETDATE(), @LastModifiedBy, @PhotoPath);

        SET @NewStudentID = CAST(SCOPE_IDENTITY() AS INT);
        SET @Message = 'Student saved successfully. ID=' + CAST(@NewStudentID AS NVARCHAR(20));
    END TRY
    BEGIN CATCH
        SET @NewStudentID = -1;
        SET @Message = 'Error: ' + ERROR_MESSAGE();
    END CATCH
END
GO

-- ============================================================
-- 3. sp_GetStudentById
-- ============================================================
IF EXISTS (SELECT 1 FROM sys.procedures WHERE name = 'sp_GetStudentById')
    DROP PROCEDURE sp_GetStudentById;
GO

CREATE PROCEDURE sp_GetStudentById
    @StudentID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT StudentID, StudentName, DateOfBirth, Email, Phone,
           Gender, AdmissionYear, IsActive, DateCreated, CreatedBy,
           DateLastModified, LastModifiedBy, PhotoPath
    FROM Student
    WHERE StudentID = @StudentID;
END
GO

-- ============================================================
-- 4. sp_GetAllStudents
-- ============================================================
IF EXISTS (SELECT 1 FROM sys.procedures WHERE name = 'sp_GetAllStudents')
    DROP PROCEDURE sp_GetAllStudents;
GO

CREATE PROCEDURE sp_GetAllStudents
AS
BEGIN
    SET NOCOUNT ON;
    SELECT StudentID, StudentName, DateOfBirth, Email, Phone,
           Gender, AdmissionYear, IsActive, DateCreated, CreatedBy,
           DateLastModified, LastModifiedBy, PhotoPath
    FROM Student
    WHERE IsActive = 1
    ORDER BY StudentName;
END
GO

-- ============================================================
-- 5. sp_UpdateStudent
-- ============================================================
IF EXISTS (SELECT 1 FROM sys.procedures WHERE name = 'sp_UpdateStudent')
    DROP PROCEDURE sp_UpdateStudent;
GO

CREATE PROCEDURE sp_UpdateStudent
    @StudentID      INT,
    @StudentName    NVARCHAR(100),
    @DateOfBirth    DATE,
    @Email          NVARCHAR(150),
    @Phone          NVARCHAR(20),
    @Gender         NVARCHAR(10),
    @AdmissionYear  INT,
    @PhotoPath      NVARCHAR(255) = NULL,
    @LastModifiedBy NVARCHAR(100),
    @Message        NVARCHAR(200) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        UPDATE Student
        SET StudentName    = @StudentName,
            DateOfBirth    = @DateOfBirth,
            Email          = @Email,
            Phone          = @Phone,
            Gender         = @Gender,
            AdmissionYear  = @AdmissionYear,
            DateLastModified = GETDATE(),
            LastModifiedBy = @LastModifiedBy,
            PhotoPath      = @PhotoPath
        WHERE StudentID = @StudentID;

        SET @Message = 'Student updated successfully.';
    END TRY
    BEGIN CATCH
        SET @Message = 'Error: ' + ERROR_MESSAGE();
    END CATCH
END
GO

-- ============================================================
-- 6. sp_DeleteStudent (soft delete)
-- ============================================================
IF EXISTS (SELECT 1 FROM sys.procedures WHERE name = 'sp_DeleteStudent')
    DROP PROCEDURE sp_DeleteStudent;
GO

CREATE PROCEDURE sp_DeleteStudent
    @StudentID INT,
    @Message   NVARCHAR(200) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        UPDATE Student
        SET IsActive = 0,
            DateLastModified = GETDATE()
        WHERE StudentID = @StudentID;

        SET @Message = 'Student deleted successfully.';
    END TRY
    BEGIN CATCH
        SET @Message = 'Error: ' + ERROR_MESSAGE();
    END CATCH
END
GO
