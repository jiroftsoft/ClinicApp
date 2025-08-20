using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using ClinicApp.ViewModels;
using Serilog;

namespace ClinicApp.Helpers
{
    public static class MedicalReportExcelGenerator
    {
        private static readonly ILogger _log = Log.ForContext(typeof(MedicalReportExcelGenerator));
        private const string PersianFontName = "Tahoma";
        private const int PersianFontSize = 10;
        private const string ClinicName = "کلینیک شفا";
        private const string ClinicAddress = "جیرفت، خیابان پرستار، بالاتر از خیابان فاطمی، کوچه 15، پلاک 23";
        private const string ClinicPhone = "021-12345678";
        private const string ReportTitlePrefix = "گزارش استفاده از خدمات پزشکی";
        private const string LogoPath = "wwwroot/images/clinic-logo.png"; // مسیر لوگو

        public static byte[] GenerateServiceUsageReport(
            ServiceDetailsViewModel serviceDetails,
            ServiceUsageStatistics statistics,
            DateTime startDate,
            DateTime endDate)
        {
            _log.Information(
                "شروع تولید گزارش اکسل برای خدمات {ServiceId} - {ServiceTitle} در بازه {StartDate} تا {EndDate}",
                serviceDetails.ServiceId,
                serviceDetails.Title,
                startDate,
                endDate);

            try
            {
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("گزارش استفاده");
                    ConfigureWorksheet(worksheet);
                    AddReportHeader(worksheet, serviceDetails, startDate, endDate);
                    AddLogo(worksheet);
                    AddSummaryStatistics(worksheet, serviceDetails, statistics, 6);
                    AddDailyStatisticsTable(worksheet, statistics, 12);
                    AddAdvancedChartAsImage(worksheet, statistics, 12 + statistics.DailyUsage.Count + 5);

                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        var fileBytes = stream.ToArray();

                        _log.Information(
                            "گزارش اکسل برای خدمات {ServiceId} - {ServiceTitle} با موفقیت تولید شد. حجم فایل: {FileSize} بایت",
                            serviceDetails.ServiceId,
                            serviceDetails.Title,
                            fileBytes.Length);

                        return fileBytes;
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error(
                    ex,
                    "خطا در تولید گزارش اکسل برای خدمات {ServiceId} - {ServiceTitle}",
                    serviceDetails?.ServiceId ?? 0,
                    serviceDetails?.Title ?? "نامشخص");

                throw new Exception("خطا در تولید گزارش اکسل. لطفاً با پشتیبانی تماس بگیرید.", ex);
            }
        }

        private static void ConfigureWorksheet(IXLWorksheet worksheet)
        {
            // راست به چپ کردن محتوای سلول‌ها
            worksheet.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            worksheet.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            worksheet.Style.Font.FontName = PersianFontName;
            worksheet.Style.Font.FontSize = PersianFontSize;

            // عرض ستون‌ها
            for (int col = 1; col <= 5; col++)
                worksheet.Column(col).Width = 20;

            // تنظیمات صفحه برای چاپ
            worksheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
            worksheet.PageSetup.PagesWide = 1; // جایگزین FitToPagesWide
            worksheet.PageSetup.PagesTall = 0; // جایگزین FitToPagesTall

            // Header و Footer
            worksheet.PageSetup.Header.Left.AddText(ClinicName + " - " + ReportTitlePrefix);
            worksheet.PageSetup.Footer.Right.AddText("صفحه &P از &N");
        }



        private static void AddReportHeader(
            IXLWorksheet worksheet,
            ServiceDetailsViewModel serviceDetails,
            DateTime startDate,
            DateTime endDate)
        {
            worksheet.Cell(1, 1).Value = ClinicName;
            worksheet.Range(1, 1, 1, 5).Merge();
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Font.FontSize = 16;
            worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            worksheet.Cell(2, 1).Value = ClinicAddress;
            worksheet.Range(2, 1, 2, 5).Merge();
            worksheet.Cell(2, 1).Style.Font.FontSize = 10;
            worksheet.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            worksheet.Cell(3, 1).Value = $"تلفن: {ClinicPhone}";
            worksheet.Range(3, 1, 3, 5).Merge();
            worksheet.Cell(3, 1).Style.Font.FontSize = 10;
            worksheet.Cell(3, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            worksheet.Cell(4, 1).Value = $"{ReportTitlePrefix}: {serviceDetails.Title}";
            worksheet.Range(4, 1, 4, 5).Merge();
            worksheet.Cell(4, 1).Style.Font.Bold = true;
            worksheet.Cell(4, 1).Style.Font.FontSize = 14;
            worksheet.Cell(4, 1).Style.Fill.BackgroundColor = XLColor.FromTheme(XLThemeColor.Accent2);
            worksheet.Cell(4, 1).Style.Font.FontColor = XLColor.White;
            worksheet.Cell(4, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            worksheet.Cell(5, 1).Value = $"بازه زمانی: {startDate.ToPersianDate()} تا {endDate.ToPersianDate()}";
            worksheet.Range(5, 1, 5, 5).Merge();
            worksheet.Cell(5, 1).Style.Font.Bold = true;
            worksheet.Cell(5, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }

        private static void AddLogo(IXLWorksheet worksheet)
        {
            // استفاده از Path.Combine برای سازگاری با تمام سیستم‌ها
            var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "clinic-logo.png");

            if (!File.Exists(logoPath))
                return;

            using (var imageStream = new FileStream(LogoPath, FileMode.Open, FileAccess.Read))
            {
                worksheet.AddPicture(imageStream).MoveTo(worksheet.Cell(1, 6)).WithSize(100, 100);
            }
        }

        private static void AddSummaryStatistics(
            IXLWorksheet worksheet,
            ServiceDetailsViewModel serviceDetails,
            ServiceUsageStatistics statistics,
            int startRow)
        {
            worksheet.Cell(startRow, 1).Value = "آمار کلی";
            worksheet.Range(startRow, 1, startRow, 4).Merge();
            worksheet.Cell(startRow, 1).Style.Font.Bold = true;
            worksheet.Cell(startRow, 1).Style.Font.FontSize = 12;
            worksheet.Cell(startRow, 1).Style.Fill.BackgroundColor = XLColor.LightGray;
            worksheet.Cell(startRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            startRow++;
            AddStatisticRow(worksheet, startRow++, "تعداد کل استفاده‌ها", statistics.TotalUsage.ToString("N0"));
            AddStatisticRow(worksheet, startRow++, "درآمد کل", $"{statistics.TotalRevenue:N0} تومان");
            AddStatisticRow(worksheet, startRow++, "کد خدمات", serviceDetails.ServiceCode);
            AddStatisticRow(worksheet, startRow++, "دسته‌بندی", serviceDetails.ServiceCategoryTitle);
            AddStatisticRow(worksheet, startRow++, "تاریخ ایجاد گزارش", DateTime.Now.ToPersianDateTime());
        }

        private static void AddStatisticRow(IXLWorksheet worksheet, int row, string label, string value)
        {
            worksheet.Cell(row, 1).Value = label;
            worksheet.Cell(row, 1).Style.Font.Bold = true;
            worksheet.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.LightBlue;

            worksheet.Cell(row, 2).Value = value;
            worksheet.Cell(row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
        }

        private static void AddDailyStatisticsTable(
      IXLWorksheet worksheet,
      ServiceUsageStatistics statistics,
      int startRow)
        {
            // عنوان جدول
            worksheet.Cell(startRow, 1).Value = "آمار روزانه استفاده از خدمات";
            worksheet.Range(startRow, 1, startRow, 3).Merge();
            worksheet.Cell(startRow, 1).Style.Font.Bold = true;
            worksheet.Cell(startRow, 1).Style.Font.FontSize = 12;
            worksheet.Cell(startRow, 1).Style.Fill.BackgroundColor = XLColor.LightGray;
            worksheet.Cell(startRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            startRow++;

            // تعریف سرستون‌ها
            var headers = new[] { "تاریخ (شمسی)", "تعداد استفاده", "درآمد (تومان)" };
            for (int col = 1; col <= headers.Length; col++)
            {
                worksheet.Cell(startRow, col).Value = headers[col - 1];
                worksheet.Cell(startRow, col).Style.Font.Bold = true;
                worksheet.Cell(startRow, col).Style.Fill.BackgroundColor = XLColor.FromTheme(XLThemeColor.Accent1);
                worksheet.Cell(startRow, col).Style.Font.FontColor = XLColor.White;
            }

            startRow++;

            // بررسی وجود داده
            if (statistics.DailyUsage == null || !statistics.DailyUsage.Any())
            {
                worksheet.Cell(startRow, 1).Value = "داده‌ای برای نمایش وجود ندارد.";
                worksheet.Range(startRow, 1, startRow, 3).Merge();
                worksheet.Cell(startRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(startRow, 1).Style.Font.Italic = true;
                return;
            }

            int dataStartRow = startRow;

            // مرتب‌سازی تاریخ‌ها
            var sortedDates = statistics.DailyUsage.Keys.OrderBy(d => d).ToList();

            foreach (var date in sortedDates)
            {
                worksheet.Cell(startRow, 1).Value = date;
                worksheet.Cell(startRow, 2).Value = statistics.DailyUsage[date];
                worksheet.Cell(startRow, 3).Value = statistics.DailyRevenue[date];
                startRow++;
            }

            // ایجاد جدول فقط در صورت وجود داده
            var tableRange = worksheet.Range(dataStartRow - 1, 1, startRow - 1, 3);
            var table = tableRange.CreateTable();

            // فرمت عددی با استفاده از ایندکس ستون (بدون استفاده از نام فارسی)
            table.Column(2).Style.NumberFormat.Format = "0";        // تعداد استفاده
            table.Column(3).Style.NumberFormat.Format = "#,##0";    // درآمد (تومان)

            table.ShowAutoFilter = true;
        }

        private static void AddAdvancedChartAsImage(
       IXLWorksheet worksheet,
       ServiceUsageStatistics statistics,
       int startRow)
        {
            const int width = 800;
            const int height = 400;
            const int maxLabelCount = 8;

            using (var bmp = new Bitmap(width, height))
            using (var g = Graphics.FromImage(bmp))
            {
                using (var titleFont = new Font("Tahoma", 12, FontStyle.Bold))
                using (var labelFont = new Font("Tahoma", 9))
                using (var axisFont = new Font("Tahoma", 8))
                {
                    g.Clear(Color.White);
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

                    const int marginLeft = 80;
                    const int marginBottom = 60;
                    const int marginTop = 50;
                    const int marginRight = 40;
                    int chartWidth = width - marginLeft - marginRight;
                    int chartHeight = height - marginTop - marginBottom;

                    var dates = statistics.DailyUsage?.Keys.OrderBy(d => d).ToList() ?? new List<string>();
                    var usageValues = dates.Select(d => statistics.DailyUsage[d]).ToList();
                    var revenueValues = dates.Select(d => statistics.DailyRevenue[d]).ToList();

                    if (!dates.Any())
                    {
                        DrawNoDataMessage(g, width, height, labelFont);
                        goto SaveAndAddToSheet;
                    }

                    int maxUsage = Math.Max(1, usageValues.Max());
                    decimal maxRevenue = Math.Max(1, revenueValues.Max());

                    using (var axisPen = new Pen(Color.Black, 2))
                    using (var gridPen = new Pen(Color.LightGray, 1))
                    {
                        g.DrawLine(axisPen, marginLeft, marginTop + chartHeight, marginLeft + chartWidth, marginTop + chartHeight);
                        g.DrawLine(axisPen, marginLeft, marginTop + chartHeight, marginLeft, marginTop);
                        g.DrawLine(new Pen(Color.Orange, 2), marginLeft + chartWidth, marginTop + chartHeight, marginLeft + chartWidth, marginTop);

                        DrawHorizontalGridLines(g, gridPen, marginLeft, marginTop, chartWidth, chartHeight, 5);
                    }

                    int pointCount = dates.Count;
                    int barWidth = Math.Max(10, Math.Min(30, chartWidth / Math.Max(1, pointCount) - 2));
                    int spacing = Math.Max(5, (chartWidth - (pointCount * barWidth)) / Math.Max(1, pointCount - 1));

                    var labelIndices = GetOptimalLabelIndices(pointCount, maxLabelCount);

                    for (int i = 0; i < pointCount; i++)
                    {
                        int x = marginLeft + i * (barWidth + spacing);

                        double usageRatio = (double)usageValues[i] / maxUsage;
                        int barHeight = (int)(chartHeight * usageRatio);
                        var barRect = new Rectangle(x, marginTop + chartHeight - barHeight, barWidth, barHeight);
                        using (var brush = new SolidBrush(Color.FromArgb(100, 30, 144, 255)))
                        {
                            g.FillRectangle(brush, barRect);
                        }
                        g.DrawRectangle(Pens.SteelBlue, barRect);

                        double revenueRatio = (double)(revenueValues[i] / maxRevenue);
                        int yRevenue = marginTop + chartHeight - (int)(chartHeight * revenueRatio);
                        var revenuePoint = new Rectangle(x + barWidth / 2 - 4, yRevenue - 4, 8, 8);
                        g.FillEllipse(Brushes.Orange, revenuePoint);
                        g.DrawEllipse(Pens.DarkOrange, revenuePoint);
                    }

                    foreach (int index in labelIndices)
                    {
                        if (index >= dates.Count) continue;

                        string dateLabel = FormatDateLabel(dates[index]);
                        var stringSize = g.MeasureString(dateLabel, axisFont);
                        int x = marginLeft + index * (barWidth + spacing);
                        float labelX = x - stringSize.Width / 2 + barWidth / 2;
                        labelX = Math.Max(marginLeft, Math.Min(labelX, width - marginRight - stringSize.Width));

                        g.DrawString(dateLabel, axisFont, Brushes.Black, labelX, marginTop + chartHeight + 10);
                    }

                    DrawVerticalAxisLabels(g, axisFont, marginLeft, marginTop, chartHeight, (double)maxUsage, "تعداد");
                    DrawVerticalAxisLabels(g, axisFont, marginLeft + chartWidth, marginTop, chartHeight, (double)maxRevenue, "درآمد");

                    string title = "تحلیل روزانه تعداد استفاده و درآمد خدمات";
                    var titleSize = g.MeasureString(title, titleFont);
                    g.DrawString(title, titleFont, Brushes.DarkBlue, (width - titleSize.Width) / 2, 10);

                    DrawChartLegend(g, labelFont, width - 150, height - 30);

                SaveAndAddToSheet:
                    using (var ms = new MemoryStream())
                    {
                        bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                        ms.Position = 0;
                        var picture = worksheet.AddPicture(ms);
                        picture.MoveTo(worksheet.Cell(startRow, 1));
                        picture.WithSize(width, height);
                    }
                }
            }
        }

        private static void DrawNoDataMessage(Graphics g, int width, int height, Font font)
        {
            string message = "داده‌ای برای نمایش وجود ندارد";
            var size = g.MeasureString(message, font);
            g.DrawString(message, font, Brushes.Gray, (width - size.Width) / 2, (height - size.Height) / 2);

            using (var pen = new Pen(Color.LightGray, 2))
            {
                var rect = new Rectangle(
                    (int)((width - size.Width) / 2 - 10),
                    (int)((height - size.Height) / 2 - 10),
                    (int)(size.Width + 20),
                    (int)(size.Height + 20)
                );
                g.DrawRectangle(pen, rect);
            }
        }

        private static void DrawHorizontalGridLines(Graphics g, Pen pen, int marginLeft, int marginTop,
            int chartWidth, int chartHeight, int lineCount)
        {
            for (int i = 1; i < lineCount; i++)
            {
                int y = marginTop + (chartHeight * i / lineCount);
                g.DrawLine(pen, marginLeft, y, marginLeft + chartWidth, y);
            }
        }

        private static List<int> GetOptimalLabelIndices(int totalPoints, int maxLabels)
        {
            var indices = new List<int>();

            if (totalPoints <= maxLabels)
            {
                for (int i = 0; i < totalPoints; i++)
                    indices.Add(i);
            }
            else
            {
                indices.Add(0);
                int step = Math.Max(1, totalPoints / (maxLabels - 2));
                for (int i = step; i < totalPoints - 1; i += step)
                {
                    indices.Add(i);
                }
                if (totalPoints > 1)
                    indices.Add(totalPoints - 1);
            }

            return indices.Distinct().OrderBy(x => x).ToList();
        }

        private static string FormatDateLabel(string persianDate)
        {
            if (string.IsNullOrEmpty(persianDate))
                return "";

            var parts = persianDate.Split('/');
            if (parts.Length >= 3)
            {
                return $"{parts[1]}/{parts[2]}";
            }
            return persianDate.Length > 5 ? persianDate.Substring(persianDate.Length - 5) : persianDate;
        }

        private static void DrawVerticalAxisLabels(Graphics g, Font font, int xPosition, int marginTop,
            int chartHeight, double maxValue, string labelPrefix)
        {
            for (int i = 0; i <= 4; i++)
            {
                int y = marginTop + chartHeight - (i * chartHeight / 4);
                string label = FormatAxisValue(maxValue * i / 4.0, labelPrefix);
                var size = g.MeasureString(label, font);

                if (xPosition < 400)
                {
                    g.DrawString(label, font, Brushes.Black, xPosition - size.Width - 5, y - size.Height / 2);
                }
                else
                {
                    g.DrawString(label, font, Brushes.OrangeRed, xPosition + 5, y - size.Height / 2);
                }
            }
        }

        private static string FormatAxisValue(double value, string prefix)
        {
            if (value >= 1000000)
                return $"{prefix}: {(value / 1000000):F1}M";
            if (value >= 1000)
                return $"{prefix}: {(value / 1000):F0}K";
            return $"{prefix}: {value:F0}";
        }

        private static void DrawChartLegend(Graphics g, Font font, int x, int y)
        {
            g.FillRectangle(Brushes.LightBlue, x, y - 15, 15, 15);
            g.DrawRectangle(Pens.SteelBlue, x, y - 15, 15, 15);
            g.DrawString("تعداد استفاده", font, Brushes.Black, x + 20, y - 15);

            g.FillEllipse(Brushes.Orange, x + 120, y - 15, 15, 15);
            g.DrawEllipse(Pens.DarkOrange, x + 120, y - 15, 15, 15);
            g.DrawString("درآمد", font, Brushes.Black, x + 140, y - 15);
        }
    }
}
