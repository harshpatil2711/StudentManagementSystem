-- ============================================================
-- sp_GetEnrollmentReceipt — Returns receipt data for a given enrollment
-- ============================================================
IF EXISTS (SELECT 1 FROM sys.objects WHERE type = 'P' AND name = 'sp_GetEnrollmentReceipt')
    DROP PROCEDURE sp_GetEnrollmentReceipt;
GO

CREATE PROCEDURE sp_GetEnrollmentReceipt
    @EnrollmentID INT
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        e.EnrollmentID,
        s.StudentID,
        s.StudentName,
        c.CourseName,
        ISNULL(c.CourseType, 'Academic') AS CourseType,
        e.EnrollmentDate,
        e.Status,
        ISNULL(sf.TotalFees, 0) AS FeePaid,
        ISNULL(co.AcademicYear, 'N/A') AS AcademicYear,
        ISNULL(CAST(co.SemesterNumber AS VARCHAR(10)), '1') AS Semester
    FROM Enrollment e
    INNER JOIN Student s ON s.StudentID = e.StudentID
    INNER JOIN CourseOffering co ON co.CourseOfferingID = e.CourseOfferingID
    INNER JOIN Course c ON c.CourseID = co.CourseID
    LEFT JOIN StudentFees sf ON sf.EnrollmentID = e.EnrollmentID
    WHERE e.EnrollmentID = @EnrollmentID
      AND e.IsActive = 1;
END
GO
