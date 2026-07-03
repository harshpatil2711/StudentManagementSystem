-- ============================================================
-- ALTER sp_SaveEnrollmentWithSkills — supports SkillID:Months pairs
-- ============================================================
IF EXISTS (SELECT 1 FROM sys.procedures WHERE name = 'sp_SaveEnrollmentWithSkills')
    DROP PROCEDURE sp_SaveEnrollmentWithSkills;
GO

CREATE PROCEDURE sp_SaveEnrollmentWithSkills
    @EnrollmentID INT = NULL,
    @StudentID INT,
    @CourseOfferingID INT,
    @EnrollmentDate DATE,
    @Status INT,
    @SkillData VARCHAR(MAX),            -- "SkillID:Months,SkillID:Months"
    @CreatedBy VARCHAR(50) = NULL,
    @LastModifiedBy VARCHAR(50),
    @Message VARCHAR(100) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM Enrollment WHERE StudentID = @StudentID
        AND CourseOfferingID = @CourseOfferingID AND IsActive = 1
        AND (@EnrollmentID IS NULL OR EnrollmentID <> @EnrollmentID))
    BEGIN
        BEGIN TRANSACTION;
        BEGIN TRY
            -- Insert or Update Enrollment
            IF @EnrollmentID IS NULL OR @EnrollmentID = 0
            BEGIN
                INSERT INTO Enrollment (StudentID, CourseOfferingID, EnrollmentDate, Status, CreatedBy, LastModifiedBy)
                VALUES (@StudentID, @CourseOfferingID, @EnrollmentDate, @Status, @CreatedBy, @LastModifiedBy);
                SET @EnrollmentID = SCOPE_IDENTITY();
            END
            ELSE
            BEGIN
                UPDATE Enrollment SET StudentID = @StudentID, CourseOfferingID = @CourseOfferingID,
                       EnrollmentDate = @EnrollmentDate, Status = @Status, LastModifiedBy = @LastModifiedBy
                WHERE EnrollmentID = @EnrollmentID;
            END

            -- Delete old skills
            DELETE FROM EnrollmentSkill WHERE EnrollmentID = @EnrollmentID;

            -- Parse SkillData: "SkillID:Months,SkillID:Months"
            DECLARE @SkillID INT, @Months INT, @Pos INT = 1, @ColonPos INT, @Pair VARCHAR(100);
            WHILE @Pos <= LEN(@SkillData)
            BEGIN
                SET @Pair = SUBSTRING(@SkillData, @Pos, CHARINDEX(',', @SkillData + ',', @Pos) - @Pos);
                SET @ColonPos = CHARINDEX(':', @Pair);
                IF @ColonPos > 0
                BEGIN
                    SET @SkillID = CAST(LEFT(@Pair, @ColonPos - 1) AS INT);
                    SET @Months = CAST(SUBSTRING(@Pair, @ColonPos + 1, LEN(@Pair)) AS INT);
                    IF @Months < 1 SET @Months = 1;
                    INSERT INTO EnrollmentSkill (EnrollmentID, SkillID, Months, CreatedBy, DateCreated, LastModifiedBy, DateLastModified)
                    VALUES (@EnrollmentID, @SkillID, @Months, @CreatedBy, GETDATE(), @LastModifiedBy, GETDATE());
                END
                SET @Pos = CHARINDEX(',', @SkillData + ',', @Pos) + 1;
                IF @Pos > LEN(@SkillData) + 1 BREAK;
            END

            -- Calculate total fee = SUM(SkillFees * Months)
            DECLARE @TotalFees DECIMAL(10,2);
            SELECT @TotalFees = SUM(S.SkillFees * ES.Months)
            FROM EnrollmentSkill ES
            INNER JOIN Skill S ON S.SkillID = ES.SkillID
            WHERE ES.EnrollmentID = @EnrollmentID;

            -- Save to StudentFees
            IF EXISTS (SELECT 1 FROM StudentFees WHERE EnrollmentID = @EnrollmentID)
                UPDATE StudentFees SET TotalFees = @TotalFees, LastModifiedBy = @LastModifiedBy,
                       DateLastModified = GETDATE() WHERE EnrollmentID = @EnrollmentID;
            ELSE
                INSERT INTO StudentFees (EnrollmentID, TotalFees, CreatedBy, LastModifiedBy)
                VALUES (@EnrollmentID, @TotalFees, @CreatedBy, @LastModifiedBy);

            COMMIT TRANSACTION;
            SET @Message = 'Enrollment Added Successfully';
        END TRY
        BEGIN CATCH
            ROLLBACK TRANSACTION;
            SET @Message = 'Error: ' + ERROR_MESSAGE();
        END CATCH
    END
    ELSE
        SET @Message = 'Student Already Enrolled';
END
GO

-- ============================================================
-- ALTER sp_GetEnrollmentSkills — returns Months from EnrollmentSkill
-- ============================================================
IF EXISTS (SELECT 1 FROM sys.procedures WHERE name = 'sp_GetEnrollmentSkills')
    DROP PROCEDURE sp_GetEnrollmentSkills;
GO

CREATE PROCEDURE sp_GetEnrollmentSkills
    @EnrollmentID INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT S.SkillID, S.SkillName, S.SkillFees, ES.Months
    FROM EnrollmentSkill ES
    INNER JOIN Skill S ON S.SkillID = ES.SkillID
    WHERE ES.EnrollmentID = @EnrollmentID
END
GO
