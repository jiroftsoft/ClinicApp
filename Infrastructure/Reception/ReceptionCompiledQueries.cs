using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Models;
using ClinicApp.Models.Entities.Reception;
using ClinicApp.Models.Enums;

namespace ClinicApp.Infrastructure.Reception
{
    /// <summary>
    /// Compiled Queries برای بهینه‌سازی عملکرد ماژول پذیرش
    /// 
    /// ویژگی‌های کلیدی:
    /// 1. Compiled Queries برای کوئری‌های پرترافیک
    /// 2. بهینه‌سازی عملکرد Database
    /// 3. کاهش Latency
    /// 4. بهبود Response Time
    /// 
    /// Architecture Principles:
    /// ✅ Single Responsibility: فقط Compiled Queries
    /// ✅ Open/Closed: باز برای توسعه، بسته برای تغییر
    /// ✅ Performance First: عملکرد در اولویت
    /// </summary>
    public static class ReceptionCompiledQueries
    {
        #region Basic CRUD Compiled Queries

        /// <summary>
        /// دریافت پذیرش با شناسه
        /// </summary>
        public static readonly Func<ApplicationDbContext, int, Task<Models.Entities.Reception.Reception>> GetReceptionById =
            EF.CompileAsyncQuery((ApplicationDbContext context, int id) =>
                context.Receptions.FirstOrDefault(r => r.ReceptionId == id && !r.IsDeleted));

        /// <summary>
        /// دریافت پذیرش با شناسه و Include
        /// </summary>
        public static readonly Func<ApplicationDbContext, int, Task<Models.Entities.Reception.Reception>> GetReceptionByIdWithIncludes =
            EF.CompileAsyncQuery((ApplicationDbContext context, int id) =>
                context.Receptions
                    .Include(r => r.Patient)
                    .Include(r => r.Doctor)
                    .Include(r => r.ActivePatientInsurance)
                    .Include(r => r.ReceptionItems)
                    .FirstOrDefault(r => r.ReceptionId == id && !r.IsDeleted));

        /// <summary>
        /// دریافت تمام پذیرش‌ها
        /// </summary>
        public static readonly Func<ApplicationDbContext, Task<List<Models.Entities.Reception.Reception>>> GetAllReceptions =
            EF.CompileAsyncQuery((ApplicationDbContext context) =>
                context.Receptions.Where(r => !r.IsDeleted));

        /// <summary>
        /// دریافت تمام پذیرش‌ها با Include
        /// </summary>
        public static readonly Func<ApplicationDbContext, Task<List<Models.Entities.Reception.Reception>>> GetAllReceptionsWithIncludes =
            EF.CompileAsyncQuery((ApplicationDbContext context) =>
                context.Receptions
                    .Include(r => r.Patient)
                    .Include(r => r.Doctor)
                    .Include(r => r.ActivePatientInsurance)
                    .Where(r => !r.IsDeleted));

        #endregion

        #region Date-Based Compiled Queries

        /// <summary>
        /// دریافت پذیرش‌ها بر اساس تاریخ
        /// </summary>
        public static readonly Func<ApplicationDbContext, DateTime, Task<List<Models.Entities.Reception.Reception>>> GetReceptionsByDate =
            EF.CompileAsyncQuery((ApplicationDbContext context, DateTime date) =>
                context.Receptions.Where(r => r.ReceptionDate.Date == date.Date && !r.IsDeleted));

        /// <summary>
        /// دریافت پذیرش‌ها بر اساس تاریخ با Include
        /// </summary>
        public static readonly Func<ApplicationDbContext, DateTime, Task<List<Models.Entities.Reception.Reception>>> GetReceptionsByDateWithIncludes =
            EF.CompileAsyncQuery((ApplicationDbContext context, DateTime date) =>
                context.Receptions
                    .Include(r => r.Patient)
                    .Include(r => r.Doctor)
                    .Include(r => r.ActivePatientInsurance)
                    .Where(r => r.ReceptionDate.Date == date.Date && !r.IsDeleted));

        /// <summary>
        /// دریافت پذیرش‌ها بر اساس بازه زمانی
        /// </summary>
        public static readonly Func<ApplicationDbContext, DateTime, DateTime, Task<List<Models.Entities.Reception.Reception>>> GetReceptionsByDateRange =
            EF.CompileAsyncQuery((ApplicationDbContext context, DateTime startDate, DateTime endDate) =>
                context.Receptions.Where(r => r.ReceptionDate >= startDate && r.ReceptionDate <= endDate && !r.IsDeleted));

        /// <summary>
        /// دریافت پذیرش‌ها بر اساس بازه زمانی با Include
        /// </summary>
        public static readonly Func<ApplicationDbContext, DateTime, DateTime, Task<List<Models.Entities.Reception.Reception>>> GetReceptionsByDateRangeWithIncludes =
            EF.CompileAsyncQuery((ApplicationDbContext context, DateTime startDate, DateTime endDate) =>
                context.Receptions
                    .Include(r => r.Patient)
                    .Include(r => r.Doctor)
                    .Include(r => r.ActivePatientInsurance)
                    .Where(r => r.ReceptionDate >= startDate && r.ReceptionDate <= endDate && !r.IsDeleted));

        #endregion

        #region Patient-Based Compiled Queries

        /// <summary>
        /// دریافت پذیرش‌های بیمار
        /// </summary>
        public static readonly Func<ApplicationDbContext, int, Task<List<Models.Entities.Reception.Reception>>> GetReceptionsByPatient =
            EF.CompileAsyncQuery((ApplicationDbContext context, int patientId) =>
                context.Receptions.Where(r => r.PatientId == patientId && !r.IsDeleted));

        /// <summary>
        /// دریافت پذیرش‌های بیمار با Include
        /// </summary>
        public static readonly Func<ApplicationDbContext, int, Task<List<Models.Entities.Reception.Reception>>> GetReceptionsByPatientWithIncludes =
            EF.CompileAsyncQuery((ApplicationDbContext context, int patientId) =>
                context.Receptions
                    .Include(r => r.Patient)
                    .Include(r => r.Doctor)
                    .Include(r => r.ActivePatientInsurance)
                    .Include(r => r.ReceptionItems)
                    .Where(r => r.PatientId == patientId && !r.IsDeleted));

        /// <summary>
        /// دریافت پذیرش‌های بیمار بر اساس تاریخ
        /// </summary>
        public static readonly Func<ApplicationDbContext, int, DateTime, Task<List<Models.Entities.Reception.Reception>>> GetReceptionsByPatientAndDate =
            EF.CompileAsyncQuery((ApplicationDbContext context, int patientId, DateTime date) =>
                context.Receptions.Where(r => r.PatientId == patientId && r.ReceptionDate.Date == date.Date && !r.IsDeleted));

        /// <summary>
        /// بررسی وجود پذیرش فعال بیمار
        /// </summary>
        public static readonly Func<ApplicationDbContext, int, DateTime, Task<bool>> HasActiveReceptionByPatient =
            EF.CompileAsyncQuery((ApplicationDbContext context, int patientId, DateTime date) =>
                context.Receptions.Any(r => r.PatientId == patientId && 
                                          r.ReceptionDate.Date == date.Date && 
                                          r.Status == ReceptionStatus.Pending &&
                                          !r.IsDeleted));

        #endregion

        #region Doctor-Based Compiled Queries

        /// <summary>
        /// دریافت پذیرش‌های پزشک
        /// </summary>
        public static readonly Func<ApplicationDbContext, int, Task<List<Models.Entities.Reception.Reception>>> GetReceptionsByDoctor =
            EF.CompileAsyncQuery((ApplicationDbContext context, int doctorId) =>
                context.Receptions.Where(r => r.DoctorId == doctorId && !r.IsDeleted));

        /// <summary>
        /// دریافت پذیرش‌های پزشک با Include
        /// </summary>
        public static readonly Func<ApplicationDbContext, int, Task<List<Models.Entities.Reception.Reception>>> GetReceptionsByDoctorWithIncludes =
            EF.CompileAsyncQuery((ApplicationDbContext context, int doctorId) =>
                context.Receptions
                    .Include(r => r.Patient)
                    .Include(r => r.Doctor)
                    .Include(r => r.ActivePatientInsurance)
                    .Include(r => r.ReceptionItems)
                    .Where(r => r.DoctorId == doctorId && !r.IsDeleted));

        /// <summary>
        /// دریافت پذیرش‌های پزشک بر اساس تاریخ
        /// </summary>
        public static readonly Func<ApplicationDbContext, int, DateTime, Task<List<Models.Entities.Reception.Reception>>> GetReceptionsByDoctorAndDate =
            EF.CompileAsyncQuery((ApplicationDbContext context, int doctorId, DateTime date) =>
                context.Receptions.Where(r => r.DoctorId == doctorId && r.ReceptionDate.Date == date.Date && !r.IsDeleted));

        /// <summary>
        /// شمارش پذیرش‌های پزشک در تاریخ
        /// </summary>
        public static readonly Func<ApplicationDbContext, int, DateTime, Task<int>> GetReceptionCountByDoctorAndDate =
            EF.CompileAsyncQuery((ApplicationDbContext context, int doctorId, DateTime date) =>
                context.Receptions.Count(r => r.DoctorId == doctorId && r.ReceptionDate.Date == date.Date && !r.IsDeleted));

        #endregion

        #region Status-Based Compiled Queries

        /// <summary>
        /// دریافت پذیرش‌ها بر اساس وضعیت
        /// </summary>
        public static readonly Func<ApplicationDbContext, ReceptionStatus, Task<List<Models.Entities.Reception.Reception>>> GetReceptionsByStatus =
            EF.CompileAsyncQuery((ApplicationDbContext context, ReceptionStatus status) =>
                context.Receptions.Where(r => r.Status == status && !r.IsDeleted));

        /// <summary>
        /// دریافت پذیرش‌ها بر اساس وضعیت با Include
        /// </summary>
        public static readonly Func<ApplicationDbContext, ReceptionStatus, Task<List<Models.Entities.Reception.Reception>>> GetReceptionsByStatusWithIncludes =
            EF.CompileAsyncQuery((ApplicationDbContext context, ReceptionStatus status) =>
                context.Receptions
                    .Include(r => r.Patient)
                    .Include(r => r.Doctor)
                    .Include(r => r.ActivePatientInsurance)
                    .Where(r => r.Status == status && !r.IsDeleted));

        /// <summary>
        /// دریافت پذیرش‌های در انتظار
        /// </summary>
        public static readonly Func<ApplicationDbContext, Task<List<Models.Entities.Reception.Reception>>> GetPendingReceptions =
            EF.CompileAsyncQuery((ApplicationDbContext context) =>
                context.Receptions.Where(r => r.Status == ReceptionStatus.Pending && !r.IsDeleted));

        /// <summary>
        /// دریافت پذیرش‌های تکمیل شده
        /// </summary>
        public static readonly Func<ApplicationDbContext, Task<List<Models.Entities.Reception.Reception>>> GetCompletedReceptions =
            EF.CompileAsyncQuery((ApplicationDbContext context) =>
                context.Receptions.Where(r => r.Status == ReceptionStatus.Completed && !r.IsDeleted));

        /// <summary>
        /// دریافت پذیرش‌های لغو شده
        /// </summary>
        public static readonly Func<ApplicationDbContext, Task<List<Models.Entities.Reception.Reception>>> GetCancelledReceptions =
            EF.CompileAsyncQuery((ApplicationDbContext context) =>
                context.Receptions.Where(r => r.Status == ReceptionStatus.Cancelled && !r.IsDeleted));

        #endregion

        #region Type-Based Compiled Queries

        /// <summary>
        /// دریافت پذیرش‌ها بر اساس نوع
        /// </summary>
        public static readonly Func<ApplicationDbContext, ReceptionType, Task<List<Models.Entities.Reception.Reception>>> GetReceptionsByType =
            EF.CompileAsyncQuery((ApplicationDbContext context, ReceptionType type) =>
                context.Receptions.Where(r => r.Type == type && !r.IsDeleted));

        /// <summary>
        /// دریافت پذیرش‌های عادی
        /// </summary>
        public static readonly Func<ApplicationDbContext, Task<List<Models.Entities.Reception.Reception>>> GetNormalReceptions =
            EF.CompileAsyncQuery((ApplicationDbContext context) =>
                context.Receptions.Where(r => r.Type == ReceptionType.Normal && !r.IsDeleted));

        /// <summary>
        /// دریافت پذیرش‌های اورژانس
        /// </summary>
        public static readonly Func<ApplicationDbContext, Task<List<Models.Entities.Reception.Reception>>> GetEmergencyReceptions =
            EF.CompileAsyncQuery((ApplicationDbContext context) =>
                context.Receptions.Where(r => r.IsEmergency && !r.IsDeleted));

        /// <summary>
        /// دریافت پذیرش‌های آنلاین
        /// </summary>
        public static readonly Func<ApplicationDbContext, Task<List<Models.Entities.Reception.Reception>>> GetOnlineReceptions =
            EF.CompileAsyncQuery((ApplicationDbContext context) =>
                context.Receptions.Where(r => r.IsOnlineReception && !r.IsDeleted));

        #endregion

        #region Statistics Compiled Queries

        /// <summary>
        /// شمارش کل پذیرش‌ها
        /// </summary>
        public static readonly Func<ApplicationDbContext, Task<int>> GetTotalReceptionsCount =
            EF.CompileAsyncQuery((ApplicationDbContext context) =>
                context.Receptions.Count(r => !r.IsDeleted));

        /// <summary>
        /// شمارش پذیرش‌ها بر اساس تاریخ
        /// </summary>
        public static readonly Func<ApplicationDbContext, DateTime, Task<int>> GetReceptionsCountByDate =
            EF.CompileAsyncQuery((ApplicationDbContext context, DateTime date) =>
                context.Receptions.Count(r => r.ReceptionDate.Date == date.Date && !r.IsDeleted));

        /// <summary>
        /// شمارش پذیرش‌های تکمیل شده بر اساس تاریخ
        /// </summary>
        public static readonly Func<ApplicationDbContext, DateTime, Task<int>> GetCompletedReceptionsCountByDate =
            EF.CompileAsyncQuery((ApplicationDbContext context, DateTime date) =>
                context.Receptions.Count(r => r.ReceptionDate.Date == date.Date && 
                                            r.Status == ReceptionStatus.Completed && 
                                            !r.IsDeleted));

        /// <summary>
        /// شمارش پذیرش‌های در انتظار بر اساس تاریخ
        /// </summary>
        public static readonly Func<ApplicationDbContext, DateTime, Task<int>> GetPendingReceptionsCountByDate =
            EF.CompileAsyncQuery((ApplicationDbContext context, DateTime date) =>
                context.Receptions.Count(r => r.ReceptionDate.Date == date.Date && 
                                            r.Status == ReceptionStatus.Pending && 
                                            !r.IsDeleted));

        /// <summary>
        /// مجموع درآمد بر اساس تاریخ
        /// </summary>
        public static readonly Func<ApplicationDbContext, DateTime, Task<decimal>> GetTotalRevenueByDate =
            EF.CompileAsyncQuery((ApplicationDbContext context, DateTime date) =>
                context.Receptions.Where(r => r.ReceptionDate.Date == date.Date && !r.IsDeleted)
                    .Sum(r => r.TotalAmount));

        /// <summary>
        /// مجموع درآمد بر اساس بازه زمانی
        /// </summary>
        public static readonly Func<ApplicationDbContext, DateTime, DateTime, Task<decimal>> GetTotalRevenueByDateRange =
            EF.CompileAsyncQuery((ApplicationDbContext context, DateTime startDate, DateTime endDate) =>
                context.Receptions.Where(r => r.ReceptionDate >= startDate && 
                                            r.ReceptionDate <= endDate && 
                                            !r.IsDeleted)
                    .Sum(r => r.TotalAmount));

        #endregion

        #region Advanced Compiled Queries

        /// <summary>
        /// دریافت پذیرش‌ها با فیلترهای پیچیده
        /// </summary>
        public static readonly Func<ApplicationDbContext, int?, int?, DateTime?, DateTime?, ReceptionStatus?, ReceptionType?, Task<List<Models.Entities.Reception.Reception>>> GetReceptionsWithFilters =
            EF.CompileAsyncQuery((ApplicationDbContext context, int? patientId, int? doctorId, DateTime? startDate, DateTime? endDate, ReceptionStatus? status, ReceptionType? type) =>
                context.Receptions.Where(r => 
                    (!patientId.HasValue || r.PatientId == patientId.Value) &&
                    (!doctorId.HasValue || r.DoctorId == doctorId.Value) &&
                    (!startDate.HasValue || r.ReceptionDate >= startDate.Value) &&
                    (!endDate.HasValue || r.ReceptionDate <= endDate.Value) &&
                    (!status.HasValue || r.Status == status.Value) &&
                    (!type.HasValue || r.Type == type.Value) &&
                    !r.IsDeleted));

        /// <summary>
        /// دریافت پذیرش‌ها با فیلترهای پیچیده و Include
        /// </summary>
        public static readonly Func<ApplicationDbContext, int?, int?, DateTime?, DateTime?, ReceptionStatus?, ReceptionType?, Task<List<Models.Entities.Reception.Reception>>> GetReceptionsWithFiltersAndIncludes =
            EF.CompileAsyncQuery((ApplicationDbContext context, int? patientId, int? doctorId, DateTime? startDate, DateTime? endDate, ReceptionStatus? status, ReceptionType? type) =>
                context.Receptions
                    .Include(r => r.Patient)
                    .Include(r => r.Doctor)
                    .Include(r => r.ActivePatientInsurance)
                    .Include(r => r.ReceptionItems)
                    .Where(r => 
                        (!patientId.HasValue || r.PatientId == patientId.Value) &&
                        (!doctorId.HasValue || r.DoctorId == doctorId.Value) &&
                        (!startDate.HasValue || r.ReceptionDate >= startDate.Value) &&
                        (!endDate.HasValue || r.ReceptionDate <= endDate.Value) &&
                        (!status.HasValue || r.Status == status.Value) &&
                        (!type.HasValue || r.Type == type.Value) &&
                        !r.IsDeleted));

        #endregion
    }
}
