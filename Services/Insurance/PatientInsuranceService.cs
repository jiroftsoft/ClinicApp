using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Core;
using ClinicApp.Extensions;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Interfaces.Insurance;
using ClinicApp.Models.Entities;
using ClinicApp.Models.Entities.Patient;
using ClinicApp.Models.Enums;
using ClinicApp.ViewModels.Insurance.PatientInsurance;
using ClinicApp.ViewModels.Insurance.InsuranceCalculation;
using Serilog;

namespace ClinicApp.Services.Insurance
{
    /// <summary>
    /// سرویس مدیریت بیمه‌های بیماران - طراحی شده برای سیستم‌های پزشکی کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. مدیریت کامل بیمه‌های بیماران (اصلی و تکمیلی)
    /// 2. استفاده از ServiceResult Enhanced pattern
    /// 3. پشتیبانی از FluentValidation
    /// 4. مدیریت کامل خطاها و لاگ‌گیری
    /// 5. پشتیبانی از صفحه‌بندی و جستجو
    /// 6. مدیریت Lookup Lists برای UI
    /// 7. رعایت استانداردهای پزشکی ایران
    /// 
    /// نکته حیاتی: این کلاس بر اساس استانداردهای سیستم‌های پزشکی ایران پیاده‌سازی شده است
    /// </summary>
    public class PatientInsuranceService : IPatientInsuranceService
    {
        private readonly IPatientInsuranceRepository _patientInsuranceRepository;
        private readonly ICombinedInsuranceCalculationService _combinedInsuranceCalculationService;
        private readonly IServiceRepository _serviceRepository;
        private readonly IPatientService _patientService;
        private readonly ILogger _log;
        private readonly ICurrentUserService _currentUserService;

        /// <summary>
        /// 🚨 CRITICAL FIX: ماسک کردن اطلاعات حساس برای لاگ
        /// </summary>
        private string MaskSensitiveData(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return value;
            if (value.Length <= 4) return new string('*', value.Length);
            return new string('*', Math.Max(0, value.Length - 4)) + value.Substring(value.Length - 4);
        }

        public PatientInsuranceService(
            IPatientInsuranceRepository patientInsuranceRepository,
            ICombinedInsuranceCalculationService combinedInsuranceCalculationService,
            IServiceRepository serviceRepository,
            IPatientService patientService,
            ILogger logger,
            ICurrentUserService currentUserService)
        {
            _patientInsuranceRepository = patientInsuranceRepository ?? throw new ArgumentNullException(nameof(patientInsuranceRepository));
            _combinedInsuranceCalculationService = combinedInsuranceCalculationService ?? throw new ArgumentNullException(nameof(combinedInsuranceCalculationService));
            _serviceRepository = serviceRepository ?? throw new ArgumentNullException(nameof(serviceRepository));
            _patientService = patientService ?? throw new ArgumentNullException(nameof(patientService));
            _log = logger.ForContext<PatientInsuranceService>();
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        #region Validation Methods

        /// <summary>
        /// بررسی وجود خدمت
        /// </summary>
        public async Task<ServiceResult<bool>> ServiceExistsAsync(int serviceId)
        {
            try
            {
                _log.Information("بررسی وجود خدمت. ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                    serviceId, _currentUserService.UserName, _currentUserService.UserId);

                // استفاده از ServiceRepository موجود
                var service = await _serviceRepository.GetByIdAsync(serviceId);
                var exists = service != null && !service.IsDeleted;
                
                _log.Information("نتیجه بررسی وجود خدمت. ServiceId: {ServiceId}, Exists: {Exists}. User: {UserName} (Id: {UserId})",
                    serviceId, exists, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<bool>.Successful(exists);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در بررسی وجود خدمت. ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                    serviceId, _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult<bool>.Failed("خطا در بررسی وجود خدمت");
            }
        }

        /// <summary>
        /// بررسی وجود بیمار
        /// </summary>
        public async Task<ServiceResult<bool>> PatientExistsAsync(int patientId)
        {
            try
            {
                _log.Information("بررسی وجود بیمار. PatientId: {PatientId}. User: {UserName} (Id: {UserId})",
                    patientId, _currentUserService.UserName, _currentUserService.UserId);

                // استفاده از PatientService موجود
                var patientResult = await _patientService.GetPatientDetailsAsync(patientId);
                var exists = patientResult.Success;
                
                _log.Information("نتیجه بررسی وجود بیمار. PatientId: {PatientId}, Exists: {Exists}. User: {UserName} (Id: {UserId})",
                    patientId, exists, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<bool>.Successful(exists);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطا در بررسی وجود بیمار. PatientId: {PatientId}. User: {UserName} (Id: {UserId})",
                    patientId, _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult<bool>.Failed("خطا در بررسی وجود بیمار");
            }
        }

        #endregion

        #region IPatientInsuranceService Implementation

        public async Task<ServiceResult<PagedResult<PatientInsuranceIndexViewModel>>> GetPatientInsurancesAsync(int? patientId, string searchTerm, int pageNumber, int pageSize)
        {
            try
            {
                _log.Information("Getting patient insurances with PatientId: {PatientId}, SearchTerm: {SearchTerm}, Page: {PageNumber}, Size: {PageSize}. User: {UserName} (Id: {UserId})", 
                    patientId, searchTerm, pageNumber, pageSize, _currentUserService.UserName, _currentUserService.UserId);

                // استفاده از متد GetPagedAsync که واقعاً کار می‌کند
                return await GetPagedAsync(searchTerm, null, null, null, null, null, null, pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error getting patient insurances with PatientId: {PatientId}, SearchTerm: {SearchTerm}, Page: {PageNumber}, Size: {PageSize}. User: {UserName} (Id: {UserId})", 
                    patientId, searchTerm, pageNumber, pageSize, _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult<PagedResult<PatientInsuranceIndexViewModel>>.Failed("خطا در دریافت لیست بیمه‌های بیماران");
            }
        }

        /// <summary>
        /// دریافت لیست بیمه‌های بیماران با فیلترهای کامل
        /// </summary>
        public async Task<ServiceResult<PagedResult<PatientInsuranceIndexViewModel>>> GetPatientInsurancesWithFiltersAsync(
            int? patientId = null, 
            string searchTerm = "", 
            int? providerId = null, 
            bool? isPrimary = null, 
            bool? isActive = null, 
            int pageNumber = 1, 
            int pageSize = 10)
        {
            try
            {
                _log.Information("Getting patient insurances with filters - PatientId: {PatientId}, SearchTerm: {SearchTerm}, ProviderId: {ProviderId}, IsPrimary: {IsPrimary}, IsActive: {IsActive}, Page: {PageNumber}, Size: {PageSize}. User: {UserName} (Id: {UserId})", 
                    patientId, searchTerm, providerId, isPrimary, isActive, pageNumber, pageSize, _currentUserService.UserName, _currentUserService.UserId);

                // استفاده از متد GetPagedAsync با فیلترهای کامل
                return await GetPagedAsync(searchTerm, providerId, null, isPrimary, isActive, null, null, pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error getting patient insurances with filters - PatientId: {PatientId}, SearchTerm: {SearchTerm}, ProviderId: {ProviderId}, IsPrimary: {IsPrimary}, IsActive: {IsActive}, Page: {PageNumber}, Size: {PageSize}. User: {UserName} (Id: {UserId})", 
                    patientId, searchTerm, providerId, isPrimary, isActive, pageNumber, pageSize, _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult<PagedResult<PatientInsuranceIndexViewModel>>.Failed("خطا در دریافت لیست بیمه‌های بیماران");
            }
        }

        public async Task<ServiceResult<PatientInsuranceDetailsViewModel>> GetPatientInsuranceDetailsAsync(int patientInsuranceId)
        {
            try
            {
                _log.Information("Getting patient insurance details for PatientInsuranceId: {PatientInsuranceId}. User: {UserName} (Id: {UserId})", 
                    patientInsuranceId, _currentUserService.UserName, _currentUserService.UserId);

                // 🏥 استفاده از GetByIdWithDetailsAsync برای دریافت اطلاعات کامل
                var entity = await _patientInsuranceRepository.GetByIdWithDetailsAsync(patientInsuranceId);
                if (entity == null)
                {
                    _log.Warning("Patient insurance not found. PatientInsuranceId: {PatientInsuranceId}. User: {UserName} (Id: {UserId})", 
                        patientInsuranceId, _currentUserService.UserName, _currentUserService.UserId);
                    return ServiceResult<PatientInsuranceDetailsViewModel>.Failed("بیمه بیمار یافت نشد");
                }

                // تبدیل Entity به Details ViewModel
                var viewModel = ConvertToDetailsViewModel(entity);
                return ServiceResult<PatientInsuranceDetailsViewModel>.Successful(viewModel);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error getting patient insurance details for PatientInsuranceId: {PatientInsuranceId}. User: {UserName} (Id: {UserId})", 
                    patientInsuranceId, _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult<PatientInsuranceDetailsViewModel>.Failed("خطا در دریافت جزئیات بیمه بیمار");
            }
        }

        /// <summary>
        /// دریافت وضعیت بیمه بیمار برای پذیرش (برای کنترلرهای جدید)
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <returns>وضعیت بیمه بیمار</returns>
        public async Task<ServiceResult<object>> GetPatientInsuranceStatusForReceptionAsync(int patientId)
        {
            // TODO: پیاده‌سازی منطق دریافت وضعیت بیمه برای پذیرش
            return ServiceResult<object>.Successful(new { PatientId = patientId, HasInsurance = true, Status = "فعال" });
        }

        /// <summary>
        /// دریافت بیمه‌های بیمار برای پذیرش (برای کنترلرهای جدید)
        /// </summary>
        /// <param name="patientId">شناسه بیمار</param>
        /// <returns>لیست بیمه‌های بیمار</returns>
        public async Task<ServiceResult<object>> GetPatientInsurancesForReceptionAsync(int patientId)
        {
            try
            {
                _log.Information("📋 دریافت بیمه‌های بیمار برای پذیرش: {PatientId}, کاربر: {UserName}", 
                    patientId, _currentUserService.UserName);

                // دریافت بیمه‌های فعال بیمار
                var patientInsurances = await _patientInsuranceRepository.GetByPatientIdAsync(patientId);
                _log.Information("📊 دریافت {Count} بیمه از دیتابیس برای بیمار {PatientId}", patientInsurances.Count, patientId);
                
                var activeInsurances = patientInsurances.Where(pi => pi.IsActive && !pi.IsDeleted).ToList();
                _log.Information("📊 {Count} بیمه فعال از {Total} بیمه برای بیمار {PatientId}", activeInsurances.Count, patientInsurances.Count, patientId);

                if (!activeInsurances.Any())
                {
                    _log.Information("هیچ بیمه فعالی برای بیمار {PatientId} یافت نشد", patientId);
                    return ServiceResult<object>.Successful(new { PatientId = patientId, Insurances = new List<object>() });
                }

                // تبدیل به فرمت مناسب برای نمایش
                var insuranceList = new List<object>();
                
                foreach (var insurance in activeInsurances)
                {
                    var insuranceData = new
                    {
                        PatientInsuranceId = insurance.PatientInsuranceId,
                        PatientId = insurance.PatientId,
                        InsurancePlanId = insurance.InsurancePlanId,
                        InsurancePlanName = insurance.InsurancePlan?.Name ?? "نامشخص",
                        InsuranceProviderId = insurance.InsurancePlan?.InsuranceProviderId,
                        InsuranceProviderName = insurance.InsurancePlan?.InsuranceProvider?.Name ?? "نامشخص",
                        PolicyNumber = insurance.PolicyNumber,
                        CardNumber = insurance.CardNumber,
                        StartDate = insurance.StartDate,
                        EndDate = insurance.EndDate,
                        IsPrimary = insurance.IsPrimary,
                        IsActive = insurance.IsActive,
                        Priority = insurance.Priority,
                        SupplementaryPolicyNumber = insurance.SupplementaryPolicyNumber,
                        SupplementaryInsuranceProviderId = insurance.SupplementaryInsuranceProviderId,
                        SupplementaryInsurancePlanId = insurance.SupplementaryInsurancePlanId,
                        SupplementaryInsuranceProviderName = insurance.SupplementaryInsuranceProvider?.Name ?? "نامشخص",
                        SupplementaryInsurancePlanName = insurance.SupplementaryInsurancePlan?.Name ?? "نامشخص"
                    };
                    
                    insuranceList.Add(insuranceData);
                }

                _log.Information("✅ {Count} بیمه فعال برای بیمار {PatientId} یافت شد", 
                    insuranceList.Count, patientId);

                return ServiceResult<object>.Successful(new { PatientId = patientId, Insurances = insuranceList });
            }
            catch (Exception ex)
            {
                _log.Error(ex, "❌ خطا در دریافت بیمه‌های بیمار برای پذیرش: {PatientId}", patientId);
                return ServiceResult<object>.Failed("خطا در دریافت بیمه‌های بیمار");
            }
        }


        public async Task<ServiceResult<int>> CreatePatientInsuranceAsync(PatientInsuranceCreateEditViewModel model)
        {
            try
            {
                _log.Information("Creating patient insurance for PatientId: {PatientId}, PolicyNumber(masked): {PolicyNumber}, PlanId: {PlanId}. User: {UserName} (Id: {UserId})", 
                    model.PatientId, MaskSensitiveData(model.PolicyNumber), model.InsurancePlanId, _currentUserService.UserName, _currentUserService.UserId);

                // استفاده از متد CreateAsync که واقعاً کار می‌کند
                return await CreateAsync(model);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error creating patient insurance for PatientId: {PatientId}, PolicyNumber(masked): {PolicyNumber}, PlanId: {PlanId}. User: {UserName} (Id: {UserId})", 
                    model.PatientId, MaskSensitiveData(model.PolicyNumber), model.InsurancePlanId, _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult<int>.Failed("خطا در ایجاد بیمه بیمار");
            }
        }

        /// <summary>
        /// افزودن بیمه تکمیلی به رکورد بیمه پایه موجود
        /// </summary>
        public async Task<ServiceResult<int>> AddSupplementaryInsuranceToExistingAsync(PatientInsuranceCreateEditViewModel model)
        {
            try
            {
                _log.Information("🏥 MEDICAL: افزودن بیمه تکمیلی به رکورد موجود. PatientId: {PatientId}, SupplementaryProviderId: {SupplementaryProviderId}, SupplementaryPlanId: {SupplementaryPlanId}. User: {UserName} (Id: {UserId})", 
                    model.PatientId, model.SupplementaryInsuranceProviderId, model.SupplementaryInsurancePlanId, _currentUserService.UserName, _currentUserService.UserId);

                // 1. پیدا کردن رکورد بیمه پایه موجود
                var existingInsurances = await _patientInsuranceRepository.GetByPatientIdAsync(model.PatientId);
                var primaryInsurance = existingInsurances.FirstOrDefault(pi => pi.IsPrimary && pi.IsActive && !pi.IsDeleted);
                
                if (primaryInsurance == null)
                {
                    _log.Warning("🏥 MEDICAL: رکورد بیمه پایه یافت نشد. PatientId: {PatientId}. User: {UserName} (Id: {UserId})", 
                        model.PatientId, _currentUserService.UserName, _currentUserService.UserId);
                    return ServiceResult<int>.Failed("رکورد بیمه پایه برای این بیمار یافت نشد");
                }

                // 2. بررسی عدم وجود بیمه تکمیلی فعال
                if (primaryInsurance.SupplementaryInsuranceProviderId.HasValue)
                {
                    _log.Warning("🏥 MEDICAL: بیمه تکمیلی فعال موجود است. PatientInsuranceId: {PatientInsuranceId}, PatientId: {PatientId}. User: {UserName} (Id: {UserId})", 
                        primaryInsurance.PatientInsuranceId, model.PatientId, _currentUserService.UserName, _currentUserService.UserId);
                    return ServiceResult<int>.Failed("بیمه تکمیلی فعال برای این بیمار موجود است");
                }

                // 3. به‌روزرسانی رکورد بیمه پایه با اطلاعات تکمیلی
                primaryInsurance.SupplementaryInsuranceProviderId = model.SupplementaryInsuranceProviderId;
                primaryInsurance.SupplementaryInsurancePlanId = model.SupplementaryInsurancePlanId;
                primaryInsurance.SupplementaryPolicyNumber = model.SupplementaryPolicyNumber;
                primaryInsurance.UpdatedAt = DateTime.UtcNow;
                primaryInsurance.UpdatedByUserId = _currentUserService.UserId;

                // 4. ذخیره تغییرات
                _patientInsuranceRepository.Update(primaryInsurance);
                await _patientInsuranceRepository.SaveChangesAsync();

                _log.Information("🏥 MEDICAL: بیمه تکمیلی با موفقیت اضافه شد. PatientInsuranceId: {PatientInsuranceId}, PatientId: {PatientId}. User: {UserName} (Id: {UserId})", 
                    primaryInsurance.PatientInsuranceId, model.PatientId, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<int>.Successful(primaryInsurance.PatientInsuranceId);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در افزودن بیمه تکمیلی. PatientId: {PatientId}, SupplementaryProviderId: {SupplementaryProviderId}, SupplementaryPlanId: {SupplementaryPlanId}. User: {UserName} (Id: {UserId})", 
                    model.PatientId, model.SupplementaryInsuranceProviderId, model.SupplementaryInsurancePlanId, _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult<int>.Failed("خطا در افزودن بیمه تکمیلی");
            }
        }

        public async Task<ServiceResult> UpdatePatientInsuranceAsync(PatientInsuranceCreateEditViewModel model)
        {
            try
            {
                _log.Information("Updating patient insurance for PatientInsuranceId: {PatientInsuranceId}, PatientId: {PatientId}, PolicyNumber(masked): {PolicyNumber}. User: {UserName} (Id: {UserId})", 
                    model.PatientInsuranceId, model.PatientId, MaskSensitiveData(model.PolicyNumber), _currentUserService.UserName, _currentUserService.UserId);

                // استفاده از متد UpdateAsync که واقعاً کار می‌کند
                var result = await UpdateAsync(model);
                if (result.Success)
                {
                    return ServiceResult.Successful("بیمه بیمار با موفقیت به‌روزرسانی شد");
                }
                else
                {
                    return ServiceResult.Failed(result.Message);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error updating patient insurance for PatientInsuranceId: {PatientInsuranceId}, PatientId: {PatientId}, PolicyNumber(masked): {PolicyNumber}. User: {UserName} (Id: {UserId})", 
                    model.PatientInsuranceId, model.PatientId, MaskSensitiveData(model.PolicyNumber), _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult.Failed("خطا در به‌روزرسانی بیمه بیمار");
            }
        }

        public async Task<ServiceResult> SoftDeletePatientInsuranceAsync(int patientInsuranceId)
        {
            try
            {
                _log.Information("Soft deleting patient insurance for PatientInsuranceId: {PatientInsuranceId}. User: {UserName} (Id: {UserId})", 
                    patientInsuranceId, _currentUserService.UserName, _currentUserService.UserId);

                // استفاده از متد DeleteAsync که واقعاً کار می‌کند
                var result = await DeleteAsync(patientInsuranceId);
                if (result.Success)
                {
                    return ServiceResult.Successful("بیمه بیمار با موفقیت حذف شد");
                }
                else
                {
                    return ServiceResult.Failed(result.Message);
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error soft deleting patient insurance for PatientInsuranceId: {PatientInsuranceId}. User: {UserName} (Id: {UserId})", 
                    patientInsuranceId, _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult.Failed("خطا در حذف بیمه بیمار");
            }
        }

        public async Task<ServiceResult<List<PatientInsuranceLookupViewModel>>> GetActivePatientInsurancesForLookupAsync(int patientId)
        {
            try
            {
                _log.Information("Getting active patient insurances for lookup for PatientId: {PatientId}. User: {UserName} (Id: {UserId})", 
                    patientId, _currentUserService.UserName, _currentUserService.UserId);

                // استفاده از متد GetActiveByPatientIdAsync که واقعاً کار می‌کند
                return await GetActiveByPatientIdAsync(patientId);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error getting active patient insurances for lookup for PatientId: {PatientId}. User: {UserName} (Id: {UserId})", 
                    patientId, _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult<List<PatientInsuranceLookupViewModel>>.Failed("خطا در دریافت بیمه‌های فعال بیمار");
            }
        }

        public async Task<ServiceResult<bool>> DoesPolicyNumberExistAsync(string policyNumber, int? excludePatientInsuranceId)
        {
            try
            {
                _log.Information("Checking if policy number exists: {PolicyNumber}(masked), ExcludeId: {ExcludeId}. User: {UserName} (Id: {UserId})", 
                    MaskSensitiveData(policyNumber), excludePatientInsuranceId, _currentUserService.UserName, _currentUserService.UserId);

                // استفاده از متد ریپازیتوری که واقعاً کار می‌کند
                var exists = await _patientInsuranceRepository.DoesPolicyNumberExistAsync(policyNumber, excludePatientInsuranceId);
                return ServiceResult<bool>.Successful(exists);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error checking policy number existence: {PolicyNumber}(masked), ExcludeId: {ExcludeId}. User: {UserName} (Id: {UserId})", 
                    MaskSensitiveData(policyNumber), excludePatientInsuranceId, _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult<bool>.Failed("خطا در بررسی وجود شماره بیمه");
            }
        }

        public async Task<ServiceResult<bool>> DoesPrimaryInsuranceExistAsync(int patientId, int? excludePatientInsuranceId)
        {
            try
            {
                _log.Information("Checking if primary insurance exists for PatientId: {PatientId}, ExcludeId: {ExcludeId}. User: {UserName} (Id: {UserId})", 
                    patientId, excludePatientInsuranceId, _currentUserService.UserName, _currentUserService.UserId);

                // استفاده از متد ریپازیتوری که واقعاً کار می‌کند
                var exists = await _patientInsuranceRepository.DoesPrimaryInsuranceExistAsync(patientId, excludePatientInsuranceId);
                return ServiceResult<bool>.Successful(exists);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error checking primary insurance existence for PatientId: {PatientId}, ExcludeId: {ExcludeId}. User: {UserName} (Id: {UserId})", 
                    patientId, excludePatientInsuranceId, _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult<bool>.Failed("خطا در بررسی وجود بیمه اصلی");
            }
        }

        public async Task<ServiceResult<bool>> DoesDateOverlapExistAsync(int patientId, DateTime startDate, DateTime endDate, int? excludePatientInsuranceId)
        {
            try
            {
                _log.Information("Checking if date overlap exists for PatientId: {PatientId}, StartDate: {StartDate}, EndDate: {EndDate}, ExcludeId: {ExcludeId}. User: {UserName} (Id: {UserId})", 
                    patientId, startDate, endDate, excludePatientInsuranceId, _currentUserService.UserName, _currentUserService.UserId);

                // استفاده از متد ریپازیتوری که واقعاً کار می‌کند
                var exists = await _patientInsuranceRepository.DoesDateOverlapExistAsync(patientId, startDate, endDate, excludePatientInsuranceId);
                return ServiceResult<bool>.Successful(exists);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error checking date overlap existence for PatientId: {PatientId}, StartDate: {StartDate}, EndDate: {EndDate}, ExcludeId: {ExcludeId}. User: {UserName} (Id: {UserId})", 
                    patientId, startDate, endDate, excludePatientInsuranceId, _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult<bool>.Failed("خطا در بررسی تداخل تاریخ");
            }
        }

        /// <summary>
        /// دریافت بیمه اصلی بیمار بر اساس شماره بیمه
        /// </summary>
        public async Task<ServiceResult<PatientInsurance>> GetPrimaryInsuranceByPolicyNumberAsync(int patientId, string policyNumber)
        {
            try
            {
                _log.Information("Getting primary insurance by policy number for PatientId: {PatientId}, PolicyNumber(masked): {PolicyNumber}. User: {UserName} (Id: {UserId})", 
                    patientId, MaskSensitiveData(policyNumber), _currentUserService.UserName, _currentUserService.UserId);

                // استفاده از متد ریپازیتوری که واقعاً کار می‌کند
                var primaryInsurance = await _patientInsuranceRepository.GetPrimaryInsuranceByPolicyNumberAsync(patientId, policyNumber);
                return ServiceResult<PatientInsurance>.Successful(primaryInsurance);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error getting primary insurance by policy number: PatientId: {PatientId}, PolicyNumber(masked): {PolicyNumber}. User: {UserName} (Id: {UserId})", 
                    patientId, MaskSensitiveData(policyNumber), _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult<PatientInsurance>.Failed("خطا در دریافت بیمه اصلی");
            }
        }

        /// <summary>
        /// دریافت شماره بیمه پایه بیمار
        /// </summary>
        public async Task<ServiceResult<string>> GetPrimaryInsurancePolicyNumberAsync(int patientId)
        {
            try
            {
                _log.Information("Getting primary insurance policy number for PatientId: {PatientId}. User: {UserName} (Id: {UserId})", 
                    patientId, _currentUserService.UserName, _currentUserService.UserId);

                // دریافت بیمه اصلی بیمار
                var primaryInsurance = await _patientInsuranceRepository.GetPrimaryInsuranceByPatientIdAsync(patientId);
                if (primaryInsurance != null && !string.IsNullOrEmpty(primaryInsurance.PolicyNumber))
                {
                    _log.Information("Primary insurance policy number found for PatientId: {PatientId}, PolicyNumber(masked): {PolicyNumber}. User: {UserName} (Id: {UserId})", 
                        patientId, MaskSensitiveData(primaryInsurance.PolicyNumber), _currentUserService.UserName, _currentUserService.UserId);
                    return ServiceResult<string>.Successful(primaryInsurance.PolicyNumber);
                }
                else
                {
                    _log.Warning("No primary insurance found for PatientId: {PatientId}. User: {UserName} (Id: {UserId})", 
                        patientId, _currentUserService.UserName, _currentUserService.UserId);
                    return ServiceResult<string>.Failed("بیمه پایه برای این بیمار تعریف نشده است");
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error getting primary insurance policy number: PatientId: {PatientId}. User: {UserName} (Id: {UserId})", 
                    patientId, _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult<string>.Failed("خطا در دریافت شماره بیمه پایه");
            }
        }

        public async Task<ServiceResult<Dictionary<string, string>>> ValidatePatientInsuranceAsync(PatientInsuranceCreateEditViewModel model)
        {
            try
            {
                _log.Information("🏥 MEDICAL: Validating patient insurance for PatientId: {PatientId}, PolicyNumber(masked): {PolicyNumber}, PatientInsuranceId: {PatientInsuranceId}. User: {UserName} (Id: {UserId})", 
                    model.PatientId, MaskSensitiveData(model.PolicyNumber), model.PatientInsuranceId, _currentUserService.UserName, _currentUserService.UserId);

                var errors = new Dictionary<string, string>();
                
                // 🏥 Medical Environment: اعتبارسنجی فیلدهای الزامی
                if (model.PatientId <= 0)
                {
                    errors.Add("PatientId", "شناسه بیمار الزامی است");
                }
                
                if (model.InsurancePlanId <= 0)
                {
                    errors.Add("InsurancePlanId", "انتخاب طرح بیمه الزامی است");
                }
                
                if (string.IsNullOrWhiteSpace(model.PolicyNumber))
                {
                    errors.Add("PolicyNumber", "شماره بیمه الزامی است");
                }
                
                if (model.StartDate == DateTime.MinValue)
                {
                    errors.Add("StartDate", "تاریخ شروع الزامی است");
                }
                
                // 🏥 Medical Environment: اعتبارسنجی بیمه تکمیلی
                if (model.SupplementaryInsuranceProviderId.HasValue && !model.SupplementaryInsurancePlanId.HasValue)
                {
                    errors.Add("SupplementaryInsurancePlanId", "اگر بیمه‌گذار تکمیلی انتخاب شده، طرح بیمه تکمیلی نیز باید انتخاب شود");
                }
                
                if (!model.SupplementaryInsuranceProviderId.HasValue && model.SupplementaryInsurancePlanId.HasValue)
                {
                    errors.Add("SupplementaryInsuranceProviderId", "اگر طرح بیمه تکمیلی انتخاب شده، بیمه‌گذار تکمیلی نیز باید انتخاب شود");
                }

                // بررسی وجود شماره بیمه تکراری (فقط برای بیمه اصلی)
                if (model.IsPrimary)
                {
                    _log.Information("🏥 MEDICAL: Validating primary insurance policy number(masked): {PolicyNumber}", MaskSensitiveData(model.PolicyNumber));
                    var policyExistsResult = await DoesPolicyNumberExistAsync(model.PolicyNumber, model.PatientInsuranceId);
                    if (policyExistsResult.Success && policyExistsResult.Data)
                    {
                        errors.Add("PolicyNumber", "شماره بیمه قبلاً ثبت شده است.");
                    }
                }
                else
                {
                    _log.Information("🏥 MEDICAL: Validating supplementary insurance policy number(masked): {PolicyNumber}", MaskSensitiveData(model.PolicyNumber));
                    // برای بیمه تکمیلی، بررسی وجود شماره بیمه تکراری
                    var policyExistsResult = await DoesPolicyNumberExistAsync(model.PolicyNumber, model.PatientInsuranceId);
                    if (policyExistsResult.Success && policyExistsResult.Data)
                    {
                        errors.Add("PolicyNumber", "شماره بیمه تکمیلی قبلاً ثبت شده است.");
                    }
                }

                // بررسی وجود بیمه اصلی برای بیمار (اگر این بیمه اصلی است)
                if (model.IsPrimary)
                {
                    var primaryExistsResult = await DoesPrimaryInsuranceExistAsync(model.PatientId, model.PatientInsuranceId);
                    if (primaryExistsResult.Success && primaryExistsResult.Data)
                    {
                        errors.Add("IsPrimary", "این بیمار قبلاً بیمه اصلی دارد.");
                    }
                }

                // بررسی تداخل تاریخ‌ها (فقط برای بیمه اصلی)
                if (model.IsPrimary)
                {
                    var dateOverlapResult = await DoesDateOverlapExistAsync(
                        model.PatientId, model.StartDate, model.EndDate ?? DateTime.MaxValue, model.PatientInsuranceId);
                    if (dateOverlapResult.Success && dateOverlapResult.Data)
                    {
                        errors.Add("StartDate", "تاریخ‌های انتخاب شده با بیمه‌های موجود این بیمار تداخل دارد.");
                    }
                }
                else
                {
                    // 🚨 CRITICAL FIX: برای بیمه تکمیلی، بیمه پایه بیمار را بررسی کنیم (نه با PolicyNumber)
                    var primaryInsuranceResult = await GetPrimaryInsuranceByPatientAsync(model.PatientId);
                    if (!primaryInsuranceResult.Success || primaryInsuranceResult.Data == null)
                    {
                        errors.Add("PolicyNumber", "ابتدا باید بیمه پایه برای این بیمار تعریف شود.");
                    }
                    else
                    {
                        var primaryInsurance = primaryInsuranceResult.Data;
                        if (!primaryInsurance.IsActive)
                        {
                            errors.Add("StartDate", "بیمه پایه این بیمار غیرفعال است. ابتدا بیمه پایه را فعال کنید.");
                        }
                        else if (primaryInsurance.EndDate.HasValue && primaryInsurance.EndDate.Value < model.StartDate)
                        {
                            errors.Add("StartDate", "تاریخ شروع بیمه تکمیلی نمی‌تواند بعد از تاریخ پایان بیمه پایه باشد.");
                        }
                    }
                }

                return ServiceResult<Dictionary<string, string>>.Successful(errors);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error validating patient insurance for PatientId: {PatientId}", model.PatientId);
                return ServiceResult<Dictionary<string, string>>.Failed("خطا در اعتبارسنجی اطلاعات");
            }
        }

        public async Task<ServiceResult<List<PatientInsuranceIndexViewModel>>> GetPatientInsurancesByPatientAsync(int patientId)
        {
            try
            {
                _log.Information("Getting patient insurances by patient for PatientId: {PatientId}. User: {UserName} (Id: {UserId})", 
                    patientId, _currentUserService.UserName, _currentUserService.UserId);

                // دریافت بیمه‌های بیمار از ریپازیتوری
                var patientInsurances = await _patientInsuranceRepository.GetByPatientIdAsync(patientId);
                var viewModels = patientInsurances.Select(ConvertToIndexViewModel).ToList();
                
                return ServiceResult<List<PatientInsuranceIndexViewModel>>.Successful(viewModels);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error getting patient insurances by patient for PatientId: {PatientId}. User: {UserName} (Id: {UserId})", 
                    patientId, _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult<List<PatientInsuranceIndexViewModel>>.Failed("خطا در دریافت بیمه‌های بیمار");
            }
        }

        /// <summary>
        /// دریافت فقط بیمه‌های تکمیلی بیمار
        /// </summary>
        public async Task<ServiceResult<List<PatientInsuranceIndexViewModel>>> GetSupplementaryInsurancesByPatientAsync(int patientId)
        {
            try
            {
                _log.Information("Getting supplementary insurances by patient for PatientId: {PatientId}. User: {UserName} (Id: {UserId})", 
                    patientId, _currentUserService.UserName, _currentUserService.UserId);

                // دریافت فقط بیمه‌های تکمیلی (غیر اصلی) از ریپازیتوری
                var supplementaryInsurances = await _patientInsuranceRepository.GetSupplementaryByPatientIdAsync(patientId);
                
                // 🚨 DEBUG: بررسی رکوردهای دریافتی
                _log.Information("🔍 DEBUG: Repository returned {Count} records for PatientId: {PatientId}", 
                    supplementaryInsurances.Count, patientId);
                
                foreach (var insurance in supplementaryInsurances)
                {
                    _log.Information("🔍 DEBUG: Record - PatientInsuranceId: {Id}, IsPrimary: {IsPrimary}, SupplementaryProviderId: {SuppProviderId}, SupplementaryPlanId: {SuppPlanId}", 
                        insurance.PatientInsuranceId, insurance.IsPrimary, insurance.SupplementaryInsuranceProviderId, insurance.SupplementaryInsurancePlanId);
                }
                
                var viewModels = supplementaryInsurances.Select(ConvertToIndexViewModel).ToList();
                
                // 🚨 DEBUG: بررسی ViewModels
                foreach (var viewModel in viewModels)
                {
                    _log.Information("🔍 DEBUG: ViewModel - PatientInsuranceId: {Id}, HasSupplementaryInsurance: {HasSupp}, SupplementaryProviderId: {SuppProviderId}, SupplementaryPlanId: {SuppPlanId}", 
                        viewModel.PatientInsuranceId, viewModel.HasSupplementaryInsurance, viewModel.SupplementaryInsuranceProviderId, viewModel.SupplementaryInsurancePlanId);
                }
                
                _log.Information("Found {Count} supplementary insurances for PatientId: {PatientId}. User: {UserName} (Id: {UserId})", 
                    viewModels.Count, patientId, _currentUserService.UserName, _currentUserService.UserId);
                
                return ServiceResult<List<PatientInsuranceIndexViewModel>>.Successful(viewModels);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error getting supplementary insurances by patient for PatientId: {PatientId}. User: {UserName} (Id: {UserId})", 
                    patientId, _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult<List<PatientInsuranceIndexViewModel>>.Failed("خطا در دریافت بیمه‌های تکمیلی بیمار");
            }
        }

        public async Task<ServiceResult<PatientInsuranceDetailsViewModel>> GetPrimaryInsuranceByPatientAsync(int patientId)
        {
            try
            {
                _log.Information("Getting primary insurance by patient for PatientId: {PatientId}. User: {UserName} (Id: {UserId})", 
                    patientId, _currentUserService.UserName, _currentUserService.UserId);

                // دریافت بیمه اصلی بیمار از ریپازیتوری
                var primaryInsurance = await _patientInsuranceRepository.GetPrimaryByPatientIdAsync(patientId);
                if (primaryInsurance == null)
                {
                    return ServiceResult<PatientInsuranceDetailsViewModel>.Failed("بیمه اصلی بیمار یافت نشد");
                }

                var viewModel = ConvertToDetailsViewModel(primaryInsurance);
                return ServiceResult<PatientInsuranceDetailsViewModel>.Successful(viewModel);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error getting primary insurance by patient for PatientId: {PatientId}. User: {UserName} (Id: {UserId})", 
                    patientId, _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult<PatientInsuranceDetailsViewModel>.Failed("خطا در دریافت بیمه اصلی بیمار");
            }
        }


        public async Task<ServiceResult> SetPrimaryInsuranceAsync(int patientInsuranceId)
        {
            try
            {
                _log.Information("Setting primary insurance for PatientInsuranceId: {PatientInsuranceId}. User: {UserName} (Id: {UserId})", 
                    patientInsuranceId, _currentUserService.UserName, _currentUserService.UserId);

                // دریافت بیمه بیمار
                var patientInsurance = await _patientInsuranceRepository.GetByIdAsync(patientInsuranceId);
                if (patientInsurance == null)
                {
                    _log.Warning("بیمه بیمار یافت نشد. PatientInsuranceId: {PatientInsuranceId}. User: {UserName} (Id: {UserId})", 
                        patientInsuranceId, _currentUserService.UserName, _currentUserService.UserId);
                    return ServiceResult.Failed("بیمه بیمار یافت نشد");
                }

                // بررسی فعال بودن بیمه
                if (!patientInsurance.IsActive)
                {
                    _log.Warning("بیمه غیرفعال نمی‌تواند بیمه اصلی باشد. PatientInsuranceId: {PatientInsuranceId}. User: {UserName} (Id: {UserId})", 
                        patientInsuranceId, _currentUserService.UserName, _currentUserService.UserId);
                    return ServiceResult.Failed("بیمه غیرفعال نمی‌تواند بیمه اصلی باشد");
                }

                // استفاده از Transaction برای اطمینان از consistency
                using (var transaction = await _patientInsuranceRepository.BeginTransactionAsync())
                {
                    try
                    {
                        // 1. حذف وضعیت اصلی از سایر بیمه‌های بیمار
                        var otherInsurances = await _patientInsuranceRepository.GetByPatientIdAsync(patientInsurance.PatientId);
                        foreach (var insurance in otherInsurances.Where(i => i.PatientInsuranceId != patientInsuranceId && i.IsPrimary))
                        {
                            insurance.IsPrimary = false;
                            insurance.UpdatedByUserId = _currentUserService.GetCurrentUserId();
                            insurance.UpdatedAt = DateTime.UtcNow;
                            _patientInsuranceRepository.Update(insurance);
                            
                            _log.Information("بیمه اصلی قبلی غیرفعال شد. PatientInsuranceId: {PatientInsuranceId}. User: {UserName} (Id: {UserId})", 
                                insurance.PatientInsuranceId, _currentUserService.UserName, _currentUserService.UserId);
                        }

                        // 2. تنظیم بیمه جدید به عنوان اصلی
                        patientInsurance.IsPrimary = true;
                        patientInsurance.UpdatedByUserId = _currentUserService.GetCurrentUserId();
                        patientInsurance.UpdatedAt = DateTime.UtcNow;
                        _patientInsuranceRepository.Update(patientInsurance);

                        // 3. Commit Transaction
                        transaction.Commit();

                        _log.Information("بیمه اصلی با موفقیت تنظیم شد. PatientInsuranceId: {PatientInsuranceId}, PatientId: {PatientId}. User: {UserName} (Id: {UserId})", 
                            patientInsuranceId, patientInsurance.PatientId, _currentUserService.UserName, _currentUserService.UserId);

                        return ServiceResult.Successful("بیمه اصلی با موفقیت تنظیم شد");
                    }
                    catch (Exception ex)
                    {
                        // Rollback Transaction در صورت خطا
                        transaction.Rollback();
                        _log.Error(ex, "خطا در Transaction تنظیم بیمه اصلی. PatientInsuranceId: {PatientInsuranceId}. User: {UserName} (Id: {UserId})", 
                            patientInsuranceId, _currentUserService.UserName, _currentUserService.UserId);
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                _log.Error(ex, "خطای سیستمی در تنظیم بیمه اصلی. PatientInsuranceId: {PatientInsuranceId}. User: {UserName} (Id: {UserId})", 
                    patientInsuranceId, _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult.Failed("خطا در تنظیم بیمه اصلی");
            }
        }

        public async Task<ServiceResult<bool>> IsPatientInsuranceValidAsync(int patientId, DateTime checkDate)
        {
            try
            {
                _log.Information("Checking if patient insurance is valid for PatientId: {PatientId}, CheckDate: {CheckDate}. User: {UserName} (Id: {UserId})", 
                    patientId, checkDate, _currentUserService.UserName, _currentUserService.UserId);

                // بررسی وجود بیمه فعال برای تاریخ مشخص شده
                var activeInsurances = await _patientInsuranceRepository.GetActiveByPatientIdAsync(patientId);
                var isValid = activeInsurances.Any(insurance => 
                    insurance.IsActive && 
                    insurance.StartDate <= checkDate && 
                    (insurance.EndDate == null || insurance.EndDate >= checkDate));

                return ServiceResult<bool>.Successful(isValid);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error checking patient insurance validity for PatientId: {PatientId}, CheckDate: {CheckDate}. User: {UserName} (Id: {UserId})", 
                    patientId, checkDate, _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult<bool>.Failed("خطا در بررسی اعتبار بیمه بیمار");
            }
        }

        /// <summary>
        /// دریافت بیمه بیمار برای ویرایش
        /// </summary>
        public async Task<ServiceResult<PatientInsuranceCreateEditViewModel>> GetPatientInsuranceForEditAsync(int patientInsuranceId)
        {
            _log.Information(
                "درخواست دریافت بیمه بیمار برای ویرایش. PatientInsuranceId: {PatientInsuranceId}. کاربر: {UserName} (شناسه: {UserId})",
                patientInsuranceId, _currentUserService.UserName, _currentUserService.UserId);

            try
            {
                // دریافت بیمه بیمار با اطلاعات مرتبط
                var entity = await _patientInsuranceRepository.GetByIdWithDetailsAsync(patientInsuranceId);
                
                if (entity == null)
                {
                    _log.Warning(
                        "بیمه بیمار با شناسه {PatientInsuranceId} یافت نشد. کاربر: {UserName} (شناسه: {UserId})",
                        patientInsuranceId, _currentUserService.UserName, _currentUserService.UserId);
                    
                    return ServiceResult<PatientInsuranceCreateEditViewModel>.Failed(
                        "بیمه بیمار یافت نشد.",
                        "PATIENT_INSURANCE_NOT_FOUND",
                        ErrorCategory.NotFound,
                        SecurityLevel.Medium);
                }

                if (entity.IsDeleted)
                {
                    _log.Warning(
                        "تلاش برای دسترسی به بیمه بیمار حذف شده. PatientInsuranceId: {PatientInsuranceId}. کاربر: {UserName} (شناسه: {UserId})",
                        patientInsuranceId, _currentUserService.UserName, _currentUserService.UserId);
                    
                    return ServiceResult<PatientInsuranceCreateEditViewModel>.Failed(
                        "بیمه بیمار یافت نشد.",
                        "PATIENT_INSURANCE_DELETED",
                        ErrorCategory.NotFound,
                        SecurityLevel.High);
                }

                // تبدیل Entity به ViewModel
                var viewModel = PatientInsuranceCreateEditViewModel.FromEntity(entity);
                
                _log.Information(
                    "بیمه بیمار برای ویرایش با موفقیت دریافت شد. PatientInsuranceId: {PatientInsuranceId}, PolicyNumber: {PolicyNumber}. کاربر: {UserName} (شناسه: {UserId})",
                    patientInsuranceId, entity.PolicyNumber, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<PatientInsuranceCreateEditViewModel>.Successful(
                    viewModel,
                    "بیمه بیمار با موفقیت دریافت شد.",
                    "GetPatientInsuranceForEdit",
                    _currentUserService.UserId,
                    _currentUserService.UserName,
                    securityLevel: SecurityLevel.Low);
            }
            catch (Exception ex)
            {
                _log.Error(
                    ex,
                    "خطای سیستمی در دریافت بیمه بیمار برای ویرایش. PatientInsuranceId: {PatientInsuranceId}. کاربر: {UserName} (شناسه: {UserId})",
                    patientInsuranceId, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<PatientInsuranceCreateEditViewModel>.Failed(
                    "خطا در دریافت بیمه بیمار. لطفاً دوباره تلاش کنید.",
                    "GET_PATIENT_INSURANCE_FOR_EDIT_ERROR",
                    ErrorCategory.General,
                    SecurityLevel.High);
            }
        }

        /// <summary>
        /// متد debug برای بررسی تعداد رکوردها
        /// </summary>
        public async Task<ServiceResult<int>> GetTotalRecordsCountAsync()
        {
            try
            {
                var result = await _patientInsuranceRepository.GetTotalRecordsCountAsync();
                return result;
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error getting total records count");
                return ServiceResult<int>.Failed("خطا در دریافت تعداد رکوردها");
            }
        }

        public async Task<ServiceResult<List<object>>> GetSimpleListAsync()
        {
            try
            {
                var result = await _patientInsuranceRepository.GetSimpleListAsync();
                return result;
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error getting simple list");
                return ServiceResult<List<object>>.Failed("خطا در دریافت لیست ساده");
            }
        }

        #endregion

        #region CRUD Operations

        /// <summary>
        /// دریافت لیست بیمه‌های بیماران با صفحه‌بندی و جستجو
        /// </summary>
        public async Task<ServiceResult<PagedResult<PatientInsuranceIndexViewModel>>> GetPagedAsync(
            string searchTerm = null,
            int? providerId = null,
            int? planId = null,
            bool? isPrimary = null,
            bool? isActive = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int pageNumber = 1,
            int pageSize = 20)
        {
            try
            {
                _log.Information(
                    "درخواست دریافت لیست بیمه‌های بیماران. SearchTerm: {SearchTerm}, ProviderId: {ProviderId}, PlanId: {PlanId}, IsPrimary: {IsPrimary}, IsActive: {IsActive}, PageNumber: {PageNumber}, PageSize: {PageSize}. کاربر: {UserName} (شناسه: {UserId})",
                    searchTerm, providerId, planId, isPrimary, isActive, pageNumber, pageSize, _currentUserService.UserName, _currentUserService.UserId);

                // استفاده از متد بهینه‌سازی شده
                _log.Information("Calling GetPagedOptimizedAsync with params: SearchTerm={SearchTerm}, ProviderId={ProviderId}, PlanId={PlanId}, IsPrimary={IsPrimary}, IsActive={IsActive}, PageNumber={PageNumber}, PageSize={PageSize}", 
                    searchTerm, providerId, planId, isPrimary, isActive, pageNumber, pageSize);
                
                var result = await _patientInsuranceRepository.GetPagedOptimizedAsync(
                    null, searchTerm, providerId, planId, isPrimary, isActive, fromDate, toDate, pageNumber, pageSize);
                
                _log.Information("GetPagedOptimizedAsync result: Success={Success}, DataNull={DataNull}, ItemsCount={ItemsCount}", 
                    result.Success, result.Data == null, result.Data?.Items?.Count ?? 0);

                if (!result.Success || result.Data == null)
                {
                    _log.Warning("خطا در دریافت لیست بیمه‌های بیماران از ریپازیتوری. Success: {Success}, Data: {Data}. کاربر: {UserName} (شناسه: {UserId})", 
                        result.Success, result.Data != null, _currentUserService.UserName, _currentUserService.UserId);
                    
                    return ServiceResult<PagedResult<PatientInsuranceIndexViewModel>>.Failed(
                        "خطا در دریافت لیست بیمه‌های بیماران از پایگاه داده");
                }

                _log.Information(
                    "لیست بیمه‌های بیماران با موفقیت دریافت شد. تعداد: {Count} از {Total}. کاربر: {UserName} (شناسه: {UserId})",
                    result.Data.Items.Count, result.Data.TotalItems, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<PagedResult<PatientInsuranceIndexViewModel>>.Successful(
                    result.Data,
                    "لیست بیمه‌های بیماران با موفقیت دریافت شد.");
            }
            catch (Exception ex)
            {
                _log.Error(ex,
                    "خطا در دریافت لیست بیمه‌های بیماران. SearchTerm: {SearchTerm}, ProviderId: {ProviderId}, PlanId: {PlanId}, IsPrimary: {IsPrimary}, IsActive: {IsActive}, PageNumber: {PageNumber}, PageSize: {PageSize}. کاربر: {UserName} (شناسه: {UserId})",
                    searchTerm, providerId, planId, isPrimary, isActive, pageNumber, pageSize, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<PagedResult<PatientInsuranceIndexViewModel>>.Failed(
                    "خطا در دریافت لیست بیمه‌های بیماران.");
            }
        }

        /// <summary>
        /// دریافت بیمه بیمار بر اساس شناسه
        /// </summary>
        public async Task<ServiceResult<PatientInsuranceDetailsViewModel>> GetByIdAsync(int id)
        {
            try
            {
                _log.Information(
                    "درخواست دریافت بیمه بیمار. Id: {Id}. کاربر: {UserName} (شناسه: {UserId})",
                    id, _currentUserService.UserName, _currentUserService.UserId);

                var patientInsurance = await _patientInsuranceRepository.GetByIdAsync(id);

                if (patientInsurance == null)
                {
                    _log.Warning(
                        "بیمه بیمار یافت نشد. Id: {Id}. کاربر: {UserName} (شناسه: {UserId})",
                        id, _currentUserService.UserName, _currentUserService.UserId);

                    return ServiceResult<PatientInsuranceDetailsViewModel>.Failed("بیمه بیمار یافت نشد");
                }

                var viewModel = ConvertToDetailsViewModel(patientInsurance);

                _log.Information(
                    "بیمه بیمار با موفقیت دریافت شد. Id: {Id}, PatientId: {PatientId}, PolicyNumber: {PolicyNumber}. کاربر: {UserName} (شناسه: {UserId})",
                    id, patientInsurance.PatientId, patientInsurance.PolicyNumber, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<PatientInsuranceDetailsViewModel>.Successful(viewModel);
            }
            catch (Exception ex)
            {
                _log.Error(ex,
                    "خطا در دریافت بیمه بیمار. Id: {Id}. کاربر: {UserName} (شناسه: {UserId})",
                    id, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<PatientInsuranceDetailsViewModel>.Failed("خطا در دریافت بیمه بیمار");
            }
        }

        /// <summary>
        /// ایجاد بیمه بیمار جدید
        /// </summary>
        public async Task<ServiceResult<int>> CreateAsync(PatientInsuranceCreateEditViewModel model)
        {
            try
            {
                _log.Information(
                    "درخواست ایجاد بیمه بیمار جدید. PatientId: {PatientId}, PolicyNumber: {PolicyNumber}, PlanId: {PlanId}. کاربر: {UserName} (شناسه: {UserId})",
                    model.PatientId, model.PolicyNumber, model.InsurancePlanId, _currentUserService.UserName, _currentUserService.UserId);

                // تبدیل به Entity
                var patientInsurance = ConvertToEntity(model);
                patientInsurance.IsActive = true;
                patientInsurance.IsDeleted = false;
                
                // 🚨 CRITICAL FIX: اضافه کردن فیلدهای Audit
                patientInsurance.CreatedAt = DateTime.UtcNow;
                patientInsurance.CreatedByUserId = _currentUserService.UserId;
                patientInsurance.UpdatedAt = null;
                patientInsurance.UpdatedByUserId = null;
                
                // 🏥 Medical Environment: بررسی مقادیر Entity قبل از ذخیره
                _log.Information("🏥 MEDICAL: === ENTITY VALUES BEFORE SAVE ===");
                _log.Information("🏥 MEDICAL: Entity.InsuranceProviderId: {InsuranceProviderId}", patientInsurance.InsuranceProviderId);
                _log.Information("🏥 MEDICAL: Entity.InsurancePlanId: {InsurancePlanId}", patientInsurance.InsurancePlanId);
                _log.Information("🏥 MEDICAL: Entity.PatientId: {PatientId}", patientInsurance.PatientId);
                _log.Information("🏥 MEDICAL: Entity.PolicyNumber(masked): {PolicyNumber}", MaskSensitiveData(patientInsurance.PolicyNumber));
                _log.Information("🏥 MEDICAL: Entity.IsPrimary: {IsPrimary}", patientInsurance.IsPrimary);
                _log.Information("🏥 MEDICAL: Entity.IsActive: {IsActive}", patientInsurance.IsActive);

                // تنظیم خودکار Priority بر اساس نوع بیمه
                if (model.IsPrimary)
                {
                    patientInsurance.Priority = InsurancePriority.Primary; // بیمه اصلی همیشه اولویت Primary
                }
                else
                {
                    // برای بیمه تکمیلی، اولویت را بر اساس تعداد بیمه‌های موجود تنظیم کن
                    var existingInsurances = await _patientInsuranceRepository.GetByPatientIdAsync(model.PatientId);
                    var existingPriorities = existingInsurances.Where(pi => !pi.IsPrimary).Select(pi => pi.Priority);
                    patientInsurance.Priority = InsurancePriorityHelper.GetNextSupplementaryPriority(existingPriorities);
                }

                // ذخیره در Repository
                _patientInsuranceRepository.Add(patientInsurance);
                await _patientInsuranceRepository.SaveChangesAsync();

                _log.Information(
                    "بیمه بیمار جدید با موفقیت ایجاد شد. Id: {Id}, PatientId: {PatientId}, PolicyNumber: {PolicyNumber}. کاربر: {UserName} (شناسه: {UserId})",
                    patientInsurance.PatientInsuranceId, patientInsurance.PatientId, patientInsurance.PolicyNumber, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<int>.Successful(patientInsurance.PatientInsuranceId);
            }
            catch (Exception ex)
            {
                _log.Error(ex,
                    "خطا در ایجاد بیمه بیمار جدید. PatientId: {PatientId}, PolicyNumber: {PolicyNumber}, PlanId: {PlanId}. کاربر: {UserName} (شناسه: {UserId})",
                    model.PatientId, model.PolicyNumber, model.InsurancePlanId, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<int>.Failed("خطا در ایجاد بیمه بیمار جدید");
            }
        }

        /// <summary>
        /// به‌روزرسانی بیمه بیمار
        /// </summary>
        public async Task<ServiceResult<bool>> UpdateAsync(PatientInsuranceCreateEditViewModel model)
        {
            try
            {
                _log.Information(
                    "درخواست به‌روزرسانی بیمه بیمار. Id: {Id}, PatientId: {PatientId}, PolicyNumber: {PolicyNumber}. کاربر: {UserName} (شناسه: {UserId})",
                    model.PatientInsuranceId, model.PatientId, model.PolicyNumber, _currentUserService.UserName, _currentUserService.UserId);

                var existingPatientInsurance = await _patientInsuranceRepository.GetByIdAsync(model.PatientInsuranceId);

                if (existingPatientInsurance == null)
                {
                    _log.Warning(
                        "بیمه بیمار برای به‌روزرسانی یافت نشد. Id: {Id}. کاربر: {UserName} (شناسه: {UserId})",
                        model.PatientInsuranceId, _currentUserService.UserName, _currentUserService.UserId);

                    return ServiceResult<bool>.Failed("بیمه بیمار یافت نشد");
                }

                // 🏥 Medical Environment: به‌روزرسانی Entity با تمام فیلدها
                existingPatientInsurance.PatientId = model.PatientId;
                existingPatientInsurance.InsurancePlanId = model.InsurancePlanId;
                existingPatientInsurance.PolicyNumber = model.PolicyNumber;
                existingPatientInsurance.IsPrimary = model.IsPrimary;
                existingPatientInsurance.StartDate = model.StartDate;
                existingPatientInsurance.EndDate = model.EndDate;
                existingPatientInsurance.IsActive = model.IsActive;
                
                // 🏥 Medical Environment: به‌روزرسانی فیلدهای بیمه تکمیلی
                existingPatientInsurance.SupplementaryInsuranceProviderId = model.SupplementaryInsuranceProviderId;
                existingPatientInsurance.SupplementaryInsurancePlanId = model.SupplementaryInsurancePlanId;
                existingPatientInsurance.SupplementaryPolicyNumber = model.SupplementaryPolicyNumber;
                
                // 🚨 CRITICAL FIX: اضافه کردن فیلدهای Audit برای Update
                existingPatientInsurance.UpdatedAt = DateTime.UtcNow;
                existingPatientInsurance.UpdatedByUserId = _currentUserService.UserId;

                // ذخیره در Repository
                _patientInsuranceRepository.Update(existingPatientInsurance);
                await _patientInsuranceRepository.SaveChangesAsync();

                _log.Information(
                    "بیمه بیمار با موفقیت به‌روزرسانی شد. Id: {Id}, PatientId: {PatientId}, PolicyNumber: {PolicyNumber}. کاربر: {UserName} (شناسه: {UserId})",
                    existingPatientInsurance.PatientInsuranceId, existingPatientInsurance.PatientId, existingPatientInsurance.PolicyNumber, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<bool>.Successful(true);
            }
            catch (Exception ex)
            {
                _log.Error(ex,
                    "خطا در به‌روزرسانی بیمه بیمار. Id: {Id}, PatientId: {PatientId}, PolicyNumber: {PolicyNumber}. کاربر: {UserName} (شناسه: {UserId})",
                    model.PatientInsuranceId, model.PatientId, model.PolicyNumber, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<bool>.Failed("خطا در به‌روزرسانی بیمه بیمار");
            }
        }

        /// <summary>
        /// حذف نرم بیمه بیمار
        /// </summary>
        public async Task<ServiceResult<bool>> DeleteAsync(int id)
        {
            try
            {
                _log.Information(
                    "درخواست حذف بیمه بیمار. Id: {Id}. کاربر: {UserName} (شناسه: {UserId})",
                    id, _currentUserService.UserName, _currentUserService.UserId);

                var patientInsurance = await _patientInsuranceRepository.GetByIdAsync(id);

                if (patientInsurance == null)
                {
                    _log.Warning(
                        "بیمه بیمار برای حذف یافت نشد. Id: {Id}. کاربر: {UserName} (شناسه: {UserId})",
                        id, _currentUserService.UserName, _currentUserService.UserId);

                    return ServiceResult<bool>.Failed("بیمه بیمار یافت نشد");
                }

                // 🚨 CRITICAL FIX: حذف نرم صحیح (Soft Delete)
                patientInsurance.IsDeleted = true;
                patientInsurance.IsActive = false;
                patientInsurance.UpdatedAt = DateTime.UtcNow;
                patientInsurance.UpdatedByUserId = _currentUserService.UserId;
                
                _patientInsuranceRepository.Update(patientInsurance);
                await _patientInsuranceRepository.SaveChangesAsync();

                _log.Information(
                    "بیمه بیمار با موفقیت حذف شد. Id: {Id}, PatientId: {PatientId}, PolicyNumber: {PolicyNumber}. کاربر: {UserName} (شناسه: {UserId})",
                    id, patientInsurance.PatientId, patientInsurance.PolicyNumber, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<bool>.Successful(true);
            }
            catch (Exception ex)
            {
                _log.Error(ex,
                    "خطا در حذف بیمه بیمار. Id: {Id}. کاربر: {UserName} (شناسه: {UserId})",
                    id, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<bool>.Failed("خطا در حذف بیمه بیمار");
            }
        }

        #endregion

        #region Business Logic Methods

        /// <summary>
        /// دریافت بیمه‌های فعال بیمار
        /// </summary>
        public async Task<ServiceResult<List<PatientInsuranceLookupViewModel>>> GetActiveByPatientIdAsync(int patientId)
        {
            try
            {
                _log.Information(
                    "درخواست دریافت بیمه‌های فعال بیمار. PatientId: {PatientId}. کاربر: {UserName} (شناسه: {UserId})",
                    patientId, _currentUserService.UserName, _currentUserService.UserId);

                var patientInsurances = await _patientInsuranceRepository.GetActiveByPatientIdAsync(patientId);
                
                _log.Information("🔍 DEBUG: Repository returned {Count} patient insurances for PatientId: {PatientId}", 
                    patientInsurances?.Count ?? 0, patientId);

                var viewModels = patientInsurances?.Select(ConvertToLookupViewModel).ToList() ?? new List<PatientInsuranceLookupViewModel>();

                _log.Information(
                    "بیمه‌های فعال بیمار با موفقیت دریافت شد. PatientId: {PatientId}, Count: {Count}. کاربر: {UserName} (شناسه: {UserId})",
                    patientId, viewModels.Count, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<List<PatientInsuranceLookupViewModel>>.Successful(
                    viewModels,
                    "بیمه‌های فعال بیمار با موفقیت دریافت شد.",
 "GetActivePatientInsurances",
 _currentUserService.UserId,
 _currentUserService.UserName,
                    securityLevel: SecurityLevel.Low);
            }
            catch (Exception ex)
            {
                _log.Error(ex,
                    "خطا در دریافت بیمه‌های فعال بیمار. PatientId: {PatientId}. کاربر: {UserName} (شناسه: {UserId})",
                    patientId, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<List<PatientInsuranceLookupViewModel>>.Failed("خطا در دریافت بیمه‌های فعال بیمار");
            }
        }

        /// <summary>
        /// دریافت بیمه اصلی بیمار
        /// </summary>
        public async Task<ServiceResult<PatientInsuranceLookupViewModel>> GetPrimaryByPatientIdAsync(int patientId)
        {
            try
            {
                _log.Information(
                    "درخواست دریافت بیمه اصلی بیمار. PatientId: {PatientId}. کاربر: {UserName} (شناسه: {UserId})",
                    patientId, _currentUserService.UserName, _currentUserService.UserId);

                var primaryInsurance = await _patientInsuranceRepository.GetPrimaryByPatientIdAsync(patientId);

                if (primaryInsurance == null)
                {
                    _log.Warning(
                        "بیمه اصلی بیمار یافت نشد. PatientId: {PatientId}. کاربر: {UserName} (شناسه: {UserId})",
                        patientId, _currentUserService.UserName, _currentUserService.UserId);

                    return ServiceResult<PatientInsuranceLookupViewModel>.Failed("بیمه اصلی بیمار یافت نشد");
                }

                var viewModel = ConvertToLookupViewModel(primaryInsurance);

                _log.Information(
                    "بیمه اصلی بیمار با موفقیت دریافت شد. PatientId: {PatientId}, PolicyNumber: {PolicyNumber}. کاربر: {UserName} (شناسه: {UserId})",
                    patientId, primaryInsurance.PolicyNumber, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<PatientInsuranceLookupViewModel>.Successful(
                    viewModel,
                    "بیمه اصلی بیمار با موفقیت دریافت شد.",
 "GetPrimaryPatientInsurance",
 _currentUserService.UserId,
 _currentUserService.UserName,
                    securityLevel: SecurityLevel.Low);
            }
            catch (Exception ex)
            {
                _log.Error(ex,
                    "خطا در دریافت بیمه اصلی بیمار. PatientId: {PatientId}. کاربر: {UserName} (شناسه: {UserId})",
                    patientId, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<PatientInsuranceLookupViewModel>.Failed("خطا در دریافت بیمه اصلی بیمار");
            }
        }

        /// <summary>
        /// دریافت بیمه‌های تکمیلی بیمار
        /// </summary>
        public async Task<ServiceResult<List<PatientInsuranceLookupViewModel>>> GetSupplementaryByPatientIdAsync(int patientId)
        {
            try
            {
                _log.Information(
                    "درخواست دریافت بیمه‌های تکمیلی بیمار. PatientId: {PatientId}. کاربر: {UserName} (شناسه: {UserId})",
                    patientId, _currentUserService.UserName, _currentUserService.UserId);

                var supplementaryInsurances = await _patientInsuranceRepository.GetSupplementaryByPatientIdAsync(patientId);

                var viewModels = supplementaryInsurances.Select(ConvertToLookupViewModel).ToList();

                _log.Information(
                    "بیمه‌های تکمیلی بیمار با موفقیت دریافت شد. PatientId: {PatientId}, Count: {Count}. کاربر: {UserName} (شناسه: {UserId})",
                    patientId, viewModels.Count, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<List<PatientInsuranceLookupViewModel>>.Successful(
                    viewModels,
                    "بیمه‌های تکمیلی بیمار با موفقیت دریافت شد.",
 "GetSupplementaryPatientInsurances",
 _currentUserService.UserId,
 _currentUserService.UserName,
                    securityLevel: SecurityLevel.Low);
            }
            catch (Exception ex)
            {
                _log.Error(ex,
                    "خطا در دریافت بیمه‌های تکمیلی بیمار. PatientId: {PatientId}. کاربر: {UserName} (شناسه: {UserId})",
                    patientId, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<List<PatientInsuranceLookupViewModel>>.Failed("خطا در دریافت بیمه‌های تکمیلی بیمار");
            }
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// تبدیل Entity به Index ViewModel
        /// 🏥 Medical Environment: پشتیبانی از بیمه‌های تکمیلی
        /// </summary>
        private PatientInsuranceIndexViewModel ConvertToIndexViewModel(PatientInsurance patientInsurance)
        {
            if (patientInsurance == null) return null;

            return new PatientInsuranceIndexViewModel
            {
                PatientInsuranceId = patientInsurance.PatientInsuranceId,
                PatientId = patientInsurance.PatientId,
                PatientName = patientInsurance.Patient != null ? $"{patientInsurance.Patient.FirstName} {patientInsurance.Patient.LastName}".Trim() : null,
                PatientCode = patientInsurance.Patient?.PatientCode ?? "",
                InsurancePlanId = patientInsurance.InsurancePlanId,
                InsurancePlanName = patientInsurance.InsurancePlan?.Name,
                InsuranceProviderName = patientInsurance.InsurancePlan?.InsuranceProvider?.Name,
                PolicyNumber = patientInsurance.PolicyNumber,
                IsPrimary = patientInsurance.IsPrimary,
                StartDate = patientInsurance.StartDate,
                EndDate = patientInsurance.EndDate,
                StartDateShamsi = patientInsurance.StartDate.ToPersianDate(),
                EndDateShamsi = patientInsurance.EndDate.ToPersianDate(),
                IsActive = patientInsurance.IsActive,
                CoveragePercent = patientInsurance.SupplementaryInsurancePlan?.CoveragePercent ?? patientInsurance.InsurancePlan?.CoveragePercent ?? 0,
                // 🏥 Medical Environment: فیلدهای بیمه تکمیلی
                SupplementaryInsuranceProviderId = patientInsurance.SupplementaryInsuranceProviderId,
                SupplementaryInsuranceProviderName = patientInsurance.SupplementaryInsuranceProvider?.Name,
                SupplementaryInsurancePlanId = patientInsurance.SupplementaryInsurancePlanId,
                SupplementaryInsurancePlanName = patientInsurance.SupplementaryInsurancePlan?.Name,
                SupplementaryPolicyNumber = patientInsurance.SupplementaryPolicyNumber,
                HasSupplementaryInsurance = patientInsurance.SupplementaryInsuranceProviderId.HasValue && 
                                            patientInsurance.SupplementaryInsurancePlanId.HasValue
            };
        }

        /// <summary>
        /// تبدیل Entity به Details ViewModel
        /// </summary>
        private PatientInsuranceDetailsViewModel ConvertToDetailsViewModel(PatientInsurance patientInsurance)
        {
            if (patientInsurance == null) return null;

            return new PatientInsuranceDetailsViewModel
            {
                PatientInsuranceId = patientInsurance.PatientInsuranceId,
                PatientId = patientInsurance.PatientId,
                PatientName = patientInsurance.Patient != null ? $"{patientInsurance.Patient.FirstName} {patientInsurance.Patient.LastName}".Trim() : null,
                PatientCode = patientInsurance.Patient?.PatientCode ?? "",
                InsurancePlanId = patientInsurance.InsurancePlanId,
                InsurancePlanName = patientInsurance.InsurancePlan?.Name,
                InsuranceProviderName = patientInsurance.InsurancePlan?.InsuranceProvider?.Name,
                PolicyNumber = patientInsurance.PolicyNumber,
                IsPrimary = patientInsurance.IsPrimary,
                StartDate = patientInsurance.StartDate,
                EndDate = patientInsurance.EndDate,
                StartDateShamsi = patientInsurance.StartDate.ToPersianDate(),
                EndDateShamsi = patientInsurance.EndDate.ToPersianDate(),
                IsActive = patientInsurance.IsActive,
                CoveragePercent = patientInsurance.InsurancePlan?.CoveragePercent ?? 0,
                Deductible = patientInsurance.InsurancePlan?.Deductible ?? 0,
                CreatedAt = patientInsurance.CreatedAt,
                UpdatedAt = patientInsurance.UpdatedAt,
                CreatedAtShamsi = patientInsurance.CreatedAt.ToPersianDateTime(),
                UpdatedAtShamsi = patientInsurance.UpdatedAt.HasValue ? patientInsurance.UpdatedAt.Value.ToPersianDateTime() : null,
                CreatedByUserName = patientInsurance.CreatedByUser != null ? $"{patientInsurance.CreatedByUser.FirstName} {patientInsurance.CreatedByUser.LastName}".Trim() : null,
                UpdatedByUserName = patientInsurance.UpdatedByUser != null ? $"{patientInsurance.UpdatedByUser.FirstName} {patientInsurance.UpdatedByUser.LastName}".Trim() : null
            };
        }

        /// <summary>
        /// تبدیل CreateEdit ViewModel به Entity
        /// </summary>
        private PatientInsurance ConvertToEntity(PatientInsuranceCreateEditViewModel model)
        {
            if (model == null) return null;

            return new PatientInsurance
            {
                PatientInsuranceId = model.PatientInsuranceId,
                PatientId = model.PatientId,
                InsurancePlanId = model.InsurancePlanId,
                InsuranceProviderId = model.InsuranceProviderId, // 🚨 CRITICAL FIX: اضافه کردن InsuranceProviderId
                PolicyNumber = model.PolicyNumber,
                IsPrimary = model.IsPrimary,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                IsActive = model.IsActive,
                Priority = model.Priority,
                // 🚨 CRITICAL FIX: اضافه کردن فیلدهای تکمیلی
                SupplementaryInsuranceProviderId = model.SupplementaryInsuranceProviderId,
                SupplementaryInsurancePlanId = model.SupplementaryInsurancePlanId,
                SupplementaryPolicyNumber = model.SupplementaryPolicyNumber
            };
        }

        /// <summary>
        /// تبدیل Entity به Lookup ViewModel
        /// </summary>
        private PatientInsuranceLookupViewModel ConvertToLookupViewModel(PatientInsurance patientInsurance)
        {
            if (patientInsurance == null) return null;

            return new PatientInsuranceLookupViewModel
            {
                PatientInsuranceId = patientInsurance.PatientInsuranceId,
                PatientId = patientInsurance.PatientId,
                PatientName = patientInsurance.Patient != null ? $"{patientInsurance.Patient.FirstName} {patientInsurance.Patient.LastName}".Trim() : null,
                InsurancePlanId = patientInsurance.InsurancePlanId,
                InsurancePlanName = patientInsurance.InsurancePlan?.Name,
                InsuranceProviderName = patientInsurance.InsurancePlan?.InsuranceProvider?.Name,
                PolicyNumber = patientInsurance.PolicyNumber,
                IsPrimary = patientInsurance.IsPrimary,
                CoveragePercent = patientInsurance.InsurancePlan?.CoveragePercent ?? 0
            };
        }

        #endregion

        #region Combined Insurance Calculation Methods

        /// <summary>
        /// محاسبه بیمه ترکیبی برای بیمار و خدمت مشخص
        /// </summary>
        public async Task<ServiceResult<CombinedInsuranceCalculationResult>> CalculateCombinedInsuranceForPatientAsync(
            int patientId, 
            int serviceId, 
            decimal serviceAmount, 
            DateTime? calculationDate = null)
        {
            try
            {
                _log.Information("🏥 MEDICAL: درخواست محاسبه بیمه ترکیبی - PatientId: {PatientId}, ServiceId: {ServiceId}, Amount: {Amount}. User: {UserName} (Id: {UserId})",
                    patientId, serviceId, serviceAmount, _currentUserService.UserName, _currentUserService.UserId);

                var effectiveDate = calculationDate ?? DateTime.UtcNow;

                var result = await _combinedInsuranceCalculationService.CalculateCombinedInsuranceAsync(
                    patientId, serviceId, serviceAmount, effectiveDate);

                if (result.Success)
                {
                    _log.Information("🏥 MEDICAL: محاسبه بیمه ترکیبی موفق - PatientId: {PatientId}, ServiceId: {ServiceId}, TotalCoverage: {TotalCoverage}, PatientShare: {PatientShare}. User: {UserName} (Id: {UserId})",
                        patientId, serviceId, result.Data.TotalInsuranceCoverage, result.Data.FinalPatientShare, _currentUserService.UserName, _currentUserService.UserId);
                }
                else
                {
                    _log.Warning("🏥 MEDICAL: خطا در محاسبه بیمه ترکیبی - PatientId: {PatientId}, ServiceId: {ServiceId}, Error: {Error}. User: {UserName} (Id: {UserId})",
                        patientId, serviceId, result.Message, _currentUserService.UserName, _currentUserService.UserId);
                }

                return result;
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطای سیستمی در محاسبه بیمه ترکیبی - PatientId: {PatientId}, ServiceId: {ServiceId}. User: {UserName} (Id: {UserId})",
                    patientId, serviceId, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<CombinedInsuranceCalculationResult>.Failed("خطا در محاسبه بیمه ترکیبی");
            }
        }

        /// <summary>
        /// دریافت اطلاعات بیمه‌های فعال بیمار (اصلی + تکمیلی)
        /// </summary>
        public async Task<ServiceResult<List<PatientInsuranceLookupViewModel>>> GetActiveAndSupplementaryByPatientIdAsync(int patientId)
        {
            try
            {
                _log.Information("🏥 MEDICAL: درخواست دریافت بیمه‌های فعال و تکمیلی - PatientId: {PatientId}. User: {UserName} (Id: {UserId})",
                    patientId, _currentUserService.UserName, _currentUserService.UserId);

                // دریافت بیمه اصلی
                var primaryInsurance = await _patientInsuranceRepository.GetPrimaryByPatientIdAsync(patientId);
                
                // دریافت بیمه‌های تکمیلی
                var supplementaryInsurances = await _patientInsuranceRepository.GetSupplementaryByPatientIdAsync(patientId);

                var result = new List<PatientInsuranceLookupViewModel>();

                // اضافه کردن بیمه اصلی
                if (primaryInsurance != null)
                {
                    result.Add(ConvertToLookupViewModel(primaryInsurance));
                }

                // اضافه کردن بیمه‌های تکمیلی
                if (supplementaryInsurances != null && supplementaryInsurances.Any())
                {
                    foreach (var supplementary in supplementaryInsurances)
                    {
                        result.Add(ConvertToLookupViewModel(supplementary));
                    }
                }

                _log.Information("🏥 MEDICAL: دریافت بیمه‌های فعال و تکمیلی موفق - PatientId: {PatientId}, Count: {Count}. User: {UserName} (Id: {UserId})",
                    patientId, result.Count, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<List<PatientInsuranceLookupViewModel>>.Successful(result);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در دریافت بیمه‌های فعال و تکمیلی - PatientId: {PatientId}. User: {UserName} (Id: {UserId})",
                    patientId, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<List<PatientInsuranceLookupViewModel>>.Failed("خطا در دریافت بیمه‌های فعال و تکمیلی");
            }
        }

        /// <summary>
        /// بررسی وجود بیمه ترکیبی برای بیمار
        /// </summary>
        public async Task<ServiceResult<bool>> HasCombinedInsuranceAsync(int patientId)
        {
            try
            {
                _log.Information("🏥 MEDICAL: بررسی وجود بیمه ترکیبی - PatientId: {PatientId}. User: {UserName} (Id: {UserId})",
                    patientId, _currentUserService.UserName, _currentUserService.UserId);

                // بررسی وجود بیمه اصلی
                var primaryInsurance = await _patientInsuranceRepository.GetPrimaryByPatientIdAsync(patientId);
                if (primaryInsurance == null)
                {
                    return ServiceResult<bool>.Successful(false);
                }

                // بررسی وجود بیمه تکمیلی
                var supplementaryInsurances = await _patientInsuranceRepository.GetSupplementaryByPatientIdAsync(patientId);
                var hasSupplementary = supplementaryInsurances != null && supplementaryInsurances.Any();

                var hasCombined = primaryInsurance != null && hasSupplementary;

                _log.Information("🏥 MEDICAL: بررسی بیمه ترکیبی - PatientId: {PatientId}, HasCombined: {HasCombined}. User: {UserName} (Id: {UserId})",
                    patientId, hasCombined, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<bool>.Successful(hasCombined);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "🏥 MEDICAL: خطا در بررسی بیمه ترکیبی - PatientId: {PatientId}. User: {UserName} (Id: {UserId})",
                    patientId, _currentUserService.UserName, _currentUserService.UserId);

                return ServiceResult<bool>.Failed("خطا در بررسی بیمه ترکیبی");
            }
        }

        #endregion

        #region Statistics Methods

        /// <summary>
        /// دریافت تعداد بیمه‌های فعال
        /// </summary>
        /// <returns>تعداد بیمه‌های فعال</returns>
        public async Task<int> GetActiveInsurancesCountAsync()
        {
            try
            {
                _log.Information("📊 دریافت تعداد بیمه‌های فعال. کاربر: {UserName}", _currentUserService.UserName);

                var today = DateTime.Today;
                var activeInsurances = await _patientInsuranceRepository.GetAllAsync();
                var activeCount = activeInsurances.Count(pi => pi.IsActive && !pi.IsDeleted && 
                    (pi.EndDate == null || pi.EndDate >= today));

                _log.Information("✅ تعداد بیمه‌های فعال: {Count}", activeCount);
                return activeCount;
            }
            catch (Exception ex)
            {
                _log.Error(ex, "❌ خطا در GetActiveInsurancesCountAsync");
                return 0;
            }
        }

        /// <summary>
        /// دریافت تعداد بیمه‌های منقضی
        /// </summary>
        /// <returns>تعداد بیمه‌های منقضی</returns>
        public async Task<int> GetExpiredInsurancesCountAsync()
        {
            try
            {
                _log.Information("📊 دریافت تعداد بیمه‌های منقضی. کاربر: {UserName}", _currentUserService.UserName);

                var today = DateTime.Today;
                var allInsurances = await _patientInsuranceRepository.GetAllAsync();
                var expiredCount = allInsurances.Count(pi => !pi.IsDeleted && 
                    pi.EndDate.HasValue && pi.EndDate < today);

                _log.Information("✅ تعداد بیمه‌های منقضی: {Count}", expiredCount);
                return expiredCount;
            }
            catch (Exception ex)
            {
                _log.Error(ex, "❌ خطا در GetExpiredInsurancesCountAsync");
                return 0;
            }
        }

        #endregion
    }
}
