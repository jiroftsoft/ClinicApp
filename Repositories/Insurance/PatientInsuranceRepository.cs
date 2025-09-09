using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Helpers;
using ClinicApp.Interfaces.Insurance;
using ClinicApp.Interfaces;
using ClinicApp.Models;
using ClinicApp.Models.Entities;
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
                return await _context.PatientInsurances
                    .Where(pi => pi.PatientId == patientId)
                    .Include(pi => pi.InsurancePlan.InsuranceProvider)
                    .OrderBy(pi => pi.IsPrimary ? 0 : 1)
                    .ThenBy(pi => pi.StartDate)
                    .AsNoTracking()
                    .ToListAsync();
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
                    .Where(pi => pi.PatientId == patientId && pi.IsActive)
                    .Include(pi => pi.InsurancePlan.InsuranceProvider)
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
        /// </summary>
        public async Task<List<PatientInsurance>> GetSupplementaryByPatientIdAsync(int patientId)
        {
            try
            {
                return await _context.PatientInsurances
                    .Where(pi => pi.PatientId == patientId && !pi.IsPrimary && pi.IsActive)
                    .Include(pi => pi.InsurancePlan.InsuranceProvider)
                    .OrderBy(pi => pi.StartDate)
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

                // تنظیم اطلاعات Audit
                var currentUser = _currentUserService.GetCurrentUserId();
                patientInsurance.UpdatedAt = DateTime.Now;
                patientInsurance.CreatedByUserId = currentUser;

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

                var query = _context.PatientInsurances
                    .Include(pi => pi.Patient)
                    .Include(pi => pi.InsurancePlan.InsuranceProvider)
                    .Where(pi => !pi.IsDeleted);

                // Filter by patient if specified
                if (patientId.HasValue)
                {
                    query = query.Where(pi => pi.PatientId == patientId.Value);
                }

                // Apply search filter
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    query = query.Where(pi => pi.PolicyNumber.Contains(searchTerm) ||
                                            pi.Patient.FullName.Contains(searchTerm) ||
                                            pi.InsurancePlan.Name.Contains(searchTerm));
                }

                var totalCount = await query.CountAsync();
                var items = await query
                    .OrderByDescending(pi => pi.CreatedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(pi => new PatientInsuranceIndexViewModel
                    {
                        PatientInsuranceId = pi.PatientInsuranceId,
                        PatientId = pi.PatientId,
                        PatientName = pi.Patient.FullName,
                        PatientCode = pi.Patient.PatientCode,
                        InsurancePlanId = pi.InsurancePlanId,
                        PolicyNumber = pi.PolicyNumber,
                        InsurancePlanName = pi.InsurancePlan.Name,
                        InsuranceProviderName = pi.InsurancePlan.InsuranceProvider.Name,
                        CoveragePercent = pi.InsurancePlan.CoveragePercent,
                        IsPrimary = pi.IsPrimary,
                        StartDate = pi.StartDate,
                        EndDate = pi.EndDate,
                        IsActive = pi.IsActive,
                        CreatedAt = pi.CreatedAt
                    })
                    .ToListAsync();

                var result = new PagedResult<PatientInsuranceIndexViewModel>
                {
                    Items = items,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalItems = totalCount
                };

                return ServiceResult<PagedResult<PatientInsuranceIndexViewModel>>.Successful(result);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error getting paged patient insurances with PatientId: {PatientId}", patientId);
                return ServiceResult<PagedResult<PatientInsuranceIndexViewModel>>.Failed("خطا در دریافت لیست بیمه‌های بیماران");
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
    }
}
