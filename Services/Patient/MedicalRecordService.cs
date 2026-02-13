using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Core;
using ClinicApp.Factories.Patient;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.Appointment;
using ClinicApp.Interfaces.Repositories;
using ClinicApp.Models.Entities.Patient;
using ClinicApp.Models.Entities.Triage;
using ClinicApp.Services.Triage;
using ClinicApp.ViewModels.Patient.MedicalRecord;
using Serilog;

namespace ClinicApp.Services.Patient
{
    /// <summary>
    /// Service برای مدیریت پرونده الکترونیک بیمار
    /// Single Responsibility: فقط Business Logic
    /// ✅ Enterprise-Grade: ServiceResult Enhanced, Factory Method, Authorization
    /// </summary>
    public class MedicalRecordService : IPatientMedicalRecordService
    {
        private readonly IMedicalRecordRepository _repository;
        private readonly IPatientService _patientService;
        private readonly IAppointmentBookingService _appointmentService;
        private readonly ITriageService _triageService;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger _logger;
        
        public MedicalRecordService(
            IMedicalRecordRepository repository,
            IPatientService patientService,
            IAppointmentBookingService appointmentService,
            ITriageService triageService,
            ICurrentUserService currentUserService,
            ILogger logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _patientService = patientService ?? throw new ArgumentNullException(nameof(patientService));
            _appointmentService = appointmentService ?? throw new ArgumentNullException(nameof(appointmentService));
            _triageService = triageService ?? throw new ArgumentNullException(nameof(triageService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _logger = logger?.ForContext<MedicalRecordService>();
        }
        
        /// <summary>
        /// دریافت پرونده الکترونیک بیمار
        /// ✅ ServiceResult Enhanced
        /// ✅ Factory Method برای تبدیل Entity → ViewModel
        /// ✅ Authorization
        /// </summary>
        public async Task<ServiceResult<MedicalRecordIndexViewModel>> GetMedicalRecordAsync(int patientId)
        {
            try
            {
                // ✅ Authorization
                if (!await ValidatePatientAccessAsync(patientId))
                {
                    return ServiceResult<MedicalRecordIndexViewModel>.Failed(
                        "دسترسی غیرمجاز",
                        "UNAUTHORIZED_ACCESS",
                        ErrorCategory.Security,
                        SecurityLevel.High);
                }
                
                _logger.Information("دریافت پرونده الکترونیک - PatientId: {PatientId}", patientId);
                
                // ✅ دریافت اطلاعات بیمار
                var patientResult = await _patientService.GetPatientDetailsAsync(patientId);
                if (!patientResult.Success)
                {
                    return ServiceResult<MedicalRecordIndexViewModel>.Failed(
                        patientResult.Message,
                        patientResult.Code);
                }
                
                var patient = patientResult.Data;
                
                // ✅ دریافت تاریخچه پزشکی
                var medicalHistories = await _repository.GetMedicalHistoriesByPatientIdAsync(patientId);
                
                // ✅ Factory Method برای تبدیل Entity → ViewModel
                var medicalHistoryViewModels = MedicalRecordFactory.ToViewModelList(medicalHistories);
                
                var viewModel = new MedicalRecordIndexViewModel
                {
                    PatientId = patientId,
                    PatientFullName = patient.FullName,
                    MedicalHistories = medicalHistoryViewModels
                };
                
                // ✅ ServiceResult Enhanced
                return ServiceResult<MedicalRecordIndexViewModel>.Successful(
                    viewModel,
                    "پرونده الکترونیک با موفقیت دریافت شد.",
                    operationName: "GetMedicalRecord",
                    userId: _currentUserService.UserId,
                    userFullName: _currentUserService.UserName);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت پرونده الکترونیک - PatientId: {PatientId}", patientId);
                return ServiceResult<MedicalRecordIndexViewModel>.Failed(
                    "خطا در دریافت پرونده الکترونیک",
                    "GET_MEDICAL_RECORD_ERROR",
                    ErrorCategory.General,
                    SecurityLevel.Medium);
            }
        }
        
        /// <summary>
        /// دریافت تاریخچه پزشکی بیمار
        /// ✅ ServiceResult Enhanced
        /// ✅ Factory Method
        /// ✅ Authorization
        /// </summary>
        public async Task<ServiceResult<List<MedicalHistoryViewModel>>> GetMedicalHistoriesAsync(int patientId)
        {
            try
            {
                // ✅ Authorization
                if (!await ValidatePatientAccessAsync(patientId))
                {
                    return ServiceResult<List<MedicalHistoryViewModel>>.Failed(
                        "دسترسی غیرمجاز",
                        "UNAUTHORIZED_ACCESS",
                        ErrorCategory.Security,
                        SecurityLevel.High);
                }
                
                _logger.Information("دریافت تاریخچه پزشکی - PatientId: {PatientId}", patientId);
                
                var medicalHistories = await _repository.GetMedicalHistoriesByPatientIdAsync(patientId);
                
                // ✅ Factory Method
                var viewModels = MedicalRecordFactory.ToViewModelList(medicalHistories);
                
                return ServiceResult<List<MedicalHistoryViewModel>>.Successful(
                    viewModels,
                    "تاریخچه پزشکی با موفقیت دریافت شد.",
                    operationName: "GetMedicalHistories",
                    userId: _currentUserService.UserId,
                    userFullName: _currentUserService.UserName);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تاریخچه پزشکی - PatientId: {PatientId}", patientId);
                return ServiceResult<List<MedicalHistoryViewModel>>.Failed(
                    "خطا در دریافت تاریخچه پزشکی",
                    "GET_MEDICAL_HISTORIES_ERROR",
                    ErrorCategory.General,
                    SecurityLevel.Medium);
            }
        }

        /// <summary>
        /// دریافت تاریخچه پزشکی با صفحه‌بندی و فیلتر — برای پرونده غنی (۵+ سال).
        /// </summary>
        public async Task<ServiceResult<PagedResult<MedicalHistoryViewModel>>> GetMedicalHistoriesPagedAsync(
            int patientId,
            int pageNumber = 1,
            int pageSize = 20,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            string searchText = null)
        {
            try
            {
                if (!await ValidatePatientAccessAsync(patientId))
                {
                    return ServiceResult<PagedResult<MedicalHistoryViewModel>>.Failed(
                        "دسترسی غیرمجاز",
                        "UNAUTHORIZED_ACCESS",
                        ErrorCategory.Security,
                        SecurityLevel.High);
                }

                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1) pageSize = 20;
                if (pageSize > 50) pageSize = 50;

                _logger.Information("دریافت تاریخچه پزشکی صفحه‌بندی - PatientId: {PatientId}, Page: {Page}, PageSize: {PageSize}",
                    patientId, pageNumber, pageSize);

                var (items, totalCount) = await _repository.GetMedicalHistoriesPagedAsync(
                    patientId, pageNumber, pageSize, fromDate, toDate, searchText);

                var viewModels = MedicalRecordFactory.ToViewModelList(items);
                var paged = new PagedResult<MedicalHistoryViewModel>(viewModels, totalCount, pageNumber, pageSize);

                return ServiceResult<PagedResult<MedicalHistoryViewModel>>.Successful(
                    paged,
                    "تاریخچه پزشکی با موفقیت دریافت شد.",
                    operationName: "GetMedicalHistoriesPaged",
                    userId: _currentUserService.UserId,
                    userFullName: _currentUserService.UserName);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تاریخچه پزشکی صفحه‌بندی - PatientId: {PatientId}", patientId);
                return ServiceResult<PagedResult<MedicalHistoryViewModel>>.Failed(
                    "خطا در دریافت تاریخچه پزشکی",
                    "GET_MEDICAL_HISTORIES_PAGED_ERROR",
                    ErrorCategory.General,
                    SecurityLevel.Medium);
            }
        }
        
        /// <summary>
        /// دریافت تاریخچه پزشکی با شناسه
        /// ✅ ServiceResult Enhanced
        /// ✅ Factory Method
        /// ✅ Authorization
        /// </summary>
        public async Task<ServiceResult<MedicalHistoryViewModel>> GetMedicalHistoryByIdAsync(
            int medicalHistoryId, int patientId)
        {
            try
            {
                // ✅ Authorization
                if (!await ValidatePatientAccessAsync(patientId))
                {
                    return ServiceResult<MedicalHistoryViewModel>.Failed(
                        "دسترسی غیرمجاز",
                        "UNAUTHORIZED_ACCESS",
                        ErrorCategory.Security,
                        SecurityLevel.High);
                }
                
                _logger.Information("دریافت تاریخچه پزشکی - MedicalHistoryId: {MedicalHistoryId}, PatientId: {PatientId}", 
                    medicalHistoryId, patientId);
                
                var entity = await _repository.GetMedicalHistoryByIdAsync(medicalHistoryId);
                if (entity == null)
                {
                    return ServiceResult<MedicalHistoryViewModel>.Failed(
                        "تاریخچه پزشکی یافت نشد",
                        "MEDICAL_HISTORY_NOT_FOUND",
                        ErrorCategory.NotFound);
                }
                
                // ✅ Authorization: بررسی تعلق به بیمار
                if (entity.PatientId != patientId)
                {
                    return ServiceResult<MedicalHistoryViewModel>.Failed(
                        "دسترسی غیرمجاز",
                        "UNAUTHORIZED_ACCESS",
                        ErrorCategory.Security,
                        SecurityLevel.High);
                }
                
                // ✅ Factory Method
                var viewModel = MedicalRecordFactory.ToViewModel(entity);
                
                return ServiceResult<MedicalHistoryViewModel>.Successful(
                    viewModel,
                    "تاریخچه پزشکی با موفقیت دریافت شد.",
                    operationName: "GetMedicalHistoryById",
                    userId: _currentUserService.UserId,
                    userFullName: _currentUserService.UserName);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تاریخچه پزشکی - MedicalHistoryId: {MedicalHistoryId}", medicalHistoryId);
                return ServiceResult<MedicalHistoryViewModel>.Failed(
                    "خطا در دریافت تاریخچه پزشکی",
                    "GET_MEDICAL_HISTORY_ERROR",
                    ErrorCategory.General,
                    SecurityLevel.Medium);
            }
        }
        
        /// <summary>
        /// ایجاد تاریخچه پزشکی جدید
        /// ✅ ServiceResult Enhanced
        /// ✅ Factory Method
        /// ✅ Authorization
        /// </summary>
        public async Task<ServiceResult> CreateMedicalHistoryAsync(
            MedicalHistoryCreateEditViewModel model, int patientId)
        {
            try
            {
                // ✅ Authorization
                if (!await ValidatePatientAccessAsync(patientId))
                {
                    return ServiceResult.Failed(
                        "دسترسی غیرمجاز",
                        "UNAUTHORIZED_ACCESS",
                        ErrorCategory.Security,
                        SecurityLevel.High);
                }
                
                _logger.Information("ایجاد تاریخچه پزشکی - PatientId: {PatientId}, Title: {Title}", 
                    patientId, model.Title);
                
                // ✅ Validation
                if (model.EndDate.HasValue && model.StartDate.HasValue && 
                    model.EndDate.Value < model.StartDate.Value)
                {
                    return ServiceResult.Failed(
                        "تاریخ پایان نمی‌تواند قبل از تاریخ شروع باشد",
                        "INVALID_DATE_RANGE",
                        ErrorCategory.Validation);
                }
                
                if (model.Type == Models.Enums.MedicalHistoryType.Medication && string.IsNullOrWhiteSpace(model.Title) && string.IsNullOrWhiteSpace(model.DrugName))
                {
                    return ServiceResult.Failed(
                        "برای نوع دارو، عنوان یا نام دارو الزامی است.",
                        "MEDICATION_NAME_REQUIRED",
                        ErrorCategory.Validation);
                }
                
                // ✅ Factory Method: تبدیل ViewModel → Entity
                var entity = MedicalRecordFactory.ToEntity(model, patientId, _currentUserService.UserId);
                
                // FIXME(Phase 2): Handle file uploads if needed
                // For now, Attachments field is stored as string (comma-separated file paths)
                // In future, implement proper file upload service
                
                // ✅ Save
                await _repository.CreateMedicalHistoryAsync(entity);
                
                _logger.Information("تاریخچه پزشکی با موفقیت ایجاد شد - MedicalHistoryId: {MedicalHistoryId}", 
                    entity.MedicalHistoryId);
                
                return ServiceResult.Successful(
                    "تاریخچه پزشکی با موفقیت ایجاد شد.",
                    "MEDICAL_HISTORY_CREATED",
                    operationName: "CreateMedicalHistory",
                    userId: _currentUserService.UserId,
                    userFullName: _currentUserService.UserName);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد تاریخچه پزشکی - PatientId: {PatientId}", patientId);
                return ServiceResult.Failed(
                    "خطا در ایجاد تاریخچه پزشکی",
                    "CREATE_MEDICAL_HISTORY_ERROR",
                    ErrorCategory.General,
                    SecurityLevel.Medium);
            }
        }
        
        /// <summary>
        /// به‌روزرسانی تاریخچه پزشکی
        /// ✅ ServiceResult Enhanced
        /// ✅ Factory Method
        /// ✅ Authorization
        /// </summary>
        public async Task<ServiceResult> UpdateMedicalHistoryAsync(
            MedicalHistoryCreateEditViewModel model, int patientId)
        {
            try
            {
                // ✅ Authorization
                if (!await ValidatePatientAccessAsync(patientId))
                {
                    return ServiceResult.Failed(
                        "دسترسی غیرمجاز",
                        "UNAUTHORIZED_ACCESS",
                        ErrorCategory.Security,
                        SecurityLevel.High);
                }
                
                if (!model.MedicalHistoryId.HasValue)
                {
                    return ServiceResult.Failed(
                        "شناسه تاریخچه پزشکی الزامی است",
                        "MEDICAL_HISTORY_ID_REQUIRED",
                        ErrorCategory.Validation);
                }
                
                _logger.Information("به‌روزرسانی تاریخچه پزشکی - MedicalHistoryId: {MedicalHistoryId}, PatientId: {PatientId}", 
                    model.MedicalHistoryId.Value, patientId);
                
                // ✅ دریافت Entity
                var entity = await _repository.GetMedicalHistoryByIdAsync(model.MedicalHistoryId.Value);
                if (entity == null)
                {
                    return ServiceResult.Failed(
                        "تاریخچه پزشکی یافت نشد",
                        "MEDICAL_HISTORY_NOT_FOUND",
                        ErrorCategory.NotFound);
                }
                
                // ✅ Authorization: بررسی تعلق به بیمار
                if (entity.PatientId != patientId)
                {
                    return ServiceResult.Failed(
                        "دسترسی غیرمجاز",
                        "UNAUTHORIZED_ACCESS",
                        ErrorCategory.Security,
                        SecurityLevel.High);
                }
                
                // ✅ Validation
                if (model.EndDate.HasValue && model.StartDate.HasValue && 
                    model.EndDate.Value < model.StartDate.Value)
                {
                    return ServiceResult.Failed(
                        "تاریخ پایان نمی‌تواند قبل از تاریخ شروع باشد",
                        "INVALID_DATE_RANGE",
                        ErrorCategory.Validation);
                }
                if (model.Type == Models.Enums.MedicalHistoryType.Medication && string.IsNullOrWhiteSpace(model.Title) && string.IsNullOrWhiteSpace(model.DrugName))
                {
                    return ServiceResult.Failed(
                        "برای نوع دارو، عنوان یا نام دارو الزامی است.",
                        "MEDICATION_NAME_REQUIRED",
                        ErrorCategory.Validation);
                }
                
                // ✅ Factory Method: به‌روزرسانی Entity از ViewModel
                MedicalRecordFactory.UpdateEntity(entity, model, _currentUserService.UserId);
                MedicalRecordFactory.SyncMedicationFromViewModel(entity, model, _currentUserService.UserId);
                MedicalRecordFactory.SyncLabFromViewModel(entity, model, _currentUserService.UserId);
                
                // ✅ Save
                await _repository.UpdateMedicalHistoryAsync(entity);
                
                _logger.Information("تاریخچه پزشکی با موفقیت به‌روزرسانی شد - MedicalHistoryId: {MedicalHistoryId}", 
                    entity.MedicalHistoryId);
                
                return ServiceResult.Successful(
                    "تاریخچه پزشکی با موفقیت به‌روزرسانی شد.",
                    "MEDICAL_HISTORY_UPDATED",
                    operationName: "UpdateMedicalHistory",
                    userId: _currentUserService.UserId,
                    userFullName: _currentUserService.UserName);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی تاریخچه پزشکی - MedicalHistoryId: {MedicalHistoryId}", 
                    model.MedicalHistoryId);
                return ServiceResult.Failed(
                    "خطا در به‌روزرسانی تاریخچه پزشکی",
                    "UPDATE_MEDICAL_HISTORY_ERROR",
                    ErrorCategory.General,
                    SecurityLevel.Medium);
            }
        }
        
        /// <summary>
        /// حذف تاریخچه پزشکی
        /// ✅ ServiceResult Enhanced
        /// ✅ Authorization
        /// </summary>
        public async Task<ServiceResult> DeleteMedicalHistoryAsync(int medicalHistoryId, int patientId)
        {
            try
            {
                // ✅ Authorization
                if (!await ValidatePatientAccessAsync(patientId))
                {
                    return ServiceResult.Failed(
                        "دسترسی غیرمجاز",
                        "UNAUTHORIZED_ACCESS",
                        ErrorCategory.Security,
                        SecurityLevel.High);
                }
                
                _logger.Information("حذف تاریخچه پزشکی - MedicalHistoryId: {MedicalHistoryId}, PatientId: {PatientId}", 
                    medicalHistoryId, patientId);
                
                // ✅ دریافت Entity
                var entity = await _repository.GetMedicalHistoryByIdAsync(medicalHistoryId);
                if (entity == null)
                {
                    return ServiceResult.Failed(
                        "تاریخچه پزشکی یافت نشد",
                        "MEDICAL_HISTORY_NOT_FOUND",
                        ErrorCategory.NotFound);
                }
                
                // ✅ Authorization: بررسی تعلق به بیمار
                if (entity.PatientId != patientId)
                {
                    return ServiceResult.Failed(
                        "دسترسی غیرمجاز",
                        "UNAUTHORIZED_ACCESS",
                        ErrorCategory.Security,
                        SecurityLevel.High);
                }
                
                // ✅ Soft Delete
                var deleted = await _repository.DeleteMedicalHistoryAsync(medicalHistoryId, _currentUserService.UserId);
                if (!deleted)
                {
                    return ServiceResult.Failed(
                        "خطا در حذف تاریخچه پزشکی",
                        "DELETE_MEDICAL_HISTORY_ERROR",
                        ErrorCategory.General);
                }
                
                _logger.Information("تاریخچه پزشکی با موفقیت حذف شد - MedicalHistoryId: {MedicalHistoryId}", 
                    medicalHistoryId);
                
                return ServiceResult.Successful(
                    "تاریخچه پزشکی با موفقیت حذف شد.",
                    "MEDICAL_HISTORY_DELETED",
                    operationName: "DeleteMedicalHistory",
                    userId: _currentUserService.UserId,
                    userFullName: _currentUserService.UserName);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف تاریخچه پزشکی - MedicalHistoryId: {MedicalHistoryId}", medicalHistoryId);
                return ServiceResult.Failed(
                    "خطا در حذف تاریخچه پزشکی",
                    "DELETE_MEDICAL_HISTORY_ERROR",
                    ErrorCategory.General,
                    SecurityLevel.Medium);
            }
        }
        
        #region Private Helpers
        
        /// <summary>
        /// ✅ Authorization Helper
        /// </summary>
        private async Task<bool> ValidatePatientAccessAsync(int patientId)
        {
            try
            {
                // ✅ SIMPLIFIED: Controller already validated user via GetCurrentPatientIdAsync()
                // We just need to verify the patientId is valid (not 0 or negative)
                if (patientId <= 0)
                {
                    _logger.Warning("❌ ValidatePatientAccess: Invalid PatientId: {PatientId}", patientId);
                    return false;
                }

                _logger.Debug("✅ ValidatePatientAccess: Access validated - PatientId: {PatientId}", patientId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ ValidatePatientAccess: Exception - PatientId: {PatientId}", patientId);
                return false;
            }
        }
        
        /// <summary>
        /// دریافت نوبت‌های پزشکی بیمار
        /// ✅ ServiceResult Enhanced
        /// ✅ Factory Method
        /// ✅ Authorization
        /// </summary>
        public async Task<ServiceResult<PagedResult<MedicalRecordAppointmentViewModel>>> GetAppointmentsAsync(
            int patientId, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                if (!await ValidatePatientAccessAsync(patientId))
                {
                    return ServiceResult<PagedResult<MedicalRecordAppointmentViewModel>>.Failed(
                        "دسترسی غیرمجاز",
                        "UNAUTHORIZED_ACCESS",
                        ErrorCategory.Security,
                        SecurityLevel.High);
                }

                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1) pageSize = 10;
                if (pageSize > 50) pageSize = 50;

                _logger.Information("دریافت نوبت‌های پزشکی - PatientId: {PatientId}, Page: {Page}, PageSize: {PageSize}",
                    patientId, pageNumber, pageSize);

                var result = await _patientService.GetPatientAppointmentsPagedAsync(patientId, pageNumber, pageSize);
                if (!result.Success)
                {
                    return ServiceResult<PagedResult<MedicalRecordAppointmentViewModel>>.Failed(
                        result.Message,
                        result.Code);
                }

                var paged = result.Data;
                var viewModels = (paged?.Items ?? new List<ViewModels.PatientAppointmentViewModel>())
                    .Select(a => new MedicalRecordAppointmentViewModel
                    {
                        AppointmentId = a.AppointmentId,
                        DoctorId = a.DoctorId,
                        DoctorName = a.DoctorName,
                        DoctorSpecialization = a.ServiceCategoryName,
                        AppointmentDate = a.AppointmentDate,
                        AppointmentDateShamsi = a.AppointmentDateShamsi,
                        AppointmentTime = a.AppointmentDate.ToString("HH:mm"),
                        Status = a.Status,
                        StatusText = a.StatusText,
                        Price = a.Price,
                        Description = a.Notes,
                        ServiceCategory = a.ServiceCategoryName,
                        IsNewPatient = false,
                        Duration = null
                    })
                    .ToList();

                var outPaged = new PagedResult<MedicalRecordAppointmentViewModel>(
                    viewModels,
                    paged?.TotalItems ?? 0,
                    pageNumber,
                    pageSize);

                return ServiceResult<PagedResult<MedicalRecordAppointmentViewModel>>.Successful(
                    outPaged,
                    "نوبت‌های پزشکی با موفقیت دریافت شد.",
                    operationName: "GetAppointments",
                    userId: _currentUserService.UserId,
                    userFullName: _currentUserService.UserName);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت نوبت‌های پزشکی - PatientId: {PatientId}", patientId);
                return ServiceResult<PagedResult<MedicalRecordAppointmentViewModel>>.Failed(
                    "خطا در دریافت نوبت‌های پزشکی",
                    "GET_APPOINTMENTS_ERROR",
                    ErrorCategory.General,
                    SecurityLevel.Medium);
            }
        }
        
        /// <summary>
        /// دریافت پذیرش‌های بیمار
        /// ✅ ServiceResult Enhanced
        /// ✅ Factory Method
        /// ✅ Authorization
        /// </summary>
        public async Task<ServiceResult<PagedResult<MedicalRecordReceptionViewModel>>> GetReceptionsAsync(
            int patientId, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                if (!await ValidatePatientAccessAsync(patientId))
                {
                    return ServiceResult<PagedResult<MedicalRecordReceptionViewModel>>.Failed(
                        "دسترسی غیرمجاز",
                        "UNAUTHORIZED_ACCESS",
                        ErrorCategory.Security,
                        SecurityLevel.High);
                }

                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1) pageSize = 10;
                if (pageSize > 50) pageSize = 50;

                _logger.Information("دریافت پذیرش‌ها - PatientId: {PatientId}, Page: {Page}, PageSize: {PageSize}",
                    patientId, pageNumber, pageSize);

                var result = await _patientService.GetPatientReceptionsPagedAsync(patientId, pageNumber, pageSize);
                if (!result.Success)
                {
                    return ServiceResult<PagedResult<MedicalRecordReceptionViewModel>>.Failed(
                        result.Message,
                        result.Code);
                }

                var paged = result.Data;
                var receptions = paged?.Items ?? new List<ViewModels.PatientReceptionViewModel>();

                var viewModels = receptions.Select(r => new MedicalRecordReceptionViewModel
                {
                    ReceptionId = r.ReceptionId,
                    ReceptionNumber = $"R{r.ReceptionId:D6}",
                    DoctorId = r.DoctorId,
                    DoctorName = r.DoctorName,
                    DepartmentName = null, // FIXME(Phase 2): از service دریافت شود
                    ClinicName = null, // FIXME(Phase 2): از service دریافت شود
                    ReceptionDate = r.ReceptionDate,
                    ReceptionDateShamsi = r.ReceptionDateShamsi,
                    ReceptionTime = r.ReceptionDate.ToString("HH:mm"),
                    Status = r.Status,
                    StatusText = r.StatusText,
                    TotalAmount = r.TotalAmount,
                    PatientShare = 0, // FIXME(Phase 2): از service دریافت شود
                    InsurerShare = 0, // FIXME(Phase 2): از service دریافت شود
                    Notes = null,
                    IsEmergency = false // FIXME(Phase 2): از service دریافت شود
                }).ToList();

                var outPaged = new PagedResult<MedicalRecordReceptionViewModel>(
                    viewModels,
                    paged?.TotalItems ?? 0,
                    pageNumber,
                    pageSize);
                return ServiceResult<PagedResult<MedicalRecordReceptionViewModel>>.Successful(
                    outPaged,
                    "پذیرش‌ها با موفقیت دریافت شد.",
                    operationName: "GetReceptions",
                    userId: _currentUserService.UserId,
                    userFullName: _currentUserService.UserName);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت پذیرش‌ها - PatientId: {PatientId}", patientId);
                return ServiceResult<PagedResult<MedicalRecordReceptionViewModel>>.Failed(
                    "خطا در دریافت پذیرش‌ها",
                    "GET_RECEPTIONS_ERROR",
                    ErrorCategory.General,
                    SecurityLevel.Medium);
            }
        }
        
        /// <summary>
        /// دریافت ارزیابی‌های تریاژ بیمار
        /// ✅ ServiceResult Enhanced
        /// ✅ Factory Method
        /// ✅ Authorization
        /// </summary>
        public async Task<ServiceResult<PagedResult<MedicalRecordTriageViewModel>>> GetTriageAssessmentsAsync(
            int patientId, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                if (!await ValidatePatientAccessAsync(patientId))
                {
                    return ServiceResult<PagedResult<MedicalRecordTriageViewModel>>.Failed(
                        "دسترسی غیرمجاز",
                        "UNAUTHORIZED_ACCESS",
                        ErrorCategory.Security,
                        SecurityLevel.High);
                }

                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1) pageSize = 10;
                if (pageSize > 50) pageSize = 50;

                _logger.Information("دریافت ارزیابی‌های تریاژ - PatientId: {PatientId}, Page: {Page}, PageSize: {PageSize}",
                    patientId, pageNumber, pageSize);

                var triageResult = await _triageService.GetPatientTriageAssessmentsAsync(patientId, includeCompleted: true);
                if (!triageResult.Success)
                {
                    return ServiceResult<PagedResult<MedicalRecordTriageViewModel>>.Failed(
                        triageResult.Message,
                        triageResult.Code ?? "TRIAGE_LOAD_FAILED");
                }

                var allAssessments = triageResult.Data ?? new List<TriageAssessment>();
                int totalCount = allAssessments.Count;
                var paged = allAssessments
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var viewModels = paged.Select(ta => MedicalRecordFactory.ToViewModel(
                    ta,
                    ta.VitalSigns?.OrderByDescending(v => v.MeasurementTime).FirstOrDefault(),
                    ta.Assessor?.FullName ?? ta.Assessor?.UserName)).ToList();

                var outPaged = new PagedResult<MedicalRecordTriageViewModel>(
                    viewModels,
                    totalCount,
                    pageNumber,
                    pageSize);
                return ServiceResult<PagedResult<MedicalRecordTriageViewModel>>.Successful(
                    outPaged,
                    "ارزیابی‌های تریاژ با موفقیت دریافت شد.",
                    operationName: "GetTriageAssessments",
                    userId: _currentUserService.UserId,
                    userFullName: _currentUserService.UserName);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت ارزیابی‌های تریاژ - PatientId: {PatientId}", patientId);
                return ServiceResult<PagedResult<MedicalRecordTriageViewModel>>.Failed(
                    "خطا در دریافت ارزیابی‌های تریاژ",
                    "GET_TRIAGE_ASSESSMENTS_ERROR",
                    ErrorCategory.General,
                    SecurityLevel.Medium);
            }
        }
        
        #endregion
    }
}

