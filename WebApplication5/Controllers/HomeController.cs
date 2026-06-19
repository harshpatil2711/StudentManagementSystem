using BusinessLayer.ViewModels;
using BusinessLayer1.DAL;
using Serilog;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace WebApplication5.Controllers
{
    public class HomeController : Controller
    {
        // GET: /Home/Index
        public ActionResult Index()
        {
            EnrollmentViewModel ev = new EnrollmentViewModel();
            ev.page = 1;
            ev.size = 5;
            try
            {
                Log.Error(new Exception("Test error"), "Manual test from {Controller}", "HomeController");
                EnrollmentDAL da = new EnrollmentDAL();
                ev.Enrollments = da.GetList(ev);
                ev.statusDict = da.getStatusList();
                ev.CourseDict = da.getCoursesList();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading Index page");
                ViewBag.Error = "An error occurred: " + ex.Message;
            }
            return View(ev);
        }

        // POST: /Home/Index
        [HttpPost]
        public ActionResult Index(EnrollmentViewModel ev)
        {
            if (ev.page < 1 || ev.size < 1)
            {
                ViewBag.Error = "Page and Size should be greater than 0";
                return View("Error");
            }

            try
            {
                EnrollmentDAL da = new EnrollmentDAL();
                ev.Enrollments = da.GetList(ev);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in POST Index with page {Page}, size {Size}", ev.page, ev.size);
                ViewBag.Error = "An error occurred: " + ex.Message;
                return View("Error");
            }

            return PartialView("_ListData", ev);
        }

        // GET: /Home/InsertEnrollment or /Home/InsertEnrollment/5
        public ActionResult InsertEnrollment(int? id)
        {
            EnrollmentInsertViewModel vm;

            try
            {
                EnrollmentDAL da = new EnrollmentDAL();

                if (id.HasValue && id.Value > 0)
                {
                    vm = da.GetEnrollmentById(id.Value);
                    if (vm == null)
                    {
                        return HttpNotFound();
                    }
                }
                else
                {
                    vm = new EnrollmentInsertViewModel();
                }

                vm.StudentDict = da.GetStudents();
                vm.CourseDict = da.GetCourseOfferings();
                vm.StatusDict = da.getStatusList();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading enrollment form for ID {Id}", id);
                vm = new EnrollmentInsertViewModel();
                vm.StudentDict = new Dictionary<int, string>();
                vm.CourseDict = new Dictionary<int, string>();
                vm.StatusDict = new Dictionary<int, string>();
            }

            return PartialView("_EnrollmentForm", vm);
        }

        // POST: /Home/InsertEnrollment
        [HttpPost]
        public ActionResult InsertEnrollment(EnrollmentInsertViewModel vm)
        {
            try
            {
                EnrollmentDAL da = new EnrollmentDAL();
                string result = da.SaveEnrollment(vm);
                return Content(result);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in InsertEnrollment POST for StudentID {StudentId}", vm.StudentID);
                return Content("Error: " + ex.Message);
            }
        }
  
        public ActionResult About()
        {
            return View();
        }

        public ActionResult Contact()
        {
            return View();
        }
        [HttpPost]
        public JsonResult DeleteEnrollment(int id)
        {
            try
            {
                EnrollmentDAL da = new EnrollmentDAL();
                string result = da.DeleteEnrollmentById(id);
                return Json(new { message = result });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting enrollment {Id} via POST", id);
                return Json(new { message = "Error: " + ex.Message });
            }
    }
    }
}