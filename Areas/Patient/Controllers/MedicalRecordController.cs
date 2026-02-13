using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Areas.Patient.Controllers.Base;
using ClinicApp.Filters;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Models;
using ClinicApp.ViewModels.Patient.MedicalRecord;
using Microsoft.AspNet.Identity;
using Serilog;
using System.IO;
using System.Text;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace ClinicApp.Areas.Patient.Controllers
{
    /// <summary>
    /// Controller برای پرونده الکترونیک بیمار
    /// Single Responsibility: فقط Orchestration
    /// ✅ AJAX-Compatible
    /// ✅ Enterprise-Grade: Authorization, ServiceResult Enhanced
    /// </summary>
    [Authorize]
    [NoCache]
    public class MedicalRecordController : BasePatientController
    {
        private readonly IPatientMedicalRecordService _medicalRecordService;
        
        public MedicalRecordController(
            IPatientMedicalRecordService medicalRecordService,
            ILogger logger,
            ICurrentUserService currentUserService,
            ApplicationDbContext context)
            : base(logger, currentUserService, context)
        {
            _medicalRecordService = medicalRecordService ?? 
                throw new ArgumentNullException(nameof(medicalRecordService));
        }
        
        /// <summary>
        /// ✅ BULLETPROOF: Enhanced AJAX request detection
        /// Checks multiple sources: Request.IsAjaxRequest() + Custom Header + Query String
        /// Healthcare-Grade: Must work across all ASP.NET configurations
        /// </summary>
        private bool IsAjaxRequestEnhanced()
        {
            if (Request.IsAjaxRequest())
                return true;
            
            if (Request.Headers["X-AJAX-Request"] == "true")
                return true;
            
            if (Request.QueryString["ajax"] == "1")
                return true;
            
            return false;
        }
        
        /// <summary>
        /// نمایش صفحه اصلی پرونده الکترونیک
        /// GET: /Patient/MedicalRecord
        /// ✅ AJAX-Compatible: پشتیبانی از درخواست‌های AJAX
        /// ✅ BULLETPROOF: Enhanced AJAX detection to prevent layout duplication
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            try
            {
                var userId = User.Identity.GetUserId();
                _logger.Information("درخواست نمایش پرونده الکترونیک - UserId: {UserId}, IsAjax: {IsAjax}", 
                    userId, IsAjaxRequestEnhanced());
                
                var patientId = await GetCurrentPatientIdAsync();
                if (patientId == null)
                {
                    _logger.Warning("⚠️ MedicalRecord access denied - patientId is null. UserId: {UserId}", 
                        userId);
                    
                    if (IsAjaxRequestEnhanced())
                    {
                        Response.StatusCode = 401;
                        return Json(new { 
                            success = false, 
                            message = "اطلاعات بیمار یافت نشد. لطفاً دوباره وارد شوید.",
                            code = "UNAUTHORIZED",
                            redirectUrl = "/Account/Login" // ✅ FIXED: Absolute path for cross-area navigation
                        }, JsonRequestBehavior.AllowGet);
                    }
                    NotificationHelper.SetError(TempData, "اطلاعات بیمار یافت نشد. لطفاً دوباره وارد شوید.");
                    return RedirectToAction("Login", "Account", new { area = "" });
                }
                
                // ✅ AJAX Request: Return Partial View (بدون Layout) - CRITICAL for preventing layout duplication
                if (IsAjaxRequestEnhanced())
                {
                    _logger.Information("✅ Returning PartialView for AJAX request");
                    return PartialView("_MedicalRecordShell", new MedicalRecordIndexViewModel
                    {
                        PatientId = patientId.Value,
                        MedicalHistories = null // Will be loaded via AJAX
                    });
                }
                
                // ✅ Normal Request: Return Full View (با Layout)
                var result = await _medicalRecordService.GetMedicalRecordAsync(patientId.Value);
                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message);
                    return View(new MedicalRecordIndexViewModel());
                }
                
                return View(result.Data);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در نمایش پرونده الکترونیک");
                if (IsAjaxRequestEnhanced())
                {
                    return Json(new { success = false, message = "خطا در بارگذاری پرونده الکترونیک" }, 
                        JsonRequestBehavior.AllowGet);
                }
                NotificationHelper.SetError(TempData, "خطا در بارگذاری پرونده الکترونیک");
                return View(new MedicalRecordIndexViewModel());
            }
        }
        
        /// <summary>
        /// Render Partial View for AJAX requests
        /// POST: /Patient/MedicalRecord/RenderPartial
        /// </summary>
        [HttpPost]
        public ActionResult RenderPartial(string partialName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(partialName))
                {
                    return new HttpStatusCodeResult(400, "Partial name is required");
                }
                
                // ✅ Security: Only allow specific partials
                var allowedPartials = new[] { 
                    "_MedicalHistorySection",
                    "_AppointmentsSection",
                    "_ReceptionsSection",
                    "_TriageSection"
                };
                
                if (!allowedPartials.Contains(partialName))
                {
                    return new HttpStatusCodeResult(403, "Partial not allowed");
                }
                
                // ✅ Read JSON data from request body (Reset stream در صورت مصرف شدن توسط فیلتر/مدل‌بایندر)
                string jsonData = null;
                if (Request.InputStream != null)
                {
                    if (Request.InputStream.CanSeek)
                        Request.InputStream.Position = 0;
                    using (var reader = new System.IO.StreamReader(Request.InputStream, System.Text.Encoding.UTF8, true, 1024, true))
                    {
                        jsonData = reader.ReadToEnd();
                    }
                }
                
                object model = null;
                if (!string.IsNullOrWhiteSpace(jsonData))
                {
                    var jsonSettings = new Newtonsoft.Json.JsonSerializerSettings
                    {
                        NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore
                    };
                    try
                    {
                        // ✅ Strongly-typed deserialization for partial views
                        if (partialName == "_TriageSection")
                        {
                            model = Newtonsoft.Json.JsonConvert.DeserializeObject<List<MedicalRecordTriageViewModel>>(jsonData, jsonSettings)
                                ?? new List<MedicalRecordTriageViewModel>();
                        }
                        else if (partialName == "_MedicalHistorySection")
                        {
                            model = Newtonsoft.Json.JsonConvert.DeserializeObject<List<MedicalHistoryViewModel>>(jsonData, jsonSettings)
                                ?? new List<MedicalHistoryViewModel>();
                        }
                        else
                        {
                            model = Newtonsoft.Json.JsonConvert.DeserializeObject(jsonData, jsonSettings);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Warning(ex, "RenderPartial deserialization failed for {PartialName}, using empty list. JsonLength: {Length}", partialName, jsonData?.Length ?? 0);
                        if (partialName == "_TriageSection")
                            model = new List<MedicalRecordTriageViewModel>();
                        else if (partialName == "_MedicalHistorySection")
                            model = new List<MedicalHistoryViewModel>();
                    }
                }
                
                if (partialName == "_MedicalHistorySection" && model is List<MedicalHistoryViewModel> list)
                    _logger.Debug("RenderPartial _MedicalHistorySection - Model count: {Count}", list.Count);
                
                return PartialView(partialName, model);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در render partial: {PartialName}", partialName);
                return new HttpStatusCodeResult(500, "Error rendering partial");
            }
        }
        
        /// <summary>
        /// Export Medical Record to PDF
        /// GET: /Patient/MedicalRecord/ExportPdf
        /// ✅ Enterprise-Grade: استفاده از QuestPDF
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> ExportPdf()
        {
            try
            {
                var patientId = await GetCurrentPatientIdAsync();
                if (patientId == null)
                {
                    NotificationHelper.SetError(TempData, "اطلاعات بیمار یافت نشد");
                    return RedirectToAction("Index");
                }
                
                var result = await _medicalRecordService.GetMedicalRecordAsync(patientId.Value);
                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message);
                    return RedirectToAction("Index");
                }
                
                var viewModel = result.Data;
                
                // ✅ Generate PDF using QuestPDF
                QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
                
                var pdfBytes = QuestPDF.Fluent.Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(56.69f); // 2cm in points (2 * 28.35)
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Vazir"));
                        
                        page.Header()
                            .Text("پرونده الکترونیک سلامت")
                            .FontSize(16)
                            .Bold()
                            .AlignCenter();
                        
                        page.Content()
                            .PaddingVertical(28.35f) // 1cm in points
                            .Column(column =>
                            {
                                column.Spacing(28.35f); // 1cm in points
                                
                                // Patient Info
                                column.Item().Text($"بیمار: {viewModel.PatientFullName}")
                                    .FontSize(12)
                                    .Bold();
                                
                                // Medical Histories
                                if (viewModel.MedicalHistories != null && viewModel.MedicalHistories.Any())
                                {
                                    column.Item()
                                        .PaddingBottom(14.17f) // 0.5cm in points
                                        .Text("تاریخچه پزشکی")
                                        .FontSize(14)
                                        .Bold();
                                    
                                    foreach (var history in viewModel.MedicalHistories)
                                    {
                                        column.Item().BorderBottom(1).PaddingBottom(14.17f) // 0.5cm in points
                                            .Column(col =>
                                            {
                                                col.Item().Text($"{history.TypeText}: {history.Title}").Bold();
                                                if (!string.IsNullOrWhiteSpace(history.Description))
                                                {
                                                    col.Item().Text(history.Description).FontSize(9);
                                                }
                                                if (history.StartDate.HasValue)
                                                {
                                                    col.Item().Text($"تاریخ شروع: {history.StartDateShamsi}").FontSize(9);
                                                }
                                            });
                                    }
                                }
                            });
                        
                        page.Footer()
                            .AlignCenter()
                            .Text(x =>
                            {
                                x.Span("صفحه ").FontSize(9);
                                x.CurrentPageNumber().FontSize(9);
                                x.Span(" از ").FontSize(9);
                                x.TotalPages().FontSize(9);
                            });
                    });
                })
                .GeneratePdf();
                
                return File(pdfBytes, "application/pdf", 
                    $"MedicalRecord_{viewModel.PatientId}_{DateTime.Now:yyyyMMdd}.pdf");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در Export PDF");
                NotificationHelper.SetError(TempData, "خطا در Export PDF");
                return RedirectToAction("Index");
            }
        }
        
        /// <summary>
        /// Export Medical Record to Excel
        /// GET: /Patient/MedicalRecord/ExportExcel
        /// ✅ Enterprise-Grade: استفاده از ClosedXML
        /// </summary>
        [HttpGet]
        public async Task<ActionResult> ExportExcel()
        {
            try
            {
                var patientId = await GetCurrentPatientIdAsync();
                if (patientId == null)
                {
                    NotificationHelper.SetError(TempData, "اطلاعات بیمار یافت نشد");
                    return RedirectToAction("Index");
                }
                
                var result = await _medicalRecordService.GetMedicalRecordAsync(patientId.Value);
                if (!result.Success)
                {
                    NotificationHelper.SetError(TempData, result.Message);
                    return RedirectToAction("Index");
                }
                
                var viewModel = result.Data;
                
                // ✅ Generate Excel using ClosedXML
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("پرونده الکترونیک");
                    
                    // Header
                    worksheet.Cell(1, 1).Value = "پرونده الکترونیک سلامت";
                    worksheet.Cell(1, 1).Style.Font.Bold = true;
                    worksheet.Cell(1, 1).Style.Font.FontSize = 14;
                    worksheet.Range(1, 1, 1, 6).Merge();
                    
                    worksheet.Cell(2, 1).Value = $"بیمار: {viewModel.PatientFullName}";
                    worksheet.Cell(2, 1).Style.Font.Bold = true;
                    worksheet.Range(2, 1, 2, 6).Merge();
                    
                    // Medical Histories Table
                    if (viewModel.MedicalHistories != null && viewModel.MedicalHistories.Any())
                    {
                        int row = 4;
                        
                        // Table Header
                        worksheet.Cell(row, 1).Value = "نوع";
                        worksheet.Cell(row, 2).Value = "عنوان";
                        worksheet.Cell(row, 3).Value = "تاریخ شروع";
                        worksheet.Cell(row, 4).Value = "تاریخ پایان";
                        worksheet.Cell(row, 5).Value = "پزشک معالج";
                        worksheet.Cell(row, 6).Value = "مرکز درمانی";
                        
                        var headerRange = worksheet.Range(row, 1, row, 6);
                        headerRange.Style.Font.Bold = true;
                        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
                        headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        
                        row++;
                        
                        // Table Data
                        foreach (var history in viewModel.MedicalHistories)
                        {
                            worksheet.Cell(row, 1).Value = history.TypeText;
                            worksheet.Cell(row, 2).Value = history.Title;
                            worksheet.Cell(row, 3).Value = history.StartDateShamsi;
                            worksheet.Cell(row, 4).Value = history.EndDateShamsi;
                            worksheet.Cell(row, 5).Value = history.DoctorName;
                            worksheet.Cell(row, 6).Value = history.MedicalCenter;
                            
                            var dataRange = worksheet.Range(row, 1, row, 6);
                            dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                            
                            row++;
                        }
                        
                        // Auto-fit columns
                        worksheet.Columns().AdjustToContents();
                    }
                    
                    // ✅ Return Excel file
                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        var content = stream.ToArray();
                        return File(content, 
                            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                            $"MedicalRecord_{viewModel.PatientId}_{DateTime.Now:yyyyMMdd}.xlsx");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در Export Excel");
                NotificationHelper.SetError(TempData, "خطا در Export Excel");
                return RedirectToAction("Index");
            }
        }
    }
}
