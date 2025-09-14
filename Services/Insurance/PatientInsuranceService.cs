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
using ClinicApp.ViewModels.Insurance.PatientInsurance;
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
        private readonly ILogger _log;
        private readonly ICurrentUserService _currentUserService;
        private IPatientInsuranceService _patientInsuranceServiceImplementation;

        public PatientInsuranceService(
            IPatientInsuranceRepository patientInsuranceRepository,
            ILogger logger,
            ICurrentUserService currentUserService)
        {
            _patientInsuranceRepository = patientInsuranceRepository ?? throw new ArgumentNullException(nameof(patientInsuranceRepository));
            _log = logger.ForContext<PatientInsuranceService>();
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

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

        public async Task<ServiceResult<PatientInsuranceDetailsViewModel>> GetPatientInsuranceDetailsAsync(int patientInsuranceId)
        {
            try
            {
                _log.Information("Getting patient insurance details for PatientInsuranceId: {PatientInsuranceId}. User: {UserName} (Id: {UserId})", 
                    patientInsuranceId, _currentUserService.UserName, _currentUserService.UserId);

                // استفاده از متد GetByIdAsync که واقعاً کار می‌کند
                return await GetByIdAsync(patientInsuranceId);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error getting patient insurance details for PatientInsuranceId: {PatientInsuranceId}. User: {UserName} (Id: {UserId})", 
                    patientInsuranceId, _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult<PatientInsuranceDetailsViewModel>.Failed("خطا در دریافت جزئیات بیمه بیمار");
            }
        }


        public async Task<ServiceResult<int>> CreatePatientInsuranceAsync(PatientInsuranceCreateEditViewModel model)
        {
            try
            {
                _log.Information("Creating patient insurance for PatientId: {PatientId}, PolicyNumber: {PolicyNumber}, PlanId: {PlanId}. User: {UserName} (Id: {UserId})", 
                    model.PatientId, model.PolicyNumber, model.InsurancePlanId, _currentUserService.UserName, _currentUserService.UserId);

                // استفاده از متد CreateAsync که واقعاً کار می‌کند
                return await CreateAsync(model);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error creating patient insurance for PatientId: {PatientId}, PolicyNumber: {PolicyNumber}, PlanId: {PlanId}. User: {UserName} (Id: {UserId})", 
                    model.PatientId, model.PolicyNumber, model.InsurancePlanId, _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult<int>.Failed("خطا در ایجاد بیمه بیمار");
            }
        }

        public async Task<ServiceResult> UpdatePatientInsuranceAsync(PatientInsuranceCreateEditViewModel model)
        {
            try
            {
                _log.Information("Updating patient insurance for PatientInsuranceId: {PatientInsuranceId}, PatientId: {PatientId}, PolicyNumber: {PolicyNumber}. User: {UserName} (Id: {UserId})", 
                    model.PatientInsuranceId, model.PatientId, model.PolicyNumber, _currentUserService.UserName, _currentUserService.UserId);

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
                _log.Error(ex, "Error updating patient insurance for PatientInsuranceId: {PatientInsuranceId}, PatientId: {PatientId}, PolicyNumber: {PolicyNumber}. User: {UserName} (Id: {UserId})", 
                    model.PatientInsuranceId, model.PatientId, model.PolicyNumber, _currentUserService.UserName, _currentUserService.UserId);
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
                _log.Information("Checking if policy number exists: {PolicyNumber}, ExcludeId: {ExcludeId}. User: {UserName} (Id: {UserId})", 
                    policyNumber, excludePatientInsuranceId, _currentUserService.UserName, _currentUserService.UserId);

                // استفاده از متد ریپازیتوری که واقعاً کار می‌کند
                var exists = await _patientInsuranceRepository.DoesPolicyNumberExistAsync(policyNumber, excludePatientInsuranceId);
                return ServiceResult<bool>.Successful(exists);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error checking policy number existence: {PolicyNumber}, ExcludeId: {ExcludeId}. User: {UserName} (Id: {UserId})", 
                    policyNumber, excludePatientInsuranceId, _currentUserService.UserName, _currentUserService.UserId);
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

        public async Task<ServiceResult<Dictionary<string, string>>> ValidatePatientInsuranceAsync(PatientInsuranceCreateEditViewModel model)
        {
            try
            {
                _log.Information("Validating patient insurance for PatientId: {PatientId}, PolicyNumber: {PolicyNumber}", 
                    model.PatientId, model.PolicyNumber);

                var errors = new Dictionary<string, string>();

                // بررسی وجود شماره بیمه تکراری
                var policyExistsResult = await DoesPolicyNumberExistAsync(model.PolicyNumber, model.PatientInsuranceId);
                if (policyExistsResult.Success && policyExistsResult.Data)
                {
                    errors.Add("PolicyNumber", "شماره بیمه قبلاً ثبت شده است.");
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

                // بررسی تداخل تاریخ‌ها
                var dateOverlapResult = await DoesDateOverlapExistAsync(
                    model.PatientId, model.StartDate, model.EndDate ?? DateTime.MaxValue, model.PatientInsuranceId);
                if (dateOverlapResult.Success && dateOverlapResult.Data)
                {
                    errors.Add("StartDate", "تاریخ‌های انتخاب شده با بیمه‌های موجود این بیمار تداخل دارد.");
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

        public async Task<ServiceResult<List<PatientInsuranceIndexViewModel>>> GetSupplementaryInsurancesByPatientAsync(int patientId)
        {
            try
            {
                _log.Information("Getting supplementary insurances by patient for PatientId: {PatientId}. User: {UserName} (Id: {UserId})", 
                    patientId, _currentUserService.UserName, _currentUserService.UserId);

                // دریافت بیمه‌های تکمیلی بیمار از ریپازیتوری
                var supplementaryInsurances = await _patientInsuranceRepository.GetSupplementaryByPatientIdAsync(patientId);
                var viewModels = supplementaryInsurances.Select(ConvertToIndexViewModel).ToList();
                
                return ServiceResult<List<PatientInsuranceIndexViewModel>>.Successful(viewModels);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "Error getting supplementary insurances by patient for PatientId: {PatientId}. User: {UserName} (Id: {UserId})", 
                    patientId, _currentUserService.UserName, _currentUserService.UserId);
                return ServiceResult<List<PatientInsuranceIndexViewModel>>.Failed("خطا در دریافت بیمه‌های تکمیلی بیمار");
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
                            insurance.UpdatedAt = DateTime.Now;
                            _patientInsuranceRepository.Update(insurance);
                            
                            _log.Information("بیمه اصلی قبلی غیرفعال شد. PatientInsuranceId: {PatientInsuranceId}. User: {UserName} (Id: {UserId})", 
                                insurance.PatientInsuranceId, _currentUserService.UserName, _currentUserService.UserId);
                        }

                        // 2. تنظیم بیمه جدید به عنوان اصلی
                        patientInsurance.IsPrimary = true;
                        patientInsurance.UpdatedByUserId = _currentUserService.GetCurrentUserId();
                        patientInsurance.UpdatedAt = DateTime.Now;
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
                var result = await _patientInsuranceRepository.GetPagedOptimizedAsync(
                    null, searchTerm, providerId, planId, isPrimary, isActive, fromDate, toDate, pageNumber, pageSize);

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

                // به‌روزرسانی Entity
                existingPatientInsurance.PatientId = model.PatientId;
                existingPatientInsurance.InsurancePlanId = model.InsurancePlanId;
                existingPatientInsurance.PolicyNumber = model.PolicyNumber;
                existingPatientInsurance.IsPrimary = model.IsPrimary;
                existingPatientInsurance.StartDate = model.StartDate;
                existingPatientInsurance.EndDate = model.EndDate;
                existingPatientInsurance.IsActive = model.IsActive;

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

                // حذف نرم
                _patientInsuranceRepository.Delete(patientInsurance);
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

                var viewModels = patientInsurances.Select(ConvertToLookupViewModel).ToList();

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
                CoveragePercent = patientInsurance.InsurancePlan?.CoveragePercent ?? 0
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
                PolicyNumber = model.PolicyNumber,
                IsPrimary = model.IsPrimary,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                IsActive = model.IsActive
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
    }
}
