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
using ClinicApp.ViewModels.Insurance.InsurancePlan;
using ClinicApp.ViewModels.Insurance.InsuranceProvider;
using Serilog;

namespace ClinicApp.Services.Insurance
{
    /// <summary>
    /// سرویس مدیریت طرح‌های بیمه - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. مدیریت کامل طرح‌های بیمه (Basic, Standard, Premium, Supplementary)
    /// 2. استفاده از ServiceResult Enhanced pattern
    /// 3. پشتیبانی از FluentValidation
    /// 4. مدیریت کامل خطاها و لاگ‌گیری
    /// 5. پشتیبانی از صفحه‌بندی و جستجو
    /// 6. مدیریت Lookup Lists برای UI
    /// 7. مدیریت روابط با InsuranceProvider
    /// 8. رعایت استانداردهای پزشکی ایران
    /// 
    /// نکته حیاتی: این کلاس بر اساس استانداردهای سیستم‌های پزشکی ایران پیاده‌سازی شده است
    /// </summary>
    public class InsurancePlanService : IInsurancePlanService
    {
        private readonly IInsurancePlanRepository _insurancePlanRepository;
        private readonly IInsuranceProviderRepository _insuranceProviderRepository;
        private readonly ILogger _log;
        private readonly ICurrentUserService _currentUserService;

        public InsurancePlanService(
            IInsurancePlanRepository insurancePlanRepository,
            IInsuranceProviderRepository insuranceProviderRepository,
            ILogger logger,
            ICurrentUserService currentUserService)
        {
            _insurancePlanRepository = insurancePlanRepository ?? throw new ArgumentNullException(nameof(insurancePlanRepository));
            _insuranceProviderRepository = insuranceProviderRepository ?? throw new ArgumentNullException(nameof(insuranceProviderRepository));
            _log = logger.ForContext<InsurancePlanService>();
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        #region CRUD Operations

        /// <summary>
        /// دریافت لیست طرح‌های بیمه با صفحه‌بندی و جستجو
        /// </summary>
        public async Task<ServiceResult<PagedResult<InsurancePlanIndexViewModel>>> GetPlansAsync(int? providerId, string searchTerm, int pageNumber, int pageSize)
        {
            _log.Information(
                "درخواست دریافت لیست طرح‌های بیمه. ProviderId: {ProviderId}, عبارت جستجو: {SearchTerm}, شماره صفحه: {PageNumber}, اندازه صفحه: {PageSize}. کاربر: {UserName} (شناسه: {UserId})",
                providerId, searchTerm, pageNumber, pageSize, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                // اعتبارسنجی ورودی‌ها
                if (pageNumber < 1) pageNumber = 1;
                if (pageSize < 1) pageSize = 10;
                if (pageSize > 100) pageSize = 100;

                // پاک‌سازی و نرمال‌سازی عبارت جستجو
                searchTerm = string.IsNullOrWhiteSpace(searchTerm) ? "" : searchTerm.Trim();

                // دریافت داده‌ها از Repository
                List<InsurancePlan> plans;
                if (providerId.HasValue)
                {
                    if (string.IsNullOrWhiteSpace(searchTerm))
                    {
                        plans = await _insurancePlanRepository.GetByProviderIdAsync(providerId.Value);
                    }
                    else
                    {
                        plans = await _insurancePlanRepository.SearchByProviderAsync(providerId.Value, searchTerm);
                    }
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(searchTerm))
                    {
                        plans = await _insurancePlanRepository.GetAllAsync();
                    }
                    else
                    {
                        plans = await _insurancePlanRepository.SearchAsync(searchTerm);
                    }
                }

                // تبدیل به ViewModel
                var items = plans.Select(ConvertToIndexViewModel).ToList();

                // محاسبه صفحه‌بندی
                var totalItems = items.Count;
                var pagedItems = items
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var pagedResult = new PagedResult<InsurancePlanIndexViewModel>
                {
                    Items = pagedItems,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalItems = totalItems
                };

                _log.Information(
                    "دریافت لیست طرح‌های بیمه با موفقیت انجام شد. تعداد نتایج: {Count}, صفحه: {Page}. کاربر: {UserName} (شناسه: {UserId})",
                    pagedResult.TotalItems, pageNumber, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<PagedResult<InsurancePlanIndexViewModel>>.Successful(
                    pagedResult,
                    "دریافت لیست طرح‌های بیمه با موفقیت انجام شد.",
 "GetPlans",
 _currentUserService.UserId,
 _currentUserService.UserName,
                    securityLevel: SecurityLevel.Low);
            }
            catch (Exception ex)
            {
                _log.Error(
                    ex,
                    "خطای سیستمی در دریافت لیست طرح‌های بیمه. ProviderId: {ProviderId}, عبارت جستجو: {SearchTerm}, شماره صفحه: {PageNumber}. کاربر: {UserName} (شناسه: {UserId})",
                    providerId, searchTerm, pageNumber, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<PagedResult<InsurancePlanIndexViewModel>>.Failed(
                    "خطا در دریافت لیست طرح‌های بیمه. لطفاً دوباره تلاش کنید.",
                    "GET_PLANS_ERROR",
                    ErrorCategory.General,
                    SecurityLevel.High);
            }
        }

        /// <summary>
        /// دریافت جزئیات طرح بیمه
        /// </summary>
        public async Task<ServiceResult<InsurancePlanDetailsViewModel>> GetPlanDetailsAsync(int planId)
        {
            _log.Information(
                "درخواست جزئیات طرح بیمه با شناسه {PlanId}. کاربر: {UserName} (شناسه: {UserId})",
                planId, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                var plan = await _insurancePlanRepository.GetByIdWithDetailsAsync(planId);
                if (plan == null)
                {
                    _log.Warning(
                        "طرح بیمه با شناسه {PlanId} یافت نشد. کاربر: {UserName} (شناسه: {UserId})",
                        planId, _currentUserService.UserName, _currentUserService.UserId);

                    return ServiceResult<InsurancePlanDetailsViewModel>.Failed(
                        "طرح بیمه مورد نظر یافت نشد.",
                        "PLAN_NOT_FOUND",
                        ErrorCategory.NotFound,
                        SecurityLevel.Medium);
                }

                var viewModel = ConvertToDetailsViewModel(plan);

                _log.Information(
                    "جزئیات طرح بیمه با موفقیت دریافت شد. PlanId: {PlanId}, Name: {Name}. کاربر: {UserName} (شناسه: {UserId})",
                    planId, plan.Name, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<InsurancePlanDetailsViewModel>.Successful(
                    viewModel,
                    "جزئیات طرح بیمه با موفقیت دریافت شد.",
 "GetPlanDetails",
 _currentUserService.UserId,
 _currentUserService.UserName,
                    securityLevel: SecurityLevel.Low);
            }
            catch (Exception ex)
            {
                _log.Error(
                    ex,
                    "خطای سیستمی در دریافت جزئیات طرح بیمه. PlanId: {PlanId}. کاربر: {UserName} (شناسه: {UserId})",
                    planId, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<InsurancePlanDetailsViewModel>.Failed(
                    "خطا در دریافت جزئیات طرح بیمه. لطفاً دوباره تلاش کنید.",
                    "GET_PLAN_DETAILS_ERROR",
                    ErrorCategory.General,
                    SecurityLevel.High);
            }
        }

        /// <summary>
        /// دریافت طرح بیمه برای ویرایش
        /// </summary>
        public async Task<ServiceResult<InsurancePlanCreateEditViewModel>> GetPlanForEditAsync(int planId)
        {
            _log.Information(
                "درخواست طرح بیمه برای ویرایش با شناسه {PlanId}. کاربر: {UserName} (شناسه: {UserId})",
                planId, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                var plan = await _insurancePlanRepository.GetByIdAsync(planId);
                if (plan == null)
                {
                    _log.Warning(
                        "طرح بیمه با شناسه {PlanId} یافت نشد. کاربر: {UserName} (شناسه: {UserId})",
                        planId, _currentUserService.UserName, _currentUserService.UserId);

                    return ServiceResult<InsurancePlanCreateEditViewModel>.Failed(
                        "طرح بیمه مورد نظر یافت نشد.",
                        "PLAN_NOT_FOUND",
                        ErrorCategory.NotFound,
                        SecurityLevel.Medium);
                }

                var viewModel = ConvertToCreateEditViewModel(plan);

                _log.Information(
                    "طرح بیمه برای ویرایش با موفقیت دریافت شد. PlanId: {PlanId}, Name: {Name}. کاربر: {UserName} (شناسه: {UserId})",
                    planId, plan.Name, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<InsurancePlanCreateEditViewModel>.Successful(
                    viewModel,
                    "طرح بیمه برای ویرایش با موفقیت دریافت شد.",
 "GetPlanForEdit",
 _currentUserService.UserId,
 _currentUserService.UserName,
                    securityLevel: SecurityLevel.Low);
            }
            catch (Exception ex)
            {
                _log.Error(
                    ex,
                    "خطای سیستمی در دریافت طرح بیمه برای ویرایش. PlanId: {PlanId}. کاربر: {UserName} (شناسه: {UserId})",
                    planId, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<InsurancePlanCreateEditViewModel>.Failed(
                    "خطا در دریافت طرح بیمه برای ویرایش. لطفاً دوباره تلاش کنید.",
                    "GET_PLAN_FOR_EDIT_ERROR",
                    ErrorCategory.General,
                    SecurityLevel.High);
            }
        }

        /// <summary>
        /// ایجاد طرح بیمه جدید
        /// </summary>
        public async Task<ServiceResult<int>> CreatePlanAsync(InsurancePlanCreateEditViewModel model)
        {
            _log.Information(
                "درخواست ایجاد طرح بیمه جدید. Name: {Name}, PlanCode: {PlanCode}, ProviderId: {ProviderId}. کاربر: {UserName} (شناسه: {UserId})",
                model?.Name, model?.PlanCode, model?.InsuranceProviderId, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                if (model == null)
                {
                    return ServiceResult<int>.Failed(
                        "اطلاعات طرح بیمه ارسال نشده است.",
                        "INVALID_MODEL",
                        ErrorCategory.Validation,
                        SecurityLevel.Medium);
                }

                // اعتبارسنجی ارائه‌دهنده بیمه
                var providerExists = await _insuranceProviderRepository.DoesExistAsync(model.InsuranceProviderId);
                if (!providerExists)
                {
                    return ServiceResult<int>.Failed(
                        "ارائه‌دهنده بیمه مورد نظر یافت نشد.",
                        "PROVIDER_NOT_FOUND",
                        ErrorCategory.Validation,
                        SecurityLevel.Medium);
                }

                // اعتبارسنجی کد طرح
                var codeExists = await _insurancePlanRepository.DoesPlanCodeExistAsync(model.PlanCode);
                if (codeExists)
                {
                    return ServiceResult<int>.Failed(
                        "کد طرح بیمه تکراری است.",
                        "DUPLICATE_PLAN_CODE",
                        ErrorCategory.Validation,
                        SecurityLevel.Medium);
                }

                // اعتبارسنجی نام طرح در ارائه‌دهنده
                var nameExists = await _insurancePlanRepository.DoesNameExistInProviderAsync(model.Name, model.InsuranceProviderId);
                if (nameExists)
                {
                    return ServiceResult<int>.Failed(
                        "نام طرح بیمه در این ارائه‌دهنده تکراری است.",
                        "DUPLICATE_PLAN_NAME",
                        ErrorCategory.Validation,
                        SecurityLevel.Medium);
                }

                // تبدیل به Entity
                var plan = ConvertToEntity(model);
                plan.IsActive = true;
                plan.IsDeleted = false;

                // ذخیره در Repository
                _insurancePlanRepository.Add(plan);
                await _insurancePlanRepository.SaveChangesAsync();

                _log.Information(
                    "طرح بیمه جدید با موفقیت ایجاد شد. PlanId: {PlanId}, Name: {Name}, PlanCode: {PlanCode}. کاربر: {UserName} (شناسه: {UserId})",
                    plan.InsurancePlanId, plan.Name, plan.PlanCode, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<int>.Successful(
                    plan.InsurancePlanId,
                    "طرح بیمه جدید با موفقیت ایجاد شد.",
 "CreatePlan",
 _currentUserService.UserId,
 _currentUserService.UserName,
                    securityLevel: SecurityLevel.Medium);
            }
            catch (Exception ex)
            {
                _log.Error(
                    ex,
                    "خطای سیستمی در ایجاد طرح بیمه جدید. Name: {Name}, PlanCode: {PlanCode}, ProviderId: {ProviderId}. کاربر: {UserName} (شناسه: {UserId})",
                    model?.Name, model?.PlanCode, model?.InsuranceProviderId, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<int>.Failed(
                    "خطا در ایجاد طرح بیمه جدید. لطفاً دوباره تلاش کنید.",
                    "CREATE_PLAN_ERROR",
                    ErrorCategory.General,
                    SecurityLevel.High);
            }
        }

        /// <summary>
        /// به‌روزرسانی طرح بیمه
        /// </summary>
        public async Task<ServiceResult> UpdatePlanAsync(InsurancePlanCreateEditViewModel model)
        {
            _log.Information(
                "درخواست به‌روزرسانی طرح بیمه. PlanId: {PlanId}, Name: {Name}, PlanCode: {PlanCode}. کاربر: {UserName} (شناسه: {UserId})",
                model?.InsurancePlanId, model?.Name, model?.PlanCode, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                if (model == null)
                {
                    return ServiceResult.Failed(
                        "اطلاعات طرح بیمه ارسال نشده است.",
                        "INVALID_MODEL",
                        ErrorCategory.Validation,
                        SecurityLevel.Medium);
                }

                // بررسی وجود طرح بیمه
                var existingPlan = await _insurancePlanRepository.GetByIdAsync(model.InsurancePlanId);
                if (existingPlan == null)
                {
                    return ServiceResult.Failed(
                        "طرح بیمه مورد نظر یافت نشد.",
                        "PLAN_NOT_FOUND",
                        ErrorCategory.NotFound,
                        SecurityLevel.Medium);
                }

                // اعتبارسنجی ارائه‌دهنده بیمه
                var providerExists = await _insuranceProviderRepository.DoesExistAsync(model.InsuranceProviderId);
                if (!providerExists)
                {
                    return ServiceResult.Failed(
                        "ارائه‌دهنده بیمه مورد نظر یافت نشد.",
                        "PROVIDER_NOT_FOUND",
                        ErrorCategory.Validation,
                        SecurityLevel.Medium);
                }

                // اعتبارسنجی کد طرح
                var codeExists = await _insurancePlanRepository.DoesPlanCodeExistAsync(model.PlanCode, model.InsurancePlanId);
                if (codeExists)
                {
                    return ServiceResult.Failed(
                        "کد طرح بیمه تکراری است.",
                        "DUPLICATE_PLAN_CODE",
                        ErrorCategory.Validation,
                        SecurityLevel.Medium);
                }

                // اعتبارسنجی نام طرح در ارائه‌دهنده
                var nameExists = await _insurancePlanRepository.DoesNameExistInProviderAsync(model.Name, model.InsuranceProviderId, model.InsurancePlanId);
                if (nameExists)
                {
                    return ServiceResult.Failed(
                        "نام طرح بیمه در این ارائه‌دهنده تکراری است.",
                        "DUPLICATE_PLAN_NAME",
                        ErrorCategory.Validation,
                        SecurityLevel.Medium);
                }

                // به‌روزرسانی Entity
                existingPlan.InsuranceProviderId = model.InsuranceProviderId;
                existingPlan.Name = model.Name;
                existingPlan.PlanCode = model.PlanCode;
                existingPlan.CoveragePercent = model.CoveragePercent;
                existingPlan.Deductible = model.Deductible;
                existingPlan.ValidFrom = model.ValidFrom;
                existingPlan.ValidTo = model.ValidTo ?? DateTime.Now.AddYears(1);
                existingPlan.IsActive = model.IsActive;

                // ذخیره در Repository
                _insurancePlanRepository.Update(existingPlan);
                await _insurancePlanRepository.SaveChangesAsync();

                _log.Information(
                    "طرح بیمه با موفقیت به‌روزرسانی شد. PlanId: {PlanId}, Name: {Name}, PlanCode: {PlanCode}. کاربر: {UserName} (شناسه: {UserId})",
                    existingPlan.InsurancePlanId, existingPlan.Name, existingPlan.PlanCode, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult.Successful(
                    "طرح بیمه با موفقیت به‌روزرسانی شد.",
 "UpdatePlan",
 _currentUserService.UserId,
 _currentUserService.UserName,
                    securityLevel: SecurityLevel.Medium);
            }
            catch (Exception ex)
            {
                _log.Error(
                    ex,
                    "خطای سیستمی در به‌روزرسانی طرح بیمه. PlanId: {PlanId}, Name: {Name}, PlanCode: {PlanCode}. کاربر: {UserName} (شناسه: {UserId})",
                    model?.InsurancePlanId, model?.Name, model?.PlanCode, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult.Failed(
                    "خطا در به‌روزرسانی طرح بیمه. لطفاً دوباره تلاش کنید.",
                    "UPDATE_PLAN_ERROR",
                    ErrorCategory.General,
                    SecurityLevel.High);
            }
        }

        /// <summary>
        /// حذف نرم طرح بیمه
        /// </summary>
        public async Task<ServiceResult> SoftDeletePlanAsync(int planId)
        {
            _log.Information(
                "درخواست حذف طرح بیمه با شناسه {PlanId}. کاربر: {UserName} (شناسه: {UserId})",
                planId, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                var plan = await _insurancePlanRepository.GetByIdAsync(planId);
                if (plan == null)
                {
                    _log.Warning(
                        "طرح بیمه با شناسه {PlanId} یافت نشد. کاربر: {UserName} (شناسه: {UserId})",
                        planId, _currentUserService.UserName, _currentUserService.UserId);

                    return ServiceResult.Failed(
                        "طرح بیمه مورد نظر یافت نشد.",
                        "PLAN_NOT_FOUND",
                        ErrorCategory.NotFound,
                        SecurityLevel.Medium);
                }

                // حذف نرم
                _insurancePlanRepository.Delete(plan);
                await _insurancePlanRepository.SaveChangesAsync();

                _log.Information(
                    "طرح بیمه با موفقیت حذف شد. PlanId: {PlanId}, Name: {Name}. کاربر: {UserName} (شناسه: {UserId})",
                    planId, plan.Name, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult.Successful(
                    "طرح بیمه با موفقیت حذف شد.",
 "SoftDeletePlan",
 _currentUserService.UserId,
 _currentUserService.UserName,
                    securityLevel: SecurityLevel.Medium);
            }
            catch (Exception ex)
            {
                _log.Error(
                    ex,
                    "خطای سیستمی در حذف طرح بیمه. PlanId: {PlanId}. کاربر: {UserName} (شناسه: {UserId})",
                    planId, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult.Failed(
                    "خطا در حذف طرح بیمه. لطفاً دوباره تلاش کنید.",
                    "DELETE_PLAN_ERROR",
                    ErrorCategory.General,
                    SecurityLevel.High);
            }
        }

        #endregion

        #region Lookup Operations

        /// <summary>
        /// دریافت طرح‌های بیمه فعال برای Lookup
        /// </summary>
        public async Task<ServiceResult<List<InsurancePlanLookupViewModel>>> GetActivePlansForLookupAsync(int? providerId = null)
        {
            _log.Information(
                "درخواست دریافت طرح‌های بیمه فعال برای Lookup. ProviderId: {ProviderId}. کاربر: {UserName} (شناسه: {UserId})",
                providerId, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                List<InsurancePlan> plans;
                if (providerId.HasValue)
                {
                    plans = await _insurancePlanRepository.GetActiveByProviderIdAsync(providerId.Value);
                }
                else
                {
                    plans = await _insurancePlanRepository.GetActiveAsync();
                }

                var lookupItems = plans.Select(ConvertToLookupViewModel).ToList();

                _log.Information(
                    "طرح‌های بیمه فعال برای Lookup با موفقیت دریافت شدند. تعداد: {Count}. کاربر: {UserName} (شناسه: {UserId})",
                    lookupItems.Count, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<List<InsurancePlanLookupViewModel>>.Successful(
                    lookupItems,
                    "طرح‌های بیمه فعال با موفقیت دریافت شدند.",
 "GetActivePlansForLookup",
 _currentUserService.UserId,
 _currentUserService.UserName,
                    securityLevel: SecurityLevel.Low);
            }
            catch (Exception ex)
            {
                _log.Error(
                    ex,
                    "خطای سیستمی در دریافت طرح‌های بیمه فعال برای Lookup. ProviderId: {ProviderId}. کاربر: {UserName} (شناسه: {UserId})",
                    providerId, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<List<InsurancePlanLookupViewModel>>.Failed(
                    "خطا در دریافت طرح‌های بیمه فعال. لطفاً دوباره تلاش کنید.",
                    "GET_ACTIVE_PLANS_ERROR",
                    ErrorCategory.General,
                    SecurityLevel.High);
            }
        }

        /// <summary>
        /// دریافت طرح‌های بیمه تکمیلی برای Lookup
        /// </summary>
        public async Task<ServiceResult<List<InsurancePlanLookupViewModel>>> GetSupplementaryInsurancePlansAsync()
        {
            _log.Information(
                "درخواست دریافت طرح‌های بیمه تکمیلی برای Lookup. کاربر: {UserName} (شناسه: {UserId})",
                _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                // دریافت تمام طرح‌های بیمه فعال
                var plans = await _insurancePlanRepository.GetActiveAsync();
                
                // فیلتر کردن طرح‌های بیمه تکمیلی (Supplementary Insurance Plans)
                // استفاده از فیلد InsuranceType برای تشخیص دقیق
                var supplementaryPlans = plans
                    .Where(p => p.InsuranceType == InsuranceType.Supplementary)
                    .OrderBy(p => p.Name)
                    .ToList();

                var lookupItems = supplementaryPlans.Select(ConvertToLookupViewModel).ToList();

                _log.Information(
                    "طرح‌های بیمه تکمیلی برای Lookup با موفقیت دریافت شدند. تعداد: {Count}. کاربر: {UserName} (شناسه: {UserId})",
                    lookupItems.Count, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<List<InsurancePlanLookupViewModel>>.Successful(
                    lookupItems,
                    "طرح‌های بیمه تکمیلی با موفقیت دریافت شدند.",
                    "GetSupplementaryInsurancePlans",
                    _currentUserService.UserId,
                    _currentUserService.UserName,
                    securityLevel: SecurityLevel.Low);
            }
            catch (Exception ex)
            {
                _log.Error(
                    ex,
                    "خطای سیستمی در دریافت طرح‌های بیمه تکمیلی برای Lookup. کاربر: {UserName} (شناسه: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<List<InsurancePlanLookupViewModel>>.Failed(
                    "خطا در دریافت طرح‌های بیمه تکمیلی. لطفاً دوباره تلاش کنید.",
                    "GET_SUPPLEMENTARY_INSURANCE_PLANS_ERROR",
                    ErrorCategory.General,
                    SecurityLevel.High);
            }
        }

        /// <summary>
        /// دریافت طرح‌های بیمه پایه برای Lookup
        /// </summary>
        public async Task<ServiceResult<List<InsurancePlanLookupViewModel>>> GetPrimaryInsurancePlansAsync()
        {
            _log.Information(
                "درخواست دریافت طرح‌های بیمه پایه برای Lookup. کاربر: {UserName} (شناسه: {UserId})",
                _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                // دریافت تمام طرح‌های بیمه فعال
                var plans = await _insurancePlanRepository.GetActiveAsync();
                
                // فیلتر کردن طرح‌های بیمه پایه (Primary Insurance Plans)
                // استفاده از فیلد InsuranceType برای تشخیص دقیق
                var primaryPlans = plans
                    .Where(p => p.InsuranceType == InsuranceType.Primary)
                    .OrderBy(p => p.Name)
                    .ToList();

                var lookupItems = primaryPlans.Select(ConvertToLookupViewModel).ToList();

                _log.Information(
                    "طرح‌های بیمه پایه برای Lookup با موفقیت دریافت شدند. تعداد: {Count}. کاربر: {UserName} (شناسه: {UserId})",
                    lookupItems.Count, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<List<InsurancePlanLookupViewModel>>.Successful(
                    lookupItems,
                    "طرح‌های بیمه پایه با موفقیت دریافت شدند.",
                    "GetPrimaryInsurancePlans",
                    _currentUserService.UserId,
                    _currentUserService.UserName,
                    securityLevel: SecurityLevel.Low);
            }
            catch (Exception ex)
            {
                _log.Error(
                    ex,
                    "خطای سیستمی در دریافت طرح‌های بیمه پایه برای Lookup. کاربر: {UserName} (شناسه: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<List<InsurancePlanLookupViewModel>>.Failed(
                    "خطا در دریافت طرح‌های بیمه پایه. لطفاً دوباره تلاش کنید.",
                    "GET_PRIMARY_INSURANCE_PLANS_ERROR",
                    ErrorCategory.General,
                    SecurityLevel.High);
            }
        }

        /// <summary>
        /// دریافت لیست شرکت‌های بیمه برای SelectList
        /// </summary>
        public async Task<ServiceResult<List<InsuranceProviderLookupViewModel>>> GetActiveProvidersForLookupAsync()
        {
            _log.Information(
                "درخواست دریافت شرکت‌های بیمه فعال برای Lookup. کاربر: {UserName} (شناسه: {UserId})",
                _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                // دریافت تمام طرح‌های بیمه فعال
                var plans = await _insurancePlanRepository.GetActiveAsync();
                
                // استخراج unique providers از plans
                var providers = plans
                    .Where(p => p.InsuranceProvider != null)
                    .GroupBy(p => new { p.InsuranceProvider.InsuranceProviderId, p.InsuranceProvider.Name, p.InsuranceProvider.Code })
                    .Select(g => new InsuranceProviderLookupViewModel
                    {
                        InsuranceProviderId = g.Key.InsuranceProviderId,
                        Name = g.Key.Name,
                        Code = g.Key.Code
                    })
                    .OrderBy(p => p.Name)
                    .ToList();

                _log.Information(
                    "شرکت‌های بیمه فعال برای Lookup با موفقیت دریافت شدند. تعداد: {Count}. کاربر: {UserName} (شناسه: {UserId})",
                    providers.Count, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<List<InsuranceProviderLookupViewModel>>.Successful(
                    providers,
                    "شرکت‌های بیمه فعال با موفقیت دریافت شدند.",
                    "GetActiveProvidersForLookup",
                    _currentUserService.UserId,
                    _currentUserService.UserName,
                    securityLevel: SecurityLevel.Low);
            }
            catch (Exception ex)
            {
                _log.Error(
                    ex,
                    "خطای سیستمی در دریافت شرکت‌های بیمه فعال برای Lookup. کاربر: {UserName} (شناسه: {UserId})",
                    _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<List<InsuranceProviderLookupViewModel>>.Failed(
                    "خطا در دریافت شرکت‌های بیمه فعال. لطفاً دوباره تلاش کنید.",
                    "GET_ACTIVE_PROVIDERS_ERROR",
                    ErrorCategory.General,
                    SecurityLevel.High);
            }
        }

        #endregion

        #region Validation Operations

        /// <summary>
        /// بررسی وجود کد طرح بیمه
        /// </summary>
        public async Task<ServiceResult<bool>> DoesPlanCodeExistAsync(string planCode, int? excludeId = null)
        {
            try
            {
                var exists = await _insurancePlanRepository.DoesPlanCodeExistAsync(planCode, excludeId);
                return ServiceResult<bool>.Successful(
                    exists,
                    "بررسی وجود کد طرح بیمه انجام شد.",
 "DoesPlanCodeExist",
 _currentUserService.UserId,
 _currentUserService.UserName,
                    securityLevel: SecurityLevel.Low);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در بررسی وجود کد طرح بیمه. PlanCode: {PlanCode}", planCode);
                return ServiceResult<bool>.Failed(
                    "خطا در بررسی وجود کد طرح بیمه.",
                    "CHECK_PLAN_CODE_EXISTS_ERROR",
                    ErrorCategory.General,
                    SecurityLevel.High);
            }
        }

        /// <summary>
        /// بررسی وجود نام طرح بیمه در ارائه‌دهنده
        /// </summary>
        public async Task<ServiceResult<bool>> DoesNameExistInProviderAsync(string name, int providerId, int? excludeId = null)
        {
            try
            {
                var exists = await _insurancePlanRepository.DoesNameExistInProviderAsync(name, providerId, excludeId);
                return ServiceResult<bool>.Successful(
                    exists,
                    "بررسی وجود نام طرح بیمه در ارائه‌دهنده انجام شد.",
 "DoesNameExistInProvider",
 _currentUserService.UserId,
 _currentUserService.UserName,
                    securityLevel: SecurityLevel.Low);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در بررسی وجود نام طرح بیمه در ارائه‌دهنده. Name: {Name}, ProviderId: {ProviderId}", name, providerId);
                return ServiceResult<bool>.Failed(
                    "خطا در بررسی وجود نام طرح بیمه در ارائه‌دهنده.",
                    "CHECK_PLAN_NAME_EXISTS_ERROR",
                    ErrorCategory.General,
                    SecurityLevel.High);
            }
        }

        #endregion

        #region Business Logic Operations

        /// <summary>
        /// دریافت طرح‌های بیمه بر اساس ارائه‌دهنده
        /// </summary>
        public async Task<ServiceResult<List<InsurancePlanIndexViewModel>>> GetPlansByProviderAsync(int providerId)
        {
            _log.Information(
                "درخواست دریافت طرح‌های بیمه بر اساس ارائه‌دهنده. ProviderId: {ProviderId}. کاربر: {UserName} (شناسه: {UserId})",
                providerId, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                var plans = await _insurancePlanRepository.GetByProviderIdAsync(providerId);
                var viewModels = plans.Select(ConvertToIndexViewModel).ToList();

                _log.Information(
                    "طرح‌های بیمه بر اساس ارائه‌دهنده با موفقیت دریافت شدند. تعداد: {Count}. کاربر: {UserName} (شناسه: {UserId})",
                    viewModels.Count, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<List<InsurancePlanIndexViewModel>>.Successful(
                    viewModels,
                    "طرح‌های بیمه بر اساس ارائه‌دهنده با موفقیت دریافت شدند.",
 "GetPlansByProvider",
 _currentUserService.UserId,
 _currentUserService.UserName,
                    securityLevel: SecurityLevel.Low);
            }
            catch (Exception ex)
            {
                _log.Error(
                    ex,
                    "خطای سیستمی در دریافت طرح‌های بیمه بر اساس ارائه‌دهنده. ProviderId: {ProviderId}. کاربر: {UserName} (شناسه: {UserId})",
                    providerId, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<List<InsurancePlanIndexViewModel>>.Failed(
                    "خطا در دریافت طرح‌های بیمه بر اساس ارائه‌دهنده. لطفاً دوباره تلاش کنید.",
                    "GET_PLANS_BY_PROVIDER_ERROR",
                    ErrorCategory.General,
                    SecurityLevel.High);
            }
        }

        /// <summary>
        /// بررسی اعتبار طرح بیمه
        /// </summary>
        public async Task<ServiceResult<bool>> IsPlanValidAsync(int planId, DateTime checkDate)
        {
            _log.Information(
                "درخواست بررسی اعتبار طرح بیمه. PlanId: {PlanId}, CheckDate: {CheckDate}. کاربر: {UserName} (شناسه: {UserId})",
                planId, checkDate, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                var plan = await _insurancePlanRepository.GetByIdAsync(planId);
                if (plan == null)
                {
                    return ServiceResult<bool>.Successful(
                        false,
                        "طرح بیمه یافت نشد.",
 "IsPlanValid",
 _currentUserService.UserId,
 _currentUserService.UserName,
                        securityLevel: SecurityLevel.Low);
                }

                var isValid = plan.IsActive && 
                             !plan.IsDeleted && 
                             plan.ValidFrom <= checkDate && 
                             plan.ValidTo >= checkDate;

                _log.Information(
                    "بررسی اعتبار طرح بیمه انجام شد. PlanId: {PlanId}, IsValid: {IsValid}. کاربر: {UserName} (شناسه: {UserId})",
                    planId, isValid, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<bool>.Successful(
                    isValid,
                    "بررسی اعتبار طرح بیمه انجام شد.",
 "IsPlanValid",
 _currentUserService.UserId,
 _currentUserService.UserName,
                    securityLevel: SecurityLevel.Low);
            }
            catch (Exception ex)
            {
                _log.Error(
                    ex,
                    "خطای سیستمی در بررسی اعتبار طرح بیمه. PlanId: {PlanId}, CheckDate: {CheckDate}. کاربر: {UserName} (شناسه: {UserId})",
                    planId, checkDate, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<bool>.Failed(
                    "خطا در بررسی اعتبار طرح بیمه.",
                    "CHECK_PLAN_VALIDITY_ERROR",
                    ErrorCategory.General,
                    SecurityLevel.High);
            }
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// تبدیل Entity به Index ViewModel
        /// </summary>
        private InsurancePlanIndexViewModel ConvertToIndexViewModel(InsurancePlan plan)
        {
            if (plan == null) return null;

            return new InsurancePlanIndexViewModel
            {
                InsurancePlanId = plan.InsurancePlanId,
                Name = plan.Name,
                PlanCode = plan.PlanCode,
                InsuranceProviderName = plan.InsuranceProvider?.Name,
                CoveragePercent = plan.CoveragePercent,
                Deductible = plan.Deductible,
                ValidFrom = plan.ValidFrom,
                ValidTo = plan.ValidTo,
                ValidFromShamsi = plan.ValidFrom.ToPersianDate(),
                ValidToShamsi = plan.ValidTo.ToPersianDate(),
                IsActive = plan.IsActive,
                CreatedAt = plan.CreatedAt,
                CreatedAtShamsi = plan.CreatedAt.ToPersianDateTime(),
                CreatedByUserName = plan.CreatedByUser != null ? $"{plan.CreatedByUser.FirstName} {plan.CreatedByUser.LastName}".Trim() : null
            };
        }

        /// <summary>
        /// تبدیل Entity به Details ViewModel
        /// </summary>
        private InsurancePlanDetailsViewModel ConvertToDetailsViewModel(InsurancePlan plan)
        {
            if (plan == null) return null;

            return new InsurancePlanDetailsViewModel
            {
                InsurancePlanId = plan.InsurancePlanId,
                Name = plan.Name,
                PlanCode = plan.PlanCode,
                InsuranceProviderId = plan.InsuranceProviderId,
                InsuranceProviderName = plan.InsuranceProvider?.Name,
                CoveragePercent = plan.CoveragePercent,
                Deductible = plan.Deductible,
                ValidFrom = plan.ValidFrom,
                ValidTo = plan.ValidTo,
                ValidFromShamsi = plan.ValidFrom.ToPersianDate(),
                ValidToShamsi = plan.ValidTo.ToPersianDate(),
                IsActive = plan.IsActive,
                CreatedAt = plan.CreatedAt,
                UpdatedAt = plan.UpdatedAt,
                CreatedAtShamsi = plan.CreatedAt.ToPersianDateTime(),
                UpdatedAtShamsi = plan.UpdatedAt.HasValue ? plan.UpdatedAt.Value.ToPersianDateTime() : null,
                CreatedByUserName = plan.CreatedByUser != null ? $"{plan.CreatedByUser.FirstName} {plan.CreatedByUser.LastName}".Trim() : null,
                UpdatedByUserName = plan.UpdatedByUser != null ? $"{plan.UpdatedByUser.FirstName} {plan.UpdatedByUser.LastName}".Trim() : null,
                PlanServiceCount = plan.PlanServices?.Count(ps => !ps.IsDeleted) ?? 0
            };
        }

        /// <summary>
        /// تبدیل Entity به CreateEdit ViewModel
        /// </summary>
        private InsurancePlanCreateEditViewModel ConvertToCreateEditViewModel(InsurancePlan plan)
        {
            if (plan == null) return null;

            return new InsurancePlanCreateEditViewModel
            {
                InsurancePlanId = plan.InsurancePlanId,
                Name = plan.Name,
                PlanCode = plan.PlanCode,
                InsuranceProviderId = plan.InsuranceProviderId,
                CoveragePercent = plan.CoveragePercent,
                Deductible = plan.Deductible,
                ValidFrom = plan.ValidFrom,
                ValidTo = plan.ValidTo,
                IsActive = plan.IsActive
            };
        }

        /// <summary>
        /// تبدیل CreateEdit ViewModel به Entity
        /// </summary>
        private InsurancePlan ConvertToEntity(InsurancePlanCreateEditViewModel model)
        {
            if (model == null) return null;

            return new InsurancePlan
            {
                InsurancePlanId = model.InsurancePlanId,
                Name = model.Name,
                PlanCode = model.PlanCode,
                InsuranceProviderId = model.InsuranceProviderId,
                CoveragePercent = model.CoveragePercent,
                Deductible = model.Deductible,
                ValidFrom = model.ValidFrom,
                ValidTo = model.ValidTo ?? DateTime.Now.AddYears(1),
                IsActive = model.IsActive
            };
        }

        /// <summary>
        /// تبدیل Entity به Lookup ViewModel
        /// </summary>
        private InsurancePlanLookupViewModel ConvertToLookupViewModel(InsurancePlan plan)
        {
            if (plan == null) return null;

            return new InsurancePlanLookupViewModel
            {
                InsurancePlanId = plan.InsurancePlanId,
                Name = plan.Name,
                PlanCode = plan.PlanCode,
                InsuranceProviderName = plan.InsuranceProvider?.Name,
                CoveragePercent = plan.CoveragePercent
            };
        }

        #endregion
    }
}
