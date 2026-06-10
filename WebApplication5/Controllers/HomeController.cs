using BusinessLayer.ViewModels;
using BusinessLayer1.DAL;
using System;
using System.Web.Mvc;

namespace WebApplication5.Controllers
{
    public class HomeController : Controller
    {
        // GET: /Home/Index
        public ActionResult Index()
        {
            EnrollmentViewModel ev = new EnrollmentViewModel();
            // Set default pagination values to ensure AJAX POST receives valid parameters
            ev.page = 1;
            ev.size = 5;
            EnrollmentDAL da = new EnrollmentDAL();

            ev.Enrollments = da.GetList(ev);
            ev.statusDict = da.getStatusList();
            ev.CourseDict = da.getCoursesList();
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

            EnrollmentDAL da = new EnrollmentDAL();

            ev.Enrollments = da.GetList(ev);
            //ev.statusDict = da.getStatusList();

            return PartialView("_ListData", ev);
        }

        // GET: /Home/InsertEnrollment or /Home/InsertEnrollment/5
        public ActionResult InsertEnrollment(int? id)
        {
            EnrollmentDAL da = new EnrollmentDAL();

            EnrollmentInsertViewModel vm;

            // EDIT MODE
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
                // INSERT MODE
                vm = new EnrollmentInsertViewModel();
            }

            vm.StudentDict = da.GetStudents();
            vm.CourseDict = da.GetCourseOfferings();
            vm.StatusDict = da.getStatusList();
            
            //return View(vm);

            return PartialView("_EnrollmentForm", vm);
        }

        // POST: /Home/InsertEnrollment
        [HttpPost]
        public ActionResult InsertEnrollment(EnrollmentInsertViewModel vm)
        {
            EnrollmentDAL da = new EnrollmentDAL();

            string result;

                result = da.SaveEnrollment(vm);
            

            return Content(result);
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
            EnrollmentDAL da = new EnrollmentDAL();
            string result = da.DeleteEnrollmentById(id);
            return Json(new { message = result });
        }

       
    }
}