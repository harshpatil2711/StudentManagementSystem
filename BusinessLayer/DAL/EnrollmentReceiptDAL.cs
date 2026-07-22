using Microsoft.Practices.EnterpriseLibrary.Data;
using Serilog;
using System;
using System.Data;
using System.Data.Common;
using BusinessLayer1.Models;

namespace BusinessLayer1.DAL
{
    public class EnrollmentReceiptDAL
    {
        private Database db;

        public EnrollmentReceiptDAL()
        {
            db = DatabaseFactory.CreateDatabase();
        }

        public EnrollmentReceipt GetReceiptData(int enrollmentId)
        {
            EnrollmentReceipt receipt = null;
            try
            {
                DbCommand cmd = db.GetStoredProcCommand("sp_GetEnrollmentReceipt");
                db.AddInParameter(cmd, "@EnrollmentID", DbType.Int32, enrollmentId);

                using (IDataReader reader = db.ExecuteReader(cmd))
                {
                    if (reader.Read())
                    {
                        receipt = new EnrollmentReceipt
                        {
                            EnrollmentID = Convert.ToInt32(reader["EnrollmentID"]),
                            StudentName = reader["StudentName"].ToString(),
                            StudentID = Convert.ToInt32(reader["StudentID"]),
                            CourseName = reader["CourseName"].ToString(),
                            CourseType = reader["CourseType"].ToString(),
                            AcademicYear = reader["AcademicYear"].ToString(),
                            Semester = reader["Semester"].ToString(),
                            EnrollmentDate = Convert.ToDateTime(reader["EnrollmentDate"]),
                            FeePaid = Convert.ToDecimal(reader["FeePaid"]),
                            GeneratedDate = DateTime.Now,
                            CollegeName = "Springfield Institute of Technology"
                        };
                    }
                    reader.Close();
                }

                if (receipt != null)
                {
                    receipt.FeeInWords = NumberToWords(Convert.ToInt64(receipt.FeePaid));
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error fetching receipt data for enrollment {Id}", enrollmentId);
            }
            return receipt;
        }

        private string NumberToWords(long number)
        {
            if (number == 0) return "Zero";

            string[] ones = { "", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine",
                              "Ten", "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen",
                              "Seventeen", "Eighteen", "Nineteen" };
            string[] tens = { "", "", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };

            string words = "";

            if (number >= 10000000)
            {
                words += ConvertLessThanThousand(number / 10000000, ones, tens) + " Crore ";
                number %= 10000000;
            }
            if (number >= 100000)
            {
                words += ConvertLessThanThousand(number / 100000, ones, tens) + " Lakh ";
                number %= 100000;
            }
            if (number >= 1000)
            {
                words += ConvertLessThanThousand(number / 1000, ones, tens) + " Thousand ";
                number %= 1000;
            }
            if (number >= 100)
            {
                words += ones[number / 100] + " Hundred ";
                number %= 100;
            }
            if (number > 0)
            {
                if (number < 20)
                    words += ones[number];
                else
                {
                    words += tens[number / 10];
                    if ((number % 10) > 0)
                        words += " " + ones[number % 10];
                }
            }

            return words.Trim() + " Rupees Only";
        }

        private string ConvertLessThanThousand(long number, string[] ones, string[] tens)
        {
            string result = "";
            if (number >= 100)
            {
                result += ones[number / 100] + " Hundred ";
                number %= 100;
            }
            if (number > 0)
            {
                if (number < 20)
                    result += ones[number];
                else
                {
                    result += tens[number / 10];
                    if ((number % 10) > 0)
                        result += " " + ones[number % 10];
                }
            }
            return result.Trim();
        }
    }
}
