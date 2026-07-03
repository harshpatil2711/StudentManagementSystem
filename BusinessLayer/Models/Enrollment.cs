using System;
using System.Collections.Generic;

namespace BusinessLayer1.Models
{
    public class Enrollment
    {
        public int EnrollmentID { get; set; }
        public string StudentName { get; set; }
        public string CourseName { get; set; }
        public string CourseType { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public string EnrollmentStatus { get; set; }
        public DateTime DateCreated { get; set; }
        public string CreatedBy { get; set; }
        public DateTime DateLastModified { get; set; }
        public string LastModifiedBy { get; set; }
        public decimal? TotalFees { get; set; }
        public List<Skill> Skills { get; set; }
    }
}
