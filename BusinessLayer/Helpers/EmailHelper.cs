using Serilog;
using System;
using System.Configuration;
using System.Net;
using System.Net.Mail;
using BusinessLayer1.Models;

namespace BusinessLayer1.Helpers
{
    public static class EmailHelper
    {
        private static string _smtpHost;
        private static int _smtpPort;
        private static string _smtpUsername;
        private static string _smtpPassword;
        private static bool _smtpSsl;
        private static string _senderEmail;
        private static string _senderName;
        private static bool _configured;

        static EmailHelper()
        {
            try
            {
                _smtpHost = ConfigurationManager.AppSettings["SmtpHost"];
                _smtpPort = int.Parse(ConfigurationManager.AppSettings["SmtpPort"] ?? "587");
                _smtpUsername = ConfigurationManager.AppSettings["SmtpUsername"];
                _smtpPassword = ConfigurationManager.AppSettings["SmtpPassword"];
                _smtpSsl = bool.Parse(ConfigurationManager.AppSettings["SmtpSsl"] ?? "true");
                _senderEmail = ConfigurationManager.AppSettings["SmtpSenderEmail"];
                _senderName = ConfigurationManager.AppSettings["SmtpSenderName"] ?? "Springfield Institute of Technology";
                _configured = !string.IsNullOrWhiteSpace(_smtpHost) &&
                               !string.IsNullOrWhiteSpace(_smtpUsername) &&
                               !string.IsNullOrWhiteSpace(_smtpPassword) &&
                               !string.IsNullOrWhiteSpace(_senderEmail);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to initialize EmailHelper SMTP configuration");
                _configured = false;
            }
        }

        public static bool IsConfigured
        {
            get { return _configured; }
        }

        public static void SendAdmissionConfirmation(AdmissionEmailData data)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            if (!_configured)
            {
                Log.Warning("Admission email not sent — SMTP not configured. EnrollmentId: {Id}", data.EnrollmentID);
                return;
            }

            if (string.IsNullOrWhiteSpace(data.Email))
            {
                Log.Warning("Admission email not sent — student has no email address. EnrollmentId: {Id}", data.EnrollmentID);
                return;
            }

            try
            {
                string subject = string.Format("Admission Confirmation - {0}", data.CourseName);
                string body = BuildEmailBody(data);

                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress(_senderEmail, _senderName);
                    mail.To.Add(new MailAddress(data.Email, data.StudentName));
                    mail.Subject = subject;
                    mail.Body = body;
                    mail.IsBodyHtml = true;

                    using (SmtpClient client = new SmtpClient(_smtpHost, _smtpPort))
                    {
                        client.EnableSsl = _smtpSsl;
                        client.Credentials = new NetworkCredential(_smtpUsername, _smtpPassword);
                        client.Send(mail);
                    }
                }

                Log.Information("Admission confirmation email sent to {Email} for enrollment {Id}", data.Email, data.EnrollmentID);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to send admission confirmation email to {Email} for enrollment {Id}",
                    data.Email, data.EnrollmentID);
            }
        }

        private static string BuildEmailBody(AdmissionEmailData data)
        {
            string enrollmentDate = data.EnrollmentDate.ToString("dd-MMM-yyyy");

            return string.Format(@"<!DOCTYPE html>
<html>
<head>
<meta charset=""utf-8"">
<meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
<style>
    * {{ margin: 0; padding: 0; box-sizing: border-box; }}
    body {{
        font-family: 'Segoe UI', Arial, Helvetica, sans-serif;
        background-color: #f4f7fc;
        padding: 20px;
    }}
    .email-container {{
        max-width: 600px;
        margin: 0 auto;
        background: #ffffff;
        border-radius: 12px;
        overflow: hidden;
        box-shadow: 0 4px 20px rgba(0,0,0,0.08);
    }}
    .header {{
        background: linear-gradient(135deg, #19376D, #2553A0);
        padding: 32px 28px;
        text-align: center;
    }}
    .header h1 {{
        color: #ffffff;
        font-size: 24px;
        font-weight: 700;
        letter-spacing: -0.5px;
    }}
    .header p {{
        color: rgba(255,255,255,0.85);
        font-size: 14px;
        margin-top: 6px;
    }}
    .body-content {{
        padding: 28px;
    }}
    .greeting {{
        font-size: 16px;
        color: #1e293b;
        margin-bottom: 20px;
        line-height: 1.6;
    }}
    .info-table {{
        width: 100%;
        border-collapse: collapse;
        margin-bottom: 20px;
    }}
    .info-table td {{
        padding: 10px 14px;
        border-bottom: 1px solid #e9edf2;
        font-size: 14px;
        color: #334155;
    }}
    .info-table td:first-child {{
        font-weight: 700;
        color: #19376D;
        width: 40%;
        background-color: #f8fafd;
    }}
    .info-table tr:last-child td {{
        border-bottom: none;
    }}
    .welcome {{
        background-color: #edf2f9;
        border-radius: 8px;
        padding: 18px;
        margin: 20px 0;
        text-align: center;
    }}
    .welcome p {{
        color: #19376D;
        font-size: 15px;
        line-height: 1.6;
        font-weight: 600;
    }}
    .contact-info {{
        background-color: #f8fafd;
        border: 1px solid #e9edf2;
        border-radius: 8px;
        padding: 16px;
        margin-top: 16px;
    }}
    .contact-info h3 {{
        color: #19376D;
        font-size: 14px;
        font-weight: 700;
        margin-bottom: 8px;
    }}
    .contact-info p {{
        color: #64748b;
        font-size: 13px;
        line-height: 1.5;
    }}
    .footer {{
        background-color: #f1f4f9;
        padding: 18px 28px;
        text-align: center;
    }}
    .footer p {{
        color: #94a3b8;
        font-size: 12px;
        line-height: 1.5;
    }}
    @@media only screen and (max-width: 480px) {{
        .header {{ padding: 24px 18px; }}
        .header h1 {{ font-size: 20px; }}
        .body-content {{ padding: 20px 16px; }}
        .info-table td {{ padding: 8px 10px; font-size: 13px; }}
        .info-table td:first-child {{ width: 35%; }}
    }}
</style>
</head>
<body>
<div class=""email-container"">
    <div class=""header"">
        <h1>Springfield Institute of Technology</h1>
        <p>Admission Confirmation</p>
    </div>
    <div class=""body-content"">
        <div class=""greeting"">
            Dear <strong>{0}</strong>,<br><br>
            Thank you for choosing Springfield Institute of Technology. We are pleased to confirm your admission for the academic year {1}.
        </div>
        <table class=""info-table"">
            <tr><td>Student Name</td><td>{0}</td></tr>
            <tr><td>Student ID</td><td>{2}</td></tr>
            <tr><td>Enrollment No</td><td>{3}</td></tr>
            <tr><td>Course</td><td>{4}</td></tr>
            <tr><td>Department</td><td>{5}</td></tr>
            <tr><td>Academic Year</td><td>{1}</td></tr>
            <tr><td>Semester</td><td>{6}</td></tr>
            <tr><td>Admission Date</td><td>{7}</td></tr>
        </table>
        <div class=""welcome"">
            <p>Welcome to Springfield Institute of Technology!<br>We look forward to a bright and successful journey together.</p>
        </div>
        <div class=""contact-info"">
            <h3>Contact Information</h3>
            <p>
                Springfield Institute of Technology<br>
                123 Education Lane, Knowledge Park<br>
                Springfield, SP 400001<br>
                Phone: +91-98765-43210<br>
                Email: admissions@springfield.edu
            </p>
        </div>
    </div>
    <div class=""footer"">
        <p>
            This is an automatically generated email. Please do not reply to this message.<br>
            For any queries, contact our admissions office at admissions@springfield.edu
        </p>
    </div>
</div>
</body>
</html>",
                data.StudentName,
                data.AcademicYear,
                data.StudentID,
                data.EnrollmentID,
                data.CourseName,
                data.Department,
                data.Semester,
                enrollmentDate);
        }
    }
}
