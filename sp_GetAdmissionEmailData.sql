IF EXISTS (SELECT 1 FROM sys.objects WHERE type = 'P' AND name = 'sp_GetAdmissionEmailData')
    DROP PROCEDURE sp_GetAdmissionEmailData;
GO

CREATE PROCEDURE sp_GetAdmissionEmailData
    @EnrollmentID INT = NULL,
    @StudentID INT = NULL,
    @CourseOfferingID INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    IF @EnrollmentID IS NULL AND @StudentID IS NOT NULL AND @CourseOfferingID IS NOT NULL
    BEGIN
        SELECT TOP 1 @EnrollmentID = EnrollmentID
        FROM Enrollment
        WHERE StudentID = @StudentID
          AND CourseOfferingID = @CourseOfferingID
          AND IsActive = 1
        ORDER BY EnrollmentID DESC;
    END

    SELECT
        e.EnrollmentID,
        s.StudentID,
        s.StudentName,
        s.Email,
        c.CourseName,
        ISNULL(c.CourseType, 'Academic') AS Department,
        ISNULL(co.AcademicYear, 'N/A') AS AcademicYear,
        ISNULL(CAST(co.SemesterNumber AS VARCHAR(10)), '1') AS Semester,
        e.EnrollmentDate
    FROM Enrollment e
    INNER JOIN Student s ON s.StudentID = e.StudentID
    INNER JOIN CourseOffering co ON co.CourseOfferingID = e.CourseOfferingID
    INNER JOIN Course c ON c.CourseID = co.CourseID
    WHERE e.EnrollmentID = @EnrollmentID
      AND e.IsActive = 1;
END
GO
