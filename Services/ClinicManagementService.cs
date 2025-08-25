using ClinicApp.Core;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.ClinicAdmin;
using ClinicApp.Models.Entities;
using ClinicApp.ViewModels;
using FluentValidation;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClinicApp.Services
{
    public class ClinicManagementService : IClinicManagementService
    {
        private readonly IClinicRepository _clinicRepo;
        private readonly IValidator<ClinicCreateEditViewModel> _validator;
        private readonly ILogger _log;

        public ClinicManagementService(
            IClinicRepository clinicRepository,
            IValidator<ClinicCreateEditViewModel> validator,
            ILogger logger)
        {
            _clinicRepo = clinicRepository;
            _validator = validator;
            _log = logger.ForContext<ClinicManagementService>();
        }

        public async Task<ServiceResult> CreateClinicAsync(ClinicCreateEditViewModel model)
        {
            _log.Information("درخواست ایجاد کلینیک جدید با نام: {ClinicName}", model.Name);

            var validationResult = await _validator.ValidateAsync(model);
            if (!validationResult.IsValid)
            {
                _log.Warning("اعتبارسنجی برای ایجاد کلینیک ناموفق بود: {@ValidationErrors}", validationResult.Errors);

                // ✅ **تغییر کلیدی و رفع خطا:**
                // ما لیست خطاهای FluentValidation را به لیست ValidationError خودمان تبدیل می‌کنیم.
                var validationErrors = validationResult.Errors
                    .Select(err => new ValidationError(err.PropertyName, err.ErrorMessage));

                return ServiceResult.FailedWithValidationErrors(
                    "اطلاعات ورودی نامعتبر است.", // یک پیام عمومی برای نتیجه کلی
                    validationErrors // پاس دادن لیست تبدیل شده
                );
            }

            try
            {
                var clinic = new Clinic();
                model.MapToEntity(clinic);

                _clinicRepo.Add(clinic);
                await _clinicRepo.SaveChangesAsync();

                _log.Information("کلینیک جدید با شناسه {ClinicId} و نام {ClinicName} با موفقیت ایجاد شد.", clinic.ClinicId, clinic.Name);
                return ServiceResult.Successful("کلینیک با موفقیت ایجاد شد.");
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطای سیستمی در هنگام ایجاد کلینیک با نام {ClinicName}", model.Name);
                return ServiceResult.Failed("خطای سیستمی در هنگام ایجاد کلینیک رخ داد. لطفاً با پشتیبانی تماس بگیرید.", "DB_ERROR", ErrorCategory.Database);
            }
        }

        public async Task<ServiceResult<PagedResult<ClinicIndexViewModel>>> GetClinicsAsync(string searchTerm, int pageNumber, int pageSize)
        {
            try
            {
                var clinics = await _clinicRepo.GetClinicsAsync(searchTerm);

                // نگاشت به ViewModel
                var clinicViewModels = clinics.Select(ClinicIndexViewModel.FromEntity).ToList();

                // صفحه‌بندی در حافظه
                var pagedResult = new PagedResult<ClinicIndexViewModel>(clinicViewModels, clinicViewModels.Count, pageNumber, pageSize);

                return ServiceResult<PagedResult<ClinicIndexViewModel>>.Successful(pagedResult);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در بازیابی لیست کلینیک‌ها با عبارت جستجوی: {SearchTerm}", searchTerm);
                return ServiceResult<PagedResult<ClinicIndexViewModel>>.Failed("خطای سیستمی در بازیابی اطلاعات رخ داد.", "DB_ERROR");
            }
        }

        public async Task<ServiceResult<ClinicDetailsViewModel>> GetClinicDetailsAsync(int clinicId)
        {
            try
            {
                var clinic = await _clinicRepo.GetByIdAsync(clinicId);
                if (clinic == null)
                    return ServiceResult<ClinicDetailsViewModel>.Failed("کلینیک مورد نظر یافت نشد.", "NOT_FOUND", ErrorCategory.NotFound);

                var viewModel = ClinicDetailsViewModel.FromEntity(clinic);
                return ServiceResult<ClinicDetailsViewModel>.Successful(viewModel);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در بازیابی جزئیات کلینیک با شناسه: {ClinicId}", clinicId);
                return ServiceResult<ClinicDetailsViewModel>.Failed("خطای سیستمی در بازیابی اطلاعات رخ داد.", "DB_ERROR");
            }
        }

        public async Task<ServiceResult<ClinicCreateEditViewModel>> GetClinicForEditAsync(int clinicId)
        {
            try
            {
                var clinic = await _clinicRepo.GetByIdAsync(clinicId);
                if (clinic == null)
                    return ServiceResult<ClinicCreateEditViewModel>.Failed("کلینیک مورد نظر یافت نشد.", "NOT_FOUND", ErrorCategory.NotFound);

                var viewModel = ClinicCreateEditViewModel.FromEntity(clinic);
                return ServiceResult<ClinicCreateEditViewModel>.Successful(viewModel);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در بازیابی اطلاعات کلینیک برای ویرایش: {ClinicId}", clinicId);
                return ServiceResult<ClinicCreateEditViewModel>.Failed("خطای سیستمی در بازیابی اطلاعات رخ داد.", "DB_ERROR");
            }
        }

        public async Task<ServiceResult> UpdateClinicAsync(ClinicCreateEditViewModel model)
        {
            _log.Information("درخواست بروزرسانی کلینیک با شناسه: {ClinicId}", model.ClinicId);

            var validationResult = await _validator.ValidateAsync(model);
            if (!validationResult.IsValid)
            {
                _log.Warning("اعتبارسنجی برای بروزرسانی کلینیک ناموفق بود: {@ValidationErrors}", validationResult.Errors);

                // ✅ **تغییر کلیدی و رفع خطا:**
                // تبدیل لیست خطاهای FluentValidation به لیست ValidationError خودمان
                var validationErrors = validationResult.Errors
                    .Select(err => new ValidationError(err.PropertyName, err.ErrorMessage));

                return ServiceResult.FailedWithValidationErrors(
                    "اطلاعات ورودی نامعتبر است.",
                    validationErrors
                );
            }

            try
            {
                var clinic = await _clinicRepo.GetByIdAsync(model.ClinicId);
                if (clinic == null)
                    return ServiceResult.Failed("کلینیک مورد نظر برای ویرایش یافت نشد.", "NOT_FOUND", ErrorCategory.NotFound);

                model.MapToEntity(clinic);
                _clinicRepo.Update(clinic);
                await _clinicRepo.SaveChangesAsync();

                _log.Information("کلینیک با شناسه {ClinicId} با موفقیت بروزرسانی شد.", clinic.ClinicId);
                return ServiceResult.Successful("کلینیک با موفقیت به‌روزرسانی شد.");
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطای سیستمی در هنگام بروزرسانی کلینک با شناسه {ClinicId}", model.ClinicId);
                return ServiceResult.Failed("خطای سیستمی در هنگام بروزرسانی کلینیک رخ داد.", "DB_ERROR", ErrorCategory.Database);
            }
        }

        public async Task<ServiceResult> SoftDeleteClinicAsync(int clinicId)
        {
            try
            {
                var clinic = await _clinicRepo.GetByIdAsync(clinicId);
                if (clinic == null)
                    return ServiceResult.Failed("کلینیک مورد نظر یافت نشد.", "NOT_FOUND", ErrorCategory.NotFound);

                // TODO: در آینده، قبل از حذف باید بررسی شود که آیا دپارتمان فعالی دارد یا خیر
                // if (clinic.Departments.Any(d => !d.IsDeleted))
                //    return ServiceResult.Failed("امکان حذف کلینیک دارای دپارتمان فعال وجود ندارد.", "BUSINESS_RULE_VIOLATION");

                _clinicRepo.Delete(clinic);
                await _clinicRepo.SaveChangesAsync();

                _log.Information("کلینیک با شناسه {ClinicId} با موفقیت حذف نرم شد.", clinicId);
                return ServiceResult.Successful("کلینیک با موفقیت حذف شد.");
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در حذف نرم کلینیک با شناسه: {ClinicId}", clinicId);
                return ServiceResult.Failed("خطای سیستمی در حذف کلینیک رخ داد.", "DB_ERROR");
            }
        }

        public async Task<ServiceResult> RestoreClinicAsync(int clinicId)
        {
            // پیاده‌سازی این متد مشابه SoftDelete است، با این تفاوت که IsDeleted = false می‌شود
            // این منطق در SaveChanges مربوط به DbContext شما مدیریت می‌شود
            throw new NotImplementedException();
        }

        public async Task<ServiceResult<List<LookupItemViewModel>>> GetActiveClinicsForLookupAsync()
        {
            try
            {
                var activeClinics = await _clinicRepo.GetActiveClinicsAsync();
                var lookupItems = activeClinics.Select(c => new LookupItemViewModel { Id = c.ClinicId, Name = c.Name }).ToList();
                return ServiceResult<List<LookupItemViewModel>>.Successful(lookupItems);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در بازیابی لیست کلینیک‌های فعال.");
                return ServiceResult<List<LookupItemViewModel>>.Failed("خطای سیستمی در بازیابی اطلاعات رخ داد.", "DB_ERROR");
            }
        }
    }
}