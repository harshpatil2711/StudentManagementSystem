using BusinessLayer.DAL;
using BusinessLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebApplication5.Controllers
{
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
            student.CreatedBy = "admin";
            student.LastModifiedBy = "admin";

            string msg = dal.InsertStudent(student);

            return Content(msg);
        }
    }
}