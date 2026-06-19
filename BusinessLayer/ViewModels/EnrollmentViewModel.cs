using System;
using System.Collections.Generic;
using BusinessLayer1.Models;

namespace BusinessLayer.ViewModels
{
    public class EnrollmentViewModel
    {
        public int page { get; set; }
        public int size { get; set; }
        public List<int> pagesizelist;
        public int? status { get; set; }
        public Dictionary<int, String> statusDict { get; set; }
        public string studentname { get; set; }
        public List<Enrollment> Enrollments { get; set; }
        public int Enrollmentcount { get; set; }
        public string courseIDs { get; set; }
        public Dictionary<int, string> CourseDict { get; set; }
        public string SortColumn { get; set; }
        public string SortDirection { get; set; }

        public EnrollmentViewModel()
        {
            page = 1;
            size = 5;
            pagesizelist = new List<int> { 1, 3, 5, 10, 15, 20 };
            status = null;
            studentname = "";
            Enrollmentcount = 1;
            SortColumn = "EnrollmentId";
            SortDirection = "ASC";
        }
    }
}
