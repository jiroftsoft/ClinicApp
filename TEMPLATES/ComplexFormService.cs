using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ClinicApp.Core;
using ClinicApp.Models.Entities.[Module];
using ClinicApp.Repositories;
using ClinicApp.ViewModels.[Module];

namespace ClinicApp.Services
{
    /// <summary>
    /// Interface برای سرویس [Module]
    /// </summary>
    public interface I[Module]Service
    {
        Task<ServiceResult<List<[Module]>>> GetAllAsync();
        Task<ServiceResult<[Module]>> GetByIdAsync(int id);
        Task<ServiceResult<[Module]>> CreateAsync([Module]CreateEditViewModel model);
        Task<ServiceResult<[Module]>> UpdateAsync(int id, [Module]CreateEditViewModel model);
        Task<ServiceResult<bool>> DeleteAsync(int id);
        Task<ServiceResult<bool>> ToggleStatusAsync(int id);
        Task<ServiceResult<List<[Module]>>> SearchAsync(string searchTerm, int pageNumber = 1, int pageSize = 10);
        Task<ServiceResult<List<InsuranceProvider>>> GetInsuranceProvidersAsync();
        Task<ServiceResult<bool>> ValidateModelAsync([Module]CreateEditViewModel model);
    }

    /// <summary>
    /// سرویس [Module] - استاندارد کامل برای فرم‌های پیچیده
    /// </summary>
    public class [Module]Service : I[Module]Service
    {
        #region Fields - فیلدها

        private readonly I[Module]Repository _repository;
        private readonly IInsuranceProviderRepository _insuranceProviderRepository;
        private readonly ILogger<[Module]Service> _logger;
        private readonly ICurrentUserService _currentUserService;

        #endregion

        #region Constructor - سازنده

        public [Module]Service(
            I[Module]Repository repository,
            IInsuranceProviderRepository insuranceProviderRepository,
            ILogger<[Module]Service> logger,
            ICurrentUserService currentUserService)
        {
            _repository = repository;
            _insuranceProviderRepository = insuranceProviderRepository;
            _logger = logger;
            _currentUserService = currentUserService;
        }

        #endregion

        #region CRUD Operations - عملیات CRUD

        /// <summary>
        /// دریافت تمام [Module]ها
        /// </summary>
        public async Task<ServiceResult<List<[Module]>>> GetAllAsync()
        {
            try
            {
                _logger.LogInformation("🔄 [Module] GetAllAsync started");

                var entities = await _repository.GetAllAsync();
                
                _logger.LogInformation("✅ [Module] GetAllAsync successful - {Count} items", entities.Count);
                return ServiceResult<List<[Module]>>.Success(entities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 [Module] GetAllAsync exception occurred");
                return ServiceResult<List<[Module]>>.Failure("خطا در دریافت اطلاعات");
            }
        }

        /// <summary>
        /// دریافت [Module] بر اساس شناسه
        /// </summary>
        public async Task<ServiceResult<[Module]>> GetByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("🔄 [Module] GetByIdAsync started for ID: {Id}", id);

                var entity = await _repository.GetByIdAsync(id);
                
                if (entity == null)
                {
                    _logger.LogWarning("⚠️ [Module] not found for ID: {Id}", id);
                    return ServiceResult<[Module]>.Failure("[Module] مورد نظر یافت نشد");
                }

                _logger.LogInformation("✅ [Module] GetByIdAsync successful for ID: {Id}", id);
                return ServiceResult<[Module]>.Success(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 [Module] GetByIdAsync exception occurred for ID: {Id}", id);
                return ServiceResult<[Module]>.Failure("خطا در دریافت اطلاعات");
            }
        }

        /// <summary>
        /// ایجاد [Module] جدید
        /// </summary>
        public async Task<ServiceResult<[Module]>> CreateAsync([Module]CreateEditViewModel model)
        {
            try
            {
                _logger.LogInformation("🔄 [Module] CreateAsync started");

                // Validation
                var validationResult = await ValidateModelAsync(model);
                if (!validationResult.IsValid)
                {
                    _logger.LogWarning("⚠️ [Module] CreateAsync validation failed: {Errors}", 
                        string.Join(", ", validationResult.Errors));
                    return ServiceResult<[Module]>.Failure(validationResult.Errors.First());
                }

                // Check for duplicate code
                var existingByCode = await _repository.GetByCodeAsync(model.Code);
                if (existingByCode != null)
                {
                    _logger.LogWarning("⚠️ [Module] CreateAsync failed - duplicate code: {Code}", model.Code);
                    return ServiceResult<[Module]>.Failure("کد [Module] تکراری است");
                }

                // Create entity
                var entity = model.ToEntity();
                entity.CreatedAt = DateTime.Now;
                entity.CreatedBy = _currentUserService.GetCurrentUserId();

                var result = await _repository.AddAsync(entity);
                await _repository.SaveChangesAsync();

                _logger.LogInformation("✅ [Module] CreateAsync successful - ID: {Id}", result.Id);
                return ServiceResult<[Module]>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 [Module] CreateAsync exception occurred");
                return ServiceResult<[Module]>.Failure("خطا در ایجاد [Module]");
            }
        }

        /// <summary>
        /// به‌روزرسانی [Module]
        /// </summary>
        public async Task<ServiceResult<[Module]>> UpdateAsync(int id, [Module]CreateEditViewModel model)
        {
            try
            {
                _logger.LogInformation("🔄 [Module] UpdateAsync started for ID: {Id}", id);

                // Get existing entity
                var existingEntity = await _repository.GetByIdAsync(id);
                if (existingEntity == null)
                {
                    _logger.LogWarning("⚠️ [Module] UpdateAsync failed - not found for ID: {Id}", id);
                    return ServiceResult<[Module]>.Failure("[Module] مورد نظر یافت نشد");
                }

                // Validation
                var validationResult = await ValidateModelAsync(model);
                if (!validationResult.IsValid)
                {
                    _logger.LogWarning("⚠️ [Module] UpdateAsync validation failed: {Errors}", 
                        string.Join(", ", validationResult.Errors));
                    return ServiceResult<[Module]>.Failure(validationResult.Errors.First());
                }

                // Check for duplicate code (excluding current entity)
                var existingByCode = await _repository.GetByCodeAsync(model.Code);
                if (existingByCode != null && existingByCode.Id != id)
                {
                    _logger.LogWarning("⚠️ [Module] UpdateAsync failed - duplicate code: {Code}", model.Code);
                    return ServiceResult<[Module]>.Failure("کد [Module] تکراری است");
                }

                // Update entity
                model.MapToEntity(existingEntity);
                existingEntity.UpdatedAt = DateTime.Now;
                existingEntity.UpdatedBy = _currentUserService.GetCurrentUserId();

                _repository.Update(existingEntity);
                await _repository.SaveChangesAsync();

                _logger.LogInformation("✅ [Module] UpdateAsync successful for ID: {Id}", id);
                return ServiceResult<[Module]>.Success(existingEntity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 [Module] UpdateAsync exception occurred for ID: {Id}", id);
                return ServiceResult<[Module]>.Failure("خطا در به‌روزرسانی [Module]");
            }
        }

        /// <summary>
        /// حذف [Module]
        /// </summary>
        public async Task<ServiceResult<bool>> DeleteAsync(int id)
        {
            try
            {
                _logger.LogInformation("🔄 [Module] DeleteAsync started for ID: {Id}", id);

                var entity = await _repository.GetByIdAsync(id);
                if (entity == null)
                {
                    _logger.LogWarning("⚠️ [Module] DeleteAsync failed - not found for ID: {Id}", id);
                    return ServiceResult<bool>.Failure("[Module] مورد نظر یافت نشد");
                }

                // Check if [Module] is in use
                var isInUse = await _repository.IsInUseAsync(id);
                if (isInUse)
                {
                    _logger.LogWarning("⚠️ [Module] DeleteAsync failed - in use for ID: {Id}", id);
                    return ServiceResult<bool>.Failure("این [Module] در حال استفاده است و نمی‌توان آن را حذف کرد");
                }

                // Soft delete
                entity.IsDeleted = true;
                entity.DeletedAt = DateTime.Now;
                entity.DeletedBy = _currentUserService.GetCurrentUserId();

                _repository.Update(entity);
                await _repository.SaveChangesAsync();

                _logger.LogInformation("✅ [Module] DeleteAsync successful for ID: {Id}", id);
                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 [Module] DeleteAsync exception occurred for ID: {Id}", id);
                return ServiceResult<bool>.Failure("خطا در حذف [Module]");
            }
        }

        /// <summary>
        /// تغییر وضعیت فعال/غیرفعال [Module]
        /// </summary>
        public async Task<ServiceResult<bool>> ToggleStatusAsync(int id)
        {
            try
            {
                _logger.LogInformation("🔄 [Module] ToggleStatusAsync started for ID: {Id}", id);

                var entity = await _repository.GetByIdAsync(id);
                if (entity == null)
                {
                    _logger.LogWarning("⚠️ [Module] ToggleStatusAsync failed - not found for ID: {Id}", id);
                    return ServiceResult<bool>.Failure("[Module] مورد نظر یافت نشد");
                }

                entity.IsActive = !entity.IsActive;
                entity.UpdatedAt = DateTime.Now;
                entity.UpdatedBy = _currentUserService.GetCurrentUserId();

                _repository.Update(entity);
                await _repository.SaveChangesAsync();

                _logger.LogInformation("✅ [Module] ToggleStatusAsync successful for ID: {Id} - New Status: {Status}", 
                    id, entity.IsActive ? "Active" : "Inactive");
                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 [Module] ToggleStatusAsync exception occurred for ID: {Id}", id);
                return ServiceResult<bool>.Failure("خطا در تغییر وضعیت [Module]");
            }
        }

        #endregion

        #region Search Operations - عملیات جستجو

        /// <summary>
        /// جستجوی [Module]
        /// </summary>
        public async Task<ServiceResult<List<[Module]>>> SearchAsync(string searchTerm, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                _logger.LogInformation("🔄 [Module] SearchAsync started - Term: {SearchTerm}, Page: {PageNumber}, Size: {PageSize}", 
                    searchTerm, pageNumber, pageSize);

                var result = await _repository.SearchAsync(searchTerm, pageNumber, pageSize);
                
                _logger.LogInformation("✅ [Module] SearchAsync successful - {Count} items found", result.Count);
                return ServiceResult<List<[Module]>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 [Module] SearchAsync exception occurred");
                return ServiceResult<List<[Module]>>.Failure("خطا در جستجوی [Module]");
            }
        }

        #endregion

        #region Lookup Operations - عملیات جستجو

        /// <summary>
        /// دریافت ارائه‌دهندگان بیمه
        /// </summary>
        public async Task<ServiceResult<List<InsuranceProvider>>> GetInsuranceProvidersAsync()
        {
            try
            {
                _logger.LogInformation("🔄 [Module] GetInsuranceProvidersAsync started");

                var providers = await _insuranceProviderRepository.GetActiveAsync();
                
                _logger.LogInformation("✅ [Module] GetInsuranceProvidersAsync successful - {Count} providers", providers.Count);
                return ServiceResult<List<InsuranceProvider>>.Success(providers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 [Module] GetInsuranceProvidersAsync exception occurred");
                return ServiceResult<List<InsuranceProvider>>.Failure("خطا در دریافت ارائه‌دهندگان بیمه");
            }
        }

        #endregion

        #region Validation - اعتبارسنجی

        /// <summary>
        /// اعتبارسنجی مدل
        /// </summary>
        public async Task<ServiceResult<bool>> ValidateModelAsync([Module]CreateEditViewModel model)
        {
            try
            {
                _logger.LogInformation("🔄 [Module] ValidateModelAsync started");

                var errors = new List<string>();

                // Basic validation
                if (string.IsNullOrWhiteSpace(model.Name))
                    errors.Add("نام الزامی است");

                if (string.IsNullOrWhiteSpace(model.Code))
                    errors.Add("کد الزامی است");

                if (model.InsuranceProviderId <= 0)
                    errors.Add("ارائه‌دهنده بیمه الزامی است");

                // Date validation
                if (string.IsNullOrWhiteSpace(model.ValidFromShamsi))
                    errors.Add("تاریخ شروع الزامی است");

                if (!string.IsNullOrWhiteSpace(model.ValidFromShamsi) && !string.IsNullOrWhiteSpace(model.ValidToShamsi))
                {
                    try
                    {
                        var fromDate = PersianDateHelper.ToGregorianDate(model.ValidFromShamsi);
                        var toDate = PersianDateHelper.ToGregorianDate(model.ValidToShamsi);
                        
                        if (fromDate >= toDate)
                            errors.Add("تاریخ پایان اعتبار نمی‌تواند قبل از تاریخ شروع اعتبار باشد");
                    }
                    catch
                    {
                        errors.Add("فرمت تاریخ نامعتبر است");
                    }
                }

                // Financial validation
                if (model.CoveragePercent < 0 || model.CoveragePercent > 100)
                    errors.Add("درصد پوشش بیمه باید بین 0 تا 100 باشد");

                if (model.Deductible < 0)
                    errors.Add("مبلغ فرانشیز باید بزرگتر یا مساوی صفر باشد");

                // Business logic validation
                if (model.CoveragePercent + (model.Deductible > 0 ? 10 : 0) > 100)
                    errors.Add("مجموع درصد پوشش و فرانشیز نباید از 100% بیشتر باشد");

                // Check insurance provider exists
                if (model.InsuranceProviderId > 0)
                {
                    var provider = await _insuranceProviderRepository.GetByIdAsync(model.InsuranceProviderId);
                    if (provider == null || !provider.IsActive)
                        errors.Add("ارائه‌دهنده بیمه انتخاب شده معتبر نیست");
                }

                if (errors.Any())
                {
                    _logger.LogWarning("⚠️ [Module] ValidateModelAsync failed - {ErrorCount} errors", errors.Count);
                    return ServiceResult<bool>.Failure(errors.First());
                }

                _logger.LogInformation("✅ [Module] ValidateModelAsync successful");
                return ServiceResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "💥 [Module] ValidateModelAsync exception occurred");
                return ServiceResult<bool>.Failure("خطا در اعتبارسنجی");
            }
        }

        #endregion

        #region Business Logic - منطق کسب‌وکار

        /// <summary>
        /// محاسبه درصد پرداخت بیمار
        /// </summary>
        public decimal CalculatePatientShare(decimal coveragePercent)
        {
            return 100 - coveragePercent;
        }

        /// <summary>
        /// محاسبه مبلغ پرداخت بیمه
        /// </summary>
        public decimal CalculateInsuranceAmount(decimal totalAmount, decimal coveragePercent)
        {
            return totalAmount * (coveragePercent / 100);
        }

        /// <summary>
        /// محاسبه مبلغ پرداخت بیمار
        /// </summary>
        public decimal CalculatePatientAmount(decimal totalAmount, decimal coveragePercent, decimal deductible)
        {
            var insuranceAmount = CalculateInsuranceAmount(totalAmount, coveragePercent);
            var patientAmount = totalAmount - insuranceAmount;
            
            // Apply deductible
            if (patientAmount > 0 && deductible > 0)
            {
                patientAmount = Math.Max(patientAmount, deductible);
            }
            
            return patientAmount;
        }

        /// <summary>
        /// بررسی اعتبار [Module]
        /// </summary>
        public bool IsValid([Module] entity)
        {
            if (entity == null) return false;
            if (!entity.IsActive) return false;
            if (entity.IsDeleted) return false;
            if (entity.ValidFrom > DateTime.Now) return false;
            if (entity.ValidTo.HasValue && entity.ValidTo < DateTime.Now) return false;
            
            return true;
        }

        #endregion
    }
}
