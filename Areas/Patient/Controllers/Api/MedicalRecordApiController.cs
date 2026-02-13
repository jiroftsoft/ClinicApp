using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using ClinicApp.Areas.Patient.Controllers.Base;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Models;
using ClinicApp.ViewModels.Patient.MedicalRecord;
using Serilog;

namespace ClinicApp.Areas.Patient.Controllers.Api
{
    /// <summary>
    /// API Controller برای Component-Based AJAX Loading
    /// Single Responsibility: فقط API Endpoints
    /// ✅ Enterprise-Grade: ServiceResult Enhanced, Authorization, AJAX-First
    /// </summary>
    [Authorize]
    public class MedicalRecordApiController : BasePatientController
    {
        private readonly IPatientMedicalRecordService _medicalRecordService;
        private readonly IDocumentUploadService _documentUploadService;
        
        public MedicalRecordApiController(
            IPatientMedicalRecordService medicalRecordService,
            IDocumentUploadService documentUploadService,
            ILogger logger,
            ICurrentUserService currentUserService,
            ApplicationDbContext context)
            : base(logger, currentUserService, context)
        {
            _medicalRecordService = medicalRecordService ?? 
                throw new ArgumentNullException(nameof(medicalRecordService));
            _documentUploadService = documentUploadService ?? 
                throw new ArgumentNullException(nameof(documentUploadService));
        }
        
        /// <summary>
        /// دریافت بخش تاریخچه پزشکی با صفحه‌بندی و فیلتر (فاز ۱.۱ پرونده غنی).
        /// GET: /Patient/Api/MedicalRecord/GetMedicalHistories?pageNumber=1&pageSize=20&fromDate=&toDate=&search=
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetMedicalHistories(
            int pageNumber = 1,
            int pageSize = 20,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            string search = null)
        {
            try
            {
                var patientId = await GetCurrentPatientIdAsync();
                if (patientId == null)
                {
                    return ErrorJsonResult("اطلاعات بیمار یافت نشد");
                }

                if (pageSize > 50) pageSize = 50;

                var result = await _medicalRecordService.GetMedicalHistoriesPagedAsync(
                    patientId.Value, pageNumber, pageSize, fromDate, toDate, search?.Trim());

                if (!result.Success)
                {
                    return ErrorJsonResult(result.Message);
                }

                var paged = result.Data;
                _logger.Information("GetMedicalHistories - PatientId: {PatientId}, Page: {Page}, Total: {Total}",
                    patientId.Value, pageNumber, paged?.TotalItems ?? 0);

                return SuccessJsonResult(paged, result.Message);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تاریخچه پزشکی");
                return ErrorJsonResult("خطا در دریافت تاریخچه پزشکی");
            }
        }
        
        /// <summary>
        /// دریافت یک تاریخچه پزشکی با شناسه (Component)
        /// GET: /Patient/Api/MedicalRecord/GetMedicalHistory?id=123
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetMedicalHistory(int id)
        {
            try
            {
                var patientId = await GetCurrentPatientIdAsync();
                if (patientId == null)
                {
                    return ErrorJsonResult("اطلاعات بیمار یافت نشد");
                }
                
                var result = await _medicalRecordService.GetMedicalHistoryByIdAsync(id, patientId.Value);
                
                if (!result.Success)
                {
                    return ErrorJsonResult(result.Message);
                }
                
                return SuccessJsonResult(result.Data, result.Message);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تاریخچه پزشکی - MedicalHistoryId: {MedicalHistoryId}", id);
                return ErrorJsonResult("خطا در دریافت تاریخچه پزشکی");
            }
        }
        
        /// <summary>
        /// دریافت نوبت‌های پزشکی (Component) — فاز ۲.۲: fromDate/toDate اختیاری برای فیلتر یکپارچه.
        /// GET: /Patient/Api/MedicalRecord/GetAppointments?pageNumber=1&pageSize=10&fromDate=&toDate=
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetAppointments(int pageNumber = 1, int pageSize = 10, DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var patientId = await GetCurrentPatientIdAsync();
                if (patientId == null)
                {
                    return ErrorJsonResult("اطلاعات بیمار یافت نشد");
                }
                
                var result = await _medicalRecordService.GetAppointmentsAsync(patientId.Value, pageNumber, pageSize);
                
                if (!result.Success)
                {
                    return ErrorJsonResult(result.Message);
                }
                
                return SuccessJsonResult(result.Data, result.Message);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت نوبت‌های پزشکی");
                return ErrorJsonResult("خطا در دریافت نوبت‌های پزشکی");
            }
        }
        
        /// <summary>
        /// دریافت پذیرش‌ها (Component) — فاز ۲.۲: fromDate/toDate اختیاری برای فیلتر یکپارچه.
        /// GET: /Patient/Api/MedicalRecord/GetReceptions?pageNumber=1&pageSize=10&fromDate=&toDate=
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetReceptions(int pageNumber = 1, int pageSize = 10, DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var patientId = await GetCurrentPatientIdAsync();
                if (patientId == null)
                {
                    return ErrorJsonResult("اطلاعات بیمار یافت نشد");
                }
                
                var result = await _medicalRecordService.GetReceptionsAsync(patientId.Value, pageNumber, pageSize);
                
                if (!result.Success)
                {
                    return ErrorJsonResult(result.Message);
                }
                
                return SuccessJsonResult(result.Data, result.Message);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت پذیرش‌ها");
                return ErrorJsonResult("خطا در دریافت پذیرش‌ها");
            }
        }
        
        /// <summary>
        /// دریافت ارزیابی‌های تریاژ (Component) — فاز ۲.۲: fromDate/toDate اختیاری برای فیلتر یکپارچه.
        /// GET: /Patient/Api/MedicalRecord/GetTriageAssessments?pageNumber=1&pageSize=10&fromDate=&toDate=
        /// </summary>
        [HttpGet]
        public async Task<JsonResult> GetTriageAssessments(int pageNumber = 1, int pageSize = 10, DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var patientId = await GetCurrentPatientIdAsync();
                if (patientId == null)
                {
                    return ErrorJsonResult("اطلاعات بیمار یافت نشد");
                }
                
                var result = await _medicalRecordService.GetTriageAssessmentsAsync(patientId.Value, pageNumber, pageSize);
                
                if (!result.Success)
                {
                    return ErrorJsonResult(result.Message);
                }
                
                return SuccessJsonResult(result.Data, result.Message);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت ارزیابی‌های تریاژ");
                return ErrorJsonResult("خطا در دریافت ارزیابی‌های تریاژ");
            }
        }
        
        /// <summary>
        /// ایجاد تاریخچه پزشکی جدید
        /// POST: /Patient/Api/MedicalRecord/CreateMedicalHistory
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> CreateMedicalHistory()
        {
            try
            {
                var patientId = await GetCurrentPatientIdAsync();
                if (patientId == null)
                {
                    return ErrorJsonResult("اطلاعات بیمار یافت نشد");
                }
                
                // ✅ Parse form data
                var model = new MedicalHistoryCreateEditViewModel();
                TryUpdateModel(model);
                
                // ✅ طبق قرارداد 01-Helpers-DateTime: تبدیل تاریخ شمسی از فرم به DateTime
                var startStr = Request.Form["StartDate"];
                var endStr = Request.Form["EndDate"];
                if (!string.IsNullOrWhiteSpace(startStr))
                    model.StartDate = PersianDateHelper.ParsePersianDate(startStr.Trim());
                if (!string.IsNullOrWhiteSpace(endStr))
                    model.EndDate = PersianDateHelper.ParsePersianDate(endStr.Trim());
                
                // ✅ بایند صریح فیلدهای دارو و آلرژی (برای اطمینان از ارسال از مودال)
                model.DrugName = Request.Form["DrugName"];
                model.Dosage = Request.Form["Dosage"];
                model.DosageUnit = Request.Form["DosageUnit"];
                model.Frequency = Request.Form["Frequency"];
                model.Route = Request.Form["Route"];
                model.Indication = Request.Form["Indication"];
                model.PrescribingDoctor = Request.Form["PrescribingDoctor"];
                var isCriticalStr = Request.Form["IsCritical"];
                model.IsCritical = !string.IsNullOrEmpty(isCriticalStr) && "true".Equals(isCriticalStr.Trim(), StringComparison.OrdinalIgnoreCase);
                
                // ✅ بایند فیلدهای آزمایش
                model.LabName = Request.Form["LabName"];
                model.LabValue = Request.Form["LabValue"];
                model.LabUnit = Request.Form["LabUnit"];
                model.LabReferenceRange = Request.Form["LabReferenceRange"];
                var labDateStr = Request.Form["LabDate"];
                if (!string.IsNullOrWhiteSpace(labDateStr))
                    model.LabDate = PersianDateHelper.ParsePersianDate(labDateStr.Trim());
                
                // ✅ داروهای مرتبط با بیماری (لیست JSON)
                var medicationsListJson = Request.Form["MedicationsListJson"];
                if (!string.IsNullOrWhiteSpace(medicationsListJson))
                {
                    try
                    {
                        model.MedicationsList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<MedicalHistoryMedicationItemDto>>(medicationsListJson)
                            ?? new List<MedicalHistoryMedicationItemDto>();
                    }
                    catch { model.MedicationsList = new List<MedicalHistoryMedicationItemDto>(); }
                }
                
                // ✅ Handle file uploads
                var attachmentPaths = new List<string>();
                if (Request.Files != null && Request.Files.Count > 0)
                {
                    var uploadPath = "~/Content/Uploads/MedicalHistory";
                    for (int i = 0; i < Request.Files.Count; i++)
                    {
                        var file = Request.Files[i];
                        if (file != null && file.ContentLength > 0)
                        {
                            var uploadResult = _documentUploadService.UploadDocument(file, uploadPath);
                            if (uploadResult.Success && uploadResult.Data != null)
                            {
                                attachmentPaths.Add(uploadResult.Data.FileUrl);
                            }
                        }
                    }
                }
                
                // ✅ Set attachment paths (comma-separated)
                if (attachmentPaths.Any())
                {
                    model.Attachments = string.Join(",", attachmentPaths);
                }
                
                var result = await _medicalRecordService.CreateMedicalHistoryAsync(model, patientId.Value);
                
                if (!result.Success)
                {
                    return ErrorJsonResult(result.Message);
                }
                
                return SuccessJsonResult(null, result.Message);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد تاریخچه پزشکی");
                return ErrorJsonResult("خطا در ایجاد تاریخچه پزشکی");
            }
        }
        
        /// <summary>
        /// به‌روزرسانی تاریخچه پزشکی
        /// POST: /Patient/Api/MedicalRecord/UpdateMedicalHistory
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> UpdateMedicalHistory()
        {
            try
            {
                var patientId = await GetCurrentPatientIdAsync();
                if (patientId == null)
                {
                    return ErrorJsonResult("اطلاعات بیمار یافت نشد");
                }
                
                // ✅ Parse form data
                var model = new MedicalHistoryCreateEditViewModel();
                TryUpdateModel(model);
                
                // ✅ طبق قرارداد 01-Helpers-DateTime: تبدیل تاریخ شمسی از فرم به DateTime
                var startStr = Request.Form["StartDate"];
                var endStr = Request.Form["EndDate"];
                if (!string.IsNullOrWhiteSpace(startStr))
                    model.StartDate = PersianDateHelper.ParsePersianDate(startStr.Trim());
                if (!string.IsNullOrWhiteSpace(endStr))
                    model.EndDate = PersianDateHelper.ParsePersianDate(endStr.Trim());
                
                // ✅ بایند صریح فیلدهای دارو و آلرژی (برای ذخیره و ویرایش)
                model.DrugName = Request.Form["DrugName"];
                model.Dosage = Request.Form["Dosage"];
                model.DosageUnit = Request.Form["DosageUnit"];
                model.Frequency = Request.Form["Frequency"];
                model.Route = Request.Form["Route"];
                model.Indication = Request.Form["Indication"];
                model.PrescribingDoctor = Request.Form["PrescribingDoctor"];
                var isCriticalStrUpdate = Request.Form["IsCritical"];
                model.IsCritical = !string.IsNullOrEmpty(isCriticalStrUpdate) && "true".Equals(isCriticalStrUpdate.Trim(), StringComparison.OrdinalIgnoreCase);
                
                // ✅ بایند فیلدهای آزمایش
                model.LabName = Request.Form["LabName"];
                model.LabValue = Request.Form["LabValue"];
                model.LabUnit = Request.Form["LabUnit"];
                model.LabReferenceRange = Request.Form["LabReferenceRange"];
                var labDateStrUpdate = Request.Form["LabDate"];
                if (!string.IsNullOrWhiteSpace(labDateStrUpdate))
                    model.LabDate = PersianDateHelper.ParsePersianDate(labDateStrUpdate.Trim());
                
                var medicationsListJsonUpdate = Request.Form["MedicationsListJson"];
                if (!string.IsNullOrWhiteSpace(medicationsListJsonUpdate))
                {
                    try
                    {
                        model.MedicationsList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<MedicalHistoryMedicationItemDto>>(medicationsListJsonUpdate)
                            ?? new List<MedicalHistoryMedicationItemDto>();
                    }
                    catch { model.MedicationsList = new List<MedicalHistoryMedicationItemDto>(); }
                }
                
                // ✅ Handle file uploads (append to existing attachments)
                var attachmentPaths = new List<string>();
                if (!string.IsNullOrWhiteSpace(model.Attachments))
                {
                    attachmentPaths.AddRange(model.Attachments.Split(',').Where(p => !string.IsNullOrWhiteSpace(p)));
                }
                
                if (Request.Files != null && Request.Files.Count > 0)
                {
                    var uploadPath = "~/Content/Uploads/MedicalHistory";
                    for (int i = 0; i < Request.Files.Count; i++)
                    {
                        var file = Request.Files[i];
                        if (file != null && file.ContentLength > 0)
                        {
                            var uploadResult = _documentUploadService.UploadDocument(file, uploadPath);
                            if (uploadResult.Success && uploadResult.Data != null)
                            {
                                attachmentPaths.Add(uploadResult.Data.FileUrl);
                            }
                        }
                    }
                }
                
                // ✅ Set attachment paths (comma-separated)
                if (attachmentPaths.Any())
                {
                    model.Attachments = string.Join(",", attachmentPaths);
                }
                
                var result = await _medicalRecordService.UpdateMedicalHistoryAsync(model, patientId.Value);
                
                if (!result.Success)
                {
                    return ErrorJsonResult(result.Message);
                }
                
                return SuccessJsonResult(null, result.Message);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی تاریخچه پزشکی");
                return ErrorJsonResult("خطا در به‌روزرسانی تاریخچه پزشکی");
            }
        }
        
        /// <summary>
        /// حذف تاریخچه پزشکی
        /// POST: /Patient/Api/MedicalRecord/DeleteMedicalHistory
        /// </summary>
        [HttpPost]
        public async Task<JsonResult> DeleteMedicalHistory(int id)
        {
            try
            {
                var patientId = await GetCurrentPatientIdAsync();
                if (patientId == null)
                {
                    return ErrorJsonResult("اطلاعات بیمار یافت نشد");
                }
                
                var result = await _medicalRecordService.DeleteMedicalHistoryAsync(id, patientId.Value);
                
                if (!result.Success)
                {
                    return ErrorJsonResult(result.Message);
                }
                
                return SuccessJsonResult(null, result.Message);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف تاریخچه پزشکی - MedicalHistoryId: {MedicalHistoryId}", id);
                return ErrorJsonResult("خطا در حذف تاریخچه پزشکی");
            }
        }
    }
}

