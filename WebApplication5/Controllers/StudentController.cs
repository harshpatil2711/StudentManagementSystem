using BusinessLayer1.DAL;
using BusinessLayer1.Helpers;
using BusinessLayer.Models;
using BusinessLayer1.Models;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Mvc;
using WebApplication5.Filters;

namespace WebApplication5.Controllers
{
    [RoleAuthorize(UserRole.Admin, UserRole.AdmissionOfficer)]
    public class StudentController : Controller
    {
        private StudentDAL dal = new StudentDAL();
        private string ServerMapRoot
        {
            get { return AppDomain.CurrentDomain.BaseDirectory; }
        }

        // GET: /Student
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public JsonResult GetStudents()
        {
            try
            {
                List<Student> students = dal.GetAllStudents();
                return Json(students, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching students for list");
                return Json(new List<Student>(), JsonRequestBehavior.AllowGet);
            }
        }

        // GET: /Student/Details/5
        public ActionResult Details(int id)
        {
            Student student = dal.GetStudentById(id);
            if (student == null)
                return HttpNotFound();
            return View(student);
        }

        // GET: /Student/StudentInsert
        public ActionResult StudentInsert()
        {
            return View();
        }

        // POST: /Student/InsertStudent
        [HttpPost]
        public ContentResult InsertStudent(Student student, HttpPostedFileBase photo)
        {
            try
            {
                student.CreatedBy = "admin";
                student.LastModifiedBy = "admin";

                string msg = dal.InsertStudent(student);

                if (msg != null && msg.Contains("success"))
                {
                    string idStr = dal.GetStudentIdFromInsertMessage(msg);
                    if (idStr != null)
                    {
                        int newId;
                        if (int.TryParse(idStr, out newId) && photo != null && photo.ContentLength > 0)
                        {
                            try
                            {
                                string photoPath = ImageHelper.SaveUploadedPhoto(
                                    photo.InputStream, photo.FileName, newId, ServerMapRoot);

                                if (!string.IsNullOrEmpty(photoPath))
                                {
                                    student.StudentID = newId;
                                    student.PhotoPath = photoPath;
                                    student.LastModifiedBy = "admin";
                                    dal.UpdateStudent(student);
                                }
                            }
                            catch (InvalidOperationException ex)
                            {
                                Log.Warning(ex, "Photo upload validation failed for student {Id}", newId);
                            }
                            catch (Exception ex)
                            {
                                Log.Error(ex, "Photo save failed for student {Id}", newId);
                            }
                        }
                    }
                }

                return Content(msg);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error inserting student {StudentName}", student.StudentName);
                return Content("Error: " + ex.Message);
            }
        }

        // GET: /Student/Edit/5
        public ActionResult Edit(int id)
        {
            Student student = dal.GetStudentById(id);
            if (student == null)
                return HttpNotFound();
            return View(student);
        }

        // POST: /Student/Edit
        [HttpPost]
        public ContentResult Edit(Student student, HttpPostedFileBase photo)
        {
            try
            {
                student.LastModifiedBy = "admin";

                Student existing = dal.GetStudentById(student.StudentID);
                if (existing == null)
                    return Content("Error: Student not found.");

                if (photo != null && photo.ContentLength > 0)
                {
                    try
                    {
                        string photoPath = ImageHelper.ReplaceUploadedPhoto(
                            photo.InputStream, photo.FileName, student.StudentID, existing.PhotoPath, ServerMapRoot);
                        student.PhotoPath = photoPath;
                    }
                    catch (InvalidOperationException ex)
                    {
                        Log.Warning(ex, "Photo upload validation failed for student {Id}", student.StudentID);
                        student.PhotoPath = existing.PhotoPath;
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Photo replace failed for student {Id}", student.StudentID);
                        student.PhotoPath = existing.PhotoPath;
                    }
                }
                else
                {
                    student.PhotoPath = existing.PhotoPath;
                }

                string msg = dal.UpdateStudent(student);
                return Content(msg);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error editing student {Id}", student.StudentID);
                return Content("Error: " + ex.Message);
            }
        }

        // GET: /Student/Delete/5
        public ActionResult Delete(int id)
        {
            Student student = dal.GetStudentById(id);
            if (student == null)
                return HttpNotFound();
            return View(student);
        }

        // POST: /Student/DeleteConfirm
        [HttpPost]
        public ContentResult DeleteConfirm(int id)
        {
            try
            {
                Student student = dal.GetStudentById(id);

                string msg = dal.DeleteStudent(id);

                if (msg != null && msg.Contains("success") && student != null)
                {
                    ImageHelper.DeleteStudentPhoto(student.PhotoPath, ServerMapRoot);
                }

                return Content(msg);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting student {Id}", id);
                return Content("Error: " + ex.Message);
            }
        }
    }
}
