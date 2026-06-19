using Microsoft.Practices.EnterpriseLibrary.Data;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using BusinessLayer1.Models;
using BusinessLayer.ViewModels;

namespace BusinessLayer1.DAL
{
    public class EnrollmentDAL
    {
        private Database db;

        public EnrollmentDAL()
        {
            db = DatabaseFactory.CreateDatabase();
        }

        public List<Enrollment> GetList(EnrollmentViewModel enroll)
        {
            List<Enrollment> list = new List<Enrollment>();
            try
            {
                DbCommand cmd = db.GetStoredProcCommand("EnrollmentDetails");

                db.AddInParameter(cmd, "@PAGE", DbType.Int32, enroll.page);
                db.AddInParameter(cmd, "@SIZE", DbType.Int32, enroll.size);
                db.AddInParameter(cmd, "@Status", DbType.Int32, enroll.status);
                db.AddInParameter(cmd, "@StudentName", DbType.String, enroll.studentname);
                db.AddInParameter(cmd, "@CourseIDs", DbType.String, enroll.courseIDs);
                db.AddInParameter(cmd, "@SortColumn", DbType.String, enroll.SortColumn ?? "EnrollmentId");
                db.AddInParameter(cmd, "@SortDirection", DbType.String, enroll.SortDirection ?? "ASC");
                db.AddOutParameter(cmd, "@EnrollmentCount", DbType.Int32, sizeof(Int32));

                using (IDataReader reader = db.ExecuteReader(cmd))
                {
                    while (reader.Read())
                    {
                        Enrollment enrollment = new Enrollment()
                        {
                            EnrollmentID = Convert.ToInt32(reader["EnrollmentId"]),
                            StudentName = reader["StudentName"].ToString(),
                            CourseName = reader["CourseName"].ToString(),
                            EnrollmentDate = Convert.ToDateTime(reader["EnrollmentDate"]),
                            EnrollmentStatus = reader["EnrollmentStatus"].ToString(),
                            DateCreated = Convert.ToDateTime(reader["DateCreated"]),
                            CreatedBy = reader["CreatedBy"].ToString(),
                            DateLastModified = Convert.ToDateTime(reader["DateLastModified"]),
                            LastModifiedBy = reader["LastModifiedBy"].ToString()
                        };

                        list.Add(enrollment);
                    }

                    reader.Close();

                    enroll.Enrollmentcount = Convert.ToInt32(
                        db.GetParameterValue(cmd, "@EnrollmentCount")
                    );
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in GetList for page {Page}, size {Size}", enroll.page, enroll.size);
                enroll.Enrollmentcount = 0;
            }
            return list;
        }

        public Dictionary<int, string> getStatusList()
        {
            Dictionary<int, string> statusDict = new Dictionary<int, string>();
            try
            {
                DbCommand cmd = db.GetStoredProcCommand("GetDistinctStatus");

                using (IDataReader reader = db.ExecuteReader(cmd))
                {
                    while (reader.Read())
                    {
                        int id = Convert.ToInt32(reader["StatusID"]);
                        string name = reader["StatusName"].ToString();
                        statusDict.Add(id, name);
                    }

                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading status list");
            }
            return statusDict;
        }

        public Dictionary<int, string> getCoursesList()
        {
            Dictionary<int, string> CourseDict = new Dictionary<int, string>();
            try
            {
                DbCommand cmd = db.GetStoredProcCommand("GetDistinctCourses");

                using (IDataReader reader = db.ExecuteReader(cmd))
                {
                    while (reader.Read())
                    {
                        int id = Convert.ToInt32(reader["COURSEID"]);
                        string name = reader["COURSENAME"].ToString();
                        CourseDict.Add(id, name);
                    }

                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading courses list");
            }
            return CourseDict;
        }

        public Dictionary<int, string> GetStudents()
        {
            Dictionary<int, string> studentDict = new Dictionary<int, string>();
            try
            {
                DbCommand cmd = db.GetStoredProcCommand("sp_GetStudents");

                using (IDataReader reader = db.ExecuteReader(cmd))
                {
                    while (reader.Read())
                    {
                        int id = Convert.ToInt32(reader["StudentID"]);
                        string name = reader["StudentName"].ToString();
                        studentDict.Add(id, name);
                    }

                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading students");
            }
            return studentDict;
        }

        public Dictionary<int, string> GetCourseOfferings()
        {
            Dictionary<int, string> courseDict = new Dictionary<int, string>();
            try
            {
                DbCommand cmd = db.GetStoredProcCommand("sp_GetCurrentYearCourseOfferings");

                using (IDataReader reader = db.ExecuteReader(cmd))
                {
                    while (reader.Read())
                    {
                        int id = Convert.ToInt32(reader["CourseOfferingID"]);
                        string name = reader["CourseOfferingName"].ToString();
                        courseDict.Add(id, name);
                    }

                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading course offerings");
            }
            return courseDict;
        }

        public EnrollmentInsertViewModel GetEnrollmentById(int id)
        {
            EnrollmentInsertViewModel vm = null;
            try
            {
                DbCommand cmd = db.GetStoredProcCommand("sp_GetEnrollmentById");
                db.AddInParameter(cmd, "@EnrollmentID", DbType.Int32, id);

                using (IDataReader reader = db.ExecuteReader(cmd))
                {
                    if (reader.Read())
                    {
                        vm = new EnrollmentInsertViewModel()
                        {
                            EnrollmentID = Convert.ToInt32(reader["EnrollmentID"]),
                            StudentID = Convert.ToInt32(reader["StudentID"]),
                            CourseOfferingID = Convert.ToInt32(reader["CourseOfferingID"]),
                            EnrollmentDate = Convert.ToDateTime(reader["EnrollmentDate"]),
                            Status = reader["Status"].ToString()
                        };
                    }

                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading enrollment by ID {Id}", id);
            }
            return vm;
        }

        public string DeleteEnrollmentById(int id)
        {
            string message = "";
            try
            {
                DbCommand cmd = db.GetStoredProcCommand("sp_DeleteEnrollment");
                db.AddInParameter(cmd, "@EnrollmentID", DbType.Int32, id);
                db.AddInParameter(cmd, "@LastModifiedBy", DbType.String, "admin");
                db.AddOutParameter(cmd, "@Message", DbType.String, 100);
                db.ExecuteNonQuery(cmd);
                message = db.GetParameterValue(cmd, "@Message").ToString();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting enrollment {Id}", id);
                message = "Error: " + ex.Message;
            }
            return message;
        }

        public string SaveEnrollment(EnrollmentInsertViewModel vm)
        {
            string message = "";
            try
            {
                DbCommand cmd = db.GetStoredProcCommand("sp_SaveEnrollment");

                db.AddInParameter(cmd, "@EnrollmentID", DbType.Int32, vm.EnrollmentID ?? (object)DBNull.Value);
                db.AddInParameter(cmd, "@StudentID", DbType.Int32, vm.StudentID);
                db.AddInParameter(cmd, "@CourseOfferingID", DbType.Int32, vm.CourseOfferingID);
                db.AddInParameter(cmd, "@EnrollmentDate", DbType.Date, vm.EnrollmentDate);
                db.AddInParameter(cmd, "@Status", DbType.Int32,
                    !string.IsNullOrEmpty(vm.Status) ? Convert.ToInt32(vm.Status) : (object)DBNull.Value);
                db.AddInParameter(cmd, "@CreatedBy", DbType.String, "admin");
                db.AddInParameter(cmd, "@LastModifiedBy", DbType.String, "admin");

                db.AddOutParameter(cmd, "@Message", DbType.String, 100);

                db.ExecuteNonQuery(cmd);

                message = db.GetParameterValue(cmd, "@Message").ToString();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error saving enrollment for StudentID {StudentId}", vm.StudentID);
                message = "Error: " + ex.Message;
            }
            return message;
        }
    }
}