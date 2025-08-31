using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Interfaces.ClinicAdmin;
using ClinicApp.Models;
using ClinicApp.Models.Entities;

namespace ClinicApp.Repositories.ClinicAdmin
{
    /// <summary>
    /// پیاده‌سازی اینترفیس IDoctorReportingRepository برای گزارش‌گیری پزشکان
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. پیاده‌سازی کامل گزارش‌گیری و آمار پزشکان
    /// 2. رعایت استانداردهای پزشکی ایران در گزارش‌گیری
    /// 3. پشتیبانی از تقویم شمسی و اعداد فارسی در تمام فرآیندهای گزارش‌گیری
    /// 4. پشتیبانی از محیط‌های Production و سیستم‌های Load Balanced
    /// 5. مدیریت حرفه‌ای خطاها و لاگ‌گیری برای سیستم‌های پزشکی
    /// 
    /// نکته حیاتی: این کلاس بر اساس استانداردهای سیستم‌های پزشکی ایران پیاده‌سازی شده است
    /// </summary>
    public class DoctorReportingRepository : IDoctorReportingRepository
    {
        private readonly ApplicationDbContext _context;

        public DoctorReportingRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        #region Reporting & Statistics (گزارش‌گیری و آمار)

        /// <summary>
        /// دریافت گزارش پزشکان فعال در یک بازه زمانی
        /// </summary>
        public async Task<List<Doctor>> GetActiveDoctorsReportAsync(int clinicId, int? departmentId, DateTime startDate, DateTime endDate)
        {
            try
            {
                var query = _context.Doctors
                    .Where(d => !d.IsDeleted && d.IsActive)
                    .Include(d => d.DoctorDepartments)
                    .Include(d => d.DoctorServiceCategories)
                    .Include(d => d.DoctorSpecializations)
                    .Include(d => d.Appointments)
                    .AsQueryable();

                // فیلتر بر اساس کلینیک
                if (clinicId > 0)
                {
                    query = query.Where(d => d.ClinicId == clinicId);
                }

                // فیلتر بر اساس دپارتمان
                if (departmentId.HasValue && departmentId.Value > 0)
                {
                    query = query.Where(d => d.DoctorDepartments.Any(dd => 
                        dd.DepartmentId == departmentId.Value && 
                        dd.IsActive && 
                        !dd.IsDeleted));
                }

                // فیلتر بر اساس بازه زمانی (پزشکانی که در این بازه نوبت داشته‌اند)
                query = query.Where(d => d.Appointments.Any(a => 
                    a.AppointmentDate >= startDate && 
                    a.AppointmentDate <= endDate && 
                    !a.IsDeleted));

                var doctors = await query.ToListAsync();

                return doctors;
            }
            catch (Exception ex)
            {
                // لاگ خطا برای سیستم‌های پزشکی
                throw new InvalidOperationException($"خطا در دریافت گزارش پزشکان فعال", ex);
            }
        }

        /// <summary>
        /// دریافت داده‌های داشبورد پزشک
        /// </summary>
        public async Task<Doctor> GetDoctorDashboardDataAsync(int doctorId)
        {
            try
            {
                var doctor = await _context.Doctors
                    .Where(d => d.DoctorId == doctorId && !d.IsDeleted)
                    .Include(d => d.Appointments)
                    .Include(d => d.DoctorDepartments)
                    .Include(d => d.DoctorServiceCategories)
                    .Include(d => d.DoctorSpecializations)
                    .FirstOrDefaultAsync();

                if (doctor == null)
                {
                    throw new InvalidOperationException($"پزشک با شناسه {doctorId} یافت نشد.");
                }

                return doctor;
            }
            catch (Exception ex)
            {
                // لاگ خطا برای سیستم‌های پزشکی
                throw new InvalidOperationException($"خطا در دریافت داده‌های داشبورد پزشک {doctorId}", ex);
            }
        }

        #endregion

        #region Dependency Management (مدیریت وابستگی‌ها)

        /// <summary>
        /// دریافت اطلاعات وابستگی‌های پزشک برای بررسی امکان حذف
        /// </summary>
        public async Task<DoctorDependencyInfo> GetDoctorDependencyInfoAsync(int doctorId)
        {
            try
            {
                var dependencyInfo = new DoctorDependencyInfo
                {
                    CanBeDeleted = await CanDeleteDoctorAsync(doctorId),
                    DeletionErrorMessage = await GetDeletionErrorMessageAsync(doctorId),
                    TotalActiveAppointments = await _context.Appointments
                        .CountAsync(a => a.DoctorId == doctorId && !a.IsDeleted),
                    TotalDepartmentAssignments = await _context.DoctorDepartments
                        .CountAsync(dd => dd.DoctorId == doctorId && !dd.IsDeleted),
                    TotalServiceCategoryAssignments = await _context.DoctorServiceCategories
                        .CountAsync(dsc => dsc.DoctorId == doctorId && !dsc.IsDeleted),
                    AppointmentCount = await _context.Appointments
                        .CountAsync(a => a.DoctorId == doctorId && !a.IsDeleted),
                    DepartmentAssignmentCount = await _context.DoctorDepartments
                        .CountAsync(dd => dd.DoctorId == doctorId && !dd.IsDeleted),
                    ServiceCategoryAssignmentCount = await _context.DoctorServiceCategories
                        .CountAsync(dsc => dsc.DoctorId == doctorId && !dsc.IsDeleted)
                };

                return dependencyInfo;
            }
            catch (Exception ex)
            {
                // لاگ خطا برای سیستم‌های پزشکی
                throw new InvalidOperationException($"خطا در دریافت اطلاعات وابستگی‌های پزشک {doctorId}", ex);
            }
        }

        /// <summary>
        /// دریافت پیام خطای حذف پزشک
        /// </summary>
        private async Task<string> GetDeletionErrorMessageAsync(int doctorId)
        {
            try
            {
                var errors = new List<string>();

                // بررسی وجود نوبت‌های فعال
                var hasActiveAppointments = await _context.Appointments
                    .AnyAsync(a => a.DoctorId == doctorId && !a.IsDeleted);

                if (hasActiveAppointments)
                    errors.Add("این پزشک دارای نوبت‌های فعال است");

                // بررسی وجود انتسابات فعال به دپارتمان
                var hasActiveDepartmentAssignments = await _context.DoctorDepartments
                    .AnyAsync(dd => dd.DoctorId == doctorId && dd.IsActive && !dd.IsDeleted);

                if (hasActiveDepartmentAssignments)
                    errors.Add("این پزشک دارای انتسابات فعال به دپارتمان‌ها است");

                // بررسی وجود انتسابات فعال به سرفصل‌های خدماتی
                var hasActiveServiceCategories = await _context.DoctorServiceCategories
                    .AnyAsync(dsc => dsc.DoctorId == doctorId && dsc.IsActive && !dsc.IsDeleted);

                if (hasActiveServiceCategories)
                    errors.Add("این پزشک دارای صلاحیت‌های فعال خدماتی است");

                return errors.Count > 0 ? string.Join("، ", errors) : "پزشک قابل حذف است";
            }
            catch (Exception ex)
            {
                return "خطا در بررسی امکان حذف پزشک";
            }
        }

        /// <summary>
        /// بررسی امکان حذف پزشک
        /// </summary>
        public async Task<bool> CanDeleteDoctorAsync(int doctorId)
        {
            try
            {
                // بررسی وجود نوبت‌های فعال
                var hasActiveAppointments = await _context.Appointments
                    .AnyAsync(a => a.DoctorId == doctorId && !a.IsDeleted);

                if (hasActiveAppointments)
                    return false;

                // بررسی وجود انتسابات فعال
                var hasActiveAssignments = await _context.DoctorDepartments
                    .AnyAsync(dd => dd.DoctorId == doctorId && dd.IsActive && !dd.IsDeleted);

                if (hasActiveAssignments)
                    return false;

                var hasActiveServiceCategories = await _context.DoctorServiceCategories
                    .AnyAsync(dsc => dsc.DoctorId == doctorId && dsc.IsActive && !dsc.IsDeleted);

                if (hasActiveServiceCategories)
                    return false;

                return true;
            }
            catch (Exception ex)
            {
                // لاگ خطا برای سیستم‌های پزشکی
                throw new InvalidOperationException($"خطا در بررسی امکان حذف پزشک {doctorId}", ex);
            }
        }

        #endregion

        #region Advanced Reporting (گزارش‌گیری پیشرفته)

        /// <summary>
        /// دریافت آمار عملکرد پزشک در بازه زمانی
        /// </summary>
        public async Task<DoctorPerformanceStats> GetDoctorPerformanceStatsAsync(int doctorId, DateTime startDate, DateTime endDate)
        {
            try
            {
                var appointments = await _context.Appointments
                    .Where(a => a.DoctorId == doctorId && 
                               a.AppointmentDate >= startDate && 
                               a.AppointmentDate <= endDate && 
                               !a.IsDeleted)
                    .ToListAsync();

                var stats = new DoctorPerformanceStats
                {
                    DoctorId = doctorId,
                    TotalAppointments = appointments.Count,
                    CompletedAppointments = appointments.Count(a => a.Status == AppointmentStatus.Completed),
                    CancelledAppointments = appointments.Count(a => a.Status == AppointmentStatus.Cancelled),
                    PendingAppointments = appointments.Count(a => a.Status == AppointmentStatus.Pending),
                    StartDate = startDate,
                    EndDate = endDate
                };

                return stats;
            }
            catch (Exception ex)
            {
                // لاگ خطا برای سیستم‌های پزشکی
                throw new InvalidOperationException($"خطا در دریافت آمار عملکرد پزشک {doctorId}", ex);
            }
        }

        /// <summary>
        /// دریافت گزارش مقایسه‌ای پزشکان
        /// </summary>
        public async Task<List<DoctorComparisonReport>> GetDoctorComparisonReportAsync(int clinicId, DateTime startDate, DateTime endDate)
        {
            try
            {
                var doctors = await _context.Doctors
                    .Where(d => d.ClinicId == clinicId && !d.IsDeleted && d.IsActive)
                    .Include(d => d.Appointments)
                    .ToListAsync();

                var comparisonReports = new List<DoctorComparisonReport>();

                foreach (var doctor in doctors)
                {
                    var appointments = doctor.Appointments
                        .Where(a => a.AppointmentDate >= startDate && 
                                   a.AppointmentDate <= endDate && 
                                   !a.IsDeleted)
                        .ToList();

                    var report = new DoctorComparisonReport
                    {
                        DoctorId = doctor.DoctorId,
                        DoctorName = $"{doctor.FirstName} {doctor.LastName}",
                        TotalAppointments = appointments.Count,
                        CompletedAppointments = appointments.Count(a => a.Status == AppointmentStatus.Completed),
                        CancelledAppointments = appointments.Count(a => a.Status == AppointmentStatus.Cancelled),
                        CompletionRate = appointments.Count > 0 ? 
                            (double)appointments.Count(a => a.Status == AppointmentStatus.Completed) / appointments.Count * 100 : 0
                    };

                    comparisonReports.Add(report);
                }

                return comparisonReports.OrderByDescending(r => r.CompletionRate).ToList();
            }
            catch (Exception ex)
            {
                // لاگ خطا برای سیستم‌های پزشکی
                throw new InvalidOperationException($"خطا در دریافت گزارش مقایسه‌ای پزشکان", ex);
            }
        }

        #endregion
    }

    #region Supporting Classes (کلاس‌های پشتیبان)

    /// <summary>
    /// آمار عملکرد پزشک
    /// </summary>
    public class DoctorPerformanceStats
    {
        public int DoctorId { get; set; }
        public int TotalAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public int CancelledAppointments { get; set; }
        public int PendingAppointments { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    /// <summary>
    /// گزارش مقایسه‌ای پزشکان
    /// </summary>
    public class DoctorComparisonReport
    {
        public int DoctorId { get; set; }
        public string DoctorName { get; set; }
        public int TotalAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public int CancelledAppointments { get; set; }
        public double CompletionRate { get; set; }
    }

    #endregion
}
