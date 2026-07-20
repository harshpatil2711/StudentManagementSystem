using BusinessLayer1.DAL;
using BusinessLayer1.Helpers;
using Serilog;
using System;
using System.Web.Mvc;
using WebApplication5.Filters;

namespace WebApplication5.Controllers
{
    [RoleAuthorize]
    public class ReportController : Controller
    {
        [HttpGet]
        public ActionResult DownloadReceipt(int id)
        {
            try
            {
                EnrollmentReceiptDAL dal = new EnrollmentReceiptDAL();
                var receiptData = dal.GetReceiptData(id);

                if (receiptData == null)
                {
                    Log.Warning("Receipt data not found for enrollment {Id}", id);
                    return HttpNotFound("Enrollment not found.");
                }

                byte[] pdfBytes = PdfHelper.GenerateEnrollmentReceipt(receiptData);

                string fileName = $"EnrollmentReceipt_{id}.pdf";

                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error generating receipt PDF for enrollment {Id}", id);
                return new HttpStatusCodeResult(500, "An error occurred while generating the receipt. Please try again.");
            }
        }
    }
}
