using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Core;
using ClinicApp.Extensions;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.Insurance;
using ClinicApp.Models;
using ClinicApp.Models.Entities;
using ClinicApp.Models.Entities.Insurance;
using ClinicApp.ViewModels.Insurance.InsuranceProvider;
using Serilog;

namespace ClinicApp.Services.Insurance
{
    /// <summary>
    /// سرویس مدیریت ارائه‌دهندگان بیمه - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. مدیریت کامل ارائه‌دهندگان بیمه (SSO, FREE, MILITARY, HEALTH, SUPPLEMENTARY)
    /// 2. استفاده از ServiceResult Enhanced pattern
    /// 3. پشتیبانی از FluentValidation
    /// 4. مدیریت کامل خطاها و لاگ‌گیری
    /// 5. پشتیبانی از صفحه‌بندی و جستجو
    /// 6. مدیریت Lookup Lists برای UI
    /// 7. رعایت استانداردهای پزشکی ایران
    /// 
    /// نکته حیاتی: این کلاس بر اساس استانداردهای سیستم‌های پزشکی ایران پیاده‌سازی شده است
    /// </summary>
    public class InsuranceProviderService : IInsuranceProviderService
    {
        private readonly IInsuranceProviderRepository _insuranceProviderRepository;
        private readonly ILogger _log;
        private readonly ICurrentUserService _currentUserService;

        public InsuranceProviderService(
            IInsuranceProviderRepository insuranceProviderRepository,
            ILogger logger,
            ICurrentUserService currentUserService)
        {
            _insuranceProviderRepository = insuranceProviderRepository ?? throw new ArgumentNullException(nameof(insuranceProviderRepository));
            _log = logger.ForContext<InsuranceProviderService>();
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        #region CRUD Operations

        /// <summary>
        /// دریافت لیست ارائه‌دهندگان بیمه با صفحه‌بندی و جستجو
        /// </summary>
        public async Task<ServiceResult<PagedResult<InsuranceProviderIndexViewModel>>> GetProvidersAsync(string searchTerm, int pageNumber, int pageSize)
        {
            _log.Information(
                "درخواست دریافت لیست ارائه‌دهندگان بیمه. عبارت جستجو: {SearchTerm}, شماره صفحه: {PageNumber}, اندازه صفحه: {PageSize}. کاربر: {UserName} (شناسه: {UserId})",
                searchTerm, pageNumber, pageSize, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                // اعتبارسنجی ورودی‌ها
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1) pageSize = 10;
                if (pageSize > 100) pageSize = 100;

                // پاک‌سازی و نرمال‌سازی عبارت جستجو
                searchTerm = string.IsNullOrWhiteSpace(searchTerm) ? "" : searchTerm.Trim();

                // دریافت داده‌ها از Repository
                List<InsuranceProvider> providers;
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    providers = await _insuranceProviderRepository.GetAllAsync();
                }
                else
                {
                    providers = await _insuranceProviderRepository.SearchAsync(searchTerm);
                }

                // تبدیل به ViewModel
                var items = providers.Select(ConvertToIndexViewModel).ToList();

                // محاسبه صفحه‌بندی
                var totalItems = items.Count;
                var pagedItems = items
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var pagedResult = new PagedResult<InsuranceProviderIndexViewModel>
                {
                    Items = pagedItems,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalItems = totalItems
                };

                _log.Information(
                    "دریافت لیست ارائه‌دهندگان بیمه با موفقیت انجام شد. تعداد نتایج: {Count}, صفحه: {Page}. کاربر: {UserName} (شناسه: {UserId})",
                    pagedResult.TotalItems, pageNumber, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<PagedResult<InsuranceProviderIndexViewModel>>.Successful(
                    pagedResult,
                    "دریافت لیست ارائه‌دهندگان بیمه با موفقیت انجام شد.",
 "GetProviders",
 _currentUserService.UserId,
 _currentUserService.UserName,
                    securityLevel: SecurityLevel.Low);
            }
            catch (Exception ex)
            {
                _log.Error(
                    ex,
                    "خطای سیستمی در دریافت لیست ارائه‌دهندگان بیمه. عبارت جستجو: {SearchTerm}, شماره صفحه: {PageNumber}. کاربر: {UserName} (شناسه: {UserId})",
                    searchTerm, pageNumber, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<PagedResult<InsuranceProviderIndexViewModel>>.Failed(
                    "خطا در دریافت لیست ارائه‌دهندگان بیمه. لطفاً دوباره تلاش کنید.",
                    "GET_PROVIDERS_ERROR",
                    ErrorCategory.General,
                    SecurityLevel.High);
            }
        }

        /// <summary>
        /// دریافت جزئیات ارائه‌دهنده بیمه
        /// </summary>
        public async Task<ServiceResult<InsuranceProviderDetailsViewModel>> GetProviderDetailsAsync(int providerId)
        {
            _log.Information(
                "درخواست جزئیات ارائه‌دهنده بیمه با شناسه {ProviderId}. کاربر: {UserName} (شناسه: {UserId})",
                providerId, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                var provider = await _insuranceProviderRepository.GetByIdWithDetailsAsync(providerId);
                if (provider == null)
                {
                    _log.Warning(
                        "ارائه‌دهنده بیمه با شناسه {ProviderId} یافت نشد. کاربر: {UserName} (شناسه: {UserId})",
                        providerId, _currentUserService.UserName, _currentUserService.UserId);

                    return ServiceResult<InsuranceProviderDetailsViewModel>.Failed(
                        "ارائه‌دهنده بیمه مورد نظر یافت نشد.",
                        "PROVIDER_NOT_FOUND",
                        ErrorCategory.NotFound,
                        SecurityLevel.Medium);
                }

                var viewModel = ConvertToDetailsViewModel(provider);

                _log.Information(
                    "جزئیات ارائه‌دهنده بیمه با موفقیت دریافت شد. ProviderId: {ProviderId}, Name: {Name}. کاربر: {UserName} (شناسه: {UserId})",
                    providerId, provider.Name, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<InsuranceProviderDetailsViewModel>.Successful(
                    viewModel,
                    "جزئیات ارائه‌دهنده بیمه با موفقیت دریافت شد.",
 "GetProviderDetails",
 _currentUserService.UserId,
 _currentUserService.UserName,
                    securityLevel: SecurityLevel.Low);
            }
            catch (Exception ex)
            {
                _log.Error(
                    ex,
                    "خطای سیستمی در دریافت جزئیات ارائه‌دهنده بیمه. ProviderId: {ProviderId}. کاربر: {UserName} (شناسه: {UserId})",
                    providerId, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<InsuranceProviderDetailsViewModel>.Failed(
                    "خطا در دریافت جزئیات ارائه‌دهنده بیمه. لطفاً دوباره تلاش کنید.",
                    "GET_PROVIDER_DETAILS_ERROR",
                    ErrorCategory.General,
                    SecurityLevel.High);
            }
        }

        /// <summary>
        /// دریافت ارائه‌دهنده بیمه برای ویرایش
        /// </summary>
        public async Task<ServiceResult<InsuranceProviderCreateEditViewModel>> GetProviderForEditAsync(int providerId)
        {
            _log.Information(
                "درخواست ارائه‌دهنده بیمه برای ویرایش با شناسه {ProviderId}. کاربر: {UserName} (شناسه: {UserId})",
                providerId, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                var provider = await _insuranceProviderRepository.GetByIdAsync(providerId);
                if (provider == null)
                {
                    _log.Warning(
                        "ارائه‌دهنده بیمه با شناسه {ProviderId} یافت نشد. کاربر: {UserName} (شناسه: {UserId})",
                        providerId, _currentUserService.UserName, _currentUserService.UserId);

                    return ServiceResult<InsuranceProviderCreateEditViewModel>.Failed(
                        "ارائه‌دهنده بیمه مورد نظر یافت نشد.",
                        "PROVIDER_NOT_FOUND",
                        ErrorCategory.NotFound,
                        SecurityLevel.Medium);
                }

                var viewModel = ConvertToCreateEditViewModel(provider);

                _log.Information(
                    "ارائه‌دهنده بیمه برای ویرایش با موفقیت دریافت شد. ProviderId: {ProviderId}, Name: {Name}. کاربر: {UserName} (شناسه: {UserId})",
                    providerId, provider.Name, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<InsuranceProviderCreateEditViewModel>.Successful(
                    viewModel,
                    "ارائه‌دهنده بیمه برای ویرایش با موفقیت دریافت شد.",
 "GetProviderForEdit",
 _currentUserService.UserId,
 _currentUserService.UserName,
                    securityLevel: SecurityLevel.Low);
            }
            catch (Exception ex)
            {
                _log.Error(
                    ex,
                    "خطای سیستمی در دریافت ارائه‌دهنده بیمه برای ویرایش. ProviderId: {ProviderId}. کاربر: {UserName} (شناسه: {UserId})",
                    providerId, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<InsuranceProviderCreateEditViewModel>.Failed(
                    "خطا در دریافت ارائه‌دهنده بیمه برای ویرایش. لطفاً دوباره تلاش کنید.",
                    "GET_PROVIDER_FOR_EDIT_ERROR",
                    ErrorCategory.General,
                    SecurityLevel.High);
            }
        }

        /// <summary>
        /// ایجاد ارائه‌دهنده بیمه جدید
        /// </summary>
        public async Task<ServiceResult<int>> CreateProviderAsync(InsuranceProviderCreateEditViewModel model)
        {
            _log.Information(
                "درخواست ایجاد ارائه‌دهنده بیمه جدید. Name: {Name}, Code: {Code}. کاربر: {UserName} (شناسه: {UserId})",
                model?.Name, model?.Code, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                if (model == null)
                {
                    return ServiceResult<int>.Failed("اطلاعات ارائه‌دهنده بیمه ارسال نشده است.");
                }

                // اعتبارسنجی کد
                var codeExists = await _insuranceProviderRepository.DoesCodeExistAsync(model.Code);
                if (codeExists)
                {
                    return ServiceResult<int>.Failed("کد ارائه‌دهنده بیمه تکراری است.");
                }

                // اعتبارسنجی نام
                var nameExists = await _insuranceProviderRepository.DoesNameExistAsync(model.Name);
                if (nameExists)
                {
                    return ServiceResult<int>.Failed("نام ارائه‌دهنده بیمه تکراری است.");
                }

                // تبدیل به Entity
                var provider = ConvertToEntity(model);
                provider.IsActive = true;
                provider.IsDeleted = false;

                // ذخیره در Repository
                _insuranceProviderRepository.Add(provider);
                await _insuranceProviderRepository.SaveChangesAsync();

                _log.Information(
                    "ارائه‌دهنده بیمه جدید با موفقیت ایجاد شد. ProviderId: {ProviderId}, Name: {Name}, Code: {Code}. کاربر: {UserName} (شناسه: {UserId})",
                    provider.InsuranceProviderId, provider.Name, provider.Code, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<int>.Successful(
                    provider.InsuranceProviderId,
                    "ارائه‌دهنده بیمه جدید با موفقیت ایجاد شد.",
 "CreateProvider",
 _currentUserService.UserId,
 _currentUserService.UserName,
                    securityLevel: SecurityLevel.Medium);
            }
            catch (Exception ex)
            {
                _log.Error(
                    ex,
                    "خطای سیستمی در ایجاد ارائه‌دهنده بیمه جدید. Name: {Name}, Code: {Code}. کاربر: {UserName} (شناسه: {UserId})",
                    model?.Name, model?.Code, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<int>.Failed(
                    "خطا در ایجاد ارائه‌دهنده بیمه جدید. لطفاً دوباره تلاش کنید.",
                    "CREATE_PROVIDER_ERROR",
                    ErrorCategory.General,
                    SecurityLevel.High);
            }
        }

        /// <summary>
        /// به‌روزرسانی ارائه‌دهنده بیمه
        /// </summary>
        public async Task<ServiceResult> UpdateProviderAsync(InsuranceProviderCreateEditViewModel model)
        {
            _log.Information(
                "درخواست به‌روزرسانی ارائه‌دهنده بیمه. ProviderId: {ProviderId}, Name: {Name}, Code: {Code}. کاربر: {UserName} (شناسه: {UserId})",
                model?.InsuranceProviderId, model?.Name, model?.Code, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                if (model == null)
                {
                    return ServiceResult.Failed("اطلاعات ارائه‌دهنده بیمه ارسال نشده است.");
                }

                // بررسی وجود ارائه‌دهنده بیمه
                var existingProvider = await _insuranceProviderRepository.GetByIdAsync(model.InsuranceProviderId);
                if (existingProvider == null)
                {
                    return ServiceResult.Failed("ارائه‌دهنده بیمه مورد نظر یافت نشد.");
                }

                // اعتبارسنجی کد
                var codeExists = await _insuranceProviderRepository.DoesCodeExistAsync(model.Code, model.InsuranceProviderId);
                if (codeExists)
                {
                    return ServiceResult.Failed("کد ارائه‌دهنده بیمه تکراری است.");
                }

                // اعتبارسنجی نام
                var nameExists = await _insuranceProviderRepository.DoesNameExistAsync(model.Name, model.InsuranceProviderId);
                if (nameExists)
                {
                    return ServiceResult.Failed("نام ارائه‌دهنده بیمه تکراری است.");
                }

                // به‌روزرسانی Entity
                existingProvider.Name = model.Name;
                existingProvider.Code = model.Code;
                existingProvider.ContactInfo = model.ContactInfo;
                existingProvider.IsActive = model.IsActive;

                // ذخیره در Repository
                _insuranceProviderRepository.Update(existingProvider);
                await _insuranceProviderRepository.SaveChangesAsync();

                _log.Information(
                    "ارائه‌دهنده بیمه با موفقیت به‌روزرسانی شد. ProviderId: {ProviderId}, Name: {Name}, Code: {Code}. کاربر: {UserName} (شناسه: {UserId})",
                    existingProvider.InsuranceProviderId, existingProvider.Name, existingProvider.Code, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult.Successful("ارائه‌دهنده بیمه با موفقیت به‌روزرسانی شد.");
            }
            catch (Exception ex)
            {
                _log.Error(
                    ex,
                    "خطای سیستمی در به‌روزرسانی ارائه‌دهنده بیمه. ProviderId: {ProviderId}, Name: {Name}, Code: {Code}. کاربر: {UserName} (شناسه: {UserId})",
                    model?.InsuranceProviderId, model?.Name, model?.Code, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult.Failed("خطا در به‌روزرسانی ارائه‌دهنده بیمه. لطفاً دوباره تلاش کنید.");
            }
        }

        /// <summary>
        /// حذف نرم ارائه‌دهنده بیمه
        /// </summary>
        public async Task<ServiceResult> SoftDeleteProviderAsync(int providerId)
        {
            _log.Information(
                "درخواست حذف ارائه‌دهنده بیمه با شناسه {ProviderId}. کاربر: {UserName} (شناسه: {UserId})",
                providerId, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                var provider = await _insuranceProviderRepository.GetByIdAsync(providerId);
                if (provider == null)
                {
                    _log.Warning(
                        "ارائه‌دهنده بیمه با شناسه {ProviderId} یافت نشد. کاربر: {UserName} (شناسه: {UserId})",
                        providerId, _currentUserService.UserName, _currentUserService.UserId);

                    return ServiceResult.Failed("ارائه‌دهنده بیمه مورد نظر یافت نشد.");
                }

                // حذف نرم
                _insuranceProviderRepository.Delete(provider);
                await _insuranceProviderRepository.SaveChangesAsync();

                _log.Information(
                    "ارائه‌دهنده بیمه با موفقیت حذف شد. ProviderId: {ProviderId}, Name: {Name}. کاربر: {UserName} (شناسه: {UserId})",
                    providerId, provider.Name, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult.Successful("ارائه‌دهنده بیمه با موفقیت حذف شد.");
            }
            catch (Exception ex)
            {
                _log.Error(
                    ex,
                    "خطای سیستمی در حذف ارائه‌دهنده بیمه. ProviderId: {ProviderId}. کاربر: {UserName} (شناسه: {UserId})",
                    providerId, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult.Failed("خطا در حذف ارائه‌دهنده بیمه. لطفاً دوباره تلاش کنید.");
            }
        }

        #endregion

        #region Lookup Operations

        /// <summary>
        /// دریافت ارائه‌دهندگان بیمه فعال برای Lookup
        /// </summary>
        public async Task<ServiceResult<List<InsuranceProviderLookupViewModel>>> GetActiveProvidersForLookupAsync()
        {
            _log.Information(
                "درخواست دریافت ارائه‌دهندگان بیمه فعال برای Lookup. کاربر: {UserName} (شناسه: {UserId})",
                _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                var providers = await _insuranceProviderRepository.GetActiveAsync();
                var lookupItems = providers.Select(ConvertToLookupViewModel).ToList();

                _log.Information(
                    "ارائه‌دهندگان بیمه فعال برای Lookup با موفقیت دریافت شدند. تعداد: {Count}. کاربر: {UserName} (شناسه: {UserId})",
                    lookupItems.Count, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<List<InsuranceProviderLookupViewModel>>.Successful(
                    lookupItems,
                    "ارائه‌دهندگان بیمه فعال با موفقیت دریافت شدند.",
 "GetActiveProvidersForLookup",
 _currentUserService.UserId,
 _currentUserService.UserName,
                    securityLevel: SecurityLevel.Low);
            }
            catch (Exception ex)
            {
                _log.Error(
                    ex,
                    "خطای سیستمی در دریافت ارائه‌دهندگان بیمه فعال برای Lookup. کاربر: {UserName} (شناسه: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<List<InsuranceProviderLookupViewModel>>.Failed(
                    "خطا در دریافت ارائه‌دهندگان بیمه فعال. لطفاً دوباره تلاش کنید.",
                    "GET_ACTIVE_PROVIDERS_ERROR",
                    ErrorCategory.General,
                    SecurityLevel.High);
            }
        }

        #endregion

        #region Validation Operations

        /// <summary>
        /// بررسی وجود کد ارائه‌دهنده بیمه
        /// </summary>
        public async Task<ServiceResult<bool>> DoesCodeExistAsync(string code, int? excludeId = null)
        {
            try
            {
                var exists = await _insuranceProviderRepository.DoesCodeExistAsync(code, excludeId);
                return ServiceResult<bool>.Successful(
                    exists,
                    "بررسی وجود کد ارائه‌دهنده بیمه انجام شد.",
 "DoesCodeExist",
 _currentUserService.UserId,
 _currentUserService.UserName,
                    securityLevel: SecurityLevel.Low);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در بررسی وجود کد ارائه‌دهنده بیمه. Code: {Code}", code);
                return ServiceResult<bool>.Failed(
                    "خطا در بررسی وجود کد ارائه‌دهنده بیمه.",
                    "CHECK_CODE_EXISTS_ERROR",
                    ErrorCategory.General,
                    SecurityLevel.High);
            }
        }

        /// <summary>
        /// بررسی وجود نام ارائه‌دهنده بیمه
        /// </summary>
        public async Task<ServiceResult<bool>> DoesNameExistAsync(string name, int? excludeId = null)
        {
            try
            {
                var exists = await _insuranceProviderRepository.DoesNameExistAsync(name, excludeId);
                return ServiceResult<bool>.Successful(
                    exists,
                    "بررسی وجود نام ارائه‌دهنده بیمه انجام شد.",
 "DoesNameExist",
 _currentUserService.UserId,
 _currentUserService.UserName,
                    securityLevel: SecurityLevel.Low);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در بررسی وجود نام ارائه‌دهنده بیمه. Name: {Name}", name);
                return ServiceResult<bool>.Failed(
                    "خطا در بررسی وجود نام ارائه‌دهنده بیمه.",
                    "CHECK_NAME_EXISTS_ERROR",
                    ErrorCategory.General,
                    SecurityLevel.High);
            }
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// تبدیل Entity به Index ViewModel
        /// </summary>
        private InsuranceProviderIndexViewModel ConvertToIndexViewModel(InsuranceProvider provider)
        {
            if (provider == null) return null;

            return new InsuranceProviderIndexViewModel
            {
                InsuranceProviderId = provider.InsuranceProviderId,
                Name = provider.Name,
                Code = provider.Code,
                ContactInfo = provider.ContactInfo,
                IsActive = provider.IsActive,
                CreatedAt = provider.CreatedAt,
                CreatedAtShamsi = provider.CreatedAt.ToPersianDateTime(),
                CreatedByUserName = provider.CreatedByUser != null ? $"{provider.CreatedByUser.FirstName} {provider.CreatedByUser.LastName}".Trim() : null
            };
        }

        /// <summary>
        /// تبدیل Entity به Details ViewModel
        /// </summary>
        private InsuranceProviderDetailsViewModel ConvertToDetailsViewModel(InsuranceProvider provider)
        {
            if (provider == null) return null;

            return new InsuranceProviderDetailsViewModel
            {
                InsuranceProviderId = provider.InsuranceProviderId,
                Name = provider.Name,
                Code = provider.Code,
                ContactInfo = provider.ContactInfo,
                IsActive = provider.IsActive,
                CreatedAt = provider.CreatedAt,
                UpdatedAt = provider.UpdatedAt,
                CreatedAtShamsi = provider.CreatedAt.ToPersianDateTime(),
                UpdatedAtShamsi = provider.UpdatedAt.HasValue ? provider.UpdatedAt.Value.ToPersianDateTime() : null,
                CreatedByUserName = provider.CreatedByUser != null ? $"{provider.CreatedByUser.FirstName} {provider.CreatedByUser.LastName}".Trim() : null,
                UpdatedByUserName = provider.UpdatedByUser != null ? $"{provider.UpdatedByUser.FirstName} {provider.UpdatedByUser.LastName}".Trim() : null,
                InsurancePlanCount = provider.InsurancePlans?.Count(ip => !ip.IsDeleted) ?? 0
            };
        }

        /// <summary>
        /// تبدیل Entity به CreateEdit ViewModel
        /// </summary>
        private InsuranceProviderCreateEditViewModel ConvertToCreateEditViewModel(InsuranceProvider provider)
        {
            if (provider == null) return null;

            return new InsuranceProviderCreateEditViewModel
            {
                InsuranceProviderId = provider.InsuranceProviderId,
                Name = provider.Name,
                Code = provider.Code,
                ContactInfo = provider.ContactInfo,
                IsActive = provider.IsActive
            };
        }

        /// <summary>
        /// تبدیل CreateEdit ViewModel به Entity
        /// </summary>
        private InsuranceProvider ConvertToEntity(InsuranceProviderCreateEditViewModel model)
        {
            if (model == null) return null;

            return new InsuranceProvider
            {
                InsuranceProviderId = model.InsuranceProviderId,
                Name = model.Name,
                Code = model.Code,
                ContactInfo = model.ContactInfo,
                IsActive = model.IsActive
            };
        }

        /// <summary>
        /// تبدیل Entity به Lookup ViewModel
        /// </summary>
        private InsuranceProviderLookupViewModel ConvertToLookupViewModel(InsuranceProvider provider)
        {
            if (provider == null) return null;

            return new InsuranceProviderLookupViewModel
            {
                InsuranceProviderId = provider.InsuranceProviderId,
                Name = provider.Name,
                Code = provider.Code
            };
        }

        #endregion
    }
}
