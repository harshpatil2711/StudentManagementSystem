using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Serilog;
using System;
using BusinessLayer1.Models;

namespace BusinessLayer1.Helpers
{
    public static class PdfHelper
    {
        public static byte[] GenerateEnrollmentReceipt(EnrollmentReceipt data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            try
            {
                return Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(36);

                        page.Header().Column(header =>
                        {
                            header.Item().PaddingBottom(4).PaddingTop(8).AlignCenter()
                                .Text(data.CollegeName)
                                .FontSize(22).Bold().FontColor(Colors.Blue.Darken3);
                            header.Item().LineHorizontal(2).LineColor(Colors.Red.Medium);
                        });

                        page.Content().Column(content =>
                        {
                            content.Item().PaddingVertical(6).AlignCenter()
                                .Text("ENROLLMENT RECEIPT")
                                .FontSize(14).Bold();

                            content.Item().PaddingBottom(6).AlignRight()
                                .Text("Date: " + data.GeneratedDate.ToString("dd-MMM-yyyy HH:mm"));

                            BuildInfoTable(content, data);
                        });

                        page.Footer().Column(footer =>
                        {
                            footer.Item().LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);
                            footer.Item().PaddingTop(6).AlignCenter()
                                .Text("Computer Generated Receipt")
                                .FontSize(10).Italic().FontColor(Colors.Grey.Medium);
                            footer.Item().PaddingTop(2).AlignCenter()
                                .Text(x =>
                                {
                                    x.DefaultTextStyle(s => s.FontSize(9).FontColor(Colors.Grey.Medium));
                                    x.CurrentPageNumber();
                                });
                        });
                    });
                }).GeneratePdf();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error generating PDF receipt for enrollment {Id}", data.EnrollmentID);
                throw;
            }
        }

        private static void BuildInfoTable(ColumnDescriptor content, EnrollmentReceipt data)
        {
            string labelsBg = Colors.Grey.Lighten4;
            string headerBg = Colors.Blue.Darken3;
            string borderColor = Colors.Grey.Lighten2;
            string labelFontColor = Colors.Grey.Darken1;
            string valueFontColor = Colors.Black;

            BuildSection(content, "STUDENT DETAILS", labelsBg, headerBg, borderColor, labelFontColor, valueFontColor,
                new string[] { "Student Name", data.StudentName },
                new string[] { "Student ID", data.StudentID.ToString() },
                new string[] { "Enrollment No", data.EnrollmentID.ToString() },
                new string[] { "Course", data.CourseName },
                new string[] { "Academic Year", data.AcademicYear },
                new string[] { "Semester", data.Semester },
                new string[] { "Date of Enrollment", data.EnrollmentDate.ToString("dd-MMM-yyyy") });

            BuildSection(content, "FEE DETAILS", labelsBg, headerBg, borderColor, labelFontColor, valueFontColor,
                new string[] { "Fee Paid", "Rs. " + data.FeePaid.ToString("N2") },
                new string[] { "Amount in Words", data.FeeInWords },
                new string[] { "Generated Date", data.GeneratedDate.ToString("dd-MMM-yyyy hh:mm tt") });
        }

        private static void BuildSection(
            ColumnDescriptor content, string title,
            string labelsBg, string headerBg, string borderColor,
            string labelFontColor, string valueFontColor,
            params string[][] rows)
        {
            content.Item().PaddingTop(8).Table(table =>
            {
                table.ColumnsDefinition(cols =>
                {
                    cols.ConstantColumn(130);
                    cols.RelativeColumn();
                });

                table.Cell().ColumnSpan(2)
                    .Background(headerBg).Padding(8)
                    .Text(title).FontSize(11).FontColor(Colors.White);

                for (int i = 0; i < rows.Length; i++)
                {
                    string[] row = rows[i];
                    string label = row[0];
                    string value = row[1];
                    bool isLast = i == rows.Length - 1;
                    bool isFeeAmount = title == "FEE DETAILS" && i == 0;
                    float bottomWidth = isLast ? 0f : 0.5f;

                    table.Cell().Background(labelsBg).PaddingVertical(5).PaddingHorizontal(8)
                        .BorderBottom(bottomWidth).BorderColor(borderColor)
                        .Text(label).FontSize(10).FontColor(labelFontColor);

                    table.Cell().PaddingVertical(5).PaddingHorizontal(8)
                        .BorderBottom(bottomWidth).BorderColor(borderColor)
                        .Text(value).FontSize(10)
                        .FontColor(isFeeAmount ? Colors.Red.Medium : valueFontColor);
                }
            });
        }
    }
}
