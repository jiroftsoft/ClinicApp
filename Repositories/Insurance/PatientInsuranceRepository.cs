using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Core;
using ClinicApp.Extensions;
using ClinicApp.Helpers;
using ClinicApp.Interfaces.Insurance;
using ClinicApp.Interfaces;
using ClinicApp.Models;
using ClinicApp.Models.Entities;
using ClinicApp.Models.Entities.Patient;
using ClinicApp.ViewModels.Insurance.PatientInsurance;
using Serilog;

namespace ClinicApp.Repositories.Insurance
{
    /// <summary>
    /// پیاده‌سازی Repository برای مدیریت بیمه‌های بیماران در سیستم کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. پیاده‌سازی کامل مدیریت بیمه‌های بیماران (Primary و Supplementary)
    /// 2. رعایت استانداردهای پزشکی ایران در مدیریت بیمه‌ها
    /// 3. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات بیمه‌ای
    /// 4. مدیریت کامل ردیابی (Audit Trail) برای حسابرسی و امنیت سیستم
    /// 5. بهینه‌سازی عملکرد با AsNoTracking و Include
    /// 6. مدیریت روابط با Patient و InsurancePlan
    /// 7. مدیریت بیمه اصلی و تکمیلی
    /// 
    /// نکته حیاتی: این کلاس بر اساس استانداردهای سیستم‌های پزشکی ایران پیاده‌سازی شده است
    /// </summary>
    public class PatientInsuranceRepository : IPatientInsuranceRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;
        private readonly ICurrentUserService _currentUserService;

        public PatientInsuranceRepository(ApplicationDbContext context, ILogger logger, ICurrentUserService currentUserService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        #region Core CRUD Operations

        /// <summary>
        /// دریافت بیمه بیمار بر اساس شناسه
        /// </summary>
        public async Task<PatientInsurance> GetByIdAsync(int id)
        {
            try
            {
                return await _context.PatientInsurances
                    .Where(pi => pi.PatientInsuranceId == id)
                    .AsNoTracking()
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت بیمه بیمار. PatientInsuranceId: {PatientInsuranceId}", id);
                throw new InvalidOperationException($"خطا در دریافت بیمه بیمار {id}", ex);
            }
        }

        /// <summary>
        /// دریافت بیمه بیمار بر اساس شناسه همراه با جزئیات کامل
        /// </summary>
        public async Task<PatientInsurance> GetByIdWithDetailsAsync(int id)
        {
            try
            {
                return await _context.PatientInsurances
                    .Where(pi => pi.PatientInsuranceId == id)
                    .Include(pi => pi.Patient)
                    .Include(pi => pi.InsurancePlan.InsuranceProvider)
                    .Include(pi => pi.CreatedByUser)
                    .Include(pi => pi.UpdatedByUser)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت بیمه بیمار با جزئیات. PatientInsuranceId: {PatientInsuranceId}", id);
                throw new InvalidOperationException($"خطا در دریافت بیمه بیمار {id} با جزئیات", ex);
            }
        }

        /// <summary>
        /// دریافت تمام بیمه‌های بیماران
        /// </summary>
        public async Task<List<PatientInsurance>> GetAllAsync()
        {
            try
            {
                return await _context.PatientInsurances
                    .Include(pi => pi.Patient)
                    .Include(pi => pi.InsurancePlan.InsuranceProvider)
                    .OrderBy(pi => pi.Patient.FirstName)
                    .ThenBy(pi => pi.Patient.LastName)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تمام بیمه‌های بیماران");
                throw new InvalidOperationException("خطا در دریافت تمام بیمه‌های بیماران", ex);
            }
        }

        /// <summary>
        /// دریافت بیمه‌های بیماران فعال
        /// </summary>
        public async Task<List<PatientInsurance>> GetActiveAsync()
        {
            try
            {
                return await _context.PatientInsurances
                    .Where(pi => pi.IsActive)
                    .Include(pi => pi.Patient)
                    .Include(pi => pi.InsurancePlan.InsuranceProvider)
                    .OrderBy(pi => pi.Patient.FirstName)
                    .ThenBy(pi => pi.Patient.LastName)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت بیمه‌های بیماران فعال");
                throw new InvalidOperationException("خطا در دریافت بیمه‌های بیماران فعال", ex);
            }
        }

        /// <summary>
        /// دریافت بیمه‌های بیمار بر اساس شناسه بیمار
        /// </summary>
        public async Task<List<PatientInsurance>> GetByPatientIdAsync(int patientId)
        {
            try
            {
                _logger.Information("🔍 جستجوی بیمه‌های بیمار {PatientId} در دیتابیس", patientId);
                
                var result = await _context.PatientInsurances
                    .Where(pi => pi.PatientId == patientId)
                    .Include(pi => pi.InsurancePlan.InsuranceProvider)
                    .OrderBy(pi => pi.IsPrimary ? 0 : 1)
                    .ThenBy(pi => pi.StartDate)
                    .AsNoTracking()
                    .ToListAsync();
                    
                _logger.Information("📊 {Count} بیمه برای بیمار {PatientId} یافت شد", result.Count, patientId);
                
                foreach (var insurance in result)
                {
                    _logger.Information("📋 بیمه: ID={InsuranceId}, Plan={PlanId}, Provider={ProviderId}, IsPrimary={IsPrimary}, IsActive={IsActive}, IsDeleted={IsDeleted}", 
                        insurance.PatientInsuranceId, insurance.InsurancePlanId, insurance.InsurancePlan?.InsuranceProviderId, 
                        insurance.IsPrimary, insurance.IsActive, insurance.IsDeleted);
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت بیمه‌های بیمار. PatientId: {PatientId}", patientId);
                throw new InvalidOperationException($"خطا در دریافت بیمه‌های بیمار {patientId}", ex);
            }
        }

        /// <summary>
        /// دریافت بیمه‌های فعال بیمار بر اساس شناسه بیمار
        /// </summary>
        public async Task<List<PatientInsurance>> GetActiveByPatientIdAsync(int patientId)
        {
            try
            {
                return await _context.PatientInsurances
                    .Where(pi => pi.PatientId == patientId && pi.IsActive && !pi.IsDeleted)
                    .Include(pi => pi.InsurancePlan)
                    .Include(pi => pi.InsurancePlan.InsuranceProvider)
                    .Include(pi => pi.SupplementaryInsuranceProvider) // بیمه‌گذار تکمیلی
                    .Include(pi => pi.SupplementaryInsurancePlan) // طرح بیمه تکمیلی
                    .Include(pi => pi.Patient)
                    .OrderBy(pi => pi.IsPrimary ? 0 : 1)
                    .ThenBy(pi => pi.StartDate)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت بیمه‌های فعال بیمار. PatientId: {PatientId}", patientId);
                throw new InvalidOperationException($"خطا در دریافت بیمه‌های فعال بیمار {patientId}", ex);
            }
        }

        /// <summary>
        /// دریافت بیمه اصلی بیمار بر اساس شناسه بیمار
        /// </summary>
        public async Task<PatientInsurance> GetPrimaryByPatientIdAsync(int patientId)
        {
            try
            {
                return await _context.PatientInsurances
                    .Where(pi => pi.PatientId == patientId && pi.IsPrimary && pi.IsActive)
                    .Include(pi => pi.InsurancePlan.InsuranceProvider)
                    .AsNoTracking()
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت بیمه اصلی بیمار. PatientId: {PatientId}", patientId);
                throw new InvalidOperationException($"خطا در دریافت بیمه اصلی بیمار {patientId}", ex);
            }
        }

        /// <summary>
        /// دریافت بیمه‌های تکمیلی بیمار بر اساس شناسه بیمار
        /// 🏥 Medical Environment: منطق صحیح برای سیستم درمانی کلینیک شفا
        /// </summary>
        public async Task<List<PatientInsurance>> GetSupplementaryByPatientIdAsync(int patientId)
        {
            try
            {
                // 🚨 CRITICAL FIX: منطق صحیح برای سیستم درمانی
                // بیمه‌های تکمیلی: 
                // 1. رکوردهایی که IsPrimary = false هستند (رکوردهای جداگانه)
                // 2. رکوردهایی که IsPrimary = true اما SupplementaryInsuranceProviderId دارند (فیلدهای تکمیلی)
                return await _context.PatientInsurances
                    .Where(pi => pi.PatientId == patientId 
                             && pi.IsActive
                             && !pi.IsDeleted
                             && (
                                 pi.IsPrimary == false || // رکوردهای جداگانه بیمه تکمیلی
                                 (pi.IsPrimary == true && pi.SupplementaryInsuranceProviderId.HasValue && pi.SupplementaryInsurancePlanId.HasValue) // فیلدهای تکمیلی در بیمه اصلی
                             ))
                    .Include(pi => pi.InsurancePlan)
                    .Include(pi => pi.SupplementaryInsuranceProvider) // بیمه‌گذار تکمیلی
                    .Include(pi => pi.SupplementaryInsurancePlan) // طرح بیمه تکمیلی
                    .Include(pi => pi.Patient) // اطلاعات بیمار
                    .OrderBy(pi => pi.Priority)
                    .ThenBy(pi => pi.StartDate)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت بیمه‌های تکمیلی بیمار. PatientId: {PatientId}", patientId);
                throw new InvalidOperationException($"خطا در دریافت بیمه‌های تکمیلی بیمار {patientId}", ex);
            }
        }

        /// <summary>
        /// دریافت بیمه‌های بیماران بر اساس شناسه طرح بیمه
        /// </summary>
        public async Task<List<PatientInsurance>> GetByPlanIdAsync(int planId)
        {
            try
            {
                return await _context.PatientInsurances
                    .Where(pi => pi.InsurancePlanId == planId)
                    .Include(pi => pi.Patient)
                    .Include(pi => pi.InsurancePlan.InsuranceProvider)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت بیمه‌های بیماران بر اساس طرح بیمه. PlanId: {PlanId}", planId);
                throw new InvalidOperationException($"خطا در دریافت بیمه‌های بیماران با طرح بیمه {planId}", ex);
            }
        }

        /// <summary>
        /// دریافت بیمه بیمار بر اساس شماره بیمه
        /// </summary>
        public async Task<PatientInsurance> GetByPolicyNumberAsync(string policyNumber)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(policyNumber))
                    return null;

                return await _context.PatientInsurances
                    .Where(pi => pi.PolicyNumber == policyNumber)
                    .Include(pi => pi.Patient)
                    .Include(pi => pi.InsurancePlan.InsuranceProvider)
                    .AsNoTracking()
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت بیمه بیمار بر اساس شماره بیمه. PolicyNumber: {PolicyNumber}", policyNumber);
                throw new InvalidOperationException($"خطا در دریافت بیمه بیمار با شماره بیمه {policyNumber}", ex);
            }
        }

        #endregion

        #region Validation Operations

        /// <summary>
        /// بررسی وجود شماره بیمه
        /// </summary>
        public async Task<bool> DoesPolicyNumberExistAsync(string policyNumber, int? excludeId = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(policyNumber))
                    return false;

                var query = _context.PatientInsurances
                    .Where(pi => pi.PolicyNumber == policyNumber);

                if (excludeId.HasValue)
                    query = query.Where(pi => pi.PatientInsuranceId != excludeId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی وجود شماره بیمه. PolicyNumber: {PolicyNumber}, ExcludeId: {ExcludeId}", policyNumber, excludeId);
                throw new InvalidOperationException($"خطا در بررسی وجود شماره بیمه {policyNumber}", ex);
            }
        }

        /// <summary>
        /// بررسی وجود بیمه اصلی برای بیمار
        /// </summary>
        public async Task<bool> DoesPrimaryInsuranceExistAsync(int patientId, int? excludeId = null)
        {
            try
            {
                var query = _context.PatientInsurances
                    .Where(pi => pi.PatientId == patientId && pi.IsPrimary && pi.IsActive);

                if (excludeId.HasValue)
                    query = query.Where(pi => pi.PatientInsuranceId != excludeId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی وجود بیمه اصلی برای بیمار. PatientId: {PatientId}, ExcludeId: {ExcludeId}", patientId, excludeId);
                throw new InvalidOperationException($"خطا در بررسی وجود بیمه اصلی برای بیمار {patientId}", ex);
            }
        }

        /// <summary>
        /// بررسی وجود بیمه بیمار
        /// </summary>
        public async Task<bool> DoesExistAsync(int id)
        {
            try
            {
                return await _context.PatientInsurances
                    .Where(pi => pi.PatientInsuranceId == id)
                    .AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی وجود بیمه بیمار. PatientInsuranceId: {PatientInsuranceId}", id);
                throw new InvalidOperationException($"خطا در بررسی وجود بیمه بیمار {id}", ex);
            }
        }

        /// <summary>
        /// بررسی تداخل تاریخ بیمه‌های بیمار
        /// </summary>
        public async Task<bool> DoesDateOverlapExistAsync(int patientId, DateTime startDate, DateTime endDate, int? excludeId = null)
        {
            try
            {
                var query = _context.PatientInsurances
                    .Where(pi => pi.PatientId == patientId && pi.IsActive &&
                                ((pi.StartDate <= startDate && pi.EndDate >= startDate) ||
                                 (pi.StartDate <= endDate && pi.EndDate >= endDate) ||
                                 (pi.StartDate >= startDate && pi.EndDate <= endDate)));

                if (excludeId.HasValue)
                    query = query.Where(pi => pi.PatientInsuranceId != excludeId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی تداخل تاریخ بیمه‌های بیمار. PatientId: {PatientId}, StartDate: {StartDate}, EndDate: {EndDate}, ExcludeId: {ExcludeId}", 
                    patientId, startDate, endDate, excludeId);
                throw new InvalidOperationException($"خطا در بررسی تداخل تاریخ بیمه‌های بیمار {patientId}", ex);
            }
        }

        #endregion

        #region Insurance Update Methods

        /// <summary>
        /// به‌روزرسانی بیمه پایه بیمار - Production Ready
        /// </summary>
        public async Task<ServiceResult<bool>> UpdatePatientPrimaryInsuranceAsync(int patientId, int insuranceId, string policyNumber, string cardNumber)
        {
            try
            {
                _logger.Information("🔄 به‌روزرسانی بیمه پایه بیمار: {PatientId}, بیمه: {InsuranceId}, کاربر: {UserName}", 
                    patientId, insuranceId, _currentUserService.UserName);

                // یافتن بیمه پایه بیمار
                var patientInsurance = await _context.PatientInsurances
                    .FirstOrDefaultAsync(pi => pi.PatientId == patientId && pi.IsPrimary && !pi.IsDeleted);

                if (patientInsurance == null)
                {
                    _logger.Warning("بیمه پایه بیمار یافت نشد: {PatientId}", patientId);
                    return ServiceResult<bool>.Failed("بیمه پایه بیمار یافت نشد", "PRIMARY_INSURANCE_NOT_FOUND", ErrorCategory.Validation, SecurityLevel.Medium);
                }

                // به‌روزرسانی اطلاعات بیمه
                patientInsurance.InsurancePlanId = insuranceId;
                patientInsurance.PolicyNumber = policyNumber;
                patientInsurance.CardNumber = cardNumber;
                patientInsurance.UpdatedAt = DateTime.Now;
                patientInsurance.UpdatedByUserId = _currentUserService.UserId;

                await _context.SaveChangesAsync();

                _logger.Information("✅ به‌روزرسانی بیمه پایه موفق: {PatientId}, بیمه: {InsuranceId}", patientId, insuranceId);
                return ServiceResult<bool>.Successful(true, "بیمه پایه با موفقیت به‌روزرسانی شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در به‌روزرسانی بیمه پایه: {PatientId}", patientId);
                return ServiceResult<bool>.Failed("خطا در به‌روزرسانی بیمه پایه", "UPDATE_PRIMARY_INSURANCE_ERROR", ErrorCategory.System, SecurityLevel.High);
            }
        }

        /// <summary>
        /// به‌روزرسانی بیمه تکمیلی بیمار - Production Ready
        /// </summary>
        public async Task<ServiceResult<bool>> UpdatePatientSupplementaryInsuranceAsync(int patientId, int insuranceId, string policyNumber, DateTime? expiryDate)
        {
            try
            {
                _logger.Information("🔄 به‌روزرسانی بیمه تکمیلی بیمار: {PatientId}, بیمه: {InsuranceId}, کاربر: {UserName}", 
                    patientId, insuranceId, _currentUserService.UserName);

                // یافتن بیمه تکمیلی بیمار
                var patientInsurance = await _context.PatientInsurances
                    .FirstOrDefaultAsync(pi => pi.PatientId == patientId && !pi.IsPrimary && !pi.IsDeleted);

                if (patientInsurance == null)
                {
                    _logger.Warning("بیمه تکمیلی بیمار یافت نشد: {PatientId}", patientId);
                    return ServiceResult<bool>.Failed("بیمه تکمیلی بیمار یافت نشد", "SUPPLEMENTARY_INSURANCE_NOT_FOUND", ErrorCategory.Validation, SecurityLevel.Medium);
                }

                // به‌روزرسانی اطلاعات بیمه
                patientInsurance.InsurancePlanId = insuranceId;
                patientInsurance.PolicyNumber = policyNumber;
                patientInsurance.EndDate = expiryDate;
                patientInsurance.UpdatedAt = DateTime.Now;
                patientInsurance.UpdatedByUserId = _currentUserService.UserId;

                await _context.SaveChangesAsync();

                _logger.Information("✅ به‌روزرسانی بیمه تکمیلی موفق: {PatientId}, بیمه: {InsuranceId}", patientId, insuranceId);
                return ServiceResult<bool>.Successful(true, "بیمه تکمیلی با موفقیت به‌روزرسانی شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در به‌روزرسانی بیمه تکمیلی: {PatientId}", patientId);
                return ServiceResult<bool>.Failed("خطا در به‌روزرسانی بیمه تکمیلی", "UPDATE_SUPPLEMENTARY_INSURANCE_ERROR", ErrorCategory.System, SecurityLevel.High);
            }
        }

        /// <summary>
        /// تغییر بیمه پایه بیمار - Production Ready
        /// </summary>
        public async Task<ServiceResult<bool>> ChangePatientPrimaryInsuranceAsync(int patientId, int newInsuranceId)
        {
            try
            {
                _logger.Information("🔄 تغییر بیمه پایه بیمار: {PatientId}, بیمه جدید: {NewInsuranceId}, کاربر: {UserName}", 
                    patientId, newInsuranceId, _currentUserService.UserName);

                // یافتن بیمه پایه بیمار
                var patientInsurance = await _context.PatientInsurances
                    .FirstOrDefaultAsync(pi => pi.PatientId == patientId && pi.IsPrimary && !pi.IsDeleted);

                if (patientInsurance == null)
                {
                    _logger.Warning("بیمه پایه بیمار یافت نشد: {PatientId}", patientId);
                    return ServiceResult<bool>.Failed("بیمه پایه بیمار یافت نشد", "PRIMARY_INSURANCE_NOT_FOUND", ErrorCategory.Validation, SecurityLevel.Medium);
                }

                // تغییر بیمه
                patientInsurance.InsurancePlanId = newInsuranceId;
                patientInsurance.UpdatedAt = DateTime.Now;
                patientInsurance.UpdatedByUserId = _currentUserService.UserId;

                await _context.SaveChangesAsync();

                _logger.Information("✅ تغییر بیمه پایه موفق: {PatientId}, بیمه جدید: {NewInsuranceId}", patientId, newInsuranceId);
                return ServiceResult<bool>.Successful(true, "بیمه پایه با موفقیت تغییر کرد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در تغییر بیمه پایه: {PatientId}", patientId);
                return ServiceResult<bool>.Failed("خطا در تغییر بیمه پایه", "CHANGE_PRIMARY_INSURANCE_ERROR", ErrorCategory.System, SecurityLevel.High);
            }
        }

        /// <summary>
        /// تغییر بیمه تکمیلی بیمار - Production Ready
        /// </summary>
        public async Task<ServiceResult<bool>> ChangePatientSupplementaryInsuranceAsync(int patientId, int newInsuranceId)
        {
            try
            {
                _logger.Information("🔄 تغییر بیمه تکمیلی بیمار: {PatientId}, بیمه جدید: {NewInsuranceId}, کاربر: {UserName}", 
                    patientId, newInsuranceId, _currentUserService.UserName);

                // یافتن بیمه تکمیلی بیمار
                var patientInsurance = await _context.PatientInsurances
                    .FirstOrDefaultAsync(pi => pi.PatientId == patientId && !pi.IsPrimary && !pi.IsDeleted);

                if (patientInsurance == null)
                {
                    _logger.Warning("بیمه تکمیلی بیمار یافت نشد: {PatientId}", patientId);
                    return ServiceResult<bool>.Failed("بیمه تکمیلی بیمار یافت نشد", "SUPPLEMENTARY_INSURANCE_NOT_FOUND", ErrorCategory.Validation, SecurityLevel.Medium);
                }

                // تغییر بیمه
                patientInsurance.InsurancePlanId = newInsuranceId;
                patientInsurance.UpdatedAt = DateTime.Now;
                patientInsurance.UpdatedByUserId = _currentUserService.UserName;

                await _context.SaveChangesAsync();

                _logger.Information("✅ تغییر بیمه تکمیلی موفق: {PatientId}, بیمه جدید: {NewInsuranceId}", patientId, newInsuranceId);
                return ServiceResult<bool>.Successful(true, "بیمه تکمیلی با موفقیت تغییر کرد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "❌ خطا در تغییر بیمه تکمیلی: {PatientId}", patientId);
                return ServiceResult<bool>.Failed("خطا در تغییر بیمه تکمیلی", "CHANGE_SUPPLEMENTARY_INSURANCE_ERROR", ErrorCategory.System, SecurityLevel.High);
            }
        }

        #endregion

        #region Search Operations

        /// <summary>
        /// جستجوی بیمه‌های بیماران بر اساس عبارت جستجو
        /// </summary>
        public async Task<List<PatientInsurance>> SearchAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetAllAsync();

                var term = searchTerm.Trim();
                return await _context.PatientInsurances
                    .Where(pi => pi.PolicyNumber.Contains(term) || 
                                pi.Patient.FirstName.Contains(term) || 
                                pi.Patient.LastName.Contains(term) ||
                                pi.Patient.NationalCode.Contains(term) ||
                                pi.InsurancePlan.Name.Contains(term) ||
                                pi.InsurancePlan.InsuranceProvider.Name.Contains(term))
                    .Include(pi => pi.Patient)
                    .Include(pi => pi.InsurancePlan.InsuranceProvider)
                    .OrderBy(pi => pi.Patient.FirstName)
                    .ThenBy(pi => pi.Patient.LastName)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در جستجوی بیمه‌های بیماران. SearchTerm: {SearchTerm}", searchTerm);
                throw new InvalidOperationException($"خطا در جستجوی بیمه‌های بیماران با عبارت {searchTerm}", ex);
            }
        }

        /// <summary>
        /// جستجوی بیمه‌های بیماران فعال بر اساس عبارت جستجو
        /// </summary>
        public async Task<List<PatientInsurance>> SearchActiveAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return await GetActiveAsync();

                var term = searchTerm.Trim();
                return await _context.PatientInsurances
                    .Where(pi => pi.IsActive && 
                                (pi.PolicyNumber.Contains(term) || 
                                 pi.Patient.FirstName.Contains(term) || 
                                 pi.Patient.LastName.Contains(term) ||
                                 pi.Patient.NationalCode.Contains(term) ||
                                 pi.InsurancePlan.Name.Contains(term) ||
                                 pi.InsurancePlan.InsuranceProvider.Name.Contains(term)))
                    .Include(pi => pi.Patient)
                    .Include(pi => pi.InsurancePlan.InsuranceProvider)
                    .OrderBy(pi => pi.Patient.FirstName)
                    .ThenBy(pi => pi.Patient.LastName)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در جستجوی بیمه‌های بیماران فعال. SearchTerm: {SearchTerm}", searchTerm);
                throw new InvalidOperationException($"خطا در جستجوی بیمه‌های بیماران فعال با عبارت {searchTerm}", ex);
            }
        }

        /// <summary>
        /// جستجوی بیمه‌های بیمار بر اساس شناسه بیمار و عبارت جستجو
        /// </summary>
        public async Task<List<PatientInsurance>> SearchByPatientAsync(int patientId, string searchTerm)
        {
            try
            {
                var query = _context.PatientInsurances
                    .Where(pi => pi.PatientId == patientId);

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var term = searchTerm.Trim();
                    query = query.Where(pi => pi.PolicyNumber.Contains(term) || 
                                            pi.InsurancePlan.Name.Contains(term) ||
                                            pi.InsurancePlan.InsuranceProvider.Name.Contains(term));
                }

                return await query
                    .Include(pi => pi.InsurancePlan.InsuranceProvider)
                    .OrderBy(pi => pi.IsPrimary ? 0 : 1)
                    .ThenBy(pi => pi.StartDate)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در جستجوی بیمه‌های بیمار. PatientId: {PatientId}, SearchTerm: {SearchTerm}", patientId, searchTerm);
                throw new InvalidOperationException($"خطا در جستجوی بیمه‌های بیمار {patientId} با عبارت {searchTerm}", ex);
            }
        }

        #endregion

        #region CRUD Operations

        /// <summary>
        /// افزودن بیمه بیمار جدید
        /// </summary>
        public void Add(PatientInsurance patientInsurance)
        {
            try
            {
                if (patientInsurance == null)
                    throw new ArgumentNullException(nameof(patientInsurance));

                // تنظیم اطلاعات Audit
                var currentUser = _currentUserService.GetCurrentUserId();
                patientInsurance.CreatedAt = DateTime.Now;
                patientInsurance.CreatedByUserId = currentUser;
                patientInsurance.UpdatedAt = DateTime.Now;
                patientInsurance.UpdatedByUserId = currentUser;

                _context.PatientInsurances.Add(patientInsurance);
                _logger.Information("بیمه بیمار جدید اضافه شد. PatientId: {PatientId}, PolicyNumber: {PolicyNumber}, PlanId: {PlanId}", 
                    patientInsurance.PatientId, patientInsurance.PolicyNumber, patientInsurance.InsurancePlanId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در افزودن بیمه بیمار. PatientId: {PatientId}, PolicyNumber: {PolicyNumber}, PlanId: {PlanId}", 
                    patientInsurance?.PatientId, patientInsurance?.PolicyNumber, patientInsurance?.InsurancePlanId);
                throw new InvalidOperationException("خطا در افزودن بیمه بیمار", ex);
            }
        }

        /// <summary>
        /// به‌روزرسانی بیمه بیمار
        /// </summary>
        public void Update(PatientInsurance patientInsurance)
        {
            try
            {
                if (patientInsurance == null)
                    throw new ArgumentNullException(nameof(patientInsurance));

                // 🏥 Medical Environment: تنظیم اطلاعات Audit (تصحیح شده)
                var currentUser = _currentUserService.GetCurrentUserId();
                patientInsurance.UpdatedAt = DateTime.UtcNow;
                patientInsurance.UpdatedByUserId = currentUser; // تصحیح: باید UpdatedByUserId باشد، نه CreatedByUserId

                _context.Entry(patientInsurance).State = EntityState.Modified;
                _logger.Information("بیمه بیمار به‌روزرسانی شد. PatientInsuranceId: {PatientInsuranceId}, PatientId: {PatientId}", 
                    patientInsurance.PatientInsuranceId, patientInsurance.PatientId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی بیمه بیمار. PatientInsuranceId: {PatientInsuranceId}", patientInsurance?.PatientInsuranceId);
                throw new InvalidOperationException("خطا در به‌روزرسانی بیمه بیمار", ex);
            }
        }

        /// <summary>
        /// حذف نرم بیمه بیمار
        /// </summary>
        public void Delete(PatientInsurance patientInsurance)
        {
            try
            {
                if (patientInsurance == null)
                    throw new ArgumentNullException(nameof(patientInsurance));

                // تنظیم اطلاعات Audit برای حذف نرم
                var currentUser = _currentUserService.GetCurrentUserId();
                patientInsurance.IsDeleted = true;
                patientInsurance.UpdatedAt = DateTime.Now;
                patientInsurance.CreatedByUserId = currentUser;

                _context.Entry(patientInsurance).State = EntityState.Modified;
                _logger.Information("بیمه بیمار حذف شد. PatientInsuranceId: {PatientInsuranceId}, PatientId: {PatientId}", 
                    patientInsurance.PatientInsuranceId, patientInsurance.PatientId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف بیمه بیمار. PatientInsuranceId: {PatientInsuranceId}", patientInsurance?.PatientInsuranceId);
                throw new InvalidOperationException("خطا در حذف بیمه بیمار", ex);
            }
        }

        #endregion

        #region Active Insurance Operations

        public async Task<ServiceResult<PatientInsurance>> GetActiveByPatientAsync(int patientId)
        {
            try
            {
                _logger.Information("Getting active patient insurance for PatientId: {PatientId}", patientId);

                var patientInsurance = await _context.PatientInsurances
                    .Include(pi => pi.Patient)
                    .Include(pi => pi.InsurancePlan.InsuranceProvider)
                    .FirstOrDefaultAsync(pi => pi.PatientId == patientId && 
                                              pi.IsActive && 
                                              !pi.IsDeleted);

                if (patientInsurance == null)
                {
                    return ServiceResult<PatientInsurance>.Failed("بیمه فعال برای بیمار یافت نشد");
                }

                return ServiceResult<PatientInsurance>.Successful(patientInsurance);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting active patient insurance for PatientId: {PatientId}", patientId);
                return ServiceResult<PatientInsurance>.Failed("خطا در دریافت بیمه فعال بیمار");
            }
        }

        #endregion

        #region IPatientInsuranceRepository Implementation

        public async Task<ServiceResult<PagedResult<PatientInsuranceIndexViewModel>>> GetPagedAsync(int? patientId, string searchTerm, int pageNumber, int pageSize)
        {
            try
            {
                _logger.Information("Getting paged patient insurances with PatientId: {PatientId}, SearchTerm: {SearchTerm}, Page: {PageNumber}, Size: {PageSize}", 
                    patientId, searchTerm, pageNumber, pageSize);

                // بهینه‌سازی: استفاده از AsNoTracking برای بهبود عملکرد
                var query = _context.PatientInsurances
                    .AsNoTracking()
                    .Include(pi => pi.Patient)
                    .Include(pi => pi.InsurancePlan.InsuranceProvider)
                    .Include(pi => pi.CreatedByUser)
                    .Where(pi => !pi.IsDeleted);

                // Filter by patient if specified
                if (patientId.HasValue)
                {
                    query = query.Where(pi => pi.PatientId == patientId.Value);
                }

                // Apply search filter with optimized search
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var searchTermLower = searchTerm.ToLower();
                    query = query.Where(pi => pi.PolicyNumber.ToLower().Contains(searchTermLower) ||
                                            (pi.Patient != null && pi.Patient.FullName.ToLower().Contains(searchTermLower)) ||
                                            (pi.InsurancePlan != null && pi.InsurancePlan.Name.ToLower().Contains(searchTermLower)));
                }

                // بهینه‌سازی: استفاده از CountAsync به صورت جداگانه برای بهبود عملکرد
                var totalCount = await query.CountAsync();
                
                // بهینه‌سازی: استفاده از Select قبل از Skip/Take برای کاهش حجم داده
                var items = await query
                    .OrderByDescending(pi => pi.CreatedAt)
                    .Select(pi => new PatientInsuranceIndexViewModel
                    {
                        PatientInsuranceId = pi.PatientInsuranceId,
                        PatientId = pi.PatientId,
                        PatientName = pi.Patient != null ? pi.Patient.FullName : null,
                        PatientCode = pi.Patient != null ? pi.Patient.PatientCode : null,
                        PatientNationalCode = pi.Patient != null ? pi.Patient.NationalCode : null,
                        InsurancePlanId = pi.InsurancePlanId,
                        PolicyNumber = pi.PolicyNumber,
                        InsurancePlanName = pi.InsurancePlan != null ? pi.InsurancePlan.Name : null,
                        InsuranceProviderName = pi.InsurancePlan != null && pi.InsurancePlan.InsuranceProvider != null ? pi.InsurancePlan.InsuranceProvider.Name : null,
                        CoveragePercent = pi.InsurancePlan != null ? pi.InsurancePlan.CoveragePercent : 0,
                        IsPrimary = pi.IsPrimary,
                        StartDate = pi.StartDate,
                        EndDate = pi.EndDate,
                        IsActive = pi.IsActive,
                        CreatedAt = pi.CreatedAt,
                        CreatedByUserName = pi.CreatedByUser != null ? pi.CreatedByUser.UserName : null
                    })
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var result = new PagedResult<PatientInsuranceIndexViewModel>
                {
                    Items = items,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalItems = totalCount
                };

                _logger.Information("Successfully retrieved {Count} patient insurances out of {Total} total records", 
                    items.Count, totalCount);

                return ServiceResult<PagedResult<PatientInsuranceIndexViewModel>>.Successful(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting paged patient insurances with PatientId: {PatientId}", patientId);
                return ServiceResult<PagedResult<PatientInsuranceIndexViewModel>>.Failed("خطا در دریافت لیست بیمه‌های بیماران");
            }
        }

        /// <summary>
        /// دریافت بهینه‌سازی شده لیست بیمه‌های بیماران با فیلترهای پیشرفته
        /// </summary>
        public async Task<ServiceResult<PagedResult<PatientInsuranceIndexViewModel>>> GetPagedOptimizedAsync(
            int? patientId = null, 
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
                _logger.Information("Getting optimized paged patient insurances with filters. Page: {PageNumber}, Size: {PageSize}", 
                    pageNumber, pageSize);

                // Debug: بررسی تعداد کل رکوردها
                var totalRecords = await _context.PatientInsurances.CountAsync(pi => !pi.IsDeleted);
                _logger.Information("Total PatientInsurances records in database: {TotalRecords}", totalRecords);
                
                // Debug: بررسی رکوردهای بدون Patient یا InsurancePlan
                var recordsWithoutPatient = await _context.PatientInsurances
                    .Where(pi => !pi.IsDeleted && pi.Patient == null)
                    .CountAsync();
                var recordsWithoutPlan = await _context.PatientInsurances
                    .Where(pi => !pi.IsDeleted && pi.InsurancePlan == null)
                    .CountAsync();
                _logger.Information("Records without Patient: {CountWithoutPatient}, Records without InsurancePlan: {CountWithoutPlan}", 
                    recordsWithoutPatient, recordsWithoutPlan);

                // بهینه‌سازی: استفاده از AsNoTracking برای بهبود عملکرد
                var query = _context.PatientInsurances
                    .AsNoTracking()
                    .Where(pi => !pi.IsDeleted);

                // اعمال فیلترها
                if (patientId.HasValue)
                    query = query.Where(pi => pi.PatientId == patientId.Value);

                if (planId.HasValue)
                    query = query.Where(pi => pi.InsurancePlanId == planId.Value);

                if (isPrimary.HasValue)
                    query = query.Where(pi => pi.IsPrimary == isPrimary.Value);

                if (isActive.HasValue)
                    query = query.Where(pi => pi.IsActive == isActive.Value);

                if (fromDate.HasValue)
                    query = query.Where(pi => pi.StartDate >= fromDate.Value);

                if (toDate.HasValue)
                    query = query.Where(pi => pi.EndDate <= toDate.Value);

                // اضافه کردن Include statements به query اصلی
                query = query
                    .Include(pi => pi.Patient)
                    .Include(pi => pi.InsurancePlan)
                    .Include(pi => pi.InsurancePlan.InsuranceProvider)
                    .Include(pi => pi.SupplementaryInsuranceProvider) // بیمه‌گذار تکمیلی
                    .Include(pi => pi.SupplementaryInsurancePlan) // طرح بیمه تکمیلی
                    .Include(pi => pi.CreatedByUser);

                // اعمال فیلتر providerId بعد از Include
                if (providerId.HasValue)
                    query = query.Where(pi => pi.InsurancePlan.InsuranceProviderId == providerId.Value);

                // جستجوی کامل در نام بیمار، کد ملی و شماره بیمه
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var searchTermLower = searchTerm.ToLower();
                    query = query.Where(pi => 
                        pi.PolicyNumber.ToLower().Contains(searchTermLower) ||
                        (pi.Patient != null && (
                            pi.Patient.FirstName.ToLower().Contains(searchTermLower) ||
                            pi.Patient.LastName.ToLower().Contains(searchTermLower) ||
                            pi.Patient.NationalCode.ToLower().Contains(searchTermLower) ||
                            pi.Patient.PatientCode.ToLower().Contains(searchTermLower)
                        )));
                }

                // شمارش کل رکوردها
                var totalCount = await query.CountAsync();
                _logger.Information("Total count after filters: {TotalCount}", totalCount);
                
                // Debug: بررسی query قبل از Select
                _logger.Information("Query before Select - PatientInsurances count: {Count}", 
                    await _context.PatientInsurances.Where(pi => !pi.IsDeleted).CountAsync());
                
                // دریافت داده‌ها با pagination - ابتدا Entity ها را دریافت می‌کنیم
                var entities = await query
                    .OrderByDescending(pi => pi.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // تبدیل Entity ها به ViewModel با فرمت تاریخ صحیح
                var items = entities.Select(pi => new PatientInsuranceIndexViewModel
                {
                    PatientInsuranceId = pi.PatientInsuranceId,
                    PatientId = pi.PatientId,
                    PatientName = pi.Patient != null ? $"{pi.Patient.FirstName} {pi.Patient.LastName}".Trim() : "نام نامشخص",
                    PatientCode = pi.Patient != null ? pi.Patient.PatientCode : "کد نامشخص",
                    PatientNationalCode = pi.Patient != null ? pi.Patient.NationalCode : "کد ملی نامشخص",
                    InsurancePlanId = pi.InsurancePlanId,
                    PolicyNumber = pi.PolicyNumber,
                    InsurancePlanName = pi.InsurancePlan != null ? pi.InsurancePlan.Name : "طرح نامشخص",
                    InsuranceProviderName = pi.InsurancePlan != null && pi.InsurancePlan.InsuranceProvider != null ? pi.InsurancePlan.InsuranceProvider.Name : "ارائه‌دهنده نامشخص",
                    CoveragePercent = pi.InsurancePlan != null ? pi.InsurancePlan.CoveragePercent : 0,
                    IsPrimary = pi.IsPrimary,
                    IsActive = pi.IsActive,
                    StartDateShamsi = pi.StartDate.ToPersianDate(), // تبدیل صحیح به شمسی
                    EndDateShamsi = pi.EndDate.HasValue ? pi.EndDate.Value.ToPersianDate() : null,
                    CreatedAt = pi.CreatedAt,
                    CreatedAtShamsi = pi.CreatedAt.ToPersianDate(), // تبدیل صحیح به شمسی
                    CreatedByUserName = "سیستم", // موقتاً ساده می‌کنیم
                    // 🏥 Medical Environment: فیلدهای بیمه تکمیلی
                    SupplementaryInsuranceProviderId = pi.SupplementaryInsuranceProviderId,
                    SupplementaryInsuranceProviderName = pi.SupplementaryInsuranceProvider?.Name,
                    SupplementaryInsurancePlanId = pi.SupplementaryInsurancePlanId,
                    SupplementaryInsurancePlanName = pi.SupplementaryInsurancePlan?.Name,
                    SupplementaryPolicyNumber = pi.SupplementaryPolicyNumber,
                    HasSupplementaryInsurance = pi.SupplementaryInsuranceProviderId.HasValue && 
                                            pi.SupplementaryInsuranceProviderId.Value > 0
                }).ToList();

                _logger.Information("Retrieved {ItemCount} items from database", items.Count);

                var result = new PagedResult<PatientInsuranceIndexViewModel>
                {
                    Items = items,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalItems = totalCount
                };

                _logger.Information("Successfully retrieved {Count} patient insurances out of {Total} total records with optimized query", 
                    items.Count, totalCount);

                return ServiceResult<PagedResult<PatientInsuranceIndexViewModel>>.Successful(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting optimized paged patient insurances");
                return ServiceResult<PagedResult<PatientInsuranceIndexViewModel>>.Failed("خطا در دریافت لیست بیمه‌های بیماران");
            }
        }

        /// <summary>
        /// متد debug برای بررسی داده‌های موجود
        /// </summary>
        public async Task<ServiceResult<int>> GetTotalRecordsCountAsync()
        {
            try
            {
                var totalCount = await _context.PatientInsurances.CountAsync(pi => !pi.IsDeleted);
                _logger.Information("Total PatientInsurances records: {TotalCount}", totalCount);
                
                return ServiceResult<int>.Successful(totalCount, "تعداد کل رکوردها دریافت شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting total records count");
                return ServiceResult<int>.Failed("خطا در دریافت تعداد رکوردها");
            }
        }

        public async Task<ServiceResult<List<object>>> GetSimpleListAsync()
        {
            try
            {
                var records = await _context.PatientInsurances
                    .Where(pi => !pi.IsDeleted)
                    .Select(pi => new
                    {
                        pi.PatientInsuranceId,
                        pi.PatientId,
                        pi.InsurancePlanId,
                        pi.PolicyNumber,
                        pi.IsPrimary,
                        pi.IsActive,
                        pi.CreatedAt
                    })
                    .ToListAsync();

                var result = records.Cast<object>().ToList();
                _logger.Information("Simple list retrieved: {Count} records", result.Count);
                return ServiceResult<List<object>>.Successful(result, "لیست ساده دریافت شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting simple list");
                return ServiceResult<List<object>>.Failed("خطا در دریافت لیست ساده");
            }
        }

        #endregion

        #region Database Operations

        /// <summary>
        /// ذخیره تغییرات در پایگاه داده
        /// </summary>
        public async Task<int> SaveChangesAsync()
        {
            try
            {
                var result = await _context.SaveChangesAsync();
                _logger.Information("تغییرات بیمه‌های بیماران ذخیره شد. تعداد رکوردهای تأثیرپذیرفته: {Count}", result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ذخیره تغییرات بیمه‌های بیماران");
                throw new InvalidOperationException("خطا در ذخیره تغییرات بیمه‌های بیماران", ex);
            }
        }

        #endregion

        #region Transaction Management

        /// <summary>
        /// شروع Transaction جدید
        /// </summary>
        public async Task<System.Data.Entity.DbContextTransaction> BeginTransactionAsync()
        {
            return _context.Database.BeginTransaction();
        }

        #endregion

        #region Supplementary Insurance Methods

        /// <summary>
        /// دریافت بیمه تکمیلی فعال بیمار
        /// </summary>
        public async Task<PatientInsurance> GetActiveSupplementaryByPatientIdAsync(int patientId, DateTime? calculationDate = null)
        {
            try
            {
                var date = calculationDate ?? DateTime.Now;
                _logger.Information("درخواست بیمه تکمیلی فعال بیمار. PatientId: {PatientId}, Date: {Date}", patientId, date);

                var activeSupplementary = await _context.PatientInsurances
                    .Where(pi => pi.PatientId == patientId && 
                                !pi.IsPrimary && 
                                pi.IsActive && 
                                !pi.IsDeleted &&
                                pi.StartDate <= date &&
                                (pi.EndDate == null || pi.EndDate >= date))
                    .Include(pi => pi.Patient)
                    .Include(pi => pi.InsurancePlan)
                    .Include(pi => pi.InsurancePlan.InsuranceProvider)
                    .OrderByDescending(pi => pi.StartDate)
                    .FirstOrDefaultAsync();

                if (activeSupplementary != null)
                {
                    _logger.Information("بیمه تکمیلی فعال بیمار یافت شد. PatientId: {PatientId}, InsuranceId: {InsuranceId}", 
                        patientId, activeSupplementary.PatientInsuranceId);
                }
                else
                {
                    _logger.Warning("بیمه تکمیلی فعال برای بیمار یافت نشد. PatientId: {PatientId}", patientId);
                }

                return activeSupplementary;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت بیمه تکمیلی فعال بیمار. PatientId: {PatientId}", patientId);
                throw new InvalidOperationException($"خطا در دریافت بیمه تکمیلی فعال بیمار {patientId}", ex);
            }
        }

        /// <summary>
        /// بررسی وجود بیمه تکمیلی فعال برای بیمار
        /// </summary>
        public async Task<bool> HasActiveSupplementaryInsuranceAsync(int patientId, DateTime? calculationDate = null)
        {
            try
            {
                var date = calculationDate ?? DateTime.Now;
                _logger.Information("بررسی وجود بیمه تکمیلی فعال. PatientId: {PatientId}, Date: {Date}", patientId, date);

                var hasActive = await _context.PatientInsurances
                    .AnyAsync(pi => pi.PatientId == patientId && 
                                   !pi.IsPrimary && 
                                   pi.IsActive && 
                                   !pi.IsDeleted &&
                                   pi.StartDate <= date &&
                                   (pi.EndDate == null || pi.EndDate >= date));

                _logger.Information("نتیجه بررسی بیمه تکمیلی فعال. PatientId: {PatientId}, HasActive: {HasActive}", 
                    patientId, hasActive);

                return hasActive;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی وجود بیمه تکمیلی فعال. PatientId: {PatientId}", patientId);
                throw new InvalidOperationException($"خطا در بررسی وجود بیمه تکمیلی فعال بیمار {patientId}", ex);
            }
        }

        /// <summary>
        /// دریافت بیمه اصلی بیمار
        /// </summary>
        public async Task<PatientInsurance> GetPrimaryInsuranceByPatientIdAsync(int patientId)
        {
            try
            {
                _logger.Information("Getting primary insurance for PatientId: {PatientId}. User: {UserName} (Id: {UserId})", 
                    patientId, _currentUserService.UserName, _currentUserService.UserId);

                return await _context.PatientInsurances
                    .Where(pi => pi.PatientId == patientId && 
                                pi.IsPrimary == true && 
                                pi.IsActive == true && 
                                pi.IsDeleted == false)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting primary insurance for PatientId: {PatientId}. User: {UserName} (Id: {UserId})", 
                    patientId, _currentUserService.UserName, _currentUserService.UserId);
                throw;
            }
        }

        /// <summary>
        /// دریافت بیمه اصلی بیمار بر اساس شماره بیمه
        /// </summary>
        public async Task<PatientInsurance> GetPrimaryInsuranceByPolicyNumberAsync(int patientId, string policyNumber)
        {
            try
            {
                _logger.Information("Getting primary insurance by policy number for PatientId: {PatientId}, PolicyNumber: {PolicyNumber}. User: {UserName} (Id: {UserId})", 
                    patientId, policyNumber, _currentUserService.UserName, _currentUserService.UserId);

                return await _context.PatientInsurances
                    .Where(pi => pi.PatientId == patientId && 
                                pi.PolicyNumber == policyNumber && 
                                pi.IsPrimary == true && 
                                pi.IsActive == true && 
                                pi.IsDeleted == false)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting primary insurance by policy number: PatientId: {PatientId}, PolicyNumber: {PolicyNumber}. User: {UserName} (Id: {UserId})", 
                    patientId, policyNumber, _currentUserService.UserName, _currentUserService.UserId);
                throw;
            }
        }

        /// <summary>
        /// دریافت آمار بیمه‌های تکمیلی
        /// </summary>
        public async Task<Dictionary<string, int>> GetSupplementaryInsuranceStatisticsAsync()
        {
            try
            {
                _logger.Information("درخواست آمار بیمه‌های تکمیلی");

                var totalSupplementary = await _context.PatientInsurances
                    .Where(pi => !pi.IsPrimary && !pi.IsDeleted)
                    .CountAsync();

                var activeSupplementary = await _context.PatientInsurances
                    .Where(pi => !pi.IsPrimary && pi.IsActive && !pi.IsDeleted)
                    .CountAsync();

                var expiredSupplementary = await _context.PatientInsurances
                    .Where(pi => !pi.IsPrimary && !pi.IsDeleted && pi.EndDate.HasValue && pi.EndDate < DateTime.Now)
                    .CountAsync();

                var statistics = new Dictionary<string, int>
                {
                    { "TotalSupplementary", totalSupplementary },
                    { "ActiveSupplementary", activeSupplementary },
                    { "ExpiredSupplementary", expiredSupplementary }
                };

                _logger.Information("آمار بیمه‌های تکمیلی با موفقیت دریافت شد. Total: {Total}, Active: {Active}, Expired: {Expired}", 
                    totalSupplementary, activeSupplementary, expiredSupplementary);

                return statistics;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت آمار بیمه‌های تکمیلی");
                throw new InvalidOperationException("خطا در دریافت آمار بیمه‌های تکمیلی", ex);
            }
        }

        #endregion

        #region New Methods for PatientInsuranceManagementService

        /// <summary>
        /// دریافت بیمه پایه فعال بیمار
        /// </summary>
        public async Task<PatientInsurance> GetActivePrimaryInsuranceAsync(int patientId)
        {
            try
            {
                _logger.Information("درخواست بیمه پایه فعال بیمار. PatientId: {PatientId}", patientId);

                var primaryInsurance = await _context.PatientInsurances
                    .Where(pi => pi.PatientId == patientId && 
                                pi.IsPrimary == true && 
                                pi.IsActive == true && 
                                pi.IsDeleted == false)
                    .FirstOrDefaultAsync();

                _logger.Information("بیمه پایه فعال بیمار دریافت شد. PatientId: {PatientId}, Found: {Found}", 
                    patientId, primaryInsurance != null);

                return primaryInsurance;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت بیمه پایه فعال بیمار. PatientId: {PatientId}", patientId);
                throw;
            }
        }

        /// <summary>
        /// دریافت بیمه تکمیلی فعال بیمار
        /// </summary>
        public async Task<PatientInsurance> GetActiveSupplementaryInsuranceAsync(int patientId)
        {
            try
            {
                _logger.Information("درخواست بیمه تکمیلی فعال بیمار. PatientId: {PatientId}", patientId);

                var supplementaryInsurance = await _context.PatientInsurances
                    .Where(pi => pi.PatientId == patientId && 
                                pi.IsPrimary == false && 
                                pi.IsActive == true && 
                                pi.IsDeleted == false)
                    .FirstOrDefaultAsync();

                _logger.Information("بیمه تکمیلی فعال بیمار دریافت شد. PatientId: {PatientId}, Found: {Found}", 
                    patientId, supplementaryInsurance != null);

                return supplementaryInsurance;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت بیمه تکمیلی فعال بیمار. PatientId: {PatientId}", patientId);
                throw;
            }
        }

        /// <summary>
        /// افزودن بیمه بیمار جدید (Async)
        /// </summary>
        public async Task<ServiceResult<PatientInsurance>> CreateAsync(PatientInsurance patientInsurance)
        {
            try
            {
                _logger.Information("شروع ایجاد بیمه بیمار جدید. PatientId: {PatientId}, IsPrimary: {IsPrimary}", 
                    patientInsurance.PatientId, patientInsurance.IsPrimary);

                // اعتبارسنجی
                if (patientInsurance == null)
                {
                    return ServiceResult<PatientInsurance>.Failed("بیمه بیمار نمی‌تواند خالی باشد");
                }

                if (patientInsurance.PatientId <= 0)
                {
                    return ServiceResult<PatientInsurance>.Failed("شناسه بیمار نامعتبر است");
                }

                if (string.IsNullOrWhiteSpace(patientInsurance.PolicyNumber))
                {
                    return ServiceResult<PatientInsurance>.Failed("شماره بیمه‌نامه الزامی است");
                }

                // بررسی تداخل شماره بیمه
                var existingPolicy = await _context.PatientInsurances
                    .Where(pi => pi.PolicyNumber == patientInsurance.PolicyNumber && 
                                pi.IsDeleted == false)
                    .FirstOrDefaultAsync();

                if (existingPolicy != null)
                {
                    return ServiceResult<PatientInsurance>.Failed("شماره بیمه‌نامه قبلاً استفاده شده است");
                }

                // افزودن به دیتابیس
                _context.PatientInsurances.Add(patientInsurance);
                await _context.SaveChangesAsync();

                _logger.Information("بیمه بیمار با موفقیت ایجاد شد. PatientId: {PatientId}, InsuranceId: {InsuranceId}", 
                    patientInsurance.PatientId, patientInsurance.PatientInsuranceId);

                return ServiceResult<PatientInsurance>.Successful(patientInsurance, "بیمه بیمار با موفقیت ایجاد شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ایجاد بیمه بیمار. PatientId: {PatientId}", patientInsurance?.PatientId);
                return ServiceResult<PatientInsurance>.Failed("خطا در ایجاد بیمه بیمار: " + ex.Message);
            }
        }

        /// <summary>
        /// به‌روزرسانی بیمه بیمار (Async)
        /// </summary>
        public async Task<ServiceResult<PatientInsurance>> UpdateAsync(PatientInsurance patientInsurance)
        {
            try
            {
                _logger.Information("شروع به‌روزرسانی بیمه بیمار. InsuranceId: {InsuranceId}, PatientId: {PatientId}", 
                    patientInsurance.PatientInsuranceId, patientInsurance.PatientId);

                // اعتبارسنجی
                if (patientInsurance == null)
                {
                    return ServiceResult<PatientInsurance>.Failed("بیمه بیمار نمی‌تواند خالی باشد");
                }

                if (patientInsurance.PatientInsuranceId <= 0)
                {
                    return ServiceResult<PatientInsurance>.Failed("شناسه بیمه نامعتبر است");
                }

                // بررسی وجود بیمه
                var existingInsurance = await _context.PatientInsurances
                    .Where(pi => pi.PatientInsuranceId == patientInsurance.PatientInsuranceId && pi.IsDeleted == false)
                    .FirstOrDefaultAsync();

                if (existingInsurance == null)
                {
                    return ServiceResult<PatientInsurance>.Failed("بیمه مورد نظر یافت نشد");
                }

                // به‌روزرسانی
                _context.Entry(existingInsurance).CurrentValues.SetValues(patientInsurance);
                await _context.SaveChangesAsync();

                _logger.Information("بیمه بیمار با موفقیت به‌روزرسانی شد. InsuranceId: {InsuranceId}", patientInsurance.PatientInsuranceId);

                return ServiceResult<PatientInsurance>.Successful(patientInsurance, "بیمه بیمار با موفقیت به‌روزرسانی شد");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی بیمه بیمار. InsuranceId: {InsuranceId}", patientInsurance?.PatientInsuranceId);
                return ServiceResult<PatientInsurance>.Failed("خطا در به‌روزرسانی بیمه بیمار: " + ex.Message);
            }
        }

        #endregion
    }
}
