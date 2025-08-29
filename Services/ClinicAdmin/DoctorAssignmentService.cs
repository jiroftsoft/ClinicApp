using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ClinicApp.Core;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.ClinicAdmin;
using ClinicApp.Models.Entities;
using ClinicApp.ViewModels.DoctorManagementVM;
using FluentValidation;
using Serilog;
using System.Collections.Generic; // Added missing import for List

namespace ClinicApp.Services.ClinicAdmin
{
    /// <summary>
    /// سرویس تخصصی برای مدیریت انتسابات پزشکان در سیستم کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. پیاده‌سازی کامل مدیریت انتسابات پزشکان به دپارتمان‌ها و سرفصل‌های خدماتی
    /// 2. رعایت استانداردهای پزشکی ایران در مدیریت انتسابات
    /// 3. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات پزشکی
    /// 4. مدیریت کامل ردیابی (Audit Trail) برای حسابرسی و امنیت سیستم
    /// 5. پشتیبانی از تقویم شمسی و اعداد فارسی در تمام فرآیندهای مدیریتی
    /// 6. پشتیبانی از محیط‌های Production و سیستم‌های Load Balanced
    /// 7. مدیریت حرفه‌ای خطاها و لاگ‌گیری برای سیستم‌های پزشکی
    /// 8. عملیات ترکیبی و انتقال پزشکان بین دپارتمان‌ها
    /// 
    /// نکته حیاتی: این کلاس بر اساس استانداردهای سیستم‌های پزشکی ایران پیاده‌سازی شده است
    /// </summary>
    public class DoctorAssignmentService : IDoctorAssignmentService
    {
        private readonly IDoctorAssignmentRepository _doctorAssignmentRepository;
        private readonly IDoctorCrudRepository _doctorRepository;
        private readonly IDoctorDepartmentService _doctorDepartmentService;
        private readonly IDoctorServiceCategoryService _doctorServiceCategoryService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IValidator<DoctorAssignmentsViewModel> _validator;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public DoctorAssignmentService(
            IDoctorAssignmentRepository doctorAssignmentRepository,
            IDoctorCrudRepository doctorRepository,
            IDoctorDepartmentService doctorDepartmentService,
            IDoctorServiceCategoryService doctorServiceCategoryService,
            ICurrentUserService currentUserService,
            IValidator<DoctorAssignmentsViewModel> validator,
            IMapper mapper)
        {
            _doctorAssignmentRepository = doctorAssignmentRepository ?? throw new ArgumentNullException(nameof(doctorAssignmentRepository));
            _doctorRepository = doctorRepository ?? throw new ArgumentNullException(nameof(doctorRepository));
            _doctorDepartmentService = doctorDepartmentService ?? throw new ArgumentNullException(nameof(doctorDepartmentService));
            _doctorServiceCategoryService = doctorServiceCategoryService ?? throw new ArgumentNullException(nameof(doctorServiceCategoryService));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = Log.ForContext<DoctorAssignmentService>();
        }

        #region Assignment Management (مدیریت انتسابات)

        /// <summary>
        /// به‌روزرسانی کامل انتسابات یک پزشک (دپارتمان‌ها و سرفصل‌های خدماتی)
        /// </summary>
        public async Task<ServiceResult> UpdateDoctorAssignmentsAsync(int doctorId, DoctorAssignmentsViewModel assignments)
        {
            try
            {
                _logger.Information("درخواست به‌روزرسانی انتسابات پزشک با شناسه: {DoctorId}", doctorId);

                // اعتبارسنجی پارامترها
                if (doctorId <= 0)
                {
                    return ServiceResult.Failed("شناسه پزشک نامعتبر است.");
                }

                if (assignments == null)
                {
                    return ServiceResult.Failed("مدل انتسابات نمی‌تواند خالی باشد.");
                }

                // تنظیم شناسه پزشک در مدل
                assignments.DoctorId = doctorId;

                // اعتبارسنجی مدل
                var validationResult = await _validator.ValidateAsync(assignments);
                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors.Select(e => new ValidationError(e.PropertyName, e.ErrorMessage)).ToList();
                    _logger.Warning("اعتبارسنجی مدل انتسابات پزشک ناموفق: {@Errors}", errors);
                    return ServiceResult.FailedWithValidationErrors("اطلاعات وارد شده صحیح نیست", errors);
                }

                // بررسی وجود پزشک
                var doctor = await _doctorRepository.GetByIdAsync(doctorId);
                if (doctor == null)
                {
                    _logger.Warning("پزشک با شناسه {DoctorId} یافت نشد", doctorId);
                    return ServiceResult.Failed("پزشک مورد نظر یافت نشد.");
                }

                // اعتبارسنجی سطح بالا
                var validationCheck = await ValidateAssignmentCompatibilityAsync(assignments);
                if (!validationCheck.Success)
                {
                    return validationCheck;
                }

                // به‌روزرسانی انتسابات دپارتمان‌ها
                await UpdateDepartmentAssignmentsAsync(doctorId, assignments.DoctorDepartments);

                // به‌روزرسانی انتسابات سرفصل‌های خدماتی
                await UpdateServiceCategoryAssignmentsAsync(doctorId, assignments.DoctorServiceCategories);

                _logger.Information("انتسابات پزشک {DoctorId} با موفقیت به‌روزرسانی شد", doctorId);

                return ServiceResult.Successful("انتسابات پزشک با موفقیت به‌روزرسانی شد.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی انتسابات پزشک {DoctorId}", doctorId);
                return ServiceResult.Failed("خطا در به‌روزرسانی انتسابات پزشک");
            }
        }

        /// <summary>
        /// دریافت تمام انتسابات یک پزشک (دپارتمان‌ها و سرفصل‌های خدماتی)
        /// </summary>
        public async Task<ServiceResult<DoctorAssignmentsViewModel>> GetDoctorAssignmentsAsync(int doctorId)
        {
            try
            {
                _logger.Information("درخواست دریافت انتسابات پزشک با شناسه: {DoctorId}", doctorId);

                // اعتبارسنجی پارامترها
                if (doctorId <= 0)
                {
                    return ServiceResult<DoctorAssignmentsViewModel>.Failed("شناسه پزشک نامعتبر است.");
                }

                // بررسی وجود پزشک
                var doctor = await _doctorRepository.GetByIdAsync(doctorId);
                if (doctor == null)
                {
                    _logger.Warning("پزشک با شناسه {DoctorId} یافت نشد", doctorId);
                    return ServiceResult<DoctorAssignmentsViewModel>.Failed("پزشک مورد نظر یافت نشد.");
                }

                // دریافت انتسابات دپارتمان‌ها
                var departmentAssignmentsResult = await _doctorDepartmentService.GetDepartmentsForDoctorAsync(doctorId, "", 1, int.MaxValue);
                var doctorDepartments = departmentAssignmentsResult.Success ? departmentAssignmentsResult.Data.Items : new List<DoctorDepartmentViewModel>();

                // دریافت انتسابات سرفصل‌های خدماتی
                var serviceCategoryAssignmentsResult = await _doctorServiceCategoryService.GetServiceCategoriesForDoctorAsync(doctorId, "", 1, int.MaxValue);
                var doctorServiceCategories = serviceCategoryAssignmentsResult.Success ? serviceCategoryAssignmentsResult.Data.Items : new List<DoctorServiceCategoryViewModel>();

                // ایجاد مدل انتسابات
                var assignments = new DoctorAssignmentsViewModel
                {
                    DoctorId = doctorId,
                    DoctorDepartments = doctorDepartments,
                    DoctorServiceCategories = doctorServiceCategories
                };

                _logger.Information("انتسابات پزشک {DoctorId} با موفقیت دریافت شد. تعداد دپارتمان‌ها: {DepartmentCount}, تعداد سرفصل‌ها: {ServiceCategoryCount}", 
                    doctorId, doctorDepartments.Count, doctorServiceCategories.Count);

                return ServiceResult<DoctorAssignmentsViewModel>.Successful(assignments);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت انتسابات پزشک {DoctorId}", doctorId);
                return ServiceResult<DoctorAssignmentsViewModel>.Failed("خطا در دریافت انتسابات پزشک");
            }
        }

        /// <summary>
        /// انتساب همزمان پزشک به دپارتمان و سرفصل‌های خدماتی مرتبط
        /// </summary>
        public async Task<ServiceResult> AssignDoctorToDepartmentWithServicesAsync(int doctorId, int departmentId, List<int> serviceCategoryIds)
        {
            try
            {
                _logger.Information("درخواست انتساب پزشک {DoctorId} به دپارتمان {DepartmentId} با سرفصل‌های خدماتی", doctorId, departmentId);

                // اعتبارسنجی پارامترها
                if (doctorId <= 0 || departmentId <= 0)
                {
                    return ServiceResult.Failed("شناسه پزشک یا دپارتمان نامعتبر است.");
                }

                // بررسی وجود پزشک
                var doctor = await _doctorRepository.GetByIdAsync(doctorId);
                if (doctor == null)
                {
                    return ServiceResult.Failed("پزشک مورد نظر یافت نشد.");
                }

                // انتساب به دپارتمان
                var departmentAssignment = new DoctorDepartmentViewModel
                {
                    DoctorId = doctorId,
                    DepartmentId = departmentId,
                    IsActive = true
                };

                var departmentResult = await _doctorDepartmentService.AssignDoctorToDepartmentAsync(departmentAssignment);
                if (!departmentResult.Success)
                {
                    return departmentResult;
                }

                // انتساب سرفصل‌های خدماتی
                if (serviceCategoryIds != null && serviceCategoryIds.Any())
                {
                    foreach (var serviceCategoryId in serviceCategoryIds)
                    {
                        var serviceAssignment = new DoctorServiceCategoryViewModel
                        {
                            DoctorId = doctorId,
                            ServiceCategoryId = serviceCategoryId,
                            IsActive = true
                        };

                        var serviceResult = await _doctorServiceCategoryService.GrantServiceCategoryToDoctorAsync(serviceAssignment);
                        if (!serviceResult.Success)
                        {
                            _logger.Warning("خطا در انتساب سرفصل خدماتی {ServiceCategoryId} به پزشک {DoctorId}", serviceCategoryId, doctorId);
                        }
                    }
                }

                _logger.Information("پزشک {DoctorId} با موفقیت به دپارتمان {DepartmentId} انتساب داده شد", doctorId, departmentId);
                return ServiceResult.Successful("پزشک با موفقیت به دپارتمان انتساب داده شد.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در انتساب پزشک {DoctorId} به دپارتمان {DepartmentId}", doctorId, departmentId);
                return ServiceResult.Failed("خطا در انتساب پزشک به دپارتمان");
            }
        }

        /// <summary>
        /// انتقال پزشک بین دپارتمان‌ها با حفظ صلاحیت‌های خدماتی
        /// </summary>
        public async Task<ServiceResult> TransferDoctorBetweenDepartmentsAsync(int doctorId, int fromDepartmentId, int toDepartmentId, bool preserveServiceCategories = true)
        {
            try
            {
                _logger.Information("درخواست انتقال پزشک {DoctorId} از دپارتمان {FromDepartmentId} به دپارتمان {ToDepartmentId}", 
                    doctorId, fromDepartmentId, toDepartmentId);

                // اعتبارسنجی پارامترها
                if (doctorId <= 0 || fromDepartmentId <= 0 || toDepartmentId <= 0)
                {
                    return ServiceResult.Failed("شناسه‌های وارد شده نامعتبر هستند.");
                }

                if (fromDepartmentId == toDepartmentId)
                {
                    return ServiceResult.Failed("دپارتمان مبدا و مقصد نمی‌توانند یکسان باشند.");
                }

                // بررسی وجود پزشک
                var doctor = await _doctorRepository.GetByIdAsync(doctorId);
                if (doctor == null)
                {
                    return ServiceResult.Failed("پزشک مورد نظر یافت نشد.");
                }

                // دریافت سرفصل‌های خدماتی فعلی (در صورت نیاز به حفظ)
                List<int> currentServiceCategories = new List<int>();
                if (preserveServiceCategories)
                {
                    var currentAssignmentsResult = await _doctorServiceCategoryService.GetServiceCategoriesForDoctorAsync(doctorId, "", 1, int.MaxValue);
                    if (currentAssignmentsResult.Success)
                    {
                        currentServiceCategories = currentAssignmentsResult.Data.Items
                            .Where(sc => sc.IsActive)
                            .Select(sc => sc.ServiceCategoryId)
                            .ToList();
                    }
                }

                // حذف انتساب از دپارتمان فعلی
                var removeResult = await _doctorDepartmentService.RevokeDoctorFromDepartmentAsync(doctorId, fromDepartmentId);
                if (!removeResult.Success)
                {
                    _logger.Warning("خطا در حذف انتساب پزشک {DoctorId} از دپارتمان {FromDepartmentId}", doctorId, fromDepartmentId);
                }

                // انتساب به دپارتمان جدید
                var newDepartmentAssignment = new DoctorDepartmentViewModel
                {
                    DoctorId = doctorId,
                    DepartmentId = toDepartmentId,
                    IsActive = true
                };

                var assignResult = await _doctorDepartmentService.AssignDoctorToDepartmentAsync(newDepartmentAssignment);
                if (!assignResult.Success)
                {
                    return assignResult;
                }

                // بازانتساب سرفصل‌های خدماتی (در صورت نیاز)
                if (preserveServiceCategories && currentServiceCategories.Any())
                {
                    foreach (var serviceCategoryId in currentServiceCategories)
                    {
                        var serviceAssignment = new DoctorServiceCategoryViewModel
                        {
                            DoctorId = doctorId,
                            ServiceCategoryId = serviceCategoryId,
                            IsActive = true
                        };

                        await _doctorServiceCategoryService.GrantServiceCategoryToDoctorAsync(serviceAssignment);
                    }
                }

                _logger.Information("پزشک {DoctorId} با موفقیت از دپارتمان {FromDepartmentId} به دپارتمان {ToDepartmentId} منتقل شد", 
                    doctorId, fromDepartmentId, toDepartmentId);

                return ServiceResult.Successful("پزشک با موفقیت بین دپارتمان‌ها منتقل شد.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در انتقال پزشک {DoctorId} بین دپارتمان‌ها", doctorId);
                return ServiceResult.Failed("خطا در انتقال پزشک بین دپارتمان‌ها");
            }
        }

        /// <summary>
        /// حذف کامل تمام انتسابات یک پزشک
        /// </summary>
        public async Task<ServiceResult> RemoveAllDoctorAssignmentsAsync(int doctorId)
        {
            try
            {
                _logger.Information("درخواست حذف تمام انتسابات پزشک {DoctorId}", doctorId);

                // اعتبارسنجی پارامترها
                if (doctorId <= 0)
                {
                    return ServiceResult.Failed("شناسه پزشک نامعتبر است.");
                }

                // بررسی وجود پزشک
                var doctor = await _doctorRepository.GetByIdAsync(doctorId);
                if (doctor == null)
                {
                    return ServiceResult.Failed("پزشک مورد نظر یافت نشد.");
                }

                // بررسی وابستگی‌ها
                var dependencies = await _doctorAssignmentRepository.GetDoctorDependenciesAsync(doctorId);
                if (dependencies.HasActiveAppointments)
                {
                    return ServiceResult.Failed($"پزشک دارای {dependencies.ActiveAppointmentsCount} نوبت فعال است و نمی‌توان انتسابات را حذف کرد.");
                }

                // حذف انتسابات دپارتمان‌ها
                var departmentAssignmentsResult = await _doctorDepartmentService.GetDepartmentsForDoctorAsync(doctorId, "", 1, int.MaxValue);
                if (departmentAssignmentsResult.Success)
                {
                    foreach (var assignment in departmentAssignmentsResult.Data.Items)
                    {
                        await _doctorDepartmentService.RevokeDoctorFromDepartmentAsync(doctorId, assignment.DepartmentId);
                    }
                }

                // حذف انتسابات سرفصل‌های خدماتی
                var serviceCategoryAssignmentsResult = await _doctorServiceCategoryService.GetServiceCategoriesForDoctorAsync(doctorId, "", 1, int.MaxValue);
                if (serviceCategoryAssignmentsResult.Success)
                {
                    foreach (var assignment in serviceCategoryAssignmentsResult.Data.Items)
                    {
                        await _doctorServiceCategoryService.RevokeServiceCategoryFromDoctorAsync(doctorId, assignment.ServiceCategoryId);
                    }
                }

                _logger.Information("تمام انتسابات پزشک {DoctorId} با موفقیت حذف شد", doctorId);
                return ServiceResult.Successful("تمام انتسابات پزشک با موفقیت حذف شد.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف تمام انتسابات پزشک {DoctorId}", doctorId);
                return ServiceResult.Failed("خطا در حذف انتسابات پزشک");
            }
        }

        #endregion

        #region Private Helper Methods (متدهای کمکی خصوصی)

        /// <summary>
        /// اعتبارسنجی سازگاری انتسابات
        /// </summary>
        private async Task<ServiceResult> ValidateAssignmentCompatibilityAsync(DoctorAssignmentsViewModel assignments)
        {
            try
            {
                // بررسی تداخل دپارتمان‌ها
                var departmentIds = assignments.DoctorDepartments?.Select(d => d.DepartmentId).Distinct().ToList() ?? new List<int>();
                if (departmentIds.Count != (assignments.DoctorDepartments?.Count ?? 0))
                {
                    return ServiceResult.Failed("انتساب تکراری به دپارتمان‌ها مجاز نیست.");
                }

                // بررسی تداخل سرفصل‌های خدماتی
                var serviceCategoryIds = assignments.DoctorServiceCategories?.Select(s => s.ServiceCategoryId).Distinct().ToList() ?? new List<int>();
                if (serviceCategoryIds.Count != (assignments.DoctorServiceCategories?.Count ?? 0))
                {
                    return ServiceResult.Failed("انتساب تکراری به سرفصل‌های خدماتی مجاز نیست.");
                }

                // بررسی سازگاری سرفصل‌های خدماتی با دپارتمان‌ها
                // این بخش می‌تواند بر اساس قوانین کسب‌وکار توسعه یابد

                return ServiceResult.Successful();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در اعتبارسنجی سازگاری انتسابات");
                return ServiceResult.Failed("خطا در اعتبارسنجی انتسابات");
            }
        }

        /// <summary>
        /// به‌روزرسانی انتسابات دپارتمان‌ها
        /// </summary>
        private async Task UpdateDepartmentAssignmentsAsync(int doctorId, List<DoctorDepartmentViewModel> newAssignments)
        {
            if (newAssignments == null) return;

            // دریافت انتسابات فعلی
            var currentAssignmentsResult = await _doctorDepartmentService.GetDepartmentsForDoctorAsync(doctorId, "", 1, int.MaxValue);
            var currentAssignments = currentAssignmentsResult.Success ? currentAssignmentsResult.Data.Items : new List<DoctorDepartmentViewModel>();

            // شناسایی انتسابات جدید
            var newDepartmentIds = newAssignments.Select(a => a.DepartmentId).ToList();
            var currentDepartmentIds = currentAssignments.Select(a => a.DepartmentId).ToList();

            // حذف انتسابات غیرضروری
            var assignmentsToRemove = currentAssignments.Where(a => !newDepartmentIds.Contains(a.DepartmentId)).ToList();
            foreach (var assignment in assignmentsToRemove)
            {
                await _doctorDepartmentService.RevokeDoctorFromDepartmentAsync(doctorId, assignment.DepartmentId);
            }

            // افزودن انتسابات جدید
            var assignmentsToAdd = newAssignments.Where(a => !currentDepartmentIds.Contains(a.DepartmentId)).ToList();
            foreach (var assignment in assignmentsToAdd)
            {
                await _doctorDepartmentService.AssignDoctorToDepartmentAsync(assignment);
            }

            // به‌روزرسانی انتسابات موجود
            var assignmentsToUpdate = newAssignments.Where(a => currentDepartmentIds.Contains(a.DepartmentId)).ToList();
            foreach (var assignment in assignmentsToUpdate)
            {
                await _doctorDepartmentService.UpdateDoctorDepartmentAssignmentAsync(assignment);
            }
        }

        /// <summary>
        /// به‌روزرسانی انتسابات سرفصل‌های خدماتی
        /// </summary>
        private async Task UpdateServiceCategoryAssignmentsAsync(int doctorId, List<DoctorServiceCategoryViewModel> newAssignments)
        {
            if (newAssignments == null) return;

            // دریافت انتسابات فعلی
            var currentAssignmentsResult = await _doctorServiceCategoryService.GetServiceCategoriesForDoctorAsync(doctorId, "", 1, int.MaxValue);
            var currentAssignments = currentAssignmentsResult.Success ? currentAssignmentsResult.Data.Items : new List<DoctorServiceCategoryViewModel>();

            // شناسایی انتسابات جدید
            var newServiceCategoryIds = newAssignments.Select(a => a.ServiceCategoryId).ToList();
            var currentServiceCategoryIds = currentAssignments.Select(a => a.ServiceCategoryId).ToList();

            // حذف انتسابات غیرضروری
            var assignmentsToRemove = currentAssignments.Where(a => !newServiceCategoryIds.Contains(a.ServiceCategoryId)).ToList();
            foreach (var assignment in assignmentsToRemove)
            {
                await _doctorServiceCategoryService.RevokeServiceCategoryFromDoctorAsync(doctorId, assignment.ServiceCategoryId);
            }

            // افزودن انتسابات جدید
            var assignmentsToAdd = newAssignments.Where(a => !currentServiceCategoryIds.Contains(a.ServiceCategoryId)).ToList();
            foreach (var assignment in assignmentsToAdd)
            {
                await _doctorServiceCategoryService.GrantServiceCategoryToDoctorAsync(assignment);
            }

            // به‌روزرسانی انتسابات موجود
            var assignmentsToUpdate = newAssignments.Where(a => currentServiceCategoryIds.Contains(a.ServiceCategoryId)).ToList();
            foreach (var assignment in assignmentsToUpdate)
            {
                await _doctorServiceCategoryService.UpdateDoctorServiceCategoryPermissionAsync(assignment);
            }
        }

        #endregion
    }
}
