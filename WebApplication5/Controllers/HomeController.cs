using BusinessLayer.Helpers;
using BusinessLayer.ViewModels;
using BusinessLayer1.DAL;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using WebApplication5.Filters;

namespace WebApplication5.Controllers
{
    [RoleAuthorize(UserRole.Admin, UserRole.AdmissionOfficer, UserRole.Clerk)]
    public class HomeController : Controller
    {
        // GET: /Home/Index
        public ActionResult Index()
        {
            EnrollmentViewModel ev = new EnrollmentViewModel();
            //ev.page = 1;
            //ev.size = 0;
            try
            {
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
        [RoleAuthorize(UserRole.Admin, UserRole.AdmissionOfficer)]
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

                // Load CourseType and skills if editing
                if (vm.EnrollmentID.HasValue && vm.EnrollmentID.Value > 0)
                {
                    vm.CourseType = da.GetCourseTypeByOfferingId(vm.CourseOfferingID);
                    if (vm.CourseType == "Skill")
                    {
                        vm.SkillList = da.GetEnrollmentSkills(vm.EnrollmentID.Value);
                        vm.SelectedSkills = string.Join(",", vm.SkillList.Select(s => $"{s.SkillID}:{s.Months}"));
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading enrollment form for ID {Id}", id);
                vm = new EnrollmentInsertViewModel();
                vm.StudentDict = new Dictionary<int, string>();
                vm.CourseDict = new Dictionary<int, string>();
                vm.StatusDict = new Dictionary<int, string>();
            }

            if (Request.IsAjaxRequest())
            {
                return PartialView("_EnrollmentForm", vm);
            }
            return View("InsertEnrollment", vm);
        }

        // GET: /Home/GetCourseType
        public JsonResult GetCourseType(int courseOfferingId)
        {
            try
            {
                EnrollmentDAL da = new EnrollmentDAL();
                string courseType = da.GetCourseTypeByOfferingId(courseOfferingId);
                return Json(new { courseType }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting course type for offering {Id}", courseOfferingId);
                return Json(new { courseType = "Academic" }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: /Home/GetCourseOfferingFee
        public JsonResult GetCourseOfferingFee(int courseOfferingId)
        {
            try
            {
                EnrollmentDAL da = new EnrollmentDAL();
                decimal fee = da.GetCourseOfferingFee(courseOfferingId);
                return Json(new { fee }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting fee for offering {Id}", courseOfferingId);
                return Json(new { fee = 0 }, JsonRequestBehavior.AllowGet);
            }
        }

        // GET: /Home/GetSkillsByOffering
        public JsonResult GetSkillsByOffering(int courseOfferingId)
        {
            try
            {
                EnrollmentDAL da = new EnrollmentDAL();
                var skills = da.GetSkillsByCourseOfferingId(courseOfferingId);
                return Json(skills, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading skills for offering {Id}", courseOfferingId);
                return Json(new List<object>(), JsonRequestBehavior.AllowGet);
            }
        }

        // POST: /Home/InsertEnrollment
        [HttpPost]
        [RoleAuthorize(UserRole.Admin, UserRole.AdmissionOfficer)]
        public ActionResult InsertEnrollment(EnrollmentInsertViewModel vm)
        {
            try
            {
                EnrollmentDAL da = new EnrollmentDAL();
                string courseType = da.GetCourseTypeByOfferingId(vm.CourseOfferingID);
                string result;

                if (courseType == "Skill")
                {
                    result = da.SaveEnrollmentWithSkills(vm);
                }
                else
                {
                    int newId;
                    result = da.SaveEnrollment(vm, out newId);
                    if (result.ToLower().Contains("success"))
                    {
                        int eid = newId > 0 ? newId : (vm.EnrollmentID ?? 0);
                        decimal courseFee = da.GetCourseOfferingFee(vm.CourseOfferingID);
                        da.SaveStudentFee(eid, courseFee, null, "admin");
                    }
                }

                return Content(result);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in InsertEnrollment POST for StudentID {StudentId}", vm.StudentID);
                return Content("Error: " + ex.Message);
            }
        }
  
        // GET: /Home/GetEnrollmentDetails/5
        public ActionResult GetEnrollmentDetails(int id)
        {
            try
            {
                EnrollmentDAL da = new EnrollmentDAL();
                var details = da.GetEnrollmentDetails(id);
                if (details == null)
                    return HttpNotFound();
                return PartialView("_EnrollmentDetails", details);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading enrollment details for {Id}", id);
                return Content("Error: " + ex.Message);
            }
        }

        // GET: /Home/ManageFees/5
        [RoleAuthorize(UserRole.Admin, UserRole.Clerk)]
        public ActionResult ManageFees(int id)
        {
            try
            {
                EnrollmentDAL da = new EnrollmentDAL();
                var fee = da.GetStudentFeeByEnrollmentId(id);
                if (fee == null)
                {
                    var enroll = da.GetEnrollmentById(id);
                    fee = new BusinessLayer1.Models.StudentFeeInfo
                    {
                        EnrollmentID = id,
                        CourseType = da.GetCourseTypeByOfferingId(enroll?.CourseOfferingID ?? 0),
                        CourseFees = da.GetCourseOfferingFee(enroll?.CourseOfferingID ?? 0)
                    };
                }
                return PartialView("_ManageFees", fee);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading fee form for enrollment {Id}", id);
                return Content("Error: " + ex.Message);
            }
        }

        // POST: /Home/SaveFee
        [HttpPost]
        [RoleAuthorize(UserRole.Admin, UserRole.Clerk)]
        public ActionResult SaveFee(int enrollmentId, decimal totalFees, decimal? feesPaid)
        {
            try
            {
                EnrollmentDAL da = new EnrollmentDAL();
                string result = da.SaveStudentFee(enrollmentId, totalFees, feesPaid, "admin");
                return Content(result);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error saving fee for enrollment {Id}", enrollmentId);
                return Content("Error: " + ex.Message);
            }
        }

        // GET: /Home/FeesEdit
        [RoleAuthorize(UserRole.Admin, UserRole.Clerk)]
        public ActionResult FeesEdit()
        {
            try
            {
                EnrollmentDAL da = new EnrollmentDAL();
                ViewBag.StudentList = da.GetStudents();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading FeesEdit page");
            }
            return View();
        }

        // GET: /Home/GetStudentEnrollmentsWithFees?studentId=5
        public JsonResult GetStudentEnrollmentsWithFees(int studentId)
        {
            try
            {
                EnrollmentDAL da = new EnrollmentDAL();
                var list = da.GetStudentEnrollmentsWithFees(studentId);
                return Json(list, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading enrollments with fees for student {Id}", studentId);
                return Json(new List<BusinessLayer1.Models.StudentEnrollmentFee>(), JsonRequestBehavior.AllowGet);
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
        [RoleAuthorize(UserRole.Admin)]
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