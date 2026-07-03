using BusinessLayer1.Models;
using System;
using System.Collections.Generic;

namespace BusinessLayer.ViewModels
{
    public class EnrollmentInsertViewModel
    {
        public int? EnrollmentID { get; set; }

        public int StudentID { get; set; }

        public int CourseOfferingID { get; set; }

        public DateTime? EnrollmentDate { get; set; }

        public string Status { get; set; }

        public string CourseType { get; set; }

        public string SelectedSkills { get; set; }

        public List<Skill> SkillList { get; set; }

        public Dictionary<int, string> StudentDict
        {
            get;
            set;
        }

        public Dictionary<int, string> CourseDict
        {
            get;
            set;
        }

        public Dictionary<int, string> StatusDict
        {
            get;
            set;
        }
    }
}
