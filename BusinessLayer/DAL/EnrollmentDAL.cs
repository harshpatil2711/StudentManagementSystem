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
                                CourseType = reader["CourseType"].ToString(),
                                EnrollmentDate = Convert.ToDateTime(reader["EnrollmentDate"]),
                                EnrollmentStatus = reader["EnrollmentStatus"].ToString(),
                                DateCreated = Convert.ToDateTime(reader["DateCreated"]),
                                CreatedBy = reader["CreatedBy"].ToString(),
                                DateLastModified = Convert.ToDateTime(reader["DateLastModified"]),
                                LastModifiedBy = reader["LastModifiedBy"].ToString(),
                                TotalFees = reader["TotalFees"] != DBNull.Value ? Convert.ToDecimal(reader["TotalFees"]) : (decimal?)null
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

        public string SaveEnrollment(EnrollmentInsertViewModel vm, out int newEnrollmentId)
        {
            string message = "";
            newEnrollmentId = 0;
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

                using (IDataReader reader = db.ExecuteReader(cmd))
                {
                    if (reader.Read() && reader["NewEnrollmentID"] != DBNull.Value)
                        newEnrollmentId = Convert.ToInt32(reader["NewEnrollmentID"]);
                    reader.Close();
                }

                message = db.GetParameterValue(cmd, "@Message").ToString();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error saving enrollment for StudentID {StudentId}", vm.StudentID);
                message = "Error: " + ex.Message;
            }
            return message;
        }

        public string GetCourseTypeByOfferingId(int courseOfferingId)
        {
            string courseType = "Academic";
            try
            {
                DbCommand cmd = db.GetStoredProcCommand("sp_GetCourseTypeByOfferingId");
                db.AddInParameter(cmd, "@CourseOfferingID", DbType.Int32, courseOfferingId);
                object result = db.ExecuteScalar(cmd);
                if (result != null)
                    courseType = result.ToString();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting course type for offering {Id}", courseOfferingId);
            }
            return courseType;
        }

        public List<Skill> GetSkillsByCourseOfferingId(int courseOfferingId)
        {
            List<Skill> skills = new List<Skill>();
            try
            {
                DbCommand cmd = db.GetStoredProcCommand("sp_GetSkillsByCourseOfferingId");
                db.AddInParameter(cmd, "@CourseOfferingID", DbType.Int32, courseOfferingId);

                using (IDataReader reader = db.ExecuteReader(cmd))
                {
                    while (reader.Read())
                    {
                        skills.Add(new Skill
                        {
                            SkillID = Convert.ToInt32(reader["SkillID"]),
                            SkillName = reader["SkillName"].ToString(),
                            SkillDurationInMonths = Convert.ToInt32(reader["SkillDurationInMonths"]),
                            SkillFees = Convert.ToDecimal(reader["SkillFees"])
                        });
                    }
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading skills for offering {Id}", courseOfferingId);
            }
            return skills;
        }

        public string SaveEnrollmentWithSkills(EnrollmentInsertViewModel vm)
        {
            string message = "";
            try
            {
                DbCommand cmd = db.GetStoredProcCommand("sp_SaveEnrollmentWithSkills");

                db.AddInParameter(cmd, "@EnrollmentID", DbType.Int32, vm.EnrollmentID ?? (object)DBNull.Value);
                db.AddInParameter(cmd, "@StudentID", DbType.Int32, vm.StudentID);
                db.AddInParameter(cmd, "@CourseOfferingID", DbType.Int32, vm.CourseOfferingID);
                db.AddInParameter(cmd, "@EnrollmentDate", DbType.Date, vm.EnrollmentDate);
                db.AddInParameter(cmd, "@Status", DbType.Int32,
                    !string.IsNullOrEmpty(vm.Status) ? Convert.ToInt32(vm.Status) : (object)DBNull.Value);
                db.AddInParameter(cmd, "@SkillData", DbType.String, vm.SelectedSkills);
                db.AddInParameter(cmd, "@CreatedBy", DbType.String, "admin");
                db.AddInParameter(cmd, "@LastModifiedBy", DbType.String, "admin");

                db.AddOutParameter(cmd, "@Message", DbType.String, 100);

                db.ExecuteNonQuery(cmd);

                message = db.GetParameterValue(cmd, "@Message").ToString();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error saving enrollment with skills for StudentID {StudentId}", vm.StudentID);
                message = "Error: " + ex.Message;
            }
            return message;
        }

        public List<Skill> GetEnrollmentSkills(int enrollmentId)
        {
            List<Skill> skills = new List<Skill>();
            try
            {
                DbCommand cmd = db.GetStoredProcCommand("sp_GetEnrollmentSkills");
                db.AddInParameter(cmd, "@EnrollmentID", DbType.Int32, enrollmentId);

                using (IDataReader reader = db.ExecuteReader(cmd))
                {
                    while (reader.Read())
                    {
                        skills.Add(new Skill
                        {
                            SkillID = Convert.ToInt32(reader["SkillID"]),
                            SkillName = reader["SkillName"].ToString(),
                            SkillFees = Convert.ToDecimal(reader["SkillFees"]),
                            Months = reader["Months"] != DBNull.Value ? Convert.ToInt32(reader["Months"]) : 0
                        });
                    }
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading enrollment skills for enrollment {Id}", enrollmentId);
            }
            return skills;
        }

        public EnrollmentDetailsModel GetEnrollmentDetails(int enrollmentId)
        {
            EnrollmentDetailsModel model = null;
            try
            {
                DbCommand cmd = db.GetStoredProcCommand("sp_GetEnrollmentDetails");
                db.AddInParameter(cmd, "@EnrollmentID", DbType.Int32, enrollmentId);
                using (IDataReader reader = db.ExecuteReader(cmd))
                {
                    // Result set 1: Basic info
                    if (reader.Read())
                    {
                        model = new EnrollmentDetailsModel
                        {
                            EnrollmentID = Convert.ToInt32(reader["EnrollmentID"]),
                            StudentName = reader["StudentName"].ToString(),
                            CourseName = reader["CourseName"].ToString(),
                            CourseType = reader["CourseType"].ToString(),
                            CourseDurationYears = reader["CourseDurationYears"] != DBNull.Value ? Convert.ToInt32(reader["CourseDurationYears"]) : (int?)null,
                            EnrollmentDate = Convert.ToDateTime(reader["EnrollmentDate"]),
                            EnrollmentStatus = reader["EnrollmentStatus"].ToString(),
                            TotalFees = reader["TotalFees"] != DBNull.Value ? Convert.ToDecimal(reader["TotalFees"]) : (decimal?)null,
                            Skills = new List<Skill>(),
                            Subjects = new List<SubjectInfo>()
                        };
                    }
                    if (model.CourseType == "Skill")
                    {
                        // Result set 2: Skills
                        if (reader.NextResult())
                        {
                            while (reader.Read())
                            {
                                if (model != null)
                                {
                                    model.Skills.Add(new Skill
                                    {
                                        SkillID = Convert.ToInt32(reader["SkillID"]),
                                        SkillName = reader["SkillName"].ToString(),
                                        SkillDurationInMonths = Convert.ToInt32(reader["SkillDurationInMonths"]),
                                        SkillFees = Convert.ToDecimal(reader["SkillFees"])
                                    });
                                }
                            }
                        }
                    }
                    else {
                        // Result set 3: Subjects
                        if (reader.NextResult())
                        {
                            while (reader.Read())
                            {
                                if (model != null)
                                {
                                    model.Subjects.Add(new SubjectInfo
                                    {
                                        SubjectID = Convert.ToInt32(reader["SubjectID"]),
                                        SubjectName = reader["SubjectName"].ToString(),
                                        Credits = Convert.ToInt32(reader["Credits"]),
                                        SemesterNumber = Convert.ToInt32(reader["SemesterNumber"])
                                    });
                                }
                            }
                        }
                    }

                   

                 

                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading enrollment details for enrollment {Id}", enrollmentId);
            }
            return model;
        }

        public StudentFeeInfo GetStudentFeeByEnrollmentId(int enrollmentId)
        {
            StudentFeeInfo feeInfo = null;
            try
            {
                DbCommand cmd = db.GetStoredProcCommand("sp_GetStudentFeeByEnrollmentId");
                db.AddInParameter(cmd, "@EnrollmentID", DbType.Int32, enrollmentId);

                using (IDataReader reader = db.ExecuteReader(cmd))
                {
                    if (reader.Read())
                    {
                        feeInfo = new StudentFeeInfo
                        {
                            StudentFeeID = Convert.ToInt32(reader["StudentFeeID"]),
                            EnrollmentID = Convert.ToInt32(reader["EnrollmentID"]),
                            TotalFees = Convert.ToDecimal(reader["TotalFees"]),
                            FeesPaid = reader["FeesPaid"] != DBNull.Value ? Convert.ToDecimal(reader["FeesPaid"]) : (decimal?)null,
                            CourseFees = reader["CourseFees"] != DBNull.Value ? Convert.ToDecimal(reader["CourseFees"]) : (decimal?)null,
                            DurationYears = reader["DurationYears"] != DBNull.Value ? Convert.ToInt32(reader["DurationYears"]) : (int?)null,
                            CourseType = reader["CourseType"].ToString()
                        };
                    }
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading student fee for enrollment {Id}", enrollmentId);
            }
            return feeInfo;
        }

        public decimal GetCourseOfferingFee(int courseOfferingId)
        {
            decimal fee = 0;
            try
            {
                DbCommand cmd = db.GetStoredProcCommand("sp_GetCourseOfferingFee");
                db.AddInParameter(cmd, "@CourseOfferingID", DbType.Int32, courseOfferingId);
                object result = db.ExecuteScalar(cmd);
                if (result != null)
                    fee = Convert.ToDecimal(result);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting fee for offering {Id}", courseOfferingId);
            }
            return fee;
        }

        public string SaveStudentFee(int enrollmentId, decimal totalFees, decimal? feesPaid, string lastModifiedBy)
        {
            string message = "";
            try
            {
                DbCommand cmd = db.GetStoredProcCommand("sp_SaveStudentFee");
                db.AddInParameter(cmd, "@EnrollmentID", DbType.Int32, enrollmentId);
                db.AddInParameter(cmd, "@TotalFees", DbType.Decimal, totalFees);
                db.AddInParameter(cmd, "@FeesPaid", DbType.Decimal, feesPaid ?? (object)DBNull.Value);
                db.AddInParameter(cmd, "@CreatedBy", DbType.String, lastModifiedBy);
                db.AddInParameter(cmd, "@LastModifiedBy", DbType.String, lastModifiedBy);
                db.AddOutParameter(cmd, "@Message", DbType.String, 100);

                db.ExecuteNonQuery(cmd);
                message = db.GetParameterValue(cmd, "@Message").ToString();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error saving fee for enrollment {Id}", enrollmentId);
                message = "Error: " + ex.Message;
            }
            return message;
        }

        public List<StudentEnrollmentFee> GetStudentEnrollmentsWithFees(int studentId)
        {
            List<StudentEnrollmentFee> list = new List<StudentEnrollmentFee>();
            try
            {
                DbCommand cmd = db.GetStoredProcCommand("sp_GetStudentEnrollmentsWithFees");
                db.AddInParameter(cmd, "@StudentID", DbType.Int32, studentId);

                using (IDataReader reader = db.ExecuteReader(cmd))
                {
                    while (reader.Read())
                    {
                        list.Add(new StudentEnrollmentFee
                        {
                            EnrollmentID = Convert.ToInt32(reader["EnrollmentID"]),
                            StudentID = Convert.ToInt32(reader["StudentID"]),
                            StudentName = reader["StudentName"].ToString(),
                            CourseName = reader["CourseName"].ToString(),
                            CourseType = reader["CourseType"].ToString(),
                            EnrollmentDate = Convert.ToDateTime(reader["EnrollmentDate"]),
                            Status = reader["Status"].ToString(),
                            TotalFees = reader["TotalFees"] != DBNull.Value ? Convert.ToDecimal(reader["TotalFees"]) : (decimal?)null,
                            FeesPaid = reader["FeesPaid"] != DBNull.Value ? Convert.ToDecimal(reader["FeesPaid"]) : (decimal?)null,
                            RemainingFees = Convert.ToDecimal(reader["RemainingFees"])
                        });
                    }
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading enrollments with fees for student {Id}", studentId);
            }
            return list;
        }

    }
}