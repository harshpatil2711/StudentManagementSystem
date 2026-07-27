using BusinessLayer.Models;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;

namespace BusinessLayer1.DAL
{
    public class StudentDAL
    {
        private Database db;

        public StudentDAL()
        {
            this.db = DatabaseFactory.CreateDatabase();
        }

        public string InsertStudent(Student student, out int newStudentId)
        {
            string message = "";
            newStudentId = -1;
            try
            {
                DbCommand cmd = db.GetStoredProcCommand("sp_InsertStudent");

                db.AddInParameter(cmd, "@StudentName", DbType.String, student.StudentName);
                db.AddInParameter(cmd, "@DateOfBirth", DbType.Date, student.DateOfBirth);
                db.AddInParameter(cmd, "@Email", DbType.String, student.Email);
                db.AddInParameter(cmd, "@Phone", DbType.String, student.Phone);
                db.AddInParameter(cmd, "@Gender", DbType.String, student.Gender);
                db.AddInParameter(cmd, "@AdmissionYear", DbType.Int32, student.AdmissionYear);
                db.AddInParameter(cmd, "@PhotoPath", DbType.String, (object)student.PhotoPath ?? DBNull.Value);
                db.AddInParameter(cmd, "@CreatedBy", DbType.String, student.CreatedBy);
                db.AddInParameter(cmd, "@LastModifiedBy", DbType.String, student.LastModifiedBy);

                db.AddOutParameter(cmd, "@Message", DbType.String, 200);
                db.AddOutParameter(cmd, "@NewStudentID", DbType.Int32, sizeof(int));

                db.ExecuteNonQuery(cmd);

                message = db.GetParameterValue(cmd, "@Message").ToString();
                object idVal = db.GetParameterValue(cmd, "@NewStudentID");
                if (idVal != null && idVal != DBNull.Value)
                {
                    newStudentId = Convert.ToInt32(idVal);
                }
            }
            catch (SqlException ex)
            {
                Log.Error(ex, "SQL error inserting student {StudentName}", student.StudentName);
                message = "Error: Database error - " + ex.Message;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error inserting student {StudentName}", student.StudentName);
                message = "Error: " + ex.Message;
            }
            return message;
        }

        public Student GetStudentById(int studentId)
        {
            try
            {
                DbCommand cmd = db.GetStoredProcCommand("sp_GetStudentById");
                db.AddInParameter(cmd, "@StudentID", DbType.Int32, studentId);

                using (IDataReader reader = db.ExecuteReader(cmd))
                {
                    if (reader.Read())
                    {
                        return MapStudent(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching student {Id}", studentId);
            }
            return null;
        }

        public List<Student> GetAllStudents()
        {
            var list = new List<Student>();
            try
            {
                DbCommand cmd = db.GetStoredProcCommand("sp_GetAllStudents");

                using (IDataReader reader = db.ExecuteReader(cmd))
                {
                    while (reader.Read())
                    {
                        list.Add(MapStudent(reader));
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching all students");
            }
            return list;
        }

        public string UpdateStudent(Student student)
        {
            string message = "";
            try
            {
                DbCommand cmd = db.GetStoredProcCommand("sp_UpdateStudent");

                db.AddInParameter(cmd, "@StudentID", DbType.Int32, student.StudentID);
                db.AddInParameter(cmd, "@StudentName", DbType.String, student.StudentName);
                db.AddInParameter(cmd, "@DateOfBirth", DbType.Date, student.DateOfBirth);
                db.AddInParameter(cmd, "@Email", DbType.String, student.Email);
                db.AddInParameter(cmd, "@Phone", DbType.String, student.Phone);
                db.AddInParameter(cmd, "@Gender", DbType.String, student.Gender);
                db.AddInParameter(cmd, "@AdmissionYear", DbType.Int32, student.AdmissionYear);
                db.AddInParameter(cmd, "@PhotoPath", DbType.String, (object)student.PhotoPath ?? DBNull.Value);
                db.AddInParameter(cmd, "@LastModifiedBy", DbType.String, student.LastModifiedBy);

                db.AddOutParameter(cmd, "@Message", DbType.String, 200);

                db.ExecuteNonQuery(cmd);

                message = db.GetParameterValue(cmd, "@Message").ToString();
            }
            catch (SqlException ex)
            {
                Log.Error(ex, "SQL error updating student {Id}", student.StudentID);
                message = "Error: Database error - " + ex.Message;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error updating student {Id}", student.StudentID);
                message = "Error: " + ex.Message;
            }
            return message;
        }

        public string DeleteStudent(int studentId)
        {
            string message = "";
            try
            {
                DbCommand cmd = db.GetStoredProcCommand("sp_DeleteStudent");

                db.AddInParameter(cmd, "@StudentID", DbType.Int32, studentId);
                db.AddOutParameter(cmd, "@Message", DbType.String, 200);

                db.ExecuteNonQuery(cmd);

                message = db.GetParameterValue(cmd, "@Message").ToString();
            }
            catch (SqlException ex)
            {
                Log.Error(ex, "SQL error deleting student {Id}", studentId);
                message = "Error: Database error - " + ex.Message;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error deleting student {Id}", studentId);
                message = "Error: " + ex.Message;
            }
            return message;
        }

        private Student MapStudent(IDataReader reader)
        {
            var s = new Student();
            s.StudentID = Convert.ToInt32(reader["StudentID"]);
            s.StudentName = reader["StudentName"] as string ?? "";
            s.DateOfBirth = reader["DateOfBirth"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["DateOfBirth"]);
            s.Email = reader["Email"] as string ?? "";
            s.Phone = reader["Phone"] as string ?? "";
            s.Gender = reader["Gender"] as string ?? "";
            s.AdmissionYear = reader["AdmissionYear"] == DBNull.Value ? 0 : Convert.ToInt32(reader["AdmissionYear"]);
            s.IsActive = reader["IsActive"] == DBNull.Value || Convert.ToBoolean(reader["IsActive"]);
            s.DateCreated = reader["DateCreated"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["DateCreated"]);
            s.CreatedBy = reader["CreatedBy"] as string ?? "";
            s.DateLastModified = reader["DateLastModified"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["DateLastModified"]);
            s.LastModifiedBy = reader["LastModifiedBy"] as string ?? "";
            s.PhotoPath = reader["PhotoPath"] as string;
            return s;
        }
    }
}
