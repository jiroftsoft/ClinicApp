using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using ClinicApp.Helpers;
using ClinicApp.Interfaces.ClinicAdmin;
using ClinicApp.Models.Entities;
using ClinicApp.ViewModels.DoctorManagementVM;
using FluentValidation;
using Serilog;
using SelectListItem = ClinicApp.ViewModels.DoctorManagementVM.SelectListItem;

namespace ClinicApp.Areas.Admin.Controllers.Shared
{
    /// <summary>
    /// کنترلر پایه مشترک برای عملیات انتساب پزشکان
    /// شامل متدهای مشترک و helper methods
    /// </summary>
    public abstract class BaseAssignmentController : Controller
    {
        #region Protected Fields (فیلدهای محافظت شده)

        protected readonly IDoctorAssignmentService _doctorAssignmentService;
        protected readonly IDoctorCrudService _doctorService;
        protected readonly IDoctorDepartmentService _doctorDepartmentService;
        protected readonly IDoctorServiceCategoryService _doctorServiceCategoryService;
        protected readonly IDoctorAssignmentHistoryService _historyService;
        protected readonly ILogger _logger;

        #endregion

        #region Constructor (سازنده)

        protected BaseAssignmentController(
            IDoctorAssignmentService doctorAssignmentService,
            IDoctorCrudService doctorService,
            IDoctorDepartmentService doctorDepartmentService,
            IDoctorServiceCategoryService doctorServiceCategoryService,
            IDoctorAssignmentHistoryService historyService)
        {
            _doctorAssignmentService = doctorAssignmentService ?? throw new ArgumentNullException(nameof(doctorAssignmentService));
            _doctorService = doctorService ?? throw new ArgumentNullException(nameof(doctorService));
            _doctorDepartmentService = doctorDepartmentService ?? throw new ArgumentNullException(nameof(doctorDepartmentService));
            _doctorServiceCategoryService = doctorServiceCategoryService ?? throw new ArgumentNullException(nameof(doctorServiceCategoryService));
            _historyService = historyService ?? throw new ArgumentNullException(nameof(historyService));
            _logger = Log.ForContext(GetType());
        }

        #endregion

        #region Protected Helper Methods (متدهای کمکی محافظت شده)

        /// <summary>
        /// دریافت اطلاعات پزشک (بدون عوارض جانبی)
        /// </summary>
        protected async Task<ServiceResult<DoctorDetailsViewModel>> GetDoctorAsync(int doctorId)
        {
            try
            {
                var doctorResult = await _doctorService.GetDoctorDetailsAsync(doctorId);
                if (!doctorResult.Success)
                {
                    _logger.Warning("پزشک با شناسه {DoctorId} یافت نشد: {Message}", doctorId, doctorResult.Message);
                }
                return doctorResult;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت اطلاعات پزشک {DoctorId}", doctorId);
                return ServiceResult<DoctorDetailsViewModel>.Failed("خطا در دریافت اطلاعات پزشک");
            }
        }

        /// <summary>
        /// دریافت انتسابات پزشک (بدون عوارض جانبی)
        /// </summary>
        protected async Task<ServiceResult<DoctorAssignmentsViewModel>> GetDoctorAssignmentsAsync(int doctorId)
        {
            try
            {
                var assignmentsResult = await _doctorAssignmentService.GetDoctorAssignmentsAsync(doctorId);
                if (!assignmentsResult.Success)
                {
                    _logger.Warning("انتسابات پزشک {DoctorId} یافت نشد: {Message}", doctorId, assignmentsResult.Message);
                }
                return assignmentsResult;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت انتسابات پزشک {DoctorId}", doctorId);
                return ServiceResult<DoctorAssignmentsViewModel>.Failed("خطا در دریافت انتسابات پزشک");
            }
        }

        /// <summary>
        /// دریافت لیست دپارتمان‌ها به صورت SelectList (انتقال منطق به سرویس)
        /// </summary>
        protected async Task<List<SelectListItem>> GetDepartmentsSelectListAsync(bool addAllOption = true)
        {
            var result = await _doctorDepartmentService.GetDepartmentsAsSelectListAsync(addAllOption);
            return result.Success ? result.Data : new List<SelectListItem>();
        }

        /// <summary>
        /// دریافت لیست سرفصل‌های خدماتی به صورت SelectList (انتقال منطق به سرویس)
        /// </summary>
        protected async Task<List<SelectListItem>> GetServiceCategoriesSelectListAsync(bool addAllOption = true)
        {
            var result = await _doctorServiceCategoryService.GetServiceCategoriesAsSelectListAsync(addAllOption);
            return result.Success ? result.Data : new List<SelectListItem>();
        }

        /// <summary>
        /// اعتبارسنجی مدل با FluentValidation
        /// </summary>
        protected async Task<bool> ValidateModelAsync<T>(T model, IValidator<T> validator)
        {
            if (model == null)
            {
                ModelState.AddModelError("", "مدل ارسالی نامعتبر است");
                return false;
            }

            var validationResult = await validator.ValidateAsync(model);
            if (!validationResult.IsValid)
            {
                foreach (var error in validationResult.Errors)
                {
                    ModelState.AddModelError("", error.ErrorMessage);
                }
                return false;
            }

            return true;
        }

        /// <summary>
        /// ثبت عملیات در تاریخچه
        /// </summary>
        protected async Task LogAssignmentOperationAsync(int doctorId, string operation, string title, string description, int? departmentId = null, AssignmentHistoryImportance importance = AssignmentHistoryImportance.Normal)
        {
            try
            {
                await _historyService.LogAssignmentOperationAsync(
                    doctorId,
                    operation,
                    title,
                    description,
                    departmentId,
                    importance: importance);
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "خطا در ثبت تاریخچه عملیات {Operation} برای پزشک {DoctorId}", operation, doctorId);
            }
        }

        /// <summary>
        /// مدیریت خطاهای عمومی
        /// </summary>
        protected void HandleGeneralError(Exception ex, string operation, int? doctorId = null)
        {
            _logger.Error(ex, "خطا در {Operation} برای پزشک {DoctorId}", operation, doctorId?.ToString() ?? "نامشخص");
            TempData["ErrorMessage"] = $"خطا در {operation}";
        }

        /// <summary>
        /// بررسی وجود پزشک (استفاده از GetDoctorAsync برای جلوگیری از تکرار کد)
        /// </summary>
        protected async Task<bool> ValidateDoctorExistsAsync(int doctorId)
        {
            if (doctorId <= 0)
            {
                return false;
            }

            var doctorResult = await GetDoctorAsync(doctorId);
            return doctorResult.Success;
        }

        #endregion
    }
}
