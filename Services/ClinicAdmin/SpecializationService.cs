using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.ClinicAdmin;
using ClinicApp.Models.Entities;
using FluentValidation;
using FluentValidation.Results;
using Serilog;

namespace ClinicApp.Services.ClinicAdmin
{
    /// <summary>
    /// سرویس برای مدیریت عملیات تخصص‌ها در سیستم کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. پیاده‌سازی کامل عملیات CRUD با اعتبارسنجی
    /// 2. مدیریت رابطه Many-to-Many با پزشکان
    /// 3. پشتیبانی از سیستم حذف نرم (Soft Delete)
    /// 4. مدیریت ردیابی (Audit Trail)
    /// 5. پشتیبانی از ترتیب نمایش (DisplayOrder)
    /// </summary>
    public class SpecializationService : ISpecializationService
    {
        private readonly ISpecializationRepository _specializationRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger _logger;

        public SpecializationService(
            ISpecializationRepository specializationRepository,
            ICurrentUserService currentUserService)
        {
            _specializationRepository = specializationRepository ?? throw new ArgumentNullException(nameof(specializationRepository));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _logger = Log.ForContext<SpecializationService>();
        }

        #region Core Operations

        /// <summary>
        /// دریافت لیست تخصص‌های فعال برای نمایش در Dropdown
        /// </summary>
        public async Task<ServiceResult<List<Specialization>>> GetActiveSpecializationsAsync()
        {
            try
            {
                _logger.Information("درخواست دریافت لیست تخصص‌های فعال");

                var specializations = await _specializationRepository.GetActiveSpecializationsAsync();

                _logger.Information("لیست تخصص‌های فعال با موفقیت دریافت شد. تعداد: {Count}", specializations.Count);

                return ServiceResult<List<Specialization>>.Successful(specializations);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت لیست تخصص‌های فعال");
                return ServiceResult<List<Specialization>>.Failed("خطا در دریافت لیست تخصص‌های فعال");
            }
        }

        /// <summary>
        /// دریافت لیست تمام تخصص‌ها برای مدیریت
        /// </summary>
        public async Task<ServiceResult<List<Specialization>>> GetAllSpecializationsAsync()
        {
            try
            {
                _logger.Information("درخواست دریافت لیست تمام تخصص‌ها");

                var specializations = await _specializationRepository.GetAllSpecializationsAsync();

                _logger.Information("لیست تمام تخصص‌ها با موفقیت دریافت شد. تعداد: {Count}", specializations.Count);

                return ServiceResult<List<Specialization>>.Successful(specializations);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت لیست تمام تخصص‌ها");
                return ServiceResult<List<Specialization>>.Failed("خطا در دریافت لیست تمام تخصص‌ها");
            }
        }

        /// <summary>
        /// دریافت تخصص بر اساس شناسه
        /// </summary>
        public async Task<ServiceResult<Specialization>> GetSpecializationByIdAsync(int specializationId)
        {
            try
            {
                _logger.Information("درخواست دریافت تخصص با شناسه: {SpecializationId}", specializationId);

                if (specializationId <= 0)
                {
                    return ServiceResult<Specialization>.Failed("شناسه تخصص نامعتبر است.");
                }

                var specialization = await _specializationRepository.GetByIdAsync(specializationId);

                if (specialization == null)
                {
                    _logger.Warning("تخصص با شناسه {SpecializationId} یافت نشد", specializationId);
                    return ServiceResult<Specialization>.Failed("تخصص مورد نظر یافت نشد.");
                }

                _logger.Information("تخصص با شناسه {SpecializationId} با موفقیت دریافت شد", specializationId);

                return ServiceResult<Specialization>.Successful(specialization);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تخصص {SpecializationId}", specializationId);
                return ServiceResult<Specialization>.Failed("خطا در دریافت تخصص");
            }
        }

        /// <summary>
        /// ایجاد تخصص جدید
        /// </summary>
        public async Task<ServiceResult<Specialization>> CreateSpecializationAsync(Specialization specialization)
        {
            try
            {
                _logger.Information("درخواست ایجاد تخصص جدید: {@Specialization}", specialization);

                if (specialization == null)
                {
                    return ServiceResult<Specialization>.Failed("اطلاعات تخصص وارد نشده است.");
                }

                // اعتبارسنجی
                var validationResult = await ValidateSpecializationAsync(specialization);
                if (!validationResult.IsValid)
                {
                    _logger.Warning("اعتبارسنجی تخصص ناموفق: {Errors}", validationResult.Errors);
                    return ServiceResult<Specialization>.Failed(validationResult.Errors.FirstOrDefault()?.ErrorMessage ?? "اطلاعات وارد شده صحیح نیست");
                }

                // بررسی تکراری نبودن نام
                var nameExists = await _specializationRepository.DoesSpecializationNameExistAsync(specialization.Name);
                if (nameExists)
                {
                    return ServiceResult<Specialization>.Failed("نام تخصص تکراری است.");
                }

                // تنظیم اطلاعات ردیابی
                var currentUserId = _currentUserService.UserId;
                specialization.CreatedByUserId = currentUserId;
                specialization.UpdatedByUserId = currentUserId;

                var createdSpecialization = await _specializationRepository.AddAsync(specialization);

                _logger.Information("تخصص جدید با شناسه {SpecializationId} با موفقیت ایجاد شد", createdSpecialization.SpecializationId);

                return ServiceResult<Specialization>.Successful(createdSpecialization);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد تخصص جدید");
                return ServiceResult<Specialization>.Failed("خطا در ایجاد تخصص جدید");
            }
        }

        /// <summary>
        /// به‌روزرسانی تخصص موجود
        /// </summary>
        public async Task<ServiceResult<Specialization>> UpdateSpecializationAsync(Specialization specialization)
        {
            try
            {
                _logger.Information("درخواست به‌روزرسانی تخصص با شناسه: {SpecializationId}", specialization.SpecializationId);

                if (specialization == null)
                {
                    return ServiceResult<Specialization>.Failed("اطلاعات تخصص وارد نشده است.");
                }

                // اعتبارسنجی
                var validationResult = await ValidateSpecializationAsync(specialization);
                if (!validationResult.IsValid)
                {
                    _logger.Warning("اعتبارسنجی تخصص ناموفق: {Errors}", validationResult.Errors);
                    return ServiceResult<Specialization>.Failed(validationResult.Errors.FirstOrDefault()?.ErrorMessage ?? "اطلاعات وارد شده صحیح نیست");
                }

                // بررسی وجود تخصص
                var existingSpecialization = await _specializationRepository.GetByIdAsync(specialization.SpecializationId);
                if (existingSpecialization == null)
                {
                    _logger.Warning("تخصص با شناسه {SpecializationId} برای به‌روزرسانی یافت نشد", specialization.SpecializationId);
                    return ServiceResult<Specialization>.Failed("تخصص مورد نظر یافت نشد.");
                }

                // بررسی تکراری نبودن نام (به جز خودش)
                var nameExists = await _specializationRepository.DoesSpecializationNameExistAsync(specialization.Name, specialization.SpecializationId);
                if (nameExists)
                {
                    return ServiceResult<Specialization>.Failed("نام تخصص تکراری است.");
                }

                // تنظیم اطلاعات ردیابی
                specialization.UpdatedByUserId = _currentUserService.UserId;

                var updatedSpecialization = await _specializationRepository.UpdateAsync(specialization);

                _logger.Information("تخصص با شناسه {SpecializationId} با موفقیت به‌روزرسانی شد", specialization.SpecializationId);

                return ServiceResult<Specialization>.Successful(updatedSpecialization);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی تخصص {SpecializationId}", specialization.SpecializationId);
                return ServiceResult<Specialization>.Failed("خطا در به‌روزرسانی تخصص");
            }
        }

        /// <summary>
        /// حذف نرم تخصص
        /// </summary>
        public async Task<ServiceResult> SoftDeleteSpecializationAsync(int specializationId)
        {
            try
            {
                _logger.Information("درخواست حذف نرم تخصص با شناسه: {SpecializationId}", specializationId);

                if (specializationId <= 0)
                {
                    return ServiceResult.Failed("شناسه تخصص نامعتبر است.");
                }

                // بررسی وجود تخصص
                var specialization = await _specializationRepository.GetByIdAsync(specializationId);
                if (specialization == null)
                {
                    _logger.Warning("تخصص با شناسه {SpecializationId} برای حذف یافت نشد", specializationId);
                    return ServiceResult.Failed("تخصص مورد نظر یافت نشد.");
                }

                // بررسی پزشکان فعال مرتبط با این تخصص
                var activeDoctorsCount = await _specializationRepository.GetActiveDoctorsCountAsync(specializationId);
                if (activeDoctorsCount > 0)
                {
                    _logger.Warning("تلاش برای حذف تخصص {SpecializationId} که {DoctorCount} پزشک فعال دارد", specializationId, activeDoctorsCount);
                    return ServiceResult.Failed($"این تخصص قابل حذف نیست زیرا {activeDoctorsCount} پزشک فعال به آن متصل است. ابتدا پزشکان را از این تخصص جدا کنید.");
                }

                var currentUserId = _currentUserService.UserId;
                var result = await _specializationRepository.SoftDeleteAsync(specializationId, currentUserId);

                if (result)
                {
                    _logger.Information("تخصص با شناسه {SpecializationId} با موفقیت حذف شد", specializationId);
                    return ServiceResult.Successful();
                }
                else
                {
                    return ServiceResult.Failed("خطا در حذف تخصص");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف نرم تخصص {SpecializationId}", specializationId);
                return ServiceResult.Failed("خطا در حذف تخصص");
            }
        }

        /// <summary>
        /// بازیابی تخصص حذف شده
        /// </summary>
        public async Task<ServiceResult> RestoreSpecializationAsync(int specializationId)
        {
            try
            {
                _logger.Information("درخواست بازیابی تخصص با شناسه: {SpecializationId}", specializationId);

                if (specializationId <= 0)
                {
                    return ServiceResult.Failed("شناسه تخصص نامعتبر است.");
                }

                var currentUserId = _currentUserService.UserId;
                var result = await _specializationRepository.RestoreAsync(specializationId, currentUserId);

                if (result)
                {
                    _logger.Information("تخصص با شناسه {SpecializationId} با موفقیت بازیابی شد", specializationId);
                    return ServiceResult.Successful();
                }
                else
                {
                    return ServiceResult.Failed("تخصص مورد نظر یافت نشد یا قبلاً بازیابی شده است.");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بازیابی تخصص {SpecializationId}", specializationId);
                return ServiceResult.Failed("خطا در بازیابی تخصص");
            }
        }

        #endregion

        #region Doctor-Specialization Relationship

        /// <summary>
        /// دریافت تخصص‌های یک پزشک
        /// </summary>
        public async Task<ServiceResult<List<Specialization>>> GetDoctorSpecializationsAsync(int doctorId)
        {
            try
            {
                _logger.Information("درخواست دریافت تخصص‌های پزشک با شناسه: {DoctorId}", doctorId);

                if (doctorId <= 0)
                {
                    return ServiceResult<List<Specialization>>.Failed("شناسه پزشک نامعتبر است.");
                }

                var specializations = await _specializationRepository.GetDoctorSpecializationsAsync(doctorId);

                _logger.Information("تخصص‌های پزشک با شناسه {DoctorId} با موفقیت دریافت شد. تعداد: {Count}", doctorId, specializations.Count);

                return ServiceResult<List<Specialization>>.Successful(specializations);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تخصص‌های پزشک {DoctorId}", doctorId);
                return ServiceResult<List<Specialization>>.Failed("خطا در دریافت تخصص‌های پزشک");
            }
        }

        /// <summary>
        /// به‌روزرسانی تخصص‌های یک پزشک
        /// </summary>
        public async Task<ServiceResult> UpdateDoctorSpecializationsAsync(int doctorId, List<int> specializationIds)
        {
            try
            {
                _logger.Information("درخواست به‌روزرسانی تخصص‌های پزشک با شناسه: {DoctorId}", doctorId);

                if (doctorId <= 0)
                {
                    return ServiceResult.Failed("شناسه پزشک نامعتبر است.");
                }

                var result = await _specializationRepository.UpdateDoctorSpecializationsAsync(doctorId, specializationIds);

                if (result)
                {
                    _logger.Information("تخصص‌های پزشک با شناسه {DoctorId} با موفقیت به‌روزرسانی شد", doctorId);
                    return ServiceResult.Successful();
                }
                else
                {
                    return ServiceResult.Failed("پزشک مورد نظر یافت نشد.");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی تخصص‌های پزشک {DoctorId}", doctorId);
                return ServiceResult.Failed("خطا در به‌روزرسانی تخصص‌های پزشک");
            }
        }

        /// <summary>
        /// دریافت تخصص‌ها بر اساس لیست شناسه‌ها
        /// </summary>
        public async Task<ServiceResult<List<Specialization>>> GetSpecializationsByIdsAsync(List<int> specializationIds)
        {
            try
            {
                if (specializationIds == null || !specializationIds.Any())
                {
                    return ServiceResult<List<Specialization>>.Successful(new List<Specialization>());
                }

                var specializations = await _specializationRepository.GetSpecializationsByIdsAsync(specializationIds);
                return ServiceResult<List<Specialization>>.Successful(specializations);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تخصص‌ها بر اساس شناسه‌ها");
                return ServiceResult<List<Specialization>>.Failed("خطا در دریافت تخصص‌ها");
            }
        }

        #endregion

        #region Validation

        /// <summary>
        /// بررسی وجود نام تخصص
        /// </summary>
        public async Task<ServiceResult<bool>> DoesSpecializationNameExistAsync(string name, int? excludeSpecializationId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return ServiceResult<bool>.Successful(false);
                }

                var exists = await _specializationRepository.DoesSpecializationNameExistAsync(name, excludeSpecializationId);
                return ServiceResult<bool>.Successful(exists);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی وجود نام تخصص: {Name}", name);
                return ServiceResult<bool>.Failed("خطا در بررسی وجود نام تخصص");
            }
        }

        /// <summary>
        /// دریافت تعداد پزشکان فعال مرتبط با تخصص
        /// </summary>
        public async Task<ServiceResult<int>> GetActiveDoctorsCountAsync(int specializationId)
        {
            try
            {
                if (specializationId <= 0)
                {
                    return ServiceResult<int>.Failed("شناسه تخصص نامعتبر است.");
                }

                var count = await _specializationRepository.GetActiveDoctorsCountAsync(specializationId);
                return ServiceResult<int>.Successful(count);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تعداد پزشکان فعال برای تخصص {SpecializationId}", specializationId);
                return ServiceResult<int>.Failed("خطا در دریافت تعداد پزشکان فعال");
            }
        }

        /// <summary>
        /// اعتبارسنجی تخصص
        /// </summary>
        private async Task<FluentValidation.Results.ValidationResult> ValidateSpecializationAsync(Specialization specialization)
        {
            var validator = new SpecializationValidator();
            return await validator.ValidateAsync(specialization);
        }

        #endregion
    }

    /// <summary>
    /// اعتبارسنج تخصص
    /// </summary>
    public class SpecializationValidator : AbstractValidator<Specialization>
    {
        public SpecializationValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("نام تخصص الزامی است.")
                .MaximumLength(100).WithMessage("نام تخصص نمی‌تواند بیشتر از 100 کاراکتر باشد.");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("توضیحات نمی‌تواند بیشتر از 500 کاراکتر باشد.");

            RuleFor(x => x.DisplayOrder)
                .GreaterThanOrEqualTo(0).WithMessage("ترتیب نمایش باید بزرگتر یا مساوی صفر باشد.");
        }
    }
}
