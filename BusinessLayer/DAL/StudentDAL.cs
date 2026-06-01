using BusinessLayer.Models;
using Microsoft.Practices.EnterpriseLibrary.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.DAL
{
    public class StudentDAL
    {
        private Database db;

        public StudentDAL()
        {
            this.db = DatabaseFactory.CreateDatabase();
        }
        public string InsertStudent(Student student)
        {
            string message = "";

            DbCommand cmd = db.GetStoredProcCommand("sp_InsertStudent");

            db.AddInParameter(cmd, "@StudentName", DbType.String, student.StudentName);
            db.AddInParameter(cmd, "@DateOfBirth", DbType.Date, student.DateOfBirth);
            db.AddInParameter(cmd, "@Email", DbType.String, student.Email);
            db.AddInParameter(cmd, "@Phone", DbType.String, student.Phone);
            db.AddInParameter(cmd, "@Gender", DbType.String, student.Gender);
            db.AddInParameter(cmd, "@AdmissionYear", DbType.Int32, student.AdmissionYear);
            db.AddInParameter(cmd, "@CreatedBy", DbType.String, student.CreatedBy);
            db.AddInParameter(cmd, "@LastModifiedBy", DbType.String, student.LastModifiedBy);

            db.AddOutParameter(cmd, "@Message", DbType.String, 100);

            db.ExecuteNonQuery(cmd);

            message = db.GetParameterValue(cmd, "@Message").ToString();

            return message;
        }
    }
}
