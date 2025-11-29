using ClinicApp.Core;
using ClinicApp.Helpers;
using ClinicApp.Interfaces.ClinicAdmin;
using ClinicApp.Models.Entities.Clinic;
using ClinicApp.ViewModels.ClinicAdmin;
using FluentValidation;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Interfaces;

namespace ClinicApp.Services.ClinicAdmin
{
    /// <summary>
    /// Service Implementation برای مدیریت حساب بانکی کلینیک
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. مدیریت حساب بانکی هر کلینیک (شماره شبا)
    /// 2. Validation کامل شماره شبا
    /// 3. مدیریت رابطه One-to-One با Clinic
    /// 4. استفاده از ServiceResult Pattern
    /// 
    /// Architecture Principles:
    /// ✅ Single Responsibility: فقط منطق کسب‌وکار حساب بانکی
    /// ✅ Separation of Concerns: جدا از Repository و Controller
    /// ✅ High Testability: Interface ساده برای Mock
    /// ✅ Clean Architecture: Service layer فقط منطق کسب‌وکار
    /// </summary>
    public class ClinicBankAccountService : IClinicBankAccountService
    {
        private readonly IClinicBankAccountRepository _repository;
        private readonly IClinicRepository _clinicRepository;
        private readonly IValidator<ClinicBankAccountCreateEditViewModel> _validator;
        private readonly ILogger _logger;
        private readonly ICurrentUserService _currentUserService;

        public ClinicBankAccountService(
            IClinicBankAccountRepository repository,
            IClinicRepository clinicRepository,
            IValidator<ClinicBankAccountCreateEditViewModel> validator,
            ILogger logger,
            ICurrentUserService currentUserService)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _clinicRepository = clinicRepository ?? throw new ArgumentNullException(nameof(clinicRepository));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _logger = logger?.ForContext<ClinicBankAccountService>() ?? throw new ArgumentNullException(nameof(logger));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        /// <summary>
        /// ایجاد حساب بانکی جدید برای کلینیک
        /// </summary>
        public async Task<ServiceResult<int>> CreateAsync(ClinicBankAccountCreateEditViewModel model)
        {
            _logger.Information("🏥 MEDICAL: درخواست ایجاد حساب بانکی برای کلینیک: {ClinicId}, User: {UserId}",
                model.ClinicId, _currentUserService?.UserId ?? "Anonymous");

            // Validation با FluentValidation
            var validationResult = await _validator.ValidateAsync(model);
            if (!validationResult.IsValid)
            {
                _logger.Warning("🏥 MEDICAL: اعتبارسنجی برای ایجاد حساب بانکی ناموفق بود: {@ValidationErrors}",
                    validationResult.Errors);

                var validationErrors = validationResult.Errors
                    .Select(err => new ValidationError(err.PropertyName, err.ErrorMessage));

                return ServiceResult<int>.FailedWithValidationErrors(
                    "اطلاعات ورودی نامعتبر است.",
                    validationErrors
                );
            }

            try
            {
                // بررسی وجود کلینیک
                var clinic = await _clinicRepository.GetByIdAsync(model.ClinicId);
                if (clinic == null)
                {
                    _logger.Warning("🏥 MEDICAL: کلینیک با شناسه {ClinicId} یافت نشد", model.ClinicId);
                    return ServiceResult<int>.Failed("کلینیک مورد نظر یافت نشد.", "CLINIC_NOT_FOUND", ErrorCategory.NotFound);
                }

                // بررسی وجود حساب بانکی برای این کلینیک
                var existingAccount = await _repository.ExistsForClinicAsync(model.ClinicId);
                if (existingAccount)
                {
                    _logger.Warning("🏥 MEDICAL: حساب بانکی برای کلینیک {ClinicId} از قبل وجود دارد", model.ClinicId);
                    return ServiceResult<int>.Failed("برای این کلینیک قبلاً حساب بانکی تعریف شده است.", "ACCOUNT_EXISTS", ErrorCategory.BusinessLogic);
                }

                // بررسی تکراری بودن شماره شبا
                var ibanExists = await _repository.IbanNumberExistsAsync(model.IbanNumber);
                if (ibanExists)
                {
                    _logger.Warning("🏥 MEDICAL: شماره شبا {IbanNumber} از قبل وجود دارد", model.IbanNumber);
                    return ServiceResult<int>.Failed("این شماره شبا قبلاً ثبت شده است.", "IBAN_EXISTS", ErrorCategory.BusinessLogic);
                }

                // ایجاد Entity
                var entity = new ClinicBankAccount
                {
                    ClinicId = model.ClinicId,
                    CreatedAt = DateTime.Now,
                    CreatedByUserId = _currentUserService?.UserId
                };

                model.MapToEntity(entity);

                // ذخیره
                _repository.Add(entity);
                await _repository.SaveChangesAsync();

                _logger.Information("🏥 MEDICAL: حساب بانکی با شناسه {ClinicBankAccountId} برای کلینیک {ClinicId} با موفقیت ایجاد شد. User: {UserId}",
                    entity.ClinicBankAccountId, model.ClinicId, _currentUserService?.UserId ?? "Anonymous");

                return ServiceResult<int>.Successful(entity.ClinicBankAccountId, "حساب بانکی با موفقیت ایجاد شد.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطای سیستمی در هنگام ایجاد حساب بانکی برای کلینیک {ClinicId}, User: {UserId}",
                    model.ClinicId, _currentUserService?.UserId ?? "Anonymous");
                return ServiceResult<int>.Failed("خطای سیستمی در هنگام ایجاد حساب بانکی رخ داد.", "DB_ERROR", ErrorCategory.Database);
            }
        }

        /// <summary>
        /// به‌روزرسانی حساب بانکی
        /// </summary>
        public async Task<ServiceResult> UpdateAsync(ClinicBankAccountCreateEditViewModel model)
        {
            _logger.Information("🏥 MEDICAL: درخواست به‌روزرسانی حساب بانکی: {ClinicBankAccountId}, User: {UserId}",
                model.ClinicBankAccountId, _currentUserService?.UserId ?? "Anonymous");

            // Validation با FluentValidation
            var validationResult = await _validator.ValidateAsync(model);
            if (!validationResult.IsValid)
            {
                _logger.Warning("🏥 MEDICAL: اعتبارسنجی برای به‌روزرسانی حساب بانکی ناموفق بود: {@ValidationErrors}",
                    validationResult.Errors);

                var validationErrors = validationResult.Errors
                    .Select(err => new ValidationError(err.PropertyName, err.ErrorMessage));

                return ServiceResult.FailedWithValidationErrors(
                    "اطلاعات ورودی نامعتبر است.",
                    validationErrors
                );
            }

            try
            {
                // دریافت Entity
                var entity = await _repository.GetByIdAsync(model.ClinicBankAccountId);
                if (entity == null)
                {
                    _logger.Warning("🏥 MEDICAL: حساب بانکی با شناسه {ClinicBankAccountId} یافت نشد", model.ClinicBankAccountId);
                    return ServiceResult.Failed("حساب بانکی مورد نظر یافت نشد.", "NOT_FOUND", ErrorCategory.NotFound);
                }

                // بررسی تکراری بودن شماره شبا (به جز خودش)
                var ibanExists = await _repository.IbanNumberExistsAsync(model.IbanNumber, model.ClinicBankAccountId);
                if (ibanExists)
                {
                    _logger.Warning("🏥 MEDICAL: شماره شبا {IbanNumber} از قبل وجود دارد", model.IbanNumber);
                    return ServiceResult.Failed("این شماره شبا قبلاً ثبت شده است.", "IBAN_EXISTS", ErrorCategory.BusinessLogic);
                }

                // به‌روزرسانی
                model.MapToEntity(entity);
                entity.UpdatedAt = DateTime.Now;
                entity.UpdatedByUserId = _currentUserService?.UserId;

                _repository.Update(entity);
                await _repository.SaveChangesAsync();

                _logger.Information("🏥 MEDICAL: حساب بانکی با شناسه {ClinicBankAccountId} با موفقیت به‌روزرسانی شد. User: {UserId}",
                    model.ClinicBankAccountId, _currentUserService?.UserId ?? "Anonymous");

                return ServiceResult.Successful("حساب بانکی با موفقیت به‌روزرسانی شد.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطای سیستمی در هنگام به‌روزرسانی حساب بانکی {ClinicBankAccountId}, User: {UserId}",
                    model.ClinicBankAccountId, _currentUserService?.UserId ?? "Anonymous");
                return ServiceResult.Failed("خطای سیستمی در هنگام به‌روزرسانی حساب بانکی رخ داد.", "DB_ERROR", ErrorCategory.Database);
            }
        }

        /// <summary>
        /// دریافت حساب بانکی بر اساس شناسه
        /// </summary>
        public async Task<ServiceResult<ClinicBankAccountDetailsViewModel>> GetByIdAsync(int clinicBankAccountId)
        {
            try
            {
                _logger.Information("🏥 MEDICAL: درخواست دریافت حساب بانکی: {ClinicBankAccountId}, User: {UserId}",
                    clinicBankAccountId, _currentUserService?.UserId ?? "Anonymous");

                var entity = await _repository.GetByIdAsync(clinicBankAccountId);
                if (entity == null)
                {
                    _logger.Warning("🏥 MEDICAL: حساب بانکی با شناسه {ClinicBankAccountId} یافت نشد", clinicBankAccountId);
                    return ServiceResult<ClinicBankAccountDetailsViewModel>.Failed("حساب بانکی مورد نظر یافت نشد.", "NOT_FOUND", ErrorCategory.NotFound);
                }

                var viewModel = ClinicBankAccountDetailsViewModel.FromEntity(entity);

                _logger.Information("🏥 MEDICAL: حساب بانکی با شناسه {ClinicBankAccountId} با موفقیت دریافت شد. User: {UserId}",
                    clinicBankAccountId, _currentUserService?.UserId ?? "Anonymous");

                return ServiceResult<ClinicBankAccountDetailsViewModel>.Successful(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطا در دریافت حساب بانکی {ClinicBankAccountId}, User: {UserId}",
                    clinicBankAccountId, _currentUserService?.UserId ?? "Anonymous");
                return ServiceResult<ClinicBankAccountDetailsViewModel>.Failed("خطای سیستمی در دریافت اطلاعات رخ داد.", "DB_ERROR");
            }
        }

        /// <summary>
        /// دریافت حساب بانکی بر اساس شناسه کلینیک
        /// </summary>
        public async Task<ServiceResult<ClinicBankAccountDetailsViewModel>> GetByClinicIdAsync(int clinicId)
        {
            try
            {
                _logger.Information("🏥 MEDICAL: درخواست دریافت حساب بانکی برای کلینیک: {ClinicId}, User: {UserId}",
                    clinicId, _currentUserService?.UserId ?? "Anonymous");

                var entity = await _repository.GetByClinicIdAsync(clinicId);
                if (entity == null)
                {
                    _logger.Warning("🏥 MEDICAL: حساب بانکی برای کلینیک {ClinicId} یافت نشد", clinicId);
                    return ServiceResult<ClinicBankAccountDetailsViewModel>.Failed("حساب بانکی برای این کلینیک یافت نشد.", "NOT_FOUND", ErrorCategory.NotFound);
                }

                var viewModel = ClinicBankAccountDetailsViewModel.FromEntity(entity);

                _logger.Information("🏥 MEDICAL: حساب بانکی برای کلینیک {ClinicId} با موفقیت دریافت شد. User: {UserId}",
                    clinicId, _currentUserService?.UserId ?? "Anonymous");

                return ServiceResult<ClinicBankAccountDetailsViewModel>.Successful(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطا در دریافت حساب بانکی برای کلینیک {ClinicId}, User: {UserId}",
                    clinicId, _currentUserService?.UserId ?? "Anonymous");
                return ServiceResult<ClinicBankAccountDetailsViewModel>.Failed("خطای سیستمی در دریافت اطلاعات رخ داد.", "DB_ERROR");
            }
        }

        /// <summary>
        /// دریافت لیست تمام حساب‌های بانکی
        /// </summary>
        public async Task<ServiceResult<List<ClinicBankAccountIndexViewModel>>> GetAllAsync()
        {
            try
            {
                _logger.Information("🏥 MEDICAL: درخواست دریافت لیست حساب‌های بانکی, User: {UserId}",
                    _currentUserService?.UserId ?? "Anonymous");

                var entities = await _repository.GetAllAsync();
                var viewModels = entities.Select(ClinicBankAccountIndexViewModel.FromEntity).ToList();

                _logger.Information("🏥 MEDICAL: {Count} حساب بانکی دریافت شد. User: {UserId}",
                    viewModels.Count, _currentUserService?.UserId ?? "Anonymous");

                return ServiceResult<List<ClinicBankAccountIndexViewModel>>.Successful(viewModels);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطا در دریافت لیست حساب‌های بانکی, User: {UserId}",
                    _currentUserService?.UserId ?? "Anonymous");
                return ServiceResult<List<ClinicBankAccountIndexViewModel>>.Failed("خطای سیستمی در دریافت اطلاعات رخ داد.", "DB_ERROR");
            }
        }

        /// <summary>
        /// دریافت اطلاعات برای ویرایش
        /// </summary>
        public async Task<ServiceResult<ClinicBankAccountCreateEditViewModel>> GetForEditAsync(int clinicBankAccountId)
        {
            try
            {
                _logger.Information("🏥 MEDICAL: درخواست دریافت اطلاعات برای ویرایش حساب بانکی: {ClinicBankAccountId}, User: {UserId}",
                    clinicBankAccountId, _currentUserService?.UserId ?? "Anonymous");

                var entity = await _repository.GetByIdAsync(clinicBankAccountId);
                if (entity == null)
                {
                    _logger.Warning("🏥 MEDICAL: حساب بانکی با شناسه {ClinicBankAccountId} یافت نشد", clinicBankAccountId);
                    return ServiceResult<ClinicBankAccountCreateEditViewModel>.Failed("حساب بانکی مورد نظر یافت نشد.", "NOT_FOUND", ErrorCategory.NotFound);
                }

                var viewModel = ClinicBankAccountCreateEditViewModel.FromEntity(entity);

                _logger.Information("🏥 MEDICAL: اطلاعات حساب بانکی {ClinicBankAccountId} برای ویرایش دریافت شد. User: {UserId}",
                    clinicBankAccountId, _currentUserService?.UserId ?? "Anonymous");

                return ServiceResult<ClinicBankAccountCreateEditViewModel>.Successful(viewModel);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطا در دریافت اطلاعات برای ویرایش حساب بانکی {ClinicBankAccountId}, User: {UserId}",
                    clinicBankAccountId, _currentUserService?.UserId ?? "Anonymous");
                return ServiceResult<ClinicBankAccountCreateEditViewModel>.Failed("خطای سیستمی در دریافت اطلاعات رخ داد.", "DB_ERROR");
            }
        }

        /// <summary>
        /// حذف حساب بانکی (Soft Delete)
        /// </summary>
        public async Task<ServiceResult> DeleteAsync(int clinicBankAccountId)
        {
            try
            {
                _logger.Information("🏥 MEDICAL: درخواست حذف حساب بانکی: {ClinicBankAccountId}, User: {UserId}",
                    clinicBankAccountId, _currentUserService?.UserId ?? "Anonymous");

                var entity = await _repository.GetByIdAsync(clinicBankAccountId);
                if (entity == null)
                {
                    _logger.Warning("🏥 MEDICAL: حساب بانکی با شناسه {ClinicBankAccountId} یافت نشد", clinicBankAccountId);
                    return ServiceResult.Failed("حساب بانکی مورد نظر یافت نشد.", "NOT_FOUND", ErrorCategory.NotFound);
                }

                _repository.Delete(entity);
                await _repository.SaveChangesAsync();

                _logger.Information("🏥 MEDICAL: حساب بانکی با شناسه {ClinicBankAccountId} با موفقیت حذف شد. User: {UserId}",
                    clinicBankAccountId, _currentUserService?.UserId ?? "Anonymous");

                return ServiceResult.Successful("حساب بانکی با موفقیت حذف شد.");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطا در حذف حساب بانکی {ClinicBankAccountId}, User: {UserId}",
                    clinicBankAccountId, _currentUserService?.UserId ?? "Anonymous");
                return ServiceResult.Failed("خطای سیستمی در حذف حساب بانکی رخ داد.", "DB_ERROR");
            }
        }

        /// <summary>
        /// بررسی وجود حساب بانکی برای کلینیک
        /// </summary>
        public async Task<ServiceResult<bool>> ExistsForClinicAsync(int clinicId)
        {
            try
            {
                var exists = await _repository.ExistsForClinicAsync(clinicId);
                return ServiceResult<bool>.Successful(exists);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "🏥 MEDICAL: خطا در بررسی وجود حساب بانکی برای کلینیک {ClinicId}", clinicId);
                return ServiceResult<bool>.Failed("خطای سیستمی در بررسی اطلاعات رخ داد.", "DB_ERROR");
            }
        }
    }
}

