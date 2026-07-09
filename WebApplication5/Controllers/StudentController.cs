using BusinessLayer.DAL;
using BusinessLayer.Models;
using BusinessLayer1.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication5.Filters;

namespace WebApplication5.Controllers
{
    [RoleAuthorize(UserRole.Admin, UserRole.AdmissionOfficer)]
    public class StudentController : Controller
    {
        StudentDAL dal = new StudentDAL();

        public ActionResult StudentInsert()
        {
            return View();
        }

        [HttpPost]
        public ContentResult InsertStudent(Student student)
        {
            try
            {
                student.CreatedBy = "admin";
                student.LastModifiedBy = "admin";

                string msg = dal.InsertStudent(student);

                return Content(msg);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error inserting student {StudentName}", student.StudentName);
                return Content("Error: " + ex.Message);
            }
        }
    }
}