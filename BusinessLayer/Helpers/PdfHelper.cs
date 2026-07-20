using iTextSharp.text;
using iTextSharp.text.pdf;
using Serilog;
using System;
using System.IO;
using BusinessLayer1.Models;

namespace BusinessLayer1.Helpers
{
    public static class PdfHelper
    {
        private static readonly BaseColor PrimaryColor = new BaseColor(25, 55, 109);
        private static readonly BaseColor AccentColor = new BaseColor(220, 38, 38);
        private static readonly BaseColor LightBg = new BaseColor(245, 247, 250);
        private static readonly BaseColor BorderColor = new BaseColor(203, 213, 225);
        private static readonly BaseColor WhiteColor = new BaseColor(255, 255, 255);
        private static readonly BaseColor BlackColor = new BaseColor(0, 0, 0);
        private static readonly BaseColor DarkGrayColor = new BaseColor(100, 100, 100);
        private static readonly BaseColor GrayColor = new BaseColor(128, 128, 128);

        public static byte[] GenerateEnrollmentReceipt(EnrollmentReceipt data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    Document document = new Document(PageSize.A4, 36f, 36f, 40f, 40f);
                    PdfWriter writer = PdfWriter.GetInstance(document, ms);
                    writer.PageEvent = new ReceiptPageEventHandler();

                    document.Open();

                    AddHeader(document, data);
                    AddReceiptInfoLine(document, data);
                    AddStudentDetailsTable(document, data);
                    AddFeeDetailsTable(document, data);
                    AddFooter(document);

                    document.Close();

                    return ms.ToArray();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error generating PDF receipt for enrollment {Id}", data.EnrollmentID);
                throw;
            }
        }

        private static void AddHeader(Document document, EnrollmentReceipt data)
        {
            Font titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 22, PrimaryColor);

            Paragraph collegeName = new Paragraph(data.CollegeName, titleFont)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 14f
            };
            document.Add(collegeName);

            PdfPTable separator = new PdfPTable(1) { WidthPercentage = 100 };
            PdfPCell sepCell = new PdfPCell(new Phrase(" "))
            {
                Border = Rectangle.BOTTOM_BORDER,
                BorderColor = AccentColor,
                BorderWidth = 2f,
                FixedHeight = 4f
            };
            separator.AddCell(sepCell);
            document.Add(separator);
        }

        private static void AddReceiptInfoLine(Document document, EnrollmentReceipt data)
        {
            Font valueFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BlackColor);
            Paragraph dateLine = new Paragraph($"Date: {data.GeneratedDate:dd-MMM-yyyy HH:mm}", valueFont)
            {
                Alignment = Element.ALIGN_RIGHT,
                SpacingBefore = 8f,
                SpacingAfter = 6f
            };
            document.Add(dateLine);
        }

        private static void AddStudentDetailsTable(Document document, EnrollmentReceipt data)
        {
            Font sectionFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, WhiteColor);
            Font labelFont = FontFactory.GetFont(FontFactory.HELVETICA, 10, DarkGrayColor);
            Font valueFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BlackColor);

            PdfPTable table = new PdfPTable(2) { WidthPercentage = 100 };
            table.SetWidths(new float[] { 2f, 3f });

            AddSectionHeader(table, sectionFont, "STUDENT DETAILS", 2);

            AddRow(table, "Student Name", data.StudentName, labelFont, valueFont, false);
            AddRow(table, "Student ID", data.StudentID.ToString(), labelFont, valueFont, false);
            AddRow(table, "Enrollment No", data.EnrollmentID.ToString(), labelFont, valueFont, false);
            AddRow(table, "Course", data.CourseName, labelFont, valueFont, false);
            AddRow(table, "Academic Year", data.AcademicYear, labelFont, valueFont, false);
            AddRow(table, "Semester", data.Semester, labelFont, valueFont, false);
            AddRow(table, "Date of Enrollment", data.EnrollmentDate.ToString("dd-MMM-yyyy"), labelFont, valueFont, true);

            document.Add(table);
        }

        private static void AddFeeDetailsTable(Document document, EnrollmentReceipt data)
        {
            Font sectionFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, WhiteColor);
            Font labelFont = FontFactory.GetFont(FontFactory.HELVETICA, 10, DarkGrayColor);
            Font valueFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, BlackColor);
            Font amountFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, AccentColor);

            PdfPTable table = new PdfPTable(2) { WidthPercentage = 100 };
            table.SetWidths(new float[] { 2f, 3f });
            table.SpacingBefore = 12f;

            AddSectionHeader(table, sectionFont, "FEE DETAILS", 2);

            AddRow(table, "Fee Paid", $"Rs. {data.FeePaid:N2}", labelFont, amountFont, false);

            AddRow(table, "Amount in Words", data.FeeInWords, labelFont, valueFont, false);

            AddRow(table, "Generated Date", data.GeneratedDate.ToString("dd-MMM-yyyy hh:mm tt"), labelFont, valueFont, true);

            document.Add(table);
        }

        private static void AddFooter(Document document)
        {
            document.Add(new Paragraph(" "));

            Font footerFont = FontFactory.GetFont(FontFactory.HELVETICA_OBLIQUE, 10, GrayColor);

            PdfPTable footerLine = new PdfPTable(1) { WidthPercentage = 60, HorizontalAlignment = Element.ALIGN_CENTER };
            PdfPCell lineCell = new PdfPCell(new Phrase(" "))
            {
                Border = Rectangle.TOP_BORDER,
                BorderColor = LightBg,
                BorderWidth = 0.5f,
                FixedHeight = 2f
            };
            footerLine.AddCell(lineCell);
            document.Add(footerLine);

            Paragraph footer = new Paragraph("Computer Generated Receipt", footerFont)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingBefore = 6f
            };
            document.Add(footer);
        }

        private static void AddSectionHeader(PdfPTable table, Font font, string text, int colspan)
        {
            PdfPCell cell = new PdfPCell(new Phrase(text, font))
            {
                Colspan = colspan,
                BackgroundColor = PrimaryColor,
                Border = Rectangle.NO_BORDER,
                PaddingTop = 8f,
                PaddingBottom = 8f,
                PaddingLeft = 10f,
                HorizontalAlignment = Element.ALIGN_LEFT
            };
            table.AddCell(cell);
        }

        private static void AddRow(PdfPTable table, string label, string value, Font labelFont, Font valueFont, bool isLast)
        {
            PdfPCell labelCell = new PdfPCell(new Phrase(label, labelFont))
            {
                Border = Rectangle.BOTTOM_BORDER,
                BorderColor = BorderColor,
                BorderWidth = isLast ? 0f : 0.5f,
                PaddingTop = 6f,
                PaddingBottom = 6f,
                PaddingLeft = 10f,
                BackgroundColor = LightBg
            };
            table.AddCell(labelCell);

            PdfPCell valueCell = new PdfPCell(new Phrase(value, valueFont))
            {
                Border = Rectangle.BOTTOM_BORDER,
                BorderColor = BorderColor,
                BorderWidth = isLast ? 0f : 0.5f,
                PaddingTop = 6f,
                PaddingBottom = 6f,
                PaddingLeft = 10f
            };
            table.AddCell(valueCell);
        }

        private class ReceiptPageEventHandler : PdfPageEventHelper
        {
            public override void OnEndPage(PdfWriter writer, Document document)
            {
                Font pageFont = FontFactory.GetFont(FontFactory.HELVETICA, 8, GrayColor);
                string pageText = $"Page {writer.PageNumber}";
                Rectangle pageSize = document.PageSize;

                PdfContentByte cb = writer.DirectContent;
                cb.BeginText();
                cb.SetFontAndSize(pageFont.BaseFont, 8);
                cb.SetGrayFill(0.5f);
                cb.ShowTextAligned(Element.ALIGN_CENTER, pageText,
                    (pageSize.Left + pageSize.Right) / 2, pageSize.Bottom - 18, 0);
                cb.EndText();
            }
        }
    }
}
