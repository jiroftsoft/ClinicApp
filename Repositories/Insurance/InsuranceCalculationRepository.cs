using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Interfaces.Insurance;
using ClinicApp.Interfaces;
using ClinicApp.Models;
using ClinicApp.Models.Entities;
using ClinicApp.Models.Entities.Insurance;
using ClinicApp.Models.Enums;
using Serilog;

namespace ClinicApp.Repositories.Insurance
{
    /// <summary>
    /// پیاده‌سازی Repository برای مدیریت محاسبات بیمه در سیستم کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. پیاده‌سازی کامل مدیریت محاسبات بیمه (محاسبه سهم بیمه، سهم بیمار، فرانشیز)
    /// 2. رعایت استانداردهای پزشکی ایران در محاسبات بیمه‌ای
    /// 3. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ تاریخچه محاسبات
    /// 4. مدیریت کامل ردیابی (Audit Trail) برای حسابرسی و امنیت سیستم
    /// 5. بهینه‌سازی عملکرد با AsNoTracking و Include
    /// 
    /// نکته حیاتی: این کلاس بر اساس استانداردهای سیستم‌های پزشکی ایران پیاده‌سازی شده است
    /// </summary>
    public class InsuranceCalculationRepository : IInsuranceCalculationRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;
        private readonly ICurrentUserService _currentUserService;

        public InsuranceCalculationRepository(ApplicationDbContext context, ILogger logger, ICurrentUserService currentUserService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        #region Core CRUD Operations

        /// <summary>
        /// دریافت محاسبه بیمه بر اساس شناسه
        /// </summary>
        public async Task<InsuranceCalculation> GetByIdAsync(int id)
        {
            try
            {
                return await _context.InsuranceCalculations
                    .Where(ic => ic.InsuranceCalculationId == id)
                    .AsNoTracking()
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت محاسبه بیمه با شناسه {InsuranceCalculationId}", id);
                throw;
            }
        }

        /// <summary>
        /// دریافت محاسبه بیمه بر اساس شناسه همراه با جزئیات کامل
        /// </summary>
        public async Task<InsuranceCalculation> GetByIdWithDetailsAsync(int id)
        {
            try
            {
                return await _context.InsuranceCalculations
                    .Include(ic => ic.Patient)
                    .Include(ic => ic.Service)
                    .Include(ic => ic.InsurancePlan)
                    .Include(ic => ic.PatientInsurance)
                    .Include(ic => ic.Reception)
                    .Include(ic => ic.Appointment)
                    .Include(ic => ic.CreatedByUser)
                    .Include(ic => ic.UpdatedByUser)
                    .Where(ic => ic.InsuranceCalculationId == id)
                    .AsNoTracking()
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت محاسبه بیمه با جزئیات با شناسه {InsuranceCalculationId}", id);
                throw;
            }
        }

        /// <summary>
        /// دریافت تمام محاسبات بیمه
        /// </summary>
        public async Task<List<InsuranceCalculation>> GetAllAsync()
        {
            try
            {
                return await _context.InsuranceCalculations
                    .AsNoTracking()
                    .OrderByDescending(ic => ic.CalculationDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تمام محاسبات بیمه");
                throw;
            }
        }

        /// <summary>
        /// دریافت محاسبات بیمه معتبر
        /// </summary>
        public async Task<List<InsuranceCalculation>> GetValidCalculationsAsync()
        {
            try
            {
                return await _context.InsuranceCalculations
                    .Where(ic => ic.IsValid)
                    .AsNoTracking()
                    .OrderByDescending(ic => ic.CalculationDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت محاسبات بیمه معتبر");
                throw;
            }
        }

        /// <summary>
        /// افزودن محاسبه بیمه جدید
        /// </summary>
        public async Task<InsuranceCalculation> AddAsync(InsuranceCalculation calculation)
        {
            try
            {
                calculation.CreatedByUserId = _currentUserService.GetCurrentUserId();
                calculation.CreatedAt = DateTime.Now;

                _context.InsuranceCalculations.Add(calculation);
                await _context.SaveChangesAsync();

                _logger.Information("محاسبه بیمه جدید با شناسه {InsuranceCalculationId} افزوده شد", calculation.InsuranceCalculationId);
                return calculation;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در افزودن محاسبه بیمه جدید");
                throw;
            }
        }

        /// <summary>
        /// به‌روزرسانی محاسبه بیمه موجود
        /// </summary>
        public async Task<InsuranceCalculation> UpdateAsync(InsuranceCalculation calculation)
        {
            try
            {
                calculation.UpdatedByUserId = _currentUserService.GetCurrentUserId();
                calculation.UpdatedAt = DateTime.Now;

                _context.Entry(calculation).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                _logger.Information("محاسبه بیمه با شناسه {InsuranceCalculationId} به‌روزرسانی شد", calculation.InsuranceCalculationId);
                return calculation;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی محاسبه بیمه با شناسه {InsuranceCalculationId}", calculation.InsuranceCalculationId);
                throw;
            }
        }

        /// <summary>
        /// حذف نرم محاسبه بیمه
        /// </summary>
        public async Task<bool> SoftDeleteAsync(int id)
        {
            try
            {
                var calculation = await _context.InsuranceCalculations.FindAsync(id);
                if (calculation == null)
                {
                    _logger.Warning("محاسبه بیمه با شناسه {InsuranceCalculationId} یافت نشد", id);
                    return false;
                }

                calculation.DeletedByUserId = _currentUserService.GetCurrentUserId();
                    calculation.DeletedAt = DateTime.Now;
                

                calculation.IsDeleted = true;
                await _context.SaveChangesAsync();

                _logger.Information("محاسبه بیمه با شناسه {InsuranceCalculationId} حذف نرم شد", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف نرم محاسبه بیمه با شناسه {InsuranceCalculationId}", id);
                throw;
            }
        }

        #endregion

        #region Search and Filter Operations

        /// <summary>
        /// جستجوی محاسبات بیمه بر اساس بیمار
        /// </summary>
        public async Task<List<InsuranceCalculation>> GetByPatientIdAsync(int patientId)
        {
            try
            {
                return await _context.InsuranceCalculations
                    .Where(ic => ic.PatientId == patientId)
                    .AsNoTracking()
                    .OrderByDescending(ic => ic.CalculationDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت محاسبات بیمه بیمار با شناسه {PatientId}", patientId);
                throw;
            }
        }

        /// <summary>
        /// جستجوی محاسبات بیمه بر اساس خدمت
        /// </summary>
        public async Task<List<InsuranceCalculation>> GetByServiceIdAsync(int serviceId)
        {
            try
            {
                return await _context.InsuranceCalculations
                    .Where(ic => ic.ServiceId == serviceId)
                    .AsNoTracking()
                    .OrderByDescending(ic => ic.CalculationDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت محاسبات بیمه خدمت با شناسه {ServiceId}", serviceId);
                throw;
            }
        }

        /// <summary>
        /// جستجوی محاسبات بیمه بر اساس طرح بیمه
        /// </summary>
        public async Task<List<InsuranceCalculation>> GetByPlanIdAsync(int planId)
        {
            try
            {
                return await _context.InsuranceCalculations
                    .Where(ic => ic.InsurancePlanId == planId)
                    .AsNoTracking()
                    .OrderByDescending(ic => ic.CalculationDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت محاسبات بیمه طرح با شناسه {PlanId}", planId);
                throw;
            }
        }

        /// <summary>
        /// جستجوی محاسبات بیمه بر اساس بیمه بیمار
        /// </summary>
        public async Task<List<InsuranceCalculation>> GetByPatientInsuranceIdAsync(int patientInsuranceId)
        {
            try
            {
                return await _context.InsuranceCalculations
                    .Where(ic => ic.PatientInsuranceId == patientInsuranceId)
                    .AsNoTracking()
                    .OrderByDescending(ic => ic.CalculationDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت محاسبات بیمه بیمار با شناسه {PatientInsuranceId}", patientInsuranceId);
                throw;
            }
        }

        /// <summary>
        /// جستجوی محاسبات بیمه بر اساس پذیرش
        /// </summary>
        public async Task<List<InsuranceCalculation>> GetByReceptionIdAsync(int receptionId)
        {
            try
            {
                return await _context.InsuranceCalculations
                    .Where(ic => ic.ReceptionId == receptionId)
                    .AsNoTracking()
                    .OrderByDescending(ic => ic.CalculationDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت محاسبات بیمه پذیرش با شناسه {ReceptionId}", receptionId);
                throw;
            }
        }

        /// <summary>
        /// جستجوی محاسبات بیمه بر اساس قرار ملاقات
        /// </summary>
        public async Task<List<InsuranceCalculation>> GetByAppointmentIdAsync(int appointmentId)
        {
            try
            {
                return await _context.InsuranceCalculations
                    .Where(ic => ic.AppointmentId == appointmentId)
                    .AsNoTracking()
                    .OrderByDescending(ic => ic.CalculationDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت محاسبات بیمه قرار ملاقات با شناسه {AppointmentId}", appointmentId);
                throw;
            }
        }

        /// <summary>
        /// جستجوی محاسبات بیمه بر اساس نوع محاسبه
        /// </summary>
        public async Task<List<InsuranceCalculation>> GetByCalculationTypeAsync(InsuranceCalculationType calculationType)
        {
            try
            {
                return await _context.InsuranceCalculations
                    .Where(ic => ic.CalculationType == calculationType)
                    .AsNoTracking()
                    .OrderByDescending(ic => ic.CalculationDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت محاسبات بیمه نوع {CalculationType}", calculationType);
                throw;
            }
        }

        /// <summary>
        /// جستجوی محاسبات بیمه بر اساس تاریخ
        /// </summary>
        public async Task<List<InsuranceCalculation>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate)
        {
            try
            {
                return await _context.InsuranceCalculations
                    .Where(ic => ic.CalculationDate >= fromDate && ic.CalculationDate <= toDate)
                    .AsNoTracking()
                    .OrderByDescending(ic => ic.CalculationDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت محاسبات بیمه از تاریخ {FromDate} تا {ToDate}", fromDate, toDate);
                throw;
            }
        }

        /// <summary>
        /// جستجوی محاسبات بیمه بر اساس وضعیت اعتبار
        /// </summary>
        public async Task<List<InsuranceCalculation>> GetByValidityAsync(bool isValid)
        {
            try
            {
                return await _context.InsuranceCalculations
                    .Where(ic => ic.IsValid == isValid)
                    .AsNoTracking()
                    .OrderByDescending(ic => ic.CalculationDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت محاسبات بیمه با وضعیت اعتبار {IsValid}", isValid);
                throw;
            }
        }

        #endregion

        #region Advanced Operations

        /// <summary>
        /// دریافت آمار محاسبات بیمه
        /// </summary>
        public async Task<object> GetCalculationStatisticsAsync()
        {
            try
            {
                var totalCalculations = await _context.InsuranceCalculations.CountAsync();
                var validCalculations = await _context.InsuranceCalculations.CountAsync(ic => ic.IsValid);
                var invalidCalculations = totalCalculations - validCalculations;
                var totalInsuranceShare = await _context.InsuranceCalculations.SumAsync(ic => ic.InsuranceShare);
                var totalPatientShare = await _context.InsuranceCalculations.SumAsync(ic => ic.PatientShare);

                return new
                {
                    TotalCalculations = totalCalculations,
                    ValidCalculations = validCalculations,
                    InvalidCalculations = invalidCalculations,
                    TotalInsuranceShare = totalInsuranceShare,
                    TotalPatientShare = totalPatientShare,
                    AverageInsuranceShare = totalCalculations > 0 ? totalInsuranceShare / totalCalculations : 0,
                    AveragePatientShare = totalCalculations > 0 ? totalPatientShare / totalCalculations : 0
                };
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت آمار محاسبات بیمه");
                throw;
            }
        }

        /// <summary>
        /// دریافت محاسبات بیمه با صفحه‌بندی
        /// </summary>
        public async Task<(List<InsuranceCalculation> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize)
        {
            try
            {
                var query = _context.InsuranceCalculations.AsNoTracking();
                var totalCount = await query.CountAsync();
                
                var items = await query
                    .OrderByDescending(ic => ic.CalculationDate)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return (items, totalCount);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت محاسبات بیمه با صفحه‌بندی");
                throw;
            }
        }

        /// <summary>
        /// جستجوی پیشرفته محاسبات بیمه
        /// </summary>
        public async Task<(List<InsuranceCalculation> Items, int TotalCount)> SearchAsync(
            string searchTerm = null,
            int? patientId = null,
            int? serviceId = null,
            int? planId = null,
            bool? isValid = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int pageNumber = 1,
            int pageSize = 10)
        {
            try
            {
                var query = _context.InsuranceCalculations
                    .Include(ic => ic.Patient)
                    .Include(ic => ic.Service)
                    .Include(ic => ic.InsurancePlan)
                    .AsNoTracking();

                // اعمال فیلترها
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    query = query.Where(ic => 
                        ic.Notes.Contains(searchTerm) ||
                        ic.Patient.FirstName.Contains(searchTerm) ||
                        ic.Patient.LastName.Contains(searchTerm) ||
                        ic.Service.Title.Contains(searchTerm) ||
                        ic.InsurancePlan.Name.Contains(searchTerm));
                }

                if (patientId.HasValue)
                    query = query.Where(ic => ic.PatientId == patientId.Value);

                if (serviceId.HasValue)
                    query = query.Where(ic => ic.ServiceId == serviceId.Value);

                if (planId.HasValue)
                    query = query.Where(ic => ic.InsurancePlanId == planId.Value);

                if (isValid.HasValue)
                    query = query.Where(ic => ic.IsValid == isValid.Value);

                if (fromDate.HasValue)
                    query = query.Where(ic => ic.CalculationDate >= fromDate.Value);

                if (toDate.HasValue)
                    query = query.Where(ic => ic.CalculationDate <= toDate.Value);

                var totalCount = await query.CountAsync();
                
                var items = await query
                    .OrderByDescending(ic => ic.CalculationDate)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return (items, totalCount);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در جستجوی پیشرفته محاسبات بیمه");
                throw;
            }
        }

        /// <summary>
        /// بررسی وجود محاسبه بیمه برای ترکیب مشخص
        /// </summary>
        public async Task<bool> ExistsAsync(int patientId, int serviceId, int planId, int? receptionId = null)
        {
            try
            {
                var query = _context.InsuranceCalculations
                    .Where(ic => ic.PatientId == patientId && 
                                ic.ServiceId == serviceId && 
                                ic.InsurancePlanId == planId);

                if (receptionId.HasValue)
                    query = query.Where(ic => ic.ReceptionId == receptionId.Value);

                return await query.AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی وجود محاسبه بیمه");
                throw;
            }
        }

        /// <summary>
        /// دریافت آخرین محاسبه بیمه برای بیمار و خدمت
        /// </summary>
        public async Task<InsuranceCalculation> GetLatestByPatientAndServiceAsync(int patientId, int serviceId)
        {
            try
            {
                return await _context.InsuranceCalculations
                    .Where(ic => ic.PatientId == patientId && ic.ServiceId == serviceId)
                    .AsNoTracking()
                    .OrderByDescending(ic => ic.CalculationDate)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت آخرین محاسبه بیمه برای بیمار {PatientId} و خدمت {ServiceId}", patientId, serviceId);
                throw;
            }
        }

        #endregion

        #region Bulk Operations

        /// <summary>
        /// افزودن چندین محاسبه بیمه
        /// </summary>
        public async Task<List<InsuranceCalculation>> AddRangeAsync(List<InsuranceCalculation> calculations)
        {
            try
            {
                var currentUserId = _currentUserService.GetCurrentUserId();
                var currentTime = DateTime.Now;

                foreach (var calculation in calculations)
                {
                    calculation.CreatedByUserId = currentUserId;
                    calculation.CreatedAt = currentTime;
                }

                _context.InsuranceCalculations.AddRange(calculations);
                await _context.SaveChangesAsync();

                _logger.Information("{Count} محاسبه بیمه جدید افزوده شد", calculations.Count);
                return calculations;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در افزودن چندین محاسبه بیمه");
                throw;
            }
        }

        /// <summary>
        /// به‌روزرسانی وضعیت اعتبار چندین محاسبه
        /// </summary>
        public async Task<int> UpdateValidityAsync(List<int> calculationIds, bool isValid)
        {
            try
            {
                var currentUserId = _currentUserService.GetCurrentUserId();
                var currentTime = DateTime.Now;

                var calculations = await _context.InsuranceCalculations
                    .Where(ic => calculationIds.Contains(ic.InsuranceCalculationId))
                    .ToListAsync();

                foreach (var calculation in calculations)
                {
                    calculation.IsValid = isValid;
                    calculation.UpdatedByUserId = currentUserId;
                    calculation.UpdatedAt = currentTime;
                }

                var updatedCount = await _context.SaveChangesAsync();
                _logger.Information("{Count} محاسبه بیمه به‌روزرسانی شد", updatedCount);
                return updatedCount;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی وضعیت اعتبار محاسبات بیمه");
                throw;
            }
        }

        /// <summary>
        /// حذف نرم چندین محاسبه بیمه
        /// </summary>
        public async Task<int> SoftDeleteRangeAsync(List<int> calculationIds)
        {
            try
            {
                var currentUserId = _currentUserService.GetCurrentUserId();
                var currentTime = DateTime.Now;

                var calculations = await _context.InsuranceCalculations
                    .Where(ic => calculationIds.Contains(ic.InsuranceCalculationId))
                    .ToListAsync();

                foreach (var calculation in calculations)
                {
                    calculation.IsDeleted = true;
                    calculation.DeletedByUserId = currentUserId;
                    calculation.DeletedAt = currentTime;
                }

                var deletedCount = await _context.SaveChangesAsync();
                _logger.Information("{Count} محاسبه بیمه حذف نرم شد", deletedCount);
                return deletedCount;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف نرم چندین محاسبه بیمه");
                throw;
            }
        }

        #endregion
    }
}
