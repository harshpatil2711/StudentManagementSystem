using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLayer1.Models;

namespace BusinessLayer.ViewModels
{
    public class EnrollmentViewModel
    {
        public int page { get; set; } = 1;
        public int size { get; set; } = 5;
        public List<int> pagesizelist = new List<int>{ 3, 5, 10, 15, 20 };
        public int? status { get; set; } = null;
        public Dictionary<int,String> statusDict { get; set; }
        public string studentname { get; set; } = "";
        public List<Enrollment> Enrollments { get; set; }
        public int Enrollmentcount { get; set; } = 1;
        
    }
}
