using System;

namespace BusinessLayer1.Models
{
    public class AdmissionEmailData
    {
        public int EnrollmentID { get; set; }
        public int StudentID { get; set; }
        public string StudentName { get; set; }
        public string Email { get; set; }
        public string CourseName { get; set; }
        public string Department { get; set; }
        public string AcademicYear { get; set; }
        public string Semester { get; set; }
        public DateTime EnrollmentDate { get; set; }
    }
}
