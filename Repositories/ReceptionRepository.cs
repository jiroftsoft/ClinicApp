using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Extensions;
using ClinicApp.Helpers;
using ClinicApp.Interfaces;
using ClinicApp.Models;
using ClinicApp.Models.Entities;
using ClinicApp.Models.Entities.Payment;
using ClinicApp.Models.Entities.Reception;
using ClinicApp.Models.Enums;
using ClinicApp.ViewModels;
using ClinicApp.ViewModels.Reception;
using Serilog;

namespace ClinicApp.Repositories
{
    /// <summary>
    /// پیاده‌سازی Repository برای مدیریت پذیرش‌های بیماران در سیستم کلینیک شفا
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. پیاده‌سازی کامل مدیریت پذیرش‌های بیماران (Normal, Emergency, Special, Online)
    /// 2. رعایت استانداردهای پزشکی ایران در مدیریت پذیرش‌ها
    /// 3. پشتیبانی از سیستم حذف نرم (Soft Delete) برای حفظ اطلاعات پزشکی
    /// 4. مدیریت کامل ردیابی (Audit Trail) برای حسابرسی و امنیت سیستم
    /// 5. بهینه‌سازی عملکرد با AsNoTracking و Include
    /// 6. مدیریت روابط با Patient، Doctor، PatientInsurance
    /// 7. مدیریت انواع پذیرش و اولویت‌ها
    /// 
    /// نکته حیاتی: این کلاس بر اساس استانداردهای سیستم‌های پزشکی ایران پیاده‌سازی شده است
    /// </summary>
    public class ReceptionRepository : IReceptionRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger _logger;
        private readonly ICurrentUserService _currentUserService;

        public ReceptionRepository(ApplicationDbContext context, ILogger logger, ICurrentUserService currentUserService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
        }

        #region Core CRUD Operations

        /// <summary>
        /// دریافت پذیرش بر اساس شناسه
        /// </summary>
        public async Task<Models.Entities.Reception.Reception> GetByIdAsync(int id)
        {
            try
            {
                return await _context.Receptions
                    .Where(r => r.ReceptionId == id && !r.IsDeleted)
                    .AsNoTracking()
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت پذیرش. ReceptionId: {ReceptionId}", id);
                throw new InvalidOperationException($"خطا در دریافت پذیرش {id}", ex);
            }
        }

        /// <summary>
        /// دریافت پذیرش بر اساس شناسه همراه با جزئیات کامل
        /// </summary>
        public async Task<Models.Entities.Reception.Reception> GetByIdWithDetailsAsync(int id)
        {
            try
            {
                return await _context.Receptions
                    .Include(r => r.Patient)
                    .Include(r => r.Doctor)
                    .Include(r => r.ActivePatientInsurance)
                    .Include(r => r.ActivePatientInsurance.InsurancePlan)
                    .Include(r => r.ActivePatientInsurance.InsurancePlan.InsuranceProvider)
                    .Include(r => r.ReceptionItems)
                    .Include(r => r.Transactions)
                    .Include(r => r.CreatedByUser)
                    .Include(r => r.UpdatedByUser)
                    .Where(r => r.ReceptionId == id && !r.IsDeleted)
                    .AsNoTracking()
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت پذیرش با جزئیات. ReceptionId: {ReceptionId}", id);
                throw new InvalidOperationException($"خطا در دریافت پذیرش با جزئیات {id}", ex);
            }
        }

        /// <summary>
        /// دریافت تمام پذیرش‌های فعال
        /// </summary>
        public async Task<List<Models.Entities.Reception.Reception>> GetAllActiveAsync()
        {
            try
            {
                return await _context.Receptions
                    .Where(r => !r.IsDeleted)
                    .AsNoTracking()
                    .OrderByDescending(r => r.ReceptionDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تمام پذیرش‌های فعال");
                throw new InvalidOperationException("خطا در دریافت تمام پذیرش‌های فعال", ex);
            }
        }

        /// <summary>
        /// دریافت پذیرش‌های بیمار
        /// </summary>
        public async Task<List<Models.Entities.Reception.Reception>> GetByPatientIdAsync(int patientId)
        {
            try
            {
                return await _context.Receptions
                    .Where(r => r.PatientId == patientId && !r.IsDeleted)
                    .AsNoTracking()
                    .OrderByDescending(r => r.ReceptionDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت پذیرش‌های بیمار. PatientId: {PatientId}", patientId);
                throw new InvalidOperationException($"خطا در دریافت پذیرش‌های بیمار {patientId}", ex);
            }
        }

        /// <summary>
        /// دریافت پذیرش‌های پزشک
        /// </summary>
        public async Task<List<Models.Entities.Reception.Reception>> GetByDoctorIdAsync(int doctorId)
        {
            try
            {
                return await _context.Receptions
                    .Where(r => r.DoctorId == doctorId && !r.IsDeleted)
                    .AsNoTracking()
                    .OrderByDescending(r => r.ReceptionDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت پذیرش‌های پزشک. DoctorId: {DoctorId}", doctorId);
                throw new InvalidOperationException($"خطا در دریافت پذیرش‌های پزشک {doctorId}", ex);
            }
        }

        /// <summary>
        /// دریافت پذیرش‌های تاریخ مشخص
        /// </summary>
        public async Task<List<Models.Entities.Reception.Reception>> GetByDateAsync(DateTime date)
        {
            try
            {
                var startDate = date.Date;
                var endDate = startDate.AddDays(1);

                return await _context.Receptions
                    .Where(r => r.ReceptionDate >= startDate && r.ReceptionDate < endDate && !r.IsDeleted)
                    .AsNoTracking()
                    .OrderBy(r => r.ReceptionDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت پذیرش‌های تاریخ. Date: {Date}", date);
                throw new InvalidOperationException($"خطا در دریافت پذیرش‌های تاریخ {date:yyyy/MM/dd}", ex);
            }
        }

        /// <summary>
        /// دریافت پذیرش‌های بازه زمانی
        /// </summary>
        public async Task<List<Models.Entities.Reception.Reception>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            try
            {
                var start = startDate.Date;
                var end = endDate.Date.AddDays(1);

                return await _context.Receptions
                    .Where(r => r.ReceptionDate >= start && r.ReceptionDate < end && !r.IsDeleted)
                    .AsNoTracking()
                    .OrderBy(r => r.ReceptionDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت پذیرش‌های بازه زمانی. StartDate: {StartDate}, EndDate: {EndDate}", startDate, endDate);
                throw new InvalidOperationException($"خطا در دریافت پذیرش‌های بازه زمانی {startDate:yyyy/MM/dd} تا {endDate:yyyy/MM/dd}", ex);
            }
        }

        #endregion

        #region Search Operations

        /// <summary>
        /// جستجوی پذیرش‌ها بر اساس کد ملی بیمار
        /// </summary>
        public async Task<List<Models.Entities.Reception.Reception>> SearchByNationalCodeAsync(string nationalCode)
        {
            try
            {
                return await _context.Receptions
                    .Include(r => r.Patient)
                    .Where(r => r.Patient.NationalCode == nationalCode && !r.IsDeleted)
                    .AsNoTracking()
                    .OrderByDescending(r => r.ReceptionDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در جستجوی پذیرش‌ها بر اساس کد ملی. NationalCode: {NationalCode}", nationalCode);
                throw new InvalidOperationException($"خطا در جستجوی پذیرش‌ها بر اساس کد ملی {nationalCode}", ex);
            }
        }

        /// <summary>
        /// جستجوی پذیرش‌ها بر اساس نام بیمار
        /// </summary>
        public async Task<List<Models.Entities.Reception.Reception>> SearchByPatientNameAsync(string patientName)
        {
            try
            {
                return await _context.Receptions
                    .Include(r => r.Patient)
                    .Where(r => (r.Patient.FirstName.Contains(patientName) || 
                                r.Patient.LastName.Contains(patientName) ||
                                (r.Patient.FirstName + " " + r.Patient.LastName).Contains(patientName)) && 
                                !r.IsDeleted)
                    .AsNoTracking()
                    .OrderByDescending(r => r.ReceptionDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در جستجوی پذیرش‌ها بر اساس نام بیمار. PatientName: {PatientName}", patientName);
                throw new InvalidOperationException($"خطا در جستجوی پذیرش‌ها بر اساس نام بیمار {patientName}", ex);
            }
        }

        /// <summary>
        /// جستجوی پذیرش‌ها بر اساس وضعیت
        /// </summary>
        public async Task<List<Models.Entities.Reception.Reception>> GetByStatusAsync(ReceptionStatus status)
        {
            try
            {
                return await _context.Receptions
                    .Where(r => r.Status == status && !r.IsDeleted)
                    .AsNoTracking()
                    .OrderByDescending(r => r.ReceptionDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در جستجوی پذیرش‌ها بر اساس وضعیت. Status: {Status}", status);
                throw new InvalidOperationException($"خطا در جستجوی پذیرش‌ها بر اساس وضعیت {status}", ex);
            }
        }

        /// <summary>
        /// جستجوی پذیرش‌ها بر اساس نوع
        /// </summary>
        public async Task<List<Models.Entities.Reception.Reception>> GetByTypeAsync(ReceptionType type)
        {
            try
            {
                return await _context.Receptions
                    .Where(r => r.Type == type && !r.IsDeleted)
                    .AsNoTracking()
                    .OrderByDescending(r => r.ReceptionDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در جستجوی پذیرش‌ها بر اساس نوع. Type: {Type}", type);
                throw new InvalidOperationException($"خطا در جستجوی پذیرش‌ها بر اساس نوع {type}", ex);
            }
        }

        /// <summary>
        /// جستجوی پذیرش‌های اورژانس
        /// </summary>
        public async Task<List<Models.Entities.Reception.Reception>> GetEmergencyReceptionsAsync()
        {
            try
            {
                return await _context.Receptions
                    .Where(r => r.IsEmergency && !r.IsDeleted)
                    .AsNoTracking()
                    .OrderByDescending(r => r.ReceptionDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در جستجوی پذیرش‌های اورژانس");
                throw new InvalidOperationException("خطا در جستجوی پذیرش‌های اورژانس", ex);
            }
        }

        #endregion

        #region Paged Operations

        /// <summary>
        /// دریافت پذیرش‌ها با صفحه‌بندی
        /// </summary>
        public async Task<ServiceResult<PagedResult<ReceptionIndexViewModel>>> GetPagedAsync(
            int? patientId, 
            int? doctorId, 
            ReceptionStatus? status, 
            string searchTerm, 
            int pageNumber, 
            int pageSize)
        {
            try
            {
                var query = _context.Receptions
                    .Include(r => r.Patient)
                    .Include(r => r.Doctor)
                    .Include(r => r.ActivePatientInsurance)
                    .Include(r => r.ActivePatientInsurance.InsurancePlan)
                    .Include(r => r.ActivePatientInsurance.InsurancePlan.InsuranceProvider)
                    .Where(r => !r.IsDeleted);

                // فیلتر بر اساس بیمار
                if (patientId.HasValue)
                    query = query.Where(r => r.PatientId == patientId.Value);

                // فیلتر بر اساس پزشک
                if (doctorId.HasValue)
                    query = query.Where(r => r.DoctorId == doctorId.Value);

                // فیلتر بر اساس وضعیت
                if (status.HasValue)
                    query = query.Where(r => r.Status == status.Value);

                // جستجو بر اساس عبارت
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    query = query.Where(r => 
                        r.Patient.FirstName.Contains(searchTerm) ||
                        r.Patient.LastName.Contains(searchTerm) ||
                        r.Patient.NationalCode.Contains(searchTerm) ||
                        r.Doctor.FirstName.Contains(searchTerm) ||
                        r.Doctor.LastName.Contains(searchTerm));
                }

                // شمارش کل رکوردها
                var totalCount = await query.CountAsync();

                // دریافت رکوردهای صفحه
                var items = await query
                    .OrderByDescending(r => r.ReceptionDate)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .AsNoTracking()
                    .ToListAsync();

                // تبدیل به ViewModel
                var viewModels = items.Select(r => new ReceptionIndexViewModel
                {
                    ReceptionId = r.ReceptionId,
                    PatientFullName = $"{r.Patient?.FirstName ?? ""} {r.Patient?.LastName ?? ""}".Trim(),
                    DoctorFullName = $"{r.Doctor?.FirstName ?? ""} {r.Doctor?.LastName ?? ""}".Trim(),
                    ReceptionDate = r.ReceptionDate != DateTime.MinValue ? r.ReceptionDate.ToPersianDateTime() : "نامشخص",
                    TotalAmount = r.TotalAmount,
                    Status = GetReceptionStatusText(r.Status)
                }).ToList();

                var pagedResult = new PagedResult<ReceptionIndexViewModel>(viewModels, totalCount, pageNumber, pageSize);

                return ServiceResult<PagedResult<ReceptionIndexViewModel>>.Successful(
                    pagedResult,
                    "دریافت لیست پذیرش‌ها با موفقیت انجام شد.",
                    operationName: "GetPagedReceptions",
                    userId: _currentUserService.UserId,
                    userFullName: _currentUserService.UserName,
                    securityLevel: Core.SecurityLevel.Low);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت پذیرش‌ها با صفحه‌بندی. جزئیات: {ExceptionMessage}, StackTrace: {StackTrace}",
                    ex.Message, ex.StackTrace);
                return ServiceResult<PagedResult<ReceptionIndexViewModel>>.Failed(
                    $"خطا در دریافت لیست پذیرش‌ها: {ex.Message}",
                    "PAGED_RECEPTIONS_ERROR",
                    Core.ErrorCategory.General,
                    Core.SecurityLevel.High);
            }
        }

        #endregion

        #region CRUD Operations

        /// <summary>
        /// افزودن پذیرش جدید
        /// </summary>
        public void Add(Models.Entities.Reception.Reception reception)
        {
            try
            {
                if (reception == null)
                    throw new ArgumentNullException(nameof(reception));

                // ✅ تنظیمات ردیابی
                reception.IsDeleted = false;
                reception.CreatedAt = DateTime.UtcNow;
                reception.CreatedByUserId = _currentUserService.UserId;
                
                // ✅ ReceptionNumber در Entity خودش تنظیم می‌شود
                // (read-only property)
                reception.UpdatedAt = null;
                reception.UpdatedByUserId = null;

                _context.Receptions.Add(reception);
                _logger.Information("پذیرش جدید اضافه شد. ReceptionId: {ReceptionId}, PatientId: {PatientId}, DoctorId: {DoctorId}", 
                    reception.ReceptionId, reception.PatientId, reception.DoctorId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در افزودن پذیرش جدید");
                throw new InvalidOperationException("خطا در افزودن پذیرش جدید", ex);
            }
        }

        /// <summary>
        /// به‌روزرسانی پذیرش
        /// </summary>
        public void Update(Models.Entities.Reception.Reception reception)
        {
            try
            {
                if (reception == null)
                    throw new ArgumentNullException(nameof(reception));

                // تنظیمات ردیابی
                reception.UpdatedAt = DateTime.UtcNow;
                reception.UpdatedByUserId = _currentUserService.UserId;

                _context.Entry(reception).State = EntityState.Modified;
                _logger.Information("پذیرش به‌روزرسانی شد. ReceptionId: {ReceptionId}", reception.ReceptionId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی پذیرش. ReceptionId: {ReceptionId}", reception.ReceptionId);
                throw new InvalidOperationException($"خطا در به‌روزرسانی پذیرش {reception.ReceptionId}", ex);
            }
        }

        /// <summary>
        /// حذف نرم پذیرش
        /// </summary>
        public void Delete(Models.Entities.Reception.Reception reception)
        {
            try
            {
                if (reception == null)
                    throw new ArgumentNullException(nameof(reception));

                // حذف نرم
                reception.IsDeleted = true;
                reception.DeletedAt = DateTime.UtcNow;
                reception.DeletedByUserId = _currentUserService.UserId;

                _context.Entry(reception).State = EntityState.Modified;
                _logger.Information("پذیرش حذف شد. ReceptionId: {ReceptionId}", reception.ReceptionId);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در حذف پذیرش. ReceptionId: {ReceptionId}", reception.ReceptionId);
                throw new InvalidOperationException($"خطا در حذف پذیرش {reception.ReceptionId}", ex);
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
                _logger.Information("تغییرات پذیرش‌ها ذخیره شد. تعداد رکوردهای تأثیرپذیرفته: {Count}", result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در ذخیره تغییرات پذیرش‌ها");
                throw new InvalidOperationException("خطا در ذخیره تغییرات پذیرش‌ها", ex);
            }
        }

        #endregion

        #region Transaction Management

        /// <summary>
        /// شروع Transaction جدید
        /// </summary>
        public System.Data.Entity.DbContextTransaction BeginTransaction()
        {
            try
            {
                return _context.Database.BeginTransaction();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در شروع Transaction");
                throw new InvalidOperationException("خطا در شروع Transaction", ex);
            }
        }

        #endregion

        #region Business Logic Operations

        /// <summary>
        /// بررسی وجود پذیرش فعال برای بیمار در تاریخ مشخص
        /// </summary>
        public async Task<bool> HasActiveReceptionAsync(int patientId, DateTime date)
        {
            try
            {
                var startDate = date.Date;
                var endDate = startDate.AddDays(1);

                return await _context.Receptions
                    .Where(r => r.PatientId == patientId && 
                                r.ReceptionDate >= startDate && 
                                r.ReceptionDate < endDate && 
                                !r.IsDeleted &&
                                r.Status != ReceptionStatus.Cancelled)
                    .AnyAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در بررسی وجود پذیرش فعال. PatientId: {PatientId}, Date: {Date}", patientId, date);
                throw new InvalidOperationException($"خطا در بررسی وجود پذیرش فعال برای بیمار {patientId} در تاریخ {date:yyyy/MM/dd}", ex);
            }
        }

        /// <summary>
        /// دریافت آخرین پذیرش بیمار
        /// </summary>
        public async Task<Models.Entities.Reception.Reception> GetLatestByPatientIdAsync(int patientId)
        {
            try
            {
                return await _context.Receptions
                    .Where(r => r.PatientId == patientId && !r.IsDeleted)
                    .AsNoTracking()
                    .OrderByDescending(r => r.ReceptionDate)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت آخرین پذیرش بیمار. PatientId: {PatientId}", patientId);
                throw new InvalidOperationException($"خطا در دریافت آخرین پذیرش بیمار {patientId}", ex);
            }
        }

        /// <summary>
        /// شمارش پذیرش‌های روز
        /// </summary>
        public async Task<int> GetDailyCountAsync(DateTime date)
        {
            try
            {
                var startDate = date.Date;
                var endDate = startDate.AddDays(1);

                return await _context.Receptions
                    .Where(r => r.ReceptionDate >= startDate && 
                                r.ReceptionDate < endDate && 
                                !r.IsDeleted)
                    .CountAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در شمارش پذیرش‌های روز. Date: {Date}", date);
                throw new InvalidOperationException($"خطا در شمارش پذیرش‌های روز {date:yyyy/MM/dd}", ex);
            }
        }

        /// <summary>
        /// شمارش پذیرش‌های پزشک در تاریخ مشخص
        /// </summary>
        public async Task<int> GetDoctorDailyCountAsync(int doctorId, DateTime date)
        {
            try
            {
                var startDate = date.Date;
                var endDate = startDate.AddDays(1);

                return await _context.Receptions
                    .Where(r => r.DoctorId == doctorId && 
                                r.ReceptionDate >= startDate && 
                                r.ReceptionDate < endDate && 
                                !r.IsDeleted)
                    .CountAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در شمارش پذیرش‌های پزشک. DoctorId: {DoctorId}, Date: {Date}", doctorId, date);
                throw new InvalidOperationException($"خطا در شمارش پذیرش‌های پزشک {doctorId} در تاریخ {date:yyyy/MM/dd}", ex);
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// تبدیل وضعیت پذیرش به متن فارسی
        /// </summary>
        private string GetReceptionStatusText(ReceptionStatus status)
        {
            try
            {
                switch (status)
                {
                    case ReceptionStatus.Pending:
                        return "در انتظار";
                    case ReceptionStatus.InProgress:
                        return "در حال انجام";
                    case ReceptionStatus.Completed:
                        return "تکمیل شده";
                    case ReceptionStatus.Cancelled:
                        return "لغو شده";
                    case ReceptionStatus.NoShow:
                        return "عدم حضور";
                    default:
                        return "نامشخص";
                }
            }
            catch (Exception ex)
            {
                _logger.Warning("خطا در تبدیل وضعیت پذیرش. وضعیت: {Status}, خطا: {Error}", status, ex.Message);
                return "نامشخص";
            }
        }

        /// <summary>
        /// تولید شماره پذیرش منحصر به فرد
        /// </summary>
        /// <returns>شماره پذیرش</returns>
        private string GenerateReceptionNumber()
        {
            try
            {
                var today = DateTime.Now;
                var year = today.Year.ToString().Substring(2); // 2 رقم آخر سال
                var month = today.Month.ToString("00");
                var day = today.Day.ToString("00");
                
                // شماره‌گذاری روزانه
                var todayStart = new DateTime(today.Year, today.Month, today.Day);
                var todayEnd = todayStart.AddDays(1);
                
                var todayCount = _context.Receptions
                    .Where(r => r.CreatedAt >= todayStart && r.CreatedAt < todayEnd && !r.IsDeleted)
                    .Count();
                
                var sequence = (todayCount + 1).ToString("000");
                
                return $"{year}{month}{day}{sequence}";
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در تولید شماره پذیرش");
                return DateTime.Now.ToString("yyyyMMddHHmmss");
            }
        }

        /// <summary>
        /// Get receptions by date
        /// </summary>
        /// <param name="date">Date</param>
        /// <returns>List of receptions</returns>
        public async Task<List<Models.Entities.Reception.Reception>> GetReceptionsByDateAsync(DateTime date)
        {
            try
            {
                _logger.Information("دریافت پذیرش‌های تاریخ {Date}", date);

                var startDate = date.Date;
                var endDate = startDate.AddDays(1);

                var receptions = await _context.Receptions
                    .Where(r => r.ReceptionDate >= startDate && r.ReceptionDate < endDate && !r.IsDeleted)
                    .Include(r => r.Patient)
                    .Include(r => r.Doctor)
                    .AsNoTracking()
                    .ToListAsync();

                _logger.Information("تعداد {Count} پذیرش برای تاریخ {Date} یافت شد", receptions.Count, date);
                return receptions;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت پذیرش‌های تاریخ {Date}", date);
                throw;
            }
        }

        /// <summary>
        /// Get receptions by doctor and date
        /// </summary>
        /// <param name="doctorId">Doctor ID</param>
        /// <param name="date">Date</param>
        /// <returns>List of receptions</returns>
        public async Task<List<Models.Entities.Reception.Reception>> GetReceptionsByDoctorAndDateAsync(int doctorId, DateTime date)
        {
            try
            {
                _logger.Information("دریافت پذیرش‌های پزشک {DoctorId} در تاریخ {Date}", doctorId, date);

                var startDate = date.Date;
                var endDate = startDate.AddDays(1);

                var receptions = await _context.Receptions
                    .Where(r => r.DoctorId == doctorId && 
                               r.ReceptionDate >= startDate && 
                               r.ReceptionDate < endDate && 
                               !r.IsDeleted)
                    .Include(r => r.Patient)
                    .Include(r => r.Doctor)
                    .AsNoTracking()
                    .ToListAsync();

                _logger.Information("تعداد {Count} پذیرش برای پزشک {DoctorId} در تاریخ {Date} یافت شد", 
                    receptions.Count, doctorId, date);
                return receptions;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت پذیرش‌های پزشک {DoctorId} در تاریخ {Date}", doctorId, date);
                throw;
            }
        }

        /// <summary>
        /// Update reception entity
        /// </summary>
        /// <param name="reception">Reception entity</param>
        /// <returns>Updated reception</returns>
        public async Task<Models.Entities.Reception.Reception> UpdateReceptionAsync(Models.Entities.Reception.Reception reception)
        {
            try
            {
                _logger.Information("به‌روزرسانی پذیرش {ReceptionId}", reception.ReceptionId);

                _context.Entry(reception).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                _logger.Information("پذیرش {ReceptionId} با موفقیت به‌روزرسانی شد", reception.ReceptionId);
                return reception;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در به‌روزرسانی پذیرش {ReceptionId}", reception.ReceptionId);
                throw;
            }
        }

        /// <summary>
        /// Get reception payments
        /// </summary>
        /// <param name="receptionId">Reception ID</param>
        /// <returns>List of payment transactions</returns>
        public async Task<List<PaymentTransaction>> GetReceptionPaymentsAsync(int receptionId)
        {
            try
            {
                _logger.Information("دریافت تراکنش‌های پرداخت پذیرش {ReceptionId}", receptionId);

                var payments = await _context.PaymentTransactions
                    .Where(p => p.ReceptionId == receptionId && !p.IsDeleted)
                    .AsNoTracking()
                    .ToListAsync();

                _logger.Information("تعداد {Count} تراکنش پرداخت برای پذیرش {ReceptionId} یافت شد", 
                    payments.Count, receptionId);
                return payments;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تراکنش‌های پرداخت پذیرش {ReceptionId}", receptionId);
                throw;
            }
        }

        /// <summary>
        /// دریافت تعداد پذیرش‌های پزشک
        /// </summary>
        /// <param name="doctorId">شناسه پزشک</param>
        /// <returns>تعداد پذیرش‌ها</returns>
        public async Task<int> GetReceptionCountByDoctorAsync(int doctorId)
        {
            try
            {
                _logger.Debug("دریافت تعداد پذیرش‌های پزشک. شناسه پزشک: {DoctorId}", doctorId);

                var count = await _context.Receptions
                    .CountAsync(r => r.DoctorId == doctorId && !r.IsDeleted);

                _logger.Debug("تعداد پذیرش‌های پزشک: {Count}", count);
                return count;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "خطا در دریافت تعداد پذیرش‌های پزشک");
                throw;
            }
        }

        #endregion
    }
}
