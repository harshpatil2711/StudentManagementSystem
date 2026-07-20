using System;

namespace BusinessLayer1.Models
{
    public class EnrollmentReceipt
    {
        public int EnrollmentID { get; set; }
        public string StudentName { get; set; }
        public int StudentID { get; set; }
        public string CourseName { get; set; }
        public string CourseType { get; set; }
        public string AcademicYear { get; set; }
        public string Semester { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public decimal FeePaid { get; set; }
        public string FeeInWords { get; set; }
        public DateTime GeneratedDate { get; set; }
        public string CollegeName { get; set; }
    }
}
