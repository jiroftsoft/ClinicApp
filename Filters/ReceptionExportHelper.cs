using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using ClinicApp.ViewModels;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Color = System.Drawing.Color;

namespace ClinicApp.Filters
{
    public static class ReceptionExportHelper
    {
        // 📌 Export به Excel با EPPlus
        public static byte[] ExportToExcel(IEnumerable<ReceptionIndexViewModel> items)
        {
            var list = items.ToList(); // تبدیل به List برای Count و index

            using (var package = new ExcelPackage())
            {
                var sheet = package.Workbook.Worksheets.Add("Receptions");

                // هدر
                string[] headers = { "شماره پذیرش", "نام بیمار", "نام پزشک", "تاریخ", "مبلغ کل", "وضعیت" };
                for (int i = 0; i < headers.Length; i++)
                {
                    sheet.Cells[1, i + 1].Value = headers[i];
                    sheet.Cells[1, i + 1].Style.Font.Bold = true;
                    sheet.Cells[1, i + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    sheet.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                    sheet.Cells[1, i + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }

                // داده‌ها
                for (int i = 0; i < list.Count; i++)
                {
                    var r = list[i];
                    var row = i + 2;

                    if (row % 2 == 0)
                        sheet.Row(row).Style.Fill.BackgroundColor.SetColor(Color.FromArgb(240, 240, 240));

                    sheet.Cells[row, 1].Value = r.ReceptionId;
                    sheet.Cells[row, 2].Value = r.PatientFullName;
                    sheet.Cells[row, 3].Value = r.DoctorFullName;
                    sheet.Cells[row, 4].Value = r.ReceptionDate;
                    sheet.Cells[row, 5].Value = r.TotalAmount;
                    sheet.Cells[row, 6].Value = r.Status;

                    sheet.Cells[row, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    sheet.Cells[row, 5].Style.Numberformat.Format = "#,##0";
                    sheet.Cells[row, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                }

                sheet.Cells[sheet.Dimension.Address].AutoFitColumns();
                return package.GetAsByteArray();
            }
        }

        // 📌 Export به PDF با QuestPDF
        public static byte[] ExportToPdf(IEnumerable<ReceptionIndexViewModel> items)
        {
            var list = items.ToList();
            var fontPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot", "fonts", "Vazir.ttf");

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(20);
                    page.DefaultTextStyle(x => x.FontFamily(fontPath).FontSize(11));

                    page.Header()
                        .Text("لیست پذیرش‌ها")
                        .FontSize(16)
                        .SemiBold()
                        .AlignCenter();

                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(60); // شماره پذیرش
                            columns.RelativeColumn();   // نام بیمار
                            columns.RelativeColumn();   // نام پزشک
                            columns.ConstantColumn(70); // تاریخ
                            columns.ConstantColumn(80); // مبلغ کل
                            columns.ConstantColumn(60); // وضعیت
                        });

                        // هدر جدول
                        string[] headers = { "شماره پذیرش", "نام بیمار", "نام پزشک", "تاریخ", "مبلغ کل", "وضعیت" };
                        table.Header(header =>
                        {
                            foreach (var h in headers)
                                header.Cell().Element(cell => cell.Background(Colors.Grey.Lighten3).Padding(5))
                                      .Text(h)
                                      .SemiBold()
                                      .FontSize(12)
                                      .AlignCenter();
                        });

                        // داده‌ها
                        foreach (var r in list)
                        {
                            table.Cell().Element(CellStyle).Text(r.ReceptionId.ToString()).AlignCenter();
                            table.Cell().Element(CellStyle).Text(r.PatientFullName).AlignLeft();
                            table.Cell().Element(CellStyle).Text(r.DoctorFullName).AlignLeft();
                            table.Cell().Element(CellStyle).Text(r.ReceptionDate).AlignCenter();
                            table.Cell().Element(CellStyle).Text(r.TotalAmount.ToString("N0")).AlignRight();
                            table.Cell().Element(CellStyle).Text(r.Status).AlignCenter();
                        }
                    });
                });
            });

            return document.GeneratePdf();
        }

        private static IContainer CellStyle(IContainer container) =>
            container.Padding(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2);


       
    }
}
