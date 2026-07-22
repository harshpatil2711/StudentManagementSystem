using Serilog;
using System;
using System.Configuration;
using System.IO;
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
            string templatePath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, "EmailTemplates", "AdmissionConfirmation.html");

            if (!File.Exists(templatePath))
            {
                throw new FileNotFoundException(
                    "Email template not found at: " + templatePath, templatePath);
            }

            string html = File.ReadAllText(templatePath);

            html = html.Replace("{{StudentName}}", data.StudentName);
            html = html.Replace("{{StudentID}}", data.StudentID.ToString());
            html = html.Replace("{{EnrollmentID}}", data.EnrollmentID.ToString());
            html = html.Replace("{{CourseName}}", data.CourseName);
            html = html.Replace("{{Department}}", data.Department);
            html = html.Replace("{{AcademicYear}}", data.AcademicYear);
            html = html.Replace("{{Semester}}", data.Semester);
            html = html.Replace("{{EnrollmentDate}}", data.EnrollmentDate.ToString("dd-MMM-yyyy"));

            return html;
        }
    }
}
