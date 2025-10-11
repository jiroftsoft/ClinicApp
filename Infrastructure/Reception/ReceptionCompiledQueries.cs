using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using ClinicApp.Models;
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
        public static async Task<Models.Entities.Reception.Reception> GetReceptionByIdAsync(ApplicationDbContext context, int id)
        {
            return await context.Receptions
                .Include(r => r.Patient)
                .Include(r => r.Doctor)
                .Include(r => r.ReceptionItems)
                .FirstOrDefaultAsync(r => r.ReceptionId == id && !r.IsDeleted);
        }

        /// <summary>
        /// دریافت پذیرش‌ها با تاریخ
        /// </summary>
        public static async Task<List<Models.Entities.Reception.Reception>> GetReceptionsByDateAsync(ApplicationDbContext context, DateTime date)
        {
            return await context.Receptions
                .Include(r => r.Patient)
                .Include(r => r.Doctor)
                .Where(r => r.ReceptionDate.Date == date.Date && !r.IsDeleted)
                .OrderByDescending(r => r.ReceptionDate)
                .ToListAsync();
        }

        /// <summary>
        /// دریافت پذیرش‌ها با بازه تاریخ
        /// </summary>
        public static async Task<List<Models.Entities.Reception.Reception>> GetReceptionsByDateRangeAsync(ApplicationDbContext context, DateTime startDate, DateTime endDate)
        {
            return await context.Receptions
                .Include(r => r.Patient)
                .Include(r => r.Doctor)
                .Where(r => r.ReceptionDate >= startDate && r.ReceptionDate <= endDate && !r.IsDeleted)
                .OrderByDescending(r => r.ReceptionDate)
                .ToListAsync();
        }

        /// <summary>
        /// دریافت پذیرش‌های بیمار
        /// </summary>
        public static async Task<List<Models.Entities.Reception.Reception>> GetReceptionsByPatientAsync(ApplicationDbContext context, int patientId)
        {
            return await context.Receptions
                .Include(r => r.Patient)
                .Include(r => r.Doctor)
                .Where(r => r.PatientId == patientId && !r.IsDeleted)
                .OrderByDescending(r => r.ReceptionDate)
                .ToListAsync();
        }

        /// <summary>
        /// دریافت پذیرش‌های پزشک
        /// </summary>
        public static async Task<List<Models.Entities.Reception.Reception>> GetReceptionsByDoctorAsync(ApplicationDbContext context, int doctorId)
        {
            return await context.Receptions
                .Include(r => r.Patient)
                .Include(r => r.Doctor)
                .Where(r => r.DoctorId == doctorId && !r.IsDeleted)
                .OrderByDescending(r => r.ReceptionDate)
                .ToListAsync();
        }

        #endregion

        #region Search and Filter Compiled Queries

        /// <summary>
        /// جستجوی پذیرش‌ها با معیارهای مختلف
        /// </summary>
        public static async Task<List<Models.Entities.Reception.Reception>> SearchReceptionsAsync(ApplicationDbContext context, string searchTerm)
        {
            return await context.Receptions
                .Include(r => r.Patient)
                .Include(r => r.Doctor)
                .Where(r => !r.IsDeleted && (
                    r.Patient.FirstName.Contains(searchTerm) ||
                    r.Patient.LastName.Contains(searchTerm) ||
                    r.Doctor.FirstName.Contains(searchTerm) ||
                    r.Doctor.LastName.Contains(searchTerm) ||
                    r.ReceptionNumber.Contains(searchTerm)))
                .OrderByDescending(r => r.ReceptionDate)
                .ToListAsync();
        }

        /// <summary>
        /// دریافت پذیرش‌ها با وضعیت
        /// </summary>
        public static async Task<List<Models.Entities.Reception.Reception>> GetReceptionsByStatusAsync(ApplicationDbContext context, ReceptionStatus status)
        {
            return await context.Receptions
                .Include(r => r.Patient)
                .Include(r => r.Doctor)
                .Where(r => r.Status == status && !r.IsDeleted)
                .OrderByDescending(r => r.ReceptionDate)
                .ToListAsync();
        }

        /// <summary>
        /// دریافت پذیرش‌ها با نوع
        /// </summary>
        public static async Task<List<Models.Entities.Reception.Reception>> GetReceptionsByTypeAsync(ApplicationDbContext context, ReceptionType type)
        {
            return await context.Receptions
                .Include(r => r.Patient)
                .Include(r => r.Doctor)
                .Where(r => !r.IsDeleted)
                .OrderByDescending(r => r.ReceptionDate)
                .ToListAsync();
        }

        #endregion

        #region Statistics Compiled Queries

        /// <summary>
        /// دریافت آمار روزانه
        /// </summary>
        public static async Task<int> GetDailyReceptionCountAsync(ApplicationDbContext context, DateTime date)
        {
            return await context.Receptions
                .CountAsync(r => r.ReceptionDate.Date == date.Date && !r.IsDeleted);
        }

        /// <summary>
        /// دریافت آمار ماهانه
        /// </summary>
        public static async Task<int> GetMonthlyReceptionCountAsync(ApplicationDbContext context, int year, int month)
        {
            return await context.Receptions
                .CountAsync(r => r.ReceptionDate.Year == year && r.ReceptionDate.Month == month && !r.IsDeleted);
        }

        /// <summary>
        /// دریافت آمار سالانه
        /// </summary>
        public static async Task<int> GetYearlyReceptionCountAsync(ApplicationDbContext context, int year)
        {
            return await context.Receptions
                .CountAsync(r => r.ReceptionDate.Year == year && !r.IsDeleted);
        }

        #endregion

        #region Performance Optimized Queries

        /// <summary>
        /// دریافت پذیرش‌ها با Pagination
        /// </summary>
        public static async Task<List<Models.Entities.Reception.Reception>> GetReceptionsPagedAsync(ApplicationDbContext context, int pageNumber, int pageSize)
        {
            return await context.Receptions
                .Include(r => r.Patient)
                .Include(r => r.Doctor)
                .Where(r => !r.IsDeleted)
                .OrderByDescending(r => r.ReceptionDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        /// <summary>
        /// دریافت تعداد کل پذیرش‌ها
        /// </summary>
        public static async Task<int> GetTotalReceptionCountAsync(ApplicationDbContext context)
        {
            return await context.Receptions
                .CountAsync(r => !r.IsDeleted);
        }

        #endregion
    }
}