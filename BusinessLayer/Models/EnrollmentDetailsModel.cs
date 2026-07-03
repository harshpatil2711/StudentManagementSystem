using System;
using System.Collections.Generic;

namespace BusinessLayer1.Models
{
    public class EnrollmentDetailsModel
    {
        public int EnrollmentID { get; set; }
        public string StudentName { get; set; }
        public string CourseName { get; set; }
        public string CourseType { get; set; }
        public int? CourseDurationYears { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public string EnrollmentStatus { get; set; }
        public decimal? TotalFees { get; set; }
        public List<Skill> Skills { get; set; }
        public List<SubjectInfo> Subjects { get; set; }
    }

    public class SubjectInfo
    {
        public int SubjectID { get; set; }
        public string SubjectName { get; set; }
        public int Credits { get; set; }
        public int SemesterNumber { get; set; }
    }

    public class StudentFeeInfo
    {
        public int? StudentFeeID { get; set; }
        public int EnrollmentID { get; set; }
        public decimal TotalFees { get; set; }
        public decimal? FeesPaid { get; set; }
        public decimal? CourseFees { get; set; }
        public int? DurationYears { get; set; }
        public string CourseType { get; set; }
    }
}
